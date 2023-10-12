using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public class Minimap : MonoBehaviour
    {
        public const int MINIMAP_SIZE = 6;

        public const float SQUARE_SIZE = 0.16f;

        public const float SCALE = 0.09f;

        public const string SHADER_NAME = "Standard";

        private const string BALL_PATH = "GameTable/HoloGameMap/Anim/Console/DynamicElements/HoloFastTravelMap/FastTravelMapNode_Wizard/Anim/Model";
        private const string LINE_PATH = "GameTable/HoloGameMap/Anim/Console/DynamicElements/HoloFastTravelMap/Lines/Line";

        private static GameObject _ballPseudoPrefab;
        private static GameObject BallPseudoPrefab
        {
            get
            {
                if (_ballPseudoPrefab == null)
                    _ballPseudoPrefab = GameObject.Find(BALL_PATH);

                return _ballPseudoPrefab;
            }
        }

        private static GameObject _linePseudoPrefab;
        private static GameObject LinePseudoPrefab
        {
            get
            {
                if (_linePseudoPrefab == null)
                    _linePseudoPrefab = GameObject.Find(LINE_PATH);

                return _linePseudoPrefab;
            }
        }

        private static GameObject FindRendererParent(GameObject obj)
        {
            if (obj.name == "RendererParent")
                return obj;

            foreach (Transform t in obj.transform)
            {
                GameObject temp = FindRendererParent(t.gameObject);
                if (temp != null)
                    return temp;
            }

            return null;
        }

        private static bool AdjacentHasEncounter(List<HoloMapBlueprint> map, HoloMapBlueprint bp, int encounterType)
        {
            foreach (var adj in AdjacentTo(map, bp))
                if (adj.specialDirectionType == encounterType && adj.GetAdjacentNode(map, adj.specialDirection) == bp)
                    return true;

            return false;
        }

        private static GameObject BuildGameObject(HoloMapBlueprint bp, List<HoloMapBlueprint> map, Transform parent, string worldId, int homeX, int homeY)
        {
            GameObject mapNode = new($"minimapnode({bp.KeyCode})");
            mapNode.transform.SetParent(parent, false);
            mapNode.transform.localPosition = new Vector3(bp.x * SQUARE_SIZE, 0f, -bp.y * SQUARE_SIZE);
            mapNode.transform.localScale = new Vector3(SCALE, SCALE, SCALE);
            GameObject ball = GameObject.Instantiate(BallPseudoPrefab, mapNode.transform, false);
            ball.name = "ball";

            if (bp.upgrade != HoloMapNode.NodeDataType.MoveArea)
            {
                P03Plugin.Log.LogInfo($"Creating minimap icon for {bp.upgrade}");
                GameObject icon = GameObject.Instantiate(FindRendererParent(RunBasedHoloMap.SpecialNodePrefabs[bp.upgrade]).gameObject, mapNode.transform, false);
                icon.name = "icon";
                icon.transform.localScale = new Vector3(3f, 3f, 3f);
                icon.transform.localEulerAngles = new Vector3(53f, 57.0029f, 57.1122f);
            }

            if (AdjacentHasEncounter(map, bp, HoloMapBlueprint.BATTLE))
            {
                P03Plugin.Log.LogInfo($"Creating skull icon for {bp.KeyCode}");
                GameObject refObject = RunBasedHoloMap.GetGameObject("neutralwestmain_2", "Nodes/MoveArea_W");
                P03Plugin.Log.LogInfo($"ref object");
                GameObject skullIcon = refObject.transform.Find("RendererParent/AdditionalIcon").gameObject;
                P03Plugin.Log.LogInfo($"skull icon");
                GameObject icon = GameObject.Instantiate(skullIcon, mapNode.transform, false);
                P03Plugin.Log.LogInfo($"instantiated");
                icon.name = "skull";
                icon.transform.localScale = new Vector3(4f, 4f, 4f);
                icon.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                icon.transform.localEulerAngles = new Vector3(53f, 57.0029f, 57.1122f);
            }

            if (AdjacentHasEncounter(map, bp, HoloMapBlueprint.TRADE))
            {
                P03Plugin.Log.LogInfo($"Creating trade icon for {bp.KeyCode}");
                GameObject refObject = RunBasedHoloMap.GetGameObject("neutralwestmain_2", "Nodes/MoveArea_W");
                P03Plugin.Log.LogInfo($"ref object");
                GameObject skullIcon = refObject.transform.Find("RendererParent/TradeIcon").gameObject;
                P03Plugin.Log.LogInfo($"icon");
                GameObject icon = GameObject.Instantiate(skullIcon, mapNode.transform, false);
                GameObject.Destroy(icon.GetComponent<AutoRotate>());
                GameObject.Destroy(icon.GetComponent<CleanupInstancedMaterialOnDestroy>());
                P03Plugin.Log.LogInfo($"instantiated");
                icon.name = "trade";
                icon.transform.localScale = new Vector3(3f, 3f, 3f);
                icon.transform.localPosition = new Vector3(0f, 0f, 0f);
                icon.transform.localEulerAngles = new Vector3(53f, 57.0029f, 57.1122f);
            }

            if (bp.x != homeX || bp.y != homeY)
            {
                BoxCollider coll = mapNode.AddComponent<BoxCollider>();
                coll.extents = ball.GetComponentInChildren<Renderer>().bounds.extents / SCALE;

                var ft = mapNode.AddComponent<HoloMinimapFastTravel>();
                ft.WorldId = worldId;
                ft.XPos = bp.x;
                ft.YPos = bp.y;
            }

            return mapNode;
        }

        public static GameObject CreateMinimap(Transform parent, List<HoloMapBlueprint> blueprint, string prefix, string worldId, int posX, int posY)
        {
            GameObject minimapContainer = new("minimapContainer");
            minimapContainer.transform.SetParent(parent, false);
            minimapContainer.transform.localPosition = new(0f, 0f, 0f);

            GameObject minimap = new("minimap");
            minimap.transform.SetParent(minimapContainer.transform, false);
            minimap.transform.localPosition = new Vector3(2.5f, 0.5f, -2.25f);
            //minimap.transform.rotation = Quaternion.LookRotation(-Vector3.up);

            Minimap map = minimap.AddComponent<Minimap>();
            map.xPosition = posX;
            map.yPosition = posY;
            map.mapPrefix = prefix;
            map.minimapNodes = blueprint.ToDictionary(bp => bp.KeyCode, bp => BuildGameObject(bp, blueprint, minimap.transform, worldId, posX, posY));
            map.bossRooms = blueprint.Where(bp => bp.opponent != Opponent.Type.Default || bp.specialTerrain == HoloMapBlueprint.LOWER_TOWER_ROOM).Select(bp => bp.KeyCode).ToArray();
            map.bossRoomTrigger = blueprint.Where(bp => bp.blockedDirections != 0).Select(bp => bp.KeyCode).ToArray();
            map.nodeKeys = map.minimapNodes.Keys.ToArray();

            // Get all of the special rooms (the battles and trades)
            List<HoloMapBlueprint> specialTriggers = blueprint.Where(bp => bp.specialDirection != 0).ToList();
            map.encounterVisibilityRooms = specialTriggers.Select(bp => bp.KeyCode).ToArray();
            map.encounterRooms = specialTriggers.Select(bp => bp.GetAdjacentNode(blueprint, bp.specialDirection).KeyCode).ToArray();

            // Create the lines
            Dictionary<string, bool> connections = new();
            foreach (var node in blueprint)
            {
                foreach (var adj in AdjacentTo(blueprint, node))
                {
                    string adjId = $"{node.KeyCode}{adj.KeyCode}";
                    string altAdjId = $"{adj.KeyCode}{node.KeyCode}";
                    if (!connections.ContainsKey(adjId))
                    {
                        GameObject line = GameObject.Instantiate(LinePseudoPrefab, minimap.transform, false);
                        Component.Destroy(line.GetComponent<HoloLineSegment>());

                        HoloLinePartialSegment seg = line.AddComponent<HoloLinePartialSegment>();
                        seg.point1 = map.minimapNodes[node.KeyCode].transform;
                        seg.point2 = map.minimapNodes[adj.KeyCode].transform;
                        seg.line = line.GetComponent<LineRenderer>();
                        seg.line.startWidth = 0.022f;

                        connections.Add(adjId, true);
                        connections.Add(altAdjId, true);
                    }
                }
            }

            return minimap;
        }

        private void Recolor(GameObject obj, Color color)
        {
            MaterialHelper.RecolorAllMaterials(obj, color, _shaderKey, true);
            obj.tag = "HoloMapFixedColor"; // This is used to make sure the colors aren't overwritten.

            foreach (Transform t in obj.transform)
                t.gameObject.tag = "HoloMapFixedColor";
        }

        private bool HasVisitedRoom(string key)
        {
            if (key == $"{xPosition},{yPosition}")
                return true;

            var areaData = Part3SaveData.Data.areaData;
            var saveData = areaData == null ? null : Part3SaveData.Data.areaData.FirstOrDefault(a => a.id.StartsWith(mapPrefix) && a.id.EndsWith($"({key})"));

            return saveData != null && saveData.visited;
        }

        public bool ShowSpecialIcon(string key)
        {
            if (!encounterRooms.Contains(key))
                return false;

            if (HasVisitedRoom(key))
                return false;

            int index = Array.IndexOf(encounterRooms, key);
            return HasVisitedRoom(encounterVisibilityRooms[index]);
        }

        void OnEnable()
        {
            //P03Plugin.Log.LogInfo($"Enabled. Minimapnodes: {minimapNodes}");

            minimapNodes ??= nodeKeys.ToDictionary(k => k, k => this.gameObject.transform.Find($"minimapnode({k})").gameObject);

            string playerPosId = $"{xPosition},{yPosition}";

            bool hasSeenBossRoom = bossRoomTrigger.Any(k => HasVisitedRoom(k));

            foreach (var item in minimapNodes)
            {
                bool isBossRoom = bossRooms.Contains(item.Key);
                bool showSpecialIcon = ShowSpecialIcon(item.Key);
                bool shouldBeActive = item.Key == playerPosId || HasVisitedRoom(item.Key) || showSpecialIcon || (isBossRoom && hasSeenBossRoom);
                item.Value.SetActive(shouldBeActive);

                Transform iconTransform = item.Value.transform.Find("icon");
                GameObject iconObject = iconTransform?.gameObject;
                Transform skullTransform = item.Value.transform.Find("skull");
                GameObject skullObject = skullTransform?.gameObject;
                Transform tradeTransform = item.Value.transform.Find("trade");
                GameObject tradeObject = tradeTransform?.gameObject;
                GameObject ballObject = item.Value.transform.Find("ball").gameObject;

                var areaData = Part3SaveData.Data.areaData;
                var saveData = areaData == null ? null : Part3SaveData.Data.areaData.FirstOrDefault(a => a.id.StartsWith(mapPrefix) && a.id.EndsWith($"({item.Key})"));
                bool shouldShowBall = iconObject == null || ((saveData == null && item.Key != playerPosId) || (saveData != null && saveData.completedNodesIds.Where(i => i >= 10).Count() > 0));
                shouldShowBall = shouldShowBall && !showSpecialIcon;

                ballObject.SetActive(shouldShowBall);
                skullObject?.SetActive(showSpecialIcon);
                tradeObject?.SetActive(showSpecialIcon);
                iconObject?.SetActive(!shouldShowBall && !showSpecialIcon);

                item.Value.GetComponentInChildren<MainInputInteractable>()?.SetEnabled(HasVisitedRoom(item.Key));

                if (shouldBeActive)
                {
                    Color color = GameColors.instance.brightBlue;
                    if (isBossRoom || (showSpecialIcon && skullObject != null))
                        color = GameColors.instance.brightRed;
                    else if (showSpecialIcon && tradeObject != null)
                        color = GameColors.instance.brightSeafoam;
                    else if (item.Key == playerPosId)
                        color = GameColors.instance.brightGold;
                    Recolor(item.Value, color);
                }
            }
        }

        public static IEnumerable<HoloMapBlueprint> AdjacentTo(List<HoloMapBlueprint> map, HoloMapBlueprint node)
        {
            if (node.isSecretRoom)
                yield break;

            if ((node.arrowDirections & RunBasedHoloMap.NORTH) != 0)
            {
                var bp = map.First(b => b.x == node.x && b.y == node.y - 1);
                if (bp != null && !bp.isSecretRoom) yield return bp;
            }
            if ((node.arrowDirections & RunBasedHoloMap.SOUTH) != 0)
            {
                var bp = map.First(b => b.x == node.x && b.y == node.y + 1);
                if (bp != null && !bp.isSecretRoom) yield return bp;
            }
            if ((node.arrowDirections & RunBasedHoloMap.EAST) != 0)
            {
                var bp = map.First(b => b.x == node.x + 1 && b.y == node.y);
                if (bp != null && !bp.isSecretRoom) yield return bp;
            }
            if ((node.arrowDirections & RunBasedHoloMap.WEST) != 0)
            {
                var bp = map.First(b => b.x == node.x - 1 && b.y == node.y);
                if (bp != null && !bp.isSecretRoom) yield return bp;
            }
        }

        [SerializeField]
        private int xPosition;

        [SerializeField]
        private int yPosition;

        [SerializeField]
        private string mapPrefix;

        [SerializeField]
        private string[] nodeKeys;

        [SerializeField]
        private string[] bossRooms;

        [SerializeField]
        private string[] bossRoomTrigger;

        [SerializeField]
        private string[] encounterRooms;

        [SerializeField]
        private string[] encounterVisibilityRooms;

        private Dictionary<string, GameObject> minimapNodes;

        private string _shaderKey = SHADER_NAME;
        public string ShaderKey
        {
            get { return _shaderKey; }
            set
            {
                _shaderKey = value;
                OnEnable();
            }
        }

        public class HoloMinimapFastTravel : MainInputInteractable
        {
            public override CursorType CursorType => CursorType.Pickup;

            [SerializeField]
            public string WorldId;

            [SerializeField]
            public int XPos;

            [SerializeField]
            public int YPos;

            private Part3SaveData.WorldPosition Position => new(WorldId, XPos, YPos);

            public override void OnCursorSelectEnd()
            {
                P03Plugin.Log.LogDebug($"Moving to {Position.worldId} [{Position.gridX}, {Position.gridY}]");
                HoloMapAreaManager.Instance.StartCoroutine(FastTravelManagement.ReturnToLocation(Position));
            }
        }
    }


    // Modified version of HoloLineSegment specifically that enables itself when either end is active,
    // not just when both ends are active
    public class HoloLinePartialSegment : ManagedBehaviour
    {
        private void UpdateLine()
        {
            if (this.line != null && this.point1 != null && this.point2 != null && (this.point1.gameObject.activeInHierarchy || this.point2.gameObject.activeInHierarchy))
            {
                this.linePositions[0] = this.point1.position;
                this.linePositions[1] = CustomMath.MidPoint(this.point1.position, this.point2.position);
                this.linePositions[2] = this.point2.position;
                this.line.SetPositions(this.linePositions);
            }
            else
            {
                this.linePositions[0] = (this.linePositions[1] = (this.linePositions[2] = Vector3.zero));
                this.line.SetPositions(this.linePositions);
            }
        }

        public override void ManagedUpdate()
        {
            this.UpdateLine();
        }

        [SerializeField]
        internal LineRenderer line = null;

        [SerializeField]
        internal Transform point1 = null;

        [SerializeField]
        internal Transform point2 = null;

        internal Vector3[] linePositions = new Vector3[3];
    }
}