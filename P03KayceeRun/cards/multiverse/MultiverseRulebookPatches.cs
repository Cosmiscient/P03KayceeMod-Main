using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    internal static class MultiverseRulebookPatches
    {


        private static bool IsMultiverseAbilityPage(RuleBookPageInfo info)
        {
            return info.abilityPage &&
                   AbilitiesUtil.allData.Any(
                        ai => ai.ability.ToString().Equals(info.pageId, System.StringComparison.InvariantCultureIgnoreCase) &&
                              ai.metaCategories.Contains(CustomCards.MultiverseAbility)
                   );
        }

        [HarmonyPatch(typeof(PageFlipper), nameof(PageFlipper.Flip))]
        [HarmonyPrefix]
        private static bool FlipOverMultiverseAbilities(PageFlipper __instance, bool forwards, float duration)
        {
            __instance.currentPageIndex = __instance.WrapIndex(__instance.currentPageIndex + (forwards ? 1 : -1));
            if (MultiverseBattleSequencer.Instance == null)
            {
                while (IsMultiverseAbilityPage(__instance.PageData[__instance.currentPageIndex]))
                    __instance.currentPageIndex = __instance.WrapIndex(__instance.currentPageIndex + (forwards ? 1 : -1));
            }
            __instance.ShowFlip(forwards, duration);
            return false;
        }
    }
}