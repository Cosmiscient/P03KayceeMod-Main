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
    }
}