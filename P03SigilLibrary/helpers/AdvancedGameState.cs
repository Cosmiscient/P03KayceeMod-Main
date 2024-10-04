using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Sigils;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Helpers
{
    [HarmonyPatch]
    public class AdvancedGameState
    {
        public static List<ConsumableItemSlot> ConsumableSlots => ItemsManager.Instance.consumableSlots.Where(s => s is not HammerItemSlot).ToList();

        static AdvancedGameState()
        {
            AdvancedGameState.StateRestored += (state) => SapphirePower.UpdateCount();
        }

        public static event Action<AdvancedGameState> StateRestored;

        public static float OFFBOARD_SLOT_Y { get; private set; } = -15f;
        public static float ONBOARD_SLOT_Y { get; private set; } = -15f;

        public List<PlayableCardState> PlayerSlots { get; private set; }

        public List<PlayableCardState> OpponentSlots { get; private set; }

        public List<PlayableCardState> OpponentQueue { get; private set; }

        public int OpponentDamage { get; internal set; }

        public int PlayerDamage { get; internal set; }

        public int LifeBalance => OpponentDamage - PlayerDamage;

        public bool GameIsActive => Mathf.Abs(LifeBalance) < 5;

        public List<List<CardInfo>> OpponentTurnPlan { get; private set; }

        public List<PlayableCardState> HandState { get; private set; }

        public List<CardInfo> MainDeckState { get; set; }

        public List<CardInfo> SideDeckState { get; set; }

        public List<string> ItemState { get; private set; }

        public int BonesState { get; private set; }

        public int MaxEnergyState { get; private set; }

        public int EnergyState { get; private set; }

        public bool PlayerRungBell { get; internal set; }

        public bool PlayerNeedsUpkeepStep { get; internal set; }

        public bool HasEverHadUpkeepStep { get; internal set; }

        public int OpponentNumTurnsTaken { get; private set; }

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

        public static AdvancedGameState GenerateFromCurrentState()
        {
            AdvancedGameState state = new();
            P03SigilLibraryPlugin.Log.LogInfo($"Saving resources {ResourcesManager.Instance.PlayerEnergy}/{ResourcesManager.Instance.PlayerEnergy} Energy. {ResourcesManager.Instance.PlayerBones} Bones.");
            state.EnergyState = ResourcesManager.Instance.PlayerEnergy;
            state.MaxEnergyState = ResourcesManager.Instance.PlayerMaxEnergy;
            state.BonesState = ResourcesManager.Instance.PlayerBones;

            var newItems = ItemsManager.Instance.Consumables.Select(i => i.Data.name).Where(s => !s.Equals("hammer", StringComparison.InvariantCultureIgnoreCase)).ToList();
            P03SigilLibraryPlugin.Log.LogInfo($"Saving Items: " + string.Join(", ", newItems));
            state.ItemState = newItems;

            P03SigilLibraryPlugin.Log.LogInfo($"Saving Cards In Hand: " + string.Join(", ", PlayerHand.Instance.CardsInHand.Select(c => c.name)));
            state.HandState = PlayerHand.Instance.CardsInHand.Select(pc => new PlayableCardState(pc)).ToList();


            P03SigilLibraryPlugin.Log.LogInfo($"Saving Cards In Main Deck: " + string.Join(", ", CardDrawPiles3D.Instance.Deck.cards.Select(c => c.name)));
            state.MainDeckState = new(CardDrawPiles3D.Instance.Deck.cards);

            P03SigilLibraryPlugin.Log.LogInfo($"Saving Cards In Side Deck: " + string.Join(", ", CardDrawPiles3D.Instance.SideDeck.cards.Select(c => c.name)));
            state.SideDeckState = new(CardDrawPiles3D.Instance.SideDeck.cards);

            P03SigilLibraryPlugin.Log.LogInfo($"Copying turn plan");
            state.OpponentTurnPlan = new(TurnManager.Instance.Opponent.TurnPlan.Select(lci => new List<CardInfo>(lci)));

            P03SigilLibraryPlugin.Log.LogInfo($"Saving Opponent Queue: " + string.Join(", ", TurnManager.Instance.Opponent.Queue.Select(c => c.name)));
            state.OpponentQueue = TurnManager.Instance.Opponent.Queue.Select(pc => pc == null ? null : new PlayableCardState(pc)).ToList();

            P03SigilLibraryPlugin.Log.LogInfo($"Saving Slots");
            state.PlayerSlots = BoardManager.Instance.playerSlots.Select(s => s.Card == null ? null : new PlayableCardState(s.Card)).ToList();
            state.OpponentSlots = BoardManager.Instance.opponentSlots.Select(s => s.Card == null ? null : new PlayableCardState(s.Card)).ToList();

            P03SigilLibraryPlugin.Log.LogInfo($"Saving damage state");
            state.PlayerDamage = LifeManager.Instance.PlayerDamage;
            state.OpponentDamage = LifeManager.Instance.OpponentDamage;

            state.OpponentNumTurnsTaken = TurnManager.Instance.opponent.NumTurnsTaken;

            return state;
        }

        public IEnumerator RestoreState(Action restoredCallback = null)
        {
            ViewManager.Instance.SwitchToView(View.Default);

            // Restore all cards
            var opSlots = BoardManager.Instance.OpponentSlotsCopy;
            var plSlots = BoardManager.Instance.PlayerSlotsCopy;
            for (int i = 0; i < opSlots.Count; i++)
            {
                if (opSlots[i].Card != null)
                {
                    opSlots[i].Card.UnassignFromSlot();
                    opSlots[i].Card.transform.localPosition += Vector3.down * 10f;
                    opSlots[i].Card.StartCoroutine(opSlots[i].Card.DestroyWhenStackIsClear());
                }
                if (plSlots[i].Card != null)
                {
                    plSlots[i].Card.UnassignFromSlot();
                    plSlots[i].Card.transform.localPosition += Vector3.down * 10f;
                    plSlots[i].Card.StartCoroutine(plSlots[i].Card.DestroyWhenStackIsClear());
                }

                if (this.PlayerSlots[i] != null)
                {
                    var pCard = this.PlayerSlots[i].Reform();
                    pCard.OpponentCard = false;
                    yield return BoardManager.Instance.AssignCardToSlot(pCard, plSlots[i], resolveTriggers: false);
                }

                if (this.OpponentSlots[i] != null)
                {
                    var oCard = this.OpponentSlots[i].Reform();
                    oCard.OpponentCard = true;
                    yield return BoardManager.Instance.AssignCardToSlot(oCard, opSlots[i], resolveTriggers: false);
                }
            }

            foreach (var card in TurnManager.Instance.Opponent.queuedCards)
            {
                GameObject.DestroyImmediate(card);
            }
            TurnManager.Instance.Opponent.Queue.Clear();

            for (int i = 0; i < this.OpponentQueue.Count; i++)
            {
                if (this.OpponentQueue[i] != null)
                {
                    var pCard = this.OpponentQueue[i].Reform();
                    pCard.OpponentCard = true;
                    BoardManager.Instance.QueueCardForSlot(pCard, opSlots[i]);
                }
            }

            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: Resetting opponent turn plan");
            TurnManager.Instance.Opponent.TurnPlan = new(OpponentTurnPlan.Select(lci => new List<CardInfo>(lci)));

            // Restore the player's hand
            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: Resetting player hand");

            yield return PlayerHand.Instance.CleanUp();
            foreach (var card in this.HandState)
            {
                var hCard = card.Reform();
                PlayerHand.Instance.ParentCardToHand(hCard, CardSpawner.Instance.spawnedPositionOffset);
                PlayerHand.Instance.cardsInHand.Add(hCard);
                PlayerHand.Instance.OnCardInspected(hCard);
                PlayerHand.Instance.SetCardPositions();
                yield return new WaitForEndOfFrame();
            }

            if (PlayerHand.Instance is PlayerHand3D ph3d)
                ph3d.aboveHandCards.Clear();

            if (PlayerHand.Instance.CardsInHand.Count > 0)
                PlayerHand.Instance.OnCardInspected(PlayerHand.Instance.CardsInHand[0]);

            // Set the scale state
            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: Resetting scale");
            LifeManager.Instance.SetNumWeightsImmediate(PlayerDamage, OpponentDamage);

            // Set the item state
            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: Resetting items to " + string.Join(",", ItemState));
            UpdateItems(ItemState);

            // Set the energy state
            P03SigilLibraryPlugin.Log.LogInfo("Restoring multiverse state: Resetting energy");
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
            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: Resetting bones");
            ResourcesManager.Instance.PlayerBones = BonesState;

            // Combat Phase state
            // Restore the decks
            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: restoring main deck");
            CardDrawPiles3D.Instance.Deck.cards.Clear();
            CardDrawPiles3D.Instance.Deck.cards.AddRange(MainDeckState);

            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: restoring side deck");
            CardDrawPiles3D.Instance.SideDeck.cards.Clear();
            CardDrawPiles3D.Instance.SideDeck.cards.AddRange(SideDeckState);

            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: restoring main pile");
            CardDrawPiles3D.Instance.pile.DestroyCardsImmediate();
            CardDrawPiles3D.Instance.pile.CreateCards(MainDeckState.Count);
            CardDrawPiles3D.Instance.pile.SetEnabled(true);

            P03SigilLibraryPlugin.Log.LogInfo("Restoring state: restoring side pile");
            CardDrawPiles3D.Instance.sidePile.DestroyCardsImmediate();
            CardDrawPiles3D.Instance.sidePile.CreateCards(SideDeckState.Count);
            CardDrawPiles3D.Instance.sidePile.SetEnabled(true);

            TurnManager.Instance.opponent.NumTurnsTaken = OpponentNumTurnsTaken;

            // Restore gems
            ResourcesManager.Instance.ForceGemsUpdate();

            yield return new WaitForSeconds(0.25f);

            // Fire event
            AdvancedGameState.StateRestored?.Invoke(this);

            restoredCallback?.Invoke();
        }

        internal static void UpdateItems(List<string> itemContents)
        {
            ItemsManager.Instance.SaveDataItemsList.Clear();
            ItemsManager.Instance.SaveDataItemsList.AddRange(itemContents);

            var slots = AdvancedGameState.ConsumableSlots;

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

        public class PlayableCardState
        {
            public CardInfo BaseCardInfo { get; private set; }
            public PlayableCardStatus Status { get; private set; }
            public List<CardModificationInfo> TemporaryMods { get; private set; }

            public PlayableCardState(PlayableCard card)
            {
                BaseCardInfo = AdvancedGameState.FullClone(card.Info);
                TemporaryMods = card.TemporaryMods.Select(m => (CardModificationInfo)m.Clone()).ToList();
                Status = new()
                {
                    anglerHooked = card.Status.anglerHooked,
                    damageTaken = card.Status.damageTaken,
                    hiddenAbilities = new(card.Status.hiddenAbilities),
                    lostShield = card.Status.lostShield,
                    lostTail = card.Status.lostTail,
                };
            }

            public PlayableCard Reform()
            {
                PlayableCard card = CardSpawner.SpawnPlayableCard(this.BaseCardInfo);
                card.Status = this.Status;
                foreach (var m in this.TemporaryMods)
                    card.AddTemporaryMod(m);
                card.RenderCard();
                return card;
            }
        }
    }
}
