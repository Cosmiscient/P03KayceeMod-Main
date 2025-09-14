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
using Infiniscryption.PackManagement;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03ExpansionPack3
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.packmanager.p03plugin")]
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

            DialogueManagement.AddSequenceDialogue(DataHelper.GetResourceString("dialogue_database", "csv", typeof(P03Pack3Plugin).Assembly));
            Pack3EncounterHelper.BuildEncounters();
            Pack3Quests.CreatePack3Quests();

            PackInfo expPack2 = PackManager.GetPackInfo("P03KCMXP3");
            expPack2.Title = "Kaycee's P03 Expansion Pack #3";
            expPack2.Description = "They said it would never happen, but it's here! The third official expansion pack breaks all of the rules, with [count] new cards that bring bones, blood, and gems to Botopia!";
            expPack2.ValidFor.Clear();
            expPack2.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            expPack2.SetTexture(TextureHelper.GetImageAsTexture("pack.png", typeof(P03Pack3Plugin).Assembly));

            EncounterPackInfo xp2EncPack = PackManager.GetPackInfo<EncounterPackInfo>("P03KCMXP3");
            xp2EncPack.Title = "Kaycee's P03 Encounter Expansion #3";
            xp2EncPack.Description = "[count] additional encounters that feature cards from the third official P03 expansion pack.";
            xp2EncPack.ValidFor.Clear();
            xp2EncPack.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            xp2EncPack.SetTexture(TextureHelper.GetImageAsTexture("encounters.png", typeof(P03Pack3Plugin).Assembly));

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
