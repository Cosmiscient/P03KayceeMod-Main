using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class GemOrangeFlying : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemOrangeFlying()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Orange Mox Airborne";
            info.rulebookDescription = "If you control an Orange Mox, this card will ignore opposing cards and strike an opponent directly.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.ORANGE_CELL, true);
            info.passive = true;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemOrangeFlying),
                TextureHelper.GetImageAsTexture("ability_orangegemflying.png", typeof(GemOrangeFlying).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPrefix]
        private static bool IncludeOrangeFlying(PlayableCard __instance, Ability ability, ref bool __result)
        {
            if (ability == Ability.Flying && __instance.HasAbility(AbilityID) && __instance.EligibleForGemBonus(GemType.Orange))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
