using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedStrafe : FuelActivatedAbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public override int FuelCost => 1;

        static ActivatedStrafe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Drive";
            info.rulebookDescription = $"Pay 1 Fuel to move in the direction inscribed in this sigil.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedStrafe),
                TextureHelper.GetImageAsTexture("ability_activated_strafe_right.png", typeof(ActivatedStrafe).Assembly)
            ).Id;

            info.SetCustomFlippedTexture(TextureHelper.GetImageAsTexture("ability_activated_strafe_left.png", typeof(ActivatedStrafe).Assembly));
        }

        public override IEnumerator ActivateAfterSpendFuel()
        {
            CardSlot toLeft = BoardManager.Instance.GetAdjacent(this.Card.Slot, true);
            CardSlot toRight = BoardManager.Instance.GetAdjacent(this.Card.Slot, false);
            ViewManager.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.25f);
            yield return this.DoStrafe(toLeft, toRight);
            yield break;
        }

        protected virtual IEnumerator DoStrafe(CardSlot toLeft, CardSlot toRight)
        {
            bool canMoveLeft = toLeft != null && toLeft.Card == null;
            bool canMoveRight = toRight != null && toRight.Card == null;

            if (this.movingLeft && !canMoveLeft)
                this.movingLeft = false;
            if (!this.movingLeft && !canMoveRight)
                this.movingLeft = true;

            CardSlot destination = this.movingLeft ? toLeft : toRight;
            bool destinationValid = this.movingLeft ? canMoveLeft : canMoveRight;
            yield return this.MoveToSlot(destination, destinationValid);

            if (destination != null && destinationValid)
            {
                yield return this.PreSuccessfulTriggerSequence();
                yield return this.LearnAbility(0f);
            }
            yield break;
        }

        protected IEnumerator MoveToSlot(CardSlot destination, bool destinationValid)
        {
            this.Card.RenderInfo.SetAbilityFlipped(this.Ability, this.movingLeft);
            this.Card.RenderInfo.flippedPortrait = this.movingLeft && this.Card.Info.flipPortraitForStrafe;
            this.Card.RenderCard();

            if (destination != null && destinationValid)
            {
                yield return BoardManager.Instance.AssignCardToSlot(this.Card, destination, 0.1f, null, true);
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                this.Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.15f);
            }

            yield break;
        }

        protected bool movingLeft;
    }
}
