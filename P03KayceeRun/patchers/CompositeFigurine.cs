using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Guid;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class CompositeFigurineManager
    {
        public class FullFigurineData
        {
            public FullFigurineData()
            {
                Type = CompositeFigurine.FigurineType.Chief;
            }

            public FullFigurineData(CompositeFigurine.FigurineType type)
            {
                Type = type;
            }

            public FullFigurineData(CompositeFigurine.FigurineData baseData)
            {
                Type = baseData.type;
                Head = new()
                {
                    material = baseData.material,
                    mesh = baseData.headMesh,
                    localPosition = baseData.pivotOffset,
                    localScale = 100 * Vector3.one
                };
                Body = new()
                {
                    material = baseData.material,
                    mesh = baseData.bodyMesh,
                    localPosition = baseData.pivotOffset,
                    localScale = 100 * Vector3.one
                };
                Arms = new()
                {
                    material = baseData.material,
                    mesh = baseData.armsMesh,
                    localPosition = baseData.pivotOffset,
                    localScale = 100 * Vector3.one
                };
            }

            public class ComponentData
            {
                public CompositeFigurine.FigurineType? CopyFrom;

                public Material material;
                public Mesh mesh;
                public GameObject prefab;
                public Color? recolor;
                public Vector3 localPosition = Vector3.zero;
                public Vector3 localScale = Vector3.one;
                public Vector3 localEulerAngles = Vector3.zero;
            }

            public CompositeFigurine.FigurineType Type { get; internal set; }

            public ComponentData Head { get; set; }
            public ComponentData Arms { get; set; }
            public ComponentData Body { get; set; }
        }

        public static CompositeFigurine.FigurineType PikeMage { get; private set; }
        public static CompositeFigurine.FigurineType Kaycee { get; private set; }
        public static CompositeFigurine.FigurineType TrainingDummy { get; private set; }
        public static CompositeFigurine.FigurineType Inspector { get; private set; }
        public static CompositeFigurine.FigurineType None { get; private set; }

        private static readonly List<FullFigurineData> newFigurines = new();
        private static readonly List<FullFigurineData> baseFigurines = new();

        public static CompositeFigurine.FigurineType Add(string pluginGuid, string name, CompositeFigurine.FigurineData data)
        {
            CompositeFigurine.FigurineType newType = GuidManager.GetEnumValue<CompositeFigurine.FigurineType>(pluginGuid, name);
            data.type = newType;
            newFigurines.Add(new FullFigurineData(data));
            return newType;
        }

        public static CompositeFigurine.FigurineType Add(string pluginGuid, string name, FullFigurineData data)
        {
            CompositeFigurine.FigurineType newType = GuidManager.GetEnumValue<CompositeFigurine.FigurineType>(pluginGuid, name);
            data.Type = newType;
            newFigurines.Add(data);
            return newType;
        }

        public static List<FullFigurineData> AllFigurines => baseFigurines.Concat(newFigurines).ToList();

        public static FullFigurineData Get(CompositeFigurine.FigurineType type)
        {
            FullFigurineData baseData = AllFigurines.FirstOrDefault(f => f.Type == type);
            if (baseData == null)
                return null;

            FullFigurineData retval = new() { Type = type, Arms = baseData.Arms, Head = baseData.Head, Body = baseData.Body };
            while (retval.Arms.CopyFrom.HasValue)
                retval.Arms = AllFigurines.FirstOrDefault(f => f.Type == retval.Arms.CopyFrom)?.Arms;
            while (retval.Head.CopyFrom.HasValue)
                retval.Head = AllFigurines.FirstOrDefault(f => f.Type == retval.Head.CopyFrom)?.Head;
            while (retval.Body.CopyFrom.HasValue)
                retval.Body = AllFigurines.FirstOrDefault(f => f.Type == retval.Body.CopyFrom)?.Body;

            return retval;
        }

        static CompositeFigurineManager()
        {
            GameObject npcBase = ResourceBank.Get<GameObject>("prefabs/map/holoplayermarker");
            CompositeFigurine figure = npcBase.GetComponentInChildren<CompositeFigurine>();

            // Initialize all the base game data
            foreach (var data in figure.figurines)
                baseFigurines.Add(new(data));

            // Add the "nothing"
            FullFigurineData nothingData = new()
            {
                Arms = new(),
                Head = new(),
                Body = new()
            };
            None = Add(P03Plugin.PluginGuid, "NoFigure", nothingData);

            // Add the pikemage
            FullFigurineData spearData = new();
            spearData.Head = new() { CopyFrom = CompositeFigurine.FigurineType.Enchantress };

            var swordBladeParent = new GameObject("SwordBlade");
            swordBladeParent.transform.localPosition = Vector3.zero;
            var swordBlade = GameObject.Instantiate(RunBasedHoloMap.GetGameObject("WizardMainPath_5/Scenery/HoloMapNPC/HoloSword (3)/blade"), swordBladeParent.transform);
            swordBlade.transform.localPosition = new Vector3(0f, 0.935f - 0.2418f, 0f);
            swordBlade.transform.localScale = new Vector3(0.75f, 0.3572f, 2.41f);
            GameObject.DontDestroyOnLoad(swordBladeParent);
            spearData.Arms = new()
            {
                prefab = swordBladeParent
            };
            // GameObject.DontDestroyOnLoad(spearData.Body.);

            var swordHiltParent = new GameObject("SwordHilt");
            swordHiltParent.transform.localPosition = Vector3.zero;
            var swordHilt = GameObject.Instantiate(RunBasedHoloMap.GetGameObject("WizardMainPath_5/Scenery/HoloMapNPC/HoloSword (3)/handle"), swordHiltParent.transform);
            MaterialHelper.HolofyAllRenderers(swordHilt, GameColors.Instance.blue, brightness: 1);
            swordHilt.transform.localPosition = new Vector3(0f, -0.1068f, 0f);
            swordHilt.transform.localScale = new Vector3(1.15f, 3.1572f, 1.91f);
            swordHilt.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            GameObject.DontDestroyOnLoad(swordHiltParent);
            spearData.Body = new()
            {
                prefab = swordHiltParent,
                recolor = GameColors.Instance.blue
            };
            // GameObject.DontDestroyOnLoad(spearData.Arms.mesh);

            PikeMage = Add(P03Plugin.PluginGuid, "Spear", spearData);
            P03Plugin.Log.LogInfo("Created pikemage figure");

            // Add Inspector
            FullFigurineData inspectorData = new();
            inspectorData.Head = new() { CopyFrom = CompositeFigurine.FigurineType.Robot };
            inspectorData.Arms = new() { CopyFrom = CompositeFigurine.FigurineType.Robot };
            var inspectorBodyParent = new GameObject("InspectorBody");
            inspectorBodyParent.transform.localPosition = Vector3.zero;
            var inspectorBody = GameObject.Instantiate(RunBasedHoloMap.GetGameObject("NeutralWestTechGate/Scenery/HoloMapNPC/Body"), inspectorBodyParent.transform);
            inspectorBody.transform.localPosition = new(0.0093f, 0.345f, 0.1066f);
            inspectorBody.transform.localScale = new(0.44f, 0.695f, 0.59f);
            MaterialHelper.HolofyAllRenderers(inspectorBody, GameColors.Instance.blue, brightness: 1);
            GameObject.DontDestroyOnLoad(inspectorBodyParent);
            inspectorData.Body = new() { prefab = inspectorBodyParent };
            Inspector = Add(P03Plugin.PluginGuid, "Inspector", inspectorData);
            P03Plugin.Log.LogInfo("Created Inspector figure");

            // Add Kaycee
            FullFigurineData kayceeData = new();
            kayceeData.Body = new() { CopyFrom = CompositeFigurine.FigurineType.Gravedigger };
            kayceeData.Arms = new() { CopyFrom = CompositeFigurine.FigurineType.Wildling };

            var kayceeIceParent = new GameObject("KayceeIce");
            kayceeIceParent.transform.localPosition = Vector3.zero;
            var ice = GameObject.Instantiate(RunBasedHoloMap.GetGameObject("UndeadMainPath_2/Scenery/HoloMapNPC/Head/Ice"), kayceeIceParent.transform);
            ice.transform.localPosition = new Vector3(-0.0189f, 0.7379f, 0.0795f);
            ice.transform.localScale = new Vector3(0.005f, 0.006f, .005f);
            ice.transform.localEulerAngles = new Vector3(5.0957f, 293.2509f, 28.965f);
            MaterialHelper.HolofyAllRenderers(ice, GameColors.Instance.brightBlue, brightness: 1);
            GameObject.DontDestroyOnLoad(kayceeIceParent);
            var gravehead = Get(CompositeFigurine.FigurineType.Gravedigger);
            kayceeData.Head = new()
            {
                mesh = gravehead.Head.mesh,
                material = gravehead.Head.material,
                localPosition = gravehead.Head.localPosition,
                localScale = new(100f, 100f, 100f),
                prefab = kayceeIceParent
            };

            Kaycee = Add(P03Plugin.PluginGuid, "Kaycee", kayceeData);
            P03Plugin.Log.LogInfo("Created kaycee figure");

            // Training Dummy
            var fullDummy = GameObject.Instantiate(AssetBundleManager.Prefabs["BotopiaDummy"]);
            FullFigurineData dummyData = new();
            dummyData.Head = new() { prefab = GameObject.Instantiate(fullDummy.transform.Find("Head").gameObject) };
            dummyData.Body = new() { prefab = GameObject.Instantiate(fullDummy.transform.Find("Body").gameObject) };
            dummyData.Arms = new() { prefab = GameObject.Instantiate(fullDummy.transform.Find("Arms").gameObject) };
            MaterialHelper.HolofyAllRenderers(dummyData.Head.prefab, GameColors.Instance.blue, brightness: 1);
            MaterialHelper.HolofyAllRenderers(dummyData.Arms.prefab, GameColors.Instance.blue, brightness: 1);
            MaterialHelper.HolofyAllRenderers(dummyData.Body.prefab, GameColors.Instance.blue, brightness: 1);
            GameObject.DontDestroyOnLoad(dummyData.Head.prefab);
            GameObject.DontDestroyOnLoad(dummyData.Arms.prefab);
            GameObject.DontDestroyOnLoad(dummyData.Body.prefab);
            TrainingDummy = Add(P03Plugin.PluginGuid, "Dummy", dummyData);
            P03Plugin.Log.LogInfo("Created training dummy figure");
        }

        [HarmonyPatch(typeof(CompositeFigurine), nameof(CompositeFigurine.SetArms))]
        [HarmonyPrefix]
        private static bool CustomArmsGeneration(CompositeFigurine __instance, CompositeFigurine.FigurineType armsType)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                if (!baseFigurines.Any(f => f.Type == armsType))
                {
                    __instance.SetArms(baseFigurines[0].Type);
                    return false;
                }
                return true;
            }

            var armsData = Get(armsType).Arms;

            var tChild = __instance.armsRenderer.transform.parent.Find("CustomArms");
            if (tChild != null)
                GameObject.Destroy(tChild.gameObject);
            __instance.armsRenderer.gameObject.SetActive(armsData.mesh != null);
            if (armsData.mesh != null)
            {
                __instance.armsRenderer.material = armsData.material;
                __instance.armsRenderer.transform.localPosition = armsData.localPosition;
                __instance.armsRenderer.transform.localScale = armsData.localScale;
                __instance.armsRenderer.transform.localEulerAngles = armsData.localEulerAngles;
                __instance.armsFilter.mesh = armsData.mesh;

                if (armsData.recolor.HasValue && armsData.prefab == null)
                    MaterialHelper.HolofyAllRenderers(__instance.armsRenderer.gameObject, armsData.recolor.Value, brightness: 1);
            }
            if (armsData.prefab != null)
            {
                var customarms = GameObject.Instantiate(armsData.prefab, __instance.armsRenderer.transform.parent);
                customarms.name = "CustomArms";

                if (armsData.recolor.HasValue)
                    MaterialHelper.HolofyAllRenderers(customarms, armsData.recolor.Value, brightness: 1);
            }

            return false;
        }

        [HarmonyPatch(typeof(CompositeFigurine), nameof(CompositeFigurine.Generate))]
        [HarmonyPrefix]
        private static bool CustomFigureGeneration(CompositeFigurine __instance, CompositeFigurine.FigurineType headType, CompositeFigurine.FigurineType armsType, CompositeFigurine.FigurineType bodyType)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                if (!baseFigurines.Any(f => f.Type == headType))
                {
                    __instance.Generate(baseFigurines[0].Type, bodyType, armsType);
                    return false;
                }
                if (!baseFigurines.Any(f => f.Type == bodyType))
                {
                    __instance.Generate(headType, baseFigurines[0].Type, armsType);
                    return false;
                }
                if (!baseFigurines.Any(f => f.Type == armsType))
                {
                    __instance.Generate(headType, bodyType, baseFigurines[0].Type);
                    return false;
                }
                return true;
            }

            __instance.SetArms(armsType);

            var headData = Get(headType).Head;

            var tChild = __instance.headRenderer.transform.parent.Find("Customhead");
            if (tChild != null)
                GameObject.Destroy(tChild.gameObject);
            __instance.headRenderer.gameObject.SetActive(headData.mesh != null);
            if (headData.mesh != null)
            {
                __instance.headRenderer.material = headData.material;
                __instance.headRenderer.transform.localPosition = headData.localPosition;
                __instance.headRenderer.transform.localScale = headData.localScale;
                __instance.headRenderer.transform.localEulerAngles = headData.localEulerAngles;
                __instance.headFilter.mesh = headData.mesh;

                if (headData.recolor.HasValue && headData.prefab == null)
                    MaterialHelper.HolofyAllRenderers(__instance.headRenderer.gameObject, headData.recolor.Value, brightness: 1);
            }
            if (headData.prefab != null)
            {
                var customhead = GameObject.Instantiate(headData.prefab, __instance.headRenderer.transform.parent);
                customhead.name = "Customhead";

                if (headData.recolor.HasValue)
                    MaterialHelper.HolofyAllRenderers(customhead, headData.recolor.Value, brightness: 1);
            }

            var bodyData = Get(bodyType).Body;

            var bChild = __instance.bodyRenderer.transform.parent.Find("Custombody");
            if (bChild != null)
                GameObject.Destroy(bChild.gameObject);
            __instance.bodyRenderer.gameObject.SetActive(bodyData.mesh != null);
            if (bodyData.mesh != null)
            {
                __instance.bodyRenderer.material = bodyData.material;
                __instance.bodyRenderer.transform.localPosition = bodyData.localPosition;
                __instance.bodyRenderer.transform.localScale = bodyData.localScale;
                __instance.bodyRenderer.transform.localEulerAngles = bodyData.localEulerAngles;
                __instance.bodyFilter.mesh = bodyData.mesh;

                if (bodyData.recolor.HasValue && bodyData.prefab == null)
                    MaterialHelper.HolofyAllRenderers(__instance.bodyRenderer.gameObject, bodyData.recolor.Value, brightness: 1);
            }
            if (bodyData.prefab != null)
            {
                var custombody = GameObject.Instantiate(bodyData.prefab, __instance.bodyRenderer.transform.parent);
                custombody.name = "Custombody";

                if (bodyData.recolor.HasValue)
                    MaterialHelper.HolofyAllRenderers(custombody, bodyData.recolor.Value, brightness: 1);
            }

            __instance.SetMaterialParams();

            return false;
        }

        public static void RotateArms(bool forward)
        {
            var figData = P03AscensionSaveData.IsP03Run ? AllFigurines : baseFigurines;
            var all = figData.Where(f => !f.Arms.CopyFrom.HasValue).Select(f => f.Type).ToList();
            int current = all.IndexOf(AscensionSaveData.Data.playerAvatarArms);
            if (forward)
            {
                current += 1;
                if (current >= all.Count)
                    current = 0;
            }
            else
            {
                current -= 1;
                if (current < 0)
                    current = all.Count - 1;
            }
            PlayerMarker.Instance.GetComponentInChildren<CompositeFigurine>().Generate(
                AscensionSaveData.Data.playerAvatarHead,
                all[current],
                AscensionSaveData.Data.playerAvatarBody
            );
            AscensionSaveData.Data.playerAvatarArms = all[current];
        }

        public static void RotateBody(bool forward)
        {
            var figData = P03AscensionSaveData.IsP03Run ? AllFigurines : baseFigurines;
            var all = figData.Where(f => !f.Body.CopyFrom.HasValue).Select(f => f.Type).ToList();
            int current = all.IndexOf(AscensionSaveData.Data.playerAvatarBody);
            if (forward)
            {
                current += 1;
                if (current >= all.Count)
                    current = 0;
            }
            else
            {
                current -= 1;
                if (current < 0)
                    current = all.Count - 1;
            }
            PlayerMarker.Instance.GetComponentInChildren<CompositeFigurine>().Generate(
                AscensionSaveData.Data.playerAvatarHead,
                AscensionSaveData.Data.playerAvatarArms,
                all[current]
            );
            AscensionSaveData.Data.playerAvatarBody = all[current];
        }

        public static void RotateHead(bool forward)
        {
            var figData = P03AscensionSaveData.IsP03Run ? AllFigurines : baseFigurines;
            var all = figData.Where(f => !f.Head.CopyFrom.HasValue).Select(f => f.Type).ToList();
            int current = all.IndexOf(AscensionSaveData.Data.playerAvatarHead);
            if (forward)
            {
                current += 1;
                if (current >= all.Count)
                    current = 0;
            }
            else
            {
                current -= 1;
                if (current < 0)
                    current = all.Count - 1;
            }
            PlayerMarker.Instance.GetComponentInChildren<CompositeFigurine>().Generate(
                all[current],
                AscensionSaveData.Data.playerAvatarArms,
                AscensionSaveData.Data.playerAvatarBody
            );
            AscensionSaveData.Data.playerAvatarHead = all[current];
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.AvatarArms), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool FixArmsGetter(ref CompositeFigurine.FigurineType __result)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __result = P03AscensionSaveData.P03Data.playerAvatarArms;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.AvatarBody), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool FixBodyGetter(ref CompositeFigurine.FigurineType __result)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __result = P03AscensionSaveData.P03Data.playerAvatarBody;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.AvatarHead), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool FixHeadGetter(ref CompositeFigurine.FigurineType __result)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                __result = P03AscensionSaveData.P03Data.playerAvatarHead;
                return false;
            }
            return true;
        }
    }
}