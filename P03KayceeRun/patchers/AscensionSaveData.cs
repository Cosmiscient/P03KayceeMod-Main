using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Quests;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Saves;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class P03AscensionSaveData
    {
        internal const string ASCENSION_SAVE_KEY = "CopyOfPart3AscensionSave";
        internal const string REGULAR_SAVE_KEY = "CopyOfPart3Save";

        private static readonly string SaveFilePath = Path.Combine(BepInEx.Paths.GameRootPath, "P03SaveFileActive.gwsave");

        internal static AscensionSaveData P03Data { get; private set; }

        public static ModdedSaveData RunStateData { get; private set; }

        internal static bool ReturningFromP03Run { get; set; }

        internal static bool P03RunExists => P03Data != null && P03Data.currentRun != null && P03Data.currentRun.playerLives > 0;

        internal static bool LeshyIsDead => (P03Data?.itemUnlockEvents.Contains(EventManagement.LESHY_IS_DEAD)).GetValueOrDefault(false);

        internal static void SetLeshyDead(bool dead, bool immediate)
        {
            if (P03Data == null)
                return;

            if (dead && !P03Data.itemUnlockEvents.Contains(EventManagement.LESHY_IS_DEAD))
                P03Data.itemUnlockEvents.Add(EventManagement.LESHY_IS_DEAD);

            if (!dead && P03Data.itemUnlockEvents.Contains(EventManagement.LESHY_IS_DEAD))
                P03Data.itemUnlockEvents.Remove(EventManagement.LESHY_IS_DEAD);

            if (immediate)
                SaveManager.SaveToFile(false);
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.Data), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool GetP03SaveData(ref AscensionSaveData __result)
        {
            if (IsP03Run)
            {
                if (P03Data == null)
                    SaveManager.LoadFromFile();
                __result = P03Data;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ModdedSaveManager), nameof(ModdedSaveManager.RunState), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool GetP03RunStateData(ref ModdedSaveData __result)
        {
            if (IsP03Run)
            {
                if (RunStateData == null)
                {
                    if (File.Exists(SaveFilePath))
                    {
                        string json = File.ReadAllText(SaveFilePath);
                        Dictionary<string, Dictionary<string, object>> internalData = SaveManager.FromJSON<Dictionary<string, Dictionary<string, object>>>(json);
                        RunStateData = internalData != null ? CreateFromInternalData(new(), internalData) : new();
                    }
                    else
                    {
                        RunStateData ??= new();
                    }
                }
                RunStateData ??= new();
                __result = RunStateData;
                return false;
            }
            return true;
        }

        public static int RandomSeed
        {
            get
            {
                // Let's make this super robust
                int retval = AscensionSaveData.Data == null ? 0 : AscensionSaveData.Data.currentRunSeed;
                try
                {
                    retval += 10 * EventManagement.CompletedZones.Count;
                    retval += 100 * EventManagement.VisitedZones.Count;
                    retval += 1000 * EventManagement.NumberOfZoneEnemiesKilled;
                    if (Part3SaveData.Data != null && Part3SaveData.Data.deck != null && Part3SaveData.Data.deck.cardIdModInfos != null)
                        retval += 10000 * Part3SaveData.Data.deck.cardIdModInfos.Select(kvp => (kvp.Value?.Count).GetValueOrDefault(0)).Sum();

                    retval += Part3SaveData.Data != null && Part3SaveData.Data.playerPos != null ? Part3SaveData.Data.playerPos.gridX : 0;
                    retval += Part3SaveData.Data != null && Part3SaveData.Data.playerPos != null ? 100000 * Part3SaveData.Data.playerPos.gridY : 0;

                    if (MultiverseBattleSequencer.Instance != null)
                        retval += 11111 * MultiverseBattleSequencer.Instance.CurrentMultiverseId;

                    return retval;
                }
                catch
                {
                    return retval;
                }
            }
        }

        internal static int MaxNumberOfItems => 3 - AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessConsumables);

        private static string SaveKey
        {
            get => SceneLoader.ActiveSceneName == "Ascension_Configure"
                    ? ASCENSION_SAVE_KEY
                    : SceneLoader.ActiveSceneName == SceneLoader.StartSceneName
                    ? REGULAR_SAVE_KEY
                    : SaveFile.IsAscension ? ASCENSION_SAVE_KEY : REGULAR_SAVE_KEY;
        }

        public static bool IsP03Run
        {
            //             get => (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part3") && SaveFile.IsAscension)
            // || ScreenManagement.ScreenState == CardTemple.Tech
            // || (!SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1")
            // && AscensionSaveData.Data != null && AscensionSaveData.Data.currentRun != null && AscensionSaveData.Data.currentRun.playerLives > 0
            // && ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, "IsP03Run"));
            get => SaveFile.IsAscension && (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part3") || (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("ascension") && ScreenManagement.ScreenState == CardTemple.Tech));
        }

        private static string ToCompressedJSON(object data)
        {
            if (data == null)
                return default;

            string value = SaveManager.ToJSON(data);
            //InfiniscryptionP03Plugin.Log.LogInfo($"JSON SAVE: {value}");
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            using MemoryStream input = new(bytes);
            using MemoryStream output = new();
            using (GZipStream stream = new(output, CompressionLevel.Optimal))
            {
                input.CopyTo(stream);
                //stream.Flush();
            }
            string result = Convert.ToBase64String(output.ToArray());
            //InfiniscryptionP03Plugin.Log.LogInfo($"B64 SAVE: {result}");
            return result;
        }

        private static T FromCompressedJSON<T>(string data)
        {
            if (string.IsNullOrEmpty(data))
                return default;

            byte[] bytes = Convert.FromBase64String(data);
            using MemoryStream input = new(bytes);
            using MemoryStream output = new();
            using (GZipStream stream = new(input, CompressionMode.Decompress))
            {
                stream.CopyTo(output);
                //output.Flush();            
            }
            string json = Encoding.Unicode.GetString(output.ToArray());
            //P03Plugin.Log.LogInfo($"SAVE JSON for {SaveKey}: {json}");
            return SaveManager.FromJSON<T>(json);
        }

        internal static void EnsureRegularSave()
        {
            // The only way there is not a copy of the regular save is because you went straight to a p03 ascension run
            // after installing the mod. This means that the current part3savedata is your actual act 3 save data
            // We don't want to lose that.
            if (ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, REGULAR_SAVE_KEY) == default)
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, REGULAR_SAVE_KEY, ToCompressedJSON(Part3SaveData.Data));
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        private static void ClearSaveData(ref Part3SaveData __instance) => ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, SaveKey, default(string));

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
        private static class Part3SaveDataFixImprovement
        {
            // Okay, I recognize that this is all kind of crazy.

            // Here's the problem: the game wants your Botopia save data to be in a specific place
            // I don't want you to lose your original Part 3 save. Plus, **I** really don't want to lose that save either!
            // Why? Because I want to be able to leave Kaycee's Mod and go explore original Botopia so I can
            // check out how it behaves, etc.

            // So what we do is we actually keep two Part3Save copies alive in the ModdedSaveFile, and we swap in whichever
            // one is necessary based on context (see the patch for LoadFromFile)

            // But whenever the file is saved, only the original part 3 save data gets saved in the normal spot
            // This fixes issues that arise when people unload the P03 KCM mod.

            [HarmonyPrefix]
            [HarmonyBefore(new string[] { "cyantist.inscryption.api" })]
            public static void Prefix(ref Part3SaveData __state)
            {
                // What this does is save a copy of the current part 3 save data somewhere else
                // The idea is that when you play part 3, every time you save we keep a copy of that data
                // And whenever you play ascension part 3, same thing.
                //
                // That way, if you switch over to the other type of part 3, we can load the last time this happened.
                // And whenever creating a new ascension part 3 run, we check to see if there is a copy of part 3 save yet
                // If not, we will end up creating one

                if (P03Data != null)
                    ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "P03AscensionSaveDataProgress", ToCompressedJSON(P03Data));

                // We have some really, really weird issues with items. Let's make sure that the save file
                // has the current item state
                if (ItemsManager.Instance != null && ItemsManager.Instance.Consumables != null)
                {
                    var newItems = ItemsManager.Instance.Consumables.Where(i => i != null && i.Data != null).Select(i => i.Data.name).Where(s => !s.Equals("hammer", StringComparison.InvariantCultureIgnoreCase)).ToList();
                    SaveManager.SaveFile.part3Data.items.Clear();
                    SaveManager.SaveFile.part3Data.items.AddRange(newItems);
                }

                P03Plugin.Log.LogInfo($"Saving P03 Save Data {SaveKey}");
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, SaveKey, ToCompressedJSON(SaveManager.SaveFile.part3Data));

                // Then, right before we actually save the data, we swap back in the original part3 data
                __state = SaveManager.SaveFile.part3Data;

                EnsurePart3Saved();
                string originalPart3String = ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, REGULAR_SAVE_KEY);
                Part3SaveData originalPart3Data = FromCompressedJSON<Part3SaveData>(originalPart3String);
                SaveManager.SaveFile.part3Data = originalPart3Data;

                // SEE BELOW FOR WHAT HAPPENS NEXT: \/ \/ \/ 
            }

            [HarmonyPostfix]
            public static void Postfix(Part3SaveData __state)
            {
                // Now that we've saved the file, we swap back whatever we had before
                SaveManager.SaveFile.part3Data = __state;

                // And we'll go ahead and save the current runstate to the custom save file
                RunStateData ??= new();
                string moddedSaveData = SaveManager.ToJSON(GetInternalData(RunStateData));
                File.WriteAllText(SaveFilePath, moddedSaveData);
            }
        }

        // private static ModdedSaveData OldRunState { get; set; }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.NewRun))]
        [HarmonyPrefix]
        [HarmonyBefore(new string[] { "cyantist.inscryption.api" })]
        private static void CacheOldRunStateIfP03Run()
        {
            if (IsP03Run)
            {
                RunStateData = new();
            }
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.OuroborosDeaths), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool P03GetOuroborosDeaths(SaveFile __instance, ref int __result)
        {
            if (IsP03Run)
            {
                __result = P03Data == null ? 0 : P03Data.currentOuroborosDeaths;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.OuroborosDeaths), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool P03GetOuroborosDeaths(SaveFile __instance, int value)
        {
            if (IsP03Run)
            {
                P03Data ??= new();
                P03Data.currentOuroborosDeaths = value;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(HoloMapArea), nameof(HoloMapArea.SaveData), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool AlwaysGetSaveDataDirectlyFromSaveFileForP03Mod(HoloMapArea __instance, ref Part3SaveData.MapAreaStateData __result)
        {
            if (!IsP03Run)
                return true;

            __result = Part3SaveData.Data.areaData.Find((Part3SaveData.MapAreaStateData x) => x.id == __instance.SaveId);
            if (__result == null)
            {
                __result = new Part3SaveData.MapAreaStateData(__instance, __instance.CheckpointId);
                Part3SaveData.Data.areaData.Add(__result);
            }
            return false;
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.TestSaveFileCorrupted))]
        [HarmonyPrefix]
        private static void RepairMissingPart3Data(SaveFile file)
        {
            if (file.part3Data == null)
            {
                file.part3Data = new Part3SaveData();
                file.part3Data.Initialize();
            }
        }

        private static Dictionary<string, Dictionary<string, object>> GetInternalData(ModdedSaveData moddedSaveData)
        {
            Traverse trav = Traverse.Create(moddedSaveData);
            return trav.Field("SaveData").GetValue<Dictionary<string, Dictionary<string, object>>>();
        }

        private static ModdedSaveData CreateFromInternalData(ModdedSaveData data, Dictionary<string, Dictionary<string, object>> internalData)
        {
            foreach (string guid in internalData.Keys)
            {
                foreach (string key in internalData[guid].Keys)
                    data.SetValue(guid, key, internalData[guid][key]);
            }
            return data;
        }

        private static bool _initializingDuringLoad = false;

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadFromFile))]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        private static void LoadPart3AscensionSaveData()
        {
            EnsurePart3Saved();
            P03Plugin.Log.LogInfo($"Loading from the save file. Getting Part3 save [{SaveKey}]");
            string part3Data = ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, SaveKey);

            Part3SaveData data = FromCompressedJSON<Part3SaveData>(part3Data);

            if (data == default(Part3SaveData))
            {
                _initializingDuringLoad = true;
                data = new Part3SaveData();
                data.Initialize();
                _initializingDuringLoad = false;
            }

            SaveManager.SaveFile.part3Data = data;

            string part3AscensionData = ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, "P03AscensionSaveDataProgress");
            AscensionSaveData ascensionData = FromCompressedJSON<AscensionSaveData>(part3AscensionData);


            if (ascensionData == default(AscensionSaveData))
            {
                ascensionData = new AscensionSaveData();
                ascensionData.Initialize();
                ascensionData.itemUnlockEvents = new() { EventManagement.P03_SAVE_MARKER };
            }


            if (LeshyIsDead && !ascensionData.activeChallenges.Contains(AscensionChallenge.FinalBoss))
                ascensionData.activeChallenges.Add(AscensionChallenge.FinalBoss);


            P03Data = ascensionData;


            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                Dictionary<string, Dictionary<string, object>> internalData = SaveManager.FromJSON<Dictionary<string, Dictionary<string, object>>>(json);
                RunStateData = internalData != null ? CreateFromInternalData(new(), internalData) : new();
            }
            else
            {
                RunStateData ??= new();
            }
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.IncrementChallengeLevel))]
        [HarmonyPostfix]
        private static void IncrementToMax(AscensionSaveData __instance)
        {
            if (__instance.itemUnlockEvents.Contains(EventManagement.P03_SAVE_MARKER))
            {
                if (__instance.challengeLevel > AscensionChallengeManagement.ChallengePointsPerLevel.Length)
                    __instance.challengeLevel = 13;
            }
        }

        [HarmonyPatch(typeof(AscensionUnlockSchedule), nameof(AscensionUnlockSchedule.NumStarterDecksUnlocked))]
        [HarmonyPrefix]
        private static bool GetP03NumStarterDecks(int level, ref int __result)
        {
            if (ScreenManagement.ScreenState == CardTemple.Tech)
            {
                __result = level == 1 ? 1 : 8;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.GetChallengePointsForLevel))]
        [HarmonyPrefix]
        private static bool GetP03ChallengePointsForLevel(int level, ref int __result)
        {
            if (ScreenManagement.ScreenState == CardTemple.Tech)
            {
                __result = level <= AscensionChallengeManagement.ChallengePointsPerLevel.Length
                    ? AscensionChallengeManagement.ChallengePointsPerLevel[level - 1]
                    : level < 0
                    ? AscensionChallengeManagement.ChallengePointsPerLevel[0]
                    : AscensionChallengeManagement.ChallengePointsPerLevel[AscensionChallengeManagement.ChallengePointsPerLevel.Length - 1] + (10 * (level - AscensionChallengeManagement.ChallengePointsPerLevel.Length));

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        private static void FixStarters(bool newRun)
        {
            bool cannotChooseDecks = AscensionUnlockSchedule.NumStarterDecksUnlocked(AscensionSaveData.Data.challengeLevel) <= 1;
            if (newRun && cannotChooseDecks)
                AscensionSaveData.Data.currentStarterDeck = IsP03Run ? StarterDecks.DEFAULT_STARTER_DECK : "Vanilla";
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.NewRun))]
        [HarmonyPostfix]
        private static void InitializePart3Save()
        {
            P03Plugin.Log.LogInfo($"Asked to start new Ascension run. Is P03 Run? {IsP03Run}");
            if (IsP03Run)
            {
                Part3SaveData data = new();
                data.Initialize();
                SaveManager.SaveFile.part3Data = data;
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        private static void EnsurePart3Saved()
        {
            if (SaveFile.IsAscension)
            {
                // Check to see if there is a part 3 save data yet
                EnsureRegularSave();
            }
        }

        [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.EndRun))]
        [HarmonyPrefix]
        private static void ClearP03SaveOnEndRun(AscensionSaveData __instance)
        {
            if (ReturningFromP03Run || ScreenManagement.ScreenState == CardTemple.Tech)
            {
                SaveManager.SaveFile.part3Data = null;
                ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, ASCENSION_SAVE_KEY, default(string));
                ReturningFromP03Run = false;
            }
        }

        [HarmonyPatch(typeof(RunState), nameof(RunState.Run), MethodType.Getter)]
        [HarmonyPostfix]
        private static void RunIsNullForP03(ref RunState __result)
        {
            if (IsP03Run)
            {
                __result ??= new();
                __result.regionTier = EventManagement.CompletedZones.Count;
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), nameof(Part3SaveData.Initialize))]
        [HarmonyPostfix]
        private static void RewritePart3IntroSequence(ref Part3SaveData __instance)
        {
            P03Plugin.Log.LogInfo($"Part3 Init {P03Plugin.Initialized}. During load? {_initializingDuringLoad}");
            if (!P03Plugin.Initialized || _initializingDuringLoad)
                return;

            if (SaveFile.IsAscension && AscensionSaveData.Data.currentRun != null)
            {
                string worldId = RunBasedHoloMap.GetAscensionWorldID(RunBasedHoloMap.Zone.Neutral);
                Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(RunBasedHoloMap.Zone.Neutral);
                Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

                __instance.playerPos = worldPosition;
                __instance.checkpointPos = new Part3SaveData.WorldPosition(__instance.playerPos);
                __instance.reachedCheckpoints = new List<string>() { __instance.playerPos.worldId };

                EventManagement.NumberOfZoneEnemiesKilled = 0;

                __instance.deck = new DeckInfo();
                __instance.deck.Cards.Clear();

                StarterDeckInfo deckInfo = StarterDecksUtil.GetInfo(AscensionSaveData.Data.currentStarterDeck);

                List<CardInfo> starterDeckCards = deckInfo.cards.Select(i => CardLoader.GetCardByName(i.name)).ToList();

                foreach (CardInfo info in starterDeckCards)
                    __instance.deck.AddCard(info);

                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.WeakStarterDeck))
                {
                    foreach (CardInfo info in __instance.deck.Cards)
                    {
                        info.mods ??= new();
                        info.mods.Add(new(Ability.BuffEnemy));
                    }
                }

                __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN));

                if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("rarestarter"))
                    __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.RARE_DRAFT_TOKEN));

                if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("ringworm"))
                {
                    __instance.deck.AddCard(CardLoader.GetCardByName(ExpansionPackCards_2.RINGWORM_CARD));
                    __instance.currency = 25;
                }

                __instance.sideDeckAbilities.Add(Ability.ConduitNull);

                if (P03Plugin.Instance.TurboMode)
                    __instance.currency = 250;

                if (IsP03Run)
                {

                    __instance.items ??= new List<string>();

                    if (MaxNumberOfItems >= 1)
                    {
                        if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("wiseclock"))
                            __instance.items.Add(WiseclockItem.ItemData.name);
                        else
                            __instance.items.Add(ShockerItem.ItemData.name);
                    }

                    if (MaxNumberOfItems >= 2 && !AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoHook))
                    {
                        if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("ufo"))
                            __instance.items.Add(UfoItem.ItemData.name);
                        else
                            __instance.items.Add("BombRemote");
                    }

                    if (MaxNumberOfItems >= 3)
                    {
                        if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("rifle"))
                            __instance.items.Add(RifleItem.ItemData.name);
                        else
                            __instance.items.Add("ShieldGenerator");
                    }
                }

                __instance.reachedCheckpoints.Add("NorthNeutralPath"); // This makes bounty hunters work properly
                                                                       // Without this, your bounty can never reach tier 1

                // TEMPORARY: Force the mycologists active at the start
                if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("goobert"))
                {
                    __instance.deck.AddCard(CardLoader.GetCardByName(CustomCards.BRAIN));
                    __instance.items[0] = GoobertHuh.ItemData.name;
                    DefaultQuestDefinitions.BrokenGenerator.InitialState.Status = QuestState.QuestStateStatus.Success;
                }

                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BOUNTY_HUNTER))
                    __instance.bounty = 45 * 2;//AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallengeManagement.BOUNTY_HUNTER); // Good fucking luck
            }
        }

        // This keeps the oil painting puzzle from breaking the game
        [HarmonyPatch(typeof(OilPaintingPuzzle), nameof(OilPaintingPuzzle.GenerateSolution))]
        [HarmonyPrefix]
        private static bool ReplaceGenerateForP03(ref List<string> __result)
        {
            if (IsP03Run)
            {
                __result = new List<string>() { null, null, CustomCards.VIRUS_SCANNER, CustomCards.VIRUS_SCANNER };
                return false;
            }
            return true;
        }
    }
}