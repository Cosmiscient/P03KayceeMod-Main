using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.PackManagement;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using Steamworks;
using UnityEngine.SceneManagement;

namespace Infiniscryption.SteamUserName
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency(P03PluginGuid)]
    public class SteamUserNamePlugin : BaseUnityPlugin
    {
        public const string P03PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";

        public const string PluginGuid = "zorro.inscryption.infiniscryption.steamusername";
        public const string PluginName = "Infiniscryption Steam Username Utility - IT'S OKAY IF THIS PLUGIN FAILS TO LOAD IF YOU AREN'T RUNNING STEAM!!";
        public const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        public static string CachedSteamUsername { get; private set; } = String.Empty;

        private void Awake()
        {
            Log = Logger;

            SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode mode)
            {
                CachedSteamUsername = SteamFriends.GetPersonaName().Trim();
            };
        }
    }
}