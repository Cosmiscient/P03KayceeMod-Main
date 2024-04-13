using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Regions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class RegionGeneratorData
    {
        // internal static Dictionary<RunBasedHoloMap.Zone, List<string>> EncountersForZone = new() {
        //     { RunBasedHoloMap.Zone.Nature, new () },
        //     { RunBasedHoloMap.Zone.Neutral, new () },
        //     { RunBasedHoloMap.Zone.Tech, new () },
        //     { RunBasedHoloMap.Zone.Undead, new () },
        //     { RunBasedHoloMap.Zone.Magic, new () },
        //     { RunBasedHoloMap.Zone.Mycologist, new () }
        // };

        private static RegionData BuildDummyRegion(string name)
        {
            return RegionManager.New(name, -1, true).AddBosses(Opponent.Type.Default);
        }

        internal static Dictionary<RunBasedHoloMap.Zone, RegionData> DummyRegionsForZone = new() {
            { RunBasedHoloMap.Zone.Nature, BuildDummyRegion("P03KCM_Nature") },
            { RunBasedHoloMap.Zone.Neutral, BuildDummyRegion("P03KCM_Neutral") },
            { RunBasedHoloMap.Zone.Tech, BuildDummyRegion("P03KCM_Tech") },
            { RunBasedHoloMap.Zone.Undead, BuildDummyRegion("P03KCM_Undead") },
            { RunBasedHoloMap.Zone.Magic, BuildDummyRegion("P03KCM_Magic") },
            { RunBasedHoloMap.Zone.Mycologist, BuildDummyRegion("P03KCM_Mycologist") }
        };

        public RunBasedHoloMap.Zone RegionCode { get; internal set; }

        public string RegionName { get; internal set; }

        public RegionData Region => RegionManager.AllRegionsCopy.RegionByName(RegionName);

        private string[][] TerrainRandoms { get; set; }

        private string[][] ObjectRandoms { get; set; }

        private string[] Wall { get; set; }

        private string[] Effects { get; set; }

        private Vector2?[] CenterOffset { get; set; }

        public string GetTerrain(int color, int seed = -1)
        {
            if (TerrainRandoms == null)
                return null;

            if (TerrainRandoms.Length == 1)
                color = 0;
            else if (color > 0)
                color = color - 1;

            if (TerrainRandoms[color] == null || TerrainRandoms[color].Length == 0)
                return null;

            if (seed >= 0)
                return TerrainRandoms[color][SeededRandom.Range(0, TerrainRandoms[color].Length, seed)];
            else
                return TerrainRandoms[color][UnityEngine.Random.RandomRangeInt(0, TerrainRandoms[color].Length)];
        }

        public string GetObject(int color, int seed = -1)
        {
            if (ObjectRandoms == null)
                return null;

            if (ObjectRandoms.Length == 1)
                color = 0;
            else if (color > 0)
                color = color - 1;

            if (ObjectRandoms[color] == null || ObjectRandoms[color].Length == 0)
                return null;

            if (seed >= 0)
                return ObjectRandoms[color][SeededRandom.Range(0, ObjectRandoms[color].Length, seed)];
            else
                return ObjectRandoms[color][UnityEngine.Random.RandomRangeInt(0, ObjectRandoms[color].Length)];
        }

        public Vector2? GetCenterOffset(int color)
        {
            if (CenterOffset == null || CenterOffset.Length == 0)
                return null;

            if (CenterOffset.Length == 1 || color == 0)
                return CenterOffset[0];

            return CenterOffset[color - 1];
        }

        public string GetWall(int color)
        {
            if (Wall == null || Wall.Length == 0)
                return null;

            if (Wall.Length == 1 || color == 0)
                return Wall[0];

            return Wall[color - 1];
        }

        public string GetEffects(int color)
        {
            if (Effects == null || Effects.Length == 0)
                return null;

            if (Effects.Length == 1 || color == 0)
                return Effects[0];

            return Effects[color - 1];
        }

        private List<Dictionary<int, string[]>> WallPrefabs { get; set; }
        private List<Dictionary<int, string[]>> CornerPrefabs { get; set; }

        internal Dictionary<int, string[]> GetWallPrefabs(int color)
        {
            if (WallPrefabs == null)
                return null;

            if (WallPrefabs.Count == 1 || color == 0)
                return WallPrefabs[0];

            return WallPrefabs[color - 1];
        }

        internal Dictionary<int, string[]> GetCornerPrefabs(int color)
        {
            if (CornerPrefabs == null)
                return null;

            if (CornerPrefabs.Count == 1 || color == 0)
                return CornerPrefabs[0];

            return CornerPrefabs[color - 1];
        }

        public Dictionary<int, Tuple<Vector3, Vector3>> WallOrientations { get; internal set; }

        public Color LightColor { get; internal set; }

        public Color MainColor { get; internal set; }

        public HoloMapNode.NodeDataType DefaultReward { get; internal set; }

        public string[][] Terrain { get; internal set; }

        public string[][] Landmarks { get; internal set; }

        public HoloMapArea.AudioLoopsConfig AudioConfig { get; internal set; }

        public GameObject ScreenPrefab { get; internal set; }

        public RegionGeneratorData(RunBasedHoloMap.Zone regionCode)
        {
            this.RegionCode = regionCode;
            RegionName = DummyRegionsForZone[regionCode].name;

            switch (regionCode)
            {
                case RunBasedHoloMap.Zone.Neutral:
                    TerrainRandoms = new string[][] { new string[] { "NeutralEastMain_4/Scenery/HoloGrass_Small (1)", "NeutralEastMain_4/Scenery/HoloGrass_Patch (1)" } };
                    ObjectRandoms = new string[][] { new string[] { "StartingIslandBattery/Scenery/HoloMeter", "StartingIslandBattery/Scenery/HoloDrone_Broken", "StartingIslandJunction/Scenery/HoloBotPiece_Leg", "StartingIslandJunction/Scenery/HoloBotPiece_Head", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloPowerPoll_1" } };
                    Wall = new string[] { "NeutralEastMain_4/Scenery/HoloWall (1)" };
                    LightColor = new Color(0.5802f, 0.8996f, 1f);
                    MainColor = new Color(0f, 0.5157f, 0.6792f);
                    DefaultReward = HoloMapNode.NodeDataType.AddCardAbility;
                    WallOrientations = new() {
                        { RunBasedHoloMap.NORTH, new(new(.08f, -.18f, 2.02f), new(7.4407f, 179.305f, .0297f)) },
                        { RunBasedHoloMap.SOUTH, new(new(.08f, -.18f, -2.02f), new(7.4407f, 359.2266f, .0297f)) },
                        { RunBasedHoloMap.WEST, new(new(-3.2f, -.18f, -.4f), new(7.4407f, 89.603f, .0297f)) },
                        { RunBasedHoloMap.EAST, new(new(3.2f, -.18f, -.4f), new(7.4407f, 270.359f, .0297f)) }
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Default;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_DefaultGround");
                    break;
                case RunBasedHoloMap.Zone.Magic:
                    TerrainRandoms = new string[][] { new string[] { "WizardEntrance/Scenery/HoloDebris (1)" } };
                    ObjectRandoms = new string[][] { new string[] { "WizardSidePath_1/Scenery/HoloRock_Floating", "WizardMainPath_2/Scenery/HoloRock_Floating (2)", "WizardSidePath_1/Scenery/HoloRock_Floating", "WizardMainPath_2/Scenery/HoloRock_Floating (2)", "WizardEntrance/Scenery/HoloGemGreen", "WizardEntrance/Scenery/HoloGemOrange", "WizardEntrance/Scenery/HoloGemBlue" } };
                    DefaultReward = HoloMapNode.NodeDataType.AttachGem;
                    LightColor = new Color(0.5802f, 0.8996f, 1f);
                    MainColor = new Color(0.1802f, 0.2778f, 0.5094f);
                    Terrain = new string[][] {
                        new string[] { null, "P03KCM_MoxObelisk", null, null, null, null, null, null, "P03KCM_MoxObelisk", null }
                    };
                    Landmarks = new string[][] {
                        new string[] { "WizardMainPath_3/Scenery/HoloGenerator" },
                        new string[] { "WizardMainPath_3/Scenery/HoloSlime_Pile_1", "WizardMainPath_3/Scenery/HoloSlime_Pile_2", "WizardMainPath_3/Scenery/HoloSlimePipe", "WizardMainPath_3/Scenery/HoloSlime_Pile_1 (1)" },
                        new string[] { "WizardSidePath_3/Scenery/HoloSword", "WizardSidePath_3/Scenery/HoloSword (1)", "WizardSidePath_3/Scenery/HoloSword (2)", "WizardSidePath_1/Scenery/HoloSword", "WizardSidePath_1/Scenery/HoloSword (1)", },
                        new string[] { "TempleWizardEntrance/Scenery/Gem", "TempleWizardEntrance/Scenery/Gem (2)", "TempleWizardEntrance/Scenery/Gem (3)", "TempleWizardMain1/Scenery/Gem", "TempleWizardMain1/Scenery/Gem (2)", "TempleWizardMain1/Scenery/Gem (3)", "TempleWizardMain2/Scenery/Gem (2)", "WizardMainPath_6/Scenery/HoloGemBlue/Gem"  },
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Wizard;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_DefaultGround");
                    break;
                case RunBasedHoloMap.Zone.Nature:
                    TerrainRandoms = new string[][] { new string[] { "NatureMainPath_2/Scenery/HoloGrass_Foliage", "NatureMainPath_2/Scenery/HoloDebris" } };
                    ObjectRandoms = new string[][]
                    {
                        new string[] { "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloCage_1", "NatureMainPath_8/Scenery/HoloRock_3", "NatureMainPath_8/Scenery/HoloTreeDead" },
                        new string[] { "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloCage_1" },
                        new string[] { "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloTree_2", "NatureMainPath_8/Scenery/HoloCage_1", "NatureMainPath_8/Scenery/HoloRock_3", "NatureMainPath_8/Scenery/HoloTreeDead" },
                        new string[] { "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloCage_1" }
                    };
                    Wall = new string[] {
                        "NatureMainPath_8/Scenery/HoloTree_2",
                        "NatureMainPath_2/Scenery/HoloTree_3" ,
                        "NatureMainPath_8/Scenery/HoloTree_2",
                        "NatureMainPath_2/Scenery/HoloTree_3"
                    };
                    Effects = new string[] {
                        "prefabs/map/holomapscenery/HoloMapSnowParticles",
                        null,
                        "prefabs/map/holomapscenery/HoloMapSnowParticles",
                        null
                    };

                    // Hey you, do you want to change the transformer back to behave like it used to? And go back to normal
                    // beast mode instead of what we have now? Well all you've gotta do is change the default reward for this
                    // region back to CreateTransformer
                    DefaultReward = HoloMapNode.NodeDataType.CreateTransformer;
                    //this.defaultReward = AscensionTransformerCardNodeData.AscensionTransformCard;
                    LightColor = new Color(.4078f, .698f, .4549f);
                    MainColor = new Color(.4431f, .448f, .1922f);
                    WallOrientations = new() {
                        { RunBasedHoloMap.NORTH, new(new(.08f, -.18f, 2.02f), new(270f, 179.305f, .0297f)) },
                        { RunBasedHoloMap.SOUTH, new(new(.08f, -.18f, -2.02f), new(270f, 359.2266f, .0297f)) },
                        { RunBasedHoloMap.WEST, new(new(-3.2f, -.18f, -.4f), new(270f, 89.603f, .0297f)) },
                        { RunBasedHoloMap.EAST, new(new(3.2f, -.18f, -.4f), new(270f, 270.359f, .0297f)) }
                    };
                    Terrain = new string[][] {
                        new string[] { null, "Tree_Hologram", null, "Tree_Hologram", null, null, "Tree_Hologram", null, "Tree_Hologram", null },
                        new string[] { "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered", "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered", null, "Tree_Hologram_SnowCovered" },
                        new string[] { null, null, null, null, null, null, null, null, null, null }
                    };
                    Landmarks = new string[][] {
                        new string[] { "NatureMainPath_2/Scenery/HoloGateway" },
                        new string[] { "NatureEntrance/Scenery/HoloRock_3", "NatureEntrance/Scenery/HoloGenerator", "NatureEntrance/Scenery/HoloLamp" },
                        new string[] { "NatureEntrance/Scenery/HoloGenerator/Generator_Cylinder", "NatureSidePath/Scenery/Generator_Cylinder (1)", "NatureSidePath/Scenery/Generator_Cylinder (2)", "NatureSidePath/Scenery/Generator_Cylinder (3)" },
                        new string[] { "NatureEntrance/Scenery/HoloLamp", "NatureMainPath_4/Scenery/HoloLamp", "NatureMainPath_10/Scenery/HoloLamp"}
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Nature;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_WetlandsGround");
                    break;
                case RunBasedHoloMap.Zone.Tech:
                    TerrainRandoms = new string[][] { new string[] { } };
                    ObjectRandoms = new string[][]
                    {
                        new string[] { "TechTower_NE/Scenery/HoloMeter", "TechTower_NE/Scenery/HoloDrone_2", "TechTower_NE/Scenery/HoloDrone_2 (1)", "TechTower_NE/Scenery/HoloDrone_2 (2)", "TechTower_NE/Scenery/HoloMeter", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)" },
                        new string[] { "TechTower_NE/Scenery/HoloMeter", "TechEdge_N/Scenery/HoloDrone_Broken", "TechEdge_N/Scenery/HoloDrone_Broken (1)", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)" },
                        new string[] { "TechTower_NE/Scenery/HoloMeter", "TechTower_NE/Scenery/HoloDrone_2", "TechTower_NE/Scenery/HoloDrone_2 (1)", "TechTower_NE/Scenery/HoloDrone_2 (2)", "TechTower_NE/Scenery/HoloMeter", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)" },
                        new string[] { "TechTower_NE/Scenery/HoloMeter", "TechEdge_N/Scenery/HoloDrone_Broken", "TechEdge_N/Scenery/HoloDrone_Broken (1)", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)", "TechEdge_N/Scenery/HoloDebris_1", "TechEdge_N/Scenery/HoloDebris_1 (1)" },
                    };
                    Terrain = new string[][] {
                        new string[] { null, null, null, null, null, null, null, null, null, null }
                    };
                    LightColor = new Color(0.6934f, 0.9233f, 1f);
                    MainColor = new Color(0.4413f, 0.5221f, 0.5472f);
                    DefaultReward = HoloMapNode.NodeDataType.BuildACard;
                    Landmarks = new string[][] {
                        new string[] { "NeutralWestTechGate/Scenery/Generator_Turbine", "NeutralWestTechGate/Scenery/Generator_Turbine (1)" },
                        new string[] { "NeutralWestMain_2/Scenery/HoloPowerPoll_1", "NeutralWestMain_2/Scenery/HoloPowerPoll_1 (1)"},
                        new string[] { "Center/Scenery/HoloTeslaCoil", "Center/Scenery/HoloMeter", "Center/Scenery/TerrainHologram_AnnoyTower"},
                        new string[] { "TechElevatorTop/Scenery/HoloTechPillar", "TechElevatorTop/Scenery/HoloTechPillar (1)", "TechElevatorTop/Scenery/HoloTechPillar (2)", "TechElevatorTop/Scenery/HoloTechPillar (3)"},
                    };
                    CenterOffset = new Vector2?[] { new(0f, -1.1f), null, new(0f, -1.1f), null };
                    WallPrefabs = new()
                    {
                        new () {
                            [RunBasedHoloMap.WEST] = (new string[] { "TempleTech_2/Scenery/HoloTechDoorWall/HoloTechTempleDoor|-3.5421,-0.19,2.505|358,270,0", "TempleTech_2/Scenery/HoloTechDoorWall/Wall|-3.5804,0.49,-2.4182|2,90,0" }),
                            [RunBasedHoloMap.NORTH] = (new string[] { "TempleTech_3/Scenery/Northwall", "TempleTech_1/Scenery/HoloConveyorBelt", "TempleTech_1/Scenery/HoloConveyorBelt (1)", "TempleTech_1/Scenery/HoloConveyorBelt (2)", "TempleTech_1/Scenery/HoloConveyorBelt (3)", "TempleTech_1/Scenery/HoloConveyorBelt (4)" }),
                            [RunBasedHoloMap.EAST] = (new string[] { "TempleTech_2/Scenery/HoloTechDoorWall/HoloTechTempleDoor|3.5421,-0.19,0.005|358,90,0", "TempleTech_2/Scenery/HoloTechDoorWall/Wall|3.5804,0.49,-0.018|358,270,0" }),
                            [RunBasedHoloMap.SOUTH] = (new string[] { "TempleTech_4/Scenery/HoloTechTempleDoor" }),
                        },
                        new () {
                            [RunBasedHoloMap.WEST] = (new string[] { "TechEdge_W/Scenery/Railing" }),
                            [RunBasedHoloMap.NORTH] = (new string[] { "TechTower_NW/Scenery/HoloHandRail", "TechTower_NW/Scenery/HoloMapTurret", "TechTower_NW/Scenery/HoloMapTurret (1)" }),
                            [RunBasedHoloMap.EAST] = (new string[] { "TechEdge_E/Scenery/Railing" }),
                            [RunBasedHoloMap.SOUTH] = (new string[] { "TechTower_SW/Scenery/HoloHandRail", "TechTower_SW/Scenery/HoloMapTurret", "TechTower_SW/Scenery/HoloMapTurret (1)" })
                        },
                        new () {
                            [RunBasedHoloMap.WEST] = (new string[] { "TempleTech_2/Scenery/HoloTechDoorWall/HoloTechTempleDoor|-3.5421,-0.19,2.505|358,270,0", "TempleTech_2/Scenery/HoloTechDoorWall/Wall|-3.5804,0.49,-2.4182|2,90,0" }),
                            [RunBasedHoloMap.NORTH] = (new string[] { "TempleTech_3/Scenery/Northwall", "TempleTech_1/Scenery/HoloConveyorBelt", "TempleTech_1/Scenery/HoloConveyorBelt (1)", "TempleTech_1/Scenery/HoloConveyorBelt (2)", "TempleTech_1/Scenery/HoloConveyorBelt (3)", "TempleTech_1/Scenery/HoloConveyorBelt (4)" }),
                            [RunBasedHoloMap.EAST] = (new string[] { "TempleTech_2/Scenery/HoloTechDoorWall/HoloTechTempleDoor|3.5421,-0.19,0.005|358,90,0", "TempleTech_2/Scenery/HoloTechDoorWall/Wall|3.5804,0.49,-0.018|358,270,0" }),
                            [RunBasedHoloMap.SOUTH] = (new string[] { "TempleTech_4/Scenery/HoloTechTempleDoor" }),
                        },
                        new () {
                            [RunBasedHoloMap.WEST] = (new string[] { "TechEdge_W/Scenery/Railing" }),
                            [RunBasedHoloMap.NORTH] = (new string[] { "TechTower_NW/Scenery/HoloHandRail", "TechTower_NW/Scenery/HoloMapTurret", "TechTower_NW/Scenery/HoloMapTurret (1)" }),
                            [RunBasedHoloMap.EAST] = (new string[] { "TechEdge_E/Scenery/Railing" }),
                            [RunBasedHoloMap.SOUTH] = (new string[] { "TechTower_SW/Scenery/HoloHandRail", "TechTower_SW/Scenery/HoloMapTurret", "TechTower_SW/Scenery/HoloMapTurret (1)" })
                        }
                    };
                    CornerPrefabs = new()
                    {
                        new () {
                            [RunBasedHoloMap.WEST] = (new string[] { "TempleTech_1/Scenery/tech_temple_holo_doubledoor"}),
                            [RunBasedHoloMap.NORTH] = (new string[] { "+TempleTech_2/Scenery/HoloTechDoorWall", "TempleTech_2/Scenery/HoloConveyorBelt", "TempleTech_2/Scenery/HoloConveyorBelt (2)", "TempleTech_2/Scenery/HoloConveyorBelt (4)", "TempleTech_2/Scenery/HoloConveyorBelt (6)|1.95,0.458,0.26", "TempleTech_2/Scenery/HoloConveyorBelt (6)", "-TempleTech_2/Scenery/HoloConveyorBelt (3)", "TempleTech_2/Scenery/HoloConveyorBelt (3)"  }),
                            [RunBasedHoloMap.EAST] = (new string[] { "TempleTech_1/Scenery/tech_temple_holo_doubledoor (1)" }),
                            [RunBasedHoloMap.SOUTH] = (new string[] { "+TempleTech_3/Scenery/Southwall_Door" })
                        },
                        null,
                        new () {
                            [RunBasedHoloMap.WEST] = (new string[] { "TempleTech_1/Scenery/tech_temple_holo_doubledoor"}),
                            [RunBasedHoloMap.NORTH] = (new string[] { "+TempleTech_2/Scenery/HoloTechDoorWall", "TempleTech_2/Scenery/HoloConveyorBelt", "TempleTech_2/Scenery/HoloConveyorBelt (2)", "TempleTech_2/Scenery/HoloConveyorBelt (4)", "TempleTech_2/Scenery/HoloConveyorBelt (6)|1.95,0.458,0.26", "TempleTech_2/Scenery/HoloConveyorBelt (6)", "-TempleTech_2/Scenery/HoloConveyorBelt (3)", "TempleTech_2/Scenery/HoloConveyorBelt (3)"  }),
                            [RunBasedHoloMap.EAST] = (new string[] { "TempleTech_1/Scenery/tech_temple_holo_doubledoor (1)" }),
                            [RunBasedHoloMap.SOUTH] = (new string[] { "+TempleTech_3/Scenery/Southwall_Door" })
                        },
                        null
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Tech;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_TechGround");
                    break;
                case RunBasedHoloMap.Zone.Undead:
                    TerrainRandoms = new string[][] {
                        new string[] { "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch" },
                        new string[] { "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch" },
                        new string[] { "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloDebris", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty },
                        new string[] { "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch" }
                    };
                    ObjectRandoms = new string[][] {
                        new string[] { "UndeadMainPath_4/Scenery/HoloGravestone", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "UndeadSecretPath_1/Scenery/HoloShovel", "UndeadMainPath_4/Scenery/HoloDirtPile_2", "UndeadMainPath_4/Scenery/HoloZombieArm", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "prefabs/map/holomapscenery/HoloMapFlyParticles" },
                        new string[] { "UndeadMainPath_4/Scenery/HoloGravestone", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "UndeadSecretPath_1/Scenery/HoloShovel", "UndeadMainPath_4/Scenery/HoloDirtPile_2", "UndeadMainPath_4/Scenery/HoloZombieArm", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "prefabs/map/holomapscenery/HoloMapFlyParticles" },
                        new string[] { "UndeadMainPath_4/Scenery/HoloZombieArm", "UndeadMainPath_4/Scenery/HoloZombieArm", "TempleUndeadEntrance/Scenery/HoloTeslaCoil", "TempleUndeadEntrance/Scenery/HoloTeslaCoil", "+TempleUndeadEntrance/Scenery/HoloCasket" },
                        new string[] { "UndeadMainPath_4/Scenery/HoloGravestone", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "UndeadSecretPath_1/Scenery/HoloShovel", "UndeadMainPath_4/Scenery/HoloDirtPile_2", "UndeadMainPath_4/Scenery/HoloZombieArm", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "prefabs/map/holomapscenery/HoloMapFlyParticles" },
                    };
                    DefaultReward = HoloMapNode.NodeDataType.OverclockCard;
                    LightColor = new Color(.1702f, .8019f, .644f);
                    MainColor = new Color(.0588f, .3608f, .3647f);
                    Wall = null;
                    WallPrefabs = new List<Dictionary<int, string[]>>()
                    {
                        null,
                        null,
                        new Dictionary<int, string[]>()
                        {
                            [RunBasedHoloMap.NORTH] = new string[] { "TempleUndeadLeft_2/Scenery/UndeadTempleWall" } ,
                            [RunBasedHoloMap.SOUTH] = new string[] { "TempleUndeadRight_1/Scenery/UndeadTempleWall (1)" } ,
                            [RunBasedHoloMap.EAST] = new string[] { "TempleUndeadMain_1/Scenery/UndeadTempleWall (1)" } ,
                            [RunBasedHoloMap.WEST] = new string[] { "TempleUndeadMain_1/Scenery/UndeadTempleWall" } ,
                        },
                        null,
                    };
                    CornerPrefabs = new List<Dictionary<int, string[]>>()
                    {
                        null,
                        null,
                        new Dictionary<int, string[]>()
                        {
                            [RunBasedHoloMap.NORTH] = new string[] { "TempleUndeadRight_1/Scenery/HoloUndeadDoorWall" } ,
                            [RunBasedHoloMap.SOUTH] = new string[] { "TempleUndeadLeft_2/Scenery/HoloUndeadDoorWall (1)" } ,
                            [RunBasedHoloMap.EAST] = new string[] { "-TempleUndeadEntrance/Scenery/HoloUndeadDoorWall (2)" } ,
                            [RunBasedHoloMap.WEST] = new string[] { "-TempleUndeadLeft_1/Scenery/HoloUndeadDoorWall (3)" } ,
                        },
                        null,
                    };
                    Terrain = new string[][] {
                        new string[] { null, "DeadTree", null, null, null, null, null, null, null, "DeadTree" },
                        new string[] { null, null, null, null, null, null, "TombStone", null, "TombStone", null },
                        new string[] { null, "P03KCM_Ghoulware", null, null, null, null, null, null, "P03KCM_Ghoulware", null },
                    };
                    Landmarks = new string[][] {
                        new string[] { "-TempleUndeadLeft_1/Scenery/HoloUndeadDoorWall (3)" },//"UndeadSmallDetour_2/Scenery/HoloTeslaCoil", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (1)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (2)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (3)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (4)", "UndeadSmallDetour_2/Scenery/HoloMapLightning", "UndeadSmallDetour_2/Scenery/HoloMapLightning (2)", "UndeadSmallDetour_2/Scenery/HoloMapLightning (3)"},
                        new string[] { "TempleUndeadEntrance/Scenery/HoloCasket", "TempleUndeadEntrance/Scenery/HoloCasket (1)", "TempleUndeadEntrance/Scenery/HoloCasket (2)", "TempleUndeadEntrance/Scenery/HoloCasket (3)"},
                        new string[] { "TempleUndeadMain_1/Scenery/HoloLibrarianBot (1)", "TempleUndeadMain_1/Scenery/HoloLibrarianBot (2)"},
                        new string[] { "TempleUndeadRight_1/Scenery/HoloUndeadDoorWall" }//"UndeadSmallDetour_2/Scenery/HoloGravestone", "UndeadSmallDetour_2/Scenery/HoloGravestone (1)", "UndeadSmallDetour_2/Scenery/HoloGravestone (2)", "UndeadSmallDetour_2/Scenery/HoloGravestone (4)"}
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Undead;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_WetlandsGround");
                    break;
                case RunBasedHoloMap.Zone.Mycologist:
                    TerrainRandoms = new string[][] { new string[] { "NeutralEastMain_4/Scenery/HoloGrass_Small (1)", "Mycologists_1/Scenery/HoloMushroom_1", "Mycologists_1/Scenery/HoloMushroom_2" } };
                    ObjectRandoms = new string[][] { new string[] { "NatureMainPath_2/Scenery/HoloTree_2", "UndeadMainPath_4/Scenery/HoloTreeDead (2)" } };
                    Wall = null;
                    LightColor = new Color(0.9057f, 0.3802f, 0.6032f);
                    MainColor = new Color(0.3279f, 0f, 0.434f);
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Undead;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_WetlandsGround");
                    break;
                default:
                    break;
            }
        }
    }
}