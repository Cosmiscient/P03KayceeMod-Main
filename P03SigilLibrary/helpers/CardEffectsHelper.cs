using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Helpers
{
    public static class CardEffectsHelper
    {
        public static int NonPassiveAttack(this PlayableCard card)
        {
            return card.Info.Attack + (card.TemporaryMods?.Where(m => !m.IsContinousEffectMod()).Select(m => m.attackAdjustment).Sum() ?? 0);
        }

        public static CardModificationInfo GetOrCreateSingletonTempMod(this PlayableCard card, string singletonId)
        {
            return card.TemporaryMods.FirstOrDefault(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(singletonId)) ?? new() { singletonId = singletonId };
        }

        public static bool IsContinousEffectMod(this CardModificationInfo mod)
        {
            if (string.IsNullOrEmpty(mod.singletonId))
                return false;

            if (AbilityIconBehaviours.DynamicAbilityCardModIds.Contains(mod.singletonId))
                return true;

            if (mod.singletonId.StartsWith("CardWith"))
                return true;

            if (mod.singletonId.StartsWith("ConduitGainAbility"))
                return true;

            if (mod.singletonId.StartsWith("VARIABLE_STAT"))
                return true;

            return false;
        }

        public static IEnumerator CardChooseSlotSequence(this AbilityBehaviour behaviour, Func<CardSlot, IEnumerator> slotSelectedCallback, List<CardSlot> validSlots, Func<CardSlot, int> aiSlotEvaluator = null, string dialogue = null, bool aimWeapon = false, CursorType cursor = CursorType.Target, bool tweenOut = true, bool tweenIn = true)
        {
            PlayableCard card = behaviour.Card;
            Vector3 originalPosition = card.transform.position;

            bool shouldAimWeapon = aimWeapon && BoardManager.Instance is BoardManager3D && behaviour.Card.StatsLayer is DiskRenderStatsLayer;
            DiskCardAnimationController dcac = behaviour.Card.Anim as DiskCardAnimationController;

            if (shouldAimWeapon)
            {
                dcac.AimWeaponAnim(behaviour.Card.OpposingSlot().transform.position);
                dcac.ShowWeaponAnim();
            }
            else if (BoardManager.Instance is BoardManager3D && tweenOut)
            {
                ViewManager.Instance.SwitchToView(View.Board, false, true);
                yield return new WaitForSeconds(0.25f);

                Vector3 a = card.Slot.IsPlayerSlot ? Vector3.forward * .2f : Vector3.back * 0.2f;
                Tween.Position(card.transform, card.transform.position + (a * 2f) + (Vector3.up * 0.15f), 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            }

            if (!string.IsNullOrEmpty(dialogue))
            {
                yield return TextDisplayer.Instance.ShowUntilInput(dialogue);
            }

            CardSlot selectedSlot = null;
            if (card.IsPlayerCard())
            {
                yield return BoardManager.Instance.ChooseTarget(
                    validSlots,
                    validSlots,
                    s => selectedSlot = s,
                    s => card?.Anim.StrongNegationEffect(),
                    !shouldAimWeapon ? null : s => dcac.AimWeaponAnim(s.transform.position),
                    () => false,
                    cursor
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
                if (shouldAimWeapon)
                {
                    dcac.AimWeaponAnim(selectedSlot.transform.position);
                    bool impactReached = false;
                    dcac.PlayAttackAnimation(false, selectedSlot, () => impactReached = true);
                    yield return new WaitUntil(() => impactReached);
                }
                yield return slotSelectedCallback(selectedSlot);
            }

            if (shouldAimWeapon)
            {
                dcac.HideWeaponAnim();
            }
            else if (BoardManager.Instance is BoardManager3D && tweenIn)
            {
                Tween.Position(card.transform, originalPosition, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
                yield return new WaitForSeconds(0.15f);
            }

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            yield break;
        }
    }
}