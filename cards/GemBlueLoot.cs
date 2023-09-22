using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GemBlueLoot : Loot
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemBlueLoot()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Looter With Sapphire";
            info.rulebookDescription = "When [creature] deals damage directly, if you control a Sapphire, draw a card for each damage dealt.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.BLUE_CELL, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.darkPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(GemBlueLoot),
                TextureHelper.GetImageAsTexture("ability_bluegemloot.png", typeof(GemBlueLoot).Assembly)
            ).Id;
        }

        public override bool RespondsToDealDamageDirectly(int amount) => base.RespondsToDealDamageDirectly(amount) && Card.EligibleForGemBonus(GemType.Blue);
    }
}
