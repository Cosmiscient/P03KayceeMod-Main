using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Shove : ActivatedAbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private bool ActivatedThisTurn = false;
		private bool didPush;
		protected bool movingLeft;

		public override int EnergyCost => 2;

        static Shove()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bulldoze";
            info.rulebookDescription = "Pay 2 Energy to cause this card to move in the direction inscribed in the sigil. Creatures that are in the way will be pushed in the same direction..";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            Shove.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Shove),
                TextureHelper.GetImageAsTexture("ability_shovepowered.png", typeof(Shove).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
			CardSlot toLeft = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, adjacentOnLeft: true);
			CardSlot toRight = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, adjacentOnLeft: false);
			Singleton<ViewManager>.Instance.SwitchToView(View.Board);
			yield return new WaitForSeconds(0.25f);
			yield return DoStrafe(toLeft, toRight);
		}

		protected virtual IEnumerator DoStrafe(CardSlot toLeft, CardSlot toRight)
		{
			bool flag = SlotHasSpace(base.Card.Slot, toLeft: true);
			bool flag2 = SlotHasSpace(base.Card.Slot, toLeft: false);
			if (movingLeft && !flag)
			{
				movingLeft = false;
			}
			if (!movingLeft && !flag2)
			{
				movingLeft = true;
			}
			CardSlot destination = (movingLeft ? toLeft : toRight);
			bool destinationValid = (movingLeft ? flag : flag2);
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
			base.Card.RenderInfo.SetAbilityFlipped(Ability, movingLeft);
			base.Card.RenderInfo.flippedPortrait = movingLeft && base.Card.Info.flipPortraitForStrafe;
			base.Card.RenderCard();
			if (destination != null && destinationValid)
			{
				CardSlot oldSlot = base.Card.Slot;
				yield return Singleton<BoardManager>.Instance.AssignCardToSlot(base.Card, destination);
				yield return PostSuccessfulMoveSequence(oldSlot);
				yield return new WaitForSeconds(0.25f);
			}
			else
			{
				base.Card.Anim.StrongNegationEffect();
				yield return new WaitForSeconds(0.15f);
			}
		}

		protected virtual IEnumerator PostSuccessfulMoveSequence(CardSlot oldSlot)
		{
			yield return PreSuccessfulTriggerSequence();
		}

		private IEnumerator RecursivePush(CardSlot slot, bool toLeft, Action<bool> canMoveResult)
		{
			CardSlot adjacent = Singleton<BoardManager>.Instance.GetAdjacent(slot, toLeft);
			if (adjacent == null)
			{
				canMoveResult?.Invoke(obj: false);
				yield break;
			}
			if (adjacent.Card == null)
			{
				yield return Singleton<BoardManager>.Instance.AssignCardToSlot(slot.Card, adjacent);
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
				yield return Singleton<BoardManager>.Instance.AssignCardToSlot(slot.Card, adjacent);
				didPush = true;
			}
			canMoveResult?.Invoke(canMove);
		}

		private bool SlotHasSpace(CardSlot slot, bool toLeft)
		{
			CardSlot adjacent = Singleton<BoardManager>.Instance.GetAdjacent(slot, toLeft);
			if (adjacent == null)
			{
				return false;
			}
			if (adjacent.Card == null)
			{
				return true;
			}
			return SlotHasSpace(adjacent, toLeft);
		}
	}
}
