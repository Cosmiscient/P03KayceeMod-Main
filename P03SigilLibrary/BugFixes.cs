using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Regions;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary
{
    [HarmonyPatch]
    internal static class BugFixes
    {
        [HarmonyPatch(typeof(ItemsUtil), nameof(ItemsUtil.GetRandomUnlockedConsumable))]
        private static bool AccountForMultipleActs(int randomSeed, ref ConsumableItemData __result)
        {
            AbilityMetaCategory cat = SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Rulebook
                                      : SaveManager.SaveFile.IsPart3 ? AbilityMetaCategory.Part1Rulebook
                                      : SaveManager.SaveFile.IsGrimora ? AbilityMetaCategory.GrimoraRulebook
                                      : AbilityMetaCategory.MagnificusRulebook;

            List<ConsumableItemData> unlockedConsumables = cat == AbilityMetaCategory.Part1Rulebook
                                                           ? ItemsUtil.GetUnlockedConsumables()
                                                           : new(ItemsUtil.AllConsumables);

            if (unlockedConsumables.Any(cid => !cid.notRandomlyGiven))
                unlockedConsumables.RemoveAll(cid => cid.notRandomlyGiven);

            if (unlockedConsumables.Any(cid => cid.rulebookCategory == cat))
                unlockedConsumables.RemoveAll(cid => cid.rulebookCategory != cat);

            __result = unlockedConsumables[SeededRandom.Range(0, unlockedConsumables.Count, randomSeed)];
            return false;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AddTemporaryMod))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static void ProperlyRemoveSingletonTempMods(PlayableCard __instance, CardModificationInfo mod)
        {
            if (!string.IsNullOrEmpty(mod.singletonId))
            {
                CardModificationInfo cardModificationInfo = __instance.temporaryMods.Find(x => String.Equals(x.singletonId, mod.singletonId));
                if (cardModificationInfo != null)
                {
                    __instance.temporaryMods.Remove(cardModificationInfo);
                    foreach (Ability ability in cardModificationInfo.abilities)
                    {
                        __instance.TriggerHandler.RemoveAbility(ability);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPrefix]
        private static void AlwaysClearThePrefabPortrait(CardDisplayer3D __instance, PlayableCard playableCard)
        {
            if (playableCard != null
                && (__instance.info?.HasAbility(Ability.Transformer)).GetValueOrDefault(false)
                && (__instance.info.animatedPortrait != null || __instance.info.evolveParams?.evolution?.animatedPortrait != null))
            {
                if (__instance.instantiatedPortraitObj != null)
                {
                    GameObject.Destroy(__instance.instantiatedPortraitObj);
                }
                __instance.portraitPrefab = null;
            }
        }

        [HarmonyPatch(typeof(Card), nameof(Card.SetInfo))]
        [HarmonyPrefix]
        private static void RemoveApperanceBehavioursBeforeSettingInfo(Card __instance, CardInfo info)
        {
            if (__instance.Info != null && !__instance.Info.name.Equals(info.name))
            {
                foreach (CardAppearanceBehaviour.Appearance appearance in __instance.Info.appearanceBehaviour)
                {
                    Type type = CustomType.GetType("DiskCardGame", appearance.ToString());
                    Component c = __instance.gameObject.GetComponent(type);
                    if (c != null)
                    {
                        if (c is CardAppearanceBehaviour cab)
                            cab.ResetAppearance();
                        UnityEngine.Object.DestroyImmediate(c);
                    }
                }
                if (__instance.Anim is DiskCardAnimationController dcac)
                {
                    if (dcac.holoPortraitParent != null)
                    {
                        List<Transform> childrenToDelete = new();
                        foreach (Transform t in dcac.holoPortraitParent)
                            childrenToDelete.Add(t);

                        foreach (Transform t in childrenToDelete)
                            UnityEngine.Object.DestroyImmediate(t.gameObject);

                        dcac.holoPortraitParent.gameObject.SetActive(false);
                    }
                }
                // You know what - for good measure - just stop live rendering at all
                // If that's relevant
                CardRenderCamera.Instance?.StopLiveRenderCard(__instance.StatsLayer);
            }
        }

        [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
        [HarmonyPostfix]
        private static IEnumerator ShowDamageSequenceSkipIfLifeLossConditionMet(IEnumerator sequence)
        {
            if (TurnManager.Instance.LifeLossConditionsMet())
                yield break;

            yield return sequence;
        }

        [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.SetIconFlipped))]
        [HarmonyPostfix]
        private static void FlipLatchedAbility(ref CardAbilityIcons __instance, Ability ability, bool flipped)
        {
            if (__instance.latchIcon != null && __instance.latchIcon.Ability == ability)
                __instance.latchIcon.SetFlippedX(flipped);
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPrefix]
        [HarmonyBefore("ATS")] // This is a bugfix to help with an issue in ATS
        private static bool StopSequenceIfAttackerIsNull(CardSlot attackingSlot) => attackingSlot != null;

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
        [HarmonyPostfix]
        private static void GemifyBuffWithTempMods(PlayableCard __instance, ref int __result)
        {
            if (__instance.IsGemified() && !__instance.Info.Gemified)
            {
                if (__instance.OpponentCard)
                {
                    if ((OpponentGemsManager.Instance?.HasGem(GemType.Orange)).GetValueOrDefault())
                        __result += 1;
                }
                else
                {
                    if ((ResourcesManager.Instance?.HasGem(GemType.Orange)).GetValueOrDefault())
                        __result += 1;
                }
            }
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
        [HarmonyPostfix]
        private static void GemifyHealthBuffWithTempMods(PlayableCard __instance, ref int __result)
        {
            if (__instance.IsGemified() && !__instance.Info.Gemified)
            {
                if (__instance.OpponentCard)
                {
                    if ((OpponentGemsManager.Instance?.HasGem(GemType.Green)).GetValueOrDefault())
                        __result += 2;
                }
                else
                {
                    if ((ResourcesManager.Instance?.HasGem(GemType.Green)).GetValueOrDefault())
                        __result += 2;
                }
            }
        }

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.RenderCard))]
        [HarmonyPrefix]
        private static void AllowGemifyToWorkWithTempMods(ref RenderStatsLayer __instance, CardRenderInfo info)
        {
            if (__instance is not DiskRenderStatsLayer drsl)
                return;

            PlayableCard pCard = __instance.PlayableCard;
            if (pCard == null)
                return;

            if (pCard.IsGemified())
            {
                drsl.gemSquares.ForEach(o => o.SetActive(true));
                if (pCard.OpponentCard)
                {
                    if ((OpponentGemsManager.Instance?.HasGem(GemType.Orange)).GetValueOrDefault())
                        info.attackTextColor = GameColors.Instance.gold;
                    if ((OpponentGemsManager.Instance?.HasGem(GemType.Green)).GetValueOrDefault())
                        info.attackTextColor = GameColors.Instance.brightLimeGreen;
                }
                else
                {
                    if (ResourcesManager.Instance.HasGem(GemType.Orange))
                        info.attackTextColor = GameColors.Instance.gold;
                    if (ResourcesManager.Instance.HasGem(GemType.Green))
                        info.attackTextColor = GameColors.Instance.brightLimeGreen;
                }
            }
        }

        [HarmonyPatch(typeof(BoardStateEvaluator), nameof(BoardStateEvaluator.EvaluateBoardState))]
        [HarmonyPostfix]
        private static void FixCellEvaluation(BoardState state, ref int __result)
        {
            if (!SaveFile.IsAscension)
                return;

            foreach (BoardState.SlotState slot in state.opponentSlots)
            {
                if (slot.card != null && slot.card.info.HasCellAbility())
                {
                    float num4 = (state.opponentSlots.Count - 1) / 2f;
                    float num5 = Mathf.Abs(num4 - state.opponentSlots.IndexOf(slot));
                    __result -= 2 * Mathf.RoundToInt(num4 - num5);
                }
            }
        }
    }
}
