using System.Collections.Generic;
using DiskCardGame;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public abstract class CardsWithAbilityHaveAbility : AbilityBehaviour
    {
        public abstract Ability RequiredAbility { get; }
        public virtual Trait RequiredTrait => Trait.None;
        public abstract Ability GainedAbility { get; }
        public virtual List<Ability> AdditionalGainedAbilities => null;
        public virtual bool AppliesToOpposing => false;
        public virtual bool AppliesToFriendly => true;

        protected CardsWithAbilityHaveAbilityManager.Rule __rule = null;
        internal virtual CardsWithAbilityHaveAbilityManager.Rule Rule
        {
            get
            {
                if (__rule != null)
                    return __rule;

                __rule = new(this.RequiredAbility, this.RequiredTrait, this.GainedAbility, this.AdditionalGainedAbilities);
                return __rule;
            }
        }
    }
}