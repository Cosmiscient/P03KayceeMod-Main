using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedBuffTeam : FuelActivatedAbilityBehaviour, IPassiveAttackBuff, IOnBellRung
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int FuelCost => 1;

        static ActivatedBuffTeam()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Supercharge";
            info.rulebookDescription = "Spend one fuel: friendly cards gain 1 power until the end of the turn. This ability can only be activated once per turn.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.activated = true;
            info.SetDefaultFuel(1);
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedBuffTeam),
                TextureHelper.GetImageAsTexture("ability_activated_gain_power.png", typeof(ActivatedBuffTeam).Assembly)
            ).Id;

            info.SetUniqueRedirect("fuel", "fuelManagerPage", GameColors.Instance.limeGreen);
        }

        public override IEnumerator ActivateAfterSpendFuel()
        {
            yield return LearnAbility(0.1f);
        }

        public int GetPassiveAttackBuff(PlayableCard target)
        {
            return this.HasActivatedThisTurn && (target.OpponentCard == this.Card.OpponentCard) ? 1 : 0;
        }

        public bool RespondsToBellRung(bool playerCombatPhase) => this.Card.OpponentCard && !playerCombatPhase;

        public IEnumerator OnBellRung(bool playerCombatPhase)
        {
            if (this.Card.OpponentCard && !playerCombatPhase)
                yield return this.OnActivatedAbility();
        }
    }
}
