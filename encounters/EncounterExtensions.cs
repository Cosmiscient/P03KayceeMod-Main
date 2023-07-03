using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Encounters
{
    [HarmonyPatch]
    public static class EncounterExtensions
    {
        private static List<string> P03OnlyEncounters = new();

        private static RunBasedHoloMap.Zone TempleToZone(this CardTemple? temple)
        {
            if (!temple.HasValue)
                return RunBasedHoloMap.Zone.Neutral;
            else if (temple.Value == CardTemple.Nature)
                return RunBasedHoloMap.Zone.Nature;
            else if (temple.Value == CardTemple.Tech)
                return RunBasedHoloMap.Zone.Tech;
            else if (temple.Value == CardTemple.Undead)
                return RunBasedHoloMap.Zone.Undead;
            else if (temple.Value == CardTemple.Wizard)
                return RunBasedHoloMap.Zone.Magic;
            
            return RunBasedHoloMap.Zone.Neutral;
        }

        internal static void SetEncounterRegion(RunBasedHoloMap.Zone zone, string encounterId)
        {
            if (!RegionGeneratorData.EncountersForZone[zone].Contains(encounterId))
                RegionGeneratorData.EncountersForZone[zone].Add(encounterId);
        }

        public static List<List<EncounterBlueprintData.CardBlueprint>> AddTurn(this List<List<EncounterBlueprintData.CardBlueprint>> blueprint, params IEnumerable<EncounterBlueprintData.CardBlueprint>[] input)
        {
            List<EncounterBlueprintData.CardBlueprint> newTurn = new();
            foreach (var subset in input)
                newTurn.AddRange(subset);
            blueprint.Add(newTurn);
            return blueprint;
        }

        public static List<List<EncounterBlueprintData.CardBlueprint>> AddTurn(this List<List<EncounterBlueprintData.CardBlueprint>> blueprint, IEnumerable<IEnumerable<EncounterBlueprintData.CardBlueprint>> input)
        {
            List<EncounterBlueprintData.CardBlueprint> newTurn = new();
            foreach (var subset in input)
                newTurn.AddRange(subset);
            blueprint.Add(newTurn);
            return blueprint;
        }

        public static List<List<EncounterBlueprintData.CardBlueprint>> AddTurn(this List<List<EncounterBlueprintData.CardBlueprint>> blueprint, IEnumerable<List<EncounterBlueprintData.CardBlueprint>> input)
        {
            List<EncounterBlueprintData.CardBlueprint> newTurn = new();
            foreach (var subset in input)
                newTurn.AddRange(subset);
            blueprint.Add(newTurn);
            return blueprint;
        }

        public static EncounterBlueprintData AddTurn(this EncounterBlueprintData blueprint, IEnumerable<IEnumerable<EncounterBlueprintData.CardBlueprint>> input)
        {
            blueprint.turns ??= new();
            blueprint.turns.AddTurn(input);
            return blueprint;
        }

        public static EncounterBlueprintData AddTurn(this EncounterBlueprintData blueprint, IEnumerable<List<EncounterBlueprintData.CardBlueprint>> input)
        {
            blueprint.turns ??= new();
            blueprint.turns.AddTurn(input);
            return blueprint;
        }

        /// <summary>
        /// Marks an encounter as being valid SPECIFICALLY for the P03 KCM Mod.
        /// </summary>
        /// <param name="region">The region this encounter should occur in. If null, it is considered "neutral" which means it could appear in any region.</param>
        /// <returns>Marking an encounter as being valid for P03 means that it CANNOT be generated in a Leshy run.</returns>
        public static EncounterBlueprintData SetP03Encounter(this EncounterBlueprintData data, CardTemple? region = null)
        {
            SetEncounterRegion(region.TempleToZone(), data.name);

            if (!P03OnlyEncounters.Contains(data.name))
                P03OnlyEncounters.Add(data.name);

            return data;
        }

        [HarmonyPatch(typeof(EncounterBlueprintData), nameof(EncounterBlueprintData.PrerequisitesMet))]
        [HarmonyPrefix]
        private static bool P03EncountersNeverHavePrerequisitesMet(ref EncounterBlueprintData __instance, ref bool __result)
        {
            if (P03OnlyEncounters.Contains(__instance.name))
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Part3Opponent), nameof(Part3Opponent.ModifyQueuedCard))]
        [HarmonyPostfix]
        private static void EnsureOverclocked(ref PlayableCard card)
        {
            if (card.Info != null && card.Info.Mods != null && card.Info.Mods.Any(m => m.fromOverclock))
                card.Anim.SetOverclocked(true);
        }
    }
}