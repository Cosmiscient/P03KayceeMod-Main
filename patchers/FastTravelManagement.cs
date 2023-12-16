using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class FastTravelManagement
    {
        private static readonly Gradient ActiveLine;
        private static readonly Gradient InactiveLine;

        static FastTravelManagement()
        {
            ActiveLine = new Gradient();
            ActiveLine.SetKeys(new GradientColorKey[] {
                new (GameColors.Instance.blue, 0f),
                new (GameColors.Instance.brightBlue, 0.5f),
                new (GameColors.Instance.blue, 1f),
            },
            new GradientAlphaKey[] {
                new (0f, 0f),
                new (1f, 0.5f),
                new (0f, 1f),
            });

            InactiveLine = new Gradient();
            InactiveLine.SetKeys(new GradientColorKey[] {
                new (GameColors.Instance.darkBlue, 0f),
                new (GameColors.Instance.blue, 0.5f),
                new (GameColors.Instance.darkBlue, 1f),
            },
            new GradientAlphaKey[] {
                new (0f, 0f),
                new (1f, 0.5f),
                new (0f, 1f),
            });
        }

        [HarmonyPatch(typeof(HoloMapWaypointNode), "OnEnable")]
        [HarmonyPostfix]
        private static void AlwaysDisableHintUI(ref HoloMapWaypointNode __instance)
        {
            if (SaveFile.IsAscension)
                __instance.fastTravelHint.gameObject.SetActive(false);
        }

        private static List<RunBasedHoloMap.Zone> ForcedOrderZones
        {
            get
            {
                string forcedZoneKey = P03AscensionSaveData.RunStateData.GetValue(P03Plugin.PluginGuid, "ForcedOrderZones");
                if (string.IsNullOrEmpty(forcedZoneKey))
                {
                    List<RunBasedHoloMap.Zone> newZones = new() { RunBasedHoloMap.Zone.Magic, RunBasedHoloMap.Zone.Nature, RunBasedHoloMap.Zone.Tech, RunBasedHoloMap.Zone.Undead };
                    int randomSeed = P03AscensionSaveData.RandomSeed;
                    newZones = newZones.OrderBy(z => SeededRandom.Value(randomSeed++) * 100f).ToList();
                    forcedZoneKey = String.Join(",", newZones.Select(z => z.ToString()));
                    P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "ForcedOrderZones", forcedZoneKey);
                    return newZones;
                }
                return forcedZoneKey.Split(',').Select(k => (RunBasedHoloMap.Zone)Enum.Parse(typeof(RunBasedHoloMap.Zone), k)).ToList();
            }
        }

        private static readonly List<string> AlwaysOffObjects = new()
        {
            "LineEnd_StartArea",
            "LineEnd_WizardTemple",
            "LineEnd_UndeadTemple"
        };

        private static readonly List<string> SpecialBridgeNodes = new()
        {
            "FastTravelMapNode_EastPath",
            "FastTravelMapNode_WestPath"
        };

        private static readonly Dictionary<string, List<RunBasedHoloMap.Zone>> LinesIndicators = new()
        {
            { "Line", new() { RunBasedHoloMap.Zone.Nature, RunBasedHoloMap.Zone.Undead }},
            { "Line (1)", new() { RunBasedHoloMap.Zone.Nature }},
            { "Line (3)", new() { RunBasedHoloMap.Zone.Undead }},
            { "Line (5)", new() { RunBasedHoloMap.Zone.Tech, RunBasedHoloMap.Zone.Magic }},
            { "Line (7)", new() { RunBasedHoloMap.Zone.Tech }},
            { "Line (8)", new() { RunBasedHoloMap.Zone.Magic }},
        };

        private static readonly Dictionary<string, RunBasedHoloMap.Zone> fastTravelNodes = new()
        {
            { "FastTravelMapNode_Wizard", RunBasedHoloMap.Zone.Magic },
            { "FastTravelMapNode_Undead", RunBasedHoloMap.Zone.Undead },
            { "FastTravelMapNode_Nature", RunBasedHoloMap.Zone.Nature },
            { "FastTravelMapNode_Tech", RunBasedHoloMap.Zone.Tech },
            { "FastTravelMapNode_NorthPath", RunBasedHoloMap.Zone.Neutral }
        };

        [HarmonyPatch(typeof(FastTravelNode), "OnCursorSelectEnd")]
        [HarmonyPrefix]
        public static bool FastTravelInAscensionMode(ref FastTravelNode __instance)
        {
            // In ascension mode, fast travel is different
            // We will NOT fast travel to the world owned by the fast travel node
            // Instead, we will dynamically create a world based on that node
            if (SaveFile.IsAscension)
            {
                if (fastTravelNodes[__instance.gameObject.name] == EventManagement.CurrentZone)
                {
                    HoloGameMap.Instance.ToggleFastTravelActive(false, false);
                    return false;
                }

                EventManagement.AddVisitedZone(__instance.gameObject.name);

                __instance.SetHoveringEffectsShown(false);
                __instance.OnSelected();

                HoloGameMap.Instance.ToggleFastTravelActive(false, false);
                HoloMapAreaManager.Instance.CurrentArea.OnAreaActive();
                HoloMapAreaManager.Instance.CurrentArea.OnAreaEnabled();

                string worldId = RunBasedHoloMap.GetAscensionWorldID(fastTravelNodes[__instance.gameObject.name]);
                Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(fastTravelNodes[__instance.gameObject.name]);
                Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

                HoloMapAreaManager.Instance.StartCoroutine(HoloMapAreaManager.Instance.DroneFlyToArea(worldPosition, false));
                Part3SaveData.Data.checkpointPos = worldPosition;

                return false;
            }

            return true;
        }

        private static bool HasCompleted(RunBasedHoloMap.Zone zone) => HasCompleted(fastTravelNodes.First(kvp => kvp.Value == zone).Key);

        private static bool CanVisit(RunBasedHoloMap.Zone zone) => CanVisit(fastTravelNodes.First(kvp => kvp.Value == zone).Key);

        private static bool HasCompleted(string mapKey) => fastTravelNodes.Keys.Contains(mapKey) && EventManagement.CompletedZones.Contains(mapKey) && fastTravelNodes[mapKey] != EventManagement.CurrentZone;

        private static bool CanVisit(string mapKey)
        {
            bool canBeActive = fastTravelNodes.Keys.Contains(mapKey);

            if (canBeActive && AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BROKEN_BRIDGE) && fastTravelNodes[mapKey] != RunBasedHoloMap.Zone.Neutral)
            {
                if (!ForcedOrderZones.Take(Mathf.Min(4, EventManagement.CompletedZones.Count + 2)).Contains(fastTravelNodes[mapKey]))
                    canBeActive = false;
            }
            return canBeActive;
        }

        [HarmonyPatch(typeof(HoloFastTravelMap), nameof(HoloFastTravelMap.SetMapActive))]
        [HarmonyPostfix]
        private static void EnsureNodesDisabled(HoloFastTravelMap __instance)
        {
            if (SaveFile.IsAscension)
                __instance.nodes.ForEach(f => f.OnFastTravelActive());
        }

        [HarmonyPatch(typeof(FastTravelNode), nameof(FastTravelNode.IsCurrentWorld), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool CalculateDifference(FastTravelNode __instance, ref bool __result)
        {
            if (SaveFile.IsAscension)
            {
                __result = fastTravelNodes.Keys.Contains(__instance.gameObject.name) && fastTravelNodes[__instance.gameObject.name] == EventManagement.CurrentZone;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FastTravelNode), nameof(FastTravelNode.OnFastTravelActive))]
        [HarmonyPostfix]
        private static void SetFastTravelNodeActive(FastTravelNode __instance)
        {
            if (SaveFile.IsAscension)
            {
                if (SpecialBridgeNodes.Contains(__instance.gameObject.name))
                {
                    __instance.gameObject.SetActive(true);
                    __instance.transform.Find("Anim").gameObject.transform.localScale = new(0f, 0f, 0f);
                    __instance.GetComponentInChildren<BoxCollider>().enabled = false;
                    return;
                }

                if (!fastTravelNodes.Keys.Contains(__instance.gameObject.name))
                {
                    __instance.gameObject.SetActive(false);
                    return;
                }

                if (HasCompleted(__instance.gameObject.name))
                {
                    __instance.gameObject.SetActive(false);
                    return;
                }

                __instance.gameObject.SetActive(true);
                __instance.GetComponentInChildren<BoxCollider>().enabled = CanVisit(__instance.gameObject.name);

                Color c = CanVisit(__instance.gameObject.name) ? GameColors.Instance.blue : GameColors.Instance.red;
                Renderer render = __instance.transform.Find("Anim").GetComponentInChildren<Renderer>();
                render.material.SetColor("_MainColor", c);
                render.material.SetColor("_RimColor", c);
            }
        }

        [HarmonyPatch(typeof(HoloMapWaypointNode), nameof(HoloMapWaypointNode.OnCursorSelectEnd))]
        [HarmonyPrefix]
        private static void SetFastTravelNodesVisible()
        {
            if (SaveFile.IsAscension)
            {
                Transform parent = HoloGameMap.Instance.fastTravelMap.gameObject.transform;

                // Always turn these off
                foreach (string key in AlwaysOffObjects)
                    parent.Find(key).gameObject.SetActive(false);


                // Modify these based on visibility
                Transform lineParent = parent.Find("Lines");
                foreach (Transform line in lineParent)
                {
                    if (!LinesIndicators.ContainsKey(line.gameObject.name))
                    {
                        line.gameObject.SetActive(false);
                        continue;
                    }

                    bool hasVisited = LinesIndicators[line.gameObject.name].All(z => HasCompleted(z));
                    bool canVisit = LinesIndicators[line.gameObject.name].Any(z => CanVisit(z));

                    line.gameObject.SetActive(canVisit);
                    line.GetComponentInChildren<HoloLineSegment>().line.colorGradient = hasVisited ? InactiveLine : ActiveLine;
                }
            }
        }

        private static readonly Dictionary<KeyCode, string> DIRECTIONS = new()
        {
            { KeyCode.K, "S"}, { KeyCode.I, "N"},{ KeyCode.J, "W"}, { KeyCode.L, "E"}
        };

        [HarmonyPatch(typeof(ViewController), nameof(ViewController.ManagedLateUpdate))]
        [HarmonyPrefix]
        private static bool MoveMap()
        {
            if (ViewManager.Instance.CurrentView != View.MapDefault)
                return true;

            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.L))
            {
                if (HoloMapAreaManager.Instance.MovingAreas || MapNodeManager.Instance.MovingNodes)
                    return true;

                HoloMapArea currentmap = HoloMapAreaManager.Instance.CurrentArea;
                if (currentmap == null)
                    return true;

                Transform arrowsParent = currentmap.transform.Find("Nodes");
                if (arrowsParent == null)
                    return true;

                foreach (KeyValuePair<KeyCode, string> move in DIRECTIONS)
                {
                    if (Input.GetKey(move.Key))
                    {
                        Transform nodeTransform = arrowsParent.Find($"MoveArea_{move.Value}");
                        if (move.Value.Equals("W") && nodeTransform == null)
                            nodeTransform = arrowsParent.Find($"MoveArea_W (NORTH)");
                        if (nodeTransform == null || !nodeTransform.gameObject.activeSelf)
                            return true;

                        MoveHoloMapAreaNode node = nodeTransform.GetComponent<MoveHoloMapAreaNode>();
                        if (node.Secret || !node.Enabled)
                            return true;

                        node.CursorSelectEnd();
                        return false;
                    }
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    List<HoloMapNode> nodes = currentmap.GetComponentsInChildren<HoloMapNode>().Where(hn => hn is not MoveHoloMapAreaNode).ToList();
                    if (nodes.Count == 1)
                    {
                        if (nodes[0].Secret || !nodes[0].Enabled)
                            return true;

                        nodes[0].CursorSelectEnd();
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool isDroneFlying = false;

        [HarmonyPatch(typeof(HoloMapAreaManager), "DroneFlyToArea")]
        private static class ManageDroneFlying
        {
            [HarmonyPrefix]
            private static void SetDroneFlying()
            {
                P03Plugin.Log.LogInfo("Drone flying = true");
                isDroneFlying = true;
            }

            [HarmonyPostfix]
            private static IEnumerator SetDroneNotFlying(IEnumerator sequence)
            {
                P03Plugin.Log.LogInfo("Drone flying = false");
                yield return sequence;
                isDroneFlying = false;
            }
        }

        [HarmonyPatch(typeof(HoloGameMap), nameof(HoloGameMap.UpdateColors))]
        [HarmonyPrefix]
        private static bool ManuallySetMapColorsIfDroneFlying(ref HoloGameMap __instance)
        {
            if (SaveFile.IsAscension && isDroneFlying)
            {
                P03Plugin.Log.LogInfo("Setting map colors after drone flight");
                HoloMapArea currentArea = HoloMapAreaManager.Instance.CurrentArea;
                __instance.SetSceneColors(currentArea.MainColor, currentArea.LightColor);
                __instance.SetSceneryColors(GameColors.Instance.blue, GameColors.Instance.gold);
                __instance.SetNodeColors(GameColors.Instance.darkBlue, GameColors.Instance.brightBlue);
                return false;
            }
            return true;
        }

        public static IEnumerator ReturnToLocation(Part3SaveData.WorldPosition worldPosition)
        {
            // We do our own special sequence when you complete a boss
            // ... we just play the drone and move you back to the hub world.

            yield return new WaitUntil(() => HoloMapAreaManager.Instance.CurrentArea != null);
            HoloGameMap.Instance.ToggleFastTravelActive(false, false);
            HoloMapAreaManager.Instance.CurrentArea.OnAreaActive();
            HoloMapAreaManager.Instance.CurrentArea.OnAreaEnabled();

            HoloMapAreaManager.Instance.StartCoroutine(HoloMapAreaManager.Instance.DroneFlyToArea(worldPosition, false));
            Part3SaveData.Data.checkpointPos = worldPosition;

            yield return new WaitForSeconds(1.75f);

            SaveManager.SaveToFile();
        }

        public static IEnumerator ReturnToHomeBase()
        {
            string worldId = RunBasedHoloMap.GetAscensionWorldID(RunBasedHoloMap.Zone.Neutral);
            Tuple<int, int> pos = RunBasedHoloMap.GetStartingSpace(RunBasedHoloMap.Zone.Neutral);
            Part3SaveData.WorldPosition worldPosition = new(worldId, pos.Item1, pos.Item2);

            yield return ReturnToLocation(worldPosition);
        }

        [HarmonyPatch(typeof(MoveHoloMapAreaNode), nameof(MoveHoloMapAreaNode.WillResetBots), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool NeverResetBotsOnMoveInAscension(ref bool __result)
        {
            if (SaveFile.IsAscension)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}