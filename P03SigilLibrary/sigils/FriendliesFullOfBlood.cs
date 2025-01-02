using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FriendliesFullOfBlood : CardsWithAbilityHaveAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability RequiredAbility => Ability.None;

        public override Ability GainedAbility => FullOfBlood.AbilityID;

        private static bool ShouldBeGivenBloodAbility(PlayableCard card)
        {
            if (card == null)
                return false;

            return !card.Info.Sacrificable;
        }

        internal override CardsWithAbilityHaveAbilityManager.Rule Rule
        {
            get
            {
                if (__rule != null)
                    return __rule;

                __rule = new(ShouldBeGivenBloodAbility, "UnsaccableGainsBlood", FullOfBlood.AbilityID);
                return __rule;
            }
        }

        static FriendliesFullOfBlood()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Blood Donation";
            info.rulebookDescription = "As long as [creature] is alive, all friendly cards can be sacrificed to pay blood costs even if they normally could not.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FriendliesFullOfBlood),
                TextureHelper.GetImageAsTexture("ability_all_sac_anyway.png", typeof(FriendliesFullOfBlood).Assembly)
            ).Id;
        }
    }
}