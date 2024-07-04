using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
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
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_cell_evolve.png", typeof(CellEvolve).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(CellEvolve),
                TextureHelper.GetImageAsTexture("ability_cellevolve.png", typeof(CellEvolve).Assembly)
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => base.RespondsToUpkeep(playerUpkeep) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override CardInfo GetTransformCardInfo()
        {
            var tInfo = base.GetTransformCardInfo();
            if (!tInfo.HasAbility(CellDeEvolve.AbilityID))
            {
                // This can only happen if the card has evolve params

                CardModificationInfo mod = new(CellDeEvolve.AbilityID);
                mod.fromEvolve = true;
                mod.nonCopyable = true;
                mod.negateAbilities.Add(CellEvolve.AbilityID);
                tInfo.mods.Add(mod);
                tInfo.evolveParams = new() { evolution = Card.Info.Clone() as CardInfo, turnsToEvolve = 1 };

            }
            return tInfo;
        }
    }
}
