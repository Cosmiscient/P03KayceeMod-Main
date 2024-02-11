using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public abstract class MultiverseLatchBase : Latch
    {
        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            if (MultiverseBattleSequencer.Instance == null)
            {
                yield return base.OnPreDeathAnimation(wasSacrifice);
                yield break;
            }

            int originalUniverse = MultiverseBattleSequencer.Instance.GetUniverseId(Card.slot);
            InteractionCursor.Instance.ForceCursorType(CursorType.Target);
            List<CardSlot> validTargets = MultiverseBattleSequencer.Instance.AllSlotsCopy;
            validTargets.RemoveAll((CardSlot x) => x.Card == null || x.Card.Dead || this.CardHasLatchMod(x.Card) || x.Card == base.Card);
            P03Plugin.Log.LogInfo($"There are {validTargets.Count} valid targets for latch");
            if (validTargets.Count > 0)
            {
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                base.Card.Anim.PlayHitAnimation();
                yield return new WaitForSeconds(0.1f);
                DiskCardAnimationController cardAnim = Card.Anim as DiskCardAnimationController;
                GameObject claw = GameObject.Instantiate<GameObject>(this.clawPrefab, cardAnim.WeaponParent.transform);
                CardSlot selectedSlot = null;

                if (Card.OpponentCard)
                {
                    yield return new WaitForSeconds(0.3f);
                    yield return this.AISelectTarget(validTargets, delegate (CardSlot s)
                    {
                        selectedSlot = s;
                    });

                    if (selectedSlot != null && selectedSlot.Card != null)
                    {
                        int targetUniverseId = MultiverseBattleSequencer.Instance.GetUniverseId(selectedSlot);
                        if (targetUniverseId != originalUniverse)
                        {
                            yield return MultiverseBattleSequencer.Instance.TravelToUniverse(targetUniverseId);
                            MultiverseBattleSequencer.Instance.ParentCardToTvScreen(Card, 0.2f);
                            yield return new WaitForSeconds(0.6f);
                        }

                        cardAnim.AimWeaponAnim(selectedSlot.transform.position);
                        yield return new WaitForSeconds(0.3f);

                        if (targetUniverseId != originalUniverse)
                        {
                            MultiverseBattleSequencer.Instance.ParentCardToTvScreen(Card, 0.2f, forceRestore: true);
                            yield return MultiverseBattleSequencer.Instance.TravelToUniverse(originalUniverse);
                        }
                    }
                }
                else
                {
                    yield return MultiverseBattleSequencer.Instance.ChooseSlotFromMultiverse(
                        (slot) => slot.Card != null && slot.Card != this.Card && !CardHasLatchMod(slot.Card),
                        delegate ()
                        {
                            MultiverseBattleSequencer.Instance.ParentCardToTvScreen(Card, 0f);
                        },
                        delegate (CardSlot slot)
                        {
                            if (slot.Card != null)
                            {
                                cardAnim.AimWeaponAnim(slot.transform.position);
                            }
                        },
                        null
                    );
                }
                CustomCoroutine.FlickerSequence(delegate
                {
                    claw.SetActive(true);
                }, delegate
                {
                    claw.SetActive(false);
                }, true, false, 0.05f, 2, null);
                if (selectedSlot != null && selectedSlot.Card != null)
                {
                    CardModificationInfo cardModificationInfo = new CardModificationInfo(this.LatchAbility);
                    cardModificationInfo.fromLatch = true;
                    selectedSlot.Card.Anim.ShowLatchAbility();
                    selectedSlot.Card.AddTemporaryMod(cardModificationInfo);
                    this.OnSuccessfullyLatched(selectedSlot.Card);
                    yield return new WaitForSeconds(0.75f);
                    yield return base.LearnAbility(0f);
                }
            }

            if (MultiverseBattleSequencer.Instance.CurrentMultiverseId != originalUniverse)
            {
                MultiverseBattleSequencer.Instance.ParentCardToTvScreen(Card, 0f, forceRestore: true);
                yield return MultiverseBattleSequencer.Instance.TravelToUniverse(originalUniverse);
            }

            InteractionCursor.Instance.ClearForcedCursorType();
            yield break;
        }
    }
}
