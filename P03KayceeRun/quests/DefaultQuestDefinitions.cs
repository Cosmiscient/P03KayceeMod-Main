using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Faces;
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
        internal static QuestDefinition Pyromania { get; private set; }
        internal static QuestDefinition Conveyors { get; private set; }
        internal static QuestDefinition BombBattles { get; private set; }
        internal static QuestDefinition BountyTarget { get; private set; }
        internal static QuestDefinition LeapBotNeo { get; private set; }

        // These are the special story maps
        internal static QuestDefinition FindGoobert { get; private set; }
        internal static QuestDefinition Prospector { get; private set; }
        internal static QuestDefinition BrokenGenerator { get; private set; }

        public const int RADIO_TURNS = 5;
        public const int POWER_TURNS = 4;
        public const int BURNED_CARDS = 3;

        internal static int DeckSizeTarget
        {
            get
            {
                int retval = P03AscensionSaveData.RunStateData.GetValueAsInt(P03Plugin.PluginGuid, "DeckSizeTarget");
                if (retval > 0)
                    return retval;

                retval = Part3SaveData.Data.deck.Cards.Count + 2;
                P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "DeckSizeTarget", retval);
                return retval;
            }
        }

        internal static RunBasedHoloMap.Zone GoobertDropoffZone
        {
            get
            {
                string dropoff = P03AscensionSaveData.RunStateData.GetValue(P03Plugin.PluginGuid, "Dropoff");
                if (!string.IsNullOrEmpty(dropoff))
                    return (RunBasedHoloMap.Zone)Enum.Parse(typeof(RunBasedHoloMap.Zone), dropoff);

                List<RunBasedHoloMap.Zone> zones = new()
                {
                    RunBasedHoloMap.Zone.Magic, RunBasedHoloMap.Zone.Nature, RunBasedHoloMap.Zone.Tech, RunBasedHoloMap.Zone.Undead
                };
                zones.Remove(EventManagement.CurrentZone);
                RunBasedHoloMap.Zone assignedDropoff = zones[SeededRandom.Range(0, zones.Count, P03AscensionSaveData.RandomSeed)];
                P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "Dropoff", assignedDropoff);
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
                                .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Rags, CompositeFigurine.FigurineType.SettlerMan))
                                .SetGenerateCondition(() => EventManagement.CompletedZones.Count <= 2);
            TippedScales.AddDialogueState("TOO EASY...", "P03TooEasyQuest")
                        .AddDialogueState("TOO EASY...", "P03TooEasyAccepted")
                        .AddDefaultActiveState("KEEP GOING...", "P03TooEasyInProgress", threshold: 3)
                        .AddDialogueState("IMPRESSIVE...", "P03TooEasyComplete")
                        .AddGainAbilitiesReward(1, Ability.DrawCopyOnDeath);

            // White Flag
            WhiteFlag = QuestManager.Add(P03Plugin.PluginGuid, "White Flag")
                                    .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Cyclops, CompositeFigurine.FigurineType.SettlerMan));
            WhiteFlag.AddDialogueState("DO THEY GIVE UP?", "P03WhiteFlagSetup")
                     .AddDefaultActiveState("DO THEY GIVE UP?", "P03WhiteFlagSetup", threshold: 1)
                     .AddDialogueState("THEY DO GIVE UP", "P03WhiteFlagReward")
                     .AddGainCardReward(CustomCards.UNC_TOKEN);

            // Deck Size
            DeckSize = QuestManager.Add(P03Plugin.PluginGuid, "Deck Size")
                                   .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Goggles, CompositeFigurine.FigurineType.Robot));
            DeckSize.AddDialogueState("ITS TOO SMALL", "P03DeckSizeSetup")
                    .AddSuccessAction(() => { int dummy = DeckSizeTarget; }) // this sets the initial decksize target
                    .AddNamedState("waiting", "ITS TOO SMALL", "P03DeckSizeSetup")
                    .SetDynamicStatus(() =>
                    {
                        return Part3SaveData.Data.deck.Cards.Count >= DeckSizeTarget
                            ? QuestState.QuestStateStatus.Success
                            : QuestState.QuestStateStatus.Active;
                    })
                    .AddDialogueState("ITS JUST RIGHT", "P03DeckSizeReward")
                    .AddDynamicMonetaryReward();

            // Smuggler
            Smuggler = QuestManager.Add(P03Plugin.PluginGuid, "Smuggler")
                                   .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Wirehead, CompositeFigurine.FigurineType.Gravedigger));
            Smuggler.AddDialogueState("SSH - COME OVER HERE", "P03SmugglerSetup")
                    .AddDialogueState("LETS DO THIS", "P03SmugglerAccepted")
                    .AddGainCardReward(CustomCards.CONTRABAND);

            SmugglerPartTwo = QuestManager.Add(P03Plugin.PluginGuid, "SmugglerPartTwo").SetPriorQuest(Smuggler).OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Wirehead, CompositeFigurine.FigurineType.Gravedigger));
            QuestState smugglerDummyState = SmugglerPartTwo.AddDummyStartingState(() => Part3SaveData.Data.deck.Cards.Any(c => c.name.Equals(CustomCards.CONTRABAND)));
            smugglerDummyState.SetNextState(QuestState.QuestStateStatus.Success, "SSH - BRING IT HERE", "P03SmugglerComplete", autoComplete: true)
                              .AddLoseCardReward(CustomCards.CONTRABAND)
                              .AddGainCardReward(CustomCards.UNC_TOKEN);
            smugglerDummyState.SetNextState(QuestState.QuestStateStatus.Failure, "WHERE DID IT GO?", "P03SmugglerFailed", autoComplete: true);

            // Donation
            Donation = QuestManager.Add(P03Plugin.PluginGuid, "Donation")
                                   .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Fishhead, CompositeFigurine.FigurineType.SettlerWoman));
            Donation.AddDialogueState("SPARE SOME CASH?", "P03DonationIntro")
                    .AddNamedState("CheckingForAvailableCash", "SPARE SOME CASH?", "P03DonationNotEnough")
                    .SetDynamicStatus(() =>
                    {
                        return Part3SaveData.Data.currency >= 10 ? QuestState.QuestStateStatus.Success : QuestState.QuestStateStatus.Active;
                    })
                    .AddDialogueState("SPARE SOME CASH?", "P03DonationComplete")
                    .AddMonetaryReward(-10);

            DonationPartTwo = QuestManager.Add(P03Plugin.PluginGuid, "DonationPartTwo").SetPriorQuest(Donation).OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Fishhead, CompositeFigurine.FigurineType.SettlerMan) { faceCode = "2-12-7" });
            DonationPartTwo.AddDialogueState("THANK YOU!", "P03DonationReward").AddGemifyCardsReward(2);

            // Fully Upgraded
            FullyUpgraded = QuestManager.Add(P03Plugin.PluginGuid, "Fully Upgraded")
                .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.BuildABot, CompositeFigurine.FigurineType.Robot))
                .SetGenerateCondition(() => EventManagement.CompletedZones.Count >= 3); // Can only happen if you've finished 3 maps

            FullyUpgraded.AddDialogueState("SHOW ME POWER", "P03FullyUpgradedFail")
                         .SetDynamicStatus(() =>
                         {
                             return Part3SaveData.Data.deck.Cards.Any(c =>
                                c.HasAbility(Ability.Transformer) &&
                                c.HasAbility(NewPermaDeath.AbilityID) &&
                                c.Gemified
                            )
                                ? QuestState.QuestStateStatus.Success
                                : QuestState.QuestStateStatus.Active;
                         })
                         .AddDialogueState("SHOW ME POWER", "P03FullyUpgradedSuccess")
                         .AddDynamicMonetaryReward();

            // I Love Bones
            ILoveBones = QuestManager.Add(P03Plugin.PluginGuid, "I Love Bones")
                                     .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Faceless, CompositeFigurine.FigurineType.Gravedigger));
            ILoveBones.SetRegionCondition(RunBasedHoloMap.Zone.Undead)
                      .AddDialogueState("I LOVE BONES!!", "P03ILoveBones")
                      .SetDynamicStatus(() =>
                      {
                          return Part3SaveData.Data.deck.Cards.Where(
                            c => c.HasAbility(Ability.Brittle) || c.HasAbility(Ability.PermaDeath) || c.HasAbility(NewPermaDeath.AbilityID)
                        ).Count() >= 3
                            ? QuestState.QuestStateStatus.Success
                            : QuestState.QuestStateStatus.Active;
                      })
                      .AddDialogueState("I LOVE BONES!!!!", "P03ILoveBonesSuccess")
                      .AddGainCardReward(CustomCards.SKELETON_LORD);

            // Radio Tower
            ListenToTheRadio = QuestManager.Add(P03Plugin.PluginGuid, "Listen To The Radio")
                                           .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Creepface, CompositeFigurine.FigurineType.SettlerMan));

            QuestState radioActiveState = ListenToTheRadio.AddDialogueState("LETS DO SCIENCE", "P03RadioQuestStart")
                            .AddDialogueState("LETS DO SCIENCE", "P03RadioQuestAccepted")
                            .AddGainCardReward(CustomCards.RADIO_TOWER)
                            .AddDefaultActiveState("LETS DO SCIENCE", "P03RadioQuestInProgress")
                            .SetDynamicStatus(() =>
                            {
                                return ListenToTheRadio.GetQuestCounter() >= RADIO_TURNS
                                    ? QuestState.QuestStateStatus.Success
                                    : !Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.RADIO_TOWER)
                                    ? QuestState.QuestStateStatus.Failure
                                    : QuestState.QuestStateStatus.Active;
                            });

            radioActiveState.AddDialogueState("YOU BROKE IT?!", "P03RadioQuestFailed", QuestState.QuestStateStatus.Failure);
            radioActiveState.AddDialogueState("A WIN FOR SCIENCE", "P03RadioQuestSucceeded")
                            .AddLoseCardReward(CustomCards.RADIO_TOWER)
                            .AddGainCardReward(CustomCards.UNC_TOKEN);

            // Power Up The Tower
            PowerUpTheTower = QuestManager.Add(P03Plugin.PluginGuid, "Power Up The Tower")
                                          .OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Pipehead, CompositeFigurine.FigurineType.Robot));

            QuestState towerActiveState = PowerUpTheTower.AddDialogueState("LOOKING FOR A JOB?", "P03PowerQuestStart")
                            .AddDialogueState("LOOKING FOR A JOB?", "P03PowerQuestAccepted")
                            .AddGainCardReward(CustomCards.POWER_TOWER)
                            .AddDefaultActiveState("GET BACK TO WORK", "P03PowerQuestInProgress")
                            .SetDynamicStatus(() =>
                            {
                                return PowerUpTheTower.GetQuestCounter() >= POWER_TURNS
                                    ? QuestState.QuestStateStatus.Success
                                    : !Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.POWER_TOWER)
                                    ? QuestState.QuestStateStatus.Failure
                                    : QuestState.QuestStateStatus.Active;
                            });

            towerActiveState.AddDialogueState("YOU BROKE IT?!", "P03PowerQuestFailed", QuestState.QuestStateStatus.Failure);
            towerActiveState.AddDialogueState("HERE'S YOUR PAYMENT", "P03PowerQuestSucceeded")
                            .AddLoseCardReward(CustomCards.POWER_TOWER)
                            .AddDynamicMonetaryReward(low: true)
                            .AddGainItemReward(ShockerItem.ItemData.name);

            // Goobert Quests
            FindGoobert = QuestManager.Add(P03Plugin.PluginGuid, "FindGoobert").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.PikeMageSolo, CompositeFigurine.FigurineType.Enchantress));
            QuestState waitingState = FindGoobert.AddDialogueState("MY FRIEND IS LOST", "P03WhereIsGoobert")
                            .AddNamedState("GoobertAvailable", "MY FRIEND IS LOST", "P03WhereIsGoobert");

            // When you fail the default state (i.e., you don't buy goobert when you see him), you end up here
            waitingState.AddDialogueState("MY FRIEND IS LOST", "P03DidNotBuyGoobert", QuestState.QuestStateStatus.Failure);

            // When you buy goobert, you end up here
            QuestState boughtState = waitingState.AddNamedState("CarryingGoobert", "YOU FOUND HIM!", () => $"P03FoundGoobert{GoobertDropoffZone}");
            boughtState.SetDynamicStatus(() =>
            {
                return !Part3SaveData.Data.items.Any(item => GoobertHuh.ItemData.name.ToLowerInvariant() == item.ToLowerInvariant())
                    ? QuestState.QuestStateStatus.Failure
                    : EventManagement.CurrentZone == GoobertDropoffZone
                    ? QuestState.QuestStateStatus.Success
                    : QuestState.QuestStateStatus.Active;
            });
            boughtState.AddDialogueState("MY FRIEND IS LOST", "P03LostGoobert", QuestState.QuestStateStatus.Failure);
            boughtState.AddDefaultActiveState("YOU FOUND HIM!", "P03GoobertHome")
                    .AddLoseItemReward(GoobertHuh.ItemData.name)
                    .AddGainItemReward(LifeItem.ItemData.name)
                    .AddMonetaryReward(13);

            // Prospector Quest
            Prospector = QuestManager.Add(P03Plugin.PluginGuid, "The Prospector").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Prospector, CompositeFigurine.FigurineType.Prospector));
            QuestState prepareState = Prospector.AddState("GOLD!", "P03ProspectorWantGold")
                      .SetDynamicStatus(() =>
                      {
                          return !Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.BRAIN)
                            ? QuestState.QuestStateStatus.Active
                            : QuestState.QuestStateStatus.Success;
                      })
                      .AddDialogueState("GOLD!", "P03ProspectorPrepareGold")
                      .AddNamedState("DoubleCheckHasBrain", "GOLD!", "P03ProspectorPrepareGold")
                      .SetDynamicStatus(() =>
                      {
                          return !Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.BRAIN)
                            ? QuestState.QuestStateStatus.Failure
                            : QuestState.QuestStateStatus.Success;
                      });

            prepareState.AddDialogueState("NO GOLD?", "P03ProspectorNoMoreGold", QuestState.QuestStateStatus.Failure);
            prepareState.AddDialogueState("GOLD!!!", "P03ProspectorReplaceGold")
                        .AddLoseCardReward(CustomCards.BRAIN)
                        .AddGainCardReward(CustomCards.BOUNTY_HUNTER_SPAWNER);

            // Generator Quest
            BrokenGenerator = QuestManager.Add(P03Plugin.PluginGuid, "Broken Generator").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.InspectorSolo, CompositeFigurine.FigurineType.Wildling));
            QuestState defaultState = BrokenGenerator.AddState("HELP!", "P03DamageRaceIntro");
            defaultState.AddDialogueState("OH NO...", "P03DamageRaceFailed", QuestState.QuestStateStatus.Failure);
            defaultState.AddDialogueState("PHEW!", "P03DamageRaceSuccess").AddDynamicMonetaryReward();

            // Pyromania
            Pyromania = QuestManager.Add(P03Plugin.PluginGuid, "Pyromania").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Pyromaniac, CompositeFigurine.FigurineType.Enchantress));
            Pyromania.SetGenerateCondition(() => Part3SaveData.Data.deck.Cards.Where(c => c.Abilities.Any(a => AbilitiesUtil.GetInfo(a).metaCategories.Contains(FireBomb.FlamingAbility))).Count() >= 2)
                     .AddDialogueState("BURN BABY BURN", "P03PyroQuestStart")
                     .AddDefaultActiveState("BURN BABY BURN", "P03PyroQuestInProgress")
                     .WaitForQuestCounter(BURNED_CARDS)
                     .AddDialogueState("SO SATISFYING...", "P03PyroQuestComplete")
                     .AddGainCardReward(ExpansionPackCards_2.FLAME_CHARMER_CARD);

            // Conveyors
            Conveyors = QuestManager.Add(P03Plugin.PluginGuid, "Conveyors").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Steambot, CompositeFigurine.FigurineType.Robot));
            Conveyors.SetGenerateCondition(() => EventManagement.CompletedZones.Count < 3 && !AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.ALL_CONVEYOR.challengeType))
                     .AddDialogueState("CONVEYOR FIELD TRIALS", "P03ConveyorQuestStart")
                     .AddDialogueState("START FIELD TRIALS?", "P03ConveyorQuestStarting")
                     .AddDefaultActiveState("FIELD TRIALS IN PROGRESS", "P03ConveyorQuestActive")
                     .WaitForQuestCounter(3)
                     .AddDialogueState("FIELD TRIALS COMPLETE", "P03ConveyorQuestComplete")
                     .AddDynamicMonetaryReward()
                     .AddGainItemReward("PocketWatch");

            // Bombs
            BombBattles = QuestManager.Add(P03Plugin.PluginGuid, "BombBattles").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.MrsBomb, CompositeFigurine.FigurineType.Robot));
            BombBattles.SetGenerateCondition(() => EventManagement.CompletedZones.Count < 3 && !AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BOMB_CHALLENGE.challengeType))
                     .AddDialogueState("BOOM BOOM BOOM", "P03BombQuestStart")
                     .AddDialogueState("LET'S BLOW IT UP", "P03BombQuestStarting")
                     .AddDefaultActiveState("KEEP UP THE BOOM", "P03BombQuestActive")
                     .WaitForQuestCounter(3)
                     .AddDialogueState("TRULY EXPLOSIVE", "P03BombQuestComplete")
                     .AddDynamicMonetaryReward()
                     .AddGainItemReward("BombRemote");

            // Bounty
            BountyTarget = QuestManager.Add(P03Plugin.PluginGuid, "BountyTarget").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.BountyHunter, CompositeFigurine.FigurineType.SettlerMan));
            BountyTarget.SetGenerateCondition(() => EventManagement.CompletedZones.Count < 3)
                        .AddDialogueState("CATCH A FUGITIVE?", "P03BountyQuestIntro")
                        .AddDialogueState("LET'S CATCH A FUGITIVE?!", "P03BountyQuestStarted")
                        .AddDefaultActiveState("LET'S GET HIM!", "P03BountyQuestInProgress")
                        .AddDialogueState("YOU GOT HIM!", "P03BountyQuestComplete")
                        .AddDynamicMonetaryReward(low: true)
                        .AddGainCardReward(CustomCards.DRAFT_TOKEN + "+Sniper");

            // LeapBot Neo
            LeapBotNeo = QuestManager.Add(P03Plugin.PluginGuid, "LeapBotNeo").OverrideNPCDescriptor(new(P03ModularNPCFace.FaceSet.Leapbot, CompositeFigurine.FigurineType.Robot));
            static bool generateLBNQuest()
            {
                return LeapBotNeo.GetQuestCounter() > 7 && Part3SaveData.Data.deck.Cards.Any(c => c.name == "LeapBot");
            }

            QuestState dummyState = LeapBotNeo.SetGenerateCondition(generateLBNQuest)
                                       .SetMustBeGeneratedCondition(generateLBNQuest)
                                       .GenerateAwayFromStartingArea()
                                       .AddDummyStartingState(() => Part3SaveData.Data.deck.Cards.Any(c => c.name == "LeapBot") ? QuestState.QuestStateStatus.Success : QuestState.QuestStateStatus.Failure);

            dummyState.AddDialogueState("DISAPPOINTING", "P03LeapBotQuestFailed", QuestState.QuestStateStatus.Failure);
            dummyState.AddDialogueState("ITS GLORIOUS", "P03LeapBotQuest")
                      .AddReward(new QuestRewardTransformCard() { CardName = "LeapBot", TransformIntoCardName = ExpansionPackCards_2.LEAPBOT_NEO });

        }
    }
}