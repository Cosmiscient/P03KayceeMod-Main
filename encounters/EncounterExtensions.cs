using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Encounters;

namespace Infiniscryption.P03KayceeRun.Encounters
{
    [HarmonyPatch]
    public static class EncounterExtensions
    {
        internal static List<string> P03OnlyEncounters = new();

        internal static Dictionary<string, EncounterBlueprintData> HolyHackerole = new();

        private static void MatchMods(CardInfo orig, CardInfo copy)
        {
            if (orig == null || copy == null || orig.mods == null || orig.mods.Count == 0)
                return;

            copy.mods = new();
            foreach (CardModificationInfo m in orig.mods)
                copy.mods.Add((CardModificationInfo)m.Clone());
        }

        static EncounterExtensions()
        {
            // Screw your stupid copying of stuff killing my card mods.
            // I'll fix it myself. The hard way.
            EncounterManager.ModifyEncountersList += delegate (List<EncounterBlueprintData> allEncounters)
            {
                if (!P03AscensionSaveData.IsP03Run)
                    return allEncounters;

                foreach (EncounterBlueprintData ebd in allEncounters)
                {
                    if (!HolyHackerole.Keys.Contains(ebd.name))
                        continue;

                    List<List<EncounterBlueprintData.CardBlueprint>> originalTurns = HolyHackerole[ebd.name].turns;

                    try
                    {
                        for (int t = 0; t < ebd.turns.Count; t++)
                        {
                            for (int c = 0; c < ebd.turns[t].Count; c++)
                            {
                                if (originalTurns[t][c].card != null)
                                    ebd.turns[t][c].card = CardLoader.GetCardByName(originalTurns[t][c].card.name);
                                if (originalTurns[t][c].replacement != null)
                                    ebd.turns[t][c].replacement = CardLoader.GetCardByName(originalTurns[t][c].replacement.name);
                                MatchMods(originalTurns[t][c].card, ebd.turns[t][c].card);
                                MatchMods(originalTurns[t][c].replacement, ebd.turns[t][c].replacement);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        P03Plugin.Log.LogError($"Failed to repair encounter {ebd.name}");
                        P03Plugin.Log.LogError(ex);
                    }
                }
                return allEncounters;
            };
        }

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
            foreach (IEnumerable<EncounterBlueprintData.CardBlueprint> subset in input)
                newTurn.AddRange(subset);
            blueprint.Add(newTurn);
            return blueprint;
        }

        public static List<List<EncounterBlueprintData.CardBlueprint>> AddTurn(this List<List<EncounterBlueprintData.CardBlueprint>> blueprint, IEnumerable<IEnumerable<EncounterBlueprintData.CardBlueprint>> input)
        {
            List<EncounterBlueprintData.CardBlueprint> newTurn = new();
            foreach (IEnumerable<EncounterBlueprintData.CardBlueprint> subset in input)
                newTurn.AddRange(subset);
            blueprint.Add(newTurn);
            return blueprint;
        }

        public static List<List<EncounterBlueprintData.CardBlueprint>> AddTurn(this List<List<EncounterBlueprintData.CardBlueprint>> blueprint, IEnumerable<List<EncounterBlueprintData.CardBlueprint>> input)
        {
            List<EncounterBlueprintData.CardBlueprint> newTurn = new();
            foreach (List<EncounterBlueprintData.CardBlueprint> subset in input)
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

            HolyHackerole[data.name] = data;

            if (!P03OnlyEncounters.Contains(data.name))
                P03OnlyEncounters.Add(data.name);

            return data;
        }

        [HarmonyPatch(typeof(EncounterBlueprintData), nameof(EncounterBlueprintData.PrerequisitesMet))]
        [HarmonyPrefix]
        private static bool P03EncountersNeverHavePrerequisitesMet(ref EncounterBlueprintData __instance, ref bool __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

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
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (card.Info != null && card.Info.Mods != null && card.Info.Mods.Any(m => m.fromOverclock))
                card.Anim.SetOverclocked(true);
        }
    }
}