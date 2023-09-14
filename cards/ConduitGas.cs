using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class ConduitGas : ConduitGainAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        protected override Ability AbilityToGive => FireBomb.AbilityID;
        protected override Ability SecondaryAbilityToGive => BurntOut.AbilityID;

        static ConduitGas()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gas Conduit";
            info.rulebookDescription = $"Cards within a circuit completed by [creature] have Fire Strike and Burnt Out.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduit = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, FireBomb.FlamingAbility, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitGas),
                TextureHelper.GetImageAsTexture("ability_conduitgas.png", typeof(ConduitGainDebuffEnemy).Assembly)
            ).Id;
        }
    }
}