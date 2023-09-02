using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class CellEvolve : Evolve
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellEvolve()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Transforms When Powered";
            info.rulebookDescription = "If [creature] is within a circuit at the beginning of the turn, it will transform into a stronger form.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(CellEvolve),
                TextureHelper.GetImageAsTexture("ability_cellevolve.png", typeof(CellEvolve).Assembly)
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return base.RespondsToUpkeep(playerUpkeep) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);
        }

        [HarmonyPatch(typeof(EvolveParams), nameof(EvolveParams.GetDefaultEvolution))]
        [HarmonyPrefix]
        internal static bool UpdateDefaultEvolutionWithCellEvolve(CardInfo info, ref CardInfo __result)
        {
            if (info.HasAbility(AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;
                CardModificationInfo cardModificationInfo = new(0, 0)
                {
                    fromEvolve = true,

                    // Make it so the card doesn't copy this mod when it re-evolves
                    nonCopyable = true
                };

                // If this came from CellEvolve (i.e., the default evolution is de-evolving)
                // we don't need to change the name or change the attack or anything like that.
                // The de-evolution will end up remove the evolution mod and the card will revert
                // back to the original version.
                //
                // But if it does not have an evolve mod, we need to add a default de-evolve mod.
                if (!info.Mods.Any(m => m.fromEvolve))
                {
                    cardModificationInfo.nameReplacement = String.Format(Localization.Translate("{0} 2.0"), cardInfo.DisplayedNameLocalized);
                    cardModificationInfo.attackAdjustment = 1;
                    cardModificationInfo.healthAdjustment = 2;
                }

                cardModificationInfo.abilities = new() { CellDeEvolve.AbilityID };
                cardModificationInfo.negateAbilities = new() { AbilityID };

                cardInfo.Mods.Add(cardModificationInfo);
                __result = cardInfo;

                return false;
            }
            return true;
        }
    }
}
