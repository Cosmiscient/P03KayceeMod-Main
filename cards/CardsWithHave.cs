using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public abstract class CardsWithAbilityHaveAbility : AbilityBehaviour
    {
        public abstract Ability RequiredAbility { get; }
        public abstract Ability GainedAbility { get; }
        public virtual bool AppliesToOpposing => false;
        public virtual bool AppliesToFriendly => true;
    }
}