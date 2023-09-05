using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class ConduitGemify : ConduitGainAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        protected override Ability AbilityToGive => Ability.None;
        protected override bool Gemify => true;

        static ConduitGemify()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bedazzling Conduit";
            info.rulebookDescription = "Cards within a circuit completed by [creature] are Gemified.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.conduit = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitGemify),
                TextureHelper.GetImageAsTexture("ability_conduit_gemify.png", typeof(ConduitGemify).Assembly)
            ).Id;
        }
    }
}