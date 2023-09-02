using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace P03KayceeRun.cards
{
    public class LatchNullConduit : Latch
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability LatchAbility => Ability.ConduitNull;

        static LatchNullConduit()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Conduit Latch";
            info.rulebookDescription = "When [creature] perishes, its owner chooses a creature to gain the Null Conduit sigil.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(LatchNullConduit),
                TextureHelper.GetImageAsTexture("ability_latch_nullconduit.png", typeof(LatchNullConduit).Assembly)
            ).Id;
        }
    }
}