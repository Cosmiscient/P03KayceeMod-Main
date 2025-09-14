using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public abstract class ConduitGainAbility : Conduit
    {
        protected abstract Ability AbilityToGive { get; }
        protected virtual Ability SecondaryAbilityToGive => Ability.None;
        protected virtual bool Gemify => false;
        private bool isActive = true;

        internal const string CONDUIT_ABILITY_ID = "ConduitGainAbilityMod";

        public static HashSet<ConduitGainAbility> ActiveAbilities = new();

        static ConduitGainAbility()
        {
            AbilityIconBehaviours.DynamicAbilityCardModIds.Add(CONDUIT_ABILITY_ID);
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            ManageAllActiveAbilityMods();
            yield break;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            ActiveAbilities.Add(this);
            yield return base.OnResolveOnBoard();
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            ActiveAbilities.Remove(this);
            isActive = false;
            yield return base.OnResolveOnBoard();
        }

        private static readonly Dictionary<PlayableCard, CardModificationInfo> ConduitAbilityMods = new();

        private static CardModificationInfo GetConduitAbilityMod(PlayableCard card)
        {
            ConduitAbilityMods.TryGetValue(card, out CardModificationInfo mod);

            if (mod == null)
            {
                mod = new() { singletonId = CONDUIT_ABILITY_ID };
                card.AddTemporaryMod(mod);
                ConduitAbilityMods.Add(card, mod);
            }

            return mod;
        }

        private static void ClearConduitAbilityMods(PlayableCard card)
        {
            if (ConduitAbilityMods.ContainsKey(card))
            {
                CardModificationInfo info = ConduitAbilityMods[card];
                card.RemoveTemporaryMod(info);
                ConduitAbilityMods.Remove(card);
                card.RenderCard();
                card.UpdateFaceUpOnBoardEffects();
            }
        }

        private static List<Ability> GetConduitAbilitiesForSlot(CardSlot slot)
        {
            List<Ability> retval = new();
            List<PlayableCard> conduits = ConduitCircuitManager.Instance.GetConduitsForSlot(slot);
            foreach (ConduitGainAbility ability in ActiveAbilities.Where(ab => ab != null))
            {
                foreach (PlayableCard card in conduits)
                {
                    if (ability.Card == card)
                    {
                        if (ability.AbilityToGive != Ability.None)
                            retval.Add(ability.AbilityToGive);
                        if (ability.SecondaryAbilityToGive != Ability.None)
                            retval.Add(ability.SecondaryAbilityToGive);
                        if (ability.Gemify)
                            retval.Add(Ability.None);
                    }
                }
            }

            return retval;
        }

        private static bool Match(List<Ability> a, CardModificationInfo bMod)
        {
            List<Ability> b = new(bMod.abilities);
            if (bMod.gemify)
                b.Add(Ability.None);

            return !a.Except(b).Any() && !b.Except(a).Any();
        }

        private static void ResolveForSlots(List<CardSlot> slots)
        {
            foreach (CardSlot slot in slots.Where(s => s.Card != null))
            {
                if (TurnManager.Instance.Opponent.queuedCards.Contains(slot.Card))
                    continue;

                List<Ability> conduitAbilities = GetConduitAbilitiesForSlot(slot);
                CardModificationInfo info = GetConduitAbilityMod(slot.Card);

                if (!Match(conduitAbilities, info))
                {
                    // This is wacky. Because we've kept a reference to the original mod
                    // And we then modify the mod in place
                    // The logic for singleton mods is totally broken
                    // We have to clear all the triggers manually
                    foreach (var a in info.abilities)
                        slot.Card.TriggerHandler.RemoveAbility(a);

                    info.abilities.Clear();
                    info.abilities.AddRange(conduitAbilities.Where(a => a != Ability.None));
                    info.gemify = conduitAbilities.Any(a => a == Ability.None);
                    slot.Card.AddTemporaryMod(info);
                    slot.Card.RenderCard();
                    slot.Card.UpdateFaceUpOnBoardEffects();
                }
            }
        }

        private static void FixCardsNotOnBoard()
        {
            foreach (PlayableCard card in PlayerHand.Instance.CardsInHand)
                ClearConduitAbilityMods(card);

            foreach (PlayableCard card in TurnManager.Instance.Opponent.queuedCards)
                ClearConduitAbilityMods(card);
        }

        private static void ClearAllCards()
        {
            FixCardsNotOnBoard();
            foreach (PlayableCard card in BoardManager.Instance.playerSlots.Where(s => s.Card != null).Select(s => s.Card))
            {
                ClearConduitAbilityMods(card);
            }

            foreach (PlayableCard card in BoardManager.Instance.opponentSlots.Where(s => s.Card != null).Select(s => s.Card))
            {
                ClearConduitAbilityMods(card);
            }
        }

        private static void CleanList() => ActiveAbilities.RemoveWhere(ab => ab == null);

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.ManagedUpdate))]
        [HarmonyPostfix]
        private static void ManageAllActiveAbilityMods()
        {
            if (!GameFlowManager.Instance || GameFlowManager.Instance.CurrentGameState != GameState.CardBattle)
            {
                return;
            }

            CleanList();

            if (ActiveAbilities.Count == 0)
            {
                ClearAllCards();
            }
            else
            {
                ResolveForSlots(BoardManager.Instance.opponentSlots);
                ResolveForSlots(BoardManager.Instance.playerSlots);
                FixCardsNotOnBoard();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static void CleanupActiveAbilities() => ActiveAbilities.Clear();

        public override void ManagedUpdate()
        {
            if (this.isActive)
                ActiveAbilities.Add(this);
        }
    }
}