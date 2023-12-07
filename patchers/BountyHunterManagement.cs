using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Sequences;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BountyHunterManagement
    {
        private static bool SlotHasBrain(CardSlot slot)
        {
            if (slot.Card != null && slot.Card.Info.name.Equals(CustomCards.BRAIN))
                return true;

            Card queueCard = BoardManager.Instance.GetCardQueuedForSlot(slot);
            return queueCard != null && queueCard.Info.name.Equals(CustomCards.BRAIN);
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static IEnumerator AcquireBrain(IEnumerator sequence)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                if (BoardManager.Instance.AllSlotsCopy.Any(SlotHasBrain))
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03BountyHunterBrain", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
                    brain.mods = new() {
                        new(Ability.DrawRandomCardOnDeath)
                    };
                    Part3SaveData.Data.deck.AddCard(brain);
                }
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.OnDie))]
        [HarmonyPostfix]
        public static IEnumerator EarnCurrencyWhenBountyHunterDies(IEnumerator sequence, PlayableCard killer, BountyHunter __instance)
        {
            yield return sequence;

            if (!P03AscensionSaveData.IsP03Run || TurnManager.Instance.Opponent is P03AscensionOpponent) // don't do this on the final boss
                yield break;

            P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
            View currentView = ViewManager.Instance.CurrentView;
            yield return new WaitForSeconds(0.4f);
            int currencyGain = Part3SaveData.Data.BountyTier * 3;
            yield return P03AnimationController.Instance.ShowChangeCurrency(currencyGain, true);
            Part3SaveData.Data.currency += currencyGain;
            yield return new WaitForSeconds(0.2f);
            P03AnimationController.Instance.SwitchToFace(currentFace);
            yield return new WaitForSeconds(0.1f);
            if (ViewManager.Instance.CurrentView != currentView)
            {
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.2f);
            }

            // Don't spawn the brain in the following situations:
            if (killer != null && (killer.HasAbility(Ability.Deathtouch) || killer.HasAbility(Ability.SteelTrap)))
                yield break;

            // Spawn at most one per run
            if (StoryEventsData.EventCompleted(EventManagement.SAW_BOUNTY_HUNTER_MEDAL))
                yield break;

            // This can only happen on the very first bounty hunter of the run. You get exactly one shot
            if (Part3SaveData.Data.bountyHunterMods.Count != 1)
                yield break;

            // Get the brain but take the ability off of it
            CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
            yield return BoardManager.Instance.CreateCardInSlot(brain, (__instance.Card as PlayableCard).Slot, 0.15f, true);
            StoryEventsData.SetEventCompleted(EventManagement.SAW_BOUNTY_HUNTER_MEDAL);
        }

        [HarmonyPatch(typeof(Part3CombatPhaseManager), nameof(Part3CombatPhaseManager.VisualizeExcessLethalDamage))]
        [HarmonyPostfix]
        private static void ExtraPunishmentForExcessDamage(int excessDamage)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (excessDamage > 3)
                Part3SaveData.Data.IncreaseBounty(excessDamage - 3);

            if (excessDamage > 10) // wow you're *really* trying hard here
                Part3SaveData.Data.IncreaseBounty(excessDamage - 10);
        }

        [HarmonyPatch(typeof(HoloBountyIcons), nameof(HoloBountyIcons.ManagedUpdate))]
        [HarmonyPrefix]
        private static void AddBountyTiersThroughSix(HoloBountyIcons __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (__instance.bountyTierObjects.Length == 3)
            {
                GameObject tierFour = Object.Instantiate(__instance.bountyTierObjects[1], __instance.bountyTierObjects[1].transform.parent);
                tierFour.name = "Level_4";
                foreach (string name in new string[] { "Star", "Star (1)" })
                {
                    GameObject star = Object.Instantiate(tierFour.transform.Find(name).gameObject, tierFour.transform);
                    star.transform.localPosition = new(star.transform.localPosition.x, 0.2f, star.transform.localPosition.z);
                }
                tierFour.SetActive(false);

                GameObject tierFive = Object.Instantiate(__instance.bountyTierObjects[2], __instance.bountyTierObjects[2].transform.parent);
                tierFive.name = "Level_5";
                foreach (string name in new string[] { "Star", "Star (1)" })
                {
                    GameObject star = Object.Instantiate(tierFive.transform.Find(name).gameObject, tierFive.transform);
                    star.transform.localPosition = new(star.transform.localPosition.x + 0.1f, 0.2f, star.transform.localPosition.z);
                }
                tierFive.SetActive(false);

                GameObject tierSix = Object.Instantiate(__instance.bountyTierObjects[2], __instance.bountyTierObjects[2].transform.parent);
                tierSix.name = "Level_6";
                foreach (string name in new string[] { "Star", "Star (1)", "Star (2)" })
                {
                    GameObject star = Object.Instantiate(tierSix.transform.Find(name).gameObject, tierSix.transform);
                    star.transform.localPosition = new(star.transform.localPosition.x, 0.2f, star.transform.localPosition.z);
                }
                tierSix.SetActive(false);

                List<GameObject> newStars = new(__instance.bountyTierObjects) {
                    tierFour,
                    tierFive,
                    tierSix
                };
                __instance.bountyTierObjects = newStars.ToArray();
            }
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.IntroductionSequence))]
        [HarmonyPrefix]
        private static void ResetBountyHunterTurns()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            Part3SaveData.Data.battlesSinceBountyHunter = 0;
        }

        [HarmonyPatch(typeof(BountyHunterGenerator), nameof(BountyHunterGenerator.TryAddBountyHunterToTurnPlan))]
        [HarmonyPrefix]
        private static bool NewGeneratorLogic(List<List<CardInfo>> turnPlan, ref List<List<CardInfo>> __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            int currentRandomSeed = P03AscensionSaveData.RandomSeed;
            int bounty = Part3SaveData.Data.bounty;
            if (ProgressionData.LearnedMechanic(MechanicsConcept.Part3Bounty) && Part3SaveData.Data.BountyTier >= 1 && SeededRandom.Value(currentRandomSeed++) < Part3SaveData.Data.battlesSinceBountyHunter * 0.4f)
            {
                List<CardModificationInfo> list = Part3SaveData.Data.bountyHunterMods.FindAll((CardModificationInfo x) => x.bountyHunterInfo.tier == Part3SaveData.Data.BountyTier);
                CardModificationInfo cardModificationInfo;
                int turn;
                if (list.Count > 0 && SeededRandom.Value(currentRandomSeed++) < list.Count * 0.33f)
                {
                    cardModificationInfo = list[SeededRandom.Range(0, list.Count, currentRandomSeed++)];
                    turn = cardModificationInfo.energyCostAdjustment;
                }
                else
                {
                    turn = SeededRandom.Range(3, 6, currentRandomSeed++) - 1;
                    cardModificationInfo = BountyHunterGenerator.GenerateMod(turn, bounty);
                }
                turn = Mathf.Clamp(turn, 0, turnPlan.Count - 1);
                if (turn > 0 && Part3SaveData.Data.battlesSinceBountyHunter > 4)
                    turn--;
                if (turn > 0 && Part3SaveData.Data.battlesSinceBountyHunter > 6)
                    turn--;
                CardInfo cardInfo = null;
                while (cardInfo == null && turn > 0)
                {
                    if (turnPlan.Count > 0)
                    {
                        cardInfo = turnPlan[turn].Find((CardInfo x) => x.PowerLevel < BountyHunterGenerator.GetStatPoints(turn, bounty));
                    }
                    if (cardInfo == null)
                    {
                        turn--;
                    }
                }
                if (cardInfo != null)
                {
                    turnPlan[turn].Remove(cardInfo);
                    turnPlan[turn].Add(BountyHunterGenerator.GenerateCardInfo(cardModificationInfo));
                    if (!Part3SaveData.Data.bountyHunterMods.Contains(cardModificationInfo))
                    {
                        Part3SaveData.Data.bountyHunterMods.Add(cardModificationInfo);
                    }
                }
            }
            Part3SaveData.Data.battlesSinceBountyHunter++;
            __result = turnPlan;
            return false;
        }
    }
}