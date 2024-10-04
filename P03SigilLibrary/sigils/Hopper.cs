using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class Hopper : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static Hopper()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Hopper";
            info.rulebookDescription = "At the end of each turn, [creature] moves to an empty space of its owner's choosing.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Hopper),
                TextureHelper.GetImageAsTexture("ability_hopper.png", typeof(Hopper).Assembly)
            ).Id;
        }

        private static int DirectAttackDamage(CardSlot slot)
        {
            if (slot.Card == null)
                return 0;

            bool hasFlying = slot.Card.HasAbility(Ability.Flying);
            return slot.Card.GetOpposingSlots()
                .Where(o => o.Card == null || (!slot.Card.HasAbility(Ability.Reach) && hasFlying))
                .Select(s => slot.Card.Attack)
                .Sum();
        }

        private bool OpponentDeathIsImminent
        {
            get
            {
                // Figure out how much unblocked damage exists
                int unblockedDamage = BoardManager.Instance.PlayerSlotsCopy
                    .Select(DirectAttackDamage).Sum();

                int predictedLifeBarNextTurn = LifeManager.Instance.Balance + unblockedDamage;
                if (ResourcesManager.Instance.PlayerMaxEnergy <= 3)
                    return predictedLifeBarNextTurn >= 4;
                else
                    return predictedLifeBarNextTurn >= 3;
            }
        }

        private int HowMuchDamageCanAbsorb(CardSlot slot)
        {
            int remainingHealth = this.Card.Health;
            int damagedAbsorbed = 0;
            bool hasReach = this.Card.HasAbility(Ability.Reach);
            foreach (var opSlot in BoardManager.Instance.PlayerSlotsCopy)
            {
                if (opSlot.Card == null)
                    continue;

                bool hasFlying = opSlot.Card.HasAbility(Ability.Flying);
                foreach (var opOpSlot in opSlot.Card.GetOpposingSlots())
                {
                    if (opOpSlot == slot)
                    {
                        if (!hasFlying || hasReach)
                        {
                            remainingHealth -= opSlot.Card.Attack;
                            damagedAbsorbed += opSlot.Card.Attack;

                            if (remainingHealth <= 0)
                                return damagedAbsorbed;
                        }
                    }
                }
            }
            return damagedAbsorbed;
        }

        public int EvaluateOpponentMoveToSlot(CardSlot slot)
        {
            // We move on the end step, so we don't want to move to a slot where
            // we're just going to die...UNLESS we are going to die! We'll assume
            // we're going to do if the opponent has enough on board to kill us
            // or get down to 2 damage (assume the opponent can add 2 damage)
            if (OpponentDeathIsImminent)
            {
                // We'll evaluate this slot based on how much damage it prevents
                // by being here.
                return -HowMuchDamageCanAbsorb(slot);
            }

            int result = 100;

            // Slightly prefer the current slot
            if (slot == this.Card.Slot)
                result -= 5;

            // If we block a queued card, increase the score by 100
            if (TurnManager.Instance.Opponent.Queue.Any(p => p.QueuedSlot == slot))
                result += 50;

            // Since we're not about to die:
            // If we get killed by the next attack, add 100 points to the score
            if (HowMuchDamageCanAbsorb(slot) >= this.Card.Health)
                result += 100;

            // Sort by the biggest power level of card you'd kill
            foreach (var op in this.Card.GetOpposingSlots())
                result -= op.Card?.PowerLevel ?? 0;

            return result;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card != null && Card.OpponentCard != playerTurnEnd;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            List<CardSlot> validslots = BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard()).FindAll(x => x.Card == null || x.Card == Card);
            if (validslots.Count == 0)
            {
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.25f);
                Card.Anim.StrongNegationEffect();
            }
            else
            {
                yield return this.CardChooseSlotSequence(
                    s => BoardManager.Instance.AssignCardToSlot(Card, s, 0.1f, null, false),
                    validslots,
                    aiSlotEvaluator: s => this.EvaluateOpponentMoveToSlot(s),
                    cursor: CursorType.Place,
                    tweenIn: false
                );
            }
            yield break;
        }
    }
}
