using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public abstract class ConduitGainAbility : Conduit
    {
        protected abstract Ability AbilityToGive { get; }
        protected virtual Ability SecondaryAbilityToGive => Ability.None;
        protected virtual bool Gemify => false;

        internal const string CONDUIT_ABILITY_ID = "ConduitGainAbilityMod";

        internal static List<ConduitGainAbility> ActiveAbilities = new();

        static ConduitGainAbility()
        {
            AbilityIconBehaviours.DynamicAbilityCardModIds.Add(CONDUIT_ABILITY_ID);
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnResolveOnBoard()
        {
            ActiveAbilities.Add(this);
            yield return base.OnResolveOnBoard();
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            ActiveAbilities.Remove(this);
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
            if (MultiverseBattleSequencer.Instance == null)
            {
                foreach (PlayableCard card in PlayerHand.Instance.CardsInHand)
                    ClearConduitAbilityMods(card);

                foreach (PlayableCard card in TurnManager.Instance.Opponent.queuedCards)
                    ClearConduitAbilityMods(card);
            }
            else
            {
                foreach (var universe in MultiverseBattleSequencer.Instance.MultiverseGames)
                {
                    if (universe == null)
                        continue;

                    if (universe.HandState != null)
                        foreach (PlayableCard card in universe.HandState)
                            ClearConduitAbilityMods(card);

                    if (universe.OpponentQueue != null)
                        foreach (PlayableCard card in universe.OpponentQueue)
                            ClearConduitAbilityMods(card);
                }
            }
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

        private static void CleanList() => ActiveAbilities.RemoveAll(ab => ab == null);

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
                if (MultiverseBattleSequencer.Instance == null)
                {
                    ResolveForSlots(BoardManager.Instance.opponentSlots);
                    ResolveForSlots(BoardManager.Instance.playerSlots);
                }
                else
                {
                    foreach (var universe in MultiverseBattleSequencer.Instance.MultiverseGames)
                    {
                        if (universe == null || universe.OpponentSlots == null || universe.PlayerSlots == null)
                            continue;

                        ResolveForSlots(universe.OpponentSlots);
                        ResolveForSlots(universe.PlayerSlots);
                    }
                }
                FixCardsNotOnBoard();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static void CleanupActiveAbilities() => ActiveAbilities.Clear();

        [HarmonyPatch(typeof(PhotographerSnapshotManager), nameof(PhotographerSnapshotManager.ApplySlotState))]
        [HarmonyPostfix]
        private static IEnumerator ResetAferPhotog(IEnumerator sequence)
        {
            yield return sequence;

            ActiveAbilities = BoardManager.Instance.AllSlotsCopy.Where(s => s.Card != null).SelectMany(s => s.Card.GetComponents<ConduitGainAbility>()).ToList();
            yield break;
        }
    }
}