using System;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class AbilityIconBehaviours
    {
        public const string CELL_INVERSE = "InverseCell";

        internal static readonly List<string> DynamicAbilityCardModIds = new();

        [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.SetColorOfDefaultIcons))]
        [HarmonyPostfix]
        private static void HandleAllColorationOfAbilities(ref CardAbilityIcons __instance, Color color, bool inConduitCircuit)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                List<GameObject> defaultIconGroups = __instance.defaultIconGroups;
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf)
                    {
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            if (info.activated)
                            {
                                abilityIconInteractable.SetColor(Color.white);
                            }
                            if (info.GetExtendedPropertyAsBool(CELL_INVERSE).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(inConduitCircuit ? info.colorOverride : color);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayAbilityIcons))]
        [HarmonyPostfix]
        private static void ColorizeConduitAbilities(CardDisplayer3D __instance, PlayableCard playableCard)
        {
            if (__instance.AbilityIcons == null || playableCard == null || playableCard.TemporaryMods == null)
                return;

            foreach (CardModificationInfo mod in playableCard.TemporaryMods)
            {
                if (mod == null || String.IsNullOrEmpty(mod.singletonId) || mod.abilities == null)
                    continue;

                if (DynamicAbilityCardModIds.Contains(mod.singletonId))
                {
                    foreach (Ability ability in mod.abilities)
                    {
                        if (!playableCard.Info.HasAbility(ability))
                        {
                            __instance.AbilityIcons
                                      .abilityIcons
                                      .Find(a => a.Ability == ability)
                                      ?.SetColor(GameColors.Instance.brightGold);
                        }
                    }
                }
            }
        }
    }
}