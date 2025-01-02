using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DigitalRuby.LightningBolt;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Sequences;
using Infiniscryption.P03SigilLibrary.Sigils;
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
        // private static bool CardHasFourOrLessIconsOnIt(CardInfo card)
        // {
        //     List<Ability> newAbilities = new();
        //     foreach (var ab in card.Abilities)
        //     {
        //         if (!newAbilities.Contains(ab))
        //         {
        //             newAbilities.Add(ab);
        //             continue;
        //         }
        //         var info = AbilitiesUtil.GetInfo(ab);
        //         if (!info.canStack)
        //         {
        //             newAbilities.Add(ab);
        //         }
        //     }
        //     return newAbilities.Count <= 4;
        // }

        // [HarmonyPatch(typeof(AddCardAbilitySequencer), nameof(AddCardAbilitySequencer.GetValidCardsFromDeck))]
        // [HarmonyPrefix]
        // internal static bool CountStackablesAsOne(ref List<CardInfo> __result)
        // {
        //     if (!P03AscensionSaveData.IsP03Run)
        //         return true;

        //     __result = Part3SaveData.Data.deck.Cards.Where(CardHasFourOrLessIconsOnIt).ToList();
        //     return false;
        // }

        [HarmonyPatch(typeof(CardAnimationController), nameof(CardAnimationController.SetMarkedForSacrifice))]
        [HarmonyPrefix]
        private static bool DiskCardSacrificed(CardAnimationController __instance, bool marked)
        {
            if (__instance is DiskCardAnimationController dcac)
            {
                dcac.StrongNegationEffect();
                if (marked)
                {
                    dcac.StrongNegationEffect();
                    dcac.lightningParent.SetActive(true);
                    foreach (var line in dcac.lightningParent.GetComponentsInChildren<LineRenderer>())
                    {
                        line.startColor = new(1, 0, 0, 0);
                        line.endColor = new(1, 0, 0, 1);
                    }
                }
                else
                {

                    bool shouldHaveLightning = dcac.Card.Info.HasAbility(Ability.PermaDeath) || dcac.Card.Info.Mods.Any(m => m.fromOverclock);
                    dcac.lightningParent.SetActive(shouldHaveLightning);
                    foreach (var line in dcac.lightningParent.GetComponentsInChildren<LineRenderer>())
                    {
                        line.startColor = new(1, 1, 1, 0);
                        line.endColor = new(1, 1, 1, .451f);
                    }
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Part3CombatPhaseManager), nameof(Part3CombatPhaseManager.ShowCardBlocked))]
        [HarmonyPostfix]
        private static IEnumerator PreventErrorWhenOutOfRegionCard(IEnumerator sequence, PlayableCard card)
        {
            if (card.Anim is DiskCardAnimationController dcac)
            {
                dcac.HideWeaponAnim();
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.15f);
            }
        }

        [HarmonyPatch(typeof(EvolveParams), nameof(EvolveParams.GetDefaultEvolution))]
        [HarmonyPrefix]
        internal static bool UpdateDefaultEvolutionWithCellEvolve(CardInfo info, ref CardInfo __result)
        {
            var pCard = info.GetPlayableCard();
            if (pCard == null)
            {
                P03Plugin.Log.LogWarning("Could not find a playable card for the info being transformed. This shouldn't happen!");
            }
            else
            {
                P03Plugin.Log.LogDebug($"Found playable card for info being transformed. It has {pCard.TemporaryMods.Count} temp mods");
            }
            if (pCard?.HasAbility(CellEvolve.AbilityID) ?? info.HasAbility(CellEvolve.AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;

                cardInfo.mods.RemoveAll(m => m.nonCopyable);
                CardModificationInfo cardModificationInfo = new(0, 0)
                {
                    fromEvolve = true,

                    // Make it so the card doesn't copy this mod when it re-evolves
                    nonCopyable = true
                };

                // If we got cell evolve from a totem, latch, or card merge, pretend this was too
                foreach (var mod in cardInfo.mods)
                {
                    if (mod.abilities.Contains(CellEvolve.AbilityID))
                    {
                        if (mod.fromCardMerge || mod.fromLatch || mod.fromTotem)
                        {
                            mod.abilities.Remove(CellEvolve.AbilityID);
                            cardModificationInfo.fromCardMerge = mod.fromCardMerge;
                            cardModificationInfo.fromLatch = mod.fromLatch;
                            cardModificationInfo.fromTotem = mod.fromTotem;
                            mod.fromLatch = false;
                            mod.fromCardMerge = false;
                            mod.fromTotem = false;
                        }
                    }
                }

                // If this came from CellEvolve (i.e., the default evolution is de-evolving)
                // we don't need to change the name or change the attack or anything like that.
                // The de-evolution will end up remove the evolution mod and the card will revert
                // back to the original version.
                //
                // But if it does not have an evolve mod, we need to add a default de-evolve mod.
                if (!info.Mods.Any(m => m.fromEvolve))
                {
                    cardModificationInfo.nameReplacement = info.GetNextEvolutionName();
                    cardModificationInfo.attackAdjustment = 1;
                    cardModificationInfo.healthAdjustment = 1;
                }

                cardModificationInfo.abilities = new() { CellDeEvolve.AbilityID };
                cardModificationInfo.negateAbilities = new() { CellEvolve.AbilityID };

                cardInfo.Mods.Add(cardModificationInfo);
                __result = cardInfo;

                return false;
            }
            if (pCard?.HasAbility(CellDeEvolve.AbilityID) ?? info.HasAbility(CellDeEvolve.AbilityID))
            {
                CardInfo cardInfo = info.Clone() as CardInfo;
                cardInfo.mods.RemoveAll(m => m.nonCopyable);
                CardModificationInfo cardModificationInfo = new(0, 0)
                {
                    fromEvolve = true,

                    // Make it so the card doesn't copy this mod when it de-evolves
                    nonCopyable = true
                };

                // If we got cell evolve from a totem, latch, or card merge, pretend this was too
                foreach (var mod in cardInfo.mods)
                {
                    if (mod.abilities.Contains(CellDeEvolve.AbilityID))
                    {
                        if (mod.fromCardMerge || mod.fromLatch || mod.fromTotem)
                        {
                            mod.abilities.Remove(CellDeEvolve.AbilityID);
                            cardModificationInfo.fromCardMerge = mod.fromCardMerge;
                            cardModificationInfo.fromLatch = mod.fromLatch;
                            cardModificationInfo.fromTotem = mod.fromTotem;
                            mod.fromLatch = false;
                            mod.fromCardMerge = false;
                            mod.fromTotem = false;
                        }
                    }
                }

                // If this came from CellDevEvolve (i.e., the default evolution is re-evolving)
                // we don't need to change the name or change the attack or anything like that.
                // The evolution will end up remove the de-evolution mod and the card will revert
                // back to the original version.
                //
                // But if it does not have an evolve mod, we need to add a default evolve mod.
                if (!info.Mods.Any(m => m.fromEvolve))
                {
                    cardModificationInfo.nameReplacement = info.GetNextDevolutionName();
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

                        prevEvolveMod.nameReplacement = info.GetNextEvolutionName();
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
                            nameReplacement = info.GetNextEvolutionName(),
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

        [HarmonyPatch(typeof(GainGem), nameof(GainGem.OnResolveOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator DontGainGemIfDead(IEnumerator sequence, GainGem __instance)
        {
            if (__instance.Card?.Dead ?? true)
                yield break;

            if (__instance.Card?.Slot == null)
                yield break;

            yield return sequence;
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

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TransformIntoCard))]
        [HarmonyPostfix]
        private static IEnumerator DieAfterTransformIfZeroHealth(IEnumerator sequence, PlayableCard __instance)
        {
            yield return sequence;

            if (!P03AscensionSaveData.IsP03Run)
                yield break;

            if (__instance.Health == 0)
                yield return __instance.Die(false);
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
            int health = __instance.Card.Health;// - __instance.Card.GetPassiveHealthBuffs();
            __instance.Card.HealDamage(__instance.Card.Status.damageTaken);
            int attack = __instance.Card.Attack;// - __instance.Card.GetPassiveAttackBuffs();
            __instance.mod.attackAdjustment = health - __instance.Card.TemporaryMods.Where(ShouldCountTempMod).Select(m => m.attackAdjustment).Sum();
            __instance.mod.healthAdjustment = attack - __instance.Card.TemporaryMods.Where(ShouldCountTempMod).Select(m => m.healthAdjustment).Sum();
            __instance.Card.OnStatsChanged();
            __instance.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.25f);
            if (__instance.Card.Health <= 0)
                yield return __instance.Card.Die(false, null, true);
            else
                yield return __instance.LearnAbility(0.25f);
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
        private static void MarkAsResurrectedBy(PlayableCard card, PlayableCard resurrectedBy)
        {
            if (resurrectedBy.Slot == null)
                return;

            string singletonId = $"{RESURRECTED_KEY}_{resurrectedBy.IsPlayerCard()}_{resurrectedBy.Slot.Index}";
            if (!card.AllCardModificationInfos().Any(m => string.Equals(singletonId, m.singletonId)))
                card.AddTemporaryMod(new() { singletonId = singletonId });
        }
        private static bool WasResurrectedBy(PlayableCard card, PlayableCard resurrectedBy)
        {
            if (resurrectedBy.Slot == null)
                return true;

            string singletonId = $"{RESURRECTED_KEY}_{resurrectedBy.IsPlayerCard()}_{resurrectedBy.Slot.Index}";
            return card.AllCardModificationInfos().Any(m => string.Equals(singletonId, m.singletonId));
        }

        [HarmonyPatch(typeof(DoubleDeath), nameof(DoubleDeath.RespondsToOtherCardDie))]
        [HarmonyPrefix]
        private static bool FixedDoubleDeathResponds(DoubleDeath __instance, PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer, ref bool __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            return __instance.Card.OnBoard && deathSlot.Card != null && deathSlot.Card.OpponentCard == __instance.Card.OpponentCard && deathSlot.Card != __instance.Card && !WasResurrectedBy(card, __instance.Card) && deathSlot.Card == card;
        }

        private static CardModificationInfo CloneAsNotCopyable(CardModificationInfo m)
        {
            var retval = m.Clone() as CardModificationInfo;
            retval.nonCopyable = true;
            return retval;
        }

        [HarmonyPatch(typeof(DoubleDeath), nameof(DoubleDeath.OnOtherCardDie))]
        [HarmonyPostfix]
        private static IEnumerator FixedDoubleDeath(IEnumerator sequence, CardSlot deathSlot, DoubleDeath __instance)
        {
            yield return __instance.PreSuccessfulTriggerSequence();
            CardInfo deathInfo = (CardInfo)deathSlot.Card.Info.Clone();
            deathInfo.mods = deathSlot.Card.Info.mods?.Select(CloneAsNotCopyable).ToList();
            deathInfo.mods ??= new();
            deathInfo.mods.AddRange(deathSlot.Card.TemporaryMods.Select(m => (CardModificationInfo)m.Clone()));
            __instance.currentlyResurrectingCards.Add(deathInfo);
            yield return BoardManager.Instance.CreateCardInSlot(deathInfo, deathSlot, 0.1f, false);
            if (deathSlot.Card != null)
            {
                MarkAsResurrectedBy(deathSlot.Card, __instance.Card);
                if (deathSlot.Card.TriggerHandler.RespondsToTrigger(Trigger.ResolveOnBoard))
                    yield return deathSlot.Card.TriggerHandler.OnTrigger(Trigger.ResolveOnBoard);

                yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.OtherCardResolve, false, deathSlot.Card);
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

        [HarmonyPatch(typeof(DropRubyOnDeath), nameof(DropRubyOnDeath.OnOtherCardDie))]
        [HarmonyPostfix]
        private static IEnumerator DRopRubyVesselInAct3(IEnumerator sequence, DropRubyOnDeath __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }
            if (__instance.Card.Info.temple != CardTemple.Tech)
            {
                yield return sequence;
                yield break;
            }
            yield return __instance.PreSuccessfulTriggerSequence();
            yield return new WaitForSeconds(0.1f);
            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("EmptyVessel_OrangeGem"), __instance.Card.Slot, 0.1f, true);
            yield return __instance.LearnAbility(0.5f);
            yield break;
        }
    }
}
