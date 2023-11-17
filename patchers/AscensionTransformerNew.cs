using System;
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
        private static bool allBeastTransformersAssigned = false;
        private static List<CardInfo> allBeastTransformers = new();

        public static BeastInfoList beastInfoList = new();

        public class BeastInfo
        {
            public string ID { get; set; }
            public int HealthChange { get; set; }
            public int EnergyChange { get; set; }
            public BeastInfo Next { get; set; }

            public BeastInfo(string id, int healthChange, int energyChange)
            {
                ID = id;
                HealthChange = healthChange;
                EnergyChange = energyChange;
                Next = null;
            }
        }

        public class BeastInfoList
        {
            private BeastInfo head;
            private BeastInfo tail;

            public void Add(string id, int healthChange, int energyChange)
            {
                BeastInfo newNode = new(id, healthChange, energyChange);

                if (head == null)
                {
                    head = newNode;
                    tail = newNode;
                }
                else
                {
                    tail.Next = newNode;
                    tail = newNode;
                }
            }

            public BeastInfo GetNodeById(string id)
            {
                BeastInfo current = head;

                while (current != null)
                {
                    if (current.ID == id)
                    {
                        return current;
                    }

                    current = current.Next;
                }

                return null; // Node with the specified ID not found
            }


            public void Print()
            {
                BeastInfo current = head;

                while (current != null)
                {
                    Console.WriteLine($"ID: {current.ID}, Health Change: {current.HealthChange}, Energy Change: {current.EnergyChange}");
                    current = current.Next;
                }
            }
        }

        [HarmonyPatch(typeof(CreateTransformerSequencer))]
        [HarmonyPatch("ShowDetailsOnScreen")]
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

        [HarmonyPatch(typeof(CreateTransformerSequencer))]
        [HarmonyPatch("PreSelectionDialogueSequence")]
        [HarmonyPrefix]
        private static void PostfixAwake(CreateTransformerSequencer __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                // Iterate through each card that's tech and has the beast transformer metacategory
                foreach (CardInfo ci in CardLoader.AllData.Where(ci => ci.temple == CardTemple.Tech && ci.metaCategories.Contains(CustomCards.NewBeastTransformers)))
                {
                    __instance.beastCards.Add(ci);
                    Debug.Log(ci.name + " Added");
                }

                Debug.Log("CODE EXECUTED");
                Debug.Log(__instance.beastCards.Count);

                if (!allBeastTransformersAssigned)
                {
                    allBeastTransformers = new List<CardInfo>(__instance.beastCards); // Create a copy
                    allBeastTransformersAssigned = true;

                    // Modify the contents of allBeastTransformers as needed
                    // ...

                    // Example: Remove abilities from existing CardInfo objects in the list
                    //for (int i = 0; i < allBeastTransformers.Count; i++)
                    //{
                    //    allBeastTransformers[i].RemoveAbilities(DoubleSprint.AbilityID);
                    //}
                }

                List<int> uniqueIndexes = new();
                int maxIndex = allBeastTransformers.Count - 1;

                for (int i = 0; i < 3; i++)
                {
                    int index;
                    do
                    {
                        index = UnityEngine.Random.Range(0, maxIndex + 1);
                    } while (uniqueIndexes.Contains(index));

                    uniqueIndexes.Add(index);
                    Debug.Log("Index Added: " + index);
                    __instance.beastCards[i] = allBeastTransformers[index];
                }

                Debug.Log("BEAST CARDS ALL INFO");
                foreach (CardInfo info in __instance.beastCards)
                {
                    Debug.Log(info.name);
                }

                Debug.Log("BACKUP BEAST CARDS ALL INFO");
                foreach (CardInfo i in allBeastTransformers)
                {
                    Debug.Log(i.name);
                }
            }
        }

        [HarmonyPatch(typeof(CreateTransformerSequencer))]
        [HarmonyPatch("UpdateModChoices")]
        [HarmonyPrefix]
        private static bool PrefixUpdateModChoices(CreateTransformerSequencer __instance, CardInfo selectedCard)
        {
            //If P03 KCM is active, randomize the beast node
            if (P03AscensionSaveData.IsP03Run)
            {
                //if (__instance.beastMods == null)
                {
                    CardModificationInfo cardModificationInfo = new();
                    cardModificationInfo.abilities.Add(Ability.Transformer);
                    cardModificationInfo.transformerBeastCardId = __instance.beastCards[0].name;
                    CardModificationInfo cardModificationInfo2 = new();
                    cardModificationInfo2.abilities.Add(Ability.Transformer);
                    cardModificationInfo2.transformerBeastCardId = __instance.beastCards[1].name;
                    CardModificationInfo cardModificationInfo3 = new();
                    cardModificationInfo3.abilities.Add(Ability.Transformer);
                    cardModificationInfo3.transformerBeastCardId = __instance.beastCards[2].name;

                    //Energy adjustment
                    cardModificationInfo.energyCostAdjustment = getCardAdjustment(true, cardModificationInfo.transformerBeastCardId);
                    cardModificationInfo2.energyCostAdjustment = getCardAdjustment(true, cardModificationInfo2.transformerBeastCardId);
                    cardModificationInfo3.energyCostAdjustment = getCardAdjustment(true, cardModificationInfo3.transformerBeastCardId);

                    //Health adjustment
                    cardModificationInfo.healthAdjustment = getCardAdjustment(false, cardModificationInfo.transformerBeastCardId);
                    cardModificationInfo2.healthAdjustment = getCardAdjustment(false, cardModificationInfo2.transformerBeastCardId);
                    cardModificationInfo3.healthAdjustment = getCardAdjustment(false, cardModificationInfo3.transformerBeastCardId);

                    Debug.Log("CardMod1 Adjustments: " + cardModificationInfo.energyCostAdjustment + ", " + cardModificationInfo.healthAdjustment);
                    Debug.Log("CardMod2 Adjustments: " + cardModificationInfo2.energyCostAdjustment + ", " + cardModificationInfo2.healthAdjustment);
                    Debug.Log("CardMod3 Adjustments: " + cardModificationInfo3.energyCostAdjustment + ", " + cardModificationInfo3.healthAdjustment);

                    __instance.beastMods = new List<CardModificationInfo> { cardModificationInfo, cardModificationInfo2, cardModificationInfo3 };
                }
                __instance.currentValidModChoices = new List<CardModificationInfo>(__instance.beastMods);
                __instance.currentValidModChoices.RemoveAll(m => m.energyCostAdjustment + selectedCard.EnergyCost > 6);

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

            if (beastInfoList.GetNodeById(beastCardName) != null)
            {
                cardEnergyChange = beastInfoList.GetNodeById(beastCardName).EnergyChange;
                cardHealthChange = beastInfoList.GetNodeById(beastCardName).HealthChange;
            }
            else
            {
                Debug.Log("BEAST LIST NODE RETURNED NULL");
            }

            switch (beastCardName)
            {
                case "CXformerWolf":
                    {
                        cardEnergyChange = 1;
                        cardHealthChange = 0;
                        break;
                    }

                case "CXformerRaven":
                    {
                        cardEnergyChange = 0;
                        cardHealthChange = 0;
                        break;
                    }

                case "CXformerAdder":
                    {
                        cardEnergyChange = 0;
                        cardHealthChange = 1;
                        break;
                    }

                default:
                    break;
            }

            return energyChange ? cardEnergyChange : cardHealthChange;
        }
    }
}