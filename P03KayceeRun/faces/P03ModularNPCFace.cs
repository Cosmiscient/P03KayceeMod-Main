using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Faces
{
    [HarmonyPatch]
    public class P03ModularNPCFace : ManagedBehaviour
    {
        public enum FaceSet : int
        {
            Cyclops = 1,
            Rags = 2,
            Creepface = 3,
            BuildABot = 4,
            Wirehead = 5,
            Pipehead = 6,
            Fishhead = 7,
            Faceless = 8,
            Goggles = 9,
            Leapbot = 10,
            MrsBomb = 11,
            Quill = 12,
            Jimmy = 13,
            Steambot = 14,
            Pyromaniac = 15,
            BountyHunter = 16,
            Prospector = 17,
            PikeMageSolo = 18,
            InspectorSolo = 19,
            DummySolo = 20,
            DredgerSolo = 21,
            KayceeSolo = 22,
            LibrariansSolo = 23,
            TrapperSolo = 24,
            TraderSolo = 25,
            RebechaSolo = 26
        }

        public static GameObject NPCFaceObject { get; private set; }

        public static P03ModularNPCFace Instance { get; private set; }

        public static readonly P03AnimationController.Face ModularNPCFace = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "ModularNPCFace");

        private static readonly Sprite[,] NPC_SPRITES;

        private static readonly int NUMBER_OF_CHOICES = (int)System.Enum.GetValues(typeof(FaceSet)).Cast<FaceSet>().Max();

        private static readonly string[] LAYER_NAMES = new string[] { "bottom", "top", "face" };

        static P03ModularNPCFace()
        {
            NPC_SPRITES = new Sprite[LAYER_NAMES.Length, NUMBER_OF_CHOICES];
            for (int i = 0; i < LAYER_NAMES.Length; i++)
            {
                for (int j = 0; j < NUMBER_OF_CHOICES; j++)
                {
                    try
                    {
                        NPC_SPRITES[i, j] = TextureHelper.GetImageAsTexture($"npc {LAYER_NAMES[i]} {j + 1}.png", typeof(P03ModularNPCFace).Assembly).ConvertTexture();
                    }
                    catch
                    {
                        // I know 21 is blank. Fill missing with blank
                        NPC_SPRITES[i, j] = TextureHelper.GetImageAsTexture($"npc {LAYER_NAMES[i]} 21.png", typeof(P03ModularNPCFace).Assembly).ConvertTexture();
                    }
                }
            }
        }

        public void SetNPCFace(string faceCode)
        {
            indices = faceCode.Split(new char[] { '-' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();

            RenderNPCFace();
        }

        public void RenderNPCFace()
        {
            for (int i = 0; i < indices.Count; i++)
                renderers[i].sprite = NPC_SPRITES[i, indices[i] - 1];
        }

        public static string GeneratedNPCFaceCode(int randomSeed = -1)
        {
            if (randomSeed == -1)
                randomSeed = P03AscensionSaveData.RandomSeed;

            List<int> indices = new();
            for (int i = 0; i < LAYER_NAMES.Length; i++)
            {
                // This is a dumb hack but it'll work okay
                int n = SeededRandom.Range(1, NUMBER_OF_CHOICES + 1, randomSeed++);
                while (((FaceSet)n).ToString().EndsWith("Solo"))
                    n = SeededRandom.Range(1, NUMBER_OF_CHOICES + 1, randomSeed++);
                indices.Add(n);
            }

            return string.Join("-", indices);
        }

        [SerializeField]
        private List<int> indices;

        private static List<GameObject> _faces;

        [SerializeField]
        private List<SpriteRenderer> renderers;

        [HarmonyPatch(typeof(P03AnimationController), "Start")]
        [HarmonyPostfix]
        public static void CreateLivesFace(ref P03AnimationController __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            // Find all the faces
            P03FaceRenderer renderer = __instance.gameObject.GetComponentInChildren<P03FaceRenderer>();
            _faces = renderer.faceObjects;

            GameObject template = _faces[(int)P03AnimationController.Face.SpearWizard];

            // Clone the currency face
            NPCFaceObject = new GameObject("P03 Modular NPC Face");
            NPCFaceObject.transform.SetParent(template.transform.parent);
            NPCFaceObject.transform.localPosition = template.transform.localPosition;
            Instance = NPCFaceObject.AddComponent<P03ModularNPCFace>();

            // Create a block for each of the four layers            
            Instance.renderers = new();
            for (int i = 0; i < LAYER_NAMES.Length; i++)
            {
                GameObject layer = Instantiate(template, NPCFaceObject.transform);
                layer.name = $"NPCFaceLayer{i}";
                layer.transform.localPosition = new(0f, -0.5f, 0f);
                layer.transform.localScale = new(20f, 20f, 1f);
                layer.SetActive(true);
                SpriteRenderer sprite = layer.GetComponent<SpriteRenderer>();
                sprite.sortingOrder += 10 * i;
                Instance.renderers.Add(sprite);
            }

            NPCFaceObject.SetActive(false);
        }

        [HarmonyPatch(typeof(P03FaceRenderer), "DisplayFace")]
        [HarmonyPrefix]
        public static bool DisplayLivesFace(ref GameObject __result, P03AnimationController.Face face)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            NPCFaceObject.SetActive(false);
            if ((int)face == (int)ModularNPCFace)
            {
                foreach (GameObject f in _faces)
                    f.SetActive(false);

                NPCFaceObject.SetActive(true);
                __result = NPCFaceObject;
                return false;
            }
            return true;
        }
    }
}
