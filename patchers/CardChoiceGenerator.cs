using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Sequences;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class CardChoiceGenerator
    {

        public static int CurrentRerollCost
        {
            get
            {
                int val = P03AscensionSaveData.RunStateData.GetValueAsInt(P03Plugin.PluginGuid, "CurrentRerollCost");
                if (val == 0)
                    CurrentRerollCost = 1;
                return 1;
            }
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "CurrentRerollCost", value);
        }

        public static int PriorRerollCost
        {
            get
            {
                int val = P03AscensionSaveData.RunStateData.GetValueAsInt(P03Plugin.PluginGuid, "PriorRerollCost");
                if (val == 0)
                    PriorRerollCost = 1;
                return 1;
            }
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "PriorRerollCost", value);
        }

        public class Part3RareCardChoicesNodeData : CardChoicesNodeData { }

        private static readonly Dictionary<RunBasedHoloMap.Zone, CardMetaCategory> selectionCategories = new()
        {
            { RunBasedHoloMap.Zone.Neutral, CustomCards.NeutralRegion },
            { RunBasedHoloMap.Zone.Magic, CustomCards.WizardRegion },
            { RunBasedHoloMap.Zone.Undead, CustomCards.UndeadRegion },
            { RunBasedHoloMap.Zone.Nature, CustomCards.NatureRegion },
            { RunBasedHoloMap.Zone.Tech, CustomCards.TechRegion }
        };

        private static string GetNameContribution(CardInfo card)
        {
            string[] nameSplit = card.displayedName.Split(' ', '-');
            return nameSplit.Length > 1
                ? nameSplit[0].ToLowerInvariant().Contains("gem") ? nameSplit[1] : nameSplit[0]
                : card.displayedName.Contains("bot") ? card.displayedName.Replace("bot", "") : card.displayedName;
        }

        private static CardInfo GenerateMycoCard(int randomSeed)
        {
            List<CardInfo> allCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => TradeChipsSequencer.IsValidDraftCard(x) && !x.metaCategories.Contains(CardMetaCategory.Rare)).ToList();
            // Remove any cards that do nothing
            allCards.RemoveAll(ci => ci.Attack == 1 && ci.Health == 1 && ci.Abilities.Count == 0);

            CardInfo left = allCards[SeededRandom.Range(0, allCards.Count, randomSeed++)];
            allCards.Remove(left);

            // Make them a little more interesting at this point
            // Remove all cards from the other side of the pool that have an ability in common
            // Reduce the number of duds
            allCards.RemoveAll(ci => ci.Abilities.Any(a => left.Abilities.Contains(a)));

            CardInfo right = allCards[SeededRandom.Range(0, allCards.Count, randomSeed++)];

            string name = GetNameContribution(left) + "-" + GetNameContribution(right);

            int health = Math.Max(left.Health, right.Health);
            int attack = Math.Max(left.Attack, right.Attack);
            List<Ability> abilities = new();
            abilities.AddRange(left.Abilities);
            abilities.AddRange(right.Abilities);
            int energyCost = Mathf.FloorToInt((left.energyCost + right.energyCost) / 2f);

            CardModificationInfo mod = new()
            {
                nonCopyable = true,
                abilities = abilities,
                healthAdjustment = health,
                attackAdjustment = attack,
                energyCostAdjustment = energyCost,
                nameReplacement = name,
                gemify = left.Gemified || right.Gemified
            };
            if (mod.abilities.Contains(Ability.Transformer))
            {
                if (left.evolveParams != null)
                    mod.transformerBeastCardId = left.evolveParams.evolution.name;
                else if (right.evolveParams != null)
                    mod.transformerBeastCardId = right.evolveParams.evolution.name;
            }

            CardInfo retval = CardLoader.GetCardByName(CustomCards.FAILED_EXPERIMENT_BASE);
            (retval.mods ??= new()).Add(mod);
            return retval;
        }

        [HarmonyPatch(typeof(Part3CardChoiceGenerator), nameof(Part3CardChoiceGenerator.GenerateChoices))]
        [HarmonyPrefix]
        public static bool AscensionChoiceGeneration(CardChoicesNodeData data, int randomSeed, ref List<CardChoice> __result)
        {
            if (P03AscensionSaveData.IsP03Run && HoloMapAreaManager.Instance != null)
            {
                RunBasedHoloMap.Zone region = RunBasedHoloMap.GetRegionCodeFromWorldID(HoloMapAreaManager.Instance.CurrentWorld.name);

                __result = new();

                if (region == RunBasedHoloMap.Zone.Mycologist)
                {
                    //int newRandomSeed = P03AscensionSaveData.RandomSeed;
                    for (int i = 0; i < 3; i++)
                        __result.Add(new() { CardInfo = GenerateMycoCard(randomSeed + (100 * i)) });

                    return false;
                }

                // We need one card specific to the region and two cards belonging to the neutral or specific region
                Predicate<IEnumerable<CardMetaCategory>> rareMatcher = data is Part3RareCardChoicesNodeData ? x => x.Any(m => m == CardMetaCategory.Rare) : x => !x.Any(m => m == CardMetaCategory.Rare);

                // Don't allow rares
                List<CardInfo> regionCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => x.metaCategories.Contains(selectionCategories[region]) && rareMatcher(x.metaCategories));
                List<CardInfo> regionAndNeutralCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => (x.metaCategories.Contains(selectionCategories[region]) || x.metaCategories.Contains(CustomCards.NeutralRegion)) && rareMatcher(x.metaCategories));

                if (regionCards.Count > 0)
                {
                    CardInfo newCard = regionCards[SeededRandom.Range(0, regionCards.Count, randomSeed++)];
                    //__result.Add(new CardChoice() { CardInfo = CustomCards.ModifyCardForAscension(newCard) });
                    __result.Add(new CardChoice() { CardInfo = CardLoader.Clone(newCard) });
                    regionCards.Remove(newCard);
                    regionAndNeutralCards.Remove(newCard);
                }

                // 50% chance that the second card also comes from the region
                if (SeededRandom.Bool(randomSeed++))
                {
                    if (regionCards.Count > 0)
                    {
                        CardInfo newCard = regionCards[SeededRandom.Range(0, regionCards.Count, randomSeed++)];
                        //__result.Add(new CardChoice() { CardInfo = CustomCards.ModifyCardForAscension(newCard) });
                        __result.Add(new CardChoice() { CardInfo = CardLoader.Clone(newCard) });
                        regionAndNeutralCards.Remove(newCard);
                    }
                }

                while (__result.Count < 3)
                {
                    CardInfo newCard = regionAndNeutralCards[SeededRandom.Range(0, regionAndNeutralCards.Count, randomSeed++)];
                    //__result.Add(new CardChoice() { CardInfo = CustomCards.ModifyCardForAscension(newCard) });
                    __result.Add(new CardChoice() { CardInfo = CardLoader.Clone(newCard) });
                    regionAndNeutralCards.Remove(newCard);
                }

                //                InfiniscryptionP03Plugin.Log.LogInfo($"I selected the following cards for region {region}: {string.Join(",", __result.Select(c => c.CardInfo.name))}");

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.CardSelectionSequence))]
        [HarmonyPostfix]
        private static IEnumerator EnsureHoloClover(IEnumerator sequence, CardSingleChoicesSequencer __instance, SpecialNodeData nodeData)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            if (__instance.rerollInteractable == null)
            {
                GameObject cardClicker = UnityEngine.Object.Instantiate(RunBasedHoloMap.SpecialNodePrefabs[HoloMapNode.NodeDataType.CardChoice], __instance.transform);
                OnboardDynamicHoloPortrait.HolofyGameObject(cardClicker, GameColors.Instance.gold);
                cardClicker.transform.Find("RendererParent").localEulerAngles = new(60f, 180f, 180f);
                cardClicker.transform.localPosition = new(0f, 5f, -1.5f);
                cardClicker.transform.localScale = new(1f, 1f, 1f);
                UnityEngine.Object.Destroy(cardClicker.GetComponentInChildren<SineWaveMovement>());
                UnityEngine.Object.Destroy(cardClicker.GetComponentInChildren<HoloMapNode>());

                // Label
                GameObject sampleObject = RunBasedHoloMap.SpecialNodePrefabs[HoloMapNode.NodeDataType.BuildACard].transform.Find("HoloFloatingLabel").gameObject;
                GameObject labelObject = UnityEngine.Object.Instantiate(sampleObject, __instance.transform);
                labelObject.transform.localPosition = new(-1.2f, 5.8f, -2.3f);
                labelObject.transform.localEulerAngles = new(90f, 0f, 0f);
                HoloFloatingLabel label = labelObject.GetComponent<HoloFloatingLabel>();
                label.line.gameObject.SetActive(false);
                label.line = null;

                GenericMainInputInteractable mii = cardClicker.AddComponent<GenericMainInputInteractable>();
                mii.SetCursorType(CursorType.Pickup);
                mii.CursorEntered = delegate (MainInputInteractable mii)
                {
                    AudioController.Instance.PlaySound2D("holomap_node_mouseover", MixerGroup.TableObjectsSFX, 0.5f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), null, new AudioParams.Randomization(true), null, false);
                    label.gameObject.SetActive(true);
                    label.SetText(Localization.Translate($"Redraw: ${CurrentRerollCost}"));
                };

                mii.CursorExited = (mii) => label.gameObject.SetActive(false);

                mii.CursorSelectEnded = delegate (MainInputInteractable mii)
                {
                    if (Part3SaveData.Data.currency < CurrentRerollCost)
                    {
                        Tween.Shake(cardClicker.transform, cardClicker.transform.position, Vector3.one * 0.1f, 0.33f, 0f);
                        AudioController.Instance.PlaySound2D("holomap_drop_currency", MixerGroup.TableObjectsSFX, 0.5f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), null, new AudioParams.Randomization(true), null, false);
                        return;
                    }

                    label.gameObject.SetActive(false);
                    Part3SaveData.Data.currency -= CurrentRerollCost;
                    int prev = PriorRerollCost;
                    PriorRerollCost = CurrentRerollCost;
                    CurrentRerollCost += prev;
                    __instance.OnRerollChoices();
                };

                __instance.rerollInteractable = mii;
            }

            if (nodeData is Part3RareCardChoicesNodeData)
            {
                MainInputInteractable reroller = __instance.rerollInteractable;
                __instance.rerollInteractable.gameObject.SetActive(false);
                __instance.GetComponentInChildren<HoloFloatingLabel>()?.gameObject.SetActive(false);
                __instance.rerollInteractable = null;
                yield return sequence;
                __instance.rerollInteractable = reroller;
            }
            else
            {
                __instance.rerollInteractable.gameObject.SetActive(true);
                yield return sequence;
            }
            __instance.rerollInteractable.gameObject.SetActive(false);
            __instance.GetComponentInChildren<HoloFloatingLabel>()?.gameObject.SetActive(false);

            yield break;
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.OnCursorEnterRerollInteractable))]
        [HarmonyPrefix]
        private static bool AllowNullAnimReroll(CardSingleChoicesSequencer __instance) => __instance.rerollAnim != null;

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.CleanUpRerollItem))]
        [HarmonyPrefix]
        private static bool AllowNullAnimCleanup(CardSingleChoicesSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (__instance.rerollInteractable != null && __instance.rerollInteractable.gameObject.activeSelf)
            {
                __instance.rerollInteractable.SetEnabled(false);

                if (__instance.rerollAnim == null)
                {
                    Tween.LocalPosition(
                        __instance.rerollInteractable.transform,
                        __instance.rerollInteractable.transform.localPosition + Vector3.down,
                        0.25f,
                        0f
                    );
                }
                else
                {
                    __instance.rerollAnim.Play("exit", 0, 0f);
                }

                CustomCoroutine.WaitThenExecute(0.25f, delegate
                {
                    __instance.rerollInteractable.gameObject.SetActive(false);
                    if (__instance.rerollAnim == null)
                        __instance.rerollInteractable.transform.localPosition -= Vector3.down;
                }, false);
            }
            return false;
        }
    }
}