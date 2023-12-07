using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public class AscensionTransformerNew
    {
        private static readonly bool allBeastTransformersAssigned = false;
        private static readonly List<CardInfo> allBeastTransformers = new();

        internal static readonly List<BeastInfo> beastInfoList = new() {
            new ("CXformerWolf", 0, 1),
            new ("CXformerRaven", 0, 1),
            new ("CXformerAdder", 1, 0),
        };

        public class BeastInfo
        {
            public string ID { get; set; }
            public int HealthChange { get; set; }
            public int EnergyChange { get; set; }

            public BeastInfo(string id, int healthChange, int energyChange)
            {
                ID = id;
                HealthChange = healthChange;
                EnergyChange = energyChange;
            }
        }

        [HarmonyPatch(typeof(CreateTransformerSequencer), nameof(CreateTransformerSequencer.ShowDetailsOnScreen))]
        [HarmonyPrefix]
        private static void HealthAndCostChangesDisplay(CreateTransformerSequencer __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                P03AddModFace component = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AddBeastMod, showEffects: false).GetComponent<P03AddModFace>();
                component.DisplayCardWithMod(__instance.selectedCard.Info, __instance.currentValidModChoices[__instance.currentModIndex]);
                component.GetComponent<P03AddBeastModFace>().SetBeastPortrait(__instance.beastCards[__instance.currentModIndex].portraitTex);

                return;
            }
        }

        [HarmonyPatch(typeof(CreateTransformerSequencer), nameof(CreateTransformerSequencer.DisplayMod))]
        [HarmonyPrefix]
        private static bool DisplayCorrectMod(CreateTransformerSequencer __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                CardModificationInfo adjustMod = new();
                int healthAdjustment = getCardAdjustment(false, __instance.beastCards[__instance.currentModIndex].name);
                int energyAdjustment = getCardAdjustment(true, __instance.beastCards[__instance.currentModIndex].name);
                adjustMod.energyCostAdjustment = __instance.selectedCard.Info.EnergyCost + energyAdjustment - __instance.beastCards[__instance.currentModIndex].EnergyCost;
                adjustMod.healthAdjustment = __instance.selectedCard.Info.Health + healthAdjustment - __instance.beastCards[__instance.currentModIndex].Health;
                __instance.cardFaceDisplayer.CardDisplayer.DisplayCard(__instance.beastCards[__instance.currentModIndex], adjustMod, false);
                __instance.cardFaceDisplayer.LeftDecal.sprite = __instance.cardFaceDisplayer.RightDecal.sprite = __instance.beastDecals[__instance.currentModIndex];

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CreateTransformerSequencer), nameof(CreateTransformerSequencer.PreSelectionDialogueSequence))]
        [HarmonyPrefix]
        private static void PostfixAwake(CreateTransformerSequencer __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __instance.beastCards = new();
                // // Iterate through each card that's tech and has the beast transformer metacategory
                // foreach (CardInfo ci in CardLoader.AllData.Where(ci => ci.temple == CardTemple.Tech && ci.metaCategories.Contains(CustomCards.NewBeastTransformers)))
                // {
                //     __instance.beastCards.Add(ci);
                //     Debug.Log(ci.name + " Added");
                // }

                // Debug.Log("CODE EXECUTED");
                // Debug.Log(__instance.beastCards.Count);

                // if (!allBeastTransformersAssigned)
                // {
                //     allBeastTransformers = new List<CardInfo>(__instance.beastCards); // Create a copy
                //     allBeastTransformersAssigned = true;

                //     // Modify the contents of allBeastTransformers as needed
                //     // ...

                //     // Example: Remove abilities from existing CardInfo objects in the list
                //     //for (int i = 0; i < allBeastTransformers.Count; i++)
                //     //{
                //     //    allBeastTransformers[i].RemoveAbilities(DoubleSprint.AbilityID);
                //     //}
                // }

                // List<int> uniqueIndexes = new();
                // int maxIndex = allBeastTransformers.Count - 1;

                // for (int i = 0; i < 3; i++)
                // {
                //     int index;
                //     do
                //     {
                //         index = Random.Range(0, maxIndex + 1);
                //     } while (uniqueIndexes.Contains(index));

                //     uniqueIndexes.Add(index);
                //     Debug.Log("Index Added: " + index);
                //     __instance.beastCards[i] = allBeastTransformers[index];
                // }

                // Debug.Log("BEAST CARDS ALL INFO");
                // foreach (CardInfo info in __instance.beastCards)
                // {
                //     Debug.Log(info.name);
                // }

                // Debug.Log("BACKUP BEAST CARDS ALL INFO");
                // foreach (CardInfo i in allBeastTransformers)
                // {
                //     Debug.Log(i.name);
                // }
            }
        }

        [HarmonyPatch(typeof(CreateTransformerSequencer), nameof(CreateTransformerSequencer.UpdateModChoices))]
        [HarmonyPrefix]
        private static bool PrefixUpdateModChoices(CreateTransformerSequencer __instance, CardInfo selectedCard)
        {
            //If P03 KCM is active, randomize the beast node
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.beastCards != null || __instance.beastCards.Count == 0)
                {
                    __instance.beastCards = new();
                    List<CardInfo> possibles = CardLoader.AllData.Where(ci =>
                        ci.temple == CardTemple.Tech
                        && ci.metaCategories.Contains(CustomCards.NewBeastTransformers)
                        && getCardAdjustment(true, ci.name) + selectedCard.EnergyCost <= 6
                    ).ToList();
                    int randomseed = P03AscensionSaveData.RandomSeed + 150;
                    for (int i = 0; i < 3; i++)
                    {
                        int idx = SeededRandom.Range(0, possibles.Count, randomseed++);
                        __instance.beastCards.Add(possibles[idx]);
                        possibles.RemoveAt(idx);
                    }
                }

                __instance.beastMods = __instance.beastCards.Take(3).Select(bc => new CardModificationInfo()
                {
                    abilities = new() { Ability.Transformer },
                    transformerBeastCardId = bc.name,
                    energyCostAdjustment = getCardAdjustment(true, bc.name),
                    healthAdjustment = getCardAdjustment(false, bc.name)
                }).ToList();

                __instance.currentValidModChoices = new List<CardModificationInfo>(__instance.beastMods.Where(m => m.energyCostAdjustment + selectedCard.EnergyCost <= 6));

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.ModifyCard))]
        [HarmonyPrefix]
        internal static bool SplitTransformerMods(DeckInfo __instance, CardInfo card, CardModificationInfo mod)
        {
            if (mod.HasAbility(Ability.Transformer))
            {
                if (mod.healthAdjustment != 0 || mod.attackAdjustment != 0 || mod.energyCostAdjustment != 0)
                {
                    // Split the mod into two mods, where the mod effects that would stack are not copyable
                    CardModificationInfo newMod = new()
                    {
                        healthAdjustment = mod.healthAdjustment,
                        attackAdjustment = mod.attackAdjustment,
                        energyCostAdjustment = mod.energyCostAdjustment,
                        nonCopyable = true
                    };

                    mod.healthAdjustment = 0;
                    mod.attackAdjustment = 0;
                    mod.energyCostAdjustment = 0;

                    card.Mods.Add(mod);
                    card.Mods.Add(newMod);

                    __instance.UpdateModDictionary();
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(Transformer), nameof(Transformer.GetTransformCardInfo))]
        [HarmonyPostfix]
        private static void ReverseStatsFromCopyableMods(ref Transformer __instance, ref CardInfo __result)
        {
            CardModificationInfo statReversingMod = null;
            foreach (CardModificationInfo myMod in __instance.Card.Info.Mods.Where(m => m != null && !m.nonCopyable))
            {
                foreach (CardModificationInfo targetMod in __result.Mods.Where(m => m != null && !m.nonCopyable))
                {
                    if (myMod.attackAdjustment == targetMod.attackAdjustment
                        && myMod.healthAdjustment == targetMod.healthAdjustment
                        && myMod.energyCostAdjustment == targetMod.energyCostAdjustment)
                    {
                        statReversingMod ??= new();
                        statReversingMod.attackAdjustment -= targetMod.attackAdjustment;
                        statReversingMod.healthAdjustment -= targetMod.healthAdjustment;
                        statReversingMod.energyCostAdjustment -= targetMod.energyCostAdjustment;
                    }
                }
            }

            if (statReversingMod != null)
                __result.Mods.Add(statReversingMod);
        }

        [HarmonyPatch(typeof(Transformer), nameof(Transformer.GetTransformCardInfo))]
        [HarmonyPrefix]
        private static bool FixCXFormer(ref Transformer __instance, ref CardInfo __result)
        {
            if (__instance.Card.Info.name.Contains("CXformer"))
            {
                __result = __instance.Card.Info.evolveParams.evolution.Clone() as CardInfo;
                return false;
            }

            return true;
        }

        private static int getCardAdjustment(bool energyChange, string beastCardName)
        {
            int cardEnergyChange = 0;
            int cardHealthChange = 0;
            BeastInfo registeredBeast = beastInfoList.FirstOrDefault(bi => bi.ID.Equals(beastCardName, System.StringComparison.OrdinalIgnoreCase));

            if (registeredBeast != null)
            {
                cardEnergyChange = registeredBeast.EnergyChange;
                cardHealthChange = registeredBeast.HealthChange;
            }
            else
            {
                Debug.Log("BEAST LIST NODE RETURNED NULL");
            }

            return energyChange ? cardEnergyChange : cardHealthChange;
        }
    }
}