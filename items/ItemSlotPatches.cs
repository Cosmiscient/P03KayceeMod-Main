using System;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    internal static class ItemSlotPatches
    {
        [HarmonyPatch(typeof(ItemPage), nameof(ItemPage.FillPage))]
        [HarmonyPostfix]
        private static void P03FillPage(ref ItemPage __instance, string headerText, params object[] otherArgs)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (__instance.itemModel != null && !__instance.itemModel.activeSelf)
                __instance.itemModel.SetActive(true);

            ConsumableItemData consumableByName = ItemsUtil.GetConsumableByName(otherArgs[0] as string);

            if (SaveManager.SaveFile.IsPart3)
                __instance.descriptionTextMesh.color = Color.white;

            if (consumableByName.name == GoobertHuh.ItemData.name)
            {
                Tuple<Color, string> goobertDialogue = GoobertHuh.GetGoobertRulebookDialogue();

                string englishText = goobertDialogue.Item2;
                if (__instance.itemModel == null)
                    englishText = $"To the user: {englishText}";

                __instance.descriptionTextMesh.text = Localization.Translate(goobertDialogue.Item2);

                __instance.descriptionTextMesh.color = goobertDialogue.Item1;
            }
        }
    }
}