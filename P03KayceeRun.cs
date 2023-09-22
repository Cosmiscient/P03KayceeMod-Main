using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Guid;
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
        public const string PluginVersion = "2.3";
        public const string CardPrefx = "P03KCM";

        internal static P03Plugin Instance;

        internal static ManualLogSource Log;

        internal static bool Initialized = false;

        internal string DebugCode => Config.Bind("P03KayceeMod", "DebugCode", "nothing", new BepInEx.Configuration.ConfigDescription("A special code to use for debugging purposes only. Don't change this unless your name is DivisionByZorro or he told you how it works.")).Value;

        internal string SecretCardComponents => Config.Bind("P03KayceeMod", "SecretCardComponents", "nothing", new BepInEx.Configuration.ConfigDescription("The secret code for the secret card")).Value;

        internal bool TurboMode => DebugCode.ToLowerInvariant().Contains("turbomode");

        private void Awake()
        {
            Instance = this;

            Log = Logger;
            Log.LogInfo($"Debug code = {DebugCode}");

            Harmony harmony = new(PluginGuid);
            harmony.PatchAll();

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
            StarterDecks.RegisterStarterDecks();
            AscensionChallengeManagement.UpdateP03Challenges();
            BossManagement.RegisterBosses();
            DefaultQuestDefinitions.DefineAllQuests();
            EncounterHelper.BuildEncounters();

            CustomCards.printAllCards();

            SceneManager.sceneLoaded += OnSceneLoaded;

            Initialized = true;

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FixDeckEditor() => Traverse.Create(Chainloader.PluginInfos["inscryption_deckeditor"].Instance as DeckEditor).Field("save").SetValue(SaveManager.SaveFile);

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Chainloader.PluginInfos.ContainsKey("inscryption_deckeditor"))
                FixDeckEditor();
        }

        private class DummyPatchTarget
        {
            private List<Ability> DummyMethod() => null;
        }

        [HarmonyPatch]
        private class DeckEditorCompatPatch
        {
            public static MethodBase TargetMethod()
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
                {
                    Type testType = assembly.GetType("DeckEditor");
                    if (testType != null)
                    {
                        MethodInfo meth = AccessTools.FirstMethod(testType, m => m.Name.Contains("GetAllAbilities"));
                        if (meth != null)
                            return meth;
                    }
                }
                return typeof(DummyPatchTarget).GetMethod("DummyMethod");
            }

            public static bool Prefix(ref List<Ability> __result)
            {
                __result = GuidManager.GetValues<Ability>()
                                      .Select(AbilitiesUtil.GetInfo)
                                      .Where(ai => ai != null)
                                      .Select(ai => ai.ability)
                                      .ToList();
                return false;
            }
        }
    }
}
