using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Slots;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ThrowSlime : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ThrowSlime()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Slimeball";
            info.rulebookDescription = "At the end of its turn, [creature] will choose one slot to become slimed. Cards in a slimed slot lose one power.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ThrowSlime),
                TextureHelper.GetImageAsTexture("ability_throw_slime.png", typeof(ThrowSlime).Assembly)
            ).Id;

            info.SetSlotRedirect("slimed", SlimedSlot.ID, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card != null && Card.OpponentCard != playerTurnEnd;

        private int CardSlotAIEvaluate(CardSlot slot)
        {
            // Protect yourself first!
            if (slot == this.Card.OpposingSlot() && slot.GetSlotModification() == SlotModificationManager.ModificationType.NoModification)
                return -100;

            // Never slime yourself
            if (slot.IsOpponentSlot() == this.Card.OpponentCard)
                return 10000;

            if (slot.GetSlotModification() == FullyLoaded.SlotModID)
                return -50;

            int baseScore = slot.GetSlotModification() == SlimedSlot.ID ? 100 : 0;

            if (slot.Card == null)
                return baseScore;

            return baseScore - (slot.Card.Attack * slot.Card.GetOpposingSlots().Count);
        }

        private IEnumerator OnSelectionSequence(CardSlot selectedSlot)
        {
            yield return FullOfOil.ThrowOil(this.Card.Slot, selectedSlot, 0.5f, GameColors.Instance.brightLimeGreen);

            yield return new WaitForSeconds(0.1f);
            yield return selectedSlot.SetSlotModification(SlimedSlot.ID); ;
            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            yield return this.CardChooseSlotSequence(
                (slot) => OnSelectionSequence(slot),
                BoardManager.Instance.AllSlotsCopy,
                CardSlotAIEvaluate
            );
        }
    }
}
