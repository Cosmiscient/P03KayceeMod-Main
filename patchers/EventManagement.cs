using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Quests;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Guid;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class EventManagement
    {
        public static List<StoryEvent> P03AscensionSaveEvents = new();
        public static List<StoryEvent> P03RunBasedStoryEvents = new();

        internal static StoryEvent NewStory(string code, bool save = false, bool run = false)
        {
            StoryEvent se = GuidManager.GetEnumValue<StoryEvent>(P03Plugin.PluginGuid, code);
            if (save)
                P03AscensionSaveEvents.Add(se);
            if (run)
                P03RunBasedStoryEvents.Add(se);
            return se;
        }

        public static readonly StoryEvent ALL_ZONE_ENEMIES_KILLED = NewStory("AllZoneEnemiesKilled");
        public static readonly StoryEvent ALL_BOSSES_KILLED = NewStory("AllBossesKilled");
        public static readonly StoryEvent HAS_DRAFT_TOKEN = NewStory("HasDraftToken");
        public static readonly StoryEvent SAW_P03_INTRODUCTION = NewStory("SawP03Introduction", save: true);
        public static readonly StoryEvent SAW_P03_PAIDRESPAWN_EXPLAIN = NewStory("SawP03PaidRespawnExplain", save: true);
        public static readonly StoryEvent SAW_P03_BOSSPAIDRESPAWN_EXPLAIN = NewStory("SawP03PaidRespawnExplain", save: true);
        public static readonly StoryEvent GOLLY_NFT = NewStory("GollyNFTIntro", save: true);
        public static readonly StoryEvent DEFEATED_P03 = NewStory("DefeatedP03");
        public static readonly StoryEvent ONLY_ONE_BOSS_LIFE = NewStory("P03AscensionOneBossLife", save: true);
        public static readonly StoryEvent OVERCLOCK_CHANGES = NewStory("P03AscensionOverclock", save: true);
        public static readonly StoryEvent TRANSFORMER_CHANGES = NewStory("P03AscensionTransformer", save: true);
        public static readonly StoryEvent HAS_DEFEATED_P03 = NewStory("HasDefeatedP03", save: true);
        public static readonly StoryEvent USED_LIFE_ITEM = NewStory("HasUsedLifeItem", save: true);
        public static readonly StoryEvent SAW_NEW_ORB = NewStory("P03HammerOrb", save: true);
        public static readonly StoryEvent GOT_STICKER_INTRODUCTION = NewStory("P03GotStickerIntro", save: true);

        public const string GAME_OVER = "GameOverZone";

        internal static readonly StoryEvent SAW_BOUNTY_HUNTER_MEDAL = NewStory("SawBountyHunterMedal", run: true);
        internal static readonly StoryEvent FLUSHED_GOOBERT = NewStory("SpecialEvent06", run: true);
        internal static readonly StoryEvent MYCO_ENTRY_APPROVED = NewStory("MycoEntryApproved", run: true);
        internal static readonly StoryEvent MYCO_ENTRY_DENIED = NewStory("MycoEntryDenied", run: true);
        internal static readonly StoryEvent MYCO_DEFEATED = NewStory("MycoDefeated", run: true);

        internal static bool SawCredits
        {
            get => ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, "SawCreditsB");
            set => ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "SawCreditsB", value);
        }

        public static Part3SaveData.WorldPosition MycologistReturnPosition
        {
            get
            {
                string key = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "MycologistReturnPosition");
                if (string.IsNullOrEmpty(key))
                    throw new InvalidOperationException("Trying to get the mycologist return position when it has never been set!");

                string[] pieces = key.Split('|');
                return new(pieces[0], int.Parse(pieces[1]), int.Parse(pieces[2]));
            }
            set
            {
                string key = $"{value.worldId}|{value.gridX}|{value.gridY}";
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "MycologistReturnPosition", key);
            }
        }

        public static List<CardInfo> MycologistTestSubjects
        {
            get
            {
                List<CardInfo> retval = new();

                string key = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "MycologistTestSubjects");
                if (!string.IsNullOrEmpty(P03Plugin.Instance.SecretCardComponents) && P03Plugin.Instance.SecretCardComponents.StartsWith("@"))
                    key = P03Plugin.Instance.SecretCardComponents;

                if (string.IsNullOrEmpty(key))
                    return retval;

                string[] pieces = key.Split('%');
                retval.AddRange(pieces.Select(CustomCards.ConvertCodeToCard));
                return retval;
            }
        }

        public static void AddMycologistsTestSubject(CardInfo info)
        {
            string subjectCode = CustomCards.ConvertCardToCompleteCode(info);
            string currentKey = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "MycologistTestSubjects");
            if (string.IsNullOrEmpty(currentKey))
                currentKey = subjectCode;
            else
                currentKey += "%" + subjectCode;
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "MycologistTestSubjects", currentKey);
        }

        public static RunBasedHoloMap.Zone CurrentZone
        {
            get
            {
                try
                {
                    return RunBasedHoloMap.Building ? RunBasedHoloMap.BuildingZone : RunBasedHoloMap.GetRegionCodeFromWorldID(HoloMapAreaManager.Instance.CurrentWorld.name);
                }
                catch (Exception)
                {
                    return RunBasedHoloMap.Zone.Neutral;
                }
            }
        }

        public static readonly MechanicsConcept[] P03_MECHANICS = new MechanicsConcept[]
        {
            MechanicsConcept.BossMultipleLives,
            MechanicsConcept.GainCurrency,
            MechanicsConcept.HoloMapCheckpoint,
            MechanicsConcept.HoloMapFastTravel,
            MechanicsConcept.OnlineFriendCards,
            MechanicsConcept.Part3AttachGem,
            MechanicsConcept.Part3Bloodstain,
            MechanicsConcept.Part3Bounty,
            MechanicsConcept.Part3BountyTiers,
            MechanicsConcept.Part3BuildACard,
            MechanicsConcept.Part3Consumables,
            MechanicsConcept.Part3CreateTransformer,
            MechanicsConcept.Part3ModifySideDeck,
            MechanicsConcept.Part3OverclockCard,
            MechanicsConcept.Part3RecycleCard,
            MechanicsConcept.Part3Respawn,
            MechanicsConcept.Part3TradeCards,
            MechanicsConcept.PhotographerRestoreSnapshot,
            MechanicsConcept.PhotographerTakeSnapshot,
            MechanicsConcept.DamageRaceBattle
        };

        public static readonly StoryEvent[] P03_ALWAYS_TRUE_STORIES = new StoryEvent[]
        {
            StoryEvent.LukeVOBeatLeshyAgain,
            StoryEvent.LukeVODieAlready,
            StoryEvent.LukeVOLeshyRematch,
            StoryEvent.LukeVOMantisGod,
            StoryEvent.LukeVOPart3Shit,
            StoryEvent.LukeVOPart3Yes,
            StoryEvent.LukeVOPart3Wtf,
            StoryEvent.LukeVOPart3File,
            StoryEvent.LukeVOOPCard,
            StoryEvent.LukeVONewRunAfterVictory,
            StoryEvent.LukeVOPart1Vision,
            StoryEvent.LukeVOPart2Bonelord,
            StoryEvent.LukeVOPart2Grimora,
            StoryEvent.LukeVOPart3CloseWin
        };

        private static readonly Dictionary<HoloMapNode.NodeDataType, float> CostAdjustments = new()
        {
            { HoloMapNode.NodeDataType.AddCardAbility, 0f },
            { HoloMapNode.NodeDataType.BuildACard, 1f },
            { UnlockAscensionItemNodeData.UnlockItemsAscension, 0.5f },
            { HoloMapNode.NodeDataType.CreateTransformer, -1f },
            { HoloMapNode.NodeDataType.OverclockCard, -1f },
            { AscensionRecycleCardNodeData.AscensionRecycleCard, -2f }
        };

        public static int EncounterDifficulty
        {
            get
            {
                int tier = CompletedZones.Count;
                int modifier = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty);
                return tier + modifier + (tier == 0 ? 0 : 1);
            }
        }

        public static IEnumerator SayDialogueOnce(string dialogueId, StoryEvent eventTracker)
        {
            if (!StoryEventsData.EventCompleted(eventTracker))
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueId, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                StoryEventsData.SetEventCompleted(eventTracker);
            }
            yield break;
        }

        [HarmonyPatch(typeof(Part3SaveData), nameof(Part3SaveData.GetDifficultyModifier))]
        [HarmonyPrefix]
        public static bool AscensionDifficultyModifierWorksDifferently(ref int __result)
        {
            if (SaveFile.IsAscension)
            {
                __result = 0;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RunState), nameof(RunState.DifficultyModifier), MethodType.Getter)]
        [HarmonyPostfix]
        public static void AscensionRunStateDifficultyModifierWorksDifference(ref int __result)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
            {
                __result = 0;
            }
        }

        public static int UpgradePrice(HoloMapNode.NodeDataType nodeType)
        {
            float baseCost = 7 + (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 3f + (2f * CompletedZones.Count) : 1f * CompletedZones.Count);

            if (CostAdjustments.ContainsKey(nodeType))
            {
                float adj = CostAdjustments[nodeType];
                if (adj != 0 && Math.Abs(adj) < 1)
                    baseCost *= adj;
                else
                    baseCost += CostAdjustments[nodeType];
            }

            return Mathf.RoundToInt(baseCost);
        }

        public static Tuple<int, int> CurrencyGainRange
        {
            get
            {
                int minExpectedUpgrades = AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 2 * CompletedZones.Count : 3 * CompletedZones.Count;
                int actualUpgrades = Part3SaveData.Data.deck.Cards.Select(c => c.mods.Count()).Sum();
                int upgradeDiff = Math.Max(0, minExpectedUpgrades - actualUpgrades - (Part3SaveData.Data.currency / 6));
                int low = 4 + CompletedZones.Count + (3 * upgradeDiff);
                int high = 8 + CompletedZones.Count + (3 * upgradeDiff);
                return new(low, high);
            }
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.OnDie))]
        [HarmonyPostfix]
        public static IEnumerator EarnCurrencyWhenBountyHunterDies(IEnumerator sequence, PlayableCard killer, BountyHunter __instance)
        {
            yield return sequence;

            if (!SaveFile.IsAscension || TurnManager.Instance.Opponent is P03AscensionOpponent) // don't do this on the final boss
                yield break;

            P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
            View currentView = ViewManager.Instance.CurrentView;
            yield return new WaitForSeconds(0.4f);
            int currencyGain = Part3SaveData.Data.BountyTier * 3;
            yield return P03AnimationController.Instance.ShowChangeCurrency(currencyGain, true);
            Part3SaveData.Data.currency += currencyGain;
            yield return new WaitForSeconds(0.2f);
            P03AnimationController.Instance.SwitchToFace(currentFace);
            yield return new WaitForSeconds(0.1f);
            if (ViewManager.Instance.CurrentView != currentView)
            {
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.2f);
            }

            // Don't spawn the brain in the following situations:
            if (killer != null && (killer.HasAbility(Ability.Deathtouch) || killer.HasAbility(Ability.SteelTrap)))
                yield break;

            // Spawn at most one per run
            if (StoryEventsData.EventCompleted(SAW_BOUNTY_HUNTER_MEDAL))
                yield break;

            // This can only happen on the very first bounty hunter of the run. You get exactly one shot
            if (Part3SaveData.Data.bountyHunterMods.Count != 1)
                yield break;

            // Get the brain but take the ability off of it
            CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
            yield return BoardManager.Instance.CreateCardInSlot(brain, (__instance.Card as PlayableCard).Slot, 0.15f, true);
            StoryEventsData.SetEventCompleted(SAW_BOUNTY_HUNTER_MEDAL);
        }

        public static int NumberOfLivesRemaining
        {
            get => ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "NumberOfLivesRemaining");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "NumberOfLivesRemaining", value);
        }

        public static int NumberOfLosses
        {
            get => ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "NumberOfLosses");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "NumberOfLosses", value);
        }

        public const int ENEMIES_TO_UNLOCK_BOSS = 4;
        public static int NumberOfZoneEnemiesKilled
        {
            get
            {
                string key = $"{CurrentZone}_EnemiesKilled";
                return ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, key);
            }
            set
            {
                string key = $"{CurrentZone}_EnemiesKilled";
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, key, value);
            }
        }

        public static List<string> CompletedZones
        {
            get
            {
                string zoneCsv = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "CompletedZones");
                return zoneCsv == default ? new List<string>() : zoneCsv.Split(',').ToList();
            }
        }

        public static void AddCompletedZone(StoryEvent storyEvent)
        {
            if (storyEvent == StoryEvent.ArchivistDefeated) AddCompletedZone("FastTravelMapNode_Undead");
            if (storyEvent == StoryEvent.CanvasDefeated) AddCompletedZone("FastTravelMapNode_Wizard");
            if (storyEvent == StoryEvent.TelegrapherDefeated) AddCompletedZone("FastTravelMapNode_Tech");
            if (storyEvent == StoryEvent.PhotographerDefeated) AddCompletedZone("FastTravelMapNode_Nature");
        }

        public static void AddCompletedZone(string id)
        {
            List<string> zones = CompletedZones;
            if (!zones.Contains(id))
                zones.Add(id);

            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "CompletedZones", string.Join(",", zones));
        }

        public static List<string> VisitedZones
        {
            get
            {
                string zoneCsv = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "VisitedZones");
                return zoneCsv == default ? new List<string>() : zoneCsv.Split(',').ToList();
            }
        }
        public static void AddVisitedZone(string id)
        {
            List<string> zones = VisitedZones;
            if (!zones.Contains(id))
                zones.Add(id);

            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "VisitedZones", string.Join(",", zones));
        }

        public static StoryEvent GetStoryEventForOpponent(Opponent.Type opponent)
        {
            return opponent == Opponent.Type.PhotographerBoss
                ? StoryEvent.PhotographerDefeated
                : opponent == Opponent.Type.TelegrapherBoss
                ? StoryEvent.TelegrapherDefeated
                : opponent == Opponent.Type.CanvasBoss
                ? StoryEvent.CanvasDefeated
                : opponent == Opponent.Type.ArchivistBoss ? StoryEvent.ArchivistDefeated : StoryEvent.WoodcarverDefeated;
        }

        [HarmonyPatch(typeof(ProgressionData), nameof(ProgressionData.LearnedMechanic))]
        [HarmonyPrefix]
        public static bool ForceMechanicsLearnd(MechanicsConcept mechanic, ref bool __result)
        {
            if (SaveFile.IsAscension && P03_MECHANICS.Contains(mechanic))
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(StoryEventsData), "SetEventCompleted")]
        [HarmonyPrefix]
        public static bool P03AscensionStoryCompleted(StoryEvent storyEvent)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run && P03AscensionSaveEvents.Contains(storyEvent))
            {
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}", true);
                return false;
            }
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run && P03RunBasedStoryEvents.Contains(storyEvent))
            {
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}", true);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(StoryEventsData), "EventCompleted")]
        [HarmonyPrefix]
        public static bool P03AscensionStoryData(ref bool __result, StoryEvent storyEvent)
        {
            if (SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
            {
                if (P03_ALWAYS_TRUE_STORIES.Contains(storyEvent))
                {
                    __result = true;
                    return false;
                }

                if (storyEvent == StoryEvent.ArchivistDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Undead");
                    return false;
                }

                if (storyEvent == StoryEvent.CanvasDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Wizard");
                    return false;
                }

                if (storyEvent == StoryEvent.TelegrapherDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Tech");
                    return false;
                }

                if (storyEvent == StoryEvent.PhotographerDefeated)
                {
                    __result = CompletedZones.Contains("FastTravelMapNode_Nature");
                    return false;
                }

                if ((int)storyEvent == (int)ALL_ZONE_ENEMIES_KILLED)
                {
                    __result = NumberOfZoneEnemiesKilled >= ENEMIES_TO_UNLOCK_BOSS;
                    return false;
                }
                if ((int)storyEvent == (int)ALL_BOSSES_KILLED)
                {
                    __result = CompletedZones.Count >= 4 || P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("finalboss");
                    return false;
                }
                if ((int)storyEvent == (int)HAS_DRAFT_TOKEN)
                {
                    __result = Part3SaveData.Data.deck.Cards.Any(card => card.name is CustomCards.DRAFT_TOKEN or CustomCards.RARE_DRAFT_TOKEN);
                    return false;
                }

                if (storyEvent == StoryEvent.GemsModuleFetched) // Simply going to this world 'completes' that story event for you
                {
                    __result = true;
                    return false;
                }

                if (storyEvent == StoryEvent.HoloTechTempleSatelliteActivated)
                {
                    __result = true;
                    return false;
                }

                if (P03AscensionSaveEvents.Contains(storyEvent))
                {
                    __result = ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}");
                    return false;
                }

                if (P03RunBasedStoryEvents.Contains(storyEvent))
                {
                    __result = ModdedSaveManager.RunState.GetValueAsBoolean(P03Plugin.PluginGuid, $"StoryEvent{storyEvent}");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPrefix]
        public static void TrackVictories(ref TurnManager __instance)
        {
            if (!SaveFile.IsAscension)
                return;

            // NOTE! In the prefix, the calculation for 'player won' hasn't happened yet
            // So we have to manually do all the checks for what constitutes 'victory'

            if (__instance.SpecialSequencer is DamageRaceBattleSequencer drbs)
            {
                if (drbs.damageDealt >= DamageRaceBattleSequencer.DAMAGE_TO_SUCCEED)
                {
                    DefaultQuestDefinitions.BrokenGenerator.InitialState.Status = QuestState.QuestStateStatus.Success;
                    if (__instance.TurnNumber <= 3)
                        AchievementManager.Unlock(P03AchievementManagement.FAST_GENERATOR);
                }
                else
                {
                    // We don't want failure in the generator to actually cause the player to lose
                    Part3SaveData.Data.playerLives += 1;
                    DefaultQuestDefinitions.BrokenGenerator.InitialState.Status = QuestState.QuestStateStatus.Failure;
                }
            }

            if (__instance.SpecialSequencer is not DamageRaceBattleSequencer)
            {
                if (__instance.Opponent.NumLives <= 0 || __instance.Opponent.Surrendered)
                {
                    NumberOfZoneEnemiesKilled++;

                    if (DefaultQuestDefinitions.TippedScales.IsDefaultActive())
                        DefaultQuestDefinitions.TippedScales.IncrementQuestCounter();
                }
            }

            if (DefaultQuestDefinitions.WhiteFlag.IsDefaultActive() && __instance.Opponent.Surrendered)
                DefaultQuestDefinitions.WhiteFlag.IncrementQuestCounter();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static IEnumerator TippedScalesQuest(IEnumerator sequence)
        {
            yield return sequence;

            if (SaveFile.IsAscension)
            {
                if (DefaultQuestDefinitions.TippedScales.IsDefaultActive())
                {
                    yield return LifeManager.Instance.ShowDamageSequence(1, 1, true, 0.125f, null, 0f, false);
                }
            }
        }

        private static bool SlotHasBrain(CardSlot slot)
        {
            if (slot.Card != null && slot.Card.Info.name.Equals(CustomCards.BRAIN))
                return true;

            Card queueCard = BoardManager.Instance.GetCardQueuedForSlot(slot);
            return queueCard != null && queueCard.Info.name.Equals(CustomCards.BRAIN);
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static IEnumerator AcquireBrain(IEnumerator sequence)
        {
            if (SaveFile.IsAscension)
            {
                if (BoardManager.Instance.AllSlotsCopy.Any(SlotHasBrain))
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03BountyHunterBrain", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    CardInfo brain = CardLoader.GetCardByName(CustomCards.BRAIN);
                    brain.mods = new() {
                        new(Ability.DrawRandomCardOnDeath)
                    };
                    Part3SaveData.Data.deck.AddCard(brain);
                }
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.curses" })]
        public static void SortOutStartOfRun(ref AscensionSaveData __instance)
        {
            // Figure out the number of lives
            NumberOfLivesRemaining = __instance.currentRun.maxPlayerLives;

            //Reset respawn cost
            LifeManagement.respawnCost = 0;
        }

        public static void FinishAscension(bool success = true)
        {
            P03Plugin.Log.LogInfo("Starting finale sequence");
            AscensionMenuScreens.ReturningFromSuccessfulRun = success;
            AscensionMenuScreens.ReturningFromFailedRun = !success;
            AscensionStatsData.TryIncrementStat(success ? AscensionStat.Type.Victories : AscensionStat.Type.Losses);

            if (success)
            {
                foreach (AscensionChallenge c in AscensionSaveData.Data.activeChallenges)
                {
                    if (!AscensionSaveData.Data.conqueredChallenges.Contains(c))
                        AscensionSaveData.Data.conqueredChallenges.Add(c);
                }

                if (!string.IsNullOrEmpty(AscensionSaveData.Data.currentStarterDeck) && !AscensionSaveData.Data.conqueredStarterDecks.Contains(AscensionSaveData.Data.currentStarterDeck))
                    AscensionSaveData.Data.conqueredStarterDecks.Add(AscensionSaveData.Data.currentStarterDeck);
            }

            // Delete the ascension save; the run is over            
            ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, P03AscensionSaveData.ASCENSION_SAVE_KEY, default(string));

            // Also delete the normal ascension current run just in case
            //AscensionSaveData.Data.currentRun = null;

            if (CompletedZones.Count > 0)
                AscensionSaveData.Data.numRunsSinceReachedFirstBoss = 0;

            // Let's no longer force this to false
            // It should go false when the screen loads
            // and leaving it 'as is' should help the restart work.
            //P03AscensionSaveData.IsP03Run = false;

            Part3SaveData.Data.checkpointPos = new Part3SaveData.WorldPosition(GAME_OVER, 0, 0);

            SaveManager.SaveToFile(false);

            P03Plugin.Log.LogInfo("Loading ascension scene");

            if (SawCredits || !success)
                SceneLoader.Load("Ascension_Configure");
            else
                SceneLoader.Load("Ascension_Credits");
        }
    }
}