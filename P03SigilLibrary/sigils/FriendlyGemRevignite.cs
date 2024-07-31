using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FriendlyGemRevignite : CardsWithAbilityHaveAbility, IPassiveAttackBuff
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability RequiredAbility => Ability.None;
        public override Trait RequiredTrait => Trait.Gem;

        public override Ability GainedAbility => FireBomb.AbilityID;
        private static readonly List<Ability> _otherGained = new() { Ability.Brittle, BurntOut.AbilityID };
        public override List<Ability> AdditionalGainedAbilities => _otherGained;

        static FriendlyGemRevignite()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gem Ignition";
            info.rulebookDescription = "As long as [creature] is alive, all friendly gems have +1 Power, Flame Strike, Brittle, and Burnt Out.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FriendlyGemRevignite),
                TextureHelper.GetImageAsTexture("ability_gem_revignite.png", typeof(FriendlyGemRevignite).Assembly)
            ).Id;
        }

        public int GetPassiveAttackBuff(PlayableCard target)
        {
            return this.Card.OnBoard && target.OpponentCard == this.Card.OpponentCard && target.HasTrait(Trait.Gem) ? 1 : 0;
        }
    }
}