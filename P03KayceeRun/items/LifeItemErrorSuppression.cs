using System.Collections;
using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    internal static class LifeItemErrorSuppression
    {
        [HarmonyPatch(typeof(Part3Weight), nameof(Part3Weight.FlickerHologram))]
        [HarmonyPostfix]
        private static IEnumerator OnlyAtEnd(IEnumerator sequence)
        {
            if (!SaveManager.SaveFile.IsPart3)
                yield break;

            yield return sequence;
        }
    }
}