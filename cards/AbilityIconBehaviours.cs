using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public const string ORANGE_CELL = "FromOrange";
        public const string GREEN_CELL = "FromGreen";
        public const string BLUE_CELL = "FromBlue";
        public const string GEM_CELL = "FromGem";
        public const string ORANGE_CELL_INVERSE = "FromOrangeInverse";
        public const string GREEN_CELL_INVERSE = "FromEmeraldInverse";
        public const string BLUE_CELL_INVERSE = "FromSapphireInverse";
        public const string GEM_CELL_INVERSE = "FromGemInverse";

        public static bool IsGemReRender(this AbilityInfo info)
        {
            return info.GetExtendedPropertyAsBool(ORANGE_CELL).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(GREEN_CELL).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(BLUE_CELL).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(GEM_CELL).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(ORANGE_CELL_INVERSE).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(GREEN_CELL_INVERSE).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(BLUE_CELL_INVERSE).GetValueOrDefault(false)
                || info.GetExtendedPropertyAsBool(GEM_CELL_INVERSE).GetValueOrDefault(false);
        }

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
        private static void RecolorGemIcons(CardDisplayer3D __instance, PlayableCard playableCard, CardRenderInfo renderInfo)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                List<GameObject> defaultIconGroups = __instance.AbilityIcons.defaultIconGroups;
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf)
                    {
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            if (info.GetExtendedPropertyAsBool(GREEN_CELL).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(!playableCard.EligibleForGemBonus(GemType.Green) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            if (info.GetExtendedPropertyAsBool(ORANGE_CELL).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(!playableCard.EligibleForGemBonus(GemType.Orange) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            if (info.GetExtendedPropertyAsBool(BLUE_CELL).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(!playableCard.EligibleForGemBonus(GemType.Blue) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            if (info.GetExtendedPropertyAsBool(GREEN_CELL_INVERSE).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(playableCard.EligibleForGemBonus(GemType.Green) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            if (info.GetExtendedPropertyAsBool(ORANGE_CELL_INVERSE).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(playableCard.EligibleForGemBonus(GemType.Orange) ? info.colorOverride : renderInfo.defaultAbilityColor);
                            }
                            if (info.GetExtendedPropertyAsBool(BLUE_CELL_INVERSE).GetValueOrDefault(false))
                            {
                                abilityIconInteractable.SetColor(playableCard.EligibleForGemBonus(GemType.Blue) ? info.colorOverride : renderInfo.defaultAbilityColor);
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
                    if (slot.Card != null && (slot.Card.HasAnyOfAbilities(GemReRenderAbilities) || slot.Card.Info.appearanceBehaviour.Contains(ReplicaAppearanceBehavior.ID)))
                        slot.Card.RenderCard();
                }
                foreach (PlayableCard card in PlayerHand.Instance.cardsInHand)
                {
                    if (card.HasAnyOfAbilities(GemReRenderAbilities) || card.Info.appearanceBehaviour.Contains(ReplicaAppearanceBehavior.ID))
                        card.RenderCard();
                }
            }
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Instance.AddGem))]
        [HarmonyPostfix]
        private static IEnumerator GemsAddSwitch(IEnumerator sequence)
        {
            yield return sequence;
            ReRenderCards();
            yield break;
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Instance.LoseGem))]
        [HarmonyPostfix]
        private static IEnumerator GemsRemoveSwitch(IEnumerator sequence)
        {
            yield return sequence;
            ReRenderCards();
            yield break;
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Instance.ForceGemsUpdate))]
        [HarmonyPostfix]
        private static void GemsForceSwitch() => ReRenderCards();
    }
}