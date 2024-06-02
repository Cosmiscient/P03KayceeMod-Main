using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Infiniscryption.PackManagerP03Plugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("inscryption_deckeditor")]
    [BepInDependency(P03PluginGuid)]
    public class DeckEditorCompatPlugin : BaseUnityPlugin
    {
        public const string P03PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";

        public const string PluginGuid = "zorro.inscryption.infiniscryption.deckeditor.p03plugin";
        public const string PluginName = "Deck Editor Patch - IT'S OKAY IF THIS PLUGIN FAILS TO LOAD!!";
        public const string PluginVersion = "2.0";

        private void Awake()
        {
            Harmony harmony = new(PluginGuid);
            var targetMethod = typeof(DeckEditor).GetMethod("GetAllAbilities", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var patchMethod = typeof(DeckEditorCompatPlugin).GetMethod("AddAllKnownAbilities", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            Logger.LogDebug($"Deck editor fixer: Target Method {targetMethod}, Patch method {patchMethod}");
            harmony.Patch(targetMethod, postfix: new HarmonyMethod(patchMethod));
        }

        public static void AddAllKnownAbilities(ref List<Ability> __result)
        {
            __result = AbilityManager.AllAbilities.Select(fab => fab.Info.ability).ToList();
        }
    }
}