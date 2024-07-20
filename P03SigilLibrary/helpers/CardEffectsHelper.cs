using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Helpers
{
    public static class CardEffectsHelper
    {
        public static IEnumerator CardChooseSlotSequence(this AbilityBehaviour behaviour, Func<CardSlot, IEnumerator> slotSelectedCallback, List<CardSlot> validSlots, Func<CardSlot, int> aiSlotEvaluator = null)
        {
            PlayableCard card = behaviour.Card;
            Vector3 originalPosition = card.transform.position;

            if (BoardManager.Instance is BoardManager3D)
            {
                ViewManager.Instance.SwitchToView(View.Board, false, true);
                yield return new WaitForSeconds(0.25f);

                Vector3 a = card.Slot.IsPlayerSlot ? Vector3.forward * .5f : Vector3.back * 0.5f;
                Tween.Position(card.transform, card.transform.position + (a * 2f) + (Vector3.up * 0.25f), 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            }

            CardSlot selectedSlot = null;
            if (card.IsPlayerCard())
            {
                yield return BoardManager.Instance.ChooseTarget(
                    validSlots,
                    validSlots,
                    s => selectedSlot = s,
                    s => card?.Anim.StrongNegationEffect(),
                    null,
                    () => false,
                    CursorType.Target
                );
            }
            else
            {
                int randomSeed = P03SigilLibraryPlugin.RandomSeed;
                Func<CardSlot, int> randomEvaluator = delegate (CardSlot slot)
                {
                    return Mathf.CeilToInt(SeededRandom.Value(randomSeed++) * 10000);
                };
                var evaluator = aiSlotEvaluator ?? randomEvaluator;
                selectedSlot = validSlots.OrderBy(s => evaluator(s)).First();

                yield return new WaitForSeconds(0.3f);
            }

            if (selectedSlot != null)
            {
                yield return slotSelectedCallback(selectedSlot);
            }

            if (BoardManager.Instance is BoardManager3D)
            {
                Tween.Position(card.transform, originalPosition, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
                yield return new WaitForSeconds(0.15f);
            }

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            yield break;
        }
    }
}