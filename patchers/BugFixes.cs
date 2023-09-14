using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class BugFixes
    {
        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AddTemporaryMod))]
        [HarmonyPrefix]
        private static bool ProperlyRemoveSingletonTempMods(PlayableCard __instance, CardModificationInfo mod)
        {
            if (!string.IsNullOrEmpty(mod.singletonId))
            {
                CardModificationInfo cardModificationInfo = __instance.temporaryMods.Find(x => String.Equals(x.singletonId, mod.singletonId));
                if (cardModificationInfo != null)
                {
                    __instance.RemoveTemporaryMod(mod, true);
                }
            }
            __instance.temporaryMods.Add(mod);
            using (List<Ability>.Enumerator enumerator = mod.abilities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Ability ability = enumerator.Current;
                    if (!__instance.temporaryMods.Exists((CardModificationInfo x) => x.negateAbilities.Contains(ability)))
                    {
                        __instance.TriggerHandler.AddAbility(ability);
                    }
                }
            }
            foreach (Ability ability2 in mod.negateAbilities)
            {
                __instance.TriggerHandler.RemoveAbility(ability2);
            }
            __instance.OnStatsChanged();
            return false;
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
    }
}