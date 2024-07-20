using DiskCardGame;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public abstract class CardsWithAbilityHaveAbility : AbilityBehaviour
    {
        public abstract Ability RequiredAbility { get; }
        public abstract Ability GainedAbility { get; }
        public virtual bool AppliesToOpposing => false;
        public virtual bool AppliesToFriendly => true;
    }
}