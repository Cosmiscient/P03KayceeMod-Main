using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class FriendliesMagicDust : CardsWithAbilityHaveAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability RequiredAbility => Ability.None;

        public override Ability GainedAbility => MagicDust.AbilityID;

        static FriendliesMagicDust()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Dust Giver";
            info.rulebookDescription = "As long as [creature] is alive, all friendly cards have Magic Dust";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(FriendliesMagicDust),
                TextureHelper.GetImageAsTexture("ability_all_magicdust.png", typeof(FriendliesMagicDust).Assembly)
            ).Id;
        }
    }
}