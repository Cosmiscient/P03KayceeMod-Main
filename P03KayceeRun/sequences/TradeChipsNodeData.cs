using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Cards;
using System.Linq;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class TradeChipsNodeData : SpecialNodeData
    {
        public static readonly HoloMapNode.NodeDataType TradeChipsForCards = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "TradeChipsForCards");

        private static bool IsDraftToken(CardInfo info)
        {
            return info.name.Equals(CustomCards.UNC_TOKEN) || info.name.Equals(CustomCards.DRAFT_TOKEN) || info.name.Equals(CustomCards.RARE_DRAFT_TOKEN);
        }

        [HarmonyPatch(typeof(HoloMapSpecialNode), nameof(HoloMapSpecialNode.OnCursorSelectEnd))]
        [HarmonyPrefix]
        public static bool NoDraftWithoutTokens(HoloMapSpecialNode __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (__instance.Data is not TradeChipsNodeData)
                return true;

            if (Part3SaveData.Data.deck.Cards.Any(IsDraftToken))
                return true;

            AudioController.Instance.PlaySound3D("holomap_encounter_select", MixerGroup.TableObjectsSFX, __instance.transform.position, 0.75f, 0f);
            Tween.Cancel(__instance.transform.GetInstanceID());
            Tween.Shake(__instance.transform, __instance.transform.localPosition, new Vector3(0.1f, 0f, 0.1f), 0.1f, 0f);
            return false;
        }

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == TradeChipsForCards)
            {
                __instance.Data = new TradeChipsNodeData();
                return false;
            }
            return true;
        }
    }
}