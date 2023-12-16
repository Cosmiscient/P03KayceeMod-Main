using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.BattleMods;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Encounters;
using InscryptionAPI.Triggers;

namespace Infiniscryption.P03KayceeRun.Encounters
{
    [HarmonyPatch]
    public static class EncounterExtensions
    {
        internal static List<string> P03OnlyEncounters = new();

        internal static Dictionary<string, EncounterBlueprintData> HolyHackerole = new();

        internal static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> PlayerTerrains = new();
        internal static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> OpposingTerrains = new();
        internal static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> OpposingTerrainQueues = new();

        internal static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> PlayerTerrainsResolved = new();
        internal static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> OpposingTerrainsResolved = new();
        internal static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> OpposingTerrainQueuesResolved = new();

        internal static Dictionary<string, Func<bool>> OpposingTerrainRepeatRules = new();

        private static void MatchMods(CardInfo orig, CardInfo copy)
        {
            if (orig == null || copy == null || orig.mods == null || orig.mods.Count == 0)
                return;

            copy.mods = new();
            foreach (CardModificationInfo m in orig.mods)
                copy.mods.Add((CardModificationInfo)m.Clone());
        }

        private static Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> FixTerrainDictionary(Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> terrain)
        {
            Dictionary<string, List<List<EncounterBlueprintData.CardBlueprint>>> retval = new();
            foreach (string key in terrain.Keys)
            {
                if (terrain[key] == null)
                    continue;

                List<List<EncounterBlueprintData.CardBlueprint>> terrainBlueprint = new();
                foreach (List<EncounterBlueprintData.CardBlueprint> bpList in terrain[key])
                {
                    if (bpList == null)
                    {
                        terrainBlueprint.Add(null);
                        continue;
                    }

                    List<EncounterBlueprintData.CardBlueprint> terrainSet = new();
                    foreach (EncounterBlueprintData.CardBlueprint bp in bpList)
                    {
                        if (bp == null)
                            continue;

                        EncounterBlueprintData.CardBlueprint clonedBluePrint = new();
                        if (bp.card != null)
                            clonedBluePrint.card = CardLoader.GetCardByName(bp.card.name);
                        if (bp.replacement != null)
                            clonedBluePrint.replacement = CardLoader.GetCardByName(bp.replacement.name);
                        MatchMods(bp.card, clonedBluePrint.card);
                        MatchMods(bp.replacement, clonedBluePrint.replacement);
                        clonedBluePrint.difficultyReplace = bp.difficultyReplace;
                        clonedBluePrint.difficultyReq = bp.difficultyReq;
                        clonedBluePrint.maxDifficulty = bp.maxDifficulty;
                        clonedBluePrint.minDifficulty = bp.minDifficulty;
                        clonedBluePrint.randomReplaceChance = 0;
                        clonedBluePrint.replacement = new();
                        terrainSet.Add(clonedBluePrint);
                    }
                    terrainBlueprint.Add(terrainSet);
                }
                retval[key] = terrainBlueprint;
            }
            return retval;
        }

        static EncounterExtensions()
        {
            // Screw your stupid copying of stuff killing my card mods.
            // I'll fix it myself. The hard way.
            EncounterManager.ModifyEncountersList += delegate (List<EncounterBlueprintData> allEncounters)
            {
                if (!P03AscensionSaveData.IsP03Run)
                    return allEncounters;

                PlayerTerrainsResolved = FixTerrainDictionary(PlayerTerrains);
                OpposingTerrainsResolved = FixTerrainDictionary(OpposingTerrains);
                OpposingTerrainQueuesResolved = FixTerrainDictionary(OpposingTerrainQueuesResolved);

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

        public static EncounterBlueprintData AddTerrainRepeatRule(this EncounterBlueprintData data, Func<bool> rule)
        {
            OpposingTerrainRepeatRules[data.name] = rule;
            return data;
        }

        public static EncounterBlueprintData AddTerrainRepeatRule(this EncounterBlueprintData data, int turnGap)
        {
            OpposingTerrainRepeatRules[data.name] = () => TurnManager.Instance != null && TurnManager.Instance.Opponent != null && TurnManager.Instance.Opponent.NumTurnsTaken > 0 && TurnManager.Instance.Opponent.NumTurnsTaken % turnGap == 0;
            return data;
        }

        public static EncounterBlueprintData AddTerrainRepeatRule(this EncounterBlueprintData data, Func<int, int> turnGap)
        {
            OpposingTerrainRepeatRules[data.name] = () => TurnManager.Instance != null && TurnManager.Instance.Opponent != null && TurnManager.Instance.Opponent.NumTurnsTaken > 0 && TurnManager.Instance.Opponent.NumTurnsTaken % turnGap(TurnManager.Instance.Opponent.Difficulty) == 0;
            return data;
        }

        /// <summary>
        /// Sets a forced terrain for this encounter for the player
        /// </summary>
        /// <param name="data">The encounter</param>
        /// <param name="terrain">The player terrain</param>
        public static EncounterBlueprintData AddPlayerTerrainSimple(this EncounterBlueprintData data, List<EncounterBlueprintData.CardBlueprint> terrain)
        {
            PlayerTerrains[data.name] = new() { terrain };
            return data;
        }

        /// <summary>
        /// Sets a forced terrain for this encounter for the enemy
        /// </summary>
        /// <param name="data">The encounter</param>
        /// <param name="terrain">The player terrain</param>
        public static EncounterBlueprintData AddEnemyTerrainSimple(this EncounterBlueprintData data, List<EncounterBlueprintData.CardBlueprint> terrain)
        {
            OpposingTerrains[data.name] = new() { terrain };
            return data;
        }

        /// <summary>
        /// Sets a forced terrain for this encounter for the enemy queue
        /// </summary>
        /// <param name="data">The encounter</param>
        /// <param name="terrain">The player terrain</param>
        public static EncounterBlueprintData AddEnemyTerrainQueueSimple(this EncounterBlueprintData data, List<EncounterBlueprintData.CardBlueprint> terrain)
        {
            OpposingTerrainQueues[data.name] = new() { terrain };
            return data;
        }

        /// <summary>
        /// Sets a forced terrain for this encounter for the player
        /// </summary>
        /// <param name="data">The encounter</param>
        /// <param name="terrain">The player terrain</param>
        public static EncounterBlueprintData AddPlayerTerrain(this EncounterBlueprintData data, List<List<EncounterBlueprintData.CardBlueprint>> terrain)
        {
            PlayerTerrains[data.name] = terrain;
            return data;
        }

        /// <summary>
        /// Sets a forced terrain for this encounter for the enemy
        /// </summary>
        /// <param name="data">The encounter</param>
        /// <param name="terrain">The player terrain</param>
        public static EncounterBlueprintData AddEnemyTerrain(this EncounterBlueprintData data, List<List<EncounterBlueprintData.CardBlueprint>> terrain)
        {
            OpposingTerrains[data.name] = terrain;
            return data;
        }

        /// <summary>
        /// Sets a forced terrain for this encounter for the enemy queue
        /// </summary>
        /// <param name="data">The encounter</param>
        /// <param name="terrain">The player terrain</param>
        public static EncounterBlueprintData AddEnemyTerrainQueue(this EncounterBlueprintData data, List<List<EncounterBlueprintData.CardBlueprint>> terrain)
        {
            OpposingTerrainQueues[data.name] = terrain;
            return data;
        }

        private static List<CardInfo> ConvertTerrainSingle(List<List<EncounterBlueprintData.CardBlueprint>> terrainDef, int difficulty)
        {
            if (terrainDef == null)
                return new() { null, null, null, null, null };

            EncounterBlueprintData data = new()
            {
                turns = new(terrainDef.Select(l => l ?? new()))
            };
            List<List<CardInfo>> resolvedCards = DiskCardGame.EncounterBuilder.BuildOpponentTurnPlan(data, difficulty, false);
            return resolvedCards.Select(l => l == null || l.Count == 0 ? null : l[0]).ToList();
        }

        private static Tuple<List<CardInfo>, List<CardInfo>, List<CardInfo>> GetTerrainForBlueprint(EncounterBlueprintData data, int difficulty)
        {
            if (data == null || string.IsNullOrEmpty(data.name))
                return null;

            // Use the game's internal turn plan builder to convert terrain blueprints to actual cards
            List<List<EncounterBlueprintData.CardBlueprint>> playerData = PlayerTerrainsResolved.ContainsKey(data.name) ? PlayerTerrainsResolved[data.name] : null;
            List<List<EncounterBlueprintData.CardBlueprint>> enemyData = OpposingTerrainsResolved.ContainsKey(data.name) ? OpposingTerrainsResolved[data.name] : null;
            List<List<EncounterBlueprintData.CardBlueprint>> enemyQueue = OpposingTerrainQueuesResolved.ContainsKey(data.name) ? OpposingTerrainQueuesResolved[data.name] : null;

            return playerData == null && enemyData == null && enemyQueue == null
                ? null
                : new(
                ConvertTerrainSingle(playerData, difficulty),
                ConvertTerrainSingle(enemyData, difficulty),
                ConvertTerrainSingle(enemyQueue, difficulty)
            );
        }

        [HarmonyPatch(typeof(HoloMapNode), nameof(HoloMapNode.CreateCardBattleData))]
        [HarmonyPostfix]
        private static void AddSpecificTerrain(HoloMapNode __instance, CardBattleNodeData __result)
        {
            if (!P03AscensionSaveData.IsP03Run || __instance.bridgeBattle)
                return;

            Tuple<List<CardInfo>, List<CardInfo>, List<CardInfo>> terrain = GetTerrainForBlueprint(__result.blueprint, __result.difficulty);

            if (terrain == null)
                return;

            EncounterData.StartCondition startCondition = new()
            {
                cardsInPlayerSlots = terrain.Item1.ToArray(),
                cardsInOpponentSlots = terrain.Item2.ToArray(),
                cardsInOpponentQueue = terrain.Item3.ToArray()
            };

            __result.PredefinedStartConditions = new() { startCondition };
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

        [HarmonyPatch(typeof(Part3Opponent), nameof(Part3Opponent.QueueNewCards))]
        [HarmonyPostfix]
        private static IEnumerator RepeatTerrain(IEnumerator sequence, Part3Opponent __instance, bool doTween, bool changeView)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            if (__instance.Blueprint == null || String.IsNullOrEmpty(__instance.Blueprint.name))
            {
                yield return sequence;
                yield break;
            }

            if (!OpposingTerrainRepeatRules.ContainsKey(__instance.Blueprint.name))
            {
                yield return sequence;
                yield break;
            }

            if (!OpposingTerrainsResolved.ContainsKey(__instance.Blueprint.name))
            {
                yield return sequence;
                yield break;
            }

            if (!OpposingTerrainRepeatRules[__instance.Blueprint.name]())
            {
                yield return sequence;
                yield break;
            }

            List<CardInfo> terrainToQueue = ConvertTerrainSingle(OpposingTerrainsResolved[__instance.Blueprint.name], __instance.Difficulty);
            if (terrainToQueue == null)
            {
                yield return sequence;
                yield break;
            }

            CardInfo[] queue = terrainToQueue.ToArray();
            CustomTriggerFinder.CallAll<IModifyTerrain>(false, t => true, t => t.ModifyOpponentQueuedTerrain(queue));
            terrainToQueue.Clear();
            terrainToQueue.AddRange(queue);

            // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
            // foreach (NonCardTriggerReceiver trigger in receivers)
            // {
            //     if (!trigger.SafeIsUnityNull() && trigger is IModifyTerrain imt)
            //     {
            //         CardInfo[] queue = terrainToQueue.ToArray();
            //         imt.ModifyOpponentQueuedTerrain(queue);
            //         terrainToQueue.Clear();
            //         terrainToQueue.AddRange(queue);
            //     }
            // }

            for (int i = 0; i < terrainToQueue.Count; i++)
            {
                P03Plugin.Log.LogInfo($"Repeating terrain queue {terrainToQueue[i]} in slot {i}");
                if (terrainToQueue[i] == null)
                    continue;

                // See if there's already a card in this slot
                if (__instance.Queue.Any(pc => pc.QueuedSlot.Index == i))
                    continue;

                yield return __instance.QueueCard(terrainToQueue[i], BoardManager.Instance.OpponentSlotsCopy[i], doTween, changeView);
            }

            yield return sequence;
            yield break;
        }
    }
}