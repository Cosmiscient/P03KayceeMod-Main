using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03SigilLibrary
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.p03kayceerun")]
    public class P03SigilLibraryPlugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03sigillibrary";
        public const string PluginName = "Infiniscryption P03 in Kaycee's Mod - Sigil Library";
        public const string PluginVersion = "1.0";
        public const string CardPrefix = "P03SIG";

        internal static P03SigilLibraryPlugin Instance;

        internal static ManualLogSource Log;

        internal static bool Initialized = false;

        private void Awake()
        {
            Instance = this;

            Log = Logger;

            Harmony harmony = new(PluginGuid);
            harmony.PatchAll();

            foreach (Type t in typeof(P03SigilLibraryPlugin).Assembly.GetTypes())
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

        public static int RandomSeed
        {
            get
            {
                // Let's make this super robust
                int retval = AscensionSaveData.Data == null ? 0 : AscensionSaveData.Data.currentRunSeed;
                try
                {
                    retval += 10 * (TurnManager.Instance?.TurnNumber ?? 1);
                    retval += 100 * (LifeManager.Instance?.OpponentDamage ?? 1);
                    retval += 1000 * (LifeManager.Instance?.PlayerDamage ?? 1);
                    retval += TurnManager.Instance?.IsPlayerTurn ?? true ? 55 : 121;
                    if (Part3SaveData.Data != null && Part3SaveData.Data.deck != null && Part3SaveData.Data.deck.cardIdModInfos != null)
                        retval += 10000 * Part3SaveData.Data.deck.cardIdModInfos.Select(kvp => (kvp.Value?.Count).GetValueOrDefault(0)).Sum();

                    retval += Part3SaveData.Data != null && Part3SaveData.Data.playerPos != null ? Part3SaveData.Data.playerPos.gridX : 0;
                    retval += Part3SaveData.Data != null && Part3SaveData.Data.playerPos != null ? 100000 * Part3SaveData.Data.playerPos.gridY : 0;

                    // A funky hack that should detect the mutiverse
                    retval += 11111 * (BoardManager.Instance?.PlayerSlotsCopy[0].Index ?? 2);

                    return retval;
                }
                catch
                {
                    return retval;
                }
            }
        }

        private void OnDestroy()
        {
            AudioHelper.FlushAudioClipCache();
            AssetBundleManager.CleanUp();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Need to *guarantee* that all of our card mod patches take hold
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
        }
    }
}
