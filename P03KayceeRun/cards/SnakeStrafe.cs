using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class SnakeStrafe : Strafe
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static SnakeStrafe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Phase Through";
            info.rulebookDescription = "At the end of its controller's turn, [creature] moves one space in the direction indicated, moving through other cards if necessary until it finds an empty space.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SnakeStrafe),
                TextureHelper.GetImageAsTexture("ability_phasing.png", typeof(SnakeStrafe).Assembly)
            ).Id;
        }

        private static CardSlot FirstEmptySlot(CardSlot thisSlot, bool toLeft)
        {
            CardSlot nextSlot = BoardManager.Instance.GetAdjacent(thisSlot, toLeft);
            while (nextSlot != null)
            {
                if (nextSlot.Card == null)
                    return nextSlot;
                nextSlot = BoardManager.Instance.GetAdjacent(nextSlot, toLeft);
            }
            return nextSlot;
        }

        public override IEnumerator DoStrafe(CardSlot toLeft, CardSlot toRight)
        {
            CardSlot destination = FirstEmptySlot(Card.Slot, movingLeft);
            if (destination == null)
            {
                movingLeft = !movingLeft;
                destination = FirstEmptySlot(Card.Slot, movingLeft);
            }
            yield return MoveToSlot(destination, destination != null);
            if (destination != null)
            {
                yield return PreSuccessfulTriggerSequence();
                yield return LearnAbility(0f);
            }
            yield break;
        }
    }
}