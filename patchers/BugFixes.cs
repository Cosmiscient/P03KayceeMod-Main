using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Regions;
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

        [HarmonyPatch(typeof(EvolveParams), nameof(EvolveParams.GetDefaultEvolution))]
        [HarmonyPrefix]
        internal static bool UpdateDefaultEvolutionWithCellEvolve(CardInfo info, ref CardInfo __result)
        {
            if (info.HasAbility(CellEvolve.AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;
                CardModificationInfo cardModificationInfo = new(0, 0)
                {
                    fromEvolve = true,

                    // Make it so the card doesn't copy this mod when it re-evolves
                    nonCopyable = true
                };

                // If this came from CellEvolve (i.e., the default evolution is de-evolving)
                // we don't need to change the name or change the attack or anything like that.
                // The de-evolution will end up remove the evolution mod and the card will revert
                // back to the original version.
                //
                // But if it does not have an evolve mod, we need to add a default de-evolve mod.
                if (!info.Mods.Any(m => m.fromEvolve))
                {
                    cardModificationInfo.nameReplacement = String.Format(Localization.Translate("{0} 2.0"), cardInfo.DisplayedNameLocalized);
                    cardModificationInfo.attackAdjustment = 1;
                    cardModificationInfo.healthAdjustment = 1;
                }

                cardModificationInfo.abilities = new() { CellDeEvolve.AbilityID };
                cardModificationInfo.negateAbilities = new() { CellEvolve.AbilityID };

                cardInfo.Mods.Add(cardModificationInfo);
                __result = cardInfo;

                return false;
            }
            if (info.HasAbility(CellDeEvolve.AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;
                CardModificationInfo cardModificationInfo = new(0, 0)
                {
                    fromEvolve = true,

                    // Make it so the card doesn't copy this mod when it de-evolves
                    nonCopyable = true
                };

                // If this came from CellDevEvolve (i.e., the default evolution is re-evolving)
                // we don't need to change the name or change the attack or anything like that.
                // The evolution will end up remove the de-evolution mod and the card will revert
                // back to the original version.
                //
                // But if it does not have an evolve mod, we need to add a default evolve mod.
                if (!info.Mods.Any(m => m.fromEvolve))
                {
                    cardModificationInfo.nameReplacement = string.Format(Localization.Translate("Beta {0}"), cardInfo.DisplayedNameLocalized);
                    cardModificationInfo.attackAdjustment = -1;
                }

                cardModificationInfo.abilities = new() { CellEvolve.AbilityID };
                cardModificationInfo.negateAbilities = new() { CellDeEvolve.AbilityID };

                cardInfo.Mods.Add(cardModificationInfo);
                __result = cardInfo;

                return false;
            }
            if (P03AscensionSaveData.IsP03Run)
            {
                CardInfo cardInfo = CardLoader.Clone(info);

                CardModificationInfo prevEvolveMod = info.Mods.FirstOrDefault(m => m.fromEvolve);
                if (prevEvolveMod != null)
                {
                    prevEvolveMod.attackAdjustment += 1;
                    prevEvolveMod.healthAdjustment += 1;

                    if (cardInfo.name.ToLowerInvariant().Contains("ringworm"))
                        prevEvolveMod.healthAdjustment += 1;

                    if (prevEvolveMod.nameReplacement.EndsWith(".0"))
                    {
                        int prevVersion = int.Parse(prevEvolveMod.nameReplacement
                                                       .Split(' ')
                                                       .Last()
                                                       .Replace(".0", ""));
                        prevEvolveMod.nameReplacement = prevEvolveMod.nameReplacement.Replace($"{prevVersion}.0", $"{prevVersion + 1}.0");
                    }
                    else
                    {
                        prevEvolveMod.nameReplacement += " 2.0";
                    }
                }
                else
                {
                    CardModificationInfo evolveMod = new(1, 1)
                    {
                        fromEvolve = true,
                        nameReplacement = cardInfo.DisplayedNameEnglish + " 2.0",
                        nonCopyable = false
                    };
                    cardInfo.mods.Add(evolveMod);
                }
                __result = cardInfo;

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(DiskScreenCardDisplayer), nameof(DiskScreenCardDisplayer.DisplayInfo))]
        [HarmonyPrefix]
        private static void AddThirdDecal(ref DiskScreenCardDisplayer __instance)
        {
            if (__instance.gameObject.transform.Find("Decal_Good") == null)
            {
                GameObject portrait = __instance.portraitRenderer.gameObject;
                GameObject decalFull = __instance.decalRenderers[1].gameObject;
                GameObject decalGood = UnityEngine.Object.Instantiate(decalFull, decalFull.transform.parent);
                decalGood.name = "Decal_Good";
                decalGood.transform.localPosition = portrait.transform.localPosition;
                decalGood.transform.localScale = new(1.2f, 1f, 0f);
                __instance.decalRenderers.Add(decalGood.GetComponent<Renderer>());
            }
        }

        [HarmonyPatch(typeof(FirstPersonAnimationController), nameof(FirstPersonAnimationController.SpawnFirstPersonAnimation))]
        [HarmonyPostfix]
        private static void EnsureActive(GameObject __result)
        {
            if (!__result.activeSelf)
                __result.SetActive(true);
        }

        [HarmonyPatch(typeof(Part3ResourcesManager), nameof(Part3ResourcesManager.CleanUp))]
        [HarmonyPostfix]
        private static IEnumerator Part3BonesReset(IEnumerator sequence, Part3ResourcesManager __instance)
        {
            yield return sequence;

            if (__instance.PlayerBones > 0)
            {
                yield return __instance.ShowSpendBones(__instance.PlayerBones);
                __instance.PlayerBones = 0;
            }
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.SwitchToScreen))]
        [HarmonyPrefix]
        private static void EnsureEverythingSyncs()
        {
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            RegionManager.SyncRegionList();
        }
    }
}