using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
	public class CellDeEvolve : Evolve
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellDeEvolve()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Transforms When Unpowered";
            info.rulebookDescription = "If [creature] is NOT within a circuit at the beginning of the turn, it will transform back its original form.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            CellDeEvolve.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(CellDeEvolve),
                TextureHelper.GetImageAsTexture("ability_celldevolve.png", typeof(CellDeEvolve).Assembly)
            ).Id;
        }

		public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return base.RespondsToUpkeep(playerUpkeep) && !ConduitCircuitManager.Instance.SlotIsWithinCircuit(this.Card.Slot);
        }

        [HarmonyPatch(typeof(EvolveParams), nameof(EvolveParams.GetDefaultEvolution))]
        [HarmonyPrefix]
        internal static bool UpdateDefaultEvolutionWithCellDeEvolve(CardInfo info, ref CardInfo __result)
        {
            if (info.HasAbility(CellDeEvolve.AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;
                CardModificationInfo cardModificationInfo = new CardModificationInfo(0, 0);
                cardModificationInfo.fromEvolve = true;

                // Make it so the card doesn't copy this mod when it de-evolves
                cardModificationInfo.nonCopyable = true;

                // If this came from CellDevEvolve (i.e., the default evolution is re-evolving)
                // we don't need to change the name or change the attack or anything like that.
                // The evolution will end up remove the de-evolution mod and the card will revert
                // back to the original version.
                //
                // But if it does not have an evolve mod, we need to add a default evolve mod.
                if (!info.Mods.Any(m => m.fromEvolve))
                {
                    cardModificationInfo.nameReplacement = String.Format(Localization.Translate("Beta {0}"), cardInfo.DisplayedNameLocalized);
                    cardModificationInfo.attackAdjustment = -1;
                }

                cardModificationInfo.abilities = new () { CellEvolve.AbilityID };
                cardModificationInfo.negateAbilities = new () { CellDeEvolve.AbilityID } ;

                cardInfo.Mods.Add(cardModificationInfo);
                __result = cardInfo;

                return false;
            }
            return true;
        }
	}
}
