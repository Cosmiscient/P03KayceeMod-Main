using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.BattleMods;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Quests;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Encounters;
using UnityEngine;
using UnityEngine.Analytics;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static partial class RunBasedHoloMap
    {
        internal static bool Building { get; private set; } = false;
        internal static Zone BuildingZone { get; private set; } = Zone.Neutral;

        // The holographic map is absolutely bonkers
        // Each screen that you see on the map is called an 'area'
        // Think of it like a region on the paper game map.
        // Each area has nodes on it. Those nodes do things.
        // What's crazy is that the arrows on the edge of the map that you think of as just UI elements,
        // those are actually map nodes. The arrow itself contains all the data of the encounter and behavior.
        // The encounter data, etc, is not stored in the map area - it's stored on the arrow of the adjacent map area.

        private static readonly Dictionary<string, HoloMapWorldData> worldDataCache = new();

        private static GameObject defaultPrefab;
        private static GameObject neutralHoloPrefab;

        private static readonly Dictionary<Opponent.Type, GameObject> BossPrefabs = new();
        public static Dictionary<HoloMapNode.NodeDataType, GameObject> SpecialNodePrefabs = new();
        internal static Dictionary<int, GameObject[]> SpecialTerrainPrefabs = new();
        internal static Dictionary<int, GameObject> ArrowPrefabs = new();

        internal static readonly Part3SaveData.WorldPosition MYCOLOGIST_HOME_POSITION = new("ascension_0_Mycologist", 0, 2);

        public enum Zone
        {
            Neutral = 0,
            Tech = 1,
            Undead = 2,
            Nature = 3,
            Magic = 4,
            Mycologist = 5
        }

        // public const int NEUTRAL = 0;
        // public const int TECH = 1;
        // public const int UNDEAD = 2;
        // public const int NATURE = 3;
        // public const int MAGIC = 4;

        private static readonly Dictionary<Zone, RegionGeneratorData> REGION_DATA = new();

        private static readonly Dictionary<string, GameObject> objectLookups = new();

        private static GameObject HOLO_NODE_BASE;
        private static GameObject HOVER_HOLO_NODE_BASE;
        private static GameObject BLOCK_ICON;

        public static readonly int EMPTY = -1;
        public static readonly int BLANK = 0;
        public static readonly int NORTH = 1;
        public static readonly int EAST = 2;
        public static readonly int SOUTH = 4;
        public static readonly int WEST = 8;
        public static readonly int ENEMY = 16;
        public static readonly int COUNTDOWN = 32;
        public static readonly int ALL_DIRECTIONS = NORTH | EAST | SOUTH | WEST;
        private static readonly Dictionary<int, string> DIR_LOOKUP = new() { { SOUTH, "S" }, { WEST, "W" }, { NORTH, "N" }, { EAST, "E" } };
        private static readonly Dictionary<int, LookDirection> LOOK_MAPPER = new() { { SOUTH, LookDirection.South }, { NORTH, LookDirection.North }, { EAST, LookDirection.East }, { WEST, LookDirection.West } };
        private static readonly Dictionary<string, LookDirection> LOOK_NAME_MAPPER = new()
        {
            {"MoveArea_S", LookDirection.South}, {"MoveArea_N", LookDirection.North}, {"MoveArea_E", LookDirection.East}, {"MoveArea_W", LookDirection.West},
            {"MoveArea_W (NORTH)", LookDirection.North}, {"MoveArea_W (SOUTH)", LookDirection.South}, {"MoveArea_E (NORTH)", LookDirection.North}, {"MoveArea_E (SOUTH)", LookDirection.South}
        };

        private static IEnumerable<int> GetDirections(int compound, bool inclusive = true, bool shuffle = false)
        {
            if (shuffle)
            {
                List<int> unshuffled = GetDirections(compound, inclusive, false).ToList();
                int start = UnityEngine.Random.Range(1, unshuffled.Count - 1);
                foreach (int i in unshuffled.Skip(start))
                    yield return i;
                foreach (int i in unshuffled.Take(start))
                    yield return i;
                yield break;
            }
            if (inclusive)
            {
                if ((compound & NORTH) != 0) yield return NORTH;
                if ((compound & EAST) != 0) yield return EAST;
                if ((compound & SOUTH) != 0) yield return SOUTH;
                if ((compound & WEST) != 0) yield return WEST;
                yield break;
            }

            if ((compound & EAST) == 0) yield return EAST;
            if ((compound & WEST) == 0) yield return WEST;
            if ((compound & NORTH) == 0) yield return NORTH;
            if ((compound & SOUTH) == 0) yield return SOUTH;
            yield return NORTH | EAST;
            yield return SOUTH | EAST;
            yield return NORTH | WEST;
            yield return SOUTH | WEST;
        }

        internal static string GetAscensionWorldbyId(Func<string, Zone> getRegionCodeFromWorldID) => throw new NotImplementedException();

        internal static GameObject GetGameObject(string singleMapKey)
        {
            if (singleMapKey == default)
                return null;

            string fixKey = singleMapKey.TrimStart('-', '+').Split('|')[0];

            GameObject retval = ResourceBank.Get<GameObject>(fixKey);
            if (retval != null)
                return retval;

            string holoMapKey = fixKey.Split('/')[0];
            string findPath = fixKey.Replace($"{holoMapKey}/", "");
            return GetGameObject(holoMapKey, findPath);
        }

        internal static GameObject[] GetGameObject(string[] multiMapKey) => multiMapKey.Select(s => GetGameObject(s)).ToArray();

        internal static GameObject GetGameObject(string holomap, string findPath)
        {
            string key = $"{holomap}/{findPath}";
            if (objectLookups.ContainsKey(key))
            {
                GameObject dictval = objectLookups[key];
                if (dictval == null)
                    objectLookups.Remove(key);
                else
                    return objectLookups[key];
            }

            P03Plugin.Log.LogInfo($"Getting {holomap} / {findPath} ");
            GameObject resource = Resources.Load<GameObject>($"prefabs/map/holomapareas/HoloMapArea_{holomap}");
            GameObject retval = resource.transform.Find(findPath).gameObject;

            objectLookups.Add(key, retval);

            return retval;
        }

        public static void AddReplace<K, V>(this Dictionary<K, V> dict, K key, Func<V> getValue)
        {
            // I want to verify that these game objects are still alive
            // If they're not, I want to recreate them
            // But I don't want to create them unless I need to
            // So this helper takes a Func that creates them to delay building them until it's necessary

            if (dict.ContainsKey(key))
            {
                V oldValue = dict[key];
                if (oldValue != null)
                {
                    P03Plugin.Log.LogInfo($"I already have a {key}");
                    return;
                }

                dict.Remove(key);
            }

            P03Plugin.Log.LogInfo($"I need to create a {key}");
            dict.Add(key, getValue());
        }

        private static void Initialize()
        {
            P03Plugin.Log.LogInfo("Initializing world data");

            REGION_DATA.Clear(); // All of the actual region data is in the region data class itself
            for (int i = 0; i < 6; i++)
                REGION_DATA.Add((Zone)i, new((Zone)i));

            HOLO_NODE_BASE ??= GetGameObject("StartingIslandJunction", "Scenery/HoloNodeBase");
            HOVER_HOLO_NODE_BASE ??= GetGameObject("Shop", "Scenery/HoloDrone_HoldingPlatform_Undead");
            BLOCK_ICON ??= GetGameObject("UndeadShortcut_Exit", "HoloStopIcon");

            defaultPrefab = Resources.Load<GameObject>("prefabs/map/holomapareas/holomaparea");
            P03Plugin.Log.LogInfo($"Default prefab is {defaultPrefab}");

            // Boss prefabs
            BossPrefabs.AddReplace(Opponent.Type.ArchivistBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleUndeadBoss"));
            BossPrefabs.AddReplace(Opponent.Type.PhotographerBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleNatureBoss"));
            BossPrefabs.AddReplace(Opponent.Type.TelegrapherBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleTech_1"));
            BossPrefabs.AddReplace(Opponent.Type.CanvasBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardBoss"));
            BossPrefabs.AddReplace(Opponent.Type.MycologistsBoss, () => Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_Mycologists_2"));

            // Special node prefabs
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.CardChoice, () => GetGameObject("StartingIslandJunction", "Nodes/CardChoiceNode3D"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.AddCardAbility, () => GetGameObject("Shop", "Nodes/ShopNode3D_AddAbility"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.OverclockCard, () => GetGameObject("Shop", "Nodes/ShopNode3D_Overclock"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.CreateTransformer, () => GetGameObject("Shop", "Nodes/ShopNode3D_Transformer"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.AttachGem, () => GetGameObject("Shop", "Nodes/ShopNode3D_AttachGem"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.RecycleCard, () => GetGameObject("NeutralWestMain_1", "Nodes/RecycleCardNode3D"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.BuildACard, () => GetGameObject("Shop", "Nodes/ShopNode3D_BuildACard"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.GainCurrency, () => GetGameObject("NatureMainPath_3", "Nodes/CurrencyGainNode3D"));
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.ModifySideDeckConduit, () => GetGameObject("TechEntrance", "Nodes/ModifySideDeckNode3D"));
            SpecialNodePrefabs.AddReplace(TradeChipsNodeData.TradeChipsForCards, () => GetDraftNode());
            SpecialNodePrefabs.AddReplace(UnlockAscensionItemNodeData.UnlockItemsAscension, () => GetItemNode());
            SpecialNodePrefabs.AddReplace(AscensionRecycleCardNodeData.AscensionRecycleCard, () => GetRecycleNode());
            SpecialNodePrefabs.AddReplace(HoloMapNode.NodeDataType.BossBattle, () => GetGameObject("TempleWizardBoss", "Nodes/BossNode3D"));

            // Special terrain prefabs
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.RIGHT_BRIDGE, () => new GameObject[] { GetGameObject("UndeadMainPath_4", "Scenery/HoloBridge_Entrance") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.LEFT_BRIDGE, () => new GameObject[] { GetGameObject("UndeadMainPath_3", "Scenery/HoloBridge_Entrance") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.FULL_BRIDGE, () => new GameObject[] { GetGameObject("NeutralEastMain_2", "Scenery") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.NORTH_BUILDING_ENTRANCE, () => GetGameObject(new string[] { "UndeadMainPath_4/Scenery/SM_Bld_Wall_Exterior_04", "UndeadMainPath_4/Scenery/SM_Bld_Wall_Exterior_04 (1)", "UndeadMainPath_4/Scenery/SM_Bld_Wall_Doorframe_02" }));
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.NORTH_GATEWAY, () => new GameObject[] { GetGameObject("NatureMainPath_2", "Scenery/HoloGateway") });
            SpecialTerrainPrefabs.AddReplace(HoloMapBlueprint.NORTH_CABIN, () => new GameObject[] { GetGameObject("TempleNature_4", "Scenery/Cabin") });

            // Let's instantiate the battle arrow prefabs
            ArrowPrefabs = new();
            ArrowPrefabs.AddReplace(EAST | ENEMY, () => GetGameObject("neutraleastmain_3", "Nodes/MoveArea_E"));
            ArrowPrefabs.AddReplace(SOUTH | ENEMY, () => GetGameObject("UndeadEntrance", "Nodes/MoveArea_S"));
            ArrowPrefabs.AddReplace(NORTH | ENEMY, () => GetGameObject("naturemainpath_2", "Nodes/MoveArea_N"));
            ArrowPrefabs.AddReplace(WEST | ENEMY, () => GetGameObject("neutralwestmain_2", "Nodes/MoveArea_W"));

            ArrowPrefabs.AddReplace(WEST | COUNTDOWN, () => GetGameObject("natureentrance", "Nodes/MoveArea_W"));
            ArrowPrefabs.AddReplace(SOUTH | COUNTDOWN, () => GetGameObject("wizardmainpath_3", "Nodes/MoveArea_S"));

            // This generates 'pseudo-prefab' objects
            // We will have one for each zone
            // Each random node will randomly turn scenery nodes on and off
            // And will set the arrows appropriately.
            neutralHoloPrefab = UnityEngine.Object.Instantiate(defaultPrefab);
            neutralHoloPrefab.SetActive(false);
        }

        private static List<Zone> CompletedRegions
        {
            get => EventManagement.CompletedZones.Select(s =>
                                    s.EndsWith("Undead") ? Zone.Undead :
                                    s.EndsWith("Wizard") ? Zone.Magic :
                                    s.EndsWith("Tech") ? Zone.Tech :
                                    s.EndsWith("Nature") ? Zone.Nature :
                                    Zone.Neutral).ToList();
        }

        private static float DistanceFromCenter(this Tuple<float, float> p) => Mathf.Sqrt(Mathf.Pow(p.Item1, 2) + Mathf.Pow(p.Item2, 2));

        private static int DistanceComparer(Tuple<float, float> a, Tuple<float, float> b)
        {
            float da = a.DistanceFromCenter();
            float db = b.DistanceFromCenter();
            return da == db ? 0 : da > db ? 1 : -1;
        }

        private static readonly float[] MULTIPLIERS = new float[] { 0.33f, 0.66f };
        private static List<Tuple<float, float>> GetSpotsForQuadrant(int quadrant)
        {
            float minX = ((quadrant & WEST) != 0) ? -3.2f : ((quadrant & EAST) != 0) ? 1.1f : -1.1f;
            float maxX = ((quadrant & WEST) != 0) ? -1.1f : ((quadrant & EAST) != 0) ? 3.2f : 1.1f;
            float minZ = ((quadrant & NORTH) != 0) ? 1.1f : ((quadrant & SOUTH) != 0) ? -2.02f : -1.1f;
            float maxZ = ((quadrant & NORTH) != 0) ? 2.02f : ((quadrant & SOUTH) != 0) ? -1.1f : 1.1f;

            List<Tuple<float, float>> retval = new();
            foreach (float m in MULTIPLIERS)
            {
                foreach (float n in MULTIPLIERS)
                    retval.Add(new(minX + (m * (maxX - minX)) - .025f + (.05f * UnityEngine.Random.value), minZ + (n * (maxZ - minZ)) - .025f + (.05f * UnityEngine.Random.value)));
            }

            retval.Sort(DistanceComparer);

            return retval;
        }

        private static GameObject GetItemNode()
        {
            GameObject baseObject = GetGameObject("Shop", "Nodes/ShopNode3D_ShieldGenItem");
            GameObject retval = UnityEngine.Object.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponentInChildren<HoloMapSpecialNode>();
            nodeData.nodeType = UnlockAscensionItemNodeData.UnlockItemsAscension;
            nodeData.repeatable = false;

            retval.SetActive(false);

            return retval;
        }

        private static GameObject GetRecycleNode()
        {
            GameObject baseObject = GetGameObject("TechTower_NW", "Nodes/ShopNode3D_Recycle");
            GameObject retval = UnityEngine.Object.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponentInChildren<HoloMapSpecialNode>();
            nodeData.nodeType = AscensionRecycleCardNodeData.AscensionRecycleCard;
            nodeData.repeatable = false;

            retval.SetActive(false);

            return retval;
        }

        private static GameObject GetDraftNode()
        {
            GameObject baseObject = GetGameObject("WizardMainPath_3", "Nodes/CardChoiceNode3D");
            GameObject retval = UnityEngine.Object.Instantiate(baseObject);

            // Turn this into a trade node
            HoloMapSpecialNode nodeData = retval.GetComponent<HoloMapSpecialNode>();
            nodeData.nodeType = TradeChipsNodeData.TradeChipsForCards;
            nodeData.repeatable = true;

            retval.transform.Find("RendererParent/Renderer_2").gameObject.SetActive(false);
            retval.transform.localEulerAngles = new(0f, 0f, 0f);

            GameObject card = retval.transform.Find("RendererParent/Renderer").gameObject;
            card.transform.localPosition = new(-0.1f, 0.05f, -0.3f);
            card.transform.localScale = new(1f, 1f, .8f);
            card.transform.localEulerAngles = new(271f, 191f, 9f);

            GameObject second = UnityEngine.Object.Instantiate(card, card.transform.parent);
            second.transform.localPosition = new(0.13f, -0.1f, -0.3f);
            nodeData.nodeRenderers.Add(second.GetComponent<Renderer>());

            GameObject third = UnityEngine.Object.Instantiate(card, card.transform.parent);
            third.transform.localPosition = new(0f, -0.01f, -0.3f);
            nodeData.nodeRenderers.Add(third.GetComponent<Renderer>());

            retval.SetActive(false);

            P03Plugin.Log.LogInfo($"Build draft node {retval}");
            return retval;
        }

        private static GameObject GetGeneratorCountdownNode(Transform parent, List<GameObject> generators, List<GameObject> rubble)
        {
            GameObject refObject = ArrowPrefabs[WEST | ENEMY];
            GameObject damageRageIcon = refObject.transform.Find("RendererParent/DamageRaceIcon").gameObject;

            GameObject baseObject = GetGameObject("WizardMainPath_3", "Nodes/CardChoiceNode3D");
            GameObject retval = UnityEngine.Object.Instantiate(baseObject);
            retval.transform.SetParent(parent);

            // Turn this into a trade node
            HoloMapSpecialNode oldNodeData = retval.GetComponent<HoloMapSpecialNode>();
            UnityEngine.Object.Destroy(oldNodeData);

            GeneratorOverloadNode nodeData = retval.AddComponent<GeneratorOverloadNode>();
            nodeData.nodeType = HoloMapNode.NodeDataType.CardBattle;
            nodeData.specialEncounterId = MoveHoloMapAreaNode.DAMAGE_RACE_SEQUENCER_NAME;
            nodeData.Data = null;
            nodeData.blueprintData = EncounterHelper.GeneratorDamageRace;
            nodeData.opponentTerrain = new CardInfo[] {
                CardLoader.GetCardByName(CustomCards.GENERATOR_TOWER),
                CardLoader.GetCardByName(CustomCards.FIREWALL_NORMAL),
                null,
                CardLoader.GetCardByName(CustomCards.GENERATOR_TOWER),
                CardLoader.GetCardByName(CustomCards.FIREWALL_NORMAL)
            };
            nodeData.encounterDifficulty = 10;
            nodeData.playerTerrain = new CardInfo[5];

            UnityEngine.Object.Destroy(retval.transform.Find("RendererParent/Renderer").gameObject);
            UnityEngine.Object.Destroy(retval.transform.Find("RendererParent/Renderer_2").gameObject);

            Transform iconParent = retval.transform.Find("RendererParent");
            GameObject instIcon = UnityEngine.Object.Instantiate(damageRageIcon, iconParent);
            instIcon.transform.localPosition = Vector3.zero;
            instIcon.SetActive(true);

            // Add an 'active only if' flag
            ActiveIfStoryFlag flag = retval.AddComponent<ActiveIfStoryFlag>();
            flag.storyFlag = DefaultQuestDefinitions.BrokenGenerator.InitialState.StateCompleteEvent;
            flag.activeIfConditionMet = false;

            nodeData.nodeRenderers = new() {
                instIcon.GetComponentInChildren<Renderer>()
            };

            // generator rubble
            nodeData.LivingGeneratorPieces.AddRange(generators);
            nodeData.DeadGeneratorPieces.AddRange(rubble);

            P03Plugin.Log.LogInfo($"Build draft node {retval}");
            return retval;
        }

        private static void BuildDialogueNode(HoloMapBlueprint blueprint, Transform parent, Transform sceneryParent, float x, float z)
        {
            // NPC
            //GameObject npcObject = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/holomapnpc"), parent);
            NPCDescriptor descriptor = NPCDescriptor.GetDescriptorForNPC(blueprint.dialogueEvent);
            QuestDefinition quest = QuestManager.Get(blueprint.dialogueEvent);

            // Special case for spear wizard - NO LONGER
            // if (descriptor.P03Face == P03AnimationController.Face.SpearWizard)
            // {
            //     GameObject npcObject = UnityEngine.Object.Instantiate(GetGameObject("WizardMainPath_5/Scenery/HoloMapNPC"));
            //     npcObject.transform.SetParent(sceneryParent);
            //     npcObject.transform.localPosition = new Vector3(x, 0.1f, z);

            //     dialogue.npc = npcObject;
            // }
            // else
            // {

            GameObject npcBase = ResourceBank.Get<GameObject>("prefabs/map/holoplayermarker");
            GameObject npcObject = UnityEngine.Object.Instantiate(npcBase.transform.Find("Anim/Model").gameObject);
            npcObject.transform.SetParent(sceneryParent);
            npcObject.transform.localPosition = new Vector3(x, 0.1f, z);
            quest.PostGenerateAction?.Invoke(npcObject);

            // Dialogue node 
            GameObject nodeObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("prefabs/map/mapnodespart3/DialogueNode3D"), parent);
            nodeObject.transform.SetParent(parent);
            nodeObject.transform.localPosition = new Vector3(npcObject.transform.localPosition.x + .2f, 1.1f, npcObject.transform.localPosition.z + .2f);

            HoloMapDialogueNode node = nodeObject.GetComponentInChildren<HoloMapDialogueNode>();

            Color defaultColor = new(node.defaultColor.r, node.defaultColor.g, node.defaultColor.b, node.defaultColor.a);
            HoloMapNode.NodeDataType dataType = node.nodeType;
            int nodeId = node.nodeId;
            List<Renderer> renderers = new(node.nodeRenderers);

            UnityEngine.Object.DestroyImmediate(node);

            // Copy the stuff from the node we're cloning
            HoloMapConditionalDialogueNode dialogue = nodeObject.AddComponent<HoloMapConditionalDialogueNode>();
            dialogue.nodeRenderers = renderers;
            dialogue.defaultColor = defaultColor;
            dialogue.nodeType = dataType;
            dialogue.nodeId = nodeId;

            // And now tell the note what dialogue event it needs
            dialogue.eventId = blueprint.dialogueEvent;

            CompositeFigurine figure = npcObject.GetComponentInChildren<CompositeFigurine>();
            figure.generateAsPlayer = false;
            figure.Generate(descriptor.head, descriptor.arms, descriptor.body);

            // figure.armsRenderer.SetMaterial(Resources.Load<Material>("art/materials/hologram/Hologram_MapSceneryBlue"));
            // figure.bodyRenderer.SetMaterial(Resources.Load<Material>("art/materials/hologram/Hologram_MapSceneryBlue"));
            // figure.headRenderer.SetMaterial(Resources.Load<Material>("art/materials/hologram/Hologram_MapSceneryBlue"));

            dialogue.npc = npcObject;

            float yRotate = (x < 0)
                            ? (z < 0 ? 45f : 135f)
                            : (z < 0 ? -45f : -135f);

            npcObject.transform.localEulerAngles = new(0f, yRotate, 0f);

            // Label
            GameObject sampleObject = SpecialNodePrefabs[HoloMapNode.NodeDataType.BuildACard].transform.Find("HoloFloatingLabel").gameObject;
            GameObject labelObject = UnityEngine.Object.Instantiate(sampleObject, nodeObject.transform);
            labelObject.transform.localPosition = new(-1f, -0.5f, 0f);
            HoloFloatingLabel label = labelObject.GetComponent<HoloFloatingLabel>();
            label.line.gameObject.SetActive(false);
            label.line = null;
            dialogue.label = label;

        }

        private static void BuildSpecialNode(HoloMapBlueprint blueprint, Zone regionId, Transform parent, Transform sceneryParent, float x, float z) => BuildSpecialNode(blueprint.upgrade, blueprint.specialTerrain, regionId, parent, sceneryParent, x, z);

        private static HoloMapNode BuildSpecialNode(HoloMapNode.NodeDataType dataType, int specialTerrain, Zone regionId, Transform parent, Transform sceneryParent, float x, float z)
        {
            if (!SpecialNodePrefabs.ContainsKey(dataType))
                return null;

            P03Plugin.Log.LogInfo($"Adding {dataType} at {x},{z}");

            GameObject defaultNode = SpecialNodePrefabs[dataType];

            P03Plugin.Log.LogInfo($"node is{defaultNode}");
            GameObject newNode = UnityEngine.Object.Instantiate(defaultNode, parent);
            newNode.SetActive(true);

            HoloMapShopNode shopNode = newNode.GetComponent<HoloMapShopNode>();
            if (shopNode != null)
            {
                // This is a shop node but we want it to behave differently than the in-game shop nodes

                shopNode.cost = EventManagement.UpgradePrice(dataType);
                shopNode.repeatable = false;
                shopNode.increasingCost = false;
            }

            if (dataType == HoloMapNode.NodeDataType.GainCurrency)
            {
                newNode.transform.localPosition = new Vector3(x, newNode.transform.localPosition.y, z);
                HoloMapGainCurrencyNode nodeData = newNode.GetComponent<HoloMapGainCurrencyNode>();
                nodeData.amount = UnityEngine.Random.Range(EventManagement.CurrencyGainRange.Item1, EventManagement.CurrencyGainRange.Item2);
            }
            else
            {
                if (sceneryParent != null)
                {
                    float yVal = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? newNode.transform.localPosition.y : .5f;
                    newNode.transform.localPosition = new Vector3(x, yVal, z);

                    yVal = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? .1f : 1.33f;

                    GameObject nodeBasePrefab = ((specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0) ? HOLO_NODE_BASE : HOVER_HOLO_NODE_BASE;
                    P03Plugin.Log.LogInfo($"nodebase is{nodeBasePrefab}");
                    GameObject nodeBase = UnityEngine.Object.Instantiate(nodeBasePrefab, sceneryParent);
                    nodeBase.transform.localPosition = new Vector3(newNode.transform.localPosition.x, yVal, newNode.transform.localPosition.z);
                }
            }

            return newNode.GetComponent<HoloMapNode>();
        }

        private static GameObject BuildP03BossNode()
        {
            GameObject hubNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandTablets");
            GameObject retval = UnityEngine.Object.Instantiate(hubNodeBase);

            Part3FinaleAreaSequencer sequencer = retval.GetComponent<Part3FinaleAreaSequencer>();
            UnityEngine.Object.Destroy(sequencer);

            //AscensionFinaleSequencer newSequencer = retval.AddComponent<AscensionFinaleSequencer>();
            //newSequencer.enabled = true;

            HoloMapArea area = retval.GetComponent<HoloMapArea>();
            area.firstEnterDialogueId = P03AscensionSaveData.LeshyIsDead ? "P03AscensionMultiverseReturn" : "P03AscensionPreIntro";

            P03Plugin.Log.LogInfo("Building boss node");
            HoloMapBossNode bossNode = BuildSpecialNode(HoloMapNode.NodeDataType.BossBattle, HoloMapBlueprint.NO_SPECIAL, Zone.Neutral, retval.transform.Find("Nodes"), null, 0f, 0f) as HoloMapBossNode;
            P03Plugin.Log.LogInfo($"Making boss invisible: {bossNode}");
            foreach (Renderer rend in bossNode.gameObject.GetComponentsInChildren<Renderer>())
                rend.enabled = false; // Hide the boss node visually - I don't want to see it

            var boss = P03AscensionSaveData.LeshyIsDead ? BossManagement.P03MultiverseOpponent : BossManagement.P03FinalBossOpponent;

            bossNode.lootNodes = new();
            bossNode.bossAnim = null;
            bossNode.specialEncounterId = BossBattleSequencer.GetSequencerIdForBoss(boss);

            P03Plugin.Log.LogInfo($"Setting boss type. Boss node data is {bossNode.Data}");
            P03Plugin.Log.LogInfo($"Type of data is {bossNode.Data.GetType()}");
            CardBattleNodeData data = bossNode.Data as CardBattleNodeData;
            data.specialBattleId = BossBattleSequencer.GetSequencerIdForBoss(boss);

            area.bossNode = bossNode;
            area.activateBossOnEnter = true;

            retval.SetActive(false);
            return retval;
        }

        [HarmonyPatch(typeof(HoloMapAreaManager), nameof(HoloMapAreaManager.MoveToAreaDirectly))]
        [HarmonyPostfix]
        private static void WeirdBossSavingBehavior(HoloMapAreaManager __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.CurrentArea.activateBossOnEnter)
                {
                    __instance.StartCoroutine(__instance.CurrentArea.ReachBossNodeSequence());
                }
            }
        }

        private static GameObject GetArrow(string name, Transform parent, bool right)
        {
            var bac = SpecialNodeHandler.Instance.buildACardSequencer.screen.gameObject;
            string key = right ? "Right" : "Left";
            GameObject cardButton = GameObject.Instantiate(bac.transform.Find($"Anim/ScreenInteractables/ArrowButton_{key}").gameObject, parent);
            cardButton.name = name;
            cardButton.SetActive(false);
            cardButton.transform.localScale = new(2f, 2f * (right ? -1f : 1f), 2f);
            return cardButton;
        }

        private static GameObject BuildMirrorNode()
        {
            GameObject shopNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            GameObject retval = UnityEngine.Object.Instantiate(shopNodeBase);

            UnityEngine.Object.Destroy(retval.transform.Find("Nodes/CurrencyGainNode3D").gameObject);
            UnityEngine.Object.Destroy(retval.transform.Find("WaypointStation").gameObject);

            Transform nodeParent = retval.transform.Find("Nodes");
            Transform sceneryParent = retval.transform.Find("Scenery");

            // Destroy all previously existing scenery
            List<GameObject> objsToDelete = new();
            for (int i = 0; i < sceneryParent.childCount; i++)
                objsToDelete.Add(sceneryParent.GetChild(i).gameObject);
            foreach (var obj in objsToDelete)
                UnityEngine.Object.Destroy(obj);

            UnityEngine.Object.Destroy(retval.transform.Find("smoke").gameObject);

            // Spawn in some training dummies
            for (int i = 0; i < 5; i++)
            {
                GameObject trainingDummy = GameObject.Instantiate(AssetBundleManager.Prefabs["BotopiaDummy"], sceneryParent);
                MaterialHelper.HolofyAllRenderers(trainingDummy, GameColors.instance.blue);
                trainingDummy.transform.localPosition = new(
                    -2.25f + (float)i + 0.5f * UnityEngine.Random.value,
                    0f,
                    -1.75f + 0.5f * UnityEngine.Random.value
                );
                trainingDummy.transform.localEulerAngles = new(
                    0f,
                    360f * UnityEngine.Random.value,
                    0f
                );
            }


            // Spawn in and holofy the painting
            var painting = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/RulesPainting"), sceneryParent);
            MaterialHelper.HolofyAllRenderers(painting, GameColors.Instance.purple);
            painting.transform.localPosition = new(0f, 0.73f, 1.55f);
            painting.transform.localScale = new(0.9f, 0.5f, 0.4f);

            // Add an arrow to the left
            Transform leftArrow = nodeParent.Find("MoveArea_W");
            // MoveHoloMapAreaNode moveRight = leftArrow.gameObject.GetComponentInChildren<MoveHoloMapAreaNode>();
            // moveRight.secret = true;
            leftArrow.gameObject.SetActive(true);

            // Add an arrow up
            Transform upArrow = nodeParent.Find("MoveArea_N");
            upArrow.GetComponentInChildren<MoveHoloMapAreaNode>().direction = ElevatorArrowPatches.MIRROR_UP;
            // MoveHoloMapAreaNode moveRight = leftArrow.gameObject.GetComponentInChildren<MoveHoloMapAreaNode>();
            // moveRight.secret = true;
            upArrow.gameObject.SetActive(true);
            upArrow.transform.localPosition = new(0f, 0.25f, 1.1f);
            upArrow.GetComponentInChildren<MoveHoloMapAreaNode>().Area = null;

            // Add the six arrows
            var leftHead = GetArrow("Figurine_HeadLeft", nodeParent, false);
            var rightHead = GetArrow("Figurine_HeadRight", nodeParent, true);
            var leftBody = GetArrow("Figurine_BodyLeft", nodeParent, false);
            var rightBody = GetArrow("Figurine_BodyRight", nodeParent, true);
            var leftArms = GetArrow("Figurine_ArmsLeft", nodeParent, false);
            var rightArms = GetArrow("Figurine_ArmsRight", nodeParent, true);

            leftHead.transform.localPosition = new(-1.4766f, 1.2766f, 1.502f);
            leftArms.transform.localPosition = new(-1.4766f, 0.7766f, 1.502f);
            leftBody.transform.localPosition = new(-1.4766f, 0.2766f, 1.502f);

            rightHead.transform.localPosition = new(1.4766f, 1.2766f, 1.502f);
            rightArms.transform.localPosition = new(1.4766f, 0.7766f, 1.502f);
            rightBody.transform.localPosition = new(1.4766f, 0.2766f, 1.502f);

            retval.SetActive(false);

            return retval;
        }

        private static GameObject BuildTestOfStrengthNode()
        {
            GameObject shopNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            GameObject retval = UnityEngine.Object.Instantiate(shopNodeBase);

            UnityEngine.Object.Destroy(retval.transform.Find("Nodes/CurrencyGainNode3D").gameObject);
            UnityEngine.Object.Destroy(retval.transform.Find("WaypointStation").gameObject);

            Transform nodeParent = retval.transform.Find("Nodes");
            Transform sceneryParent = retval.transform.Find("Scenery");

            // Destroy all previously existing scenery
            List<GameObject> objsToDelete = new();
            for (int i = 0; i < sceneryParent.childCount; i++)
                objsToDelete.Add(sceneryParent.GetChild(i).gameObject);
            foreach (var obj in objsToDelete)
                UnityEngine.Object.Destroy(obj);

            UnityEngine.Object.Destroy(retval.transform.Find("smoke").gameObject);

            // Add a generator
            var generator = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/environment/DamageRaceGenerator"));
            generator.transform.SetParent(sceneryParent);
            generator.GetComponentInChildren<DamageRaceGenerator>().alarmAudioSource.volume = 0;
            GameObject.Destroy(generator.GetComponentInChildren<Animator>());
            generator.transform.Find("Anim").gameObject.SetActive(true);
            MaterialHelper.HolofyAllRenderers(generator.transform.Find("Anim/Main").gameObject, GameColors.Instance.limeGreen);
            generator.transform.localScale = new(0.4f, 0.4f, 0.4f);
            generator.transform.localPosition = new(2.6f, 0f, 0f);
            generator.transform.localEulerAngles = new(0f, 235f, 0f);

            // Add a series of lights 
            var lightArray = TestOfStrengthLightArray.Create(sceneryParent);
            lightArray.transform.localPosition = new(0f, 0f, 0f);

            // Add the actual battle
            GameObject refObject = ArrowPrefabs[WEST | ENEMY];
            GameObject damageRageIcon = refObject.transform.Find("RendererParent/DamageRaceIcon").gameObject;

            GameObject nodePrefab = GetGameObject("WizardMainPath_3", "Nodes/CardChoiceNode3D");
            GameObject nodeObj = UnityEngine.Object.Instantiate(nodePrefab);
            nodeObj.transform.SetParent(nodeParent);

            // Turn this into a trade node
            HoloMapSpecialNode oldNodeData = nodeObj.GetComponent<HoloMapSpecialNode>();
            UnityEngine.Object.Destroy(oldNodeData);

            HoloMapNode nodeData = nodeObj.AddComponent<HoloMapNode>();
            nodeData.nodeType = HoloMapNode.NodeDataType.CardBattle;
            nodeData.specialEncounterId = BossBattleSequencer.GetSequencerIdForBoss(BossManagement.TestOfStrengthOpponent);
            nodeData.blueprintData = EncounterHelper.GeneratorDamageRace;
            nodeData.opponentTerrain = new CardInfo[5];
            nodeData.playerTerrain = new CardInfo[5];

            UnityEngine.Object.Destroy(nodeObj.transform.Find("RendererParent/Renderer").gameObject);
            UnityEngine.Object.Destroy(nodeObj.transform.Find("RendererParent/Renderer_2").gameObject);

            Transform iconParent = nodeObj.transform.Find("RendererParent");
            GameObject instIcon = UnityEngine.Object.Instantiate(damageRageIcon, iconParent);
            instIcon.transform.localPosition = Vector3.zero;
            instIcon.SetActive(true);

            nodeData.nodeRenderers = new() {
                instIcon.GetComponentInChildren<Renderer>()
            };

            nodeObj.transform.localPosition = new(1.8f, 0f, 0f);
            nodeObj.transform.localEulerAngles = new(0f, 283f, 0f);

            HoloMapArea area = retval.GetComponent<HoloMapArea>();
            //area.firstEnterDialogueId = "P03FinalShopNode";

            // Add an arrow to the left
            Transform leftArrow = nodeParent.Find("MoveArea_W");
            // MoveHoloMapAreaNode moveRight = leftArrow.gameObject.GetComponentInChildren<MoveHoloMapAreaNode>();
            // moveRight.secret = true;
            leftArrow.gameObject.SetActive(true);

            Transform downArrow = nodeParent.Find("MoveArea_S");
            downArrow.GetComponentInChildren<MoveHoloMapAreaNode>().Area = null;
            downArrow.gameObject.SetActive(false);

            retval.SetActive(false);
            return retval;
        }

        private static GameObject BuildFinalShopNode()
        {
            GameObject shopNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            GameObject retval = UnityEngine.Object.Instantiate(shopNodeBase);

            UnityEngine.Object.Destroy(retval.transform.Find("Nodes/CurrencyGainNode3D").gameObject);
            UnityEngine.Object.Destroy(retval.transform.Find("WaypointStation").gameObject);

            Transform nodeParent = retval.transform.Find("Nodes");
            Transform sceneryParent = retval.transform.Find("Scenery");

            // Delete all non-bridge scenery
            List<Transform> deleteChildren = new();
            for (int i = 0; i < sceneryParent.childCount; i++)
                if (!sceneryParent.GetChild(i).gameObject.name.ToLowerInvariant().Contains("bridge"))
                    deleteChildren.Add(sceneryParent.GetChild(i));

            foreach (var t in deleteChildren)
                GameObject.Destroy(t.gameObject);

            UnityEngine.Object.Destroy(retval.transform.Find("smoke").gameObject);

            // Make a secret node to the right
            Transform rightArrow = nodeParent.Find("MoveArea_E");
            MoveHoloMapAreaNode moveRight = rightArrow.gameObject.GetComponentInChildren<MoveHoloMapAreaNode>();
            moveRight.secret = true;
            rightArrow.gameObject.SetActive(true);

            // Copy/rotate the bridge
            GameObject bridge = UnityEngine.Object.Instantiate(sceneryParent.Find("HoloBridge_Entrance").gameObject, sceneryParent);
            bridge.transform.localEulerAngles = new(0f, 0f, 0f);
            bridge.transform.localPosition = new(bridge.transform.localPosition.x, bridge.transform.localPosition.y, -1.9f);

            // Make the final shop nodes
            BuildSpecialNode(HoloMapNode.NodeDataType.CreateTransformer, 0, Zone.Neutral, nodeParent, sceneryParent, 1.5f, 1f);
            BuildSpecialNode(HoloMapNode.NodeDataType.OverclockCard, 0, Zone.Neutral, nodeParent, sceneryParent, 1.5f, -1f);
            BuildSpecialNode(HoloMapNode.NodeDataType.BuildACard, HoloMapBlueprint.FULL_BRIDGE, Zone.Neutral, nodeParent, sceneryParent, -1.5f, 1f);
            BuildSpecialNode(HoloMapNode.NodeDataType.AttachGem, HoloMapBlueprint.FULL_BRIDGE, Zone.Neutral, nodeParent, sceneryParent, -1.5f, -1f);
            BuildSpecialNode(HoloMapNode.NodeDataType.AddCardAbility, HoloMapBlueprint.FULL_BRIDGE, Zone.Neutral, nodeParent, sceneryParent, -2.5f, 0f);
            BuildSpecialNode(UnlockAscensionItemNodeData.UnlockItemsAscension, 0, Zone.Neutral, nodeParent, sceneryParent, 2.5f, 0f);

            HoloMapArea area = retval.GetComponent<HoloMapArea>();
            area.firstEnterDialogueId = "P03FinalShopNode";
            area.screenPrefab = ResourceBank.Get<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_Shop");

            retval.SetActive(false);
            return retval;
        }

        private static GameObject BuildHubNode()
        {
            GameObject hubNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            GameObject retval = UnityEngine.Object.Instantiate(hubNodeBase);

            // We don't want the bottom arrow
            retval.transform.Find("Nodes/MoveArea_S").gameObject.SetActive(false);
            UnityEngine.Object.Destroy(retval.transform.Find("Nodes/CurrencyGainNode3D").gameObject);

            // We need to set a conditional up arrow
            HoloMapArea areaData = retval.GetComponent<HoloMapArea>();
            BlockDirections(retval, areaData, NORTH, EventManagement.ALL_BOSSES_KILLED);

            // We need to add the draft node
            Transform nodes = retval.transform.Find("Nodes");
            Transform scenery = retval.transform.Find("Scenery");
            HoloMapNode node = BuildSpecialNode(TradeChipsNodeData.TradeChipsForCards, 0, Zone.Neutral, nodes, scenery, 1.5f, 0f);

            // Make a secret node to the right
            Transform rightArrow = nodes.Find("MoveArea_E");
            MoveHoloMapAreaNode moveRight = rightArrow.gameObject.GetComponentInChildren<MoveHoloMapAreaNode>();
            moveRight.secret = true;
            rightArrow.gameObject.SetActive(true);

            // Maybe add rebecha?
            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BROKEN_BRIDGE))
            {
                HoloMapBlueprint rbBp = new(191985)
                {
                    x = 0,
                    y = 3,
                    dialogueEvent = DefaultQuestDefinitions.Rebecha.EventId
                };
                QuestManager.Get(DefaultQuestDefinitions.Rebecha.EventId).QuestGenerated = true;
                BuildDialogueNode(rbBp, nodes, scenery, -2f, -2f);
            }

            retval.SetActive(false);
            return retval;
        }

        private static void BlockDirections(GameObject area, HoloMapArea areaData, int blocked, StoryEvent storyEvent)
        {
            P03Plugin.Log.LogInfo($"Blocking directions");
            List<GameObject> blockIcons = new();
            List<LookDirection> blockedDirections = new();
            foreach (int direction in GetDirections(blocked, true))
            {
                blockedDirections.Add(LOOK_MAPPER[direction]);

                GameObject blockIcon = UnityEngine.Object.Instantiate(BLOCK_ICON, area.transform);
                blockIcons.Add(blockIcon);
                Vector3 pos = REGION_DATA[Zone.Neutral].WallOrientations[direction].Item1;
                blockIcon.transform.localPosition = new(pos.x, 0.3f, pos.z);
                blockIcon.transform.localEulerAngles = REGION_DATA[Zone.Neutral].WallOrientations[direction].Item2;
            }

            BlockDirectionsAreaSequencer sequencer = area.AddComponent<BlockDirectionsAreaSequencer>();
            sequencer.stopIcons = blockIcons;
            sequencer.unblockStoryEvent = storyEvent;
            sequencer.blockedDirections = blockedDirections;
            areaData.specialSequencer = sequencer;
        }

        private static void CleanBattleFromArrow(GameObject room, string direction)
        {
            GameObject southArrow = room.transform.Find($"Nodes/MoveArea_{direction}").gameObject;
            MoveHoloMapAreaNode southNode = southArrow.GetComponent<MoveHoloMapAreaNode>();

            southNode.nodeType = HoloMapNode.NodeDataType.MoveArea;
            southNode.blueprintData = null;
        }

        private static void BuildFastTravelNode(Transform sceneryParent, Transform nodesParent, float x, float z, Zone currentZone)
        {
            P03Plugin.Log.LogInfo($"Creating fast travel node");
            // GameObject hubNodeBase = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_StartingIslandWaypoint");
            // GameObject waypointBase = hubNodeBase.transform.Find("WaypointStation").gameObject;

            GameObject newWaypoint = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapinteractables/WaypointStation"), nodesParent);
            Animator anim = newWaypoint.GetComponentInChildren<Animator>();
            UnityEngine.Object.DestroyImmediate(anim);
            ConditionalWaypointController cwc = newWaypoint.AddComponent<ConditionalWaypointController>();
            cwc.StoryFlag = currentZone == Zone.Magic ? StoryEvent.CanvasDefeated :
                            currentZone == Zone.Tech ? StoryEvent.TelegrapherDefeated :
                            currentZone == Zone.Nature ? StoryEvent.PhotographerDefeated :
                            StoryEvent.ArchivistDefeated;

            newWaypoint.transform.localPosition = new(x - 0.5f, newWaypoint.transform.localPosition.y, z);
        }

        private static void BuildHoloPeltMinigame(Transform sceneryParent, Transform nodesParent, float x, float z)
        {
            P03Plugin.Log.LogDebug("Building Holo Pelt Minigame!");
            GameObject well = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapinteractables/HoloPeltMinigame"), nodesParent);
            well.transform.localPosition = new(x, well.transform.localPosition.y, z);

            // Label
            GameObject sampleObject = SpecialNodePrefabs[HoloMapNode.NodeDataType.BuildACard].transform.Find("HoloFloatingLabel").gameObject;
            GameObject labelObject = UnityEngine.Object.Instantiate(sampleObject, well.transform);
            labelObject.transform.localPosition = new(-1.0017f, -0.2872f, 0.2128f);
            HoloFloatingLabel label = labelObject.GetComponent<HoloFloatingLabel>();
            label.line.gameObject.SetActive(false);
            label.line = null;
            labelObject.name = "HoverText";
        }

        private static void BuildMycologistWell(Transform sceneryParent, Transform nodesParent, float x, float z)
        {
            // Get the well prefab
            P03Plugin.Log.LogDebug("Building Well!");
            GameObject well = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapinteractables/HoloWell"), nodesParent);
            well.transform.localPosition = new(x, well.transform.localPosition.y, z);

            HoloMapWell existingWell = well.GetComponentInChildren<HoloMapWell>();

            HoloMapNode node = well.GetComponentInChildren<HoloMapNode>();
            //if (node != null)
            //    GameObject.Destroy(node);

            MycologistWell newWell = well.AddComponent<MycologistWell>();
            newWell.anim = existingWell.anim;
            newWell.handleDown = existingWell.handleDown;

            UnityEngine.Object.Destroy(existingWell);
        }

        private static void BuildBrokenGeneratorRoom(GameObject roomObject, Transform sceneryParent, Transform nodesParent)
        {
            // Generate the scenery 
            GameObject generator = UnityEngine.Object.Instantiate(GetGameObject("NatureEntrance/Scenery/HoloGenerator"), sceneryParent);
            generator.transform.localPosition = new(.396f, 0f, .813f);
            generator.transform.localEulerAngles = new(0f, 50f, 0f);

            GameObject cylinder1 = UnityEngine.Object.Instantiate(GetGameObject("NatureEntrance/Scenery/HoloGenerator/Generator_Cylinder"), sceneryParent);
            GameObject cylinder2 = UnityEngine.Object.Instantiate(GetGameObject("NatureEntrance/Scenery/HoloGenerator/Generator_Cylinder"), sceneryParent);

            GameObject toxicSlime = UnityEngine.Object.Instantiate(GetGameObject("WizardMainPath_3/Scenery/HoloSlime_Pile_2"), sceneryParent);
            toxicSlime.transform.localPosition = new(-1.34f, toxicSlime.transform.localPosition.y, -.92f);
            toxicSlime.transform.localEulerAngles = new(0f, 50f, 0f);

            GameObject toxicSlime2 = UnityEngine.Object.Instantiate(GetGameObject("WizardMainPath_3/Scenery/HoloSlime_Pile_2"), sceneryParent);
            toxicSlime2.transform.localPosition = new(-1.86f, toxicSlime.transform.localPosition.y, -.28f);

            List<GameObject> generators = new() { generator };
            List<GameObject> rubble = new() { toxicSlime, toxicSlime2 };

            // Create the damage race node
            GameObject damageRaceNode = GetGeneratorCountdownNode(nodesParent, generators, rubble);
            damageRaceNode.transform.localPosition = new(-0.9f, .25f, -1.45f);
        }

        private static GameObject BuildLowerTowerRoom()
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/map/holomapareas/HoloMapArea_TempleWizardEntrance");
            GameObject retval = UnityEngine.Object.Instantiate(prefab);

            // No dialogue
            HoloMapArea area = retval.GetComponent<HoloMapArea>();
            area.firstEnterDialogueId = null;

            // Kill the shop node:
            retval.transform.Find("Nodes/MoveArea_E").gameObject.SetActive(false);

            // Kill the open door
            retval.transform.Find("Scenery/Doorframe").gameObject.SetActive(false);

            // Remove the battle info from the West and South arrows
            CleanBattleFromArrow(retval, "W (NORTH)");
            CleanBattleFromArrow(retval, "S");

            // Fill the open door with a clone of the wall piece
            GameObject rightWall = retval.transform.Find("Scenery/RightWall").gameObject;
            GameObject newRightWall = UnityEngine.Object.Instantiate(rightWall, rightWall.transform.parent);
            newRightWall.transform.localPosition = new Vector3(rightWall.transform.localPosition.x, rightWall.transform.localPosition.y, 0.45f);

            retval.SetActive(false);
            return retval;
        }

        private static EncounterBlueprintData GetBlueprintForRegion(Zone regionId, int color, int encounterIndex, bool useDefaultRegionSelection)
        {
            string encounterName = default;
            Zone regionZone = color == 1 || !useDefaultRegionSelection ? Zone.Neutral : regionId;

            // This is just a failsafe. The index should always match UNLESS you uninstalled a mod partway through a run.
            // I could just let this fail because you're a dumbass, but I'm a nice guy.
            if (encounterIndex <= -1 || encounterIndex >= REGION_DATA[regionZone].Region.encounters.Count)
            {
                List<string> encounters = REGION_DATA[regionZone].Region.encounters.Select(ebd => ebd.name).ToList();
                encounterName = encounters[UnityEngine.Random.Range(0, encounters.Count)];
            }
            else
            {
                encounterName = REGION_DATA[regionZone].Region.encounters[encounterIndex].name;
            }

            // Get the encounter from the manager based on the name
            return EncounterManager.AllEncountersCopy.First(bp => bp.name == encounterName);
        }

        private static GameObject BuildMapAreaPrefab(Zone regionId, HoloMapBlueprint bp)
        {
            P03Plugin.Log.LogInfo($"Building gameobject for [{bp.x},{bp.y}]");

            if (bp.opponent == Opponent.Type.P03Boss)
            {
                GameObject retval = BuildP03BossNode();
                retval.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";
                return retval;
            }

            if (bp.opponent != Opponent.Type.Default)
            {
                GameObject retval = UnityEngine.Object.Instantiate(BossPrefabs[bp.opponent]);
                if (bp.opponent == Opponent.Type.TelegrapherBoss)
                {
                    // retval.transform.Find("Nodes/MoveArea_E").gameObject.SetActive(false);
                    // retval.transform.Find("Nodes/MoveArea_W").gameObject.SetActive(false);
                    CleanBattleFromArrow(retval, "E");
                    CleanBattleFromArrow(retval, "S");
                    retval.GetComponent<HoloMapArea>().screenPrefab = REGION_DATA[regionId].ScreenPrefab;
                }

                //FlyBackToCenterIfBossDefeated returnToCenter = retval.AddComponent<FlyBackToCenterIfBossDefeated>();
                //retval.GetComponent<HoloMapArea>().specialSequencer = returnToCenter;

                // This is a bit of a CYA
                // We want to make sure the battle id and the opponent always match - this can get out of sync with some of our custom patches
                HoloMapBossNode bossNode = retval.GetComponentInChildren<HoloMapBossNode>();
                bossNode.specialEncounterId = BossBattleSequencer.GetSequencerIdForBoss(bp.opponent);
                CardBattleNodeData bossData = bossNode.Data as CardBattleNodeData;
                bossData.specialBattleId = BossBattleSequencer.GetSequencerIdForBoss(bp.opponent);

                // Destroy all of the loot nodes
                foreach (HoloMapNode node in bossNode.lootNodes)
                    UnityEngine.Object.Destroy(node.gameObject);
                bossNode.lootNodes.Clear();

                P03Plugin.Log.LogInfo($"Setting special battle id {bossData.specialBattleId} for opponent {bp.opponent}");

                retval.SetActive(false);
                retval.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";
                return retval;
            }

            RegionGeneratorData genData = REGION_DATA[regionId];
            if (bp.specialTerrain == HoloMapBlueprint.MYCOLOGIST_WELL)
                genData = REGION_DATA[Zone.Mycologist];

            if (bp.upgrade == HoloMapNode.NodeDataType.FastTravel)
                return BuildHubNode();

            if (bp.specialTerrain == HoloMapBlueprint.LOWER_TOWER_ROOM)
                return BuildLowerTowerRoom();

            if (bp.specialTerrain == HoloMapBlueprint.FINAL_SHOP_NODE)
                return BuildFinalShopNode();

            if (bp.specialTerrain == HoloMapBlueprint.TEST_OF_STREGTH)
                return BuildTestOfStrengthNode();

            if (bp.specialTerrain == HoloMapBlueprint.MIRROR)
                return BuildMirrorNode();

            P03Plugin.Log.LogInfo($"Instantiating base object {neutralHoloPrefab}");
            GameObject area = UnityEngine.Object.Instantiate(neutralHoloPrefab);
            area.name = $"ProceduralMapArea_{regionId}_{bp.x}_{bp.y})";

            HoloMapArea areaData = area.GetComponent<HoloMapArea>();
            areaData.screenPrefab = null;

            var customOffset = genData.GetCenterOffset(bp.color);
            areaData.centerOffset = customOffset.GetValueOrDefault(areaData.centerOffset);

            P03Plugin.Log.LogInfo($"Getting nodes");
            GameObject nodes = area.transform.Find("Nodes").gameObject;

            if (DIR_LOOKUP.ContainsKey(bp.specialDirection))
            {
                if (bp.specialDirectionType == HoloMapBlueprint.TRADE)
                {
                    GameObject arrowToReplace = area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[bp.specialDirection]}").gameObject;
                    arrowToReplace.GetComponent<HoloMapNode>().nodeType = HoloMapNode.NodeDataType.MoveAreaTrade;
                }
                if (bp.specialDirectionType is HoloMapBlueprint.BATTLE or HoloMapBlueprint.NEUTRAL_BATTLE)
                {
                    P03Plugin.Log.LogInfo($"Finding arrow to destroy");
                    GameObject arrowToReplace = area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[bp.specialDirection]}").gameObject;
                    P03Plugin.Log.LogInfo($"Destroying arrow");
                    UnityEngine.Object.DestroyImmediate(arrowToReplace);

                    P03Plugin.Log.LogInfo($"Copying arrow");
                    GameObject newArrow = UnityEngine.Object.Instantiate(ArrowPrefabs[bp.specialDirection | ENEMY], nodes.transform);
                    newArrow.name = $"MoveArea_{DIR_LOOKUP[bp.specialDirection]}";
                    HoloMapNode node = newArrow.GetComponent<HoloMapNode>();

                    node.blueprintData = GetBlueprintForRegion(regionId, bp.color, bp.encounterIndex, bp.specialDirectionType == HoloMapBlueprint.BATTLE);
                    node.encounterDifficulty = bp.encounterDifficulty;
                    node.bridgeBattle = (bp.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) != 0;

                    foreach (BattleModManager.ID id in bp.battleMods)
                        BattleModManager.SetBlueprintRule(node.blueprintData.name, id);

                    if (bp.battleTerrainIndex > 0 && (bp.specialTerrain & HoloMapBlueprint.FULL_BRIDGE) == 0)
                    {
                        string[] terrain = genData.Terrain[bp.battleTerrainIndex - 1];
                        if (UnityEngine.Random.value > 0.5)
                        {
                            node.playerTerrain = terrain.Take(5).Select(s => s == default ? null : CardLoader.GetCardByName(s)).ToArray();
                            node.opponentTerrain = terrain.Skip(5).Select(s => s == default ? null : CardLoader.GetCardByName(s)).ToArray();
                        }
                        else
                        {
                            node.playerTerrain = terrain.Take(5).Select(s => s == default ? null : CardLoader.GetCardByName(s)).Reverse().ToArray();
                            node.opponentTerrain = terrain.Skip(5).Select(s => s == default ? null : CardLoader.GetCardByName(s)).Reverse().ToArray();
                        }
                    }
                    else
                    {
                        node.playerTerrain = new CardInfo[5];
                        node.opponentTerrain = new CardInfo[5];
                    }
                }
            }

            P03Plugin.Log.LogInfo($"Setting corners (walls with openings) active");
            Transform scenery = area.transform.Find("Scenery");
            var cornerPrefabs = genData.GetCornerPrefabs(bp.color);
            if (cornerPrefabs != null && cornerPrefabs.Keys.Count > 0)
            {
                foreach (int key in DIR_LOOKUP.Keys)
                {
                    if ((bp.arrowDirections & key) != 0)
                    {
                        foreach (string cornerPrefabKey in cornerPrefabs[key])
                        {
                            var obj = UnityEngine.Object.Instantiate(GetGameObject(cornerPrefabKey), scenery);
                            obj.SetActive(true);
                            if (cornerPrefabKey.StartsWith("-"))
                            {
                                obj.transform.localPosition = new(-obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z);
                                obj.transform.localEulerAngles += new Vector3(0f, 180f, 0f);
                            }
                            if (cornerPrefabKey.StartsWith("+"))
                            {
                                for (int i = 0; i < Math.Min(obj.transform.childCount, 3); i++)
                                    obj.transform.GetChild(i).gameObject.SetActive(true);
                            }
                            if (cornerPrefabKey.Contains('|'))
                            {
                                string[] comps = cornerPrefabKey.Split('|');

                                string locKey = comps[1];
                                string[] xyz = locKey.Split(',');
                                obj.transform.localPosition = new(float.Parse(xyz[0], CultureInfo.InvariantCulture), float.Parse(xyz[1], CultureInfo.InvariantCulture), float.Parse(xyz[2], CultureInfo.InvariantCulture));

                                if (comps.Length > 2)
                                {
                                    string rotKey = cornerPrefabKey.Split('|')[2];
                                    string[] xyzr = rotKey.Split(',');
                                    obj.transform.localEulerAngles = new(float.Parse(xyzr[0], CultureInfo.InvariantCulture), float.Parse(xyzr[1], CultureInfo.InvariantCulture), float.Parse(xyzr[2], CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }
                }
            }

            P03Plugin.Log.LogInfo($"Setting arrows and walls active");
            var wallPrefabs = genData.GetWallPrefabs(bp.color);
            if (wallPrefabs != null && wallPrefabs.Keys.Count > 0)
            {
                foreach (int key in DIR_LOOKUP.Keys)
                {
                    area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.SetActive((bp.arrowDirections & key) != 0);

                    // If this is a secret arrow:
                    if ((bp.secretDirection & key) != 0)
                        area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.GetComponentInChildren<MoveHoloMapAreaNode>().secret = true;

                    if ((bp.arrowDirections & key) == 0)
                    {
                        foreach (string wallPrefabKey in wallPrefabs[key])
                        {
                            var obj = UnityEngine.Object.Instantiate(GetGameObject(wallPrefabKey), scenery);
                            obj.SetActive(true);
                            if (wallPrefabKey.StartsWith("-"))
                            {
                                obj.transform.localPosition = new(-obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z);
                                obj.transform.localEulerAngles += new Vector3(0f, 180f, 0f);
                            }
                            if (wallPrefabKey.StartsWith("+"))
                            {
                                for (int i = 0; i < Math.Min(obj.transform.childCount, 3); i++)
                                    obj.transform.GetChild(i).gameObject.SetActive(true);
                            }
                            if (wallPrefabKey.Contains('|'))
                            {
                                string[] comps = wallPrefabKey.Split('|');

                                string locKey = comps[1];
                                string[] xyz = locKey.Split(',');
                                obj.transform.localPosition = new(float.Parse(xyz[0], CultureInfo.InvariantCulture), float.Parse(xyz[1], CultureInfo.InvariantCulture), float.Parse(xyz[2], CultureInfo.InvariantCulture));

                                if (comps.Length > 2)
                                {
                                    string rotKey = wallPrefabKey.Split('|')[2];
                                    string[] xyzr = rotKey.Split(',');
                                    obj.transform.localEulerAngles = new(float.Parse(xyzr[0], CultureInfo.InvariantCulture), float.Parse(xyzr[1], CultureInfo.InvariantCulture), float.Parse(xyzr[2], CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                GameObject wall = GetGameObject(genData.GetWall(bp.color));
                foreach (int key in DIR_LOOKUP.Keys)
                {
                    // Set only the correct arrows active
                    area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.SetActive((bp.arrowDirections & key) != 0);

                    // If this is a secret arrow:
                    if ((bp.secretDirection & key) != 0)
                        area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").gameObject.GetComponentInChildren<MoveHoloMapAreaNode>().secret = true;

                    // Walls
                    if (wall != null)
                    {
                        if ((bp.arrowDirections & key) == 0)
                        {
                            GameObject wallClone = UnityEngine.Object.Instantiate(wall, scenery);
                            wallClone.transform.localPosition = genData.WallOrientations[key].Item1;
                            wallClone.transform.localEulerAngles = genData.WallOrientations[key].Item2;
                        }
                    }
                }
            }

            // Offset the arrows if this area has a custom offset.
            if (customOffset.HasValue)
            {
                var offset3 = new Vector3(customOffset.Value.x, 0f, customOffset.Value.y);

                foreach (int key in DIR_LOOKUP.Keys)
                {
                    area.transform.Find($"Nodes/MoveArea_{DIR_LOOKUP[key]}").localPosition += offset3;
                }
            }

            if (bp.specialTerrain == HoloMapBlueprint.BROKEN_GENERATOR)
                BuildBrokenGeneratorRoom(area, scenery, nodes.transform);

            P03Plugin.Log.LogInfo($"Generating random scenery");

            // Add the landmarks if necessary
            if ((bp.specialTerrain & HoloMapBlueprint.LANDMARKER) != 0)
            {
                if (regionId == Zone.Tech)
                {
                    BuildElevatorRoom(bp, area, scenery, nodes.transform);
                }
                else
                {
                    foreach (string objId in genData.Landmarks[bp.color - 1])
                        UnityEngine.Object.Instantiate(GetGameObject(objId), scenery);
                }
            }

            // Get effects mabye
            string effectsKey = genData.GetEffects(bp.color);
            if (!string.IsNullOrEmpty(effectsKey))
            {
                GameObject effectsObject = UnityEngine.Object.Instantiate(GetGameObject(effectsKey), scenery);
                effectsObject.transform.localPosition = Vector3.zero;
                effectsObject.transform.localEulerAngles = Vector3.zero;
            }

            // Add the normal scenery
            // For each section of the board that doesn't have an arrow on it
            List<int> directions = GetDirections(bp.arrowDirections, false, true).ToList();

            // Special rules for the tech factory; don't generate scenery in the three north quadrants
            if (regionId == Zone.Tech && (bp.color == 1 || bp.color == 3))
            {
                directions = new() { SOUTH | WEST, SOUTH | EAST };
            }

            int quadrants = directions.Count;

            while (directions.Count > 0)
            {
                bool firstQuadrant = quadrants == directions.Count;
                bool secondQuadrant = quadrants - 1 == directions.Count;

                int dir = directions[0];

                if (bp.specialTerrain == HoloMapBlueprint.BROKEN_GENERATOR && firstQuadrant)
                    dir = NORTH | EAST;

                if (bp.specialTerrain == HoloMapBlueprint.BROKEN_GENERATOR && secondQuadrant)
                    dir = SOUTH | EAST;

                if ((bp.specialTerrain & HoloMapBlueprint.FAST_TRAVEL_NODE) > 0 && firstQuadrant)
                    dir = SOUTH | EAST;

                directions.Remove(dir);

                List<Tuple<float, float>> sceneryLocations = GetSpotsForQuadrant(dir);

                bool firstObject = true;
                while (sceneryLocations.Count > 0)
                {
                    int spIdx = firstObject ? 0 : UnityEngine.Random.Range(0, sceneryLocations.Count);
                    Tuple<float, float> specialLocation = sceneryLocations[spIdx];
                    sceneryLocations.RemoveAt(spIdx);

                    if (firstQuadrant && firstObject && (bp.specialTerrain & HoloMapBlueprint.FAST_TRAVEL_NODE) > 0)
                    {
                        BuildFastTravelNode(scenery.transform, nodes.transform, specialLocation.Item1, specialLocation.Item2, regionId);
                        firstObject = false;
                        break; // Nothing else goes in this quadrant
                    }

                    if (firstQuadrant && firstObject && bp.upgrade != HoloMapNode.NodeDataType.MoveArea)
                    {
                        BuildSpecialNode(bp, regionId, nodes.transform, scenery.transform, specialLocation.Item1, specialLocation.Item2);
                        firstObject = false;
                        continue;
                    }

                    if (secondQuadrant && firstObject && bp.specialTerrain == HoloMapBlueprint.MYCOLOGIST_WELL)
                    {
                        BuildMycologistWell(scenery.transform, nodes.transform, specialLocation.Item1, specialLocation.Item2);
                        firstObject = false;
                        continue;
                    }

                    if (secondQuadrant && firstObject && bp.specialTerrain == HoloMapBlueprint.HOLO_PELT_MINIGAME)
                    {
                        BuildHoloPeltMinigame(scenery.transform, nodes.transform, specialLocation.Item1, specialLocation.Item2);
                        firstObject = false;
                        continue;
                    }

                    if (secondQuadrant && firstObject && !bp.EligibleForDialogue)
                    {
                        BuildDialogueNode(bp, nodes.transform, scenery.transform, specialLocation.Item1, specialLocation.Item2);
                        firstObject = false;
                        continue;
                    }

                    if (firstObject && (bp.specialTerrain & HoloMapBlueprint.LANDMARKER) != 0)
                    {
                        firstObject = false;
                        continue;
                    }

                    string sceneryKey = firstObject && bp.specialTerrain != HoloMapBlueprint.BROKEN_GENERATOR ? genData.GetObject(bp.color, bp.randomSeed) : genData.GetTerrain(bp.color, bp.randomSeed);
                    bp.randomSeed += 1;

                    firstObject = false;

                    if (string.IsNullOrEmpty(sceneryKey))
                        continue;

                    //string sceneryKey = scenerySource[UnityEngine.Random.Range(0, scenerySource.Length)];
                    GameObject sceneryObject = UnityEngine.Object.Instantiate(GetGameObject(sceneryKey), scenery);
                    sceneryObject.transform.localPosition = new Vector3(specialLocation.Item1, sceneryObject.transform.localPosition.y, specialLocation.Item2);

                    if (!sceneryKey.StartsWith("+"))
                        sceneryObject.transform.localEulerAngles = new Vector3(sceneryObject.transform.localEulerAngles.x, UnityEngine.Random.Range(0f, 360f), sceneryObject.transform.localEulerAngles.z);
                }
            }

            P03Plugin.Log.LogInfo($"Generating special terrain");
            foreach (int key in SpecialTerrainPrefabs.Keys)
            {
                if ((bp.specialTerrain & key) != 0)
                {
                    foreach (GameObject obj in SpecialTerrainPrefabs[key])
                        UnityEngine.Object.Instantiate(obj, scenery);
                }
            }

            P03Plugin.Log.LogInfo($"Setting grid data");
            areaData.GridX = bp.x;
            areaData.GridY = bp.y;
            areaData.audioLoopsConfig = genData.AudioConfig;
            areaData.screenPrefab ??= genData.ScreenPrefab; // Only set if it wasn't set already by someone else
            areaData.mainColor = genData.MainColor;
            areaData.lightColor = genData.MainColor;

            if (bp.blockedDirections != BLANK)
                BlockDirections(area, areaData, bp.blockedDirections, EventManagement.ALL_ZONE_ENEMIES_KILLED);

            // Give every node a unique id
            int nodeId = 10;
            foreach (MapNode node in area.GetComponentsInChildren<MapNode>())
                node.nodeId = node is MoveHoloMapAreaNode ? nodeId++ - 10 : nodeId++;

            if ((bp.specialTerrain & HoloMapBlueprint.FAST_TRAVEL_NODE) != 0)
            {
                if (!EventManagement.SawMapInfo)
                    areaData.firstEnterDialogueId = "P03FastTravelKaycee";
            }

            area.SetActive(false);
            return area;
        }

        private static void BuildElevatorRoom(HoloMapBlueprint bp, GameObject area, Transform scenery, Transform nodes)
        {
            if (bp.y >= 3)
                BuildSouthElevatorRoom(bp, area, scenery, nodes);
            else
                BuildNorthElevatorRoom(bp, area, scenery, nodes);
        }

        private static void BuildNorthElevatorRoom(HoloMapBlueprint bp, GameObject area, Transform scenery, Transform nodes)
        {
            // North elevator cannot have a down arrow
            area.transform.Find("Nodes/MoveArea_S").GetComponentInChildren<MoveHoloMapAreaNode>().direction = ElevatorArrowPatches.ELEVATOR_DOWN;

            // Add the down elevator sequencer
            // var elevator = area.AddComponent<AscensionElevatorDown>();
            // area.GetComponent<HoloMapArea>().specialSequencer = elevator;
            area.GetComponent<HoloMapArea>().screenPrefab = ResourceBank.Get<GameObject>("prefabs/map/holomapscreens/HoloMapScreen_TechElevatorTop");

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                    GameObject.Instantiate(GetGameObject($"TechCenter/Scenery/HoloTechPillar"), scenery);
                else
                    GameObject.Instantiate(GetGameObject($"TechCenter/Scenery/HoloTechPillar ({i})"), scenery);
            }
        }

        private static void BuildSouthElevatorRoom(HoloMapBlueprint bp, GameObject area, Transform scenery, Transform nodes)
        {
            // South elevator up arrow goes in a special direction
            area.transform.Find("Nodes/MoveArea_N").GetComponentInChildren<MoveHoloMapAreaNode>().direction = ElevatorArrowPatches.ELEVATOR_UP;

            // Create all the missing pieces
            var elevatorBeam = GameObject.Instantiate(GetGameObject("TechCenter/Scenery/LiftPillar"), scenery);
            elevatorBeam.SetActive(false);

            for (int i = 0; i < 4; i++)
            {
                GameObject.Instantiate(GetGameObject($"TechCenter/Scenery/HoloSatelliteDish_{i + 1}"), scenery);

                if (i == 0)
                    GameObject.Instantiate(GetGameObject($"TechCenter/Scenery/HoloTechPillar"), scenery);
                else
                    GameObject.Instantiate(GetGameObject($"TechCenter/Scenery/HoloTechPillar ({i})"), scenery);
            }
        }

        private static void ConnectArea(HoloMapWorldData.AreaData[,] map, HoloMapBlueprint bp)
        {
            GameObject area = map[bp.x, bp.y].prefab;

            if (area == null)
                return;

            HoloMapArea areaData = area.GetComponent<HoloMapArea>();

            // The index of DirectionNodes has to correspond to the integer value of the LookDirection enumeration
            areaData.DirectionNodes.Clear();
            for (int i = 0; i < 4; i++)
                areaData.DirectionNodes.Add(null);

            Transform nodes = area.transform.Find("Nodes");

            foreach (Transform arrow in nodes)
            {
                if (arrow.gameObject.name.StartsWith("MoveArea"))
                    areaData.DirectionNodes[(int)LOOK_NAME_MAPPER[arrow.gameObject.name]] = arrow.gameObject.activeSelf ? arrow.gameObject.GetComponent<MoveHoloMapAreaNode>() : null;
            }
        }

        public static string GetAscensionWorldID(Zone regionCode) => regionCode == Zone.Neutral ? $"ascension_0_{regionCode}" : $"ascension_{EventManagement.CompletedZones.Count}_{regionCode}";

        public static Zone GetRegionCodeFromWorldID(string worldId)
        {
            string[] components = worldId.Split('_');
            return (Zone)Enum.Parse(typeof(Zone), components[components.Length - 1]);
        }

        public static int GetStartingQuadrant(Zone regionCode)
        {
            var space = GetStartingSpace(regionCode);
            return space.Item1 <= 2 ? (space.Item2 <= 2 ? 1 : 2) : (space.Item2 <= 2 ? 3 : 4);
        }

        public static Tuple<int, int> GetStartingSpace(Zone regionCode)
        {
            if (regionCode == Zone.Nature)
                return new(0, 3);
            if (regionCode == Zone.Tech)
                return new(3, 5);
            return new(0, 2);
        }

        [Obsolete]
        public static HoloMapWorldData GetAscensionWorldbyId(string id)
        {
            P03Plugin.Log.LogInfo($"Getting world for {id}");

            HoloMapWorldData data = ScriptableObject.CreateInstance<HoloMapWorldData>();
            data.name = id;

            BattleModManager.ResetAllBlueprintRules();

            string[] idSplit = id.Split('_');
            int regionCount = int.Parse(idSplit[1]);
            Zone regionCode = (Zone)Enum.Parse(typeof(Zone), idSplit[2]);

            // Start the process
            Building = true;
            BuildingZone = regionCode;

            List<HoloMapBlueprint> blueprints = BuildBlueprint(regionCount, regionCode, P03AscensionSaveData.RandomSeed);

            int xDimension = blueprints.Select(b => b.x).Max() + 1;
            int yDimension = blueprints.Select(b => b.y).Max() + 1;

            data.areas = new HoloMapWorldData.AreaData[xDimension, yDimension];

            foreach (HoloMapBlueprint bp in blueprints)
            {
                GameObject mapArea = BuildMapAreaPrefab(regionCode, bp);

                if (regionCode != Zone.Neutral)
                    Minimap.CreateMinimap(mapArea.transform, blueprints, $"ProceduralMapArea_{regionCode}", GetAscensionWorldID(regionCode), bp.x, bp.y);

                data.areas[bp.x, bp.y] = new() { prefab = mapArea };
            }

            // The second pass creates relationships between everything
            foreach (HoloMapBlueprint bp in blueprints)
                ConnectArea(data.areas, bp);

            return data;
        }

        [HarmonyPatch(typeof(HoloMapArea), "OnAreaActive")]
        [HarmonyPrefix]
        public static void ActivateObject(HoloMapArea __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (SaveFile.IsAscension && !__instance.gameObject.activeSelf)
                __instance.gameObject.SetActive(true);

            // This is a bizarre hack to try to get rid of that stupid floating currency node
            // There should never be a currency node in this zone
            if (RunBasedHoloMap.GetRegionCodeFromWorldID(HoloMapAreaManager.Instance.CurrentWorld.name) == Zone.Neutral)
            {
                List<MapNode> nodes = new List<MapNode>(__instance.GetComponentsInChildren<HoloMapGainCurrencyNode>());

                foreach (var n in nodes)
                {
                    MapNodeManager.Instance.nodes.Remove(n);
                    GameObject.Destroy(n.gameObject);
                }
            }
        }

        private static bool ValidateWorldData(HoloMapWorldData data)
        {
            if (data == null || data.areas == null)
                return false;

            for (int i = 0; i < data.areas.GetLength(0); i++)
            {
                for (int j = 0; j < data.areas.GetLength(1); j++)
                {
                    if (data.areas[i, j] != null && data.areas[i, j].prefab != null)
                        return true;
                }
            }

            return false;
        }

        public static void ClearWorldData()
        {
            P03Plugin.Log.LogInfo("Clearing world data");

            // This completely clears the cache of game objects that we have access to
            foreach (KeyValuePair<string, HoloMapWorldData> entry in worldDataCache)
            {
                for (int i = 0; i < entry.Value.areas.GetLength(0); i++)
                {
                    for (int j = 0; j < entry.Value.areas.GetLength(1); j++)
                    {
                        if (entry.Value.areas[i, j] != null && entry.Value.areas[i, j].prefab != null)
                            UnityEngine.Object.DestroyImmediate(entry.Value.areas[i, j].prefab);
                    }
                }
            }

            worldDataCache.Clear();
            BattleModManager.ResetAllBlueprintRules();

            foreach (KeyValuePair<HoloMapNode.NodeDataType, GameObject> entry in SpecialNodePrefabs)
            {
                if (entry.Value != null && !objectLookups.Values.Contains(entry.Value))
                    UnityEngine.Object.DestroyImmediate(entry.Value);
            }

            SpecialNodePrefabs.Clear();

            foreach (KeyValuePair<int, GameObject> entry in ArrowPrefabs)
            {
                if (entry.Value != null && !objectLookups.Values.Contains(entry.Value))
                    UnityEngine.Object.DestroyImmediate(entry.Value);
            }

            ArrowPrefabs.Clear();

            foreach (KeyValuePair<int, GameObject[]> entry in SpecialTerrainPrefabs)
            {
                foreach (GameObject obj in entry.Value)
                {
                    if (obj != null && !objectLookups.Values.Contains(obj))
                        UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            SpecialTerrainPrefabs.Clear();
        }

        [HarmonyPatch(typeof(HoloMapDataLoader), "GetWorldById")]
        [HarmonyPrefix]
        [Obsolete]
        private static bool PatchGetAscensionWorldById(ref HoloMapWorldData __result, string id)
        {
            if (id.ToLowerInvariant().StartsWith("ascension_"))
            {
                if (worldDataCache.ContainsKey(id) && ValidateWorldData(worldDataCache[id]))
                {
                    __result = worldDataCache[id];
                    return false;
                }

                try
                {
                    Initialize();
                    if (worldDataCache.ContainsKey(id))
                        worldDataCache.Remove(id);
                    worldDataCache.Add(id, GetAscensionWorldbyId(id));
                    __result = worldDataCache[id];
                    return false;

                }
                finally
                {
                    Building = false;
                }
            }
            return true;
        }
    }
}