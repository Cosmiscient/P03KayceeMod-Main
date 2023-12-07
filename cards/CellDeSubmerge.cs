using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class CellDeSubmerge : Submerge
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellDeSubmerge()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Waterborne When Unpowered";
            info.rulebookDescription = "If [creature] is NOT within a circuit it submerges itself during its opponent's turn. While submerged, opposing creatures attack its owner directly.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.CELL_INVERSE, true).conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(CellDeSubmerge),
                TextureHelper.GetImageAsTexture("ability_celldesubmerge.png", typeof(CellDeSubmerge).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => base.RespondsToTurnEnd(playerTurnEnd) && !ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);
    }
}
