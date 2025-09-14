using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FriendliesStinkyWhenFueled : CardsWithAbilityHaveAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability RequiredAbility => Ability.None;
        public override Ability GainedAbility => Ability.DebuffEnemy;

        private static bool ShouldBeGivenFuelAbility(PlayableCard card)
        {
            if (card == null)
                return false;

            return card.GetCurrentFuel() > 0;
        }

        internal override CardsWithAbilityHaveAbilityManager.Rule Rule
        {
            get
            {
                if (__rule != null)
                    return __rule;

                __rule = new(ShouldBeGivenFuelAbility, "FueledGainsStinky", Ability.DebuffEnemy);
                return __rule;
            }
        }

        static FriendliesStinkyWhenFueled()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Coal Roller";
            info.rulebookDescription = "As long as [creature] is alive, all friendly cards which have fuel are Stinky.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FriendliesStinkyWhenFueled),
                TextureHelper.GetImageAsTexture("ability_all_debuffenemy_when_fueled.png", typeof(FriendliesStinkyWhenFueled).Assembly)
            ).Id;

            info.SetAbilityRedirect("Stinky", Ability.DebuffEnemy, GameColors.Instance.limeGreen);
        }
    }
}