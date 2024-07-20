using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.BattleMods;
using Infiniscryption.P03KayceeRun.Quests;
using Infiniscryption.P03KayceeRun.Sequences;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static partial class RunBasedHoloMap
    {
        private static readonly int[][] NSEW = new int[][] { new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { -1, 0 } };

        private static CardTemple ToTemple(this Zone zone)
        {
            return zone == Zone.Magic
                ? CardTemple.Wizard
                : zone == Zone.Nature ? CardTemple.Nature : zone == Zone.Undead ? CardTemple.Undead : CardTemple.Tech;
        }

        private static IEnumerable<HoloMapBlueprint> AdjacentToQuadrant(this HoloMapBlueprint[,] map, int x, int y)
        {
            int minX = x <= 2 ? 0 : 3;
            int minY = y <= 2 ? 0 : 3;
            int maxX = x <= 2 ? 2 : 5;
            int maxY = y <= 2 ? 2 : 5;

            return NSEW.Where(p => x + p[0] >= minX &&
                                   y + p[1] >= minY &&
                                   x + p[0] <= maxX &&
                                   y + p[1] <= maxY)
                       .Select(p => map[x + p[0], y + p[1]]);
        }

        private static int OppositeDirection(int direction)
        {
            int retval = 0;
            if ((direction & NORTH) != 0)
                retval |= SOUTH;
            if ((direction & SOUTH) != 0)
                retval |= NORTH;
            if ((direction & EAST) != 0)
                retval |= WEST;
            if ((direction & WEST) != 0)
                retval |= EAST;
            return retval;
        }

        public static IEnumerable<HoloMapBlueprint> AdjacentToQuadrant(this HoloMapBlueprint[,] map, HoloMapBlueprint node) => map.AdjacentToQuadrant(node.x, node.y);

        public static IEnumerable<HoloMapBlueprint> AdjacentTo(this HoloMapBlueprint[,] map, int x, int y)
        {
            return NSEW.Where(p => x + p[0] >= 0 &&
                                   y + p[1] >= 0 &&
                                   x + p[0] < map.GetLength(0) &&
                                   y + p[1] < map.GetLength(1))
                       .Select(p => map[x + p[0], y + p[1]]);
        }

        public static Tuple<int, int> GetAdjacentLocation(this HoloMapBlueprint node, int direction)
        {
            int x = direction == WEST ? node.x - 1 : direction == EAST ? node.x + 1 : node.x;
            int y = direction == NORTH ? node.y - 1 : direction == SOUTH ? node.y + 1 : node.y;
            return x < 0 || x >= 6 || y < 0 || y >= 6 ? null : new(x, y);
        }

        public static HoloMapBlueprint GetAdjacentNode(int x, int y, HoloMapBlueprint[,] map, int direction)
        {
            x = direction == WEST ? x - 1 : direction == EAST ? x + 1 : x;
            y = direction == NORTH ? y - 1 : direction == SOUTH ? y + 1 : y;
            return x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1) ? null : map[x, y];
        }

        public static HoloMapBlueprint[,] ToMap(this List<HoloMapBlueprint> bp)
        {
            HoloMapBlueprint[,] retval = new HoloMapBlueprint[6, 6];
            foreach (HoloMapBlueprint hmb in bp)
                retval[hmb.x, hmb.y] = hmb;

            return retval;
        }

        public static HoloMapBlueprint GetAdjacentNode(this HoloMapBlueprint node, List<HoloMapBlueprint> map, int direction)
        {
            Tuple<int, int> newLoc = GetAdjacentLocation(node, direction);
            return newLoc == null ? null : map.FirstOrDefault(bp => bp.x == newLoc.Item1 && bp.y == newLoc.Item2);
        }

        public static HoloMapBlueprint GetAdjacentNode(this HoloMapBlueprint node, HoloMapBlueprint[,] map, int direction) => GetAdjacentNode(node.x, node.y, map, direction);

        public static List<HoloMapBlueprint> GetPointOfInterestNodes(this List<HoloMapBlueprint> nodes, Func<HoloMapBlueprint, bool> filter = null)
        {
            Func<HoloMapBlueprint, bool> activeFilter = filter ?? ((HoloMapBlueprint i) => true);
            List<HoloMapBlueprint> deadEndPOI = nodes.Where(activeFilter).Where(bp => bp.IsDeadEnd && bp.EligibleForUpgrade).ToList();
            return deadEndPOI.Count > 0 ? deadEndPOI : nodes.Where(activeFilter).Where(bp => bp.EligibleForUpgrade).ToList();
        }

        public static HoloMapBlueprint GetRandomPointOfInterest(this List<HoloMapBlueprint> nodes, Func<HoloMapBlueprint, bool> filter = null, int randomSeed = -1)
        {
            if (randomSeed != -1)
                UnityEngine.Random.InitState(randomSeed);

            List<HoloMapBlueprint> possibles = nodes.GetPointOfInterestNodes(filter: filter);
            return possibles.Count == 0 ? null : possibles[UnityEngine.Random.Range(0, possibles.Count)];
        }

        public static IEnumerable<HoloMapBlueprint> AdjacentTo(this HoloMapBlueprint[,] map, HoloMapBlueprint node) => map.AdjacentTo(node.x, node.y);

        public static int DirTo(this HoloMapBlueprint start, HoloMapBlueprint end)
        {
            int retval = BLANK;
            retval |= start.x == end.x ? 0 : start.x < end.x ? EAST : WEST;
            retval |= start.y == end.y ? 0 : start.y < end.y ? SOUTH : NORTH;
            return retval;
        }

        private static void CrawlQuadrant(HoloMapBlueprint[,] map, int color)
        {
            List<HoloMapBlueprint> possibles = new();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] != null && map[i, j].color == color)
                        possibles.Add(map[i, j]);
                }
            }

            HoloMapBlueprint startNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
            CrawlQuadrant(map, startNode.x, startNode.y);
        }

        private static void CrawlQuadrant(HoloMapBlueprint[,] map, int x, int y)
        {
            // Find all adjacent uncrawled nodes
            List<HoloMapBlueprint> uncrawled = map.AdjacentTo(x, y)
                                                  .Where(bp => bp != null)
                                                  .Where(bp => map[x, y].color == bp.color)
                                                  .Where(bp => bp.arrowDirections == BLANK)
                                                  .ToList();

            if (uncrawled.Count == 0)
                return;

            // Pick a random adjacent uncrawled node
            HoloMapBlueprint current = map[x, y];
            HoloMapBlueprint next = uncrawled[UnityEngine.Random.Range(0, uncrawled.Count)];
            current.arrowDirections |= current.DirTo(next);
            next.arrowDirections |= next.DirTo(current);

            CrawlQuadrant(map, next.x, next.y);

            // double check this one again
            CrawlQuadrant(map, x, y);
        }

        private static void ConnectQuadrants(HoloMapBlueprint[,] map, Zone region)
        {
            // This is too hard to generalize, although maybe I'll come up with a way to do it?
            int v = region == Zone.Magic ? 3 : 2;

            if (region == Zone.Tech)
            {
                // Now we need to set up some very important rooms:
                // The south elevator
                // The north elevator
                // The boss room
                var southElevator = map[3, 3];
                var northElevator = map[3, 2];
                var bossRoom = map[3, 1];

                northElevator.arrowDirections |= SOUTH;
                southElevator.arrowDirections |= NORTH;
                southElevator.specialTerrain |= HoloMapBlueprint.LANDMARKER;
                northElevator.specialTerrain |= HoloMapBlueprint.LANDMARKER;
                northElevator.specialTerrain |= HoloMapBlueprint.FAST_TRAVEL_NODE;
                northElevator.specialTerrain |= HoloMapBlueprint.NORTH_BUILDING_ENTRANCE;
                northElevator.arrowDirections |= NORTH;
                northElevator.color = 4; // It's in the upper half, but you aren't in the factory yet
                bossRoom.opponent = Opponent.Type.TelegrapherBoss;
                bossRoom.arrowDirections = WEST | EAST | SOUTH;

                map[2, 1] ??= new(123459) { x = 2, y = 1, arrowDirections = BLANK };
                map[4, 1] ??= new(123469) { x = 4, y = 1, arrowDirections = BLANK };
                map[2, 1].arrowDirections |= EAST;
                map[4, 1].arrowDirections |= WEST;
            }
            else
            {
                for (int i = 2; i >= 0; i--)
                {
                    if (map[i, v] != null && map[i, v + 1] != null)
                    {
                        map[i, v].arrowDirections = map[i, v].arrowDirections | SOUTH;
                        map[i, v + 1].arrowDirections = map[i, v + 1].arrowDirections | NORTH;
                        break;
                    }
                }

                for (int i = 3; i <= 5; i++)
                {
                    if (map[i, v] != null && map[i, v + 1] != null)
                    {
                        map[i, v].arrowDirections = map[i, v].arrowDirections | SOUTH;
                        map[i, v + 1].arrowDirections = map[i, v + 1].arrowDirections | NORTH;

                        // Add the landmarks for the undead zone
                        map[i, v + 1].specialTerrain |= HoloMapBlueprint.LANDMARKER;

                        break;
                    }
                }
            }

            if (region != Zone.Tech)
            {
                for (int j = v; j >= 0; j--)
                {
                    if (map[2, j] != null && map[3, j] != null)
                    {
                        map[2, j].arrowDirections = map[2, j].arrowDirections | EAST;
                        map[3, j].arrowDirections = map[3, j].arrowDirections | WEST;

                        // Add the landmarks for the undead zone
                        map[2, j].specialTerrain |= HoloMapBlueprint.LANDMARKER;

                        break;
                    }
                }
            }

            for (int j = v + 1; j <= 5; j++)
            {
                if (map[2, j] != null && map[3, j] != null)
                {
                    map[2, j].arrowDirections = map[2, j].arrowDirections | EAST;
                    map[3, j].arrowDirections = map[3, j].arrowDirections | WEST;
                    break;
                }
            }
        }

        private static void DiscoverAndTrimDeadEnds(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // Look for all 'dead ends' - that is, nodes where you can only move in one direction
            // The goal of this is to find pointless hallways; that is, a path that simply leads to a dead end with nothing interesting.
            // There's no need to have to walk through a hallway just to get to a dead end
            // This trims those by removing the dead end and turning the hallway into the dead end.

            // It should make the maps feel smaller. Which is good, actually. There's not a lot to do on these maps.

            // We ignore the first node, because that's the starting node. And we can't risk killing the starting node
            List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.NumberOfArrows == 1 && bp != nodes[0]).ToList();
            int i = 0;
            foreach (HoloMapBlueprint deadEnd in possibles)
            {
                HoloMapBlueprint adjacent = deadEnd.GetAdjacentNode(map, deadEnd.arrowDirections);

                // If the node leading into a dead end only has two directions
                // And the color of the dead end has more than two nodes
                // We kill the dead end and make the hall leading into it into a dead end
                if (adjacent.NumberOfArrows == 2 && nodes.Where(bp => bp.color == deadEnd.color).Count() > 2)
                {
                    // Kill the arrow going into the dead end node
                    // Right, so, the arrow going to the dead end will be the opposite direction of the arrow leaving the dead end
                    // We AND with the complement to get rid of it
                    adjacent.arrowDirections &= ~OppositeDirection(deadEnd.arrowDirections);

                    // Now just delete the node
                    map[deadEnd.x, deadEnd.y] = null;
                    nodes.Remove(deadEnd);
                }

                i++;
                if (i >= 2)
                    break;
            }
        }

        private static void DiscoverAndCreateLandmarks(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            for (int c = 1; c <= 4; c++)
            {
                List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.color == c && bp.NumberOfArrows >= 3).ToList();
                if (possibles.Count == 0)
                    possibles = nodes.Where(bp => bp.color == c && bp.NumberOfArrows == 2).ToList();
                if (possibles.Count == 0)
                    possibles = nodes.Where(bp => bp.color == c).ToList();

                HoloMapBlueprint landmarkNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
                landmarkNode.specialTerrain |= HoloMapBlueprint.LANDMARKER;
            }
        }

        private static void DiscoverAndCreateHoloTraps(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            for (int i = 1; i < 5; i++)
            {
                List<HoloMapBlueprint> possibles = nodes.Where(bp =>
                        (bp.color == i || i == 0)
                        && bp.specialTerrain == 0
                        && bp.isSecretRoom == false
                        && bp.opponent == Opponent.Type.Default
                        && bp.dialogueEvent == SpecialEvent.None
                ).ToList();

                if (possibles.Count == 0)
                    possibles = nodes.Where(bp =>
                            bp.specialTerrain == 0
                            && bp.isSecretRoom == false
                            && bp.opponent == Opponent.Type.Default
                            && bp.dialogueEvent == SpecialEvent.None
                    ).ToList();

                if (possibles.Count == 0)
                    possibles = nodes.Where(bp =>
                            bp.isSecretRoom == false
                            && bp.opponent == Opponent.Type.Default
                            && (bp.specialTerrain & HoloMapBlueprint.HOLO_PELT_MINIGAME) == 0
                            && bp.dialogueEvent == SpecialEvent.None
                    ).ToList();

                if (possibles.Count == 0)
                    possibles = nodes.Where(bp =>
                            bp.isSecretRoom == false
                            && bp.opponent == Opponent.Type.Default
                            && (bp.specialTerrain & HoloMapBlueprint.HOLO_PELT_MINIGAME) == 0
                    ).ToList();

                if (possibles.Count == 0)
                    possibles = nodes.Where(bp =>
                            (bp.specialTerrain & HoloMapBlueprint.HOLO_PELT_MINIGAME) == 0
                            && bp.opponent == Opponent.Type.Default
                    ).ToList();

                HoloMapBlueprint holoTrapNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
                holoTrapNode.specialTerrain |= HoloMapBlueprint.HOLO_PELT_MINIGAME;
            }
        }

        private static void DiscoverAndCreateBridge(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, Zone region)
        {
            if (region is Zone.Nature or Zone.Tech)
                return; // Nature doesn't have bridges for flavor, tech doesn't have bridges for mechanics

            // This is a goofy one. We're looking for a section on the map where the area could be a bridge.
            // If so, roll the dice and make a bridge
            float bridgeOdds = 0.95f;
            List<HoloMapBlueprint> bridgeNodes = nodes.Where(bp => bp.arrowDirections == (EAST | WEST)).ToList();
            while (bridgeNodes.Count > 0 && bridgeOdds > 0f)
            {
                HoloMapBlueprint bridge = bridgeNodes[UnityEngine.Random.Range(0, bridgeNodes.Count)];
                if (UnityEngine.Random.value < bridgeOdds)
                {
                    bridge.specialTerrain |= HoloMapBlueprint.FULL_BRIDGE;
                    map[bridge.x - 1, bridge.y].specialTerrain |= HoloMapBlueprint.LEFT_BRIDGE;
                    map[bridge.x + 1, bridge.y].specialTerrain |= HoloMapBlueprint.RIGHT_BRIDGE;
                    bridgeOdds -= 0.25f;
                }
                bridgeNodes.Remove(bridge);
            }
        }

        private static bool DiscoverAndCreateTrade(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // The goal here is to find battles that exist in hallways; that is, battles where the room that
            // are in when you click the arrow to fight only has one way in.
            // Then we make the entrace to THAT room force you to trade your way in

            // So again: the room must have a 'special' arrow, the type must be enemy, and there can be only two arrows
            // One would be the special direction, and one would be the path backward
            List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.color != nodes[0].color && bp.specialDirection != 0 && bp.specialDirectionType == HoloMapBlueprint.BATTLE && bp.NumberOfArrows == 2).ToList();

            if (possibles.Count == 0)
                return false; // This means we couldn't find a spot for the

            HoloMapBlueprint battleNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];

            // Find the way back. It's the arrow directions with the special direction removed
            int directionBack = battleNode.arrowDirections & ~battleNode.specialDirection;

            // Find the node in that direction
            HoloMapBlueprint prevNode = GetAdjacentNode(battleNode, map, directionBack);

            // Set the special direction and special type
            prevNode.specialDirection = OppositeDirection(directionBack);
            prevNode.specialDirectionType = HoloMapBlueprint.TRADE;

            return true;
        }

        private static bool ForceTrade(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // This gets called when we couldn't naturally fit a trade in the map
            // Here, we pick a random encounter from the map and move it to make room for a trade
            List<HoloMapBlueprint> possibles = nodes.Where(bp => bp.specialDirection != 0 && bp.specialDirectionType == 0 && bp.color != 1).ToList();

            while (possibles.Count > 0)
            {
                HoloMapBlueprint oldBattleNode = possibles[UnityEngine.Random.Range(0, possibles.Count)];
                possibles.Remove(oldBattleNode);

                // Find the node that the battle leads you to
                HoloMapBlueprint newBattleNode = GetAdjacentNode(oldBattleNode, map, oldBattleNode.specialDirection);

                // Find a location in any direction that is null
                foreach (int dir in DIR_LOOKUP.Keys)
                {
                    Tuple<int, int> xy = GetAdjacentLocation(newBattleNode, dir);

                    if (xy == null)
                        continue;

                    if (map[xy.Item1, xy.Item2] == null)
                    {
                        // Good. We found a spot
                        HoloMapBlueprint brandNewNode = new(newBattleNode.randomSeed + 1000)
                        {
                            x = xy.Item1,
                            y = xy.Item2
                        };

                        nodes.Add(brandNewNode);
                        map[xy.Item1, xy.Item2] = brandNewNode;

                        // Give the new node the upgrade that was hiding behind the battle
                        brandNewNode.upgrade = newBattleNode.upgrade;
                        newBattleNode.upgrade = HoloMapNode.NodeDataType.MoveArea;

                        // Make all arrows match up
                        brandNewNode.arrowDirections = brandNewNode.DirTo(newBattleNode);
                        newBattleNode.arrowDirections |= newBattleNode.DirTo(brandNewNode);
                        newBattleNode.specialDirection = newBattleNode.DirTo(brandNewNode);

                        // Make the new battle have the right info
                        newBattleNode.encounterDifficulty = oldBattleNode.encounterDifficulty;
                        newBattleNode.encounterIndex = oldBattleNode.encounterIndex;

                        // Make the old battle node into trade node
                        oldBattleNode.specialDirectionType = HoloMapBlueprint.TRADE;

                        return true;
                    }
                }
            }

            return false;
        }

        private static bool DiscoverAndCreateEnemyEncounter(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int tier, Zone region, HoloMapNode.NodeDataType reward, List<int> usedIndices, int color = -1)
        {
            // The goal here is to find four rooms that have only one entrance
            // Then back out to the first spot that doesn't have a choice
            // Then put an enemy encounter there
            // And put something of interest in the 

            HoloMapBlueprint enemyNode = null;
            HoloMapBlueprint rewardNode = null;
            if (color == nodes[0].color && nodes[0].NumberOfArrows == 1) // This bit only works if there's only one way out of the starting node
            {
                // If this is the region you start in, we do the work a little bit differently.
                // We walk until we find the first node with a choice
                enemyNode = nodes[0];
                rewardNode = enemyNode.GetAdjacentNode(map, enemyNode.arrowDirections);
                for (int i = 0; i < 3; i++)
                {
                    if (rewardNode.NumberOfArrows == 2)
                    {
                        int dirToEnemyNode = DirTo(rewardNode, enemyNode);
                        int dirToNextRewardNode = rewardNode.arrowDirections & ~dirToEnemyNode;
                        HoloMapBlueprint nextRewardNode = rewardNode.GetAdjacentNode(map, dirToNextRewardNode);
                        enemyNode = rewardNode;
                        rewardNode = nextRewardNode;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                rewardNode = nodes.GetRandomPointOfInterest(bp => (bp.color == color || color == -1) && bp.IsDeadEnd);
            }

            if (rewardNode != null)
            {
                enemyNode ??= rewardNode.GetAdjacentNode(map, rewardNode.arrowDirections);
                enemyNode.specialDirection = DirTo(enemyNode, rewardNode);
                enemyNode.encounterDifficulty = EventManagement.EncounterDifficulty;

                if (enemyNode.color == nodes[0].color)
                {
                    bool assigned = false;
                    if (P03Plugin.Instance.DebugCode.Contains("neutralencounter["))
                    {
                        int startIndex = P03Plugin.Instance.DebugCode.IndexOf("neutralencounter[");
                        string encounterDataString = P03Plugin.Instance.DebugCode.Substring(startIndex).Replace("neutralencounter[", "").Split(']')[0];
                        for (int i = 0; i < REGION_DATA[Zone.Neutral].Region.encounters.Count; i++)
                        {
                            if (REGION_DATA[Zone.Neutral].Region.encounters[i].name.Equals(encounterDataString, StringComparison.InvariantCultureIgnoreCase))
                            {
                                enemyNode.encounterIndex = i;
                                assigned = true;
                                break;
                            }
                        }
                    }
                    if (!assigned)
                    {
                        enemyNode.encounterIndex = UnityEngine.Random.Range(0, REGION_DATA[Zone.Neutral].Region.encounters.Count);
                    }
                    //enemyNode.encounterIndex = REGION_DATA[Zone.Neutral].encounters.Length - 1;
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, REGION_DATA[region].Region.encounters.Count);
                    int sanity = 0;
                    while (usedIndices.Contains(index) && sanity < 20)
                    {
                        index += 1;
                        sanity += 1;
                        if (index == REGION_DATA[region].Region.encounters.Count)
                            index = 0;
                    }
                    enemyNode.encounterIndex = index;
                    usedIndices.Add(index);
                }

                // 50% change of terrain
                if (UnityEngine.Random.value < 0.5f)
                    enemyNode.battleTerrainIndex = UnityEngine.Random.Range(0, REGION_DATA[region].Terrain.Length) + 1;

                rewardNode.upgrade = reward;

                // Make sure the reward node and the enemy node have the same quandrant marking
                // AFTER this has been sorted out
                rewardNode.color = enemyNode.color;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void DiscoverAndCreateCanvasBoss(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            // We need two spaces vertically to make this work.
            HoloMapBlueprint bossIntroRoom = null;
            for (int j = map.GetLength(1) - 1; j >= 2; j--)
            {
                for (int i = map.GetLength(0) - 1; i >= 0; i--)
                {
                    if (map[i, j] != null && map[i, j - 1] == null && map[i, j - 2] == null)
                    {
                        bossIntroRoom = map[i, j];
                        break;
                    }
                }
                if (bossIntroRoom != null)
                    break;
            }

            // Now we've found the space
            bossIntroRoom.specialTerrain |= HoloMapBlueprint.NORTH_BUILDING_ENTRANCE;
            bossIntroRoom.specialTerrain |= HoloMapBlueprint.FAST_TRAVEL_NODE;
            bossIntroRoom.specialTerrain &= ~HoloMapBlueprint.LANDMARKER;
            bossIntroRoom.arrowDirections |= NORTH;
            bossIntroRoom.blockedDirections |= NORTH;
            bossIntroRoom.blockEvent = EventManagement.ALL_ZONE_ENEMIES_KILLED;

            // We need to create the special lower tower room
            HoloMapBlueprint lowerTowerRoom = new(0)
            {
                specialTerrain = HoloMapBlueprint.LOWER_TOWER_ROOM,
                arrowDirections = NORTH | SOUTH,
                x = bossIntroRoom.x,
                y = bossIntroRoom.y - 1,
                color = bossIntroRoom.color
            };
            map[lowerTowerRoom.x, lowerTowerRoom.y] = lowerTowerRoom;
            nodes.Add(lowerTowerRoom);

            HoloMapBlueprint bossRoom = new(0)
            {
                opponent = Opponent.Type.CanvasBoss,
                arrowDirections = SOUTH,
                x = bossIntroRoom.x,
                y = bossIntroRoom.y - 2,
                color = bossIntroRoom.color
            };
            map[bossRoom.x, bossRoom.y] = bossRoom;
            nodes.Add(bossRoom);

        }

        private static bool CanHaveSecretRoom(this HoloMapBlueprint node, HoloMapBlueprint[,] map, int direction, bool strict = true) => node.opponent == Opponent.Type.Default && node.GetAdjacentLocation(direction) != null && node.GetAdjacentNode(map, direction) == null && ((node.arrowDirections & OppositeDirection(direction)) == 0 || !strict);

        private static bool CanHaveSecretRoom(this HoloMapBlueprint node, HoloMapBlueprint[,] map, bool strict = true)
        {
            // You can only have a secret room if:
            // a) You have an adjacent null space
            // b) You do NOT have an exit in the opposite direction of that adjacent null space
            //    WHY? Let's say you have an exit to the LEFT 
            //    That means that your neighbor has an exit to the RIGHT
            //    If the secret direction is to the RIGHT, the player will have their mouse cursor over the spot
            //    where the secret arrow is automatically. It makes the secret not really a secret.
            //    If strict is false, this second condition is ignored (I'd rather have an easy to find secret room
            //    than fail to create one at all)
            foreach (int dir in DIR_LOOKUP.Keys)
            {
                if (node.CanHaveSecretRoom(map, dir, strict: strict))
                    return true;
            }

            return false;
        }

        private static void DiscoverAndCreateSecretRoom(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, int randomSeed)
        {
            // Get a list of all possible neighbors
            List<HoloMapBlueprint> neighbors = nodes.Where(bp => bp.CanHaveSecretRoom(map)).ToList();
            if (neighbors.Count == 0) // 
                neighbors = nodes.Where(bp => bp.CanHaveSecretRoom(map, strict: false)).ToList();

            // Pick a random neighbor
            P03Plugin.Log.LogDebug($"Found {neighbors.Count} possibilities for secret room");
            HoloMapBlueprint secretSpaceNeighbor = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];

            // Figure out the direction for the secret room
            int adjDir = 0;
            foreach (int dir in DIR_LOOKUP.Keys)
            {
                if (secretSpaceNeighbor.CanHaveSecretRoom(map, dir))
                {
                    adjDir = dir;
                    break;
                }
            }
            if (adjDir == 0)
            {
                foreach (int dir in DIR_LOOKUP.Keys)
                {
                    if (secretSpaceNeighbor.CanHaveSecretRoom(map, dir, strict: false))
                    {
                        adjDir = dir;
                        break;
                    }
                }
            }

            Tuple<int, int> location = secretSpaceNeighbor.GetAdjacentLocation(adjDir);

            // Create a new space on the map
            HoloMapBlueprint newBp = new(randomSeed);
            nodes.Add(newBp);

            newBp.x = location.Item1;
            newBp.y = location.Item2;
            map[newBp.x, newBp.y] = newBp;

            newBp.arrowDirections |= OppositeDirection(adjDir);
            newBp.isSecretRoom = true;
            secretSpaceNeighbor.arrowDirections |= adjDir;
            secretSpaceNeighbor.secretDirection |= adjDir;
            newBp.color = secretSpaceNeighbor.color;

            if (EventManagement.CompletedZones.Count == 2 || (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("goobert") && EventManagement.CompletedZones.Count == 0))
                newBp.specialTerrain = HoloMapBlueprint.MYCOLOGIST_WELL;
        }

        private static void DiscoverAndCreateBossRoom(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes, Zone region)
        {
            if (region == Zone.Neutral)
                return;

            if (region == Zone.Magic)
            {
                DiscoverAndCreateCanvasBoss(map, nodes);
                return;
            }

            // We need a room that has a blank room above it
            // And does not have the same color as the starting room
            Func<HoloMapBlueprint, bool> colorCondition = (bp) => bp.color != nodes[0].color && (region != Zone.Undead || bp.color == 3);
            List<HoloMapBlueprint> bossPossibles = nodes.Where(bp => bp.y >= 1 && map[bp.x, bp.y - 1] == null && colorCondition(bp)).ToList();
            HoloMapBlueprint bossIntroRoom = bossPossibles[UnityEngine.Random.Range(0, bossPossibles.Count)];

            if (region == Zone.Nature)
                bossIntroRoom.specialTerrain |= HoloMapBlueprint.NORTH_CABIN;

            bossIntroRoom.specialTerrain |= HoloMapBlueprint.FAST_TRAVEL_NODE;

            if (region != Zone.Undead)
                bossIntroRoom.specialTerrain &= ~HoloMapBlueprint.LANDMARKER;
            else
                bossIntroRoom.specialTerrain |= HoloMapBlueprint.LANDMARKER;

            bossIntroRoom.arrowDirections |= NORTH;
            bossIntroRoom.blockedDirections |= NORTH;
            bossIntroRoom.blockEvent = EventManagement.ALL_ZONE_ENEMIES_KILLED;

            HoloMapBlueprint bossRoom = new(bossIntroRoom.randomSeed + (200 * bossIntroRoom.x))
            {
                x = bossIntroRoom.x,
                y = bossIntroRoom.y - 1,
                opponent = (region == Zone.Undead) ? Opponent.Type.ArchivistBoss : (region == Zone.Nature ? Opponent.Type.PhotographerBoss : Opponent.Type.TelegrapherBoss)
            };
            bossRoom.arrowDirections |= SOUTH;
            bossRoom.color = bossIntroRoom.color;

            map[bossRoom.x, bossRoom.y] = bossRoom;
            nodes.Add(bossRoom);
        }

        private static List<HoloMapBlueprint> BuildHubBlueprint(int seed)
        {
            List<HoloMapBlueprint> retval = new() {
                new(seed) { upgrade = HoloMapNode.NodeDataType.FastTravel, x = 0, y = 2, arrowDirections = NORTH | EAST, secretDirection = EAST },
                new(seed) { specialTerrain = HoloMapBlueprint.FINAL_SHOP_NODE, x = 0, y = 1, arrowDirections = NORTH | SOUTH | EAST, secretDirection = EAST },
                new(seed) { specialTerrain = HoloMapBlueprint.TEST_OF_STREGTH, x = 1, y = 1, arrowDirections = WEST, isSecretRoom = true },
                new(seed) { specialTerrain = HoloMapBlueprint.MIRROR, x = 1, y = 2, arrowDirections = WEST, isSecretRoom = true },
                new(seed) { opponent = Opponent.Type.P03Boss, x = 0, y = 0, arrowDirections = SOUTH }
            };

            return retval;
        }

        private static Tuple<int, int> BuildTurboRoom(List<HoloMapBlueprint> map, int seed, Tuple<int, int> loc, HoloMapNode.NodeDataType? reward = null, int? encounterIndex = null, int specialType = HoloMapBlueprint.BATTLE)
        {
            int x = loc.Item1;
            int y = loc.Item2;
            HoloMapBlueprint bp = new(seed) { x = x, y = y };

            // The only guaranteed direction is backwards
            if (x == 0 && y != 2)
                bp.arrowDirections |= y % 2 == 0 ? NORTH : EAST;
            else if (x == 5)
                bp.arrowDirections |= y % 2 == 0 ? WEST : NORTH;
            else if (y % 2 == 0 && !(x == 0 && y == 2))
                bp.arrowDirections |= WEST;
            else if (y % 2 == 1)
                bp.arrowDirections |= EAST;

            if (encounterIndex.HasValue)
            {
                if (y % 2 == 0)
                    bp.specialDirection = x == 5 ? SOUTH : EAST;
                if (y % 2 == 1)
                    bp.specialDirection = x == 0 ? SOUTH : WEST;
                bp.arrowDirections |= bp.specialDirection;
                bp.specialDirectionType = specialType;
                bp.encounterIndex = encounterIndex.Value;
                bp.encounterDifficulty = EventManagement.EncounterDifficulty;

                if (encounterIndex.Value == 0 && !(x == 0 && y == 2))
                    bp.specialTerrain |= HoloMapBlueprint.FAST_TRAVEL_NODE;
            }
            if (reward.HasValue)
            {
                bp.upgrade = reward.Value;
            }

            if (!reward.HasValue && !encounterIndex.HasValue)
                bp.specialTerrain |= HoloMapBlueprint.FAST_TRAVEL_NODE;

            map.Add(bp);
            P03Plugin.Log.LogDebug($"Turbo Room [{x}, {y}] DIR={bp.arrowDirections} S={bp.specialDirection}");

            return x == 0
                ? y % 2 == 0 ? new(x + 1, y) : new(x, y + 1)
                : x == 5 ? y % 2 == 0 ? new(x, y + 1) : new(x - 1, y) : y % 2 == 0 ? new(x + 1, y) : new(x - 1, y);
        }

        // Builds one room for each regional encounter, then gives you fast travel to leave
        // then gives you one room for each neutral encounter, then gives you fast travel to leave again
        private static List<HoloMapBlueprint> BuildTurboBlueprint(int seed, Zone zone)
        {
            List<HoloMapBlueprint> retval = new();

            Tuple<int, int> roomPos = new(0, 2);
            RegionGeneratorData data = REGION_DATA[zone];
            List<HoloMapNode.NodeDataType> rewards = new() {
                HoloMapNode.NodeDataType.CardChoice,
                HoloMapNode.NodeDataType.AddCardAbility,
                data.DefaultReward,
                HoloMapNode.NodeDataType.AddCardAbility,
                data.DefaultReward
            };
            for (int i = 0; i < data.Region.encounters.Count; i++)
            {
                HoloMapNode.NodeDataType? reward = null;
                if (rewards.Count > 0)
                {
                    reward = rewards[0];
                    rewards.RemoveAt(0);
                }

                roomPos = BuildTurboRoom(retval, seed++, roomPos, reward, i);
            }

            RegionGeneratorData neutral = REGION_DATA[Zone.Neutral];
            for (int i = 0; i < neutral.Region.encounters.Count; i++)
            {
                HoloMapNode.NodeDataType? reward = null;
                if (rewards.Count > 0)
                {
                    reward = rewards[0];
                    rewards.RemoveAt(0);
                }

                roomPos = BuildTurboRoom(retval, seed++, roomPos, reward, i);
            }

            BuildTurboRoom(retval, seed++, roomPos);

            return retval;
        }

        private static List<HoloMapBlueprint> BuildMycologistBlueprint(int seed)
        {
            List<HoloMapBlueprint> retval = new() {
                new(seed++) { x = 0, y = 2, arrowDirections = EAST, specialDirection = EAST, specialDirectionType = HoloMapBlueprint.TRADE },
                new(seed++) { x = 1, y = 2, arrowDirections = WEST | NORTH, specialDirection = NORTH, specialDirectionType = HoloMapBlueprint.TRADE },
                new(seed++) { x = 1, y = 1, arrowDirections = SOUTH, opponent = Opponent.Type.MycologistsBoss }
            };
            return retval;
        }

        private static void LogBlueprint(HoloMapBlueprint[,] bpBlueprint)
        {
            // Log to the file for debug purposes
            for (int j = 0; j < bpBlueprint.GetLength(1); j++)
            {
                List<string> lines = new() { "", "", "", "", "" };
                for (int i = 0; i < bpBlueprint.GetLength(0); i++)
                {
                    for (int s = 0; s < lines.Count; s++)
                        lines[s] += bpBlueprint[i, j] == null ? "     " : bpBlueprint[i, j].DebugString[s];
                }

                for (int s = 0; s < lines.Count; s++)
                    P03Plugin.Log.LogDebug(lines[s]);
            }
        }

        private static void BuildStoryEvents(List<HoloMapBlueprint> blueprint, Zone region)
        {
            // Get all the story events we're supposed to get
            List<Tuple<SpecialEvent, Predicate<HoloMapBlueprint>>> stevents = QuestManager.GetSpecialEventForZone(region);
            foreach (Tuple<SpecialEvent, Predicate<HoloMapBlueprint>> storyData in stevents)
            {
                SpecialEvent se = storyData.Item1;
                Predicate<HoloMapBlueprint> pred = storyData.Item2;

                List<HoloMapBlueprint> locations = blueprint.Where(bp => bp.dialogueEvent == SpecialEvent.None && pred(bp)).ToList();
                if (locations.Count == 0)
                    locations = blueprint.Where(bp => bp.dialogueEvent == SpecialEvent.None).ToList();

                HoloMapBlueprint target = locations[UnityEngine.Random.Range(0, locations.Count)];
                target.dialogueEvent = se;

                // Special rule for the generator
                if (se == DefaultQuestDefinitions.BrokenGenerator.EventId)
                    target.specialTerrain = HoloMapBlueprint.BROKEN_GENERATOR;
            }
        }

        private static void FixDisconnectedRooms(HoloMapBlueprint[,] map, List<HoloMapBlueprint> nodes)
        {
            foreach (HoloMapBlueprint bp in nodes.Where(b => b.arrowDirections == 0)) // anything without arrows leaving it
            {
                foreach (int dir in DIR_LOOKUP.Keys)
                {
                    // We connect a disconnected room (that is, a room with ZERO exits)
                    // to EXACTLY ONE adjacent room. This way we guarantee not to make a loop.
                    if (bp.GetAdjacentNode(map, dir) != null)
                    {
                        bp.arrowDirections |= dir;
                        bp.GetAdjacentNode(map, dir).arrowDirections |= OppositeDirection(dir);
                        bp.color = bp.GetAdjacentNode(map, dir).color; // And, if we had to do this, we make sure that the two have the same quandrant marking
                        break;
                    }
                }
            }
        }

        public static void ShapeMapForRegion(HoloMapBlueprint[,] bpBlueprint, Zone region)
        {
            int x, y;
            if (region == Zone.Tech)
            {
                // Set the bottom corners empty
                bpBlueprint[0, 4] = bpBlueprint[0, 5] = null;
                bpBlueprint[5, 5] = bpBlueprint[5, 4] = null;

                // Remove the spots that could be north of the boss
                bpBlueprint[2, 0] = bpBlueprint[3, 0] = null;

                // Remove the spot that would be east of the landing
                bpBlueprint[4, 2] = null;

                // Randomly chop a corner
                int[] corners = new int[] { 1, 4 };
                bpBlueprint[corners[UnityEngine.Random.Range(0, 2)], 4] = null;

                // Make sure we leave space such that the 

                // Randomly chop an interior side on the bottom
                // But it can't be the starting space
                int[] xs = new int[] { 1, 2, 4 };
                x = xs[UnityEngine.Random.Range(0, xs.Length)];
                bpBlueprint[x, 5] = null;
            }
            if (region == Zone.Magic)
            {
                // Take off the entire top two rows
                for (int i = 0; i < 6; i++)
                {
                    bpBlueprint[i, 0] = null;

                    if (i is not 2 and not 3)
                        bpBlueprint[i, 1] = null;
                }

                // Take out one of the middle two segments
                x = UnityEngine.Random.value < 0.5f ? 2 : 3;
                bpBlueprint[x, 3] = bpBlueprint[x, 4] = null;

                // Take out a corner
                x = UnityEngine.Random.value < 0.5f ? 0 : 5;
                y = x == 0 ? 5 : UnityEngine.Random.value < 0.5f ? 2 : 5;
                bpBlueprint[x, y] = null;
            }
            if (region == Zone.Nature)
            {
                int offset = UnityEngine.Random.Range(0, 4);
                for (int i = 0; i < 3; i++)
                {
                    bpBlueprint[i + offset, 0] = null;
                    if (i == 1)
                    {
                        bpBlueprint[i + offset, 1] = null;
                        if (UnityEngine.Random.value < 0.5f)
                            bpBlueprint[i + offset - 1, 1] = null;
                        else
                            bpBlueprint[i + offset + 1, 1] = null;
                    }
                }
                if (offset <= 1)
                    bpBlueprint[0, 5] = null;
                else
                    bpBlueprint[0, 0] = null;

                offset = offset <= 1 ? 2 : 0;
                offset = UnityEngine.Random.value < 0.5f ? offset : offset + 1;

                for (int i = 0; i < 3; i++)
                {
                    bpBlueprint[i + offset, 5] = null;
                    if (i == 1)
                    {
                        bpBlueprint[i + offset, 4] = null;
                        if (UnityEngine.Random.value < 0.5f)
                            bpBlueprint[i + offset - 1, 4] = null;
                        else
                            bpBlueprint[i + offset + 1, 4] = null;
                    }
                }
                if (offset <= 1)
                    bpBlueprint[5, 5] = null;
                else
                    bpBlueprint[5, 0] = null;
            }
            if (region == Zone.Undead)
            {
                bool pointUp = UnityEngine.Random.value < 0.5f;

                int[] ys = pointUp ? new int[] { 5, 4, 1, 0 } : new int[] { 0, 1, 4, 5 };

                bpBlueprint[1, ys[0]] = bpBlueprint[2, ys[0]] = bpBlueprint[3, ys[0]] = bpBlueprint[4, ys[0]] = null;
                bpBlueprint[2, ys[1]] = bpBlueprint[3, ys[1]] = null;
                bpBlueprint[0, ys[2]] = bpBlueprint[5, ys[2]] = null;
                bpBlueprint[0, ys[3]] = bpBlueprint[1, ys[3]] = bpBlueprint[4, ys[3]] = bpBlueprint[5, ys[3]] = null;

                bpBlueprint[UnityEngine.Random.Range(2, 4), pointUp ? 2 : 3] = null;
            }

            int v = region == Zone.Magic ? 4 : 3;
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
            {
                for (int j = 0; j < bpBlueprint.GetLength(1); j++)
                {
                    if (bpBlueprint[i, j] != null)
                        bpBlueprint[i, j].color = i < 3 ? j < v ? 1 : 2 : j < v ? 3 : 4;
                }
            }
        }

        private static List<BattleModManager.BattleModDefinition> SelectMods(List<BattleModManager.BattleModDefinition> mods, int difficulty, int randomSeed)
        {
            // For 1 and 2, just pick a single mod
            if (difficulty == 1 && P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("conveyor"))
                return mods.Where(d => d.ID == ConveyorBattle.ID).ToList();

            if (difficulty <= 2)
                return mods.Where(d => d.Difficulty == difficulty).OrderBy(d => SeededRandom.Value(randomSeed++)).Take(1).ToList();

            int totalDifficulty = 0;
            List<BattleModManager.BattleModDefinition> retval = new();
            List<BattleModManager.BattleModDefinition> remaining = new(mods.OrderBy(d => SeededRandom.Value(randomSeed++)));

            // The first two mods are purely randomly selected
            while (retval.Count < 2 && totalDifficulty < difficulty)
            {
                BattleModManager.BattleModDefinition next = remaining.FirstOrDefault(d => d.Difficulty <= (difficulty - totalDifficulty));

                if (next == null)
                    break;

                totalDifficulty += next.Difficulty;
                retval.Add(next);
                remaining.Remove(next);
            }

            // At this point, add two more difficulty to the total.
            // We've added two modifiers already. That's a lot. A third is a LOT
            totalDifficulty += 2;

            // If we still have to make up room, add the last mod that gets us
            // closest to the total we want.
            if (totalDifficulty < difficulty && remaining.Count > 0)
                retval.Add(remaining.OrderBy(d => Mathf.Abs(d.Difficulty - (difficulty - totalDifficulty))).First());

            return retval;
        }

        private static void BuildBattleModifiers(List<HoloMapBlueprint> rooms, Zone region, int order, int seed)
        {
            // The goal is to create modded battles with a total difficulty matching:
            // Which map this is (1-4) and number of difficulty challenges
            // So if you have one "extra difficulty" on, and this is map 2, the total difficulty is 3
            int difficultyKey = order + 1 + AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty);

            List<BattleModManager.BattleModDefinition> validMods = BattleModManager.AllBattleMods.Where(d => d.Regions == null || d.Regions.Contains(region.ToTemple())).ToList();
            int randomSeed = seed + 10;

            // Only one battle gets modded per map by default
            int numberOfModdedBattles = 1;

            // But starting with difficulty 4, we add another battle.
            // So the table looks like this:
            // Difficulty 1-4: 1 battle
            // Difficulty 5-6: 2 battles
            // Note that EACH of these have the difficulty.
            // So with both difficulty challenges on, in map four ALL FOUR battles
            // have a +6 battle mod on them.
            if (difficultyKey >= 5)
                numberOfModdedBattles = 2;
            List<HoloMapBlueprint> targets = rooms.Where(bp => bp.IsBattleRoom && (order > 0 || bp.color != 1)).OrderBy(x => SeededRandom.Value(randomSeed++)).Take(numberOfModdedBattles).ToList();

            // And now we move the difficulty key down by 1 if it's above 3
            // Basically, at the difficult 4 step, the difficulty increases by the count
            // of modded battles more so than the difficulty of the battles themselves
            if (difficultyKey > 3)
                difficultyKey -= 1;

            foreach (HoloMapBlueprint bp in targets)
                bp.battleMods.AddRange(SelectMods(validMods, difficultyKey, randomSeed++).Select(d => d.ID));
        }

        private static List<HoloMapBlueprint> BuildBlueprint(int order, Zone region, int seed, int stackDepth = 0)
        {
            string blueprintKey = $"ascensionBlueprint{order}{region}";
            string savedBlueprint = P03AscensionSaveData.RunStateData.GetValue(P03Plugin.PluginGuid, blueprintKey);

            if (savedBlueprint != default)
                return savedBlueprint.Split('|').Select(s => new HoloMapBlueprint(s)).ToList();

            if (region == Zone.Neutral)
                return BuildHubBlueprint(seed);

            if (region == Zone.Mycologist)
                return BuildMycologistBlueprint(seed);

            if (P03Plugin.Instance.TurboMode)
                return BuildTurboBlueprint(seed, region);

            UnityEngine.Random.InitState(seed);

            // Start with a 6x6 grid
            HoloMapBlueprint[,] bpBlueprint = new HoloMapBlueprint[6, 6];
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                    bpBlueprint[i, j] = new HoloMapBlueprint(seed + (10 * i) + (100 * j)) { x = i, y = j, arrowDirections = BLANK };
            }

            // Reshape for region
            ShapeMapForRegion(bpBlueprint, region);

            // Crawl and mark each quadrant.
            for (int i = 1; i <= 4; i++)
                CrawlQuadrant(bpBlueprint, i);

            // Set up the connections between quadrants
            ConnectQuadrants(bpBlueprint, region);

            // Revalidate all x/y pairs
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (bpBlueprint[i, j] != null)
                    {
                        bpBlueprint[i, j].x = i;
                        bpBlueprint[i, j].y = j;
                    }
                }
            }

            // Figure out the starting space
            var startingPosition = GetStartingSpace(region);
            HoloMapBlueprint startSpace = bpBlueprint[startingPosition.Item1, startingPosition.Item2];
            List<HoloMapBlueprint> retval = new() { startSpace };
            for (int i = 0; i < bpBlueprint.GetLength(0); i++)
            {
                for (int j = 0; j < bpBlueprint.GetLength(1); j++)
                {
                    if (bpBlueprint[i, j] != null && bpBlueprint[i, j] != startSpace)
                        retval.Add(bpBlueprint[i, j]);
                }
            }

            // Make sure that every room sees at least one other room 
            FixDisconnectedRooms(bpBlueprint, retval);

            startSpace.upgrade = TradeChipsNodeData.TradeChipsForCards;

            // Tech region has special landmarks (just the elevator)
            // Its boss room was created during map creation
            // And it has no bridges
            if (region != Zone.Tech)
            {
                if (region != Zone.Undead)
                    DiscoverAndCreateLandmarks(bpBlueprint, retval);

                DiscoverAndCreateBossRoom(bpBlueprint, retval, region);
                DiscoverAndCreateBridge(bpBlueprint, retval, region);
            }

            // Add four enemy encounters and rewards
            int seedForChoice = (seed * 2) + 10;

            List<int> colorsWithoutEnemies = new() { 1, 2, 3, 4 };
            List<int> usedIndices = new();
            int numberOfEncountersAdded = 0;
            while (colorsWithoutEnemies.Count > 0)
            {
                UnityEngine.Random.InitState(seedForChoice + (colorsWithoutEnemies.Count * 1000));
                int colorToUse = colorsWithoutEnemies[UnityEngine.Random.Range(0, colorsWithoutEnemies.Count)];
                HoloMapNode.NodeDataType type = colorsWithoutEnemies.Count <= 2 ? HoloMapNode.NodeDataType.AddCardAbility : REGION_DATA[region].DefaultReward;
                if (DiscoverAndCreateEnemyEncounter(bpBlueprint, retval, order, region, type, usedIndices, colorToUse))
                    numberOfEncountersAdded += 1;
                colorsWithoutEnemies.Remove(colorToUse);
            }

            int remainingEncountersToAdd = EventManagement.ENEMIES_TO_UNLOCK_BOSS - numberOfEncountersAdded;
            for (int i = 0; i < remainingEncountersToAdd; i++)
            {
                if (DiscoverAndCreateEnemyEncounter(bpBlueprint, retval, order, region, REGION_DATA[region].DefaultReward, usedIndices))
                    numberOfEncountersAdded += 1;
            }

            P03Plugin.Log.LogDebug($"I have created {numberOfEncountersAdded} enemy encounters");

            // Add one trade node
            bool traded = DiscoverAndCreateTrade(bpBlueprint, retval);
            P03Plugin.Log.LogDebug($"Created a trade node? {traded}");

            if (!traded)
            {
                traded = ForceTrade(bpBlueprint, retval);
                P03Plugin.Log.LogDebug($"Forcing a trade. Successful? {traded}");
            }

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BATTLE_MODIFIERS))
            {
                BuildBattleModifiers(retval, region, order, seed);
                P03Plugin.Log.LogDebug($"I have set up battle modifiers");
            }

            // Add four card choice nodes
            P03Plugin.Log.LogDebug($"Adding upgrades");
            int cardChoiceNodes = 0;
            for (int i = 1; i < 5; i++) // one for each color 1-4
            {
                HoloMapBlueprint node = retval.GetRandomPointOfInterest(bp => bp.color == i);
                if (node != null)
                {
                    node.upgrade = HoloMapNode.NodeDataType.CardChoice;
                    cardChoiceNodes += 1;
                }
            }
            for (int i = cardChoiceNodes; i < 4; i++) // Just in case we couldn't find a valid point of interest in every quadrant
                retval.GetRandomPointOfInterest().upgrade = HoloMapNode.NodeDataType.CardChoice;

            // And now we're just going to add one more regional upgrade
            retval.GetRandomPointOfInterest().upgrade = REGION_DATA[region].DefaultReward;

            for (int i = 0; i < 3; i++)
                retval.GetRandomPointOfInterest().upgrade = UnlockAscensionItemNodeData.UnlockItemsAscension;

            retval.GetRandomPointOfInterest().upgrade = AscensionRecycleCardNodeData.AscensionRecycleCard;

            // Add two hidden currency nodes
            P03Plugin.Log.LogDebug($"Adding currency nodes");
            for (int i = 0; i < 2; i++)
            {
                HoloMapBlueprint tbp2 = retval.GetRandomPointOfInterest();
                if (tbp2 != null)
                    tbp2.upgrade = HoloMapNode.NodeDataType.GainCurrency;
            }

            // Add one of each of the default upgrades for each completed zone
            foreach (Zone cRegion in CompletedRegions)
            {
                HoloMapBlueprint tbp2 = retval.GetRandomPointOfInterest();
                if (tbp2 != null)
                    tbp2.upgrade = REGION_DATA[cRegion].DefaultReward;
            }

            // If there are cards of every cost in the pool, add two cost change nodes
            if (EventManagement.AllFourResourcesInPool)
            {
                P03Plugin.Log.LogDebug($"Adding 2 cost swap nodes");
                for (int i = 0; i < 2; i++)
                {
                    HoloMapBlueprint tbp2 = retval.GetRandomPointOfInterest();
                    if (tbp2 != null)
                        tbp2.upgrade = SwapCardCostNodeData.SwapCardCost;
                }
            }

            // Add story events data
            if (CompletedRegions.Count < 3)
                DiscoverAndCreateSecretRoom(bpBlueprint, retval, seed * 2);

            BuildStoryEvents(retval, region);

            // Add the pelts
            if (DefaultQuestDefinitions.TrapperPelts.QuestGenerated && region == Zone.Nature)
                DiscoverAndCreateHoloTraps(bpBlueprint, retval);

            LogBlueprint(bpBlueprint);

            if (!IsBlueprintValid(retval))
            {
                if (stackDepth == 500)
                    throw new InvalidOperationException("Could not generate a valid map after 500 attempts - something has gone horribly wrong!");
                retval = BuildBlueprint(order, region, seed + 25, stackDepth + 1);
            }

            savedBlueprint = string.Join("|", retval.Select(b => b.ToString()));
            P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, blueprintKey, savedBlueprint);
            if (order >= 1)
                EventManagement.SawMapInfo = true;
            SaveManager.SaveToFile();
            return retval;
        }

        public static void TravelMap(HoloMapBlueprint current, List<HoloMapBlueprint> map)
        {
            for (int idx = 0; idx < 4; idx++)
            {
                int dir = idx == 0 ? NORTH : idx == 1 ? SOUTH : idx == 2 ? EAST : WEST;
                int xDelta = NSEW[idx][0];
                int yDelta = NSEW[idx][1];
                if ((current.arrowDirections & dir) != 0)
                {
                    HoloMapBlueprint node = map.FirstOrDefault(b => b.x == current.x + xDelta && b.y == current.y + yDelta);
                    if (node != null)
                    {
                        map.Remove(node);
                        TravelMap(node, map);
                    }
                }
            }
        }

        public static bool IsBlueprintValid(List<HoloMapBlueprint> blueprint)
        {
            // Make sure we can travel the entire map
            List<HoloMapBlueprint> bpCopy = new(blueprint);
            TravelMap(bpCopy[0], bpCopy);

            if (bpCopy.Count > 0)
            {
                P03Plugin.Log.LogDebug($"Map failed validation - could not visit entire map from start. {bpCopy.Count} nodes remaining");
                return false;
            }

            // Make sure there is a boss node
            if (!blueprint.Any(bp => bp.opponent != Opponent.Type.Default))
            {
                P03Plugin.Log.LogDebug("Map failed validation - no boss");
                return false;
            }

            // Make sure there are four enemy nodes
            if (blueprint.Where(bp => bp.specialDirection != 0 && bp.specialDirectionType == HoloMapBlueprint.BATTLE).Count() < 4)
            {
                P03Plugin.Log.LogDebug("Map failed validation - not enough enemy encounters");
                return false;
            }

            return true;
        }
    }
}