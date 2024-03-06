using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Cards.Multiverse;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Quests;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03KayceeRun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.achievements")]
    [BepInDependency("zorro.inscryption.infiniscryption.spells")]
    public class P03Plugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
        public const string PluginName = "Infiniscryption P03 in Kaycee's Mod";
        public const string PluginVersion = "4.0";
        public const string CardPrefx = "P03KCM";

        internal static P03Plugin Instance;

        internal static ManualLogSource Log;

        internal static bool Initialized = false;

        internal string DebugCode => Config.Bind("P03KayceeMod", "DebugCode", "nothing", new BepInEx.Configuration.ConfigDescription("A special code to use for debugging purposes only. Don't change this unless your name is DivisionByZorro or he told you how it works.")).Value;

        internal bool SkipCanvasFace => Config.Bind("P03KayceeMod", "SkipCanvasFace", true, new BepInEx.Configuration.ConfigDescription("If True, skips the creation of the face for the Unfinished Boss. You can change this if you really want to change his face.")).Value;

        internal string SecretCardComponents => Config.Bind("P03KayceeMod", "SecretCardComponents", "nothing", new BepInEx.Configuration.ConfigDescription("The secret code for the secret card")).Value;

        internal bool TurboMode => DebugCode.ToLowerInvariant().Contains("turbomode");

        private void Awake()
        {
            Instance = this;

            Log = Logger;
            Log.LogInfo($"Debug code = {DebugCode}");

            Harmony harmony = new(PluginGuid);
            harmony.PatchAll();

            DialogueManagement.TrackForTranslation = true;

            // Call dialogue sequence
            DialogueManagement.AddSequenceDialogue();

            foreach (Type t in typeof(P03Plugin).Assembly.GetTypes())
            {
                try
                {
                    RuntimeHelpers.RunClassConstructor(t.TypeHandle);
                }
                catch (TypeLoadException ex)
                {
                    Log.LogWarning("Failed to force load static constructor!");
                    Log.LogWarning(ex);
                }
            }

            CustomCards.RegisterCustomCards(harmony);
            MultiverseCards.CreateCards();
            StarterDecks.RegisterStarterDecks();
            AscensionChallengeManagement.UpdateP03Challenges();
            BossManagement.RegisterBosses();
            DefaultQuestDefinitions.DefineAllQuests();
            EncounterHelper.BuildEncounters();
            MultiverseEncounters.CreateMultiverseEncounters();
            DialogueManagement.TrackForTranslation = false;
            DialogueManagement.ResolveCurrentTranslation();

            CustomCards.printAllCards();

            SceneManager.sceneLoaded += OnSceneLoaded;

            Initialized = true;

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FixDeckEditor() => Traverse.Create(Chainloader.PluginInfos["inscryption_deckeditor"].Instance as DeckEditor).Field("save").SetValue(SaveManager.SaveFile);

        private void OnDestroy()
        {
            AudioHelper.FlushAudioClipCache();
            AssetBundleManager.CleanUp();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Chainloader.PluginInfos.ContainsKey("inscryption_deckeditor"))
                FixDeckEditor();

            // Need to *guarantee* that all of our card mod patches take hold
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            EncounterManager.SyncEncounterList();
        }
    }
}
