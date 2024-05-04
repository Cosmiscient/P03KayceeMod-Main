using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
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
                            if (info.metaCategories.Contains(CustomCards.MultiverseAbility))
                            {
                                abilityIconInteractable.SetColor(Color.black);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.UpdateAbilityIcons))]
        [HarmonyPostfix]
        private static void RainbowMultiverse(CardAbilityIcons __instance, PlayableCard playableCard)
        {
            if (MultiverseBattleSequencer.Instance != null)
            {
                List<GameObject> defaultIconGroups = __instance.defaultIconGroups;
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf && group.transform.parent.gameObject.name.Contains("Invisible"))
                    {
                        //P03Plugin.Log.LogInfo($"Updating ability icon colors for group {group}");
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            // Create the dummy
                            // Look for a duplicate icon
                            string duplicateName = abilityIconInteractable.transform.parent.gameObject.name + "_" + abilityIconInteractable.gameObject.name + "_rainbow";
                            Transform existing = abilityIconInteractable.transform.Find(duplicateName);
                            //P03Plugin.Log.LogInfo($"Is there already a rainbow? {existing}");
                            Renderer rend = null;
                            if (existing.SafeIsUnityNull())
                            {
                                //P03Plugin.Log.LogInfo($"Creating rainbow");
                                GameObject duplicate = GameObject.Instantiate(abilityIconInteractable.gameObject, abilityIconInteractable.transform);
                                duplicate.name = duplicateName;
                                duplicate.transform.localPosition = new Vector3(0f, 0f, 0.0925f);
                                duplicate.transform.localScale = Vector3.one;
                                duplicate.SetActive(true);

                                AbilityIconInteractable dummyIcon = duplicate.GetComponent<AbilityIconInteractable>();
                                GameObject.DestroyImmediate(dummyIcon);

                                foreach (var collider in duplicate.GetComponents<Collider>())
                                    GameObject.DestroyImmediate(collider);
                                rend = duplicate.GetComponent<Renderer>();
                                rend.material.shader = Shader.Find("Standard");
                                rend.material.EnableKeyword("_EMISSION");
                            }
                            else
                            {
                                rend = existing.gameObject.GetComponent<Renderer>();
                            }
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            //P03Plugin.Log.LogInfo($"Icon {abilityIconInteractable.gameObject.name} {info.rulebookName} is multiverse? {info.metaCategories.Contains(CustomCards.MultiverseAbility)}");
                            if (info.metaCategories.Contains(CustomCards.MultiverseAbility) && !abilityIconInteractable.gameObject.name.Contains("rainbow"))
                            {
                                var texture = abilityIconInteractable.LoadIcon(null, info, (playableCard?.OpponentCard).GetValueOrDefault(false));
                                rend.material.mainTexture = texture;
                                rend.enabled = true;
                                rend.gameObject.SetActive(true);
                                Tween.Value(0f, 100f, (float v) => rend.material.SetColor("_EmissionColor", RareDiscCardAppearance.GetLinearRGBGradient(v)), 3.5f, 0f, loop: Tween.LoopType.Loop);
                            }
                            else
                            {
                                rend.enabled = false;
                                rend.gameObject.SetActive(false);
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
                        //P03Plugin.Log.LogInfo($"Updating ability icon colors for group {group}");
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
                    if (slot.Card != null && (slot.Card.HasAnyOfAbilities(GemReRenderAbilities) || slot.Card.Info.appearanceBehaviour.Contains(ReplicaAppearanceBehavior.ID)))
                    {
                        slot.Card.RenderCard();
                        slot.Card.UpdateFaceUpOnBoardEffects();
                    }
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