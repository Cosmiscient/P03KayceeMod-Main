using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Cards.Multiverse;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03ExpansionPack3
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.p03kayceerun")]
    public class P03Pack3Plugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03expansionpack3";
        public const string PluginName = "Infiniscryption P03 in Kaycee's Mod - Expansion Pack 3";
        public const string PluginVersion = "1.0";
        public const string CardPrefix = "P03KCMXP3";

        internal static P03Pack3Plugin Instance;

        internal static ManualLogSource Log;

        internal static bool Initialized = false;

        private void Awake()
        {
            Instance = this;

            Log = Logger;

            Harmony harmony = new(PluginGuid);
            harmony.PatchAll();

            foreach (Type t in typeof(P03Pack3Plugin).Assembly.GetTypes())
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

            Initialized = true;

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        private void OnDestroy()
        {
            // AudioHelper.FlushAudioClipCache();
            // AssetBundleManager.CleanUp();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Need to *guarantee* that all of our card mod patches take hold
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            EncounterManager.SyncEncounterList();
        }
    }
}
