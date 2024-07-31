using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using InscryptionAPI.Slots;
using UnityEngine;
using GBC;
using Infiniscryption.P03SigilLibrary.Helpers;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FireBombWhenFueled : AbilityBehaviour, IOnPostSingularSlotAttackSlot, IOnMissileStrike
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private bool pretendHaveFuel = false;
        private bool ignoreFuelCheck = false;

        static FireBombWhenFueled()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fire Strike When Fueled";
            info.rulebookDescription = "When [creature] attacks, if it fueled, it consumes one fuel to set the target space on fire for three turns.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.passive = false;
            info.SetExtendedProperty(AbilityIconBehaviours.ACTIVE_WHEN_FUELED, true);
            info.colorOverride = GameColors.Instance.darkLimeGreen;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular, BurningSlotBase.FlamingAbility };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FireBombWhenFueled),
                TextureHelper.GetImageAsTexture("ability_flame_when_fueled.png", typeof(FireBombWhenFueled).Assembly)
            ).Id;
        }

        public bool RespondsToPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot) => ((ignoreFuelCheck && pretendHaveFuel) || this.Card.GetCurrentFuel() > 0) && attackingSlot == Card.Slot;

        public IEnumerator OnPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot)
        {
            if ((ignoreFuelCheck && pretendHaveFuel) || this.Card.TrySpendFuel(1))
            {
                //AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, targetSlot.transform.position, .7f);
                // The fireball should play and then delete itself, but we'll destroy it after some time anyway
                yield return BurningSlotBase.SetSlotOnFireBasic(2, targetSlot, attackingSlot);
                yield break;
            }
        }

        public bool RespondsToStrikeQueued(CardSlot targetSlot) => this.Card.GetCurrentFuel() > 0;

        public IEnumerator OnStrikeQueued(CardSlot targetSlot)
        {
            if (this.Card.TrySpendFuel(1))
                pretendHaveFuel = true;

            yield break;
        }

        public bool RespondsToPreStrikeHit(CardSlot targetSlot) => true;

        public IEnumerator OnPreStrikeHit(CardSlot targetSlot)
        {
            ignoreFuelCheck = true;
            yield break;
        }

        public bool RespondsToPostStrikeHit(CardSlot targetSlot) => true;

        public IEnumerator OnPostStrikeHit(CardSlot targetSlot)
        {
            ignoreFuelCheck = false;
            yield break;
        }

        public bool RespondsToPostAllStrike() => true;

        public IEnumerator OnPostAllStrike()
        {
            pretendHaveFuel = false;
            yield break;
        }
    }
}
