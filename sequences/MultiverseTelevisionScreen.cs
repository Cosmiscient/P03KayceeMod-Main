using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class MultiverseTelevisionScreen : MainInputInteractable
    {
        internal static readonly View TVScreenView = GuidManager.GetEnumValue<View>(P03Plugin.PluginGuid, "TVScreenView");

        public int xIndex;
        public int yIndex;

        public const float DEFAULT_SCALE = 0.6f;

        public static readonly Vector3 DEFAULT_SCALE_VECTOR = new(DEFAULT_SCALE, DEFAULT_SCALE, DEFAULT_SCALE);

        private SpriteRenderer SpriteRenderer;
        public Renderer FrameRenderer;
        public Renderer ContentsRenderer;
        private Texture2D ScreenshotTexture;

        internal bool MoveForward = false;

        public override CursorType CursorType => CursorType.Place;

        public override void ManagedUpdate()
        {
            this.SetEnabled(TurnManager.Instance.IsPlayerMainPhase && !(MultiverseBattleSequencer.Instance?.MultiverseTravelLocked).GetValueOrDefault(true));
        }

        public override void OnCursorSelectStart()
        {
            if (TurnManager.Instance.IsPlayerMainPhase)
            {
                if (!(MultiverseBattleSequencer.Instance?.MultiverseTravelLocked).GetValueOrDefault(true))
                {
                    MultiverseBattleSequencer.Instance?.StartCoroutine(MultiverseBattleSequencer.Instance.TraverseMultiverse(MoveForward));
                }
            }
        }

        private static GameObject _screenshotCamera = null;
        private static GameObject ScreenshotCamera
        {
            get
            {
                if (_screenshotCamera == null)
                {
                    _screenshotCamera = new("ScreenshotCamera");
                    Camera playerCamera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();

                    Camera newCam = _screenshotCamera.AddComponent<Camera>();
                    newCam.CopyFrom(playerCamera);
                    newCam.targetTexture = new RenderTexture(Screen.width / 2, Screen.height / 2, 0); // needs to be square
                    newCam.targetTexture.Create();

                    ViewInfo viewInfo = ViewManager.GetViewInfo(View.BoardCentered);
                    _screenshotCamera.transform.position = new(1.15f, 11.5f, -1.2f);
                    _screenshotCamera.transform.localEulerAngles = viewInfo.camRotation;
                    newCam.fieldOfView = viewInfo.fov;

                    _screenshotCamera.SetActive(false);
                }
                return _screenshotCamera;
            }
        }

        private static bool _captureScreenshotNextFrame = false;
        public static bool CaptureScreenshotNextFrame
        {
            get => _captureScreenshotNextFrame;
            set
            {
                _captureScreenshotNextFrame = value;
                ScreenshotCamera.SetActive(value);
            }
        }
        public static Texture2D LastCapturedScreenshot = null;

        public static readonly Color FrameColor = new(0f, 0.085f, 0.085f, 1.0f);

        private void LateUpdate()
        {
            if (CaptureScreenshotNextFrame)
            {
                CaptureScreenshot();
                CaptureScreenshotNextFrame = false;
            }
        }

        public static MultiverseTelevisionScreen Create(Transform parent, bool hugeScreen = false)
        {
            // Make sure we can take screenshots
            _ = ScreenshotCamera;

            // Set up the TV screen itself
            GameObject retval = Instantiate(SpecialNodeHandler.Instance.buildACardSequencer.screen.gameObject, parent);
            Destroy(retval.transform.Find("Anim/ScreenInteractables").gameObject);
            Destroy(retval.transform.Find("RenderCamera/Content/BuildACardInterface").gameObject);
            retval.transform.Find("Anim/CableStart_1").gameObject.SetActive(false);
            retval.transform.Find("Anim/CableStart_2").gameObject.SetActive(false);
            retval.transform.Find("Anim/CableStart_3").gameObject.SetActive(false);

            float scale = hugeScreen ? DEFAULT_SCALE * 3.5f : DEFAULT_SCALE;
            retval.transform.localScale = new(scale, scale, scale);

            retval.transform.localPosition = new(0.6f, 0.2f, -1.8f); // below gems module
            retval.transform.localEulerAngles = new(270f, 180f, 0f);

            // Make the TV frame and arm glow juuuuust a little bit to be more visible
            GameObject frame = retval.transform.Find("Anim/BasicScreen/Frame").gameObject;
            Renderer frameRenderer = frame.GetComponent<Renderer>();
            frameRenderer.material.EnableKeyword("_EMISSION");
            frameRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            frameRenderer.material.SetColor("_EmissionColor", FrameColor);

            GameObject screen = retval.transform.Find("Anim/BasicScreen/ScreenPlane").gameObject;
            Renderer screenRenderer = screen.GetComponent<Renderer>();
            RenderTexture oldTexture = screenRenderer.material.mainTexture as RenderTexture;
            RenderTexture newTexture = new(oldTexture);
            screenRenderer.material.mainTexture = newTexture;

            Camera camera = retval.GetComponentInChildren<Camera>();
            camera.targetTexture = newTexture;

            GameObject pole = retval.transform.Find("Anim/Pole").gameObject;
            Destroy(pole);

            retval.SetActive(true);

            // Create the stupid thingo
            GameObject displaytexture = GameObject.CreatePrimitive(PrimitiveType.Quad);
            displaytexture.layer = LayerMask.NameToLayer("CardOffscreen");
            displaytexture.transform.SetParent(retval.transform.Find("RenderCamera/Content"));
            displaytexture.transform.localPosition = new(0f, 0f, 7f);
            displaytexture.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            displaytexture.transform.localScale = new(28f, 16f, 1f);
            GameObject.Destroy(displaytexture.GetComponent<Collider>());

            // Set the TV settings
            OLDTVScreen tvScreen = retval.GetComponentInChildren<OLDTVScreen>();
            tvScreen.chromaticAberrationMagnetude = 0.1f;
            tvScreen.noiseMagnetude = 0.15f;
            // tvScreen.staticMagnetude = 0.2f;
            // tvScreen.staticVertical = 15;
            // tvScreen.staticVerticalScroll = 0.1f;
            tvScreen.screenSaturation = 0f;

            OLDTVTube tvTube = retval.GetComponentInChildren<OLDTVTube>();
            tvTube.radialDistortion = 0.6f;
            tvTube.reflexMagnetude = 0.2f;

            MultiverseTelevisionScreen screenController = retval.AddComponent<MultiverseTelevisionScreen>();
            screenController.FrameRenderer = frameRenderer;
            screenController.ContentsRenderer = displaytexture.GetComponent<Renderer>();
            screenController.ContentsRenderer.material.EnableKeyword("_EMISSION");
            screenController.ContentsRenderer.material.SetColor("_EmissionColor", Color.white);
            screenController.ContentsRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            screenController.ContentsRenderer.enabled = false;

            // Create a box collider
            BoxCollider collider = retval.AddComponent<BoxCollider>();
            collider.size = new(4f, 3f, 1f);

            // Go ahead and take a screenshot as a test
            //screenController.StartCoroutine(screenController.SetScreenshotContents());

            return screenController;
        }

        public void SetScreenContents(Texture2D screenTexture)
        {
            Texture2D oldTexture = this.ScreenshotTexture;
            this.ScreenshotTexture = screenTexture;
            this.ContentsRenderer.enabled = screenTexture != null;
            if (this.ContentsRenderer.enabled)
            {
                Material material = this.ContentsRenderer.material;
                material.SetTexture("_MainTex", screenTexture);
                material.SetTexture("_EmissionMap", screenTexture);
            }
        }

        public IEnumerator SetScreenshotContents()
        {
            CaptureScreenshotNextFrame = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            SetScreenContents(LastCapturedScreenshot);
        }

        private static void CaptureScreenshot()
        {
            Camera playerCamera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();
            Camera camera = ScreenshotCamera.GetComponent<Camera>();

            RenderTexture currentRT = RenderTexture.active;

            RenderTexture.active = camera.targetTexture;

            camera.Render();

            Texture2D screenshot = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            screenshot.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = currentRT;

            LastCapturedScreenshot = screenshot;
        }

        [HarmonyPatch(typeof(ViewManager), nameof(ViewManager.GetViewInfo))]
        [HarmonyPrefix]
        private static bool TVScreenViewInfo(View view, ref ViewInfo __result)
        {
            if (view != TVScreenView)
                return true;

            __result = new()
            {
                camPosition = new Vector3(0f, 6.8637f, -1.2497f),
                camRotation = new Vector3(17.4f, 0f, 0f),
                fov = 60f
            };

            return false;
        }

        [HarmonyPatch(typeof(ViewController), nameof(ViewController.SwitchToControlMode))]
        [HarmonyPostfix]
        private static void TVScreenControlModes(ViewController __instance, ViewController.ControlMode mode)
        {
            if (MultiverseBattleSequencer.Instance == null)
                return;

            if (mode == ViewController.ControlMode.CardGameDefault || mode == ViewController.ControlMode.CardGameChoosingSlot)
            {
                __instance.allowedViews.Add(TVScreenView);
                if (mode == ViewController.ControlMode.CardGameDefault)
                {
                    __instance.altTransitionInputs.Add(new(TVScreenView, View.Consumables, Button.LookRight));
                    __instance.altTransitionInputs.Add(new(TVScreenView, View.Scales, Button.LookLeft));
                }
            }
        }
    }
}