using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GemOrangeFlying : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemOrangeFlying()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Airborne With Ruby";
            info.rulebookDescription = "If you control a ruby, this card will ignore opposing cards and strike an opponent directly.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.ORANGE_CELL, true);
            info.passive = true;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.darkPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(GemOrangeFlying),
                TextureHelper.GetImageAsTexture("ability_orangegemflying.png", typeof(GemOrangeFlying).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPostfix]
        private static void IncludeOrangeFlying(PlayableCard __instance, Ability ability, ref bool __result)
        {
            if (!__result && ability == Ability.Flying)
            {
                if (__instance.HasAbility(AbilityID) && __instance.EligibleForGemBonus(GemType.Orange))
                    __result = true;
            }
        }
    }
}
