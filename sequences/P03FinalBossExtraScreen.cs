using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class P03FinalBossExtraScreen : MonoBehaviour
    {
        public bool RespondsToDownloading = false;
        public bool PulsesWithMusic = false;

        public int xIndex;
        public int yIndex;

        public static readonly P03AnimationController.Face LOADING_FACE = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "LOADINGFACE");
        public static readonly P03AnimationController.Face LOOKUP_FACE = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "LOOKUP_FACE");
        public static readonly P03AnimationController.Face BIG_MOON_FACE = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "BIG_MOON_FACE");

        public const float DEFAULT_SCALE = 0.6f;
        public const float BUMP_SCALE = 0.63f;

        public static readonly Vector3 DEFAULT_SCALE_VECTOR = new(DEFAULT_SCALE, DEFAULT_SCALE, DEFAULT_SCALE);
        public static readonly Vector3 BUMP_SCALE_VECTOR = new(BUMP_SCALE, BUMP_SCALE, BUMP_SCALE);

        private static List<Sprite> GetLoadingFace()
        {
            Texture2D loadingTexture = TextureHelper.GetImageAsTexture("p03_face_loading.png", typeof(P03FinalBossExtraScreen).Assembly);
            List<Sprite> retval = new();
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 2; x++)
                    retval.Add(Sprite.Create(loadingTexture, new Rect(x * 90, y * 65, 90, 65), new(0.5f, 0.5f)));
            }

            return retval;
        }

        private static Sprite FromFaceKey(string facekey)
        {
            Texture2D texture = Resources.Load<Texture2D>($"art/character/p03 face/p03_face_{facekey}");
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        private static Sprite BigMoonSprite()
        {
            Texture2D texture = TextureHelper.GetImageAsTexture("moon_small_red.png", typeof(P03FinalBossExtraScreen).Assembly);
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        private static readonly Dictionary<P03AnimationController.Face, List<Sprite>> PreparedSprites = new()
        {
            { LOADING_FACE, GetLoadingFace() },
            { LOOKUP_FACE, new() { FromFaceKey("lookup") }},
            { BIG_MOON_FACE, new() { BigMoonSprite() }}
        };

        private SpriteRenderer SpriteRenderer;
        public Renderer FrameRenderer;

        public static readonly Color FrameColor = new(0f, 0.085f, 0.085f, 1.0f);
        public static readonly Color RedFrameColor = new(.15f, 0.085f, 0.085f, 1f);

        private List<Sprite> CurrentSprites;
        private bool animated = false;
        private int frame = 0;

        private const float SECONDS_PER_FRAME = 0.25f;
        private float currentTick = 0f;

        private void Update()
        {
            if (animated)
            {
                currentTick += Time.deltaTime;
                while (currentTick > SECONDS_PER_FRAME)
                {
                    currentTick -= SECONDS_PER_FRAME;
                    frame += 1;
                    if (frame == CurrentSprites.Count)
                        frame = 0;
                    SpriteRenderer.sprite = CurrentSprites[frame];
                }
            }
        }

        public void ShowFace(P03AnimationController.Face face)
        {
            if (!PreparedSprites.ContainsKey(face))
            {
                try
                {
                    string facekey = face == P03AnimationController.Face.Bored ? "impatient" : face.ToString().ToLowerInvariant();
                    Sprite sprite = FromFaceKey(facekey);
                    PreparedSprites[face] = new() { sprite };
                }
                catch
                {
                    ShowFace(P03AnimationController.Face.Default);
                    PreparedSprites[face] = PreparedSprites[P03AnimationController.Face.Default];
                    return;
                }
            }

            CurrentSprites = PreparedSprites[face];
            SpriteRenderer.sprite = CurrentSprites[0];
            frame = 0;
            animated = CurrentSprites.Count > 1;
        }

        public static P03FinalBossExtraScreen Create(Transform parent, bool hugeScreen = false)
        {
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
            //BonesTVScreen.transform.localPosition = new (-1.1717f, 0.22f, -0.85f); // Right side of thingo
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

            // Put a texture renderer on the TV screen that we can update with the appropriate textures:
            GameObject displaytexture = new("DisplayTexture")
            {
                layer = LayerMask.NameToLayer("CardOffscreen")
            };
            displaytexture.transform.SetParent(retval.transform.Find("RenderCamera/Content"));
            displaytexture.transform.localPosition = hugeScreen ? new Vector3(0f, 0f, 7.0601f) : new Vector3(-1f, 0f, 7.0601f);
            displaytexture.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            displaytexture.transform.localScale = hugeScreen ? new Vector3(3f, 2f, 2f) : new Vector3(25f, 20f, 10f);

            SpriteRenderer spriteRenderer = displaytexture.AddComponent<SpriteRenderer>();
            spriteRenderer.material.SetColor("_Color", GameColors.Instance.brightBlue * 0.85f);

            spriteRenderer.material.EnableKeyword("_EMISSION");
            spriteRenderer.material.SetColor("_EmissionColor", Color.white);

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

            displaytexture.SetActive(true);

            P03FinalBossExtraScreen screenController = retval.AddComponent<P03FinalBossExtraScreen>();
            screenController.SpriteRenderer = spriteRenderer;
            screenController.FrameRenderer = frameRenderer;

            return screenController;
        }

        public class Orbiter : ManagedBehaviour
        {
            private float HorizontalRadius = 0;
            public float VerticalRadius = 0;
            public Vector3 CenterPoint;
            public float RotationSpeed = 2f * Mathf.PI / 14f;
            private const float FULL_ROTATION = 2f * Mathf.PI;
            private float Theta = 0f;
            private bool hasBeenReset = false;
            private bool pendingStop = true;
            private bool destroyOnStop = false;

            public Vector3 LastResetPoint { get; private set; }
            public Vector3 LastResetRotation { get; private set; }
            public float LastResetTheta { get; private set; }

            public void Reset()
            {
                LastResetPoint = transform.position;
                LastResetRotation = transform.eulerAngles;

                Tween.Rotation(transform, Vector3.zero, 0.4f, 0f);
                CenterPoint = new(CenterPoint.x, transform.position.y, CenterPoint.z);
                float xOffset = transform.position.x - CenterPoint.x;
                float yOffset = transform.position.z - CenterPoint.z;

                Theta = Mathf.Acos(yOffset / VerticalRadius);
                HorizontalRadius = xOffset / Mathf.Sin(Theta);

                if (HorizontalRadius < 0)
                {
                    Theta = (2f * Mathf.PI) - Theta;
                    HorizontalRadius = xOffset / Mathf.Sin(Theta);
                }
                hasBeenReset = true;
                pendingStop = false;
            }

            public void Stop(bool immediate = false, bool destroyAfter = false, float? stopTheta = null)
            {
                if (!hasBeenReset)
                    return;

                if (stopTheta.HasValue)
                {
                    LastResetTheta = stopTheta.Value;
                    while (LastResetTheta < 0)
                        LastResetTheta += FULL_ROTATION;
                    while (LastResetTheta > FULL_ROTATION)
                        LastResetTheta -= FULL_ROTATION;
                    LastResetPoint = new(
                        (HorizontalRadius * Mathf.Sin(stopTheta.Value)) + CenterPoint.x,
                        LastResetPoint.y,
                        (VerticalRadius * Mathf.Cos(stopTheta.Value)) + CenterPoint.z
                    );
                }

                if (immediate)
                {
                    hasBeenReset = false;
                    pendingStop = false;
                    Tween.Rotation(transform, LastResetRotation, 0.25f, 0f);
                    Tween.Position(transform, LastResetPoint, 0.2f, 0f, completeCallback: delegate ()
                    {
                        if (destroyAfter)
                            Destroy(gameObject);
                        else
                            enabled = false;
                    });
                }
                else
                {
                    destroyOnStop = true;
                    pendingStop = true;
                }
            }

            public override void ManagedUpdate()
            {
                if (enabled && hasBeenReset)
                {
                    Theta += Time.deltaTime * RotationSpeed;
                    while (Theta > FULL_ROTATION)
                        Theta -= FULL_ROTATION;
                    while (Theta < 0)
                        Theta += FULL_ROTATION;

                    float x = (HorizontalRadius * Mathf.Sin(Theta)) + CenterPoint.x;
                    float z = (VerticalRadius * Mathf.Cos(Theta)) + CenterPoint.z;

                    Vector3 newPosition = new(x, CenterPoint.y, z);
                    //transform.position = newPosition;

                    // Trying this trick from the rule painting orbiter
                    // Don't know why it works :')
                    transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 10f);

                    if (pendingStop)
                    {
                        float distanceToOriginal = Mathf.Abs(Theta - LastResetTheta);
                        if (distanceToOriginal < 0.0873f) // approx 5 degrees
                            Stop(immediate: true, destroyAfter: destroyOnStop, stopTheta: LastResetTheta);
                    }
                }
            }
        }

        public Orbiter StartRotation(Transform center)
        {
            GameObject anim = transform.Find("Anim").gameObject;
            Orbiter o = anim.GetComponent<Orbiter>();
            if (o != null)
                return o;

            PulsesWithMusic = false;

            anim.GetComponent<Animator>().enabled = false;
            o = anim.AddComponent<Orbiter>();
            o.CenterPoint = center.position;
            o.VerticalRadius = 2.8f + (0.1f * xIndex);
            o.Reset();
            return o;
        }
    }
}