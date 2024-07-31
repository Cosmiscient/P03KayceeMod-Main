using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public static class AbilityIconBehaviours
    {
        public const string CELL_INVERSE = "InverseCell";
        public const string ORANGE_CELL = "FromOrange";
        public const string GREEN_CELL = "FromGreen";
        public const string BLUE_CELL = "FromBlue";
        public const string GEM_CELL = "FromGem";
        public const string ORANGE_CELL_INVERSE = "FromOrangeInverse";
        public const string GREEN_CELL_INVERSE = "FromEmeraldInverse";
        public const string BLUE_CELL_INVERSE = "FromSapphireInverse";
        public const string GEM_CELL_INVERSE = "FromGemInverse";
        public const string ACTIVE_WHEN_FUELED = "ActiveWhenFueled";

        private readonly static List<CardAppearanceBehaviour.Appearance> GemReRenderAppearances = new();
        public static void AddGemReRenderAppearance(CardAppearanceBehaviour.Appearance id)
        {
            GemReRenderAppearances.Add(id);
        }

        public static bool IsGemReRender(this AbilityInfo info)
        {
            return (info.GetExtendedPropertyAsBool(ORANGE_CELL) ?? false)
                || (info.GetExtendedPropertyAsBool(GREEN_CELL) ?? false)
                || (info.GetExtendedPropertyAsBool(BLUE_CELL) ?? false)
                || (info.GetExtendedPropertyAsBool(GEM_CELL) ?? false)
                || (info.GetExtendedPropertyAsBool(ORANGE_CELL_INVERSE) ?? false)
                || (info.GetExtendedPropertyAsBool(GREEN_CELL_INVERSE) ?? false)
                || (info.GetExtendedPropertyAsBool(BLUE_CELL_INVERSE) ?? false)
                || (info.GetExtendedPropertyAsBool(GEM_CELL_INVERSE) ?? false);
        }

        internal static readonly HashSet<string> DynamicAbilityCardModIds = new();

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
                            else if (info.GetExtendedPropertyAsBool(CELL_INVERSE) ?? false)
                            {
                                abilityIconInteractable.SetColor(inConduitCircuit ? info.colorOverride : color);
                            }
                            else if (info.conduitCell)
                            {
                                abilityIconInteractable.SetColor(!inConduitCircuit ? info.colorOverride : color);
                            }
                            else if (info.hasColorOverride)
                            {
                                abilityIconInteractable.SetColor(info.colorOverride);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayAbilityIcons))]
        [HarmonyPostfix]
        private static void RecolorGemIcons(CardDisplayer3D __instance, PlayableCard playableCard, CardRenderInfo renderInfo)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                List<GameObject> defaultIconGroups = __instance.AbilityIcons.defaultIconGroups;
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf)
                    {
                        //P03SigilLibraryPlugin.Log.LogInfo($"Updating ability icon colors for group {group}");
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            if (info.GetExtendedPropertyAsBool(GREEN_CELL) ?? false)
                            {
                                abilityIconInteractable.SetColor(!playableCard.EligibleForGemBonus(GemType.Green) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            else if (info.GetExtendedPropertyAsBool(ORANGE_CELL) ?? false)
                            {
                                abilityIconInteractable.SetColor(!playableCard.EligibleForGemBonus(GemType.Orange) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            else if (info.GetExtendedPropertyAsBool(BLUE_CELL) ?? false)
                            {
                                abilityIconInteractable.SetColor(!playableCard.EligibleForGemBonus(GemType.Blue) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            else if (info.GetExtendedPropertyAsBool(GREEN_CELL_INVERSE) ?? false)
                            {
                                abilityIconInteractable.SetColor(playableCard.EligibleForGemBonus(GemType.Green) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            else if (info.GetExtendedPropertyAsBool(ORANGE_CELL_INVERSE) ?? false)
                            {
                                abilityIconInteractable.SetColor(playableCard.EligibleForGemBonus(GemType.Orange) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            else if (info.GetExtendedPropertyAsBool(BLUE_CELL_INVERSE) ?? false)
                            {
                                abilityIconInteractable.SetColor(playableCard.EligibleForGemBonus(GemType.Blue) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            else if (info.GetExtendedPropertyAsBool(ACTIVE_WHEN_FUELED) ?? false)
                            {
                                abilityIconInteractable.SetColor(playableCard.GetCurrentFuel() <= 0 ? info.colorOverride : renderInfo.defaultAbilityColor);
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
            if (!SaveManager.SaveFile.IsPart3)
                return;

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

        private static Ability[] _gemReRenderAbilities;
        private static Ability[] GemReRenderAbilities
        {
            get
            {
                _gemReRenderAbilities ??= AbilityManager.AllAbilityInfos.Where(ai => ai.IsGemReRender()).Select(ai => ai.ability).ToArray();
                return _gemReRenderAbilities;
            }
        }

        private static void ReRenderCards()
        {
            if (GameFlowManager.IsCardBattle)
            {
                foreach (CardSlot slot in BoardManager.Instance.AllSlotsCopy)
                {
                    if (slot.Card != null && (slot.Card.HasAnyOfAbilities(GemReRenderAbilities) || slot.Card.Info.appearanceBehaviour.Any(a => GemReRenderAppearances.Contains(a))))
                    {
                        slot.Card.RenderCard();
                        slot.Card.UpdateFaceUpOnBoardEffects();
                    }
                }
                foreach (PlayableCard card in PlayerHand.Instance.cardsInHand)
                {
                    if (card.HasAnyOfAbilities(GemReRenderAbilities) || card.Info.appearanceBehaviour.Any(a => GemReRenderAppearances.Contains(a)))
                        card.RenderCard();
                }
            }
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Instance.AddGem))]
        [HarmonyPostfix]
        private static IEnumerator GemsAddSwitch(IEnumerator sequence)
        {
            yield return sequence;

            if (SaveManager.SaveFile.IsPart3)
                ReRenderCards();

            yield break;
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Instance.LoseGem))]
        [HarmonyPostfix]
        private static IEnumerator GemsRemoveSwitch(IEnumerator sequence)
        {
            yield return sequence;

            if (SaveManager.SaveFile.IsPart3)
                ReRenderCards();

            yield break;
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Instance.ForceGemsUpdate))]
        [HarmonyPostfix]
        private static void GemsForceSwitch()
        {
            if (SaveManager.SaveFile.IsPart3)
                ReRenderCards();
        }
    }
}