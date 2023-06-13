using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class DoubleSprint : AbilityBehaviour
    {
		protected bool movingLeft;

		public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static DoubleSprint()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Double Sprinter";
            info.rulebookDescription = "At the end of the owner's turn, a card bearing this sigil will move in the direction inscribed in the sigil twice.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

			DoubleSprint.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(DoubleSprint),
                TextureHelper.GetImageAsTexture("ability_turbosprinter.png", typeof(Necromancer).Assembly)
            ).Id;
        }

		public override bool RespondsToTurnEnd(bool playerTurnEnd)
		{
			if (base.Card != null)
			{
				return base.Card.OpponentCard != playerTurnEnd;
			}
			return false;
		}

		public override IEnumerator OnTurnEnd(bool playerTurnEnd)
		{
			CardSlot toLeft = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, adjacentOnLeft: true);
			CardSlot toRight = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, adjacentOnLeft: false);
			Singleton<ViewManager>.Instance.SwitchToView(View.Board);
			yield return new WaitForSeconds(0.25f);
			yield return DoStrafe(toLeft, toRight);

			toLeft = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, adjacentOnLeft: true);
			toRight = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, adjacentOnLeft: false);
			yield return new WaitForSeconds(0.25f);
			yield return DoStrafe(toLeft, toRight);
		}

		protected virtual IEnumerator DoStrafe(CardSlot toLeft, CardSlot toRight)
		{
			bool flag = toLeft != null && toLeft.Card == null;
			bool flag2 = toRight != null && toRight.Card == null;
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
			yield return MoveToSlot(destination, destinationValid);
			if (destination != null && destinationValid)
			{
				yield return PreSuccessfulTriggerSequence();
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
			if (base.Card.Info.name == "Snelk" && oldSlot.Card == null)
			{
				yield return Singleton<BoardManager>.Instance.CreateCardInSlot(CardLoader.GetCardByName("Snelk_Neck"), oldSlot);
			}
		}
	}
}