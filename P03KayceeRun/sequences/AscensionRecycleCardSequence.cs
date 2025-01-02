using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class AscensionRecycleCardSequence : RecycleCardSequencer
    {
        public static AscensionRecycleCardSequence Instance { get; private set; }

        public static bool ShouldOverrideCardDisplayer { get; private set; }

        [HarmonyPatch(typeof(MenuController), nameof(MenuController.TransitionToAscensionMenu))]
        [HarmonyPrefix]
        private static void EnsureOverrideDisplayerResets() => ShouldOverrideCardDisplayer = false;

        public AscensionRecycleCardSequence()
        {
            cardArray = SpecialNodeHandler.Instance.recycleCardSequencer.cardArray;
            deckPile = SpecialNodeHandler.Instance.recycleCardSequencer.deckPile;
            recycleMachine = SpecialNodeHandler.Instance.recycleCardSequencer.recycleMachine;
        }

        private GameObject _selectableCardPrefab;
        private GameObject SelectableCardPrefab
        {
            get
            {
                if (_selectableCardPrefab == null)
                    _selectableCardPrefab = SpecialNodeHandler.Instance.buildACardSequencer.selectableCardPrefab;

                return _selectableCardPrefab;
            }
        }

        private Transform _cardExamineMarker;
        private Transform CardExamineMarker
        {
            get
            {
                if (_cardExamineMarker == null)
                    _cardExamineMarker = SpecialNodeHandler.Instance.buildACardSequencer.cardExamineMarker;

                return _cardExamineMarker;
            }
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool HandleAscensionItems(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            if (nodeData is AscensionRecycleCardNodeData)
            {
                if (Instance == null)
                    Instance = __instance.gameObject.AddComponent<AscensionRecycleCardSequence>();

                SpecialNodeHandler.Instance.StartCoroutine(Instance.RecycleCardForDraftTokenSequence());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RecycleMachine), nameof(RecycleMachine.DisplayCard))]
        [HarmonyPostfix]
        public static void DisplayTokenString(ref RecycleMachine __instance, CardInfo card, int currencyValue, string format)
        {
            if (ShouldOverrideCardDisplayer)
            {
                __instance.currencyText.text = card.metaCategories.Contains(CardMetaCategory.Rare)
                    ? card.ModAbilities.Count > 0 ? "RARE++" : "RARE"
                    : card.Gemified || card.ModAbilities.Count > 0 ? "TKN++" : "TKN";
            }
        }

        [HarmonyPatch(typeof(RecycleCardSequencer), nameof(GetCardStatPointsValue))]
        [HarmonyPrefix]
        public static bool GetSPForAscension(CardInfo info, ref int __result)
        {
            if (ShouldOverrideCardDisplayer)
            {
                __result = info.metaCategories.Contains(CardMetaCategory.Rare)
                    ? info.ModAbilities.Count > 0 ? 4 : 3
                    : info.Gemified || info.mods.Count > 0 ? 2 : 1;
                return false;
            }
            return true;
        }

        private CardInfo GetCardInfo()
        {
            if (selectedCardInfo.HasCardMetaCategory(CardMetaCategory.Rare))
            {
                CardInfo rareCard = CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN);
                if (selectedCardInfo.ModAbilities.Count > 0)
                {
                    CardModificationInfo cardMod = new()
                    {
                        abilities = new List<Ability>(selectedCardInfo.ModAbilities)
                    };
                    rareCard.mods.Add(cardMod);
                }
                return rareCard;
            }

            CardInfo baseCard = CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN);

            List<Ability> abilities = selectedCardInfo.Abilities;
            if (abilities.Count > 0)
            {
                CardModificationInfo cardMod = new()
                {
                    abilities = new List<Ability>(abilities),
                    energyCostAdjustment = abilities.Count
                };
                baseCard.mods.Add(cardMod);
            }

            if (selectedCardInfo.GetStartingFuel() > 0)
                baseCard.SetStartingFuel(selectedCardInfo.GetStartingFuel());

            return baseCard;
        }

        public IEnumerator RecycleCardForDraftTokenSequence()
        {
            ShouldOverrideCardDisplayer = true;
            cardValueMode = CardValueMode.StatPoints;

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03AscensionRecycle", TextDisplayer.MessageAdvanceMode.Input);
            yield return InitializeRecycleMachine();
            yield return new WaitForSeconds(1.25f);
            yield return SelectAndRecycleCard();
            recycleMachine.OpenHatch();
            yield return new WaitForSeconds(0.5f);

            GameObject cardGO = Instantiate(SelectableCardPrefab);
            SelectableCard card = cardGO.GetComponent<SelectableCard>();

            CardInfo cardInfo = GetCardInfo();

            card.Initialize(cardInfo);
            card.SetEnabled(false);
            card.SetInteractionEnabled(false);
            card.transform.position = new Vector3(-1f, 10f, 0.0f);
            Tween.Position(card.transform, CardExamineMarker.position, 0.25f, 0.0f, Tween.EaseOut);
            Tween.Rotation(card.transform, CardExamineMarker.rotation, 0.25f, 0.0f, Tween.EaseOut);

            yield return TextDisplayer.Instance.PlayDialogueEvent($"P03AscensionToken{selectedCardValue}", TextDisplayer.MessageAdvanceMode.Input);
            card.ExitBoard(0.25f, Vector3.zero);

            Part3SaveData.Data.deck.AddCard(cardInfo);

            yield return new WaitForSeconds(0.25f);
            recycleMachine.CloseHatch();
            yield return new WaitForSeconds(0.4f);
            yield return CleanupRecycleMachine();

            yield return new WaitForSeconds(0.6f);

            ShouldOverrideCardDisplayer = false;

            GameFlowManager.Instance?.TransitionToGameState(GameState.Map);
        }
    }
}