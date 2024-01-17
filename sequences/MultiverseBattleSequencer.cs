using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class MultiverseBattleSequencer : BossBattleSequencer
    {
        public override Opponent.Type BossType => BossManagement.P03MultiverseOpponent;
        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_P03_MULTIVERSE;

        private bool HasExplainedOpponentMultiverseLifeSharing = false;
        private bool HasExplainedPlayerMultiverseLifeSharing = false;

        public const int NUMBER_OF_MULTIVERSES = 3;

        public int CurrentMultiverseId { get; private set; } = 0;

        private readonly MultiverseGameState[] multiverseGames = new MultiverseGameState[NUMBER_OF_MULTIVERSES] { null, null, null };

        public bool MultiverseTravelLocked { get; private set; }

        public bool PlayerCanTravelMultiverse => (BoardManager.Instance as BoardManager3D).Bell.PressingAllowed() && !MultiverseTravelLocked;

        public bool AllOtherMultiversesReadyForCombat
        {
            get
            {
                for (int i = 0; i < multiverseGames.Length; i++)
                {
                    if (i != CurrentMultiverseId && !multiverseGames[i].PlayerRungBell)
                        return false;
                }
                return true;
            }
        }

        public void TraverseMultiverse(bool forward, bool rungBell)
        {
            if (MultiverseTravelLocked)
                return;

            MultiverseTravelLocked = true;

            multiverseGames[CurrentMultiverseId] = MultiverseGameState.GenerateFromCurrentState(rungBell);
            if (forward)
            {
                if (CurrentMultiverseId == NUMBER_OF_MULTIVERSES - 1)
                    CurrentMultiverseId = 0;
                else
                    CurrentMultiverseId++;
            }
            else
            {
                if (CurrentMultiverseId == 0)
                    CurrentMultiverseId = NUMBER_OF_MULTIVERSES - 1;
                else
                    CurrentMultiverseId--;
            }
            if (multiverseGames[CurrentMultiverseId] == null)
                InitializeMultiverse();
            else
                multiverseGames[CurrentMultiverseId].RestoreState(() => MultiverseTravelLocked = false);
        }

        private void InitializeMultiverse()
        {
            MultiverseTravelLocked = false;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            foreach (var multiverse in multiverseGames)
                if (multiverse != null)
                    multiverse.PlayerRungBell = false;

            yield break;
        }

        public override IEnumerator PreHandDraw()
        {
            int randomSeed = P03AscensionSaveData.RandomSeed;
            multiverseGames[1] = MultiverseGameState.GenerateAlternateStartingState(randomSeed++, GameColors.Instance.limeGreen);
            multiverseGames[2] = MultiverseGameState.GenerateAlternateStartingState(randomSeed++, GameColors.Instance.glowRed);
            yield break;
        }

        private IEnumerator BalanceAllMultiverses()
        {
            // This makes sure that all multiverses are alive
            for (int i = 0; i < multiverseGames.Length; i++)
            {
                int playerDamage = i == CurrentMultiverseId ? LifeManager.Instance.PlayerDamage : multiverseGames[i].PlayerDamage;
                int opponentDamage = i == CurrentMultiverseId ? LifeManager.Instance.OpponentDamage : multiverseGames[i].OpponentDamage;

                int additionalOpponentDamage = 0;
                int additionalPlayerDamage = 0;
                if ((playerDamage - opponentDamage) >= 5)
                {
                    additionalOpponentDamage = playerDamage - opponentDamage - 4;
                }
                if ((opponentDamage - playerDamage) >= 5)
                {
                    additionalPlayerDamage = opponentDamage - playerDamage - 4;
                }

                if (additionalOpponentDamage > 0)
                {
                    if (i == CurrentMultiverseId)
                        LifeManager.Instance.ShowDamageSequence(additionalOpponentDamage, additionalOpponentDamage, false);
                    else
                        multiverseGames[i].OpponentDamage += additionalOpponentDamage;
                }
                if (additionalPlayerDamage > 0)
                {
                    if (i == CurrentMultiverseId)
                        LifeManager.Instance.ShowDamageSequence(additionalPlayerDamage, additionalPlayerDamage, true);
                    else
                        multiverseGames[i].PlayerDamage += additionalPlayerDamage;
                }
            }
            yield break;
        }

        private bool MultiverseNeedsBalancing(bool forOpponent, bool checkForGameOver)
        {
            int unbalancedCount = 0;
            for (int i = 0; i < multiverseGames.Length; i++)
            {
                int balance = i == CurrentMultiverseId ? LifeManager.Instance.Balance : multiverseGames[i].LifeBalance;
                if ((forOpponent && balance >= 5) || (!forOpponent && balance <= -5))
                    unbalancedCount += 1;
            }
            if (checkForGameOver)
                return unbalancedCount == multiverseGames.Length;
            return unbalancedCount > 0;
        }

        private bool LifeLossConditionsMet()
        {
            return MultiverseNeedsBalancing(true, true) || MultiverseNeedsBalancing(false, true);
        }

        public IEnumerator LifeSharingSequence(bool balanceForOpponent)
        {
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.1f);

            // Switch to the multiverse that has < 0 life
            if (Mathf.Abs(LifeManager.Instance.Balance) < 5)
            {
                bool hasSwitched = false;
                for (int i = 0; i < multiverseGames.Length; i++)
                {
                    if (Mathf.Abs(multiverseGames[i].LifeBalance) >= 5)
                    {
                        multiverseGames[i].RestoreState(delegate ()
                        {
                            hasSwitched = true;
                            CurrentMultiverseId = i;
                        });
                        yield return new WaitUntil(() => hasSwitched);
                        break;
                    }
                }
                if (!hasSwitched)
                {
                    ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                    yield break;
                }
            }

            if (balanceForOpponent)
            {
                if (!HasExplainedOpponentMultiverseLifeSharing)
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03OpponentLifeSharing", TextDisplayer.MessageAdvanceMode.Input);
                    HasExplainedOpponentMultiverseLifeSharing = true;
                }
            }
            else
            {
                if (!HasExplainedPlayerMultiverseLifeSharing)
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03PlayerLifeSharing", TextDisplayer.MessageAdvanceMode.Input);
                    HasExplainedPlayerMultiverseLifeSharing = true;
                }
            }

            yield return BalanceAllMultiverses();

            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        [HarmonyPatch(typeof(CombatBell), nameof(CombatBell.OnCursorSelectStart))]
        [HarmonyPrefix]
        private static bool MultiverseBellRingHandler(CombatBell __instance)
        {
            if (TurnManager.Instance.SpecialSequencer is MultiverseBattleSequencer mbs && __instance.Pressable)
            {
                __instance.OnBellPressed();
                if (mbs.AllOtherMultiversesReadyForCombat)
                    CustomCoroutine.WaitOnConditionThenExecute(() => mbs.PlayerCanTravelMultiverse, () => TurnManager.Instance.OnCombatBellRang());
                else
                    CustomCoroutine.WaitOnConditionThenExecute(() => mbs.PlayerCanTravelMultiverse, () => mbs.TraverseMultiverse(true, true));

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.IsPlayerMainPhase), MethodType.Getter)]
        [HarmonyPostfix]
        private static void CheckIfMultiverseBellRungForPlayerMainPhase(TurnManager __instance, ref bool __result)
        {
            if (__result && __instance.SpecialSequencer is MultiverseBattleSequencer mbs && mbs.multiverseGames[mbs.CurrentMultiverseId].PlayerRungBell)
                __result = false;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoCombatPhase))]
        [HarmonyPostfix]
        private static IEnumerator DoMultiverseCombat(IEnumerator sequence, TurnManager __instance, bool playerIsAttacker)
        {
            if (__instance.SpecialSequencer is not MultiverseBattleSequencer mbs)
            {
                yield return sequence;
                yield break;
            }

            __instance.PlayerCanInitiateCombat = false;

            for (int i = 0; i < mbs.multiverseGames.Length; i++)
            {
                bool hasRestored = false;
                mbs.multiverseGames[i].RestoreState(delegate ()
                {
                    hasRestored = true;
                    mbs.CurrentMultiverseId = i;
                });
                yield return new WaitUntil(() => hasRestored);
                yield return __instance.CombatPhaseManager.DoCombatPhase(playerIsAttacker, __instance.SpecialSequencer);
                yield return new WaitForSeconds(0.5f);
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.GameSequence))]
        [HarmonyPostfix]
        private static IEnumerator DoMultiverseGameState(IEnumerator sequence, TurnManager __instance, EncounterData encounterData)
        {
            if (__instance.SpecialSequencer is not MultiverseBattleSequencer mbs)
            {
                yield return sequence;
                yield break;
            }

            __instance.ResetGameVars();
            yield return new WaitForEndOfFrame();
            yield return __instance.SetupPhase(encounterData);
            while (!__instance.GameIsOver())
            {
                while (!mbs.LifeLossConditionsMet())
                {
                    int num = __instance.TurnNumber;
                    __instance.TurnNumber = num + 1;
                    yield return __instance.PlayerTurn();
                    if (mbs.LifeLossConditionsMet())
                    {
                        break;
                    }
                    yield return mbs.BalanceAllMultiverses();
                    yield return __instance.OpponentTurn();
                    if (!mbs.LifeLossConditionsMet())
                        yield return mbs.BalanceAllMultiverses();
                }
                if (__instance.ScalesTippedToOpponent())
                {
                    Opponent opponent = __instance.opponent;
                    int num = opponent.NumLives;
                    opponent.NumLives = num - 1;
                    if (__instance.SpecialSequencer != null)
                    {
                        yield return __instance.SpecialSequencer.OpponentLifeLost();
                    }
                    yield return __instance.opponent.LifeLostSequence();
                    if (__instance.opponent.NumLives > 0)
                    {
                        yield return LifeManager.Instance.ShowResetSequence();
                    }
                    yield return __instance.opponent.PostResetScalesSequence();
                }
            }
            yield return __instance.CleanupPhase();
            __instance.GameEnded = true;
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.OpponentTurn))]
        [HarmonyPostfix]
        private static IEnumerator OpponentMultiverseTurn(IEnumerator sequence, TurnManager __instance)
        {
            if (__instance.SpecialSequencer is not MultiverseBattleSequencer mbs)
            {
                yield return sequence;
                yield break;
            }

            __instance.IsPlayerTurn = false;
            if (PlayerHand.Instance != null)
            {
                PlayerHand.Instance.PlayingLocked = true;
            }
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            if (__instance.Opponent.SkipNextTurn)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("OpponentSkipTurn", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
                __instance.Opponent.SkipNextTurn = false;
            }
            else
            {
                for (int i = 0; i < mbs.multiverseGames.Length; i++)
                {
                    if (i != mbs.CurrentMultiverseId)
                    {
                        bool hasSwitched = false;
                        mbs.multiverseGames[i].RestoreState(delegate ()
                        {
                            hasSwitched = true;
                            mbs.CurrentMultiverseId = i;
                        });
                        yield return new WaitUntil(() => hasSwitched);
                        yield return new WaitForSeconds(0.15f);
                    }

                    yield return __instance.DoUpkeepPhase(false);
                    yield return __instance.opponent.PlayCardsInQueue(0.1f);
                    yield return __instance.opponent.QueueNewCards(true, true);
                }
                yield return __instance.DoCombatPhase(false);
                if (mbs.LifeLossConditionsMet())
                {
                    yield return new WaitForSeconds(0.5f);
                    yield break;
                }
                yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.TurnEnd, true, false);
            }
        }
    }
}