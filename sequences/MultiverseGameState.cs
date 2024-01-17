using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MultiverseGameState
    {
        public const float OFFBOARD_SLOT_Y = -15f;
        public const float ONBOARD_SLOT_Y = 0f;

        // public BoardState BoardState { get; private set; }

        public List<CardSlot> PlayerSlots { get; private set; }

        public List<CardSlot> OpponentSlots { get; private set; }

        public List<PlayableCard> OpponentQueue { get; private set; }

        public int OpponentDamage { get; internal set; }

        public int PlayerDamage { get; internal set; }

        public int LifeBalance => OpponentDamage - PlayerDamage;

        public List<List<CardInfo>> OpponentTurnPlan { get; private set; }

        public List<BoardState.CardState> HandState { get; private set; }

        public List<CardInfo> MainDeckState { get; set; }

        public List<CardInfo> SideDeckState { get; set; }

        public List<string> ItemState { get; private set; }

        public int BonesState { get; private set; }

        public int MaxEnergyState { get; private set; }

        public int EnergyState { get; private set; }

        public bool PlayerRungBell { get; internal set; }

        public TurnManager.PlayerTurnPhase PlayerTurnPhase { get; private set; }
        public bool IsPlayerTurn { get; private set; }
        public bool IsPlayerUpkeep { get; private set; }
        public bool IsSetupPhase { get; private set; }

        public int DamageDealtThisPhase { get; private set; }

        public LightColorState ColorState { get; private set; }

        public static MultiverseGameState GenerateAlternateStartingState(int randomSeed, Color colorPrefab)
        {
            // This should be done at the start of the game
            var state = GenerateFromCurrentState(false);
            state.MainDeckState = state.MainDeckState.OrderBy(ci => SeededRandom.Value(randomSeed++) * 100f).ToList();
            state.SideDeckState = state.SideDeckState.OrderBy(ci => SeededRandom.Value(randomSeed++) * 100f).ToList();

            // Mock the starting draw
            state.HandState.Add(new BoardState.CardState(state.SideDeckState[0], state.SideDeckState[0].Attack, state.SideDeckState[0].Health, new(), null, null));
            state.SideDeckState.RemoveAt(0);
            for (int i = 0; i < 3; i++)
                state.HandState.Add(new BoardState.CardState(state.MainDeckState[i], state.MainDeckState[i].Attack, state.MainDeckState[i].Health, new(), null, null));

            state.MainDeckState.RemoveAt(0);
            state.MainDeckState.RemoveAt(0);
            state.MainDeckState.RemoveAt(0);

            // Change the color
            state.ColorState = LightColorState.GetPreset(colorPrefab);

            // Clone the cardslots
            state.PlayerSlots = BoardManager.Instance.playerSlots.Select(cs => GameObject.Instantiate(cs.gameObject, cs.transform.parent).GetComponent<CardSlot>()).ToList();
            state.OpponentSlots = BoardManager.Instance.opponentSlots.Select(cs => GameObject.Instantiate(cs.gameObject, cs.transform.parent).GetComponent<CardSlot>()).ToList();

            return state;
        }

        public static MultiverseGameState GenerateFromCurrentState(bool rungBell)
        {
            MultiverseGameState state = new();
            state.EnergyState = ResourcesManager.Instance.PlayerEnergy;
            state.MaxEnergyState = ResourcesManager.Instance.PlayerMaxEnergy;
            state.BonesState = ResourcesManager.Instance.PlayerBones;
            state.ItemState = new(ItemsManager.Instance.SaveDataItemsList);
            state.HandState = PlayerHand.Instance.CardsInHand.Select(pc => new BoardState.CardState(pc.Info, pc.Info.Attack, pc.Info.Health, pc.temporaryMods, pc.Status, null)).ToList();
            state.PlayerRungBell = rungBell;
            state.PlayerTurnPhase = TurnManager.Instance.PlayerPhase;
            state.IsPlayerTurn = TurnManager.Instance.IsPlayerTurn;
            state.IsSetupPhase = TurnManager.Instance.IsSetupPhase;
            state.DamageDealtThisPhase = TurnManager.Instance.CombatPhaseManager.DamageDealtThisPhase;
            state.MainDeckState = new(CardDrawPiles3D.Instance.Deck.cards);
            state.SideDeckState = new(CardDrawPiles3D.Instance.SideDeck.cards);
            state.ColorState = LightColorState.FromCurrent();
            state.OpponentTurnPlan = new(TurnManager.Instance.Opponent.TurnPlan.Select(lci => new List<CardInfo>(lci)));

            state.OpponentQueue = new(TurnManager.Instance.Opponent.Queue);
            state.PlayerSlots = new(BoardManager.Instance.playerSlots);
            state.OpponentSlots = new(BoardManager.Instance.opponentSlots);

            return state;
        }

        public void RestoreState(Action restoredCallback = null)
        {
            // Remove all cards
            foreach (CardSlot cardSlot in BoardManager.Instance.AllSlots)
            {
                if (cardSlot.Card != null)
                {
                    PlayableCard card = cardSlot.Card;
                    cardSlot.Card.UnassignFromSlot();
                    GameObject.Destroy(card.gameObject);
                }
            }

            // Restore all cards
            BoardManager.Instance.AllSlots.ForEach(cs => SetCardSlotPosition(cs.gameObject, false));
            TurnManager.Instance.Opponent.Queue.ForEach(pc => SetCardSlotPosition(pc.gameObject, false));

            BoardManager.Instance.playerSlots = this.PlayerSlots;
            BoardManager.Instance.opponentSlots = this.OpponentSlots;
            BoardManager.Instance.allSlots = null;

            TurnManager.Instance.Opponent.Queue.Clear();
            TurnManager.Instance.Opponent.Queue.AddRange(this.OpponentQueue);

            TurnManager.Instance.Opponent.TurnPlan = new(OpponentTurnPlan.Select(lci => lci.Select(ci => (CardInfo)ci.Clone()).ToList()));

            // Restore the player's hand
            PlayerHand.Instance.CardsInHand.ForEach(p => GameObject.Destroy(p.gameObject));
            HandState.Select(FromCardState).ForEach(AddCardToHand);

            // Set the scale state
            LifeManager.Instance.SetNumWeightsImmediate(PlayerDamage, OpponentDamage);

            // Set the item state
            ItemsManager.Instance.SaveDataItemsList.Clear();
            ItemState.ForEach(x => ItemsManager.Instance.SaveDataItemsList.Add(x));
            ItemsManager.Instance.UpdateItems(true);

            // Set the energy state
            ResourcesManager.Instance.PlayerMaxEnergy = MaxEnergyState;
            ResourcesManager.Instance.PlayerEnergy = EnergyState;
            for (int i = 1; i <= 6; i++)
            {
                if (i < MaxEnergyState)
                    ForceCellOpen(i - 1);
                else
                    ForceCellClosed(i - 1);

                ResourceDrone.Instance.SetCellOn(i - 1, i < EnergyState, immediate: true);
            }

            // Set the bones state
            ResourcesManager.Instance.PlayerBones = BonesState;

            // Combat Phase state
            Traverse.Create(TurnManager.Instance.CombatPhaseManager).Field("<DamageDealtThisPhase>k__BackingField").SetValue(DamageDealtThisPhase);

            // Restore the decks
            CardDrawPiles3D.Instance.Deck.cards.Clear();
            CardDrawPiles3D.Instance.Deck.cards.AddRange(MainDeckState);
            CardDrawPiles3D.Instance.SideDeck.cards.Clear();
            CardDrawPiles3D.Instance.SideDeck.cards.AddRange(SideDeckState);
            CardDrawPiles3D.Instance.pile.DestroyCardsImmediate();
            CardDrawPiles3D.Instance.pile.CreateCards(MainDeckState.Count);
            CardDrawPiles3D.Instance.sidePile.DestroyCardsImmediate();
            CardDrawPiles3D.Instance.sidePile.CreateCards(SideDeckState.Count);

            // Restore colors
            ColorState.RestoreState();

            // Wait for everything to calm down
            if (restoredCallback != null)
                CustomCoroutine.WaitThenExecute(0.25f, restoredCallback);
        }

        private static void SetCardSlotPosition(GameObject obj, bool onBoard)
        {
            obj.transform.localPosition = new Vector3(
                obj.transform.localPosition.x,
                onBoard ? ONBOARD_SLOT_Y : OFFBOARD_SLOT_Y,
                obj.transform.localPosition.z
            );
        }

        private void ForceCellOpen(int cellIndex)
        {
            if (cellIndex >= ResourceDrone.Instance.cellsOpen)
                ResourceDrone.Instance.cellAnims[cellIndex].Play("open", 0, 1f);
        }

        private void ForceCellClosed(int cellIndex)
        {
            if (cellIndex < ResourceDrone.Instance.cellsOpen)
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

        public class LightColorState
        {
            public Color MainLightColor { get; private set; }
            public Color CardLightColor { get; private set; }
            public Color InteractablesColor { get; private set; }
            public Color SlotDefaultColor { get; private set; }
            public Color SlotInteractableColor { get; private set; }
            public Color SlotHighlightedColor { get; private set; }
            public Color QueueSlotDefaultColor { get; private set; }
            public Color QueueSlotInteractableColor { get; private set; }
            public Color QueueSlotHighlightedColor { get; private set; }

            public static LightColorState FromCurrent()
            {
                LightColorState state = new();
                state.MainLightColor = ExplorableAreaManager.Instance.hangingLight.color;
                state.CardLightColor = ExplorableAreaManager.Instance.hangingCardsLight.color;
                state.InteractablesColor = (BoardManager.Instance as BoardManager3D).Bell.currentHighlightedColor;
                state.SlotDefaultColor = BoardManager.Instance.AllSlots[0].currentDefaultColor;
                state.SlotInteractableColor = BoardManager.Instance.AllSlots[0].currentInteractableColor;
                state.SlotHighlightedColor = BoardManager.Instance.AllSlots[0].currentHighlightedColor;
                state.QueueSlotDefaultColor = BoardManager.Instance.OpponentQueueSlots[0].currentDefaultColor;
                state.QueueSlotInteractableColor = BoardManager.Instance.OpponentQueueSlots[0].currentInteractableColor;
                state.QueueSlotHighlightedColor = BoardManager.Instance.OpponentQueueSlots[0].currentHighlightedColor;
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

                return color;
            }

            private static Color GetSlotComplement(Color color)
            {
                Color retval = new Color(color.r, color.g, color.b);
                if (color == GameColors.Instance.blue)
                    retval = GameColors.Instance.limeGreen;
                if (color == GameColors.instance.purple)
                    retval = GameColors.instance.gray;
                if (color == GameColors.Instance.glowRed || color == GameColors.Instance.red)
                    retval = GameColors.instance.orange;
                if (color == GameColors.Instance.darkLimeGreen || color == GameColors.Instance.limeGreen)
                    retval = GameColors.Instance.brightBlue;

                retval.a = 0.5f;
                return retval;
            }

            public static LightColorState GetPreset(Color color)
            {
                LightColorState state = new();
                state.MainLightColor = state.SlotInteractableColor = state.QueueSlotInteractableColor = color;
                state.CardLightColor = Color.black;
                state.InteractablesColor = state.QueueSlotHighlightedColor = state.SlotHighlightedColor = GetHighlightComplement(color);
                state.QueueSlotDefaultColor = state.SlotDefaultColor = GetSlotComplement(color);
                return state;
            }

            public void RestoreState()
            {
                TableVisualEffectsManager.Instance.ChangeTableColors(
                    MainLightColor, CardLightColor, InteractablesColor, SlotDefaultColor, SlotInteractableColor, SlotHighlightedColor, QueueSlotDefaultColor, QueueSlotInteractableColor, QueueSlotHighlightedColor
                );
            }
        }
    }
}
