using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class TargetRequired : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static TargetRequired()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Target Required";
            info.rulebookDescription = "[creature] can only attack lanes that have cards in them.";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = true;
            info.passive = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(TargetRequired),
                TextureHelper.GetImageAsTexture("ability_target_requiredpng.png", typeof(TargetRequired).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.CanAttackDirectly))]
        [HarmonyPostfix]
        private static void TargetRequiredCannotAttackDirectly(PlayableCard __instance, ref bool __result)
        {
            __result = __result && !__instance.HasAbility(AbilityID);
        }
    }
}
