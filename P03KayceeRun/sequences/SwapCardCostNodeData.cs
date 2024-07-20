using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;
using System.Linq;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class SwapCardCostNodeData : SpecialNodeData
    {
        public static readonly string[] PART1_ITEMS_TO_PART3 = new string[] { };

        public static readonly HoloMapNode.NodeDataType SwapCardCost = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "SwapCardCostNodeData");

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == SwapCardCost)
            {
                __instance.Data = new SwapCardCostNodeData();
                return false;
            }
            return true;
        }
    }
}