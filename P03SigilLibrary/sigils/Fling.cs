using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class Fling : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int totalPower = 0;

        private List<CardSlot> ValidTargets => BoardManager.Instance.GetSlotsCopy(this.Card.OpponentCard).Where(s => s.Card != null).ToList();

        static Fling()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fling";
            info.rulebookDescription = "When [creature] is played, if one or more cards was sacrificed to play it, [creature] deals damage to a card of the player's choice equal to the total attack value of all sacrificed cards.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            Fling.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Fling),
                TextureHelper.GetImageAsTexture("ability_fling.png", typeof(Fling).Assembly)
            ).Id;
        }

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => sacrifice.Attack > 0;

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            totalPower += sacrifice.Attack;
            yield break;
        }

        private List<CardSlot> ValidSacrificeSlots
        {
            get
            {
                return BoardManager.Instance.OpponentSlotsCopy.Where(s => s.Card != null && s.Card != this.Card && s.Card.Attack > 0 && !s.Card.HasAbility(Ability.ExplodeOnDeath)).ToList();
            }
        }

        private bool ValidOpponentActivate
        {
            get
            {
                return this.Card.OpponentCard && ValidSacrificeSlots.Count > 0;
            }
        }

        public override bool RespondsToResolveOnBoard() => (ValidOpponentActivate || totalPower > 0) && ValidTargets.Count > 0;

        private IEnumerator OnSelectionSequence(CardSlot slot)
        {
            if (slot.Card != null)
                yield return slot.Card.TakeDamage(totalPower, this.Card);
        }

        private int EvaluateTargetSlot(CardSlot slot)
        {
            if (slot.Card == null)
                return 10000;

            if (slot == this.Card.OpposingSlot())
            {
                if (slot.Card.Health <= this.Card.Attack)
                    return 1000;
                if (slot.Card.Health <= (this.Card.Attack + totalPower))
                    return -1000;
            }

            if (slot.Card.Health <= totalPower)
                return -slot.Card.PowerLevel;

            return 500 - slot.Card.PowerLevel;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            if (this.Card.OpponentCard)
            {
                var sac = ValidSacrificeSlots.OrderBy(s => s.Card.PowerLevel).FirstOrDefault();
                if (sac == null)
                    yield break;

                totalPower = sac.Card.Attack;
                yield return sac.Card.Die(true);
            }

            yield return this.CardChooseSlotSequence(
                (slot) => OnSelectionSequence(slot),
                ValidTargets,
                aiSlotEvaluator: EvaluateTargetSlot,
                aimWeapon: true
            );
        }
    }
}
