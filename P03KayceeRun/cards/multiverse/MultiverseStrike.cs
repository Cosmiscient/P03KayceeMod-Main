using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseStrike : AbilityBehaviour, IOnPostSlotAttackSequence
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MultiverseStrike()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.DoubleStrike);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse Strike";
            info.rulebookDescription = "[creature] will strike each opposing space in every universe other than its own.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseStrike),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_doublestrike")
            ).Id;
        }

        public bool RespondsToPostSlotAttackSequence(CardSlot attackingSlot) => MultiverseBattleSequencer.Instance != null && Card.OnBoard && !Card.Dead && Card.Slot == attackingSlot;

        private List<CardSlot> GetOtherUniverseSlots()
        {
            int idx = Card.Slot.Index % 10;
            int currentUniverse = MultiverseBattleSequencer.Instance.GetUniverseId(Card.Slot);

            List<CardSlot> retval = new();
            for (int i = 0; i < MultiverseBattleSequencer.Instance.MultiverseGames.Length; i++)
            {
                if (i == currentUniverse)
                    continue;

                var universe = MultiverseBattleSequencer.Instance.MultiverseGames[i];
                var slots = Card.OpponentCard ? universe.PlayerSlots : universe.OpponentSlots;
                retval.Add(slots[idx]);
            }

            return retval;
        }

        public IEnumerator OnPostSlotAttackSequence(CardSlot attackingSlot)
        {
            if (!RespondsToPostSlotAttackSequence(attackingSlot))
                yield break;

            P03Plugin.Log.LogInfo("Starting multiverse slot attack");

            // Get the id for the current universe
            int startingUniverse = MultiverseBattleSequencer.Instance.CurrentMultiverseId;
            var targets = GetOtherUniverseSlots();

            P03Plugin.Log.LogInfo($"There are {targets.Count} other targets");

            foreach (var opposingSlot in targets)
            {
                yield return CrossUniverseAttack(opposingSlot);
            }

            if (MultiverseBattleSequencer.Instance.CurrentMultiverseId != startingUniverse)
                yield return MultiverseBattleSequencer.Instance.TravelToUniverse(startingUniverse);

            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator CrossUniverseAttack(CardSlot opposingSlot)
        {
            // This whole sequence handles what happens when you attack a slot in a different universe
            CardSlot originalAttackingSlot = Card.slot;
            P03Plugin.Log.LogInfo("Starting cross-universe attack");

            if (Card.Anim.DoingAttackAnimation)
            {
                P03Plugin.Log.LogInfo("Waiting for existing attack to finish");
                yield return new WaitUntil(() => !Card.Anim.DoingAttackAnimation);
                yield return new WaitForSeconds(0.25f);
            }

            P03Plugin.Log.LogInfo("Triggering cards on board");
            yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.SlotTargetedForAttack, false, opposingSlot, Card);
            yield return new WaitForSeconds(0.025f);

            if (opposingSlot.Card != null && Card.AttackIsBlocked(opposingSlot))
            {
                P03Plugin.Log.LogInfo("Attack is blocked");
                yield return TurnManager.Instance.CombatPhaseManager.ShowCardBlocked(Card);
                yield break;
            }

            // Okay - the evidence we have so far tells us that we can do this attack
            P03Plugin.Log.LogInfo("Traveling to target universe");
            int targetUniverse = MultiverseBattleSequencer.Instance.GetUniverseId(opposingSlot);
            yield return MultiverseBattleSequencer.Instance.TravelToUniverse(targetUniverse);

            // And we go ahead and trigger the "slot targeted for attack" trigger again:
            P03Plugin.Log.LogInfo("Triggering for slottargetredforattack");
            yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.SlotTargetedForAttack, false, opposingSlot, Card);
            yield return new WaitForSeconds(0.025f);

            if (Card.CanAttackDirectly(opposingSlot))
            {
                P03Plugin.Log.LogInfo("Attacking opponent directly");
                TurnManager.Instance.CombatPhaseManager.DamageDealtThisPhase += Card.Attack;
                yield return MultiverseBattleSequencer.Instance.VisualizeMultiversalAttack(Card, opposingSlot);
                if (Card.TriggerHandler.RespondsToTrigger(Trigger.DealDamageDirectly, Card.Attack))
                {
                    yield return Card.TriggerHandler.OnTrigger(Trigger.DealDamageDirectly, Card.Attack);
                }
                yield return LifeManager.Instance.ShowDamageSequence(Card.Attack, Card.Attack, Card.OpponentCard);
                yield break;
            }

            P03Plugin.Log.LogInfo("Attacking card");
            bool impactFrameReached = false;
            bool readyToUnpause = false;

            // The attack visualization happens on its own coroutine
            StartCoroutine(MultiverseBattleSequencer.Instance.VisualizeMultiversalAttack(Card, opposingSlot, () => impactFrameReached = true, () => readyToUnpause));
            yield return new WaitUntil(() => impactFrameReached);

            yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.CardGettingAttacked, false, opposingSlot.Card);
            if (!Card.Dead && Card.Slot != null)
            {
                // We don't do the reach animation for a multiverse attack
                readyToUnpause = true;
                yield return new WaitForSeconds(0.05f);

                int overkillDamage = Card.Attack - opposingSlot.Card.Health;
                yield return opposingSlot.Card.TakeDamage(Card.Attack, Card);
                yield return TurnManager.Instance.CombatPhaseManager.DealOverkillDamage(overkillDamage, Card.slot, opposingSlot);
            }

            // Just in case!
            if (!readyToUnpause)
            {
                readyToUnpause = true;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}

