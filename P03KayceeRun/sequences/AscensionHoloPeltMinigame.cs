using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Saves;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public static class AscensionHoloPeltMinigame
    {
        internal const int ACTIVE = 0;
        internal const int WIN = 1;
        internal const int LOSE = 2;

        internal static Dictionary<int, int> PeltStatus
        {
            get
            {
                string dstr = P03AscensionSaveData.RunStateData.GetValue(P03Plugin.PluginGuid, "PeltStatus");
                Dictionary<int, int> retval = new();
                if (string.IsNullOrEmpty(dstr))
                    return retval;

                foreach (string d in dstr.Split('|'))
                    retval[int.Parse(d.Split(':')[0])] = int.Parse(d.Split(':')[1]);

                return retval;
            }
        }

        internal static Dictionary<int, int> UpdatePeltStatus(int key, int value)
        {
            var status = PeltStatus;
            status[key] = value;
            string dstr = string.Join("|", status.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "PeltStatus", dstr);
            return status;
        }

        private static GameObject GetBuybackNode(HoloMapPeltMinigame __instance)
        {
            return __instance.gameObject.transform.Find("BuybackNode")?.gameObject;
        }

        private static void HoverTrapInteractable(HoloMapGenericInteractable node)
        {
            var status = PeltStatus;
            if (!status.ContainsKey(node.nodeId))
                status = UpdatePeltStatus(node.nodeId, ACTIVE);

            node.transform.parent.Find("HoverText")?.gameObject.SetActive(status[node.nodeId] == LOSE);
            node.transform.parent.Find("HoverText")?.GetComponent<HoloFloatingLabel>().SetText("Reset: $1");
        }

        private static void EndHoverTrapInteractable(HoloMapGenericInteractable node)
        {
            node.transform.parent.Find("HoverText")?.gameObject.SetActive(false);
        }

        private static void SetMinigameFailed(HoloMapPeltMinigame __instance)
        {
            MaterialHelper.HolofyAllRenderers(__instance.trapInteractable.gameObject, GameColors.Instance.brightGold);
            MaterialHelper.HolofyAllRenderers(__instance.trapAnim.gameObject, GameColors.Instance.brightGold);
            __instance.trapInteractable.defaultColor = GameColors.Instance.brightGold;
            __instance.rewardNode.gameObject.SetActive(false);
            __instance.rabbitAnim.gameObject.SetActive(true);
            __instance.trapShut = true;
            __instance.trapAnim.Play("shut", 0, 1f);
            UpdatePeltStatus(__instance.trapInteractable.nodeId, LOSE);
            __instance.trapInteractable.SetEnabled(true);
        }

        private static void ResetMinigameToPlayable(HoloMapPeltMinigame __instance)
        {
            __instance.trapInteractable.SetEnabled(false);
            UpdatePeltStatus(__instance.trapInteractable.nodeId, ACTIVE);
            Tween.LocalPosition(__instance.trapAnim.transform, Vector3.up * 0.5f, 0.1f, 0f, Tween.EaseOut);
            Tween.LocalPosition(__instance.trapAnim.transform, Vector3.zero, 0.1f, 0.1f, Tween.EaseOut);
            MaterialHelper.HolofyAllRenderers(__instance.trapInteractable.gameObject, GameColors.Instance.blue);
            MaterialHelper.HolofyAllRenderers(__instance.trapAnim.gameObject, GameColors.Instance.blue);
            __instance.trapInteractable.defaultColor = GameColors.Instance.blue;
            __instance.trapAnim.Play("open", 0, 0f);
            CustomCoroutine.WaitThenExecute(0.1f, delegate ()
            {
                __instance.trapInteractable.SetEnabled(true);
                __instance.trapShut = false;
            });
        }

        [HarmonyPatch(typeof(HoloMapPeltMinigame), nameof(HoloMapPeltMinigame.Start))]
        [HarmonyPrefix]
        private static bool AscensionMinigameStart(HoloMapPeltMinigame __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            __instance.trapInteractable.CursorEntered = mii => HoverTrapInteractable(mii as HoloMapGenericInteractable);
            __instance.trapInteractable.CursorExited = mii => EndHoverTrapInteractable(mii as HoloMapGenericInteractable);

            P03Plugin.Log.LogDebug("Starting up holo pelt minigame");

            var status = PeltStatus;
            if (!status.ContainsKey(__instance.trapInteractable.nodeId))
                status = UpdatePeltStatus(__instance.trapInteractable.nodeId, ACTIVE);

            if (status[__instance.trapInteractable.nodeId] == LOSE)
            {
                SetMinigameFailed(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(HoloMapPeltMinigame), nameof(HoloMapPeltMinigame.OnTrapPressed))]
        [HarmonyPrefix]
        private static bool AscensionMinigameTrapPressed(HoloMapPeltMinigame __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            var status = PeltStatus;
            if (!status.ContainsKey(__instance.trapInteractable.nodeId))
                status = UpdatePeltStatus(__instance.trapInteractable.nodeId, ACTIVE);

            if (status[__instance.trapInteractable.nodeId] == LOSE)
            {
                if (Part3SaveData.Data.currency > 0)
                {
                    Part3SaveData.Data.currency -= 1;
                    ResetMinigameToPlayable(__instance);
                }
            }

            if (!__instance.trapShut)
            {
                AudioController.Instance.PlaySound2D("holomap_node_selected", MixerGroup.TableObjectsSFX, 1f, 0f, null, null, null, null, false);
                __instance.StartCoroutine(__instance.TrapShutSequence());
            }

            return false;
        }

        [HarmonyPatch(typeof(HoloMapPeltMinigame), nameof(HoloMapPeltMinigame.TrapShutSequence))]
        [HarmonyPostfix]
        [HarmonyPriority(HarmonyLib.Priority.VeryLow)]
        private static IEnumerator AscensionMinigameTrapSequence(IEnumerator sequence, HoloMapPeltMinigame __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                P03Plugin.Log.LogDebug("Running original holo pelt game because this is not P03KCM");
                yield return sequence;
                yield break;
            }

            if (__instance.rabbitTrappable)
            {
                P03Plugin.Log.LogDebug("Running original holo pelt game because rabbit is trappable");
                UpdatePeltStatus(__instance.trapInteractable.nodeId, WIN);
                yield return sequence;
                yield break;
            }

            P03Plugin.Log.LogDebug("Running new holo pelt code");

            __instance.trapShut = true;
            __instance.trapAnim.Play("shut", 0, 0f);
            __instance.trapInteractable.SetEnabled(false);
            Tween.LocalPosition(__instance.trapAnim.transform, Vector3.up * 0.5f, 0.1f, 0f, Tween.EaseOut);
            Tween.LocalPosition(__instance.trapAnim.transform, Vector3.zero, 0.1f, 0.1f, Tween.EaseOut);

            yield return new WaitForSeconds(0.2f);
            SetMinigameFailed(__instance);
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
    }
}