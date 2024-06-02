using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Regions;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class BugFixes
    {
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
                P03Plugin.Log.LogDebug($"Switching card from {__instance.Info.name} to {info.name}");
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
            if (!P03AscensionSaveData.IsP03Run)
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

        [HarmonyPatch(typeof(EvolveParams), nameof(EvolveParams.GetDefaultEvolution))]
        [HarmonyPrefix]
        internal static bool UpdateDefaultEvolutionWithCellEvolve(CardInfo info, ref CardInfo __result)
        {
            if (info.HasAbility(CellEvolve.AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;
                cardInfo.mods.RemoveAll(m => m.nonCopyable);
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
                cardInfo.mods.RemoveAll(m => m.nonCopyable);
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
                    if (cardInfo.name.Equals($"{P03Plugin.CardPrefx}_MineCart_Overdrive"))
                    {
                        int prevVersion = int.Parse(prevEvolveMod.nameReplacement.Replace("er", ""));
                        prevEvolveMod.nameReplacement = $"{prevVersion + 1}er";

                        int numberOfStrafes = prevEvolveMod.abilities.Where(a => a == Ability.Strafe).Count() + 1;
                        if (numberOfStrafes < 9)
                        {
                            prevEvolveMod.abilities.Add(Ability.Strafe);
                        }
                        else
                        {
                            int numberOfDoubleStrafes = prevEvolveMod.abilities.Where(a => a == DoubleSprint.AbilityID).Count() + 1;
                            if (numberOfDoubleStrafes < 9)
                            {
                                prevEvolveMod.abilities.Add(DoubleSprint.AbilityID);
                                prevEvolveMod.abilities.Remove(Ability.Strafe);
                            }
                            else
                            {
                                int nubmerOfRampagers = prevEvolveMod.abilities.Where(a => a == Ability.StrafeSwap).Count() + 1;
                                if (nubmerOfRampagers < 9)
                                {
                                    prevEvolveMod.abilities.Add(Ability.StrafeSwap);
                                }
                                else
                                {
                                    prevEvolveMod.healthAdjustment += 10;
                                    prevEvolveMod.attackAdjustment += 10;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cardInfo.HasAbility(Ability.BuffNeighbours))
                        {
                            prevEvolveMod.abilities.Add(Ability.BuffNeighbours);
                        }
                        else
                        {
                            prevEvolveMod.attackAdjustment += 1;
                            prevEvolveMod.healthAdjustment += 1;
                        }

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
                }
                else
                {
                    if (cardInfo.name.Equals($"{P03Plugin.CardPrefx}_MineCart_Overdrive"))
                    {
                        CardModificationInfo evolveMod = new();
                        evolveMod.abilities.Add(Ability.Strafe);
                        evolveMod.fromEvolve = true;
                        evolveMod.nonCopyable = false;
                        evolveMod.nameReplacement = "51er";
                        cardInfo.mods.Add(evolveMod);
                    }
                    else
                    {
                        CardModificationInfo evolveMod = new()
                        {
                            healthAdjustment = cardInfo.HasAbility(Ability.BuffNeighbours) ? 0 : 1,
                            attackAdjustment = cardInfo.HasAbility(Ability.BuffNeighbours) ? 0 : 1,
                            abilities = cardInfo.HasAbility(Ability.BuffNeighbours) ? new() { Ability.BuffNeighbours } : new(),
                            fromEvolve = true,
                            nameReplacement = cardInfo.DisplayedNameEnglish + " 2.0",
                            nonCopyable = false
                        };
                        cardInfo.mods.Add(evolveMod);
                    }
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
                decalGood.transform.localPosition = portrait.transform.localPosition + new Vector3(0f, 0f, -0.0001f);
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

        [HarmonyPatch(typeof(DialogueEventsData), nameof(DialogueEventsData.EventIsPlayed))]
        [HarmonyPrefix]
        private static bool NoFecundityCommentsForP03(string eventId, ref bool __result)
        {
            if (eventId.Equals("AscensionFecundityNerf") && P03AscensionSaveData.IsP03Run)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Attack), MethodType.Getter)]
        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.VeryLow)]
        // private static void SwapStatsAttackPatch(PlayableCard __instance, ref int __result)
        // {
        //     if (!__instance.HasAbility(Ability.SwapStats))
        //         return;

        //     SwapStats swapper = __instance.GetComponent<SwapStats>();
        //     if (swapper == null || !swapper.swapped)
        //         return;

        //     __result = Mathf.Max(0, __instance.MaxHealth - __instance.Status.damageTaken);
        // }

        // [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Health), MethodType.Getter)]
        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.VeryLow)]
        // private static void SwapStatsHealthPatch(PlayableCard __instance, ref int __result)
        // {
        //     if (!__instance.HasAbility(Ability.SwapStats))
        //         return;

        //     SwapStats swapper = __instance.GetComponent<SwapStats>();
        //     if (swapper == null || !swapper.swapped)
        //         return;

        //     __result = Mathf.Max(0, __instance.Info.Attack + __instance.GetAttackModifications() + __instance.GetPassiveAttackBuffs());
        // }

        // [HarmonyPatch(typeof(SwapStats), nameof(SwapStats.Start))]
        // [HarmonyPrefix]
        // private static bool DontDoThisForP03() => !P03AscensionSaveData.IsP03Run;

        private static bool ShouldCountTempMod(CardModificationInfo mod)
        {
            if (string.IsNullOrEmpty(mod.singletonId))
                return true;

            if (mod.singletonId.Equals("zeroout") || mod.singletonId.Equals("statswap"))
                return false;

            return true;
        }

        [HarmonyPatch(typeof(SwapStats), nameof(SwapStats.OnTakeDamage))]
        [HarmonyPostfix]
        private static IEnumerator SimpleSwapStatsFixHopefully(IEnumerator sequence, SwapStats __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            P03Plugin.Log.LogInfo("In custom swap stats damage trigger!");
            yield return new WaitForSeconds(0.5f);
            if (__instance.Card.Info.name == "SwapBot")
            {
                __instance.swapped = !__instance.swapped;
                if (__instance.swapped)
                {
                    __instance.Card.SwitchToAlternatePortrait();
                }
                else
                {
                    __instance.Card.SwitchToDefaultPortrait();
                }
            }
            int health = __instance.Card.Health - __instance.Card.GetPassiveHealthBuffs();
            __instance.Card.HealDamage(__instance.Card.Status.damageTaken);
            int attack = __instance.Card.Attack - __instance.Card.GetPassiveAttackBuffs();
            __instance.mod.attackAdjustment = health - __instance.Card.TemporaryMods.Where(ShouldCountTempMod).Select(m => m.attackAdjustment).Sum();
            __instance.mod.healthAdjustment = attack - __instance.Card.TemporaryMods.Where(ShouldCountTempMod).Select(m => m.healthAdjustment).Sum();
            __instance.Card.OnStatsChanged();
            __instance.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.25f);
            yield return __instance.Card.Health <= 0 ? __instance.Card.Die(false, null, true) : (object)__instance.LearnAbility(0.25f);
            yield break;
        }

        [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.AddCard))]
        [HarmonyPrefix]
        private static bool DontRemodCardsThanksToAPICopyingModsOnClone(DeckInfo __instance, CardInfo card, ref CardInfo __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            CardInfo cardInfo = card.Clone() as CardInfo;
            __instance.Cards.Add(cardInfo);
            __instance.cardIds.Add(card.name);
            __instance.UpdateModDictionary();
            __result = cardInfo;
            return false;
        }

        [HarmonyPatch(typeof(DrawCopy), nameof(DrawCopy.CardToDrawTempMods), MethodType.Getter)]
        [HarmonyPostfix]
        private static void EnsureAllFecundityOverrides(ref List<CardModificationInfo> __result)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __result[0].negateAbilities.Add(Ability.DrawCopy);
            }
        }


        [HarmonyPatch(typeof(ExplodeOnDeath), nameof(ExplodeOnDeath.BombCard))]
        [HarmonyPostfix]
        private static IEnumerator PreventErrors(IEnumerator sequence, ExplodeOnDeath __instance, PlayableCard target, PlayableCard attacker)
        {
            if (target == null || attacker == null || __instance.bombPrefab == null || target.Anim == null)
                yield break;

            while (sequence.MoveNext())
            {
                if (target == null || attacker == null || __instance.bombPrefab == null || target.Anim == null)
                    yield break;

                yield return sequence.Current;
            }
            yield break;
        }

        [HarmonyPatch(typeof(PhotographerSnapshotManager), nameof(PhotographerSnapshotManager.ApplySlotState))]
        [HarmonyPostfix]
        private static IEnumerator DontLetTempModsGetOverwritten(IEnumerator sequence, BoardState.SlotState slotState, CardSlot slot)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            yield return BoardManager.Instance.CreateCardInSlot(slotState.card.info, slot, 0f, false);
            PlayableCard card = slot.Card;
            (card.Anim as DiskCardAnimationController).Expand(true);
            foreach (var mod in slotState.card.temporaryMods)
            {
                if (string.IsNullOrEmpty(mod.singletonId) || !card.TemporaryMods.Any(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(mod.singletonId)))
                    card.TemporaryMods.Add(mod);
            }
            //card.TemporaryMods = new List<CardModificationInfo>(slotState.card.temporaryMods);
            card.Status = new PlayableCardStatus(slotState.card.status);
            card.OnStatsChanged();
            ResourcesManager.Instance.ForceGemsUpdate();
            foreach (var restorer in card.GetComponents<IRestoreFromSnapshot>())
                restorer?.RestoreFromSnapshot(slotState.card);
            yield break;
        }

        [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.HasTrait))]
        [HarmonyPostfix]
        private static void ChangeGemTraitBehavior(CardInfo __instance, Trait trait, ref bool __result)
        {
            if (P03AscensionSaveData.IsP03Run && trait == Trait.Gem && !__result)
            {
                if (__instance.HasAnyOfAbilities(Ability.GainGemBlue, Ability.GainGemGreen, Ability.GainGemOrange, Ability.GainGemTriple))
                {
                    __result = true;
                    return;
                }

                var pCard = __instance.GetPlayableCard();
                if (pCard != null && pCard.Info == __instance)
                {
                    if (pCard.HasAnyOfAbilities(Ability.GainGemBlue, Ability.GainGemGreen, Ability.GainGemOrange, Ability.GainGemTriple))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FriendCardCreator), nameof(FriendCardCreator.FriendToCard))]
        [HarmonyPostfix]
        private static void MaxEnergyCostOfSix(CardInfo __result)
        {
            if (__result != null && __result.Mods != null)
                foreach (var mod in __result.Mods.Where(m => m != null))
                    mod.energyCostAdjustment = Mathf.Min(mod.energyCostAdjustment, 6);
        }

        [HarmonyPatch(typeof(TextDisplayer), nameof(TextDisplayer.ManagedUpdate))]
        [HarmonyPrefix]
        private static void AdvanceTextWithSpacebar(TextDisplayer __instance)
        {
            if (InputButtons.GetButtonUp(Button.EndTurn))
                __instance.continuePressed = true;
        }

        [HarmonyPatch(typeof(Opponent), nameof(Opponent.LoadBlueprint))]
        [HarmonyPostfix]
        private static void OpponentLoadBlueprint(string blueprintId, ref EncounterBlueprintData __result)
        {
            __result ??= EncounterManager.AllEncountersCopy.FirstOrDefault(e => e.name.Equals(blueprintId, StringComparison.InvariantCultureIgnoreCase));
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ChooseTarget))]
        [HarmonyPostfix]
        private static IEnumerator ChoosingTargetLocksUpSlots(IEnumerator sequence, BoardManager __instance)
        {
            __instance.ChoosingSlot = true;
            yield return sequence;
            __instance.ChoosingSlot = false;
        }

        [HarmonyPatch(typeof(SelectableCardArray), nameof(SelectableCardArray.SelectCardFrom))]
        [HarmonyPostfix]
        private static IEnumerator EnsureCursorSelectable(IEnumerator sequence)
        {
            bool interactionDisabled = InteractionCursor.Instance.InteractionDisabled;
            InteractionCursor.Instance.InteractionDisabled = false;
            yield return sequence;
            InteractionCursor.Instance.InteractionDisabled = interactionDisabled;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.SetInfo))]
        [HarmonyPrefix]
        private static void ResetHidePortrait(Card __instance)
        {
            __instance.renderInfo.hidePortrait = false;
        }

        [HarmonyPatch(typeof(HoloMapNode), nameof(HoloMapNode.Awake))]
        [HarmonyPrefix]
        private static void EnsureNotNullRenderers(HoloMapNode __instance)
        {
            __instance.nodeRenderers ??= new();
        }

        [HarmonyPatch(typeof(BuildACardInfo), nameof(BuildACardInfo.Initialize))]
        [HarmonyPostfix]
        private static void BACNotCopyable(BuildACardInfo __instance)
        {
            __instance.mod.nonCopyable = true;
        }

        [HarmonyPatch(typeof(BuildACardInfo), nameof(BuildACardInfo.ToCardInfo))]
        [HarmonyPostfix]
        private static void BuildCardDifferently(ref CardInfo __result)
        {
            __result.mods.Add(new(__result.mods[0].attackAdjustment, __result.mods[0].healthAdjustment));
            __result.mods[0].attackAdjustment = 0;
            __result.mods[0].healthAdjustment = 0;
        }

        [HarmonyPatch(typeof(BoardState.SlotState))]
        [HarmonyPatch(new Type[] { typeof(CardSlot) })]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPrefix]
        private static bool RemoveGoobertDuplicates(BoardState.SlotState __instance, CardSlot slot)
        {
            if (slot != null && slot.Card != null && slot.Card.slot != slot)
                return false;
            return true;
        }

        private static readonly Trait NonDoubleDeathable = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "NoDoubleDeath");

        private const string RESURRECTED_KEY = "HasBeenResurrectedByDoubleDeath";
        private static bool CanBeDoubleDeathed(PlayableCard card) => !card.TemporaryMods.Any(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(RESURRECTED_KEY));

        [HarmonyPatch(typeof(DoubleDeath), nameof(DoubleDeath.RespondsToOtherCardDie))]
        [HarmonyPrefix]
        private static bool FixedDoubleDeathResponds(DoubleDeath __instance, PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer, ref bool __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            return __instance.Card.OnBoard && deathSlot.Card != null && deathSlot.Card.OpponentCard == __instance.Card.OpponentCard && deathSlot.Card != __instance.Card && CanBeDoubleDeathed(card) && deathSlot.Card == card;
        }

        [HarmonyPatch(typeof(DoubleDeath), nameof(DoubleDeath.OnOtherCardDie))]
        [HarmonyPostfix]
        private static IEnumerator FixedDoubleDeath(IEnumerator sequence, CardSlot deathSlot, DoubleDeath __instance)
        {
            yield return __instance.PreSuccessfulTriggerSequence();
            CardInfo deathInfo = (CardInfo)deathSlot.Card.Info.Clone();
            deathInfo.mods = deathSlot.Card.Info.mods?.Select(m => (CardModificationInfo)m.Clone()).ToList();
            deathInfo.mods ??= new();
            deathInfo.mods.AddRange(deathSlot.Card.TemporaryMods.Select(m => (CardModificationInfo)m.Clone()));
            __instance.currentlyResurrectingCards.Add(deathInfo);
            yield return BoardManager.Instance.CreateCardInSlot(deathInfo, deathSlot, 0.1f, false);
            if (deathSlot.Card != null)
            {
                deathSlot.Card.AddTemporaryMod(new() { singletonId = RESURRECTED_KEY });
                if (deathSlot.Card.TriggerHandler.RespondsToTrigger(Trigger.ResolveOnBoard))
                    yield return deathSlot.Card.TriggerHandler.OnTrigger(Trigger.ResolveOnBoard);

                yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.OtherCardResolve, false, deathSlot.Card);
            }
            yield return new WaitForSeconds(0.1f);
            if (deathSlot.Card != null)
            {
                yield return deathSlot.Card.Die(false, __instance.Card, true);
            }
            yield return __instance.LearnAbility(0.5f);
            __instance.currentlyResurrectingCards.Clear();
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        private static IEnumerator EnsureAssignCardToSlotHappensOnlyWhenEverythingIsGood(IEnumerator sequence, PlayableCard card, CardSlot slot)
        {
            if (card.SafeIsUnityNull() || card.Dead)
                yield break;

            if (slot.SafeIsUnityNull())
                yield break;

            yield return sequence;
        }
    }
}
