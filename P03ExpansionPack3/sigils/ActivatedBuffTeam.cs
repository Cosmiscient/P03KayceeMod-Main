using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03ExpansionPack3.Managers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class ActivatedBuffTeam : ActivatedAbilityBehaviour, IPassiveAttackBuff
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private static bool hasActivated;

        public override bool CanActivate()
        {
            return Card.GetCurrentFuel() > 0 && !hasActivated;
        }

        static ActivatedBuffTeam()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Supercharge";
            info.rulebookDescription = "Activate: Friendly cards gain 1 power until the end of the turn. This ability can only be activated once per turn.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Pack3Plugin.PluginGuid,
                info,
                typeof(ActivatedBuffTeam),
                TextureHelper.GetImageAsTexture("ability_activated_gain_power.png", typeof(ActivatedBuffTeam).Assembly)
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            hasActivated = false;
            if (!playerUpkeep && !Card.OpponentCard)
                yield return Activate();
        }

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard) => otherCard == this.Card && this.Card.OpponentCard;

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            yield return Activate();
        }

        public override IEnumerator Activate()
        {
            if (this.Card.TrySpendFuel())
            {
                hasActivated = true;
                yield return LearnAbility(0.1f);
            }
        }

        public int GetPassiveAttackBuff(PlayableCard target)
        {
            return hasActivated && target.IsPlayerCard() == this.Card.IsPlayerCard() ? 1 : 0;
        }
    }
}
