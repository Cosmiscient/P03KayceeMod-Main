using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Slots;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ThrowFire : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ThrowFire()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fireball";
            info.rulebookDescription = "At the end of its turn, [creature] chooses a card slot to set on fire for three turns.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ThrowFire),
                TextureHelper.GetImageAsTexture("ability_throw_fire.png", typeof(ThrowFire).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card != null && Card.OpponentCard != playerTurnEnd;

        private int CardSlotAIEvaluate(CardSlot slot)
        {
            if (slot.Card == null)
                return 0;

            if (slot.Card.Health == 1 && slot.Card.GetTotalShields() == 0)
                return -slot.Card.PowerLevel;

            return -slot.Card.PowerLevel / 2;
        }

        private IEnumerator OnSelectionSequence(CardSlot selectedSlot, bool playerTurnEnd)
        {
            yield return Molotov.BombCard(selectedSlot, this.Card);

            // If you set your own slot on fire for some reason??
            // Have it do damage and burn down right now
            if (playerTurnEnd == selectedSlot.IsPlayerSlot)
            {
                var burningBehaviour = selectedSlot.GetComponent<BurningSlotBase>();
                if (burningBehaviour != null)
                    yield return burningBehaviour.OnTurnEnd(playerTurnEnd);
            }
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            yield return this.CardChooseSlotSequence(
                (slot) => OnSelectionSequence(slot, playerTurnEnd),
                BoardManager.Instance.AllSlotsCopy,
                CardSlotAIEvaluate
            );
        }
    }
}
