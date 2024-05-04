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
    public class P03BellFace : ManagedBehaviour
    {
        public static GameObject P03BellFaceObject { get; private set; }

        // public static P03BellFace Instance { get; private set; }

        public static readonly P03AnimationController.Face ID = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "P03BellFace");

        private static readonly Sprite BELL_A = Sprite.Create(TextureHelper.GetImageAsTexture("P03_face_bell_a.png", typeof(P03BellFace).Assembly), new Rect(0f, 0f, 73f, 60f), new Vector2(0.5f, 0.5f));
        private static readonly Sprite BELL_B = Sprite.Create(TextureHelper.GetImageAsTexture("P03_face_bell_b.png", typeof(P03BellFace).Assembly), new Rect(0f, 0f, 73f, 60f), new Vector2(0.5f, 0.5f));

        private SpriteRenderer renderer;

        private float currentTick = 0;
        private bool currentFrame = false;

        public override void ManagedUpdate()
        {
            currentTick += Time.deltaTime;
            while (currentTick > 0.5f)
            {
                currentTick -= 0.5f;
                currentFrame = !currentFrame;
            }
            this.renderer.sprite = currentFrame ? BELL_B : BELL_A;
        }

        [HarmonyPatch(typeof(P03AnimationController), "Start")]
        [HarmonyPostfix]
        public static void CreateTrollFace(ref P03AnimationController __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            // Clone the default face
            P03FaceRenderer renderer = __instance.gameObject.GetComponentInChildren<P03FaceRenderer>();
            P03BellFaceObject = Instantiate(renderer.faceObjects[(int)P03AnimationController.Face.Dredger], renderer.faceObjects[(int)P03AnimationController.Face.Dredger].transform.parent);
            P03BellFaceObject.name = "Face_Bell";

            SpriteRenderer spriteRenderer = P03BellFaceObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = BELL_A;

            P03BellFaceObject.AddComponent<P03BellFace>().renderer = spriteRenderer;

            P03BellFaceObject.SetActive(false);
        }

        [HarmonyPatch(typeof(P03FaceRenderer), "DisplayFace")]
        [HarmonyPrefix]
        public static bool DisplayLivesFace(ref GameObject __result, P03AnimationController.Face face, P03FaceRenderer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            P03BellFaceObject.SetActive(false);
            if ((int)face == (int)ID)
            {
                foreach (GameObject f in __instance.faceObjects)
                    f.SetActive(false);

                P03BellFaceObject.SetActive(true);
                __result = P03BellFaceObject;
                return false;
            }
            return true;
        }
    }
}
