using System.Collections;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class SwapWithGunbot : SpecialCardBehaviour
    {
        public static readonly SpecialTriggeredAbility ID = SpecialTriggeredAbilityManager.Add(P03Pack3Plugin.PluginGuid, "DannysSpecialAbility", typeof(SwapWithGunbot)).Id;

        public override bool RespondsToCardGettingAttacked(PlayableCard source) => source == this.Card;

        public override IEnumerator OnCardGettingAttacked(PlayableCard card)
        {
            // Find a gunbot that's alive
            var slots = BoardManager.Instance.GetSlotsCopy(this.PlayableCard.IsPlayerCard());
            CardSlot gunbotSlot = slots.FirstOrDefault(s => s.Card != null && s.Card.HasTrait(Cards.GunbotSwapTrait));
            if (gunbotSlot != null)
            {
                this.Card.Anim.StrongNegationEffect();
                float x = (gunbotSlot.transform.position.x + this.PlayableCard.Slot.transform.position.x) / 2f;
                float y = gunbotSlot.Card.transform.position.y + 0.35f;
                float z = gunbotSlot.Card.transform.position.z;
                Tween.Position(gunbotSlot.Card.transform, new Vector3(x, y, z), 0.3f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

                CardSlot oldSlot = this.PlayableCard.Slot;
                yield return BoardManager.Instance.AssignCardToSlot(this.PlayableCard, gunbotSlot);
                yield return BoardManager.Instance.AssignCardToSlot(gunbotSlot.Card, oldSlot);
            }
        }
    }
}