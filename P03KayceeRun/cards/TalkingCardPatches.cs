using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.TalkingCards.Create;
using Sirenix.Serialization.Utilities;
using UnityEngine;

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
                if (__instance.CardCameraParent.SafeIsUnityNull())
                {
                    __instance.CardCameraParent = CardRenderCamera.Instance.GetLiveRenderCamera(__instance.Card.StatsLayer)?.transform;
                }
                if (__instance.Text.SafeIsUnityNull())
                {
                    var text = __instance.gameObject.GetComponent<SequentialText>();
                    if (text.SafeIsUnityNull() && !__instance.CardCameraParent.SafeIsUnityNull())
                        text = __instance.CardCameraParent?.GetComponentInChildren<SequentialText>();
                    __instance.Text = text;
                }
                if (__instance.Face.SafeIsUnityNull())
                {
                    var face = __instance.gameObject.GetComponent<CharacterFace>();
                    if (face.SafeIsUnityNull() && !__instance.CardCameraParent.SafeIsUnityNull())
                        face = __instance.CardCameraParent?.GetComponentInChildren<CharacterFace>();
                    __instance.Face = face;
                }
            }
        }

        [HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.InterruptCurrentDialogue))]
        [HarmonyPostfix]
        private static IEnumerator EnsureTextNotNull(IEnumerator sequence, TalkingCard __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.CardCameraParent.SafeIsUnityNull())
                {
                    __instance.CardCameraParent = CardRenderCamera.Instance.GetLiveRenderCamera(__instance.Card.StatsLayer)?.transform;
                }
                if (__instance.Text.SafeIsUnityNull())
                {
                    var text = __instance.gameObject.GetComponent<SequentialText>();
                    if (text.SafeIsUnityNull() && !__instance.CardCameraParent.SafeIsUnityNull())
                        text = __instance.CardCameraParent?.GetComponentInChildren<SequentialText>();
                    __instance.Text = text;
                }
                if (__instance.Face.SafeIsUnityNull())
                {
                    var face = __instance.gameObject.GetComponent<CharacterFace>();
                    if (face.SafeIsUnityNull() && !__instance.CardCameraParent.SafeIsUnityNull())
                        face = __instance.CardCameraParent?.GetComponentInChildren<CharacterFace>();
                    __instance.Face = face;
                }
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.PlayLine))]
        [HarmonyPostfix]
        private static IEnumerator EnsureTextNotNullEachLine(IEnumerator sequence, TalkingCard __instance, DialogueEvent.Line line)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.CardCameraParent.SafeIsUnityNull())
                {
                    __instance.CardCameraParent = CardRenderCamera.Instance.GetLiveRenderCamera(__instance.Card.StatsLayer)?.transform;
                }
                if (__instance.Text.SafeIsUnityNull())
                {
                    var text = __instance.gameObject.GetComponent<SequentialText>();
                    if (text.SafeIsUnityNull() && !__instance.CardCameraParent.SafeIsUnityNull())
                        text = __instance.CardCameraParent?.GetComponentInChildren<SequentialText>();
                    __instance.Text = text;
                }
                if (__instance.Face.SafeIsUnityNull())
                {
                    var face = __instance.gameObject.GetComponent<CharacterFace>();
                    if (face.SafeIsUnityNull() && !__instance.CardCameraParent.SafeIsUnityNull())
                        face = __instance.CardCameraParent?.GetComponentInChildren<CharacterFace>();
                    __instance.Face = face;
                }
            }

            P03Plugin.Log.LogInfo(line.text);
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

        [HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.RespondsToDrawn))]
        [HarmonyPrefix]
        private static bool CheckNamesNotInstances(TalkingCard __instance, ref bool __result)
        {
            __result = !TurnManager.Instance.TalkingCardsDrawnThisGame.Any(i => i.name.Equals(__instance.Card.Info.name, System.StringComparison.InvariantCultureIgnoreCase));
            return false;
        }

        [HarmonyPatch(typeof(DiskTalkingCard), nameof(DiskTalkingCard.ManagedUpdate))]
        [HarmonyPostfix]
        private static void MakeSureWeReset(DiskTalkingCard __instance)
        {
            if (__instance.clearDialogueTimer < 0f && __instance.Text == null)
            {
                __instance.SetAbilityIconsShown(true);
            }
        }

        [HarmonyPatch(typeof(SelectableCardArray), nameof(SelectableCardArray.SelectCardFrom))]
        [HarmonyPostfix]
        private static IEnumerator ChangeToNegativeEffectSometimes(IEnumerator sequence, SelectableCardArray __instance, List<CardInfo> cards, CardPile pile, Action<SelectableCard> cardSelectedCallback, Func<bool> cancelCondition, bool forPositiveEffect)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            if (__instance == SpecialNodeHandler.Instance.tradeCardsSequencer?.cardArray
               || __instance == SpecialNodeHandler.Instance.recycleCardSequencer?.cardArray
               || __instance == SpecialNodeHandler.Instance.buildACardSequencer?.cardArray)
            {
                if (!forPositiveEffect)
                    yield return sequence;
                else
                    yield return __instance.SelectCardFrom(cards, pile, cardSelectedCallback, cancelCondition, false);
                yield break;
            }

            yield return sequence;
        }
    }
}