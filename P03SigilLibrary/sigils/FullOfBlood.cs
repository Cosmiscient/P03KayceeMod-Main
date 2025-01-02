using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class FullOfBlood : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        static FullOfBlood()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Full of Blood";
            info.rulebookDescription = "[creature] can be sacrificed to pay blood costs.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.flipYIfOpponent = false;
            info.passive = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FullOfBlood),
                TextureHelper.GetImageAsTexture("ability_sac_anyway.png", typeof(FullOfBlood).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.CanBeSacrificed), MethodType.Getter)]
        [HarmonyPostfix]
        private static void SacrificeFullOfBlood(PlayableCard __instance, ref bool __result)
        {
            __result = __result || (!__instance.FaceDown && __instance.HasAbility(AbilityID));
        }
    }
}
