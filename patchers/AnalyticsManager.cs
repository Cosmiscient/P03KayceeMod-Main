using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Text;
using System.IO;
using System.IO.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Quests;
using System.Net.Http;
using Infiniscryption.P03KayceeRun.Encounters;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class AnalyticsPatches
    {
        private static readonly HttpClient client = new HttpClient();

        [HarmonyPatch(typeof(AnalyticsManager), nameof(AnalyticsManager.SendFailedEncounterEvent))]
        [HarmonyPrefix]
        private static void SendFailedEncounterEvent(EncounterBlueprintData blueprint, int difficulty, int turnNumber)
        {
            if (!EncounterExtensions.P03OnlyEncounters.Contains(blueprint.name))
                return;

            // This sends encounter failure data to a Google Form
            Dictionary<string, string> values = new ()
            {
                { "entry.1934213090", blueprint.name },
                { "entry.176058667", turnNumber.ToString() },
                { "entry.207667890", difficulty.ToString() },
                { "entry.1064213994", AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty).ToString() }
            };

            FormUrlEncodedContent content = new(values);
            client.PostAsync("https://docs.google.com/forms/u/0/d/e/1FAIpQLSeRE03ZXAG882ByPjU7MuRBVYm3cvpk-bUJwNc_Ig9FGPPO3A/formResponse", content)
                  .ContinueWith(t => P03Plugin.Log.LogError(t.Exception), System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}