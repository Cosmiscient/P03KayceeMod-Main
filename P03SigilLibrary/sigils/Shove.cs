using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class Shove : ActivatedAbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private bool didPush;
        protected bool movingLeft;

        public override int EnergyCost => 1;

        static Shove()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bulldoze";
            info.rulebookDescription = "Pay 1 Energy to cause this card to move in the direction inscribed in the sigil. Creatures that are in the way will be pushed in the same direction..";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Shove),
                TextureHelper.GetImageAsTexture("ability_shovepowered.png", typeof(Shove).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            CardSlot toLeft = BoardManager.Instance.GetAdjacent(Card.Slot, adjacentOnLeft: true);
            CardSlot toRight = BoardManager.Instance.GetAdjacent(Card.Slot, adjacentOnLeft: false);
            ViewManager.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.25f);
            yield return DoStrafe(toLeft, toRight);
        }

        protected virtual IEnumerator DoStrafe(CardSlot toLeft, CardSlot toRight)
        {
            bool flag = SlotHasSpace(Card.Slot, toLeft: true);
            bool flag2 = SlotHasSpace(Card.Slot, toLeft: false);
            if (movingLeft && !flag)
            {
                movingLeft = false;
            }
            if (!movingLeft && !flag2)
            {
                movingLeft = true;
            }
            CardSlot destination = movingLeft ? toLeft : toRight;
            bool destinationValid = movingLeft ? flag : flag2;
            if (destination != null && destination.Card != null)
            {
                didPush = false;
                yield return RecursivePush(destination, movingLeft, null);
            }
            yield return MoveToSlot(destination, destinationValid);
            if (didPush)
            {
                yield return LearnAbility();
            }
        }

        protected IEnumerator MoveToSlot(CardSlot destination, bool destinationValid)
        {
            Card.RenderInfo.SetAbilityFlipped(Ability, movingLeft);
            Card.RenderInfo.flippedPortrait = movingLeft && Card.Info.flipPortraitForStrafe;
            Card.RenderCard();
            if (destination != null && destinationValid)
            {
                CardSlot oldSlot = Card.Slot;
                yield return BoardManager.Instance.AssignCardToSlot(Card, destination);
                yield return PostSuccessfulMoveSequence(oldSlot);
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.15f);
            }
        }

        protected virtual IEnumerator PostSuccessfulMoveSequence(CardSlot oldSlot)
        {
            yield return PreSuccessfulTriggerSequence();
        }

        private IEnumerator RecursivePush(CardSlot slot, bool toLeft, Action<bool> canMoveResult)
        {
            CardSlot adjacent = BoardManager.Instance.GetAdjacent(slot, toLeft);
            if (adjacent == null)
            {
                canMoveResult?.Invoke(obj: false);
                yield break;
            }
            if (adjacent.Card == null)
            {
                yield return BoardManager.Instance.AssignCardToSlot(slot.Card, adjacent);
                didPush = true;
                canMoveResult?.Invoke(obj: true);
                yield break;
            }
            bool canMove = false;
            yield return RecursivePush(adjacent, toLeft, delegate (bool movePossible)
            {
                canMove = movePossible;
            });
            if (canMove)
            {
                yield return BoardManager.Instance.AssignCardToSlot(slot.Card, adjacent);
                didPush = true;
            }
            canMoveResult?.Invoke(canMove);
        }

        private bool SlotHasSpace(CardSlot slot, bool toLeft)
        {
            CardSlot adjacent = BoardManager.Instance.GetAdjacent(slot, toLeft);
            if (adjacent == null)
            {
                return false;
            }
            return adjacent.Card == null ? true : SlotHasSpace(adjacent, toLeft);
        }
    }
}
