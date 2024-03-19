using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.BattleMods;
using Infiniscryption.P03KayceeRun.Quests;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class HoloMapBlueprint
    {
        public const int NO_SPECIAL = 0;
        public const int LEFT_BRIDGE = 1 << 0;
        public const int RIGHT_BRIDGE = 1 << 1;
        public const int FULL_BRIDGE = 1 << 2;
        public const int NORTH_BUILDING_ENTRANCE = 1 << 3;
        public const int NORTH_GATEWAY = 1 << 4;
        public const int NORTH_CABIN = 1 << 5;
        public const int LOWER_TOWER_ROOM = 1 << 6;
        public const int LANDMARKER = 1 << 7;
        public const int BROKEN_GENERATOR = 1 << 8;
        public const int MYCOLOGIST_WELL = 1 << 9;
        public const int FAST_TRAVEL_NODE = 1 << 10;
        public const int FINAL_SHOP_NODE = 1 << 11;
        public const int TEST_OF_STREGTH = 1 << 12;

        public const int BATTLE = 0;
        public const int TRADE = 1;
        public const int NEUTRAL_BATTLE = 2;

        public int randomSeed;
        public int x;
        public int y;
        public int arrowDirections;
        public int specialDirection;
        public int secretDirection;
        public int specialDirectionType;
        public Opponent.Type opponent;
        public HoloMapNode.NodeDataType upgrade;
        public int specialTerrain;
        public int blockedDirections;
        public StoryEvent blockEvent;
        public int battleTerrainIndex;
        public int encounterDifficulty;
        public bool isSecretRoom;
        public List<BattleModManager.ID> battleMods = new();

        public SpecialEvent dialogueEvent;

        public int encounterIndex;

        public int distance; // used only for generation - doesn't get saved or parsed
        public int color;

        private string GetBattleModString() => String.Join("#", battleMods.Select(i => i.ToString()));

        public override string ToString() => $"[{randomSeed},{x},{y},{arrowDirections},{specialDirection},{specialDirectionType},{encounterDifficulty},{(int)opponent},{(int)upgrade},{specialTerrain},{blockedDirections},{(int)blockEvent},{battleTerrainIndex},{color},{(int)dialogueEvent},{secretDirection},{isSecretRoom},{encounterIndex},{GetBattleModString()}]";

        public HoloMapBlueprint(int randomSeed) { this.randomSeed = randomSeed; encounterIndex = -1; upgrade = HoloMapNode.NodeDataType.MoveArea; }

        public HoloMapBlueprint(string parsed)
        {
            string[] split = parsed.Replace("[", "").Replace("]", "").Split(',');
            randomSeed = int.Parse(split[0]);
            x = int.Parse(split[1]);
            y = int.Parse(split[2]);
            arrowDirections = int.Parse(split[3]);
            specialDirection = int.Parse(split[4]);
            specialDirectionType = int.Parse(split[5]);
            encounterDifficulty = int.Parse(split[6]);
            opponent = (Opponent.Type)int.Parse(split[7]);
            upgrade = (HoloMapNode.NodeDataType)int.Parse(split[8]);
            specialTerrain = int.Parse(split[9]);
            blockedDirections = int.Parse(split[10]);
            blockEvent = (StoryEvent)int.Parse(split[11]);
            battleTerrainIndex = int.Parse(split[12]);
            color = int.Parse(split[13]);

            // From this point forward, extensions to the blueprint HAVE TO CHECK AND HAVE DEFAULTS
            // because we have to be backwards compatible
            dialogueEvent = (SpecialEvent)(split.Length > 14 ? int.Parse(split[14]) : 0);
            secretDirection = split.Length > 15 ? int.Parse(split[15]) : 0;
            isSecretRoom = split.Length > 16 && bool.Parse(split[16]);
            encounterIndex = split.Length > 17 ? int.Parse(split[17]) : -1;

            battleMods = new();
            if (split.Length > 18)
            {
                string[] allMods = split[18].Split('#');
                foreach (string p in allMods)
                {
                    if (int.TryParse(p, out int id))
                        battleMods.Add((BattleModManager.ID)id);
                }
            }
        }

        public bool EligibleForUpgrade
        {
            get => opponent == Opponent.Type.Default &&
                       upgrade == HoloMapNode.NodeDataType.MoveArea &&
                       (specialTerrain & LANDMARKER) == 0 &&
                       (specialTerrain & BROKEN_GENERATOR) == 0 &&
                       (specialTerrain & MYCOLOGIST_WELL) == 0 &&
                       (specialTerrain & LOWER_TOWER_ROOM) == 0 &&
                       (specialTerrain & FAST_TRAVEL_NODE) == 0;
        }

        public bool EligibleForDialogue => dialogueEvent == SpecialEvent.None;

        public bool IsBattleRoom => (specialDirectionType is BATTLE or NEUTRAL_BATTLE) && specialDirection != 0;

        public bool IsDeadEnd
        {
            get => arrowDirections == RunBasedHoloMap.NORTH ||
                       arrowDirections == RunBasedHoloMap.SOUTH ||
                       arrowDirections == RunBasedHoloMap.WEST ||
                       arrowDirections == RunBasedHoloMap.EAST;
        }

        public int NumberOfArrows
        {
            get => (((arrowDirections & RunBasedHoloMap.NORTH) != 0) ? 1 : 0) +
                       (((arrowDirections & RunBasedHoloMap.SOUTH) != 0) ? 1 : 0) +
                       (((arrowDirections & RunBasedHoloMap.EAST) != 0) ? 1 : 0) +
                       (((arrowDirections & RunBasedHoloMap.WEST) != 0) ? 1 : 0);
        }

        public List<string> DebugString
        {
            get
            {
                List<string> retval = new();
                string code = ((specialTerrain & LANDMARKER) != 0) ? "L" : opponent != Opponent.Type.Default ? "B" : specialDirection != RunBasedHoloMap.BLANK ? "E" : upgrade != HoloMapNode.NodeDataType.MoveArea ? "U" : " ";
                retval.Add("#---#");
                retval.Add((arrowDirections & RunBasedHoloMap.NORTH) != 0 ? $"|{color}| |" : $"|{color}  |");
                retval.Add("|" + ((arrowDirections & RunBasedHoloMap.WEST) != 0 ? $"-{code}" : $" {code}") + ((arrowDirections & RunBasedHoloMap.EAST) != 0 ? "-|" : " |"));
                retval.Add((arrowDirections & RunBasedHoloMap.SOUTH) != 0 ? "| | |" : "|   |");
                retval.Add("#---#");
                return retval;
            }
        }

        public string KeyCode => $"{x},{y}";
    }
}