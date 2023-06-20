using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public class AscensionTransformerNew
    {
        private static bool allBeastTransformersAssigned = false;
        private static List<CardInfo> allBeastTransformers = new List<CardInfo>();

        [HarmonyPatch(typeof(CreateTransformerSequencer))]
        [HarmonyPatch("ShowDetailsOnScreen")]
        [HarmonyPrefix]
        private static void HealthAndCostChangesDisplay(CreateTransformerSequencer __instance)
        {
            if (SaveFile.IsAscension)
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
            if (SaveFile.IsAscension)
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

                List<int> uniqueIndexes = new List<int>();
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
        private static void PrefixUpdateModChoices(CreateTransformerSequencer __instance)
        {
            //If P03 KCM is active, randomize the beast node
            if (SaveFile.IsAscension)
            {
                if (__instance.beastMods == null)
                {
                    CardModificationInfo cardModificationInfo = new CardModificationInfo();
                    cardModificationInfo.abilities.Add(Ability.Transformer);
                    cardModificationInfo.transformerBeastCardId = __instance.beastCards[0].name;
                    CardModificationInfo cardModificationInfo2 = new CardModificationInfo();
                    cardModificationInfo2.abilities.Add(Ability.Transformer);
                    cardModificationInfo2.transformerBeastCardId = __instance.beastCards[1].name;
                    CardModificationInfo cardModificationInfo3 = new CardModificationInfo();
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

                return;
            }
        }

        private static int getCardAdjustment(bool energyChange, string beastCardName)
        {
            int cardEnergyChange = 0;
            int cardHealthChange = 0;

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

                case "P03KCM_RoboRiverSnapper":
                    {
                        cardEnergyChange = 1;
                        cardHealthChange = 4;
                        break; 
                    }

                case "P03KCM_RoboMole":
                    {
                        cardEnergyChange = 1;
                        cardHealthChange = 2;
                        break;
                    }

                case "P03KCM_RoboRabbit":
                    {
                        cardEnergyChange = -2;
                        cardHealthChange = 0;
                        break;
                    }
            }

            if (energyChange)
            {
                return cardEnergyChange;
            }
            else
            return cardHealthChange;
        }
    }
}