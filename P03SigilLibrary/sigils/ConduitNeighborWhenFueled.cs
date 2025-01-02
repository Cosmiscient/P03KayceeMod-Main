using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ConduitNeighborWhenFueled : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ConduitNeighborWhenFueled()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gas Generator";
            info.rulebookDescription = "As long as it has fuel, [creature] will cause all friendly cards to behave as if they are part of a completed conduit.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.SetDefaultFuel(2);
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ConduitNeighborWhenFueled),
                TextureHelper.GetImageAsTexture("ability_staticelectricity_when_fueled.png", typeof(ConduitNeighborWhenFueled).Assembly)
            ).Id;

            info.SetUniqueRedirect("fuel", "fuelManagerPage", GameColors.Instance.limeGreen);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == this.Card.IsPlayerCard();

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            this.Card.TrySpendFuel(1);
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPostfix]
        private static void IfFueledHasConduitNeighbor(PlayableCard __instance, Ability ability, ref bool __result)
        {
            if (!__result && ability == ConduitNeighbor.AbilityID)
            {
                if (__instance.HasAbility(AbilityID) && __instance.GetCurrentFuel() > 0)
                    __result = true;
            }
        }
    }
}
