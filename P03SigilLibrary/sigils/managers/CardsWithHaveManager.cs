using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class CardsWithAbilityHaveAbilityManager : Singleton<CardsWithAbilityHaveAbilityManager>
    {
        public const string RuleKey = "CardWith";

        public class Rule
        {
            public Ability requiredAbility;
            public Trait requiredTrait;
            public Ability[] gainedAbilities;
            public string modId;

            private Func<PlayableCard, bool> customCondition = null;

            public bool CardIsEligible(PlayableCard card)
            {
                if (card == null)
                    return false;

                if (customCondition != null)
                    return customCondition(card);

                bool abilityEligible = requiredAbility == Ability.None || card.HasAbility(requiredAbility);
                bool traitEligible = requiredTrait == Trait.None || card.HasTrait(requiredTrait);
                return abilityEligible && traitEligible;
            }

            internal Rule(Func<PlayableCard, bool> cond, string uniqueKey, Ability gained)
            {
                customCondition = cond;
                modId = $"{RuleKey}{uniqueKey}";
                gainedAbilities = new Ability[] { gained };

                AbilityIconBehaviours.DynamicAbilityCardModIds.Add(modId);
            }

            public Rule(Ability required, Trait reqTrait, Ability gained, List<Ability> additionalGained)
            {
                requiredAbility = required;
                requiredTrait = reqTrait;
                gainedAbilities = new Ability[additionalGained == null ? 1 : additionalGained.Count + 1];
                gainedAbilities[0] = gained;

                if (additionalGained != null)
                    for (int i = 0; i < additionalGained.Count; i++)
                        gainedAbilities[i + 1] = additionalGained[i];

                modId = RuleKey +
                        (required == Ability.None ? string.Empty : required.ToString()) +
                        (requiredTrait == Trait.None ? string.Empty : requiredTrait.ToString()) +
                        "Gains" + string.Join("", gainedAbilities);

                AbilityIconBehaviours.DynamicAbilityCardModIds.Add(modId);
            }
        }

        private readonly List<Rule> PlayerRules = new();
        private readonly List<Rule> OpponentRules = new();

        private void ApplyAbilities(List<Rule> rules, List<CardSlot> slots)
        {
            foreach (CardSlot slot in slots.Where(s => s.Card != null))
            {
                foreach (Rule rule in rules)
                {
                    if (rule.CardIsEligible(slot.Card))
                    {
                        if (!slot.Card.TemporaryMods.Any(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(rule.modId)))
                        {
                            CardModificationInfo info = new()
                            {
                                singletonId = rule.modId,
                                abilities = new(rule.gainedAbilities)
                            };
                            slot.Card.AddTemporaryMod(info);
                        }
                    }
                }

                // Remove all where there is no longer a rule for it
                // Remove all where the rule no longer applies to this card
                List<CardModificationInfo> modsToRemove = slot.Card.temporaryMods.Where(m =>
                    !string.IsNullOrEmpty(m.singletonId) &&
                    m.singletonId.StartsWith(RuleKey) &&
                    !rules.Any(r => r.modId.Equals(m.singletonId) && r.CardIsEligible(slot.Card))
                ).ToList();
                foreach (CardModificationInfo mod in modsToRemove)
                {
                    slot.Card.RemoveTemporaryMod(mod);
                }
            }
        }

        private void DiscoverAbilities()
        {
            PlayerRules.Clear();
            OpponentRules.Clear();

            foreach (CardSlot slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
            {
                if (slot.Card != null)
                {
                    foreach (CardsWithAbilityHaveAbility abilityComb in slot.Card.GetComponents<CardsWithAbilityHaveAbility>())
                    {
                        if ((abilityComb.AppliesToFriendly && !slot.Card.OpponentCard) ||
                            (abilityComb.AppliesToOpposing && slot.Card.OpponentCard))
                        {
                            PlayerRules.Add(abilityComb.Rule);
                        }

                        if ((abilityComb.AppliesToFriendly && slot.Card.OpponentCard) ||
                            (abilityComb.AppliesToOpposing && !slot.Card.OpponentCard))
                        {
                            OpponentRules.Add(abilityComb.Rule);
                        }
                    }
                }
            }
        }

        public override void ManagedUpdate()
        {
            // Find all cards that have a 'cardswithhave' ability on them
            DiscoverAbilities();
            ApplyAbilities(PlayerRules, BoardManager.Instance.playerSlots);
            ApplyAbilities(OpponentRules, BoardManager.Instance.opponentSlots);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPrefix]
        private static void CreateManager()
        {
            if (Instance == null)
            {
                TurnManager.Instance.gameObject.AddComponent<CardsWithAbilityHaveAbilityManager>();
            }
        }
    }
}