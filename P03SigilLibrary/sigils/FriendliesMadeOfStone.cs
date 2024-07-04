using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FriendliesMadeOfStone : CardsWithAbilityHaveAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability RequiredAbility => Ability.None;

        public override Ability GainedAbility => Ability.MadeOfStone;

        static FriendliesMadeOfStone()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Stone Giver";
            info.rulebookDescription = "As long as [creature] is alive, all friendly cards are Made of Stone";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FriendliesMadeOfStone),
                TextureHelper.GetImageAsTexture("ability_all_madeofstone.png", typeof(FriendliesMadeOfStone).Assembly)
            ).Id;
        }
    }
}