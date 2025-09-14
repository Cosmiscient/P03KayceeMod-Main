using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class RotateBoardSequencePatch
    {
        private static CardSlot GetClockwiseSlot(CardSlot slot)
        {
            return slot.IsPlayerSlot
                ? slot.Index == 0 ? BoardManager.Instance.opponentSlots[0] : BoardManager.Instance.playerSlots[slot.Index - 1]
                : slot.Index + 1 >= BoardManager.Instance.opponentSlots.Count
                ? BoardManager.Instance.playerSlots[BoardManager.Instance.playerSlots.Count - 1]
                : BoardManager.Instance.opponentSlots[slot.Index + 1];
        }

        private static CardSlot GetDestinationForSlot(CardSlot slot, List<CardSlot> playerSlots, List<CardSlot> opponentSlots, bool nullOnly = false)
        {
            CardSlot currentInvestigatedSlot = GetClockwiseSlot(slot);
            while (currentInvestigatedSlot != slot)
            {
                if (currentInvestigatedSlot.Card == null || (!nullOnly && !currentInvestigatedSlot.Card.Info.HasTrait(CustomCards.Unrotateable)))
                    return currentInvestigatedSlot;

                currentInvestigatedSlot = GetClockwiseSlot(currentInvestigatedSlot);
            }
            return slot;
        }

        private static IEnumerator ManageAssignment(PlayableCard card, CardSlot slot)
        {
            card.SetIsOpponentCard(!slot.IsPlayerSlot);
            // We no longer trigger right away
            yield return BoardManager.Instance.AssignCardToSlot(card, slot, 0.1f, null, false);
            ResourcesManager.Instance.ForceGemsUpdate();
            if (card.FaceDown)
            {
                bool flag = slot.Index == 0 && !slot.IsPlayerSlot;
                bool flag2 = slot.Index == BoardManager.Instance.GetSlots(false).Count - 1 && slot.IsPlayerSlot;
                if (flag || flag2)
                {
                    card.SetFaceDown(false, false);
                    card.UpdateFaceUpOnBoardEffects();
                }
            }
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.MoveAllCardsClockwise))]
        [HarmonyPostfix]
        private static IEnumerator MoveAllCardsClockwiseAccountForUnrotateable(IEnumerator sequence)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            Dictionary<PlayableCard, CardSlot> cardDestinations = new();
            Dictionary<PlayableCard, CardSlot> originalLocations = new();
            List<CardSlot> playerSlots = BoardManager.Instance.GetSlots(true);
            List<CardSlot> opponentSlots = BoardManager.Instance.GetSlots(false);


            foreach (CardSlot slot in BoardManager.Instance.AllSlots)
            {
                if (slot.Card != null && !slot.Card.Info.HasTrait(CustomCards.Unrotateable))
                {
                    cardDestinations.Add(slot.Card, GetDestinationForSlot(slot, playerSlots, opponentSlots));
                    originalLocations.Add(slot.Card, slot);
                }
            }

            foreach (CardSlot cardSlot in BoardManager.Instance.AllSlots)
            {
                if (cardSlot.Card != null && !cardSlot.Card.Info.HasTrait(CustomCards.Unrotateable))
                {
                    cardSlot.Card.Slot = null;
                    cardSlot.Card = null;
                }
            }
            foreach (KeyValuePair<PlayableCard, CardSlot> assignment in cardDestinations)
            {
                yield return ManageAssignment(assignment.Key, assignment.Value);
            }
            foreach (KeyValuePair<PlayableCard, CardSlot> orig in originalLocations)
            {
                // Let's see if this card *doesn't have a slot now*
                if (!orig.Key.SafeIsUnityNull() && !orig.Key.Dead && (orig.Key.Slot == null || orig.Key.Slot.Card != orig.Key))
                {
                    // Crap we gotta find a new home for this card
                    var newSlot = GetDestinationForSlot(orig.Value, playerSlots, opponentSlots, nullOnly: true);
                    if (newSlot == orig.Value) // Crap there's no place for this card...
                    {
                        orig.Key.ExitBoard(0, Vector3.down * 10);
                    }
                    else
                    {
                        yield return ManageAssignment(orig.Key, newSlot);
                    }
                }
            }

            foreach (var slot in BoardManager.Instance.PlayerSlotsCopy.Reverse<CardSlot>().Concat(BoardManager.Instance.OpponentSlotsCopy))
            {
                if (ShouldUpdate(slot))
                {
                    slot.Card.UpdateFaceUpOnBoardEffects();
                    yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.OtherCardAssignedToSlot, false, slot.Card);
                }
            }

            ResourcesManager.Instance.ForceGemsUpdate();

            yield break;
        }

        private static bool ShouldUpdate(CardSlot slot)
        {
            try
            {
                if (slot == null)
                    return false;

                if (slot.Card == null)
                    return false;

                if (slot.Card.SafeIsUnityNull())
                    return false;

                if (slot.Card.Anim.SafeIsUnityNull())
                    return false;

                if (slot.Card.Dead)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}