using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;
using InscryptionAPI.Guid;
using Infiniscryption.P03KayceeRun.Sequences;
using System.Collections;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;

namespace Infiniscryption.P03KayceeRun.Quests
{
    public abstract class QuestReward
    {
        public abstract IEnumerator GrantReward();
    }

    public class QuestRewardCoins : QuestReward
    {
        public virtual int Amount { get; set; }

        public override IEnumerator GrantReward()
        {
            if (this.Amount != 0)
            {
                P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
                View currentView = ViewManager.Instance.CurrentView;
                yield return new WaitForSeconds(0.4f);
                yield return P03AnimationController.Instance.ShowChangeCurrency(this.Amount, true);
                Part3SaveData.Data.currency += this.Amount;
                yield return new WaitForSeconds(0.2f);
                P03AnimationController.Instance.SwitchToFace(currentFace);
                yield return new WaitForSeconds(0.1f);
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }
    }

    public class QuestRewardDynamicCoins : QuestRewardCoins
    {
        public override int Amount 
        { 
            get 
            {
                if (base.Amount > 0)
                    return base.Amount;
                
                var range = EventManagement.CurrencyGainRange;
                return (range.Item1 + range.Item2) / 2;
            }
            set { base.Amount = value; }
        }
    }

    public class QuestRewardCard : QuestReward
    {
        public string CardName { get; set; }
        public string DialogueId { get; set; }

        protected virtual IEnumerator DoCardAction(SelectableCard card, CardInfo info)
        {
            Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName(this.CardName));

            if (String.IsNullOrEmpty(this.DialogueId))
                yield return new WaitForSeconds(1.5f);
            else
                yield return TextDisplayer.Instance.PlayDialogueEvent(this.DialogueId, TextDisplayer.MessageAdvanceMode.Input);
        }

        protected SelectableCard lastCreatedCard;

        protected IEnumerator DisplayCard(CardInfo cardInfo)
        {
            GameObject cardGO = UnityEngine.Object.Instantiate<GameObject>(SpecialNodeHandler.Instance.buildACardSequencer.selectableCardPrefab);
            lastCreatedCard = cardGO.GetComponent<SelectableCard>();
            
            lastCreatedCard.Initialize(cardInfo);
            lastCreatedCard.SetEnabled(false);
            lastCreatedCard.SetInteractionEnabled(false);
            lastCreatedCard.transform.position = new Vector3(-1f, 10f, 0.0f);

            Transform targetPosition = SpecialNodeHandler.Instance.buildACardSequencer.cardExamineMarker;
            Tween.Position(lastCreatedCard.transform, targetPosition.position, 0.25f, 0.0f, Tween.EaseOut);
            Tween.Rotation(lastCreatedCard.transform, targetPosition.rotation, 0.25f, 0.0f, Tween.EaseOut);
            yield return new WaitForSeconds(0.25f);
        }

        protected IEnumerator RemoveCard()
        {
            lastCreatedCard.ExitBoard(0.25f, Vector3.zero);
            yield return new WaitForSeconds(0.25f);
        }

        public override IEnumerator GrantReward()
        {
            if (String.IsNullOrEmpty(this.CardName))
                yield break;

            CardInfo cardInfo = CardLoader.GetCardByName(this.CardName);

            yield return this.DisplayCard(cardInfo);
            yield return this.DoCardAction(lastCreatedCard, cardInfo);
            yield return this.RemoveCard();
        }
    }

    public class QuestRewardLoseCard : QuestRewardCard
    {
        protected override IEnumerator DoCardAction(SelectableCard card, CardInfo info)
        {
            yield return new WaitForSeconds(0.75f);
            Part3SaveData.Data.deck.RemoveCardByName(this.CardName);
            card.Anim.PlayPermaDeathAnimation();
            yield return new WaitForSeconds(0.75f);
            yield break;
        }

        public override IEnumerator GrantReward()
        {
            if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == this.CardName))
                yield break;

            yield return base.GrantReward();
        }
    }

    public class QuestRewardItem : QuestReward
    {
        public string ItemName { get; set; }

        public override IEnumerator GrantReward()
        {
            if (!string.IsNullOrEmpty(this.ItemName))
            {
                if (Part3SaveData.Data.items.Count < P03AscensionSaveData.MaxNumberOfItems)
                {
                    View currentView = ViewManager.Instance.CurrentView;
                    yield return new WaitForEndOfFrame();
                    ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
                    yield return new WaitForSeconds(0.8f);
                    Part3SaveData.Data.items.Add(this.ItemName);
                    yield return new WaitForEndOfFrame();
                    ItemsManager.Instance.UpdateItems(false);
                    yield return new WaitForSeconds(1f);
                    ViewManager.Instance.SwitchToView(currentView, false, false);
                    yield return new WaitForSeconds(0.2f);
                }
            }
            yield break;
        }
    }

    public class QuestRewardLoseItem : QuestRewardItem
    {
        public override IEnumerator GrantReward()
        {
            if (!string.IsNullOrEmpty(this.ItemName))
            {
                if (Part3SaveData.Data.items.Contains(this.ItemName))
                {
                    ItemSlot slot = ItemsManager.Instance.Slots.First(s => s.Item != null && s.Item.name.Equals(this.ItemName));

                    View currentView = ViewManager.Instance.CurrentView;
                    yield return new WaitForEndOfFrame();
                    ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
                    yield return new WaitForSeconds(0.8f);
                    slot.Item.PlayExitAnimation();
                    yield return new WaitForSeconds(1f);
                    ItemsManager.Instance.RemoveItemFromSaveData(this.ItemName);
                    slot.DestroyItem();
                    ViewManager.Instance.SwitchToView(currentView, false, false);
                    yield return new WaitForSeconds(0.2f);
                }
            }
            yield break;
        }
    }

    public class QuestRewardModifyRandomCards : QuestRewardCard
    {
        public Ability Ability { get; set; } = Ability.NUM_ABILITIES;
        public bool Gemify { get; set; }
        public int AttackAdjustment { get; set; }
        public int HealthAdjustment { get; set; }
        public int NumberOfCards { get; set; }

        protected override IEnumerator DoCardAction(SelectableCard card, CardInfo info)
        {
            yield return new WaitForSeconds(1.5f);
            card.Anim.SetFaceDown(true, false);
            yield return new WaitForSeconds(0.2f);
            card.Anim.SetShaking(true);

            CardModificationInfo mod = new();
            mod.gemify = this.Gemify;
            if (this.Ability != Ability.NUM_ABILITIES)
                mod.abilities = new() { this.Ability };
            mod.attackAdjustment = this.AttackAdjustment;
            mod.healthAdjustment = this.HealthAdjustment;
            Part3SaveData.Data.deck.ModifyCard(info, mod);

            card.SetInfo(info);
            yield return new WaitForSeconds(0.45f);
            card.Anim.SetShaking(false);
            card.SetFaceDown(false, false);
            yield return new WaitForSeconds(1.5f);
        }

        public override IEnumerator GrantReward()
        {
            if (this.NumberOfCards > 0)
            {
                IEnumerable<CardInfo> cardQuery = Part3SaveData.Data.deck.Cards;
                if (this.Gemify)
                    cardQuery = cardQuery.Where(c => !c.Gemified);
                if (this.Ability != Ability.NUM_ABILITIES)
                    cardQuery = cardQuery.Where(c => !c.HasAbility(this.Ability));
                List<CardInfo> cards = cardQuery.ToList();
                
                int randomSeed = P03AscensionSaveData.RandomSeed;
                while (cards.Count > this.NumberOfCards)
                    cards.RemoveAt(SeededRandom.Range(0, cards.Count, randomSeed++));
                
                foreach (CardInfo card in cards)
                {
                    yield return this.DisplayCard(card);
                    yield return this.DoCardAction(lastCreatedCard, card);
                    yield return this.RemoveCard();
                }
            }
            yield break;
        }
    }

    public class QuestRewardAction : QuestReward
    {
        public Action RewardAction { get; set; }

        public override IEnumerator GrantReward()
        {
            if (this.RewardAction != null)
                this.RewardAction();
            yield break;
        }
    }
}