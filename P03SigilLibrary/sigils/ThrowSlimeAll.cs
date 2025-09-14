using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Slots;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ThrowSlimeAll : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ThrowSlimeAll()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Slime Vandal";
            info.rulebookDescription = "When [creature] is played, it slimes all slots on the board. Cards in slimed slots lose one power.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ThrowSlimeAll),
                TextureHelper.GetImageAsTexture("ability_slime_all.png", typeof(ThrowSlimeAll).Assembly)
            ).Id;

            info.SetSlotRedirect("slimed", SlimedSlot.ID, GameColors.Instance.limeGreen);
            info.SetSlotRedirect("slimes", SlimedSlot.ID, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToResolveOnBoard() => true;

        private static IEnumerator SlimeSlotSingle(CardSlot source, CardSlot target, float speed = 0.5f)
        {
            yield return FullOfOil.ThrowOil(source, target, speed, GameColors.Instance.brightLimeGreen);
            yield return target.SetSlotModification(SlimedSlot.ID);
        }

        public static IEnumerator SlimeCardsAsync(List<CardSlot> target, PlayableCard attacker, float speed = 0.5f, Color? color = null)
        {
            if (BoardManager.Instance is BoardManager3D)
            {
                for (int i = 0; i < target.Count; i++)
                    attacker.StartCoroutine(SlimeSlotSingle(attacker.Slot, target[i]));

                yield return new WaitForSeconds(speed * 2f);
            }

            foreach (CardSlot slot in target)
                yield return slot.SetSlotModification(SlimedSlot.ID);

            yield return new WaitForSeconds(speed / 2f);
            yield break;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            Card.Anim.LightNegationEffect();
            List<CardSlot> slots = BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard);
            slots.AddRange(BoardManager.Instance.GetSlotsCopy(Card.OpponentCard));

            yield return SlimeCardsAsync(slots, Card);
        }
    }
}
