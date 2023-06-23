using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;
using InscryptionAPI.Guid;
using Infiniscryption.P03KayceeRun.Sequences;
using System.Collections;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun.Quests
{
    [HarmonyPatch]
    internal static class DefaultQuestDefinitions
    {
        internal static QuestDefinition TippedScales { get; private set; }
        internal static QuestDefinition WhiteFlag { get; private set; }
        internal static QuestDefinition DeckSize { get; private set; }
        internal static QuestDefinition Smuggler { get; private set; }
        internal static QuestDefinition SmugglerPartTwo { get; private set; }
        internal static QuestDefinition Donation { get; private set; }
        internal static QuestDefinition DonationPartTwo { get; private set; }
        internal static QuestDefinition FullyUpgraded { get; private set; }
        internal static QuestDefinition ILoveBones { get; private set; }
        internal static QuestDefinition ListenToTheRadio { get; private set; }
        internal static QuestDefinition PowerUpTheTower { get; private set; }

        // These are the special story maps
        internal static QuestDefinition FindGoobert { get; private set; }
        internal static QuestDefinition Prospector { get; private set; }
        internal static QuestDefinition BrokenGenerator { get; private set; }

        public const int RADIO_TURNS = 5;
        public const int POWER_TURNS = 4;

        internal static int DeckSizeTarget
        {
            get
            {
                int retval = ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "DeckSizeTarget");
                if (retval > 0)
                    return retval;

                retval = Part3SaveData.Data.deck.Cards.Count + 2;
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "DeckSizeTarget", retval);
                return retval;
            }
        }

        internal static RunBasedHoloMap.Zone GoobertDropoffZone
        {
            get
            {
                string dropoff = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "Dropoff");
                if (!string.IsNullOrEmpty(dropoff))
                    return (RunBasedHoloMap.Zone)Enum.Parse(typeof(RunBasedHoloMap.Zone), dropoff);

                List<RunBasedHoloMap.Zone> zones = new () 
                { 
                    RunBasedHoloMap.Zone.Magic, RunBasedHoloMap.Zone.Nature, RunBasedHoloMap.Zone.Tech, RunBasedHoloMap.Zone.Undead
                };
                zones.Remove(EventManagement.CurrentZone);
                RunBasedHoloMap.Zone assignedDropoff = zones[SeededRandom.Range(0, zones.Count, P03AscensionSaveData.RandomSeed)];
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "Dropoff", assignedDropoff);
                return assignedDropoff;
            }
        }

        internal static void DefineAllQuests()
        {
            // GoobertQuest = 1,
            // ProspectorQuest = 2,
            // BrokenGeneratorQuest = 10,

            // Tipped Scales
            TippedScales = QuestManager.Add(P03Plugin.PluginGuid, "Tipped Scales")
                                .SetGenerateCondition(() => EventManagement.CompletedZones.Count <= 2);
            TippedScales.AddDialogueState("TOO EASY...", "P03TooEasyQuest")
                        .AddDialogueState("TOO EASY...", "P03TooEasyAccepted")
                        .AddDefaultActiveState("KEEP GOING...", "P03TooEasyInProgress", threshold:5)
                        .AddDialogueState("IMPRESSIVE...", "P03TooEasyComplete")
                        .AddGainAbilitiesReward(1, Ability.DrawCopyOnDeath);

            // White Flag
            WhiteFlag = QuestManager.Add(P03Plugin.PluginGuid, "White Flag");
            WhiteFlag.AddDialogueState("DO THEY GIVE UP?", "P03WhiteFlagSetup")
                     .AddDefaultActiveState("DO THEY GIVE UP?", "P03WhiteFlagSetup", threshold:1)
                     .AddDialogueState("THEY DO GIVE UP", "P03WhiteFlagReward")
                     .AddGainCardReward(CustomCards.UNC_TOKEN);

            // Deck Size
            DeckSize = QuestManager.Add(P03Plugin.PluginGuid, "Deck Size");
            DeckSize.AddDialogueState("ITS TOO SMALL", "P03DeckSizeSetup")
                    .AddSuccessAction(() => { var dummy = DeckSizeTarget; }) // this sets the initial decksize target
                    .AddNamedState("waiting", "ITS TOO SMALL", "P03DeckSizeSetup")
                    .SetDynamicStatus(() => {
                        if (Part3SaveData.Data.deck.Cards.Count >= DeckSizeTarget)
                            return QuestState.QuestStateStatus.Success;
                        return QuestState.QuestStateStatus.Active;
                    })
                    .AddDialogueState("ITS JUST RIGHT", "P03DeckSizeReward")
                    .AddDynamicMonetaryReward();

            // Smuggler
            Smuggler = QuestManager.Add(P03Plugin.PluginGuid, "Smuggler");
            Smuggler.AddDialogueState("SSH - COME OVER HERE", "P03SmugglerSetup")
                    .AddDialogueState("LETS DO THIS", "P03SmugglerAccepted")
                    .AddGainCardReward(CustomCards.CONTRABAND);

            SmugglerPartTwo = QuestManager.Add(P03Plugin.PluginGuid, "SmugglerPartTwo").SetPriorQuest(Smuggler);
            var smugglerDummyState = SmugglerPartTwo.AddDummyStartingState(() => Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.CONTRABAND)));
            smugglerDummyState.SetNextState(QuestState.QuestStateStatus.Success, "SSH - BRING IT HERE", "P03SmugglerComplete", autoComplete:true)
                              .AddLoseCardReward(CustomCards.CONTRABAND)
                              .AddGainCardReward(CustomCards.UNC_TOKEN);
            smugglerDummyState.SetNextState(QuestState.QuestStateStatus.Failure, "WHERE DID IT GO?", "P03SmugglerFailed", autoComplete:true);

            // Donation
            Donation = QuestManager.Add(P03Plugin.PluginGuid, "Donation");
            Donation.AddDialogueState("SPARE SOME CASH?", "P03DonationIntro")
                    .AddNamedState("CheckingForAvailableCash", "SPARE SOME CASH?", "P03DonationNotEnough")
                    .SetDynamicStatus(() => {
                        if (Part3SaveData.Data.currency > 10)
                            return QuestState.QuestStateStatus.Success;
                        else
                            return QuestState.QuestStateStatus.Active;
                    })
                    .AddDialogueState("SPARE SOME CASH?", "P03DonationComplete")
                    .AddMonetaryReward(-10);

            DonationPartTwo = QuestManager.Add(P03Plugin.PluginGuid, "DonationPartTwo").SetPriorQuest(Donation);
            DonationPartTwo.AddDialogueState("THANK YOU!", "P03DonationReward").AddGemifyCardsReward(2);

            // Fully Upgraded
            FullyUpgraded = QuestManager.Add(P03Plugin.PluginGuid, "Fully Upgraded")
                .SetGenerateCondition(() => EventManagement.CompletedZones.Count >= 3); // Can only happen if you've finished 3 maps
            
            FullyUpgraded.AddDialogueState("SHOW ME POWER", "P03FullyUpgradedFail")
                         .SetDynamicStatus(() => {
                            if (Part3SaveData.Data.deck.Cards.Any(c =>
                                c.HasAbility(Ability.Transformer) &&
                                c.HasAbility(NewPermaDeath.AbilityID) &&
                                c.Gemified
                            ))
                                return QuestState.QuestStateStatus.Success;
                            else
                                return QuestState.QuestStateStatus.Active;
                         })
                         .AddDialogueState("SHOW ME POWER", "P03FullyUpgradedSuccess")
                         .AddDynamicMonetaryReward();

            // I Love Bones
            ILoveBones = QuestManager.Add(P03Plugin.PluginGuid, "I Love Bones");
            ILoveBones.SetRegionCondition(RunBasedHoloMap.Zone.Undead)
                      .AddDialogueState("I LOVE BONES!!", "P03ILoveBones")
                      .SetDynamicStatus(() => {
                        if (Part3SaveData.Data.deck.Cards.Where(
                            c => c.HasAbility(Ability.Brittle) || c.HasAbility(Ability.PermaDeath) || c.HasAbility(NewPermaDeath.AbilityID)
                        ).Count() >= 3)
                            return QuestState.QuestStateStatus.Success;
                        else
                            return QuestState.QuestStateStatus.Active;
                      })
                      .AddDialogueState("I LOVE BONES!!!!", "P03ILoveBonesSuccess")
                      .AddGainCardReward(CustomCards.SKELETON_LORD);

            // Radio Tower
            ListenToTheRadio = QuestManager.Add(P03Plugin.PluginGuid, "Listen To The Radio");

            var radioActiveState = ListenToTheRadio.AddDialogueState("LETS DO SCIENCE", "P03RadioQuestStart")
                            .AddDialogueState("LETS DO SCIENCE", "P03RadioQuestAccepted")
                            .AddGainCardReward(CustomCards.RADIO_TOWER)
                            .AddDefaultActiveState("LETS DO SCIENCE", "P03RadioQuestInProgress")
                            .SetDynamicStatus(() => {
                                if (ListenToTheRadio.GetQuestCounter() >= RADIO_TURNS)
                                    return QuestState.QuestStateStatus.Success;
                                else if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.RADIO_TOWER))
                                    return QuestState.QuestStateStatus.Failure;
                                else
                                    return QuestState.QuestStateStatus.Active;
                            });
            
            radioActiveState.AddDialogueState("YOU BROKE IT?!", "P03RadioQuestFailed", QuestState.QuestStateStatus.Failure);
            radioActiveState.AddDialogueState("A WIN FOR SCIENCE", "P03RadioQuestSucceeded")
                            .AddLoseCardReward(CustomCards.RADIO_TOWER)
                            .AddGainCardReward(CustomCards.UNC_TOKEN);

            // Power Up The Tower
            PowerUpTheTower = QuestManager.Add(P03Plugin.PluginGuid, "Power Up The Tower");
            
            var towerActiveState = PowerUpTheTower.AddDialogueState("LOOKING FOR A JOB?", "P03PowerQuestStart")
                            .AddDialogueState("LOOKING FOR A JOB?", "P03PowerQuestAccepted")
                            .AddGainCardReward(CustomCards.POWER_TOWER)
                            .AddDefaultActiveState("GET BACK TO WORK", "P03PowerQuestInProgress")
                            .SetDynamicStatus(() => {
                                if (PowerUpTheTower.GetQuestCounter() >= POWER_TURNS)
                                    return QuestState.QuestStateStatus.Success;
                                else if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.POWER_TOWER))
                                    return QuestState.QuestStateStatus.Failure;
                                else
                                    return QuestState.QuestStateStatus.Active;
                            });

            towerActiveState.AddDialogueState("YOU BROKE IT?!", "P03PowerQuestFailed", QuestState.QuestStateStatus.Failure);
            towerActiveState.AddDialogueState("HERE'S YOUR PAYMENT", "P03PowerQuestSucceeded")
                            .AddLoseCardReward(CustomCards.POWER_TOWER)
                            .AddDynamicMonetaryReward();

            // Goobert Quests
            FindGoobert = QuestManager.Add(P03Plugin.PluginGuid, "FindGoobert");
            QuestState waitingState = FindGoobert.AddDialogueState("MY FRIEND IS LOST", "P03WhereIsGoobert")
                            .AddNamedState("GoobertAvailable", "MY FRIEND IS LOST", "P03WhereIsGoobert");
                            
            // When you fail the default state (i.e., you don't buy goobert when you see him), you end up here
            waitingState.AddDialogueState("MY FRIEND IS LOST", "P03DidNotBuyGoobert", QuestState.QuestStateStatus.Failure);

            // When you buy goobert, you end up here
            QuestState boughtState = waitingState.AddNamedState("CarryingGoobert", "YOU FOUND HIM!", $"P03FoundGoobert{GoobertDropoffZone.ToString()}");
            boughtState.SetDynamicStatus(() => {
                if (!Part3SaveData.Data.items.Any(item => GoobertHuh.ItemData.name.ToLowerInvariant() == item.ToLowerInvariant()))
                    return QuestState.QuestStateStatus.Failure;
                if (EventManagement.CurrentZone == DefaultQuestDefinitions.GoobertDropoffZone)
                    return QuestState.QuestStateStatus.Success;
                return QuestState.QuestStateStatus.Active;
            });
            boughtState.AddDialogueState("MY FRIEND IS LOST", "P03LostGoobert", QuestState.QuestStateStatus.Failure);
            boughtState.AddDefaultActiveState("YOU FOUND HIM!", "P03GoobertHome")
                    .AddLoseItemReward(GoobertHuh.ItemData.name)
                    .AddGainItemReward(LifeItem.ItemData.name)
                    .AddMonetaryReward(13);

            // Prospector Quest
            Prospector = QuestManager.Add(P03Plugin.PluginGuid, "The Prospector");
            QuestState prepareState = Prospector.AddState("GOLD!", "P03ProspectorWantGold")
                      .SetDynamicStatus(() => {
                        if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.BRAIN))
                            return QuestState.QuestStateStatus.Active;
                        else
                            return QuestState.QuestStateStatus.Success;
                      })
                      .AddDialogueState("GOLD!", "P03ProspectorPrepareGold")
                      .AddNamedState("DoubleCheckHasBrain", "GOLD!", "P03ProspectorPrepareGold")
                      .SetDynamicStatus(() => {
                        if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.BRAIN))
                            return QuestState.QuestStateStatus.Failure;
                        else
                            return QuestState.QuestStateStatus.Success;
                      });

            prepareState.AddDialogueState("NO GOLD?", "P03ProspectorNoMoreGold", QuestState.QuestStateStatus.Failure);
            prepareState.AddDialogueState("GOLD!!!", "P03ProspectorReplaceGold")
                        .AddLoseCardReward(CustomCards.BRAIN)
                        .AddGainCardReward(CustomCards.BOUNTY_HUNTER_SPAWNER);

            // Generator Quest
            BrokenGenerator = QuestManager.Add(P03Plugin.PluginGuid, "Broken Generator");
            QuestState defaultState = BrokenGenerator.AddState("HELP!", "P03DamageRaceIntro");
            defaultState.AddDialogueState("OH NO...", "P03DamageRaceFailed", QuestState.QuestStateStatus.Failure);
            defaultState.AddDialogueState("PHEW!", "P03DamageRaceSuccess").AddDynamicMonetaryReward();;
        }
    }
}