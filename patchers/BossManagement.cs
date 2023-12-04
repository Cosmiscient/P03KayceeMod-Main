using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Encounters;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BossManagement
    {
        public static readonly string P03FinalBossAI = AIManager.Add(P03Plugin.PluginGuid, "P03FinalBossAI", typeof(P03FinalBossOpponentAI)).Id;
        public static Opponent.Type P03FinalBossOpponent { get; private set; }

        private const int BOSS_MONEY_REWARD = 5;

        //10 was way too quiet... 0.15?
        public static float bossMusicVolume = 0.15f;

        [HarmonyPatch(typeof(DamageRaceBattleSequencer), nameof(DamageRaceBattleSequencer.DamageAddedToScale))]
        [HarmonyPostfix]
        private static IEnumerator FixPlayerDamage(IEnumerator sequence)
        {
            yield return sequence;
            LifeManager.Instance.PlayerDamage = 0;
            yield break;
        }

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.IntroSequence))]
        [HarmonyPostfix]
        public static IEnumerator ReduceLivesOnBossNode(IEnumerator sequence)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            //If traditional lives isnt activated, set the number of lives remaning to 1
            if (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES))
            {
                EventManagement.NumberOfLivesRemaining = 1;
            }

            bool hasShownLivesDrop = false;
            while (sequence.MoveNext())
            {

                if (sequence.Current is WaitForSeconds)
                {
                    yield return sequence.Current;
                    sequence.MoveNext();

                    if (EventManagement.NumberOfLivesRemaining > 1 && !hasShownLivesDrop)
                    {
                        int livesToDrop = EventManagement.NumberOfLivesRemaining - 1;
                        yield return P03LivesFace.ShowChangeLives(-livesToDrop, true);
                        EventManagement.NumberOfLivesRemaining = 1;

                        yield return EventManagement.SayDialogueOnce("P03OnlyOneBossLife", EventManagement.ONLY_ONE_BOSS_LIFE);
                    }
                    hasShownLivesDrop = true;
                }
                yield return sequence.Current;
            }

            yield break;
        }

        [HarmonyPatch(typeof(CanvasBossOpponent), nameof(CanvasBossOpponent.IntroSequence))]
        [HarmonyPostfix]
        public static IEnumerator CanvasResetLives(IEnumerator sequence)
        {
            yield return ReduceLivesOnBossNode(sequence);

            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.BossDefeatedSequence))]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03ResetLives(IEnumerator sequence, Part3BossOpponent __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            if (P03AscensionSaveData.IsP03Run && AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES))
            {
                // Reset lives to maximum
                if (EventManagement.NumberOfLivesRemaining < AscensionSaveData.Data.currentRun.maxPlayerLives)
                {
                    int livesToAdd = AscensionSaveData.Data.currentRun.maxPlayerLives - EventManagement.NumberOfLivesRemaining;
                    yield return P03LivesFace.ShowChangeLives(livesToAdd, true);
                    yield return new WaitForSeconds(0.5f);
                    EventManagement.NumberOfLivesRemaining = AscensionSaveData.Data.currentRun.maxPlayerLives;
                }
            }

            if (__instance is not P03AscensionOpponent and not MycologistAscensionBossOpponent)
            {
                TurnManager.Instance.PostBattleSpecialNode = AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoBossRares)
                    ? new CardChoicesNodeData()
                    : (SpecialNodeData)new CardChoiceGenerator.Part3RareCardChoicesNodeData();
            }

            yield return sequence;
            yield break;
        }

        private static IEnumerator DropLoot(IEnumerator sequence, HoloMapBossNode bossNode)
        {
            Debug.Log("Droploot begin");
            List<HoloMapNode> droppedNodes;

            droppedNodes = new List<HoloMapNode>(); //this line shouldnt be necessary wtf

            droppedNodes.ForEach(delegate (HoloMapNode x)
            {
                if (x != null)
                {
                    Debug.Log("Triggered");
                    x.gameObject.SetActive(!x.Completed);
                }
            });

            foreach (HoloMapNode lootNode in droppedNodes)
            {
                lootNode.PlaySpawnAnimation(bossNode.transform.position, lootNode.transform.position);
                Debug.Log("Triggered 1");
                yield return new WaitForSeconds(0.1f);
            }

            //HoloMapGainCurrencyNode holoMapGainCurrencyNode = new HoloMapGainCurrencyNode();
            //holoMapGainCurrencyNode.amount = 3;
            //holoMapGainCurrencyNode.SetActive(true);
            //holoMapGainCurrencyNode.SetEnabled(true);
            //holoMapGainCurrencyNode.gameObject.
            ////holoMapGainCurrencyNode.transform.parent = bossNode.transform;
            ////holoMapGainCurrencyNode.gameObject.SetActive(true);
            //droppedNodes.Add(holoMapGainCurrencyNode);

            //Debug.Log("List Size: " + droppedNodes.Count());
            //foreach (HoloMapNode lootNode in droppedNodes)
            //{
            //    bool lootnodeNull;
            //    if (lootNode == null)
            //    {
            //        lootnodeNull = true;
            //    }
            //    else
            //    {
            //        lootnodeNull = false;
            //    }

            //    Debug.Log("Is lootnode null? " + lootnodeNull);

            //    if (bossNode == null)
            //    {
            //        lootnodeNull = true;
            //    }
            //    else
            //    {
            //        lootnodeNull = false;
            //    }

            //    Debug.Log("Is boss node null? " + lootnodeNull);
            //    lootNode.PlaySpawnAnimation(bossNode.transform.position, lootNode.transform.position);
            //    Debug.Log("7");
            //    yield return new WaitForSeconds(0.1f);
            //}
            Debug.Log("Droploot over");
        }

        [HarmonyPatch(typeof(HoloMapBossNode), nameof(HoloMapBossNode.BossDefeatedSequence))]
        [HarmonyPostfix]
        private static IEnumerator FixForMycologists(IEnumerator sequence, HoloMapBossNode __instance)
        {
            // The game doesn't play the normal boss defeated sequence when you beat the mycologists because
            // they are a different sort of boss. We need mycologists to behave more normally, hence this patch
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.bossDefeatedStoryEvent == StoryEvent.MycologistsDefeated)
                {
                    (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = true;
                    ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
                    yield return new WaitForSeconds(0.1f);
                    yield return new WaitUntil(() => HoloGameMap.Instance.FullyUnrolled);
                    StoryEventsData.SetEventCompleted(__instance.bossDefeatedStoryEvent, false, true);
                    HoloGameMap.Instance.ShowBossDefeated(__instance.bossDefeatedStoryEvent);
                    yield break;
                }

                //yield return DropLoot(sequence, __instance);
            }
            yield return sequence;
        }

        [HarmonyPatch(typeof(HoloGameMap), nameof(HoloGameMap.BossDefeatedSequence))]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03BossDefeatedSequence(IEnumerator sequence, StoryEvent bossDefeatedStoryEvent)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            P03Plugin.Log.LogDebug($"Defeated boss {bossDefeatedStoryEvent}");

            AscensionStatsData.TryIncrementStat(AscensionStat.Type.BossesDefeated);

            if (bossDefeatedStoryEvent != StoryEvent.MycologistsDefeated)
            {
                EventManagement.AddCompletedZone(bossDefeatedStoryEvent);

                // if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoBossRares))
                // {
                //     Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));
                //     ChallengeActivationUI.TryShowActivation(AscensionChallenge.NoBossRares);
                //     yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossDraftToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                // }
                // else
                // {
                //     Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));
                //     yield return TextDisplayer.Instance.PlayDialogueEvent("Part3AscensionBossRareToken", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                // }

                yield return new WaitForSeconds(0.2f);

                if (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TRADITIONAL_LIVES))
                {
                    // P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default, true, true);

                    // P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
                    // View currentView = ViewManager.Instance.CurrentView;

                    // ViewManager.Instance.SwitchToView(View.P03Face, false, true);
                    // yield return new WaitForSeconds(0.05f);

                    // HoloGameMap.Instance.Jump();
                    // HoloGameMap.Instance.ShowPoweredOn(poweredOn: false);
                    // yield return new WaitForSeconds(0.2f);

                    //Increase respawn cost if the player has already dropped below 0
                    //if (LifeManagement.respawnCost != 0)
                    //{
                    //    yield return P03PaidRespawnFace.ShowChangePRCost(-LifeManagement.respawnCost, true);
                    //    yield return new WaitForSeconds(0.4f);
                    //}

                    // List<string> dialogueOptions = new() {
                    //     "Part3AscensionPayBoss",
                    //     "Part3AscensionPayBoss2",
                    //     "Part3AscensionPayBoss3",
                    //     "Part3AscensionPayBoss4",
                    //     "Part3AscensionPayBoss5",
                    //     "Part3AscensionPayBoss6",
                    //     "Part3AscensionPayBoss7",
                    //     "Part3AscensionPayBoss8",
                    //     "Part3AscensionPayBoss9"
                    // };

                    // string finalDialogue = dialogueOptions[Random.Range(0, dialogueOptions.Count)];

                    // yield return TextDisplayer.Instance.PlayDialogueEvent(finalDialogue, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    // yield return new WaitForSeconds(0.4f);

                    // yield return P03AnimationController.Instance.ShowChangeCurrency(BOSS_MONEY_REWARD, true);
                    // Part3SaveData.Data.currency += BOSS_MONEY_REWARD;

                    // yield return new WaitForSeconds(0.2f);
                    // P03AnimationController.Instance.SwitchToFace(currentFace);
                    // yield return new WaitForSeconds(0.1f);
                    Part3SaveData.WorldPosition worldPosition = new(HoloMapAreaManager.Instance.CurrentWorld.Id, HoloMapAreaManager.Instance.CurrentArea.GridX, HoloMapAreaManager.Instance.CurrentArea.GridY);
                    HoloMapAreaManager.Instance.MoveToAreaDirectly(worldPosition);

                    // if (ViewManager.Instance.CurrentView != currentView)
                    // {
                    //     ViewManager.Instance.SwitchToView(currentView, false, false);
                    //     yield return new WaitForSeconds(0.2f);
                    // }
                    //P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default, true, true);
                    //yield return new WaitForSeconds(0.1f);

                    //VictoryFeastNodeData.IsAscension.Equals(true);
                }

                HoloGameMap.Instance.Jump();
                HoloGameMap.Instance.ShowPoweredOn(poweredOn: true);

                //Wait a moment
                yield return new WaitForSeconds(0.45f);
                //Move outside boss room
                HoloMapAreaManager.Instance.MoveAreas(HoloMapAreaManager.Instance.CurrentArea, LookDirection.South, 1);
            }
            else
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03Yuck", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return FastTravelManagement.ReturnToLocation(EventManagement.MycologistReturnPosition);
            }

            SaveManager.SaveToFile(false);

            yield break;
        }

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.CleanupBossEffects))]
        [HarmonyPostfix]
        private static IEnumerator CleanupBossEffects(IEnumerator sequence)
        {
            //Debug.Log("Boss cleanup initiated!!!");
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield break;
            }

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.PAINTING_CHALLENGE))
            {
                //Debug.Log("About to call clear canvas rule!!!");
                //AscensionChallengeManagement.dummyCanvasBoss.CleanupBossEffects();
                //AscensionChallengeManagement.dummyCanvasBoss.DestroyScenery();
                GameObject CanvasBackground = GameObject.Find("LightQuadTableEffect(Clone)");
                //CanvasBackground.SetActive(false);
                Object.Destroy(CanvasBackground);

            }

            yield return sequence;
        }

        public static void RegisterBosses()
        {
            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.CanvasBoss)
                .SetNewSequencer(P03Plugin.PluginGuid, "AscensionCanvasSequencer", typeof(CanvasAscensionSequencer));

            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.ArchivistBoss).Opponent = typeof(ArchivistAscensionOpponent);

            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.TelegrapherBoss)
                .SetOpponent(typeof(TelegrapherAscensionOpponent))
                .SetNewSequencer(P03Plugin.PluginGuid, "AscensionTelgrapherSequencer", typeof(TelegrapherAscensionSequencer));

            OpponentManager.BaseGameOpponents.OpponentById(Opponent.Type.MycologistsBoss)
                .SetOpponent(typeof(MycologistAscensionBossOpponent))
                .SetNewSequencer(P03Plugin.PluginGuid, "AscensionMycologistsSequencer", typeof(MycologistAscensionSequence));

            P03FinalBossOpponent = OpponentManager.Add(P03Plugin.PluginGuid, "P03AscensionFinalBoss", string.Empty, typeof(P03AscensionOpponent))
                .SetNewSequencer(P03Plugin.PluginGuid, "P03FinalBossSequencer", typeof(P03FinalBossSequencer))
                .Id;
        }
    }
}