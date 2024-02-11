using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using TMPro;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Faces
{
    [HarmonyPatch]
    public class P03TrollFace : ManagedBehaviour
    {
        public static GameObject P03TrollFaceObject { get; private set; }

        // public static P03TrollFace Instance { get; private set; }

        public static readonly P03AnimationController.Face ID = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "P03TrollFace");

        [HarmonyPatch(typeof(P03AnimationController), "Start")]
        [HarmonyPostfix]
        public static void CreateTrollFace(ref P03AnimationController __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            // Clone the default face
            P03FaceRenderer renderer = __instance.gameObject.GetComponentInChildren<P03FaceRenderer>();
            P03TrollFaceObject = Instantiate(renderer.faceObjects[(int)P03AnimationController.Face.Angry], renderer.faceObjects[(int)P03AnimationController.Face.Angry].transform.parent);
            P03TrollFaceObject.name = "Face_Troll";

            Sprite trollSprite = Sprite.Create(TextureHelper.GetImageAsTexture("P03face_troll.png", typeof(P03TrollFace).Assembly), new Rect(0f, 0f, 90f, 65f), new Vector2(0.5f, 0.5f));
            SpriteRenderer spriteRenderer = P03TrollFaceObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = trollSprite;

            P03TrollFaceObject.SetActive(false);
        }

        [HarmonyPatch(typeof(P03FaceRenderer), "DisplayFace")]
        [HarmonyPrefix]
        public static bool DisplayLivesFace(ref GameObject __result, P03AnimationController.Face face, P03FaceRenderer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            P03TrollFaceObject.SetActive(false);
            if ((int)face == (int)ID)
            {
                foreach (GameObject f in __instance.faceObjects)
                    f.SetActive(false);

                P03TrollFaceObject.SetActive(true);
                __result = P03TrollFaceObject;
                return false;
            }
            return true;
        }
    }
}
