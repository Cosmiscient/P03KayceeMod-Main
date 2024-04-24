using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Cards.Multiverse;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class MultiverseGameState
    {
        public static List<ConsumableItemSlot> ConsumableSlots => ItemsManager.Instance.consumableSlots.Where(s => s is not HammerItemSlot).ToList();

        public enum Phase
        {
            Any = -1,
            GameIsOver = 0,
            PlayerUpkeepAndMain = 1,
            PlayerCombat = 3,
            PlayerEnd = 4,
            AfterPlayer = 5,
            OpponentUpkeep = 6,
            OpponentMain = 7,
            OpponentCombat = 8,
            OpponentEnd = 9,
            AfterOpponent = 10
        }

        private static GameObject _offboardHandParent;
        private static GameObject OffboardHandParent
        {
            get
            {
                if (_offboardHandParent.SafeIsUnityNull())
                {
                    _offboardHandParent = new("OffboardHandParent");
                    _offboardHandParent.transform.position = new(0f, -20f, 0f);
                }
                return _offboardHandParent;
            }
        }

        public static event Action<MultiverseGameState> StateRestored;

        public static float OFFBOARD_SLOT_Y { get; private set; } = -15f;
        public static float ONBOARD_SLOT_Y { get; private set; } = -15f;

        // public BoardState BoardState { get; private set; }

        public List<CardSlot> PlayerSlots { get; private set; }

        public List<CardSlot> OpponentSlots { get; private set; }

        public List<PlayableCard> OpponentQueue { get; private set; }

        public int OpponentDamage { get; internal set; }

        public int PlayerDamage { get; internal set; }

        public int LifeBalance => OpponentDamage - PlayerDamage;

        public bool GameIsActive => Mathf.Abs(LifeBalance) < 5;

        public List<List<CardInfo>> OpponentTurnPlan { get; private set; }

        public List<PlayableCard> HandState { get; private set; }

        public List<CardInfo> MainDeckState { get; set; }

        public List<CardInfo> SideDeckState { get; set; }

        public List<string> ItemState { get; private set; }

        public int BonesState { get; private set; }

        public int MaxEnergyState { get; private set; }

        public int EnergyState { get; private set; }

        public bool PlayerRungBell { get; internal set; }

        public bool PlayerNeedsUpkeepStep { get; internal set; }

        public bool HasEverHadUpkeepStep { get; internal set; }

        public P03AnimationController.Face P03Face { get; private set; }

        public int OpponentNumTurnsTaken { get; private set; }

        public TurnManager.PlayerTurnPhase PlayerTurnPhase { get; private set; }
        public bool IsPlayerTurn { get; private set; }
        public bool IsPlayerUpkeep { get; private set; }
        public bool IsSetupPhase { get; private set; }
        public Phase CurrentPhase { get; private set; }

        public int DamageDealtThisPhase { get; private set; }

        public LightColorState ColorState { get; private set; }

        private Dictionary<Phase, List<IMultiverseDelayedCoroutine>> Callbacks { get; set; }

        public Texture2D Screenshot { get; private set; }

        public void SetScreenshot(Texture2D screenshot)
        {
            if (this.Screenshot != null)
                GameObject.Destroy(Screenshot);
            Screenshot = screenshot;
        }

        public bool HasAbility(Ability ability, bool forPlayer)
        {
            var slots = forPlayer ? PlayerSlots : OpponentSlots;
            return slots.Any(s => s.Card != null && s.Card.HasAbility(ability));
        }

        private static CardInfo FullClone(CardInfo info)
        {
            CardInfo retval = (CardInfo)info.Clone();
            retval.mods = retval.mods.Select(cm => (CardModificationInfo)cm.Clone()).ToList();
            return retval;
        }

        private static void DestroyChildCard(CardSlot slot)
        {
            for (int i = 0; i < slot.transform.childCount; i++)
            {
                Transform c = slot.transform.GetChild(i);
                if (c.gameObject.name.ToLowerInvariant().StartsWith("card"))
                {
                    GameObject.Destroy(c.gameObject);
                    return;
                }
            }
        }

        public static MultiverseGameState GenerateAlternateStartingState(int randomSeed, int colorIndex)
        {
            // This should be done at the start of the game
            MultiverseGameState state = new();

            // The starting draw has already happened. We need to put the player's hand back in the deck
            state.SideDeckState = Part3CardDrawPiles.CreateVesselDeck();
            state.MainDeckState = Part3SaveData.Data.deck.CardInfos.Select(ci => CardLoader.Clone(ci)).ToList();
            state.HandState = new();

            // We need to shuffle the decks
            state.MainDeckState = state.MainDeckState.OrderBy(ci => SeededRandom.Value(randomSeed++) * 100f).Select(ci => FullClone(ci)).ToList();
            state.SideDeckState = state.SideDeckState.OrderBy(ci => SeededRandom.Value(randomSeed++) * 100f).Select(ci => FullClone(ci)).ToList();

            // Change the color
            state.ColorState = LightColorState.GetPreset(colorIndex);

            // Items
            state.ItemState = new();
            state.ItemState.AddRange(MultiverseBattleSequencer.Instance.ItemStartingState);

            // Reset some basic stuff
            state.MaxEnergyState = 0;
            state.EnergyState = 0;
            state.BonesState = 0;
            state.OpponentDamage = 0;
            state.PlayerDamage = 0;
            state.OpponentNumTurnsTaken = 0;
            state.DamageDealtThisPhase = 0;
            state.HasEverHadUpkeepStep = false;
            state.P03Face = P03AnimationController.Face.Default;
            state.CurrentPhase = (MultiverseBattleSequencer.Instance?.ActiveMultiverse?.CurrentPhase).GetValueOrDefault(Phase.OpponentUpkeep);

            // Clone the cardslots
            state.PlayerSlots = BoardManager.Instance.playerSlots.Select(cs => GameObject.Instantiate(cs.gameObject, cs.transform.parent).GetComponent<CardSlot>()).ToList();
            state.OpponentSlots = BoardManager.Instance.opponentSlots.Select(cs => GameObject.Instantiate(cs.gameObject, cs.transform.parent).GetComponent<CardSlot>()).ToList();
            state.OpponentQueue = new();
            state.OpponentTurnPlan = new();

            for (int i = 0; i < state.PlayerSlots.Count; i++)
            {
                state.PlayerSlots[i].opposingSlot = state.OpponentSlots[i];
                state.OpponentSlots[i].opposingSlot = state.PlayerSlots[i];

                state.PlayerSlots[i].Card = null;
                state.OpponentSlots[i].Card = null;

                DestroyChildCard(state.PlayerSlots[i]);
                DestroyChildCard(state.OpponentSlots[i]);
            }

            return state;
        }

        public static MultiverseGameState GenerateFromCurrentState()
        {
            MultiverseGameState state = new();
            P03Plugin.Log.LogInfo($"Saving resources {ResourcesManager.Instance.PlayerEnergy}/{ResourcesManager.Instance.PlayerEnergy} Energy. {ResourcesManager.Instance.PlayerBones} Bones.");
            state.EnergyState = ResourcesManager.Instance.PlayerEnergy;
            state.MaxEnergyState = ResourcesManager.Instance.PlayerMaxEnergy;
            state.BonesState = ResourcesManager.Instance.PlayerBones;

            var newItems = ItemsManager.Instance.Consumables.Select(i => i.Data.name).Where(s => !s.Equals("hammer", StringComparison.InvariantCultureIgnoreCase)).ToList();
            P03Plugin.Log.LogInfo($"Saving Items: " + string.Join(", ", newItems));
            state.ItemState = newItems;

            P03Plugin.Log.LogInfo($"Saving Cards In Hand: " + string.Join(", ", PlayerHand.Instance.CardsInHand.Select(c => c.name)));
            state.HandState = new(PlayerHand.Instance.CardsInHand);

            state.PlayerRungBell = (MultiverseBattleSequencer.Instance?.ActiveMultiverse?.PlayerRungBell).GetValueOrDefault(false);

            P03Plugin.Log.LogInfo($"Saving Turn State. Phase {TurnManager.Instance.PlayerPhase} IsPlayerTurn {TurnManager.Instance.IsPlayerTurn} IsSetup {TurnManager.Instance.IsSetupPhase} Damage Dealt {TurnManager.Instance.CombatPhaseManager.DamageDealtThisPhase}");
            state.PlayerTurnPhase = TurnManager.Instance.PlayerPhase;
            state.IsPlayerTurn = TurnManager.Instance.IsPlayerTurn;
            state.IsSetupPhase = TurnManager.Instance.IsSetupPhase;
            state.CurrentPhase = MultiverseBattleSequencer.Instance.CurrentPhase;
            state.DamageDealtThisPhase = TurnManager.Instance.CombatPhaseManager.DamageDealtThisPhase;

            P03Plugin.Log.LogInfo($"Saving Cards In Main Deck: " + string.Join(", ", CardDrawPiles3D.Instance.Deck.cards.Select(c => c.name)));
            state.MainDeckState = new(CardDrawPiles3D.Instance.Deck.cards);

            P03Plugin.Log.LogInfo($"Saving Cards In Side Deck: " + string.Join(", ", CardDrawPiles3D.Instance.SideDeck.cards.Select(c => c.name)));
            state.SideDeckState = new(CardDrawPiles3D.Instance.SideDeck.cards);

            P03Plugin.Log.LogInfo($"Saving Lights");
            state.ColorState = LightColorState.FromCurrent();

            P03Plugin.Log.LogInfo($"Copying turn plan");
            state.OpponentTurnPlan = new(TurnManager.Instance.Opponent.TurnPlan.Select(lci => new List<CardInfo>(lci)));

            P03Plugin.Log.LogInfo($"Saving Opponent Queue: " + string.Join(", ", TurnManager.Instance.Opponent.Queue.Select(c => c.name)));
            state.OpponentQueue = new(TurnManager.Instance.Opponent.Queue);

            P03Plugin.Log.LogInfo($"Saving Slots");
            state.PlayerSlots = new(BoardManager.Instance.playerSlots);
            state.OpponentSlots = new(BoardManager.Instance.opponentSlots);

            P03Plugin.Log.LogInfo($"Saving Player Draw status");
            state.PlayerNeedsUpkeepStep = (MultiverseBattleSequencer.Instance?.ActiveMultiverse?.PlayerNeedsUpkeepStep).GetValueOrDefault(false);
            state.HasEverHadUpkeepStep = (MultiverseBattleSequencer.Instance?.ActiveMultiverse?.HasEverHadUpkeepStep).GetValueOrDefault(false);

            P03Plugin.Log.LogInfo($"Saving damage state");
            state.PlayerDamage = LifeManager.Instance.PlayerDamage;
            state.OpponentDamage = LifeManager.Instance.OpponentDamage;

            state.P03Face = P03AnimationController.Instance.CurrentFace;

            state.OpponentNumTurnsTaken = TurnManager.Instance.opponent.NumTurnsTaken;

            state.SetScreenshot(MultiverseBattleSequencer.Instance?.ActiveMultiverse?.Screenshot);

            state.Callbacks = MultiverseBattleSequencer.Instance?.ActiveMultiverse?.Callbacks;

            return state;
        }

        public void RegisterCallback(IMultiverseDelayedCoroutine callback)
        {
            RegisterCallback(Phase.Any, callback);
        }

        public void RegisterCallback(Phase phase, IMultiverseDelayedCoroutine callback)
        {
            this.Callbacks ??= new();

            if (!this.Callbacks.ContainsKey(phase))
                this.Callbacks.Add(phase, new());
            this.Callbacks[phase].Add(callback);
        }

        public IEnumerator DoCallbacks(Phase phase)
        {
            if (Callbacks != null)
            {
                if (Callbacks.ContainsKey(Phase.Any) && Callbacks[Phase.Any] != null)
                {
                    foreach (var co in Callbacks[Phase.Any].Where(co => !co.SafeIsUnityNull()))
                        yield return co.DoCallback();

                    Callbacks[Phase.Any].Clear();
                }

                if (Callbacks.ContainsKey(phase) && Callbacks[phase] != null)
                {
                    foreach (var co in Callbacks[phase].Where(co => !co.SafeIsUnityNull()))
                        yield return co.DoCallback();

                    Callbacks[phase].Clear();
                }
            }
            yield break;
        }

        private IEnumerator CleanUpAsCurrent()
        {
            foreach (var slot in BoardManager.Instance.AllSlotsCopy)
            {
                if (slot.Card != null)
                {
                    foreach (var behavior in slot.Card.GetComponents<SpecialCardBehaviour>())
                        behavior.OnCleanUp();
                    GameObject.Destroy(slot.Card.gameObject);
                }
            }

            OpponentSlots.Clear();
            PlayerSlots.Clear();

            List<PlayableCard> queueCards = new();
            queueCards.AddRange(TurnManager.Instance.opponent.queuedCards);
            TurnManager.Instance.opponent.queuedCards.Clear();

            foreach (var card in queueCards)
            {
                if (card != null)
                {
                    foreach (var behavior in card.GetComponents<SpecialCardBehaviour>())
                        behavior.OnCleanUp();
                    GameObject.Destroy(card.gameObject);
                }
            }

            OpponentQueue.Clear();

            yield return new WaitForEndOfFrame();
            List<PlayableCard> cardsInHand = new(PlayerHand.Instance.cardsInHand);
            PlayerHand.Instance.cardsInHand.Clear();

            foreach (var card in cardsInHand)
                if (card != null)
                    GameObject.Destroy(card.gameObject);

            HandState.Clear();

            yield return new WaitForEndOfFrame();

            yield break;
        }

        internal IEnumerator CleanUp()
        {

            if (this == MultiverseBattleSequencer.Instance.ActiveMultiverse)
            {
                P03Plugin.Log.LogInfo("Cleaning up current universe");
                yield return CleanUpAsCurrent();
                yield break;
            }

            P03Plugin.Log.LogInfo("Cleaning up universe");

            // Deletes everything in this game state
            foreach (var card in OpponentQueue)
                GameObject.Destroy(card.gameObject);

            OpponentQueue.Clear();

            foreach (var slot in PlayerSlots.Concat(OpponentSlots))
            {
                if (slot.Card != null)
                {
                    foreach (var behavior in slot.Card.GetComponents<SpecialCardBehaviour>())
                        behavior.OnCleanUp();
                    GameObject.Destroy(slot.Card.gameObject);
                }
            }

            foreach (var card in HandState)
                GameObject.Destroy(card.gameObject);

            yield return new WaitForEndOfFrame();

            foreach (var slot in PlayerSlots.Concat(OpponentSlots))
                GameObject.Destroy(slot.gameObject);

            P03Plugin.Log.LogInfo("Universe cleanup complete");
        }

        public IEnumerator RestoreState(Action restoredCallback = null)
        {
            ViewManager.Instance.SwitchToView(View.Default);

            UIManager.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, .4f);
            CameraEffects.Instance.Shake(0.1f, .4f);
            AudioController.Instance.PlaySound2D("glitch_error", MixerGroup.None, .5f, 0f, pitch: new AudioParams.Pitch(AudioParams.Pitch.Variation.Medium));

            // Remove all cards
            // P03Plugin.Log.LogInfo("Restoring multiverse state: Removing cards in slots");
            // foreach (CardSlot cardSlot in BoardManager.Instance.AllSlots)
            // {
            //     if (cardSlot.Card != null)
            //     {
            //         PlayableCard card = cardSlot.Card;
            //         cardSlot.Card.UnassignFromSlot();
            //         GameObject.Destroy(card.gameObject);
            //     }
            // }

            // Restore all cards
            P03Plugin.Log.LogInfo("Restoring multiverse state: Moving slots away from position");
            BoardManager.Instance.AllSlots.ForEach(cs => SetCardSlotPosition(cs.gameObject, false));
            TurnManager.Instance.Opponent.Queue.ForEach(pc => SetCardSlotPosition(pc.gameObject, false));

            P03Plugin.Log.LogInfo("Restoring multiverse state: Changing active slots");
            BoardManager.Instance.playerSlots.Clear();
            BoardManager.Instance.opponentSlots.Clear();
            BoardManager.Instance.playerSlots.AddRange(this.PlayerSlots);
            BoardManager.Instance.opponentSlots.AddRange(this.OpponentSlots);
            BoardManager.Instance.allSlots = null;

            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting opponent queue");
            TurnManager.Instance.Opponent.Queue.Clear();
            TurnManager.Instance.Opponent.Queue.AddRange(this.OpponentQueue);

            P03Plugin.Log.LogInfo("Restoring multiverse state: Moving slots into position");
            BoardManager.Instance.AllSlots.ForEach(cs => SetCardSlotPosition(cs.gameObject, true));
            TurnManager.Instance.Opponent.Queue.ForEach(pc => SetCardSlotPosition(pc.gameObject, true));

            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting opponent turn plan");
            TurnManager.Instance.Opponent.TurnPlan = new(OpponentTurnPlan.Select(lci => new List<CardInfo>(lci)));

            // Restore the player's hand
            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting player hand");
            foreach (var card in PlayerHand.Instance.CardsInHand)
                card.transform.SetParent(OffboardHandParent.transform, false);
            PlayerHand.Instance.CardsInHand.Clear();
            foreach (var card in this.HandState)
                card.transform.SetParent(PlayerHand.Instance.cardsParent);
            PlayerHand.Instance.CardsInHand.AddRange(this.HandState);
            (PlayerHand.Instance as PlayerHand3D).aboveHandCards.Clear();

            if (PlayerHand.Instance.CardsInHand.Count > 0)
                PlayerHand.Instance.OnCardInspected(PlayerHand.Instance.CardsInHand[0]);

            // Set the scale state
            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting scale");
            LifeManager.Instance.SetNumWeightsImmediate(PlayerDamage, OpponentDamage);

            // Set the item state
            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting items to " + string.Join(",", ItemState));
            UpdateItems(ItemState);

            // Set the energy state
            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting energy");
            ResourcesManager.Instance.PlayerMaxEnergy = MaxEnergyState;
            ResourcesManager.Instance.PlayerEnergy = EnergyState;
            for (int i = 0; i < ResourceDrone.Instance.cellAnims.Count; i++)
            {
                ResourceDrone.Instance.cellAnims[i].Play(
                    MaxEnergyState < (i + 1) ? "close" : "open",
                    0,
                    1f
                );
                if (EnergyState < (i + 1))
                    ResourceDrone.Instance.cellRenderers[i].material.DisableKeyword("_EMISSION");
                else
                    ResourceDrone.Instance.cellRenderers[i].material.EnableKeyword("_EMISSION");
            }

            // Set the bones state
            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting bones");
            ResourcesManager.Instance.PlayerBones = BonesState;

            // Combat Phase state
            P03Plugin.Log.LogInfo("Restoring multiverse state: Resetting damage dealt this phase");
            Traverse.Create(TurnManager.Instance.CombatPhaseManager).Field("<DamageDealtThisPhase>k__BackingField").SetValue(DamageDealtThisPhase);

            // Restore the decks
            P03Plugin.Log.LogInfo("Restoring multiverse state: restoring main deck");
            CardDrawPiles3D.Instance.Deck.cards.Clear();
            CardDrawPiles3D.Instance.Deck.cards.AddRange(MainDeckState);
            P03Plugin.Log.LogInfo("Restoring multiverse state: restoring main deck");

            P03Plugin.Log.LogInfo("Restoring multiverse state: restoring side deck");
            CardDrawPiles3D.Instance.SideDeck.cards.Clear();
            CardDrawPiles3D.Instance.SideDeck.cards.AddRange(SideDeckState);

            P03Plugin.Log.LogInfo("Restoring multiverse state: restoring main pile");
            CardDrawPiles3D.Instance.pile.DestroyCardsImmediate();
            CardDrawPiles3D.Instance.pile.CreateCards(MainDeckState.Count);
            CardDrawPiles3D.Instance.pile.SetEnabled(true);

            P03Plugin.Log.LogInfo("Restoring multiverse state: restoring side pile");
            CardDrawPiles3D.Instance.sidePile.DestroyCardsImmediate();
            CardDrawPiles3D.Instance.sidePile.CreateCards(SideDeckState.Count);
            CardDrawPiles3D.Instance.sidePile.SetEnabled(true);

            TurnManager.Instance.opponent.NumTurnsTaken = OpponentNumTurnsTaken;

            // Restore colors
            P03Plugin.Log.LogInfo("Restoring colors");
            ColorState.RestoreState();

            // Restore gems
            ResourcesManager.Instance.ForceGemsUpdate();

            // Restore face
            if (MultiverseBattleSequencer.Instance.CurrentPhase == Phase.PlayerUpkeepAndMain && this.PlayerRungBell)
                P03AnimationController.Instance.FaceRenderer.DisplayFace(P03BellFace.ID);
            else if (this.P03Face == P03BellFace.ID)
                P03AnimationController.Instance.FaceRenderer.DisplayFace(P03AnimationController.Face.Default);
            else
                P03AnimationController.Instance.FaceRenderer.DisplayFace(this.P03Face);

            // States
            yield return MultiverseBattleSequencer.Instance.SetPhase(this.CurrentPhase, suppressCallbacks: true);

            // Fire event
            MultiverseGameState.StateRestored?.Invoke(this);

            // Wait for everything to calm down
            yield return new WaitForSeconds(0.4f);

            // Do all the coroutine callbacks
            yield return DoCallbacks(this.CurrentPhase);

            restoredCallback?.Invoke();
        }

        internal static void UpdateItems(List<string> itemContents)
        {
            ItemsManager.Instance.SaveDataItemsList.Clear();
            ItemsManager.Instance.SaveDataItemsList.AddRange(itemContents);

            var slots = MultiverseGameState.ConsumableSlots;

            foreach (var itemSlot in slots)
            {
                try
                {
                    itemSlot.DestroyItem();
                }
                catch
                {
                    // Do nothing
                }
            }

            for (int i = 0; i < itemContents.Count; i++)
                slots[i].CreateItem(itemContents[i], true);

            ItemsManager.Instance.OnUpdateItems(true);
        }

        private static void SetCardSlotPosition(GameObject obj, bool onBoard)
        {
            // if (ONBOARD_SLOT_Y < -2f && !onBoard)
            //     ONBOARD_SLOT_Y = obj.transform.localPosition.y;
            if (onBoard && obj.transform.localPosition.y < -5f)
                obj.transform.localPosition = obj.transform.localPosition + Vector3.up * 20f;
            else if (!onBoard && obj.transform.localPosition.y > -5f)
                obj.transform.localPosition = obj.transform.localPosition - Vector3.up * 20f;
        }

        private void ForceCellOpen(int cellIndex)
        {
            ResourceDrone.Instance.cellAnims[cellIndex].Play("open", 0, 1f);
        }

        private void ForceCellClosed(int cellIndex)
        {
            ResourceDrone.Instance.cellAnims[cellIndex].Play("close", 0, 1f);
        }

        private void ApplySlotStates(List<BoardState.SlotState> slotStates, List<CardSlot> actualSlots)
        {
            for (int i = 0; i < slotStates.Count; i++)
            {
                if (slotStates[i].card != null)
                {
                    BoardManager.Instance.StartCoroutine(this.ApplySlotState(slotStates[i], actualSlots[i]));
                }
            }
        }

        private IEnumerator ApplySlotState(BoardState.SlotState slotState, CardSlot slot)
        {
            yield return BoardManager.Instance.CreateCardInSlot(slotState.card.info, slot, 0f, false);
            PlayableCard card = slot.Card;
            (card.Anim as DiskCardAnimationController)?.Expand(true);
            foreach (var mod in slotState.card.temporaryMods)
            {
                if (string.IsNullOrEmpty(mod.singletonId) || !card.TemporaryMods.Any(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(mod.singletonId)))
                    card.TemporaryMods.Add(mod);
            }
            card.Status = new PlayableCardStatus(slotState.card.status);
            card.OnStatsChanged();
            ResourcesManager.Instance.ForceGemsUpdate();
            ConduitGainAbility.ActiveAbilities = BoardManager.Instance.AllSlotsCopy.Where(s => s.Card != null).SelectMany(s => s.Card.GetComponents<ConduitGainAbility>()).ToList();
            yield break;
        }

        private PlayableCard FromCardState(BoardState.CardState state)
        {
            PlayableCard card = CardSpawner.SpawnPlayableCard(state.info);
            if (state.status != null)
                card.Status = state.status;
            foreach (var mod in state.temporaryMods)
            {
                if (string.IsNullOrEmpty(mod.singletonId) || !card.TemporaryMods.Any(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(mod.singletonId)))
                    card.TemporaryMods.Add(mod);
            }
            card.OnStatsChanged();
            return card;
        }

        private void RestoreQueueState(List<BoardState.CardState> queueState)
        {
            TurnManager.Instance.Opponent.Queue.ForEach(pc => GameObject.Destroy(pc.gameObject));
            TurnManager.Instance.Opponent.Queue.Clear();

            for (int i = 0; i < queueState.Count; i++)
            {
                if (queueState[i] == null)
                    continue;

                PlayableCard card = FromCardState(queueState[i]);
                BoardManager.Instance.QueueCardForSlot(card, BoardManager.Instance.OpponentSlotsCopy[i], 0f, false, false);
            }
        }

        private void AddCardToHand(PlayableCard card)
        {
            PlayerHand.Instance.ParentCardToHand(card, CardSpawner.Instance.spawnedPositionOffset);
            PlayerHand.Instance.cardsInHand.Add(card);
            PlayerHand.Instance.SetCardPositions();
        }

        public bool HasPhaseCallback(MultiverseGameState.Phase phase)
        {
            if (Callbacks == null)
                return false;

            if (!Callbacks.ContainsKey(phase))
                return false;

            return Callbacks[phase].Count > 0;
        }

        public bool RespondsToTrigger(Trigger trigger, params object[] args)
        {
            // Temporarily assign the slots back and then check noncardtriggerreceivers
            List<CardSlot> playerSlots = new(BoardManager.Instance.playerSlots);
            List<CardSlot> opponentSlots = new(BoardManager.Instance.opponentSlots);
            BoardManager.Instance.playerSlots.Clear();
            BoardManager.Instance.playerSlots.AddRange(this.PlayerSlots);
            BoardManager.Instance.opponentSlots.Clear();
            BoardManager.Instance.opponentSlots.AddRange(this.OpponentSlots);
            BoardManager.Instance.allSlots = null;

            bool foundReceiver = false;
            foreach (var card in this.PlayerSlots.Where(c => c.Card != null).Select(c => c.Card))
            {
                if (card.TriggerHandler.RespondsToTrigger(trigger, args))
                {
                    foundReceiver = true;
                    break;
                }
            }

            if (!foundReceiver)
            {
                foreach (var card in this.OpponentSlots.Where(c => c.Card != null).Select(c => c.Card))
                {
                    if (card.TriggerHandler.RespondsToTrigger(trigger, args))
                    {
                        foundReceiver = true;
                        break;
                    }
                }
            }

            if (!foundReceiver)
            {
                foreach (var triggerReceiver in GlobalTriggerHandler.Instance.nonCardReceivers)
                {
                    if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, triggerReceiver, args))
                    {
                        foundReceiver = true;
                        break;
                    }
                }
            }

            BoardManager.Instance.playerSlots.Clear();
            BoardManager.Instance.playerSlots.AddRange(playerSlots);
            BoardManager.Instance.opponentSlots.Clear();
            BoardManager.Instance.opponentSlots.AddRange(opponentSlots);
            BoardManager.Instance.allSlots = null;

            return foundReceiver;
        }

        public class LightColorState
        {
            private static List<P03AnimationController.Face> SpriteFaces = new()
            {
                P03AnimationController.Face.Choking,
                P03AnimationController.Face.Default,
                P03AnimationController.Face.Dying,
                P03AnimationController.Face.Happy,
                P03AnimationController.Face.Angry,
                P03AnimationController.Face.Bored,
                P03AnimationController.Face.Thinking,
                P03TrollFace.ID
            };

            public Color MainLightColor { get; private set; }
            public Color CardLightColor { get; private set; }
            public Color InteractablesColor { get; private set; }
            public Color SlotDefaultColor { get; private set; }
            public Color SlotInteractableColor { get; private set; }
            public Color SlotHighlightedColor { get; private set; }
            public Color QueueSlotDefaultColor { get; private set; }
            public Color QueueSlotInteractableColor { get; private set; }
            public Color QueueSlotHighlightedColor { get; private set; }
            public Color P03FaceColor { get; private set; }

            public static LightColorState FromCurrent()
            {
                LightColorState state = new()
                {
                    MainLightColor = ExplorableAreaManager.Instance.hangingLight.color,
                    CardLightColor = ExplorableAreaManager.Instance.hangingCardsLight.color,
                    InteractablesColor = (BoardManager.Instance as BoardManager3D).Bell.currentHighlightedColor,
                    SlotDefaultColor = BoardManager.Instance.AllSlots[0].currentDefaultColor,
                    SlotInteractableColor = BoardManager.Instance.AllSlots[0].currentInteractableColor,
                    SlotHighlightedColor = BoardManager.Instance.AllSlots[0].currentHighlightedColor,
                    QueueSlotDefaultColor = BoardManager.Instance.OpponentQueueSlots[0].currentDefaultColor,
                    QueueSlotInteractableColor = BoardManager.Instance.OpponentQueueSlots[0].currentInteractableColor,
                    QueueSlotHighlightedColor = BoardManager.Instance.OpponentQueueSlots[0].currentHighlightedColor,
                    P03FaceColor = P03AnimationController.Instance.FaceRenderer.faceObjects[(int)P03AnimationController.Face.Angry].GetComponent<SpriteRenderer>().color
                };
                return state;
            }

            private static Color GetHighlightComplement(Color color)
            {
                if (color == GameColors.Instance.blue)
                    return GameColors.Instance.seafoam;
                if (color == GameColors.Instance.purple)
                    return GameColors.Instance.lightPurple;
                if (color == GameColors.Instance.glowRed || color == GameColors.Instance.red)
                    return GameColors.Instance.brownOrange;
                if (color == GameColors.Instance.darkLimeGreen || color == GameColors.Instance.limeGreen)
                    return GameColors.Instance.fuschia;
                if (color == GameColors.Instance.gold)
                    return GameColors.Instance.darkLimeGreen;
                if (color == GameColors.Instance.nearWhite)
                    return GameColors.Instance.brightNearWhite;
                if (color == GameColors.Instance.darkFuschia)
                    return GameColors.Instance.limeGreen;

                return color;
            }

            private static Color GetSlotComplement(Color color)
            {
                Color retval = new Color(color.r, color.g, color.b);

                if (color == GameColors.Instance.blue)
                    retval = GameColors.Instance.limeGreen;
                if (color == GameColors.Instance.purple)
                    retval = GameColors.Instance.gray;
                if (color == GameColors.Instance.glowRed || color == GameColors.Instance.red)
                    retval = GameColors.Instance.orange;
                if (color == GameColors.Instance.darkLimeGreen)
                    retval = GameColors.Instance.brightBlue;
                if (color == GameColors.instance.limeGreen)
                    retval = GameColors.Instance.darkBlue;
                if (color == GameColors.Instance.gold)
                    retval = GameColors.Instance.fuschia;
                if (color == GameColors.Instance.nearWhite)
                    retval = GameColors.Instance.darkBlue;
                if (color == GameColors.Instance.darkFuschia)
                    retval = GameColors.Instance.brownOrange;

                retval.a = 0.5f;
                return retval;
            }

            private static readonly List<Color> UniverseColors = new()
            {
                GameColors.Instance.blue,
                GameColors.Instance.limeGreen,
                GameColors.Instance.glowRed,
                GameColors.Instance.nearWhite,
                GameColors.instance.gold,
                GameColors.Instance.purple,
                GameColors.Instance.darkLimeGreen,
                GameColors.Instance.darkFuschia
            };

            public static LightColorState GetPreset(int index)
            {
                return GetPreset(UniverseColors[index]);
            }

            public static LightColorState GetPreset(Color color)
            {
                LightColorState state = new();
                state.MainLightColor = state.SlotInteractableColor = state.QueueSlotInteractableColor = color;
                state.CardLightColor = Color.black;
                state.InteractablesColor = state.QueueSlotHighlightedColor = state.SlotHighlightedColor = GetHighlightComplement(color);
                state.QueueSlotDefaultColor = state.SlotDefaultColor = GetSlotComplement(color);
                state.P03FaceColor = color;
                return state;
            }

            public void RestoreState()
            {
                TableVisualEffectsManager.Instance.ChangeTableColors(
                    MainLightColor, CardLightColor, InteractablesColor, SlotDefaultColor, SlotInteractableColor, SlotHighlightedColor, QueueSlotDefaultColor, QueueSlotInteractableColor, QueueSlotHighlightedColor
                );

                // Particles
                P03AnimationController.Instance.SetWifiColor(MainLightColor);
                // GameObject tunnel = MultiverseBattleSequencer.Instance.CyberspaceParticles;
                // if (tunnel != null)
                //     foreach (ParticleSystem particles in tunnel.GetComponentsInChildren<ParticleSystem>())
                //         particles.UpdateParticleColors(MainLightColor, 0.2f);


                foreach (var face in SpriteFaces)
                {
                    try
                    {
                        GameObject obj = P03AnimationController.Instance.FaceRenderer.DisplayFace(face);
                        SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
                        spriteRenderer.color = P03FaceColor;
                    }
                    catch
                    {

                    }
                }
                P03AnimationController.Instance.FaceRenderer.DisplayFace(P03AnimationController.Instance.CurrentFace);
            }
        }
    }
}
