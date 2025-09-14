using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
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
            info.rulebookDescription = "If [creature] is NOT within a circuit at the beginning of the turn, it will transform back into its original form.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.CELL_INVERSE, true).conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(CellDeEvolve),
                TextureHelper.GetImageAsTexture("ability_celldevolve.png", typeof(CellDeEvolve).Assembly)
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => base.RespondsToUpkeep(playerUpkeep) && !ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override CardInfo GetTransformCardInfo()
        {
            // Handle the temporary mods
            foreach (var mod in Card.TemporaryMods)
            {
                if (mod.HasAbility(AbilityID))
                {
                    mod.abilities.Remove(AbilityID);
                    mod.abilities.Add(CellEvolve.AbilityID);
                }
            }
            return base.GetTransformCardInfo();
        }
    }
}
