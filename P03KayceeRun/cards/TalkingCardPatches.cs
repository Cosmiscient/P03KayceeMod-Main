using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.TalkingCards.Create;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class TalkingCardPatches
    {
        [HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.SubscribeToTextEvents))]
        [HarmonyPostfix]
        private static void TrySetTextBetter(TalkingCard __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __instance.CardCameraParent ??= CardRenderCamera.Instance.GetLiveRenderCamera(__instance.Card.StatsLayer)?.transform;
                __instance.Text ??= __instance.gameObject.GetComponent<SequentialText>() ?? __instance.gameObject.GetComponent<SequentialText>() ?? __instance.CardCameraParent?.GetComponentInChildren<SequentialText>();
            }
        }

        [HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.InterruptCurrentDialogue))]
        [HarmonyPostfix]
        private static IEnumerator EnsureTextNotNull(IEnumerator sequence, TalkingCard __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __instance.CardCameraParent ??= CardRenderCamera.Instance.GetLiveRenderCamera(__instance.Card.StatsLayer)?.transform;
                __instance.Text ??= __instance.gameObject.GetComponent<SequentialText>() ?? __instance.gameObject.GetComponent<SequentialText>() ?? __instance.CardCameraParent?.GetComponentInChildren<SequentialText>();
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.SetFaceAnimTrigger))]
        [HarmonyPrefix]
        private static bool SetVoiceIdWithFaceAnimTrigger(TalkingCard __instance, string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return true;

            if (!trigger.StartsWith("voice."))
                return true;

            string voiceId = trigger.Replace("voice.", "");

            if (string.IsNullOrEmpty(voiceId))
            {
                if (__instance is ITalkingCard icard)
                {
                    voiceId = icard.FaceInfo.voiceId;
                }
            }

            if (!string.IsNullOrEmpty(voiceId))
            {
                __instance.Face.voiceSoundId = voiceId;
            }
            return false;
        }
    }
}