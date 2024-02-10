using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Cards.Multiverse;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Triggers;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class MultiverseBattleSequencer : BossBattleSequencer
    {
        internal static MultiverseBattleSequencer Instance { get; private set; }


        public override Opponent.Type BossType => BossManagement.P03MultiverseOpponent;
        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_P03_MULTIVERSE;

        public bool ScalesTippedToOpponent { get; private set; } = false;
        public bool GameIsOver { get; private set; } = false;

        private MultiverseTelevisionScreen LeftScreen;
        private MultiverseTelevisionScreen RightScreen;

        private bool HasExplainedOpponentMultiverseLifeSharing = false;
        private bool HasExplainedPlayerMultiverseLifeSharing = false;

        public const int NUMBER_OF_MULTIVERSES = 3;

        public int CurrentMultiverseId { get; private set; } = 0;
        public MultiverseGameState ActiveMultiverse => MultiverseGames[CurrentMultiverseId];

        public int NumberOfPlayerWins { get; private set; } = 0;
        public int NumberOfPlayerLosses { get; private set; } = 0;

        private int NextUniverseUp => MultiverseGames.Length - 1 + NumberOfPlayerLosses + NumberOfPlayerWins;

        private static readonly List<string> LoseDialogues = new()
        {
            "P03LoseUniverseA", "P03LoseUniverseB", "P03LoseUniverseC", "P03LoseUniverseD"
        };

        public List<CardSlot> AllSlotsCopy => MultiverseGames.SelectMany(gs => gs.PlayerSlots).Concat(MultiverseGames.SelectMany(gs => gs.OpponentSlots)).ToList();

        internal readonly MultiverseGameState[] MultiverseGames = new MultiverseGameState[NUMBER_OF_MULTIVERSES] { null, null, null };

        public bool MultiverseTravelLocked { get; private set; }

        public bool PlayerCanTravelMultiverse => (BoardManager.Instance as BoardManager3D).Bell.PressingAllowed() && !MultiverseTravelLocked;

        public void TestTeleportationEffects()
        {
            TeleportationEffects(null);
        }

        public void TeleportationEffects(CardSlot slot)
        {
            CardSlot target = slot ?? BoardManager.Instance.OpponentSlotsCopy[0];
            AudioController.Instance.PlaySound3D("multiverse_teleport", MixerGroup.TableObjectsSFX, target.transform.position, 1f);
            var obj = GameObject.Instantiate(ResourceBank.Get<GameObject>("p03kcm/prefabs/cardslot_teleport"), target.transform);
            obj.transform.localPosition = Vector3.zero;

            if (slot != null)
                CustomCoroutine.WaitThenExecute(0.5f, () => GameObject.Destroy(obj));
        }

        public int GetUniverseId(CardSlot slot)
        {
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                if (MultiverseGames[i].PlayerSlots.Contains(slot) || MultiverseGames[i].OpponentSlots.Contains(slot))
                    return i;
            }
            return -1;
        }

        public bool OpposesInAnyUniverse(CardSlot a, CardSlot b)
        {
            return ((a.Index % 10) == (b.Index % 10)) && (a.IsPlayerSlot != b.IsPlayerSlot);
        }

        public bool InSameUniverse(CardSlot a, CardSlot b)
        {
            foreach (var universe in MultiverseGames)
            {
                if (universe.PlayerSlots.Contains(a) || universe.OpponentSlots.Contains(a))
                    return universe.PlayerSlots.Contains(b) || universe.OpponentSlots.Contains(b);
            }
            return false;
        }

        public bool InSameUniverse(PlayableCard a, PlayableCard b) => InSameUniverse(a.Slot, b.Slot);

        public int FindNextMainPhase()
        {
            for (int i = CurrentMultiverseId + 1; i < MultiverseGames.Length; i++)
                if (!MultiverseGames[i].PlayerRungBell)
                    return i;
            for (int i = 0; i < CurrentMultiverseId; i++)
                if (!MultiverseGames[i].PlayerRungBell)
                    return i;
            return -1;
        }

        public IEnumerator TravelToUniverse(int universeId)
        {
            MultiverseTravelLocked = true;
            MultiverseTelevisionScreen.CaptureScreenshotNextFrame = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            MultiverseGames[CurrentMultiverseId] = MultiverseGameState.GenerateFromCurrentState();
            MultiverseGames[CurrentMultiverseId].SetScreenshot(MultiverseTelevisionScreen.LastCapturedScreenshot);
            yield return MultiverseGames[universeId].RestoreState();
            CurrentMultiverseId = universeId;

            int prev = CurrentMultiverseId - 1;
            if (prev < 0)
                prev = MultiverseGames.Length - 1;
            int next = CurrentMultiverseId + 1;
            if (next >= MultiverseGames.Length)
                next = 0;

            LeftScreen.SetScreenContents(MultiverseGames[prev].Screenshot);
            RightScreen.SetScreenContents(MultiverseGames[next].Screenshot);

            MultiverseTravelLocked = false;
            yield break;
        }

        public IEnumerator TraverseMultiverse(bool forward)
        {
            if (MultiverseTravelLocked)
                yield break;

            MultiverseTravelLocked = true;

            MultiverseGames[CurrentMultiverseId] = MultiverseGameState.GenerateFromCurrentState();

            int nextMultiverseId = forward ? CurrentMultiverseId + 1 : CurrentMultiverseId - 1;

            if (forward)
            {
                if (CurrentMultiverseId == NUMBER_OF_MULTIVERSES - 1)
                    nextMultiverseId = 0;
            }
            else
            {
                if (CurrentMultiverseId == 0)
                    nextMultiverseId = NUMBER_OF_MULTIVERSES - 1;
            }

            yield return TravelToUniverse(nextMultiverseId);
        }

        internal IEnumerator IterateForAllMultiverses(System.Func<object[], IEnumerator> sequencer, params object[] parameters)
        {
            yield return IterateForAllMultiverses(c => true, sequencer, parameters);
        }

        internal IEnumerator IterateForAllMultiverses(System.Func<MultiverseGameState, bool> condition, System.Func<object[], IEnumerator> sequencer, params object[] parameters)
        {
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                if (MultiverseGames[i] == null || (MultiverseGames[i].GameIsActive && condition(MultiverseGames[i])))
                {
                    if (i != CurrentMultiverseId)
                    {
                        yield return TravelToUniverse(i);
                        yield return new WaitForSeconds(0.4f);
                    }

                    string logMessage = string.Join(", ", parameters);
                    P03Plugin.Log.LogInfo($"Invoking multiverse iteration {CurrentMultiverseId} for {sequencer} with {logMessage}");
                    yield return sequencer(parameters);
                }
            }

            // Go back to the first multiverse at the end
            yield return TravelToUniverse(0);
            yield return new WaitForSeconds(0.4f);
        }

        public override IEnumerator PreBoardSetup()
        {
            LeftScreen = MultiverseTelevisionScreen.Create(BoardManager.Instance.gameObject.transform);
            LeftScreen.transform.localEulerAngles = new(0f, 340f, 0f);
            LeftScreen.transform.localPosition = new(-2.2f, 0.9f, 3.7f);

            RightScreen = MultiverseTelevisionScreen.Create(BoardManager.Instance.gameObject.transform);
            RightScreen.transform.localEulerAngles = new(0f, 20f, 0f);
            RightScreen.transform.localPosition = new(2.2f, 0.9f, 3.7f);
            RightScreen.MoveForward = true;
            yield break;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (playerUpkeep)
            {
                // if (PlayerHand.Instance != null && TurnManager.Instance.TurnNumber == 1)
                //     yield return CardDrawPiles.Instance.DrawOpeningHand(TurnManager.Instance.GetFixedHand());

                bool showEnergyModule = (!ResourcesManager.Instance.EnergyAtMax || ResourcesManager.Instance.PlayerEnergy < ResourcesManager.Instance.PlayerMaxEnergy) && SaveManager.SaveFile.IsPart3;
                if (showEnergyModule)
                {
                    ViewManager.Instance.SwitchToView(View.Default, false, true);
                    yield return new WaitForSeconds(0.1f);
                }
                yield return ResourcesManager.Instance.AddMaxEnergy(1);
                yield return ResourcesManager.Instance.RefreshEnergy();
                if (showEnergyModule)
                {
                    yield return new WaitForSeconds(0.25f);
                    ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                }

                if (PlayerHand.Instance != null && TurnManager.Instance.TurnNumber > 1)
                    yield return CardDrawPiles.Instance.DrawPhaseSequence();
            }

            ActiveMultiverse.PlayerNeedsUpkeepStep = false;
            ActiveMultiverse.HasEverHadUpkeepStep = true;

            yield break;
        }

        private void CreateMultiverses()
        {
            int randomSeed = P03AscensionSaveData.RandomSeed;
            MultiverseGames[1] = MultiverseGameState.GenerateAlternateStartingState(randomSeed++, 1);
            MultiverseGames[2] = MultiverseGameState.GenerateAlternateStartingState(randomSeed++, 2);
        }

        private static Vector3 BehindScreenPosition = new(0f, 0f, 1.5f);
        private static Vector3 InFrontScreenPosition = new(0f, 0f, -1.5f);
        private static Vector3 ScreenAttackRotation = new(45f, 0f, 180f);

        public class OriginalTransformState
        {
            public OriginalTransformState(PlayableCard card)
            {
                originalCard = card;
                currentPosition = card.transform.localPosition;
                currentRotation = card.transform.localEulerAngles;
                currentParent = card.transform.parent;
            }

            private PlayableCard originalCard;
            private Vector3 currentPosition;
            private Vector3 currentRotation;
            private Transform currentParent;

            public void Restore()
            {
                originalCard.transform.SetParent(currentParent);
                originalCard.transform.localPosition = currentPosition;
                originalCard.transform.localEulerAngles = currentRotation;
            }
        }

        private static ConditionalWeakTable<PlayableCard, OriginalTransformState> originalStateTable = new();

        public void ParentCardToTvScreen(PlayableCard card, float duration, bool forceRestore = false)
        {
            int homeUniverse = GetUniverseId(card.slot);

            if (homeUniverse == CurrentMultiverseId || forceRestore)
            {
                if (originalStateTable.TryGetValue(card, out OriginalTransformState state))
                {
                    if (duration > 0f)
                    {
                        Tween.LocalPosition(card.transform, BehindScreenPosition, .2f, 0f, completeCallback: delegate ()
                        {
                            state.Restore();
                            originalStateTable.Remove(card);
                        });
                    }
                    else
                    {
                        state.Restore();
                        originalStateTable.Remove(card);
                    }
                }

                return;
            }

            if (!originalStateTable.TryGetValue(card, out OriginalTransformState _))
                originalStateTable.Add(card, new(card));

            MultiverseTelevisionScreen screen = LeftScreen;
            if (homeUniverse == CurrentMultiverseId + 1 || (homeUniverse == 0 && CurrentMultiverseId == MultiverseGames.Length - 1))
                screen = RightScreen;

            // Put the card behind the tv
            card.transform.SetParent(screen.transform, true);
            card.transform.localEulerAngles = ScreenAttackRotation;

            if (duration > 0f)
            {
                card.transform.localPosition = BehindScreenPosition;
                Tween.LocalPosition(card.transform, InFrontScreenPosition, .2f, 0f);
            }
            else
            {
                card.transform.localPosition = InFrontScreenPosition;
            }
        }

        public IEnumerator VisualizeMultiversalAttack(PlayableCard card, CardSlot target, Action callback = null, Func<bool> pauseCondition = null)
        {
            Vector3 currentPosition = card.transform.localPosition;
            Vector3 currentRotation = card.transform.localEulerAngles;
            Transform currentParent = card.transform.parent;

            ParentCardToTvScreen(card, .2f);
            yield return new WaitForSeconds(0.6f);

            bool impactKeyframeReached = false;
            card.Anim.PlayAttackAnimation(target.Card == null, target, delegate ()
            {
                impactKeyframeReached = true;
                callback?.Invoke();
            });

            yield return new WaitUntil(() => impactKeyframeReached);

            if (pauseCondition != null)
            {
                card.Anim.SetAnimationPaused(true);
                yield return new WaitUntil(() => pauseCondition());
                card.Anim.SetAnimationPaused(false);
            }

            ParentCardToTvScreen(card, .2f, forceRestore: true);
        }

        public IEnumerator ChooseSlotFromMultiverse(Predicate<CardSlot> filter, Action onMultiverseSwitch, Action<CardSlot> onHighlight, Action<CardSlot> onSelection)
        {
            BoardManager.Instance.ChoosingSlot = true;
            InteractionCursor.Instance.ForceCursorType(CursorType.Place);

            ViewManager.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.CardGameChoosingSlot, false);

            ViewManager.Instance.SwitchToView(BoardManager.Instance.boardView, false, false);
            BoardManager.Instance.cancelledPlacementWithInput = false;

            BoardManager.Instance.LastSelectedSlot = null;
            foreach (CardSlot cardSlot in BoardManager.Instance.OpponentSlotsCopy)
            {
                cardSlot.SetEnabled(false);
                cardSlot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);
            }
            BoardManager.Instance.SetQueueSlotsEnabled(false);

            foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
            {
                cardSlot.Chooseable = filter(cardSlot);
                if (cardSlot.Chooseable)
                {
                    if (onHighlight != null)
                        cardSlot.CursorEntered += (mii) => onHighlight(mii as CardSlot);

                    if (onSelection != null)
                        cardSlot.CursorSelectStarted += (mii) => onSelection(mii as CardSlot);
                }
            }

            while (BoardManager.Instance.LastSelectedSlot != null)
            {
                if (Instance.MultiverseTravelLocked)
                {
                    foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
                    {
                        cardSlot.Chooseable = false;
                        cardSlot.ClearDelegates();
                    }
                    yield return new WaitUntil(() => !Instance.MultiverseTravelLocked);
                    onMultiverseSwitch?.Invoke();
                    foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
                    {
                        cardSlot.Chooseable = filter(cardSlot);
                        if (cardSlot.Chooseable)
                        {
                            if (onHighlight != null)
                                cardSlot.CursorEntered += (mii) => onHighlight(mii as CardSlot);

                            if (onSelection != null)
                                cardSlot.CursorSelectStarted += (mii) => onSelection(mii as CardSlot);
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }

            foreach (CardSlot cardSlot in BoardManager.Instance.OpponentSlotsCopy)
            {
                cardSlot.SetEnabled(true);
                cardSlot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
            }
            BoardManager.Instance.SetQueueSlotsEnabled(true);

            foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
            {
                cardSlot.Chooseable = false;
                cardSlot.ClearDelegates();
            }

            ViewManager.Instance.Controller.SwitchToControlMode(BoardManager.Instance.defaultViewMode, false);
            BoardManager.Instance.ChoosingSlot = false;
            InteractionCursor.Instance.ClearForcedCursorType();
            yield break;
        }

        private IEnumerator VisualizeLosingUniverse(int universeId, bool opponentLost)
        {
            if (universeId != CurrentMultiverseId)
                yield return TravelToUniverse(universeId);

            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.4f);

            string dialogue = opponentLost ? LoseDialogues[NumberOfPlayerWins] : "P03WinUniverse";
            TextDisplayer.Instance.PlayDialogueEvent(dialogue, TextDisplayer.MessageAdvanceMode.Input);

            if (opponentLost)
                NumberOfPlayerWins += 1;
            else
                NumberOfPlayerLosses += 1;

            // Dolly zoom in
            Camera camera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();
            float fov = camera.fieldOfView;
            float z = camera.transform.position.z - P03AnimationController.Instance.transform.position.z;
            float width = z * 2f * Mathf.Tan(0.5f * fov * Mathf.Deg2Rad);
            float zTarget = 0.33f * z;

            Tween.Value(z, zTarget, delegate (float newZ)
            {
                camera.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(width / (2f * newZ)) * 2f;
                camera.transform.position = new(
                    camera.transform.position.x,
                    camera.transform.position.y,
                    P03AnimationController.Instance.transform.position.z + newZ
                );
            }, 1.5f, 0f);

            MultiverseGames[universeId] = MultiverseGameState.GenerateAlternateStartingState(P03AscensionSaveData.RandomSeed + NextUniverseUp * 10, NextUniverseUp);
            yield return TravelToUniverse(universeId);

            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.33f);

            yield return SetupMultiverseOpponent(TurnManager.Instance, null, NextUniverseUp);
            yield return new WaitForSeconds(0.33f);
        }

        private IEnumerator CheckForOpponentLoss()
        {
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                var universe = MultiverseGames[i];
                if ((universe.OpponentDamage - universe.PlayerDamage) > 5)
                {
                    if (NumberOfPlayerWins < 4)
                    {
                        yield return VisualizeLosingUniverse(i, opponentLost: true);
                    }
                    else
                    {
                        ScalesTippedToOpponent = true;
                        GameIsOver = true;
                    }
                }
            }
        }

        private IEnumerator CheckForPlayerLoss()
        {
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                var universe = MultiverseGames[i];
                if ((universe.PlayerDamage - universe.OpponentDamage) > 5)
                {
                    if (NumberOfPlayerLosses == 0)
                    {
                        yield return VisualizeLosingUniverse(i, opponentLost: false);
                    }
                    else
                    {
                        ScalesTippedToOpponent = false;
                        GameIsOver = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.IsPlayerMainPhase), MethodType.Getter)]
        [HarmonyPostfix]
        private static void CheckIfMultiverseBellRungForPlayerMainPhase(TurnManager __instance, ref bool __result)
        {
            if (__result && __instance.SpecialSequencer is MultiverseBattleSequencer mbs && mbs.MultiverseGames[mbs.CurrentMultiverseId].PlayerRungBell)
                __result = false;
        }

        private static IEnumerator DoCombatPhase(params object[] parameters)
        {
            yield return TurnManager.Instance.CombatPhaseManager.DoCombatPhase((bool)parameters[0], TurnManager.Instance.SpecialSequencer);
            yield return new WaitForSeconds(0.5f);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.GameSequence))]
        [HarmonyPostfix]
        private static IEnumerator DoMultiverseGameSequence(IEnumerator sequence, TurnManager __instance, EncounterData encounterData)
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
                while (!mbs.GameIsOver)
                {
                    __instance.TurnNumber += 1;
                    yield return __instance.PlayerTurn();

                    yield return mbs.CheckForPlayerLoss();

                    if (mbs.GameIsOver)
                        break;

                    yield return __instance.OpponentTurn();

                    yield return mbs.CheckForOpponentLoss();
                }
                // if (__instance.ScalesTippedToOpponent())
                // {
                //     Opponent opponent = __instance.opponent;
                //     int num = opponent.NumLives;
                //     opponent.NumLives = num - 1;
                //     if (__instance.SpecialSequencer != null)
                //     {
                //         yield return __instance.SpecialSequencer.OpponentLifeLost();
                //     }
                //     yield return __instance.opponent.LifeLostSequence();
                //     if (__instance.opponent.NumLives > 0)
                //     {
                //         yield return LifeManager.Instance.ShowResetSequence();
                //     }
                //     yield return __instance.opponent.PostResetScalesSequence();
                // }
            }
            yield return __instance.CleanupPhase();
            __instance.GameEnded = true;
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.ScalesTippedToOpponent))]
        [HarmonyPrefix]
        private static bool MultiverseTippedScales(TurnManager __instance, ref bool __result)
        {
            if (__instance.SpecialSequencer is not MultiverseBattleSequencer mbs)
                return true;

            __result = mbs.ScalesTippedToOpponent;
            return false;
        }

        private static IEnumerator OpponentUpkeepSteps(params object[] parameters)
        {
            yield return TurnManager.Instance.DoUpkeepPhase(false);
            yield return TurnManager.Instance.opponent.PlayCardsInQueue(0.1f);
            yield return TurnManager.Instance.opponent.QueueNewCards(true, true);
        }

        private static IEnumerator OpponentEndSteps(params object[] parameter)
        {
            yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.TurnEnd, true, false);
        }

        private static IEnumerator EntireOpponentTurn(params object[] parameters)
        {
            if (TurnManager.Instance.Opponent.SkipNextTurn)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("OpponentSkipTurn", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
                TurnManager.Instance.Opponent.SkipNextTurn = false;
                yield break;
            }
            yield return OpponentUpkeepSteps();
            yield return DoCombatPhase(false);
            if (Math.Abs(LifeManager.Instance.Balance) >= 5)
                yield break;
            yield return OpponentEndSteps();
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

            // We cannot run an opponent turn for any unverse that hasn't ever had an upkeep step
            // This prevents you from winning (or losing I guess?) on your turn, then having
            // the opponent take a turn before you
            yield return mbs.IterateForAllMultiverses((mbs) => mbs.HasEverHadUpkeepStep, EntireOpponentTurn);
        }

        private static IEnumerator SetupMultiverseOpponent(params object[] parameters)
        {
            TurnManager tm = parameters[0] as TurnManager;
            EncounterData data = parameters[1] as EncounterData;
            int multiverseBlueprintIndex = (tm.SpecialSequencer as MultiverseBattleSequencer).CurrentMultiverseId;
            if (parameters.Length > 2)
                multiverseBlueprintIndex = (int)parameters[2];

            yield return new WaitForSeconds(0.2f);

            if (PlayerHand.Instance.CardsInHand.Count == 0)
            {
                yield return CardDrawPiles.Instance.DrawOpeningHand(tm.GetFixedHand());
                yield return new WaitForSeconds(0.35f);
            }

            EncounterBlueprintData blueprint = MultiverseEncounters.MultiverseBossPhaseOne[multiverseBlueprintIndex];
            int difficulty = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty);
            var turnPlan = EncounterBuilder.BuildOpponentTurnPlan(blueprint, difficulty, false);
            tm.opponent.ReplaceAndAppendTurnPlan(turnPlan);

            if (data != null)
                yield return tm.PlacePreSetCards(data);

            yield return tm.opponent.QueueNewCards(true, true);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static IEnumerator MultiverseSetupPhase(IEnumerator sequence, EncounterData encounterData, TurnManager __instance)
        {
            if (__instance.SpecialSequencer is not MultiverseBattleSequencer mbs)
            {
                yield return sequence;
                yield break;
            }

            MultiverseBattleSequencer.Instance = mbs;

            __instance.IsSetupPhase = true;
            PlayerHand.Instance.PlayingLocked = true;
            if (mbs != null)
            {
                yield return mbs.PreBoardSetup();
            }
            yield return new WaitForSeconds(0.15f);
            yield return LifeManager.Instance.Initialize(mbs == null || mbs.ShowScalesOnStart);
            if (ProgressionData.LearnedMechanic(MechanicsConcept.Rulebook) && TableRuleBook.Instance != null)
            {
                TableRuleBook.Instance.SetOnBoard(true);
            }
            __instance.StartCoroutine(BoardManager.Instance.Initialize());
            __instance.StartCoroutine(ResourcesManager.Instance.Setup());

            yield return __instance.opponent.IntroSequence(encounterData);


            if (BoonsHandler.Instance != null)
            {
                yield return BoonsHandler.Instance.ActivatePreCombatBoons();
            }
            yield return mbs.PreDeckSetup();
            PlayerHand.Instance.Initialize();
            yield return CardDrawPiles.Instance.Initialize();
            yield return mbs.PreHandDraw();
            yield return CardDrawPiles.Instance.DrawOpeningHand(__instance.GetFixedHand());

            // Now we can create the multiverses and start using them
            mbs.CreateMultiverses();

            yield return mbs.IterateForAllMultiverses(SetupMultiverseOpponent, __instance, encounterData);

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.StartingDamage))
            {
                ChallengeActivationUI.TryShowActivation(AscensionChallenge.StartingDamage);
                yield return LifeManager.Instance.ShowDamageSequence(1, 1, true, 0.125f, null, 0f, false);
            }
            __instance.IsSetupPhase = false;
            yield break;
        }

        private static IEnumerator PlayerEndStep(params object[] parameters)
        {
            TurnManager.Instance.PlayerPhase = TurnManager.PlayerTurnPhase.End;
            yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.TurnEnd, true, true);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
        [HarmonyPostfix]
        private static IEnumerator MultiversePlayerTurn(IEnumerator sequence, TurnManager __instance)
        {
            if (__instance.SpecialSequencer is not MultiverseBattleSequencer mbs)
            {
                yield return sequence;
                yield break;
            }

            if (PlayerHand.Instance != null)
            {
                PlayerHand.Instance.PlayingLocked = true;
            }
            __instance.IsPlayerTurn = true;
            __instance.PlayerPhase = TurnManager.PlayerTurnPhase.Draw;
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            foreach (var multiverse in mbs.MultiverseGames)
            {
                if (multiverse != null)
                {
                    multiverse.PlayerRungBell = false;
                    multiverse.PlayerNeedsUpkeepStep = true;
                }
            }

            yield return __instance.DoUpkeepPhase(true);

            // I've moved the "add one max energy" and the "draw your card for turn" steps
            // out of the turn manager and into the sequencer to help deal with the various
            // multiverses

            __instance.PlayerPhase = TurnManager.PlayerTurnPhase.Main;
            yield return new WaitForSeconds(0.25f);

            yield return mbs.PlayerPostDraw();

            __instance.PlayerCanInitiateCombat = true;
            if (PlayerHand.Instance != null)
            {
                PlayerHand.Instance.PlayingLocked = false;
            }
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            bool readyForCombat = false;
            while (!readyForCombat)
            {
                while (!__instance.playerInitiatedCombat)
                {
                    if (!mbs.PlayerCanTravelMultiverse)
                        yield return new WaitUntil(() => mbs.PlayerCanTravelMultiverse);

                    if (__instance.LifeLossConditionsMet() && GlobalTriggerHandler.Instance.StackSize == 0)
                    {
                        yield break;
                    }
                    if (mbs.ActiveMultiverse.PlayerNeedsUpkeepStep)
                    {
                        yield return __instance.DoUpkeepPhase(true);
                        __instance.PlayerPhase = TurnManager.PlayerTurnPhase.Main;
                        yield return new WaitForSeconds(0.25f);
                        yield return mbs.PlayerPostDraw();
                    }
                    yield return new WaitForEndOfFrame();
                }
                mbs.ActiveMultiverse.PlayerRungBell = true;
                int nextIdx = mbs.FindNextMainPhase();
                if (nextIdx == -1)
                {
                    readyForCombat = true;
                }
                else
                {
                    yield return mbs.TravelToUniverse(nextIdx);
                    __instance.playerInitiatedCombat = false;
                }
            }
            __instance.playerInitiatedCombat = false;
            __instance.PlayerPhase = TurnManager.PlayerTurnPhase.Combat;

            yield return mbs.IterateForAllMultiverses(DoCombatPhase, true);

            if (__instance.LifeLossConditionsMet())
            {
                yield break;
            }

            yield return mbs.IterateForAllMultiverses(
                state => state.RespondsToTrigger(Trigger.TurnEnd, true),
                PlayerEndStep
            );
            yield break;
        }

        [HarmonyPatch(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator MultiverseTriggerHandler(IEnumerator sequence, Trigger trigger, bool triggerFacedown, object[] otherArgs)
        {
            if (MultiverseBattleSequencer.Instance != null && MultiverseBattleSequencer.Instance.ActiveMultiverse != null)
                yield return MultiverseBattleSequencer.Instance.ActiveMultiverse.DoCallbacks(trigger);

            yield return sequence;

            if (MultiverseBattleSequencer.Instance == null)
                yield break;

            List<TriggerReceiver> receivers = new();
            foreach (var universe in MultiverseBattleSequencer.Instance.MultiverseGames)
            {
                if (universe == MultiverseBattleSequencer.Instance.ActiveMultiverse)
                    continue;

                receivers.AddRange(
                    universe.PlayerSlots.Concat(universe.OpponentSlots)
                            .Where(s => s.Card != null)
                            .Select(s => s.Card)
                            .Where(s => triggerFacedown || !s.FaceDown)
                            .SelectMany(c => c.TriggerHandler.GetAllReceivers())
                            .Where(r => r is IMultiverseAbility)
                );
            }
            foreach (var receiver in receivers)
            {
                if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                    yield return GlobalTriggerHandler.Instance.TriggerSequence(trigger, receiver, otherArgs);
            }
            yield break;
        }

        protected class RefBoolean
        {
            public bool Value;
        }

        private static ConditionalWeakTable<CardSlot, RefBoolean> IsPlayerSlot = new();

        [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.IsPlayerSlot), MethodType.Getter)]
        [HarmonyPostfix]
        private static void MultiverseIsPlayerSlot(CardSlot __instance, ref bool __result)
        {
            if (Instance == null || Instance.MultiverseGames == null)
                return;

            if (IsPlayerSlot.TryGetValue(__instance, out RefBoolean value))
            {
                __result = value.Value;
                return;
            }

            if (BoardManager.Instance.playerSlots.Contains(__instance))
            {
                __result = true;
            }
            else if (BoardManager.Instance.opponentSlots.Contains(__instance))
            {
                __result = false;
            }
            else
            {
                __result = false;
                foreach (var universe in Instance.MultiverseGames.Where(u => u != null && u.PlayerSlots != null))
                {
                    if (universe.PlayerSlots.Contains(__instance))
                    {
                        __result = true;
                        break;
                    }
                }
            }

            IsPlayerSlot.Add(__instance, new() { Value = __result });
            return;
        }

        private class RefInt
        {
            public int Value;
        }
        private static ConditionalWeakTable<CardSlot, RefInt> SlotIndex = new();

        [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.Index), MethodType.Getter)]
        [HarmonyPostfix]
        private static void MultiverseIndexLookup(CardSlot __instance, ref int __result)
        {
            if (Instance == null || __result >= 0 || Instance.MultiverseGames == null)
                return;

            if (SlotIndex.TryGetValue(__instance, out RefInt value))
            {
                __result = value.Value;
                return;
            }

            __result = -1;
            for (int i = 0; i < Instance.MultiverseGames.Length; i++)
            {
                if (__instance.IsPlayerSlot)
                    __result = (Instance.MultiverseGames[i]?.PlayerSlots?.IndexOf(__instance)).GetValueOrDefault(-1);
                else
                    __result = (Instance.MultiverseGames[i]?.OpponentSlots?.IndexOf(__instance)).GetValueOrDefault(-1);

                if (__result >= 0)
                {
                    __result += (i + 1) * 10;
                    break;
                }
            }
            if (__result == -1)
            {
                if (BoardManager.Instance.playerSlots.Contains(__instance))
                {
                    __result = BoardManager.Instance.playerSlots.IndexOf(__instance) + 10;
                }
                else if (BoardManager.Instance.opponentSlots.Contains(__instance))
                {
                    __result = BoardManager.Instance.opponentSlots.IndexOf(__instance) + 10;
                }
            }
            SlotIndex.Add(__instance, new() { Value = __result });
            return;
        }

        private static ConditionalWeakTable<PlayableCard, string> DefaultColors = new();
        private static Color GetCardColor(PlayableCard card)
        {
            if (DefaultColors.TryGetValue(card, out string colorKey))
                return DiscCardColorAppearance.GetColorFromString(colorKey).GetValueOrDefault(GameColors.Instance.blue);
            return GameColors.Instance.blue;
        }
        private static void SetCardColor(PlayableCard card, string colorString)
        {
            if (DefaultColors.TryGetValue(card, out string colorKey))
                DefaultColors.Remove(card);
            DefaultColors.Add(card, colorString);
        }

        [HarmonyPatch(typeof(CardSpawner), nameof(CardSpawner.SpawnPlayableCard))]
        [HarmonyPrefix]
        private static bool MultiverseCardSpawner(CardInfo info, ref PlayableCard __result)
        {
            if (MultiverseBattleSequencer.Instance == null)
                return true;

            GameObject gameObject = GameObject.Instantiate<GameObject>(CardSpawner.Instance.PlayableCardPrefab);
            PlayableCard component = gameObject.GetComponent<PlayableCard>();

            Color cardColor = (MultiverseBattleSequencer.Instance.ActiveMultiverse?.ColorState?.P03FaceColor).GetValueOrDefault(GameColors.Instance.blue);

            SetCardColor(component, ColorUtility.ToHtmlStringRGB(cardColor));
            if (component.StatsLayer is DiskRenderStatsLayer drsl)
                drsl.SetLightColor(cardColor);
            component.SetInfo(info);
            __result = component;
            return false;
        }
    }
}