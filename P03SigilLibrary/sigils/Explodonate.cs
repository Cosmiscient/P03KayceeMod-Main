using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class Explodonate : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static Explodonate()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Explodonate";
            info.rulebookDescription = "When [creature] dies, it explodes and deals 10 damage to all five adjacent slots.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_explodonate.png", typeof(Explodonate).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Explodonate),
                TextureHelper.GetImageAsTexture("ability_explodonate.png", typeof(Explodonate).Assembly)
            ).Id;
        }

        private IEnumerator BombCard(CardSlot slot)
        {
            if (slot.Card == null)
                yield break;

            GameObject bomb = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/DetonatorHoloBomb"));
            bomb.transform.position = Card.transform.position + (Vector3.up * 0.1f);
            Tween.Position(bomb.transform, slot.Card.transform.position + (Vector3.up * 0.1f), 0.5f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.5f);
            slot.Card.Anim.PlayHitAnimation();
            Destroy(bomb);
            yield return slot.Card.TakeDamage(10, Card);
            yield break;
        }

        protected IEnumerator ExplodonateSequence()
        {
            CardSlot slot = Card.Slot;
            List<CardSlot> friendlySlots = BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard);
            List<CardSlot> opposingSlots = BoardManager.Instance.GetSlotsCopy(Card.OpponentCard);

            if (slot.Index > 0)
            {
                yield return BombCard(friendlySlots[slot.Index % 10 - 1]);
                yield return BombCard(opposingSlots[slot.Index % 10 - 1]);
            }
            yield return BombCard(opposingSlots[slot.Index % 10]);
            if (slot.Index < friendlySlots.Count - 1)
            {
                yield return BombCard(opposingSlots[slot.Index % 10 + 1]);
                yield return BombCard(friendlySlots[slot.Index % 10 + 1]);
            }
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => this.Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            yield return ExplodonateSequence();
        }
    }
}
