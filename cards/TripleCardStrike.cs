using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class TripleCardStrike : AbilityBehaviour, IGetOpposingSlots
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static TripleCardStrike()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Big Strike";
            info.rulebookDescription = "[creature] attacks all cards in all lanes opposing it, or attacks just the center opposing lane if there are no cards it can attack.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.flipYIfOpponent = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(TripleCardStrike),
                TextureHelper.GetImageAsTexture("ability_tricard_strike.png", typeof(TripleCardStrike).Assembly)
            ).Id;
        }

        private bool CanAttackSlot(CardSlot slot)
        {
            if (slot.Card == null)
                return false;

            if (slot.Card.FaceDown)
                return false;

            if (Card.HasAbility(Ability.Flying) && !slot.Card.HasAbility(Ability.Reach))
                return false;

            return true;
        }

        public List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots)
        {
            List<CardSlot> retval = new();

            int slot = Card.Slot.Index;
            List<CardSlot> opposingSlots = BoardManager.Instance.GetSlots(Card.OpponentCard);
            if (slot > 0 && CanAttackSlot(opposingSlots[slot - 1]))
                retval.Add(opposingSlots[slot - 1]);
            if (CanAttackSlot(opposingSlots[slot]))
                retval.Add(opposingSlots[slot]);
            if (slot + 1 < opposingSlots.Count && CanAttackSlot(opposingSlots[slot + 1]))
                retval.Add(opposingSlots[slot + 1]);

            return retval;
        }

        public bool RemoveDefaultAttackSlot() => GetOpposingSlots(null, null).Count > 0;

        public bool RespondsToGetOpposingSlots() => true;
    }
}