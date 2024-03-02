using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Faces;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class LifeManagement
    {
        //Read only
        public static int respawnCostExpensive = -15;

        //If the challenge is enabled, respawn cost is set to the expensive cost when the respawn cost goes uo
        public static int respawnCostIncrease = -5;

        public static int respawnCost = 0;

        [HarmonyPatch(typeof(Part3SaveData), "OnRespawn")]
        [HarmonyPrefix]
        public static void ManagePlayerLivesLeft()
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES))
            {
                // Reduce the number of lives
                EventManagement.NumberOfLivesRemaining--;
                EventManagement.NumberOfZoneEnemiesKilled = 0;
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), "OnRespawn")]
        [HarmonyPostfix]
        public static void BountHunterChallenge()
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BOUNTY_HUNTER))
            {
                ChallengeActivationUI.Instance.ShowActivation(AscensionChallengeManagement.BOUNTY_HUNTER);
                Part3SaveData.Data.bounty = 45 * 2;//AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallengeManagement.BOUNTY_HUNTER); // good fucking luck
            }
        }

        //Create our own respawn sequence and go through each potential life system

        public static IEnumerator DetractCoins(IEnumerator sequence)
        {
            //Enabling this means it triggers AFTER you respawn, lose all your money and fly with the drone 
            //yield return sequence;

            Debug.Log("DETRACT COINS TRIGGERED");

            //Make render quad invisible and make it visible again later
            GameObject projectingQuad = GameObject.Find("ProjectingLightQuad");
            Renderer renderer = projectingQuad.GetComponentInChildren<Renderer>();
            renderer.enabled = false;

            //If less lives is enabled, set the respawn cost to be expensive
            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.LessLives))
            {
                //This method makes sure it only triggers once per run,
                //And only when the respawn increase kicks in after the freebie
                if (!AscensionChallengeManagement.ExpensiveRespawnUIPlayed && (respawnCostIncrease == respawnCostExpensive))
                {
                    ChallengeActivationUI.Instance.ShowActivation(AscensionChallenge.LessLives);
                    AscensionChallengeManagement.ExpensiveRespawnUIPlayed = true;
                }

                respawnCostIncrease = respawnCostExpensive;
            }
            else
            {
                respawnCostIncrease = -5;
            }

            EventManagement.NumberOfLosses += 1;

            //If you lost to a boss, move out of the boss room before losing coins
            bool diedToBoss = (Part3GameFlowManager.Instance as Part3GameFlowManager).diedToBoss;
            if (diedToBoss)
            {
                ViewManager.Instance.SwitchToView(View.MapDefault, false, false);
                yield return new WaitForSeconds(0.1f);
                HoloMapAreaManager.Instance.MoveAreas(HoloMapAreaManager.Instance.CurrentArea, LookDirection.South, 1);
                yield return new WaitForSeconds(0.2f);
                yield return new WaitUntil(() => !HoloMapAreaManager.Instance.MovingAreas);
            }

            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default, true, true);

            P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
            View currentView = ViewManager.Instance.CurrentView;

            //P03AnimationController.Instance.SetProjectorLightActive(false);

            //There's a weird bug with this sequence that keeps the light coming out of P03's face... not sure how to fix that.

            //If the player hasn't had the paid respawn system explained, explain it
            if (!StoryEventsData.EventCompleted(EventManagement.SAW_P03_PAIDRESPAWN_EXPLAIN))
            {
                //yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return new WaitForSeconds(0.2f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionPaidRespawn", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                StoryEventsData.SetEventCompleted(EventManagement.SAW_P03_PAIDRESPAWN_EXPLAIN);
                yield return new WaitForSeconds(0.4f);
            }
            else
            {
                //If the player hasn't died to a boss, explain it

                //if (diedToBoss)
                if (diedToBoss && (!StoryEventsData.EventCompleted(EventManagement.SAW_P03_BOSSPAIDRESPAWN_EXPLAIN)))
                {
                    //yield return new WaitForSeconds(0.2f);
                    ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                    yield return new WaitForSeconds(0.2f);
                    yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossPaidRespawn", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    StoryEventsData.SetEventCompleted(EventManagement.SAW_P03_BOSSPAIDRESPAWN_EXPLAIN);
                    yield return new WaitForSeconds(0.4f);
                }
            }

            //Show increase in respawn cost
            //yield return new WaitForSeconds(0.4f);
            yield return P03PaidRespawnFace.ShowChangePRCost(respawnCost, true, diedToBoss);
            yield return new WaitForSeconds(0.4f);

            //Set the respawn cost to be your number of losses (- 1 to account for the freebie) multiplied by the respawn cost increase
            respawnCost = respawnCostIncrease * (EventManagement.NumberOfLosses - 1);

            //Double respawn cost if died to boss
            if (diedToBoss)
            {
                respawnCost *= 2;
            }

            //If the respawn cost puts currency in negative, end the run
            if (Part3SaveData.Data.currency + respawnCost < 0)
            {
                //Uncomment if you don't want a negative currency to be shown
                //currencyLoss = -Part3SaveData.Data.currency;
                yield return P03AnimationController.Instance.ShowChangeCurrency(respawnCost, true);
                yield return LostAscensionRunSequence();
                yield break;
            }

            //Show the player losing money
            yield return P03AnimationController.Instance.ShowChangeCurrency(respawnCost, true);
            Part3SaveData.Data.currency += respawnCost;

            //Re-enable projection
            renderer.enabled = true;

            //Reset respawn cost in case it was doubled by a boss loss
            respawnCost = respawnCostIncrease * (EventManagement.NumberOfLosses - 1);

            yield return new WaitForSeconds(0.2f);
            P03AnimationController.Instance.SwitchToFace(currentFace);
            yield return new WaitForSeconds(0.1f);

            Part3SaveData.WorldPosition worldPosition = new(HoloMapAreaManager.Instance.CurrentWorld.Id, HoloMapAreaManager.Instance.CurrentArea.GridX, HoloMapAreaManager.Instance.CurrentArea.GridY);
            HoloMapAreaManager.Instance.MoveToAreaDirectly(worldPosition);

            if (ViewManager.Instance.CurrentView != currentView)
            {
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.2f);
            }

            //HoloMapAreaManager.Instance.MoveToAreaDirectly(HoloMapAreaManager.Instance.CurrentArea.CenterPosition);
            //HoloMapAreaManager.Instance.MoveAreas(HoloMapAreaManager.Instance.CurrentArea, LookDirection.North);
            //HoloMapAreaManager.Instance.CurrentWorld.GetInstanceID();
            //HoloMapAreaManager.Instance.MoveToAreaDirectly(worldPosition);

            //Reset lives to 0
            Part3SaveData.Data.playerLives = 1;

            //Reset Bounty score
            Part3SaveData.Data.bounty = 45 * 2 * AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallengeManagement.BOUNTY_HUNTER);

            HoloMapAreaManager.Instance.CurrentArea.OnAreaActive();
            HoloMapAreaManager.Instance.CurrentArea.OnAreaEnabled();

            yield return sequence.Current;
        }

        [HarmonyPatch(typeof(Part3SaveData), "IncreaseBounty")]
        [HarmonyPrefix]
        private static void ResetLivesIfWon()
        {
            //If this is a P03 kaycee mod run AND trad lives arent on, reset lives if the player just won a battle
            //If the lives arent reset, there's a bug where the player keeps losing constantly even after a victory
            //Since the life count isn't > 0
            if (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES) && SaveFile.IsAscension)
            {
                Part3SaveData.Data.playerLives = 1;
                Debug.Log("Lives Set: " + Part3SaveData.Data.playerLives);
            }
        }

        [HarmonyPatch(typeof(Part3GameFlowManager), "PlayerRespawnSequence")]
        [HarmonyPostfix]
        public static IEnumerator ShowLivesAtStartOfRespawn(IEnumerator sequence)
        {
            //If the game file isn't P03's KCM then don't show lives at the start of respawn
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            SaveManager.savingDisabled = true;

            Debug.Log("Challenge Active? : " + AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES));

            //If the lives challenge isn't activated, do the detract coins sequence instead
            if (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES))
            {

                Debug.Log(Part3SaveData.Data.playerLives);
                Debug.Log("DetractCoins is about to trigger");

                yield return DetractCoins(sequence);

                yield return new WaitForEndOfFrame();
                SaveManager.savingDisabled = false;
                SaveManager.SaveToFile(true);
                yield break;
            }

            //This method makes sure it only triggers once per run
            if (!AscensionChallengeManagement.TradLivesUIPlayed)
            {
                ChallengeActivationUI.Instance.ShowActivation(AscensionChallengeManagement.TRADITIONAL_LIVES);
                AscensionChallengeManagement.TradLivesUIPlayed = true;
            }

            Debug.Log("SHOW LIVES TRIGGERED");

            bool hasShownLivesLost = false;
            while (sequence.MoveNext())
            {
                if (sequence.Current is WaitForSeconds && !hasShownLivesLost)
                {
                    yield return sequence.Current;

                    // Now we show the lives sequence
                    yield return P03LivesFace.ShowChangeLives(-1, true);
                    yield return new WaitForSeconds(0.1f);
                    hasShownLivesLost = true;

                    // And if we have no more lives, we stop this sequence entirely and move to the end of game sequence:
                    if (EventManagement.NumberOfLivesRemaining == 1) // It hasn't been decremented yet
                    {
                        yield return LostAscensionRunSequence();
                        yield break;
                    }

                    bool diedToBoss = Traverse.Create(Part3GameFlowManager.Instance).Field("diedToBoss").GetValue<bool>();
                    bool createBloodStain = !diedToBoss && Part3SaveData.Data.currency > 0;
                    if (!createBloodStain)
                    {
                        ViewManager.Instance.SwitchToView(View.MapDefault, false, false);
                        yield return new WaitForSeconds(0.1f);
                    }

                    continue;
                }
                yield return sequence.Current;
            }

            yield return new WaitForEndOfFrame();
            SaveManager.savingDisabled = false;
            SaveManager.SaveToFile(true);
            yield break;
        }

        private static IEnumerator LostAscensionRunSequence()
        {
            HoloGameMap.Instance.Jump();
            HoloGameMap.Instance.ShowPoweredOn(poweredOn: false);
            yield return new WaitForSeconds(0.1f);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default, true, true);
            yield return new WaitForSeconds(0.1f);

            List<string> dialogueOptions = new() {
                "Part3AscensionDeath",
                "Part3AscensionDeath2",
                "Part3AscensionDeath3",
                "Part3AscensionDeath4",
                "Part3AscensionDeath5"
            };

            if (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES))
            {
                dialogueOptions.Add("Part3AscensionDeathPay");
                dialogueOptions.Add("Part3AscensionDeathPay2");
                dialogueOptions.Add("Part3AscensionDeathPay3");
            }

            string finalDialogue = dialogueOptions[Random.Range(0, dialogueOptions.Count)];

            yield return TextDisplayer.Instance.PlayDialogueEvent(finalDialogue, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);
            EventManagement.FinishAscension(false);
            yield break;
        }

        [HarmonyPatch(typeof(Part3SaveData), "ResetBattlesInCurrentZone")]
        [HarmonyPrefix]
        public static bool ResetBattlesForAscension(ref Part3SaveData __instance)
        {
            if (SaveFile.IsAscension)
            {
                foreach (Part3SaveData.MapAreaStateData area in __instance.areaData)
                {
                    area.clearedBattles.ForEach(delegate (LookDirection x)
                    {
                        area.clearedDirections.Remove(x);
                    });
                    area.clearedBattles.Clear();
                }
                return false;
            }
            return true;
        }
    }
}