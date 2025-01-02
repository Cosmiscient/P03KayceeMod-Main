using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ActivatedFutureSight : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int EnergyCost => 1;

        public override bool CanActivate() => CardDrawPiles.Instance.Deck.cards.Count > 0;

        static ActivatedFutureSight()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Future Sight";
            info.rulebookDescription = "Look at the top card of your deck. You can play it.";
            info.canStack = false;
            info.powerLevel = 4;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedFutureSight),
                TextureHelper.GetImageAsTexture("ability_activated_future_sight.png", typeof(ActivatedFutureSight).Assembly)
            ).Id;
        }

        private static readonly ConditionalWeakTable<Deck, CardInfo> _topCardTable = new();

        private static CardInfo GetAndCacheTopCardOfDeck()
        {
            var cached = CachedTopCardOfDeck;
            if (cached != null)
                return cached;

            if (CardDrawPiles.Instance == null)
                return null;

            List<CardInfo> list = CardDrawPiles.Instance.Deck.cards;
            if (list.Count == 0)
                return null;

            var cardInfo = list[SeededRandom.Range(0, list.Count, CardDrawPiles.Instance.Deck.randomSeed)];
            CachedTopCardOfDeck = cardInfo;
            return cardInfo;
        }

        private static CardInfo CachedTopCardOfDeck
        {
            get
            {
                if (CardDrawPiles.Instance == null)
                    return null;

                List<CardInfo> list = CardDrawPiles.Instance.Deck.cards;
                if (list.Count == 0)
                    return null;

                if (_topCardTable.TryGetValue(CardDrawPiles.Instance.Deck, out CardInfo info))
                {
                    if (list.Contains(info))
                    {
                        return info;
                    }
                    else
                    {
                        CachedTopCardOfDeck = null;
                        return null;
                    }
                }

                return null;
            }
            set
            {
                if (CardDrawPiles.Instance == null)
                    return;

                _topCardTable.Remove(CardDrawPiles.Instance.Deck);

                if (value != null)
                    _topCardTable.Add(CardDrawPiles.Instance.Deck, value);
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        private static void CleanUpTopOfDeck()
        {
            CachedTopCardOfDeck = null;
        }

        [HarmonyPatch(typeof(Deck), nameof(Deck.Draw), new[] { typeof(CardInfo) })]
        [HarmonyPrefix]
        private static bool DrawKnownTopCard(Deck __instance, CardInfo specificCard, ref CardInfo __result)
        {
            if (__instance != CardDrawPiles.Instance.Deck)
                return true;

            if (specificCard != null && __instance.cards.Contains(specificCard))
            {
                CachedTopCardOfDeck = null;
                return true;
            }

            if (CachedTopCardOfDeck != null)
            {
                __result = CachedTopCardOfDeck;
                CachedTopCardOfDeck = null;
                __instance.cards.Remove(__result);
                return false;
            }

            return true;
        }

        private static PlayableCard CurrentDisplayedCard = null;
        private static bool PlayerClickedCard = false;

        [HarmonyPatch(typeof(ViewController), nameof(ViewController.ManagedLateUpdate))]
        [HarmonyPrefix]
        private static bool SwitchViewsWhenLookingAtFutureCard(ViewController __instance)
        {
            if (CurrentDisplayedCard == null)
                return true;

            if (ViewManager.Instance.CurrentView != CardDrawPiles3D.Instance.pilesView)
                return true;

            if (__instance.LockState != ViewLockState.Locked)
            {
                if (InputButtons.GetButtonRepeating(Button.LookDown) || InputButtons.GetButtonRepeating(Button.LookLeft))
                {
                    ViewManager.Instance.SwitchToView(View.Default);
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.PlayCardOnSlot))]
        [HarmonyPostfix]
        private static IEnumerator PlayFutureCard(IEnumerator sequence, PlayableCard card, CardSlot slot)
        {
            if (card != CurrentDisplayedCard)
            {
                yield return sequence;
                yield break;
            }

            CardDrawPiles3D.Instance.Pile.Draw();
            CardDrawPiles3D.Instance.Deck.cards.Remove(CachedTopCardOfDeck);
            CachedTopCardOfDeck = null;

            if (card.TriggerHandler.RespondsToTrigger(Trigger.PlayFromHand))
                yield return card.TriggerHandler.OnTrigger(Trigger.PlayFromHand);

            yield return BoardManager.Instance.ResolveCardOnBoard(card, slot);

        }

        [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.OnCardSelected))]
        [HarmonyPrefix]
        private static bool SpecialPlayerCard(PlayerHand __instance, PlayableCard card)
        {
            if (CurrentDisplayedCard == null)
                return true;

            if (card != CurrentDisplayedCard)
                return false;

            if (!__instance.AllowCardInHandSelection(card))
                return false;

            // Mark the card as now having been selected
            PlayerClickedCard = true;
            return false;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ShowCardNearBoard))]
        [HarmonyPrefix]
        private static bool ShowFutureCard(BoardManager __instance, PlayableCard card, bool showNearBoard)
        {
            if (card != CurrentDisplayedCard || showNearBoard)
                return true;

            card.SetEnabled(false);
            Tween.LocalPosition(card.transform, card.transform.localPosition + (3f * Vector3.down), .2f, 0f, completeCallback: () => GameObject.Destroy(card.gameObject));

            return false;
        }

        public override IEnumerator Activate()
        {
            CardInfo info = GetAndCacheTopCardOfDeck();

            if (info == null)
                yield break;

            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            ViewManager.Instance.SwitchToView(CardDrawPiles3D.Instance.pilesView);
            yield return new WaitForSeconds(0.2f);

            PlayableCard card = CardSpawner.SpawnPlayableCard(CardLoader.Clone(info));

            card.transform.position = new(3.1f, 6.11f, -3.7f);
            card.transform.eulerAngles = new(30f, 22f, 8f);

            if (card.TriggerHandler.RespondsToTrigger(Trigger.Drawn))
                yield return Card.TriggerHandler.OnTrigger(Trigger.Drawn);

            if (!card.CanPlay())
            {
                HintsHandler.OnNonplayableCardClicked(card, PlayerHand.Instance.CardsInHand);
                yield return new WaitForSeconds(2.0f);
                ViewManager.Instance.SwitchToView(View.Default);
            }
            else
            {
                CurrentDisplayedCard = card;
                PlayerClickedCard = false;
                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                yield return new WaitUntil(() => ViewManager.Instance.CurrentView != CardDrawPiles3D.Instance.pilesView || PlayerClickedCard);

                if (PlayerClickedCard)
                {
                    yield return PlayerHand.Instance.SelectSlotForCard(card);
                    CurrentDisplayedCard = null;
                    yield break;
                }
            }
            Tween.LocalPosition(card.transform, card.transform.localPosition + (3f * Vector3.down), .2f, 0f, completeCallback: () => GameObject.Destroy(card));
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            yield break;
        }
    }
}
