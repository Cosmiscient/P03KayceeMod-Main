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
using InscryptionAPI.RuleBook;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FireBomb : AbilityBehaviour, IOnPostSingularSlotAttackSlot
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static FireBomb()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fire Strike";
            info.rulebookDescription = "When [creature] attacks, it sets the target space on fire for three turns.";
            info.canStack = false;
            info.powerLevel = 4;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular, BurningSlotBase.FlamingAbility, AbilityMetaCategory.BountyHunter };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_fire_bomb.png", typeof(FireBomb).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FireBomb),
                TextureHelper.GetImageAsTexture("ability_fire_bomb.png", typeof(FireBomb).Assembly)
            ).Id;

            info.SetSlotRedirect("on fire", BurningSlotBase.GetFireLevel(2), GameColors.Instance.limeGreen);
        }

        public bool RespondsToPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot) => attackingSlot == Card.Slot;

        public IEnumerator OnPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot)
        {
            //AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, targetSlot.transform.position, .7f);
            // The fireball should play and then delete itself, but we'll destroy it after some time anyway
            yield return BurningSlotBase.SetSlotOnFireBasic(2, targetSlot, attackingSlot);
            yield break;
        }
    }
}
