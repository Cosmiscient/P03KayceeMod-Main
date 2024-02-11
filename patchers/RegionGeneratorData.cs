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

        public string[] TerrainRandoms { get; internal set; }

        public string[] ObjectRandoms { get; internal set; }

        public string Wall { get; internal set; }

        public Dictionary<int, Tuple<Vector3, Vector3>> WallOrientations { get; internal set; }

        public Color LightColor { get; internal set; }

        public Color MainColor { get; internal set; }

        public HoloMapNode.NodeDataType DefaultReward { get; internal set; }

        public string[][] Terrain { get; internal set; }

        public string[][] Landmarks { get; internal set; }

        public HoloMapArea.AudioLoopsConfig AudioConfig { get; internal set; }

        public GameObject ScreenPrefab { get; internal set; }

        public Dictionary<int, string[]> WallPrefabs { get; internal set; }

        public RegionGeneratorData(RunBasedHoloMap.Zone regionCode)
        {
            this.RegionCode = regionCode;
            RegionName = DummyRegionsForZone[regionCode].name;

            switch (regionCode)
            {
                case RunBasedHoloMap.Zone.Neutral:
                    TerrainRandoms = new string[] { "NeutralEastMain_4/Scenery/HoloGrass_Small (1)", "NeutralEastMain_4/Scenery/HoloGrass_Patch (1)" };
                    ObjectRandoms = new string[] { "StartingIslandBattery/Scenery/HoloMeter", "StartingIslandBattery/Scenery/HoloDrone_Broken", "StartingIslandJunction/Scenery/HoloBotPiece_Leg", "StartingIslandJunction/Scenery/HoloBotPiece_Head", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloDrone_1", "NeutralEastMain_4/Scenery/HoloPowerPoll_1" };
                    Wall = "NeutralEastMain_4/Scenery/HoloWall (1)";
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
                    TerrainRandoms = new string[] { "WizardEntrance/Scenery/HoloDebris (1)" };
                    ObjectRandoms = new string[] { "WizardSidePath_1/Scenery/HoloRock_Floating", "WizardMainPath_2/Scenery/HoloRock_Floating (2)", "WizardSidePath_1/Scenery/HoloRock_Floating", "WizardMainPath_2/Scenery/HoloRock_Floating (2)", "WizardEntrance/Scenery/HoloGemGreen", "WizardEntrance/Scenery/HoloGemOrange", "WizardEntrance/Scenery/HoloGemBlue" };
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
                    TerrainRandoms = new string[] { "NatureMainPath_2/Scenery/HoloGrass_Foliage", "NatureMainPath_2/Scenery/HoloDebris" };
                    ObjectRandoms = new string[] { "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloTree_2", "NatureMainPath_2/Scenery/HoloCage_1" };
                    Wall = "NatureMainPath_2/Scenery/HoloTree_3";

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
                    TerrainRandoms = new string[] { };
                    ObjectRandoms = new string[] { };
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
                    WallPrefabs = new()
                    {
                        [RunBasedHoloMap.WEST] = (new string[] { "TechEdge_W/Scenery/Railing" }),
                        [RunBasedHoloMap.NORTH] = (new string[] { "TechTower_NW/Scenery/HoloHandRail", "TechTower_NW/Scenery/HoloMapTurret", "TechTower_NW/Scenery/HoloMapTurret (1)" }),
                        [RunBasedHoloMap.EAST] = (new string[] { "TechEdge_E/Scenery/Railing" }),
                        [RunBasedHoloMap.SOUTH] = (new string[] { "TechTower_SW/Scenery/HoloHandRail", "TechTower_SW/Scenery/HoloMapTurret", "TechTower_SW/Scenery/HoloMapTurret (1)" })
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Tech;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_TechGround");
                    break;
                case RunBasedHoloMap.Zone.Undead:
                    TerrainRandoms = new string[] { "UndeadMainPath_4/Scenery/HoloDebris", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch", "UndeadMainPath_4/Scenery/HoloGrass_Patch" };
                    ObjectRandoms = new string[] { "UndeadMainPath_4/Scenery/HoloGravestone", "UndeadMainPath_4/Scenery/HoloTreeDead (2)", "UndeadSecretPath_1/Scenery/HoloShovel", "UndeadMainPath_4/Scenery/HoloDirtPile_2", "UndeadMainPath_4/Scenery/HoloZombieArm", "UndeadMainPath_4/Scenery/HoloTreeDead (2)" };
                    DefaultReward = HoloMapNode.NodeDataType.OverclockCard;
                    LightColor = new Color(.1702f, .8019f, .644f);
                    MainColor = new Color(.0588f, .3608f, .3647f);
                    Wall = null;
                    Terrain = new string[][] {
                        new string[] { null, "DeadTree", null, null, null, null, null, null, null, "DeadTree" },
                        new string[] { null, null, null, null, null, null, "TombStone", null, "TombStone", null },
                        new string[] { null, "P03KCM_Ghoulware", null, null, null, null, null, null, "P03KCM_Ghoulware", null },
                    };
                    Landmarks = new string[][] {
                        new string[] { "UndeadSmallDetour_2/Scenery/HoloTeslaCoil", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (1)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (2)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (3)", "UndeadSmallDetour_2/Scenery/HoloTeslaCoil (4)", "UndeadSmallDetour_2/Scenery/HoloMapLightning", "UndeadSmallDetour_2/Scenery/HoloMapLightning (2)", "UndeadSmallDetour_2/Scenery/HoloMapLightning (3)"},
                        new string[] { "TempleUndeadEntrance/Scenery/HoloCasket", "TempleUndeadEntrance/Scenery/HoloCasket (1)", "TempleUndeadEntrance/Scenery/HoloCasket (2)", "TempleUndeadEntrance/Scenery/HoloCasket (3)"},
                        new string[] { "TempleUndeadMain_1/Scenery/HoloLibrarianBot (1)", "TempleUndeadMain_1/Scenery/HoloLibrarianBot (2)"},
                        new string[] { "UndeadSmallDetour_2/Scenery/HoloGravestone", "UndeadSmallDetour_2/Scenery/HoloGravestone (1)", "UndeadSmallDetour_2/Scenery/HoloGravestone (2)", "UndeadSmallDetour_2/Scenery/HoloGravestone (4)"}
                    };
                    AudioConfig = HoloMapArea.AudioLoopsConfig.Undead;
                    ScreenPrefab = Resources.Load<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_WetlandsGround");
                    break;
                case RunBasedHoloMap.Zone.Mycologist:
                    TerrainRandoms = new string[] { "NeutralEastMain_4/Scenery/HoloGrass_Small (1)", "Mycologists_1/Scenery/HoloMushroom_1", "Mycologists_1/Scenery/HoloMushroom_2" };
                    ObjectRandoms = new string[] { "NatureMainPath_2/Scenery/HoloTree_2", "UndeadMainPath_4/Scenery/HoloTreeDead (2)" };
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