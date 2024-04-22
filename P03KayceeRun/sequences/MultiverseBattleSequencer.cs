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
using Infiniscryption.P03KayceeRun.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Triggers;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Faces;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class MultiverseBattleSequencer : BossBattleSequencer
    {
        internal static MultiverseBattleSequencer Instance { get; private set; }

        public override Opponent.Type BossType => BossManagement.P03MultiverseOpponent;
        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_P03_MULTIVERSE;

        internal bool HasSeenMagnificusBrush { get; set; } = false;
        internal bool HasSeenGrimoraQuill { get; set; } = false;

        public bool ScalesTippedToOpponent { get; private set; } = false;
        public bool GameIsOver { get; private set; } = false;

        public MultiverseGameState.Phase CurrentPhase { get; private set; } = MultiverseGameState.Phase.GameIsOver;

        private MultiverseTelevisionScreen LeftScreen;
        private MultiverseTelevisionScreen RightScreen;

        internal readonly List<string> ItemStartingState = new();

        public const int NUMBER_OF_MULTIVERSES = 3;

        public int CurrentMultiverseId { get; private set; } = 0;
        public MultiverseGameState ActiveMultiverse => MultiverseGames[CurrentMultiverseId];

        public int NumberOfPlayerWins { get; private set; } = 0;
        public int NumberOfPlayerLosses { get; private set; } = 0;

        internal GameObject CyberspaceParticles => TurnManager.Instance.transform.Find("Cyberspace_Particles")?.gameObject;

        private int NextUniverseUp => MultiverseGames.Length - 1 + NumberOfPlayerLosses + NumberOfPlayerWins;

        private static readonly List<string> LoseDialogues = new()
        {
            "P03LoseUniverseA", "P03LoseUniverseB", "P03LoseUniverseC", "P03LoseUniverseD"
        };

        public List<CardSlot> AllSlotsCopy => MultiverseGames.SelectMany(gs => gs.PlayerSlots).Concat(MultiverseGames.SelectMany(gs => gs.OpponentSlots)).ToList();

        internal readonly MultiverseGameState[] MultiverseGames = new MultiverseGameState[NUMBER_OF_MULTIVERSES] { null, null, null };

        public bool MultiverseTravelLocked { get; private set; }
        private bool MultiverseTravelExplicitlyAllowed = false;
        public bool PlayerCanTravelMultiverse => !MultiverseTravelLocked && (MultiverseTravelExplicitlyAllowed || (CurrentPhase == MultiverseGameState.Phase.PlayerUpkeepAndMain && GlobalTriggerHandler.Instance.StackSize == 0));

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

        private static readonly ConditionalWeakTable<CardSlot, RefInt> UniverseIdLookup = new();
        public int GetUniverseId(CardSlot slot)
        {
            if (UniverseIdLookup.TryGetValue(slot, out RefInt value))
                return value.Value;

            int result = -1;
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                if (MultiverseGames[i].PlayerSlots.Contains(slot) || MultiverseGames[i].OpponentSlots.Contains(slot))
                {
                    result = i;
                    break;
                }
            }
            UniverseIdLookup.Add(slot, new() { Value = result });
            return result;
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

        public IEnumerator TravelToUniverse(int universeId, bool force = false)
        {
            if (universeId == CurrentMultiverseId && !force)
                yield break;

            MultiverseTravelLocked = true;
            MultiverseTelevisionScreen.CaptureScreenshotNextFrame = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (universeId != CurrentMultiverseId)
            {
                MultiverseGames[CurrentMultiverseId] = MultiverseGameState.GenerateFromCurrentState();
                MultiverseGames[CurrentMultiverseId].SetScreenshot(MultiverseTelevisionScreen.LastCapturedScreenshot);
            }

            CurrentMultiverseId = universeId;
            yield return MultiverseGames[universeId].RestoreState();

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

        private List<CardInfo> _deadMultiverseCards = new();
        public List<CardInfo> DeadMultiverseCards => new(_deadMultiverseCards);

        public void RemoveDeadMultiverseCard(CardInfo card)
        {
            if (_deadMultiverseCards.Contains(card))
                _deadMultiverseCards.Remove(card);
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => true;
        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            if (card.IsMultiverseCard())
            {
                CardInfo newInfo = (CardInfo)card.Info.Clone();
                foreach (var mod in card.temporaryMods)
                    newInfo.mods.Add((CardModificationInfo)mod.Clone());
                _deadMultiverseCards.Add(newInfo);
            }
            yield break;
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

            if (BoardManager.Instance.PlayerSlotsCopy.Count == 5)
            {
                P03AscensionOpponent.CreateAllSlots();
                // Tween each of the four things that need to move
            }



            yield break;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return GameEnd(true);

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

                if (PlayerHand.Instance != null && TurnManager.Instance.TurnNumber % 2 == 0)
                {
                    if (!PlayerHand.Instance.CardsInHand.Any(c => c.Info.name.Equals(CustomCards.MAG_BRUSH)))
                    {
                        yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(CustomCards.MAG_BRUSH), new List<CardModificationInfo>(), CardDrawPiles.Instance.drawFromPilesSpawnOffset, 0.15f);
                    }
                }

                if (PlayerHand.Instance != null && TurnManager.Instance.TurnNumber % 2 == 1 && TurnManager.Instance.TurnNumber >= 3)
                {
                    if (!PlayerHand.Instance.CardsInHand.Any(c => c.Info.name.Equals(CustomCards.GRIM_QUIL)))
                    {
                        if ((_deadMultiverseCards.Count - MultiverseGames.SelectMany(g => g.HandState).Where(c => c.Info.name.Equals(CustomCards.GRIM_QUIL)).Count()) >= 1)
                            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(CustomCards.GRIM_QUIL), new List<CardModificationInfo>(), CardDrawPiles.Instance.drawFromPilesSpawnOffset, 0.15f);
                    }
                }
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
        private static Vector3 OpponentScreenAttackRotation = new(45f, 0f, 180f);
        private static Vector3 PlayerScreenAttackRotation = new(45f, 0f, 0f);

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
            card.transform.localEulerAngles = card.OpponentCard ? OpponentScreenAttackRotation : PlayerScreenAttackRotation;

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

        public IEnumerator SetPhase(MultiverseGameState.Phase phase, bool suppressCallbacks = false)
        {
            CurrentPhase = phase;

            if (phase == MultiverseGameState.Phase.PlayerCombat && P03AnimationController.Instance.CurrentFace == P03BellFace.ID)
                P03AnimationController.Instance.FaceRenderer.DisplayFace(P03AnimationController.Face.Default);

            if (!suppressCallbacks && ActiveMultiverse != null)
                yield return ActiveMultiverse.DoCallbacks(phase);
        }

        public IEnumerator ChooseSlotFromMultiverse(Predicate<CardSlot> filter, Action onMultiverseSwitch, Action<CardSlot> onHighlight, Action<CardSlot> onSelection)
        {
            P03Plugin.Log.LogInfo("Preparing to choose multiverse slot");
            BoardManager.Instance.ChoosingSlot = true;
            bool interactionDisabled = InteractionCursor.Instance.InteractionDisabled;
            InteractionCursor.Instance.InteractionDisabled = false;
            InteractionCursor.Instance.ForceCursorType(CursorType.Place);

            ViewLockState currentLockState = ViewManager.Instance.Controller.LockState;
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            ViewManager.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.CardGameChoosingSlot, true);

            BoardManager.Instance.SetQueueSlotsEnabled(false);

            Action<CardSlot> selectionAction = (slot) => BoardManager.Instance.LastSelectedSlot = slot;
            if (onSelection != null)
                selectionAction += onSelection;

            foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
            {
                cardSlot.Chooseable = filter(cardSlot);
                if (cardSlot.Chooseable)
                {
                    if (onHighlight != null)
                        cardSlot.CursorEntered += (mii) => onHighlight(mii as CardSlot);

                    cardSlot.CursorSelectStarted += (mii) => selectionAction(mii as CardSlot);
                }
            }

            P03Plugin.Log.LogInfo("Starting selection loop");
            BoardManager.Instance.LastSelectedSlot = null;

            MultiverseTravelExplicitlyAllowed = true;
            while (BoardManager.Instance.LastSelectedSlot == null)
            {
                if (Instance.MultiverseTravelLocked)
                {
                    P03Plugin.Log.LogInfo("Multiverse travel is happening. Waiting...");
                    foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
                    {
                        cardSlot.Chooseable = false;
                        cardSlot.ClearDelegates();
                    }
                    yield return new WaitUntil(() => !Instance.MultiverseTravelLocked);
                    P03Plugin.Log.LogInfo("Multiverse travel complete");
                    onMultiverseSwitch?.Invoke();
                    foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
                    {
                        cardSlot.Chooseable = filter(cardSlot);
                        if (cardSlot.Chooseable)
                        {
                            if (onHighlight != null)
                                cardSlot.CursorEntered += (mii) => onHighlight(mii as CardSlot);

                            cardSlot.CursorSelectStarted += (mii) => selectionAction(mii as CardSlot);
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            MultiverseTravelExplicitlyAllowed = false;

            BoardManager.Instance.SetQueueSlotsEnabled(true);

            foreach (var cardSlot in BoardManager.Instance.AllSlotsCopy)
            {
                cardSlot.Chooseable = false;
                cardSlot.ClearDelegates();
            }

            ViewManager.Instance.Controller.SwitchToControlMode(BoardManager.Instance.defaultViewMode, false);
            BoardManager.Instance.ChoosingSlot = false;
            InteractionCursor.Instance.ClearForcedCursorType();
            InteractionCursor.Instance.InteractionDisabled = interactionDisabled;
            ViewManager.Instance.Controller.LockState = currentLockState;
            yield break;
        }

        private IEnumerator VisualizeLosingUniverse(int universeId, bool opponentLost)
        {
            if (universeId != CurrentMultiverseId)
                yield return TravelToUniverse(universeId);

            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.4f);

            string dialogue = opponentLost ? LoseDialogues[NumberOfPlayerWins] : "P03WinUniverse";
            yield return TextDisplayer.Instance.PlayDialogueEvent(dialogue, TextDisplayer.MessageAdvanceMode.Input);

            if (opponentLost)
                NumberOfPlayerWins += 1;
            else
                NumberOfPlayerLosses += 1;

            // Dolly zoom in
            Camera camera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();
            Transform cameraParent = camera.transform.parent;
            Vector3 originalPosition = camera.transform.position;
            float x = cameraParent.localEulerAngles.x;
            float fov = camera.fieldOfView;
            float z = camera.transform.position.z - P03AnimationController.Instance.transform.position.z;
            float width = z * 2f * Mathf.Tan(0.5f * fov * Mathf.Deg2Rad);
            float zTarget = 0.2f * z;

            Tween.Value(z, zTarget, delegate (float newZ)
            {
                camera.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(width / (2f * newZ)) * 2f;
                camera.transform.position = new(
                    camera.transform.position.x,
                    camera.transform.position.y,
                    P03AnimationController.Instance.transform.position.z + newZ
                );

                cameraParent.localEulerAngles = new(x * (newZ - zTarget) / (z - zTarget), 0f, 0f);
            }, 1.0f, 0f);

            yield return new WaitForSeconds(1.0f);

            foreach (CardSlot slot in BoardManager.Instance.AllSlots)
            {
                if (slot.GetSlotModification() != SlotModificationManager.ModificationType.NoModification)
                    yield return slot.SetSlotModification(SlotModificationManager.ModificationType.NoModification);
            }

            // Need to remove everything from the hand and board
            var newUniverse = MultiverseGameState.GenerateAlternateStartingState(P03AscensionSaveData.RandomSeed + NextUniverseUp * 10, NextUniverseUp);
            var oldUniverse = ActiveMultiverse;
            P03Plugin.Log.LogInfo("Created new universe; waiting a frame to debug");
            yield return new WaitForEndOfFrame();
            yield return oldUniverse.CleanUp();
            MultiverseGames[universeId] = newUniverse;
            yield return TravelToUniverse(universeId, force: true);

            foreach (CardSlot slot in BoardManager.Instance.AllSlots)
            {
                if (slot.GetSlotModification() != SlotModificationManager.ModificationType.NoModification)
                    yield return slot.SetSlotModification(SlotModificationManager.ModificationType.NoModification);
            }

            yield return oldUniverse.CleanUp();

            Tween.Value(zTarget, z, delegate (float newZ)
            {
                camera.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(width / (2f * newZ)) * 2f;
                camera.transform.position = new(
                    camera.transform.position.x,
                    camera.transform.position.y,
                    P03AnimationController.Instance.transform.position.z + newZ
                );

                cameraParent.localEulerAngles = new(x * (newZ - zTarget) / (z - zTarget), 0f, 0f);
            }, 0.33f, 0f);

            Tween.Position(camera.transform, originalPosition, 1f, 0f);

            yield return SetupMultiverseOpponent(TurnManager.Instance, null, NextUniverseUp);
            yield return new WaitForSeconds(0.33f);
        }

        private IEnumerator CheckForOpponentLoss()
        {
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                if (GameIsOver)
                    yield break;

                var universe = MultiverseGames[i];
                P03Plugin.Log.LogInfo($"Universe {i}: scales tipped {universe.OpponentDamage - universe.PlayerDamage} towards opponent");
                if ((universe.OpponentDamage - universe.PlayerDamage) >= 5)
                {
                    if (NumberOfPlayerWins < 3)
                    {
                        P03Plugin.Log.LogInfo("Opponent lost a universe");
                        yield return VisualizeLosingUniverse(i, opponentLost: true);
                    }
                    else
                    {
                        ScalesTippedToOpponent = true;
                        TurnManager.Instance.Opponent.Surrendered = true;
                        GameIsOver = true;
                    }
                }
            }
        }

        private IEnumerator CheckForPlayerLoss()
        {
            for (int i = 0; i < MultiverseGames.Length; i++)
            {
                if (GameIsOver)
                    yield break;

                var universe = MultiverseGames[i];
                if ((universe.PlayerDamage - universe.OpponentDamage) >= 5)
                {
                    if (NumberOfPlayerLosses == 0)
                    {
                        yield return VisualizeLosingUniverse(i, opponentLost: false);
                    }
                    else
                    {
                        ScalesTippedToOpponent = false;
                        TurnManager.Instance.PlayerSurrendered = true;
                        GameIsOver = true;
                    }
                }
            }
        }

        private IEnumerator CheckForGameLosses()
        {
            P03Plugin.Log.LogInfo("Checking to see if opponent has lost in any universe");
            yield return CheckForOpponentLoss();
            P03Plugin.Log.LogInfo("Checking to see if player has lost in any universe");
            yield return CheckForPlayerLoss();
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

            yield return mbs.SetPhase(MultiverseGameState.Phase.GameIsOver);
            __instance.ResetGameVars();
            mbs.ItemStartingState.AddRange(ItemsManager.Instance.Consumables.Select(i => i.Data.name).Where(s => !s.Equals("hammer", StringComparison.InvariantCultureIgnoreCase)));
            yield return new WaitForEndOfFrame();
            yield return __instance.SetupPhase(encounterData);
            while (!__instance.GameIsOver())
            {
                while (!mbs.GameIsOver)
                {
                    __instance.TurnNumber += 1;
                    yield return __instance.PlayerTurn();
                    yield return mbs.SetPhase(MultiverseGameState.Phase.AfterPlayer);
                    yield return mbs.CheckForGameLosses();

                    if (mbs.GameIsOver)
                        break;

                    yield return __instance.OpponentTurn();
                    yield return mbs.SetPhase(MultiverseGameState.Phase.AfterOpponent);
                    yield return mbs.CheckForGameLosses();
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

            yield return Instance.SetPhase(MultiverseGameState.Phase.OpponentUpkeep);
            yield return OpponentUpkeepSteps();

            yield return Instance.SetPhase(MultiverseGameState.Phase.OpponentCombat);
            yield return DoCombatPhase(false);
            if (Math.Abs(LifeManager.Instance.Balance) >= 5)
                yield break;

            yield return Instance.SetPhase(MultiverseGameState.Phase.OpponentEnd);
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

        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            EncounterData data = base.BuildCustomEncounter(nodeData);
            data.aiId = BossManagement.P03FinalBossAI;
            return data;
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

            // I have to manually handle terrain
            Tuple<List<CardInfo>, List<CardInfo>, List<CardInfo>> terrain = EncounterExtensions.GetTerrainForBlueprint(blueprint, difficulty);
            if (terrain != null)
            {

                EncounterData.StartCondition startCondition = new()
                {
                    cardsInPlayerSlots = terrain.Item1.ToArray(),
                    cardsInOpponentSlots = terrain.Item2.ToArray(),
                    cardsInOpponentQueue = terrain.Item3.ToArray()
                };

                data ??= new();

                data.startConditions = new() { startCondition };
            }

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

            yield return Instance.SetPhase(MultiverseGameState.Phase.PlayerUpkeepAndMain);
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
                    if (mbs.MultiverseTravelLocked)
                        yield return new WaitUntil(() => !mbs.MultiverseTravelLocked);

                    if (__instance.LifeLossConditionsMet() && GlobalTriggerHandler.Instance.StackSize == 0)
                    {
                        yield break;
                    }
                    if (mbs.ActiveMultiverse.PlayerNeedsUpkeepStep)
                    {
                        mbs.MultiverseTravelLocked = true;
                        yield return Instance.SetPhase(MultiverseGameState.Phase.PlayerUpkeepAndMain);
                        yield return __instance.DoUpkeepPhase(true);
                        __instance.PlayerPhase = TurnManager.PlayerTurnPhase.Main;
                        yield return new WaitForSeconds(0.25f);
                        yield return mbs.PlayerPostDraw();
                        mbs.MultiverseTravelLocked = false;
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

            yield return Instance.SetPhase(MultiverseGameState.Phase.PlayerCombat);
            __instance.playerInitiatedCombat = false;
            __instance.PlayerPhase = TurnManager.PlayerTurnPhase.Combat;

            yield return mbs.IterateForAllMultiverses(DoCombatPhase, true);

            if (__instance.LifeLossConditionsMet())
            {
                yield break;
            }

            yield return Instance.SetPhase(MultiverseGameState.Phase.PlayerEnd);
            yield return mbs.IterateForAllMultiverses(
                state => state.RespondsToTrigger(Trigger.TurnEnd, true) || state.HasPhaseCallback(MultiverseGameState.Phase.PlayerEnd),
                PlayerEndStep
            );
            yield break;
        }

        [HarmonyPatch(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator MultiverseTriggerHandler(IEnumerator sequence, Trigger trigger, bool triggerFacedown, object[] otherArgs)
        {
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

        private void AnimeFlyAway(Transform transform, Vector3 offset, float duration = 0.3f)
        {
            Tween.Position(transform, transform.position + offset, duration, 0f, completeCallback: () => transform.gameObject.SetActive(false));
        }


        public override IEnumerator PreCleanUp()
        {
            OpponentAnimationController.Instance.ClearLookTarget();
            InteractionCursor.Instance.InteractionDisabled = true;
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            ViewManager.Instance.SwitchToView(View.DefaultUpwards, false, false);

            UIManager.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, .4f);
            CameraEffects.Instance.Shake(0.1f, .4f);
            AudioController.Instance.PlaySound2D("cam_switch", MixerGroup.None, .6f, 0f, pitch: new AudioParams.Pitch(AudioParams.Pitch.Variation.Medium));
            MultiverseGameState.LightColorState.GetPreset(GameColors.Instance.blue).RestoreState();

            yield return new WaitForSeconds(1f);

            if (TurnManager.Instance.PlayerWon)
            {
                PauseMenu.pausingDisabled = true;
                yield return TextDisplayer.Instance.PlayDialogueEvent(LoseDialogues[3], TextDisplayer.MessageAdvanceMode.Input);
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return new WaitForSeconds(0.25f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03GetsIt", TextDisplayer.MessageAdvanceMode.Input);
                ViewManager.Instance.SwitchToView(View.DefaultUpwards, false, false);
                yield return new WaitForSeconds(0.25f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03HuntsForGrimora", TextDisplayer.MessageAdvanceMode.Input);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);

                yield return new WaitForSeconds(1.5f);


                // Duplicate the arm
                P03AscensionSaveData.SetLeshyDead(false, true);
                GameObject leftArm = P03AnimationController.Instance.transform.Find("Body/RotatingHead/Head/HeadAnim/LeftLeshyArm").gameObject;
                GameObject newLeftArm = GameObject.Instantiate(leftArm, leftArm.transform.parent);
                var anim = newLeftArm.GetComponentInChildren<Animator>();
                if (anim != null)
                    GameObject.Destroy(anim);

                newLeftArm.transform.SetParent(P03AnimationController.Instance.transform, true);

                Transform elbow = newLeftArm.transform.Find("LeshyNew_Root/LeshyNew_LowerBack/LeshyNew_Chest/LeshyNew_Chest1/LeshyNew_Chest2/LeshyNew_RightCollar/LeshyNew_RightShoulder/LeshyNew_RightElbow");
                Transform wrist = newLeftArm.transform.Find("LeshyNew_Root/LeshyNew_LowerBack/LeshyNew_Chest/LeshyNew_Chest1/LeshyNew_Chest2/LeshyNew_RightCollar/LeshyNew_RightShoulder/LeshyNew_RightElbow/LeshyNew_RightWrist");

                newLeftArm.SetActive(true);
                elbow.localPosition = new(3.2957f, 7.5788f, 1.2436f);
                elbow.localEulerAngles = new(13.182f, 334.1849f, 43.4195f);
                yield return new WaitForSeconds(0.1f);

                Tween.LocalPosition(elbow, new Vector3(-4.0613f, -3.3674f, 1.3031f), 3f, 0f, Tween.EaseInOut);
                Tween.LocalPosition(wrist, new Vector3(-5.9689f, -0.0341f, -0.0775f), 3f, 0f, Tween.EaseInOut);
                Tween.LocalRotation(wrist, new Vector3(65.9436f, 159.2189f, 172.2731f), 3f, 0f, Tween.EaseInOut);

                yield return new WaitForSeconds(4f);

                Tween.LocalPosition(elbow, new Vector3(-3.174f, -1.9196f, -0.8266f), 2f, 0f, Tween.EaseInOut);
                Tween.LocalRotation(elbow, new Vector3(358.182f, 304.1849f, 43.4195f), 2f, 0f, Tween.EaseInOut);

                yield return new WaitForSeconds(3f);

                Tween.LocalRotation(elbow, new Vector3(343.182f, 29.1849f, 53.4195f), 0.15f, 0f, Tween.EaseInOut);
                yield return new WaitForSeconds(0.09f);

                foreach (var ps in CyberspaceParticles.GetComponentsInChildren<ParticleSystem>())
                {
                    // Stop making new particles
                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);

                    // Update all currently existing particles
                    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
                    int num = ps.GetParticles(particles);

                    for (int i = 0; i < num; i++)
                    {
                        particles[i].velocity = new Vector3(
                            UnityEngine.Random.value < 0.5f ? -1f : 1f,
                            1f - (UnityEngine.Random.value * 2f),
                            0f
                        ) * 20f;
                        particles[i].angularVelocity = 5f;
                    }

                    ps.SetParticles(particles, num);
                }

                var shake = Tween.Shake(CameraEffects.Instance.transform, Vector3.zero, new Vector3(0.1f, 0.1f, 0f), 0.05f, 0f, loop: Tween.LoopType.Loop);

                var timedelta = Time.fixedDeltaTime;

                Time.timeScale = 0.045f;
                Time.fixedDeltaTime = timedelta * 0.045f;

                AudioController.Instance.SetLoopPaused(true);
                AudioController.Instance.PlaySound2D("anime_sword_hit_2", MixerGroup.TableObjectsSFX, 0.8f);

                Transform head = P03AnimationController.Instance.headAnim.transform.parent;
                Tween.LocalPosition(head, new Vector3(-15.5001f, 3.9984f, -0.1134f), 0.3f, 0f);
                Tween.Rotate(head, new Vector3(0f, 0f, 360f), Space.Self, 0.1f, 0f, loop: Tween.LoopType.Loop);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Disconnected, false, false);

                // And make all the other stuff go away
                AnimeFlyAway(ResourceDrone.Instance.transform, Vector3.right * 9f);
                AnimeFlyAway(ItemsManager.Instance.transform, new Vector3(9f, 0f, 0f));
                GameObject.Destroy((ItemsManager.Instance as Part3ItemsManager).hammerSlot.Consumable.gameObject, 0.1f);
                AnimeFlyAway((BoardManager.Instance as BoardManager3D).bell.transform, Vector3.left * 9f);
                AnimeFlyAway(LifeManager.Instance.Scales3D.transform, Vector3.right * -9f);
                AnimeFlyAway(LeftScreen.transform, Vector3.right * 11f);
                AnimeFlyAway(RightScreen.transform, Vector3.right * 11f);

                if ((CardDrawPiles.Instance as CardDrawPiles3D).pile != null)
                    StartCoroutine((CardDrawPiles.Instance as CardDrawPiles3D).pile.DestroyCards(Vector3.right * 9f, 30f, 0.0075f));

                if ((CardDrawPiles.Instance as CardDrawPiles3D).sidePile != null)
                    StartCoroutine((CardDrawPiles.Instance as CardDrawPiles3D).sidePile.DestroyCards(Vector3.right * 9f, 30f, 0.0075f));

                StartCoroutine(PlayerHand.Instance.CleanUp());
                foreach (var slot in BoardManager.Instance.AllSlotsCopy)
                {
                    slot.Card?.ExitBoard(0.25f, Vector3.right * 9f);
                    slot.SetShown(false, true);
                }

                foreach (var slot in BoardManager.Instance.opponentQueueSlots)
                    slot.SetShown(false, true);

                foreach (var card in TurnManager.Instance.opponent.queuedCards)
                    card.ExitBoard(0.25f, Vector3.right * 9f);

                yield return new WaitForSeconds(0.3f);
                Time.timeScale = 1f;
                Time.fixedDeltaTime = timedelta;
                shake.Stop();

                Tween.LocalPosition(elbow, new Vector3(4.7227f, 7.0035f, 0.5946f), 2f, 0f, completeCallback: () => GameObject.Destroy(leftArm));

                yield return new WaitForSeconds(5f);
                FactoryScrybes scrybes = FactoryManager.Instance.Scrybes;
                scrybes.Show();
                scrybes.EnterOtherScrybes();
                yield return new WaitForSeconds(2f);

                scrybes.leshy.gameObject.SetActive(true);
                scrybes.leshy.transform.localScale = new(0.6f, 0.5f, 0.7f);
                Tween.LocalPosition(scrybes.leshy.transform, new Vector3(-4.75f, 7.99f, 0f), 1f, 0f);


                yield return TextDisplayer.Instance.PlayDialogueEvent("GrimoraMultiverseFinale", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.magnificus.SetHeadTrigger("point_brush");
                yield return TextDisplayer.Instance.PlayDialogueEvent("MagnificusMultiverseFinale", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.leshy.SetEyesAnimated(true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyMultiverseFinale", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.leshy.SetEyesAnimated(false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("GrimoraMultiverseFinale2", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.magnificus.SetHeadTrigger("pensive");
                yield return TextDisplayer.Instance.PlayDialogueEvent("MagnificusMultiverseFinale2", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.leshy.SetEyesAnimated(true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyMultiverseFinale2", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.leshy.SetEyesAnimated(false);

                AchievementManager.Unlock(P03AchievementManagement.MULTIVERSE);
                yield return new WaitForSeconds(1.5f);
                PauseMenu.pausingDisabled = false;
                EventManagement.FinishAscension(true);
            }
            else
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseLost", TextDisplayer.MessageAdvanceMode.Input);
            }
        }

        public override IEnumerator GameEnd(bool playerWon)
        {
            if (!playerWon)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseLost2", TextDisplayer.MessageAdvanceMode.Input);
                yield return new WaitForSeconds(1.5f);
                EventManagement.FinishAscension(false);
            }
        }

        public static List<CardSlot> GetParentSlotList(CardSlot slot)
        {
            List<CardSlot> slotsToCheck = BoardManager.Instance.GetSlots(slot.IsPlayerSlot);
            if (MultiverseBattleSequencer.Instance != null && MultiverseBattleSequencer.Instance.MultiverseGames != null)
            {
                int uIdx = MultiverseBattleSequencer.Instance.GetUniverseId(slot);
                var universe = MultiverseBattleSequencer.Instance.MultiverseGames[uIdx];
                slotsToCheck = slot.IsPlayerSlot ? universe.PlayerSlots : universe.OpponentSlots;
            }
            return slotsToCheck;
        }

        protected class RefBoolean
        {
            public bool Value;
        }

        private static ConditionalWeakTable<CardSlot, RefBoolean> IsPlayerSlot = new();

        [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.IsPlayerSlot), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool MultiverseIsPlayerSlot(CardSlot __instance, ref bool __result)
        {
            if (IsPlayerSlot.TryGetValue(__instance, out RefBoolean value))
            {
                __result = value.Value;
                return false;
            }

            if (BoardManager.Instance.playerSlots.Contains(__instance))
            {
                __result = true;
            }
            else if (BoardManager.Instance.opponentSlots.Contains(__instance))
            {
                __result = false;
            }
            else if (Instance != null && Instance.MultiverseGames != null)
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
            else
            {
                return true;
            }

            IsPlayerSlot.Add(__instance, new() { Value = __result });
            return false;
        }

        private class RefInt
        {
            public int Value;
        }
        private static ConditionalWeakTable<CardSlot, RefInt> SlotIndex = new();

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        internal static void ClearAllSlotCacheShenanigans()
        {
            foreach (var slot in BoardManager.Instance.AllSlots)
            {
                SlotIndex.Remove(slot);
                IsPlayerSlot.Remove(slot);
            }
        }

        [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.Index), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool CachedMultiverseAwareIndexLookup(CardSlot __instance, ref int __result)
        {
            if (SlotIndex.TryGetValue(__instance, out RefInt value))
            {
                __result = value.Value;
                if (__instance.transform.localPosition.y > -3f)
                    __result %= 10;
                return false;
            }

            if (Instance == null)
            {
                __result = BoardManager.Instance.GetSlots(__instance.IsPlayerSlot).IndexOf(__instance);
                SlotIndex.Add(__instance, new() { Value = __result });
                return false;
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
            if (__instance.transform.localPosition.y > -3f)
                __result %= 10;
            return false;
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