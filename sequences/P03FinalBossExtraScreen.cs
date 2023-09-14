using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class P03FinalBossExtraScreen : MonoBehaviour
    {
        public bool RespondsToDownloading = false;
        public bool PulsesWithMusic = false;

        public static readonly P03AnimationController.Face LOADING_FACE = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "LOADINGFACE");
        public static readonly P03AnimationController.Face LOOKUP_FACE = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "LOOKUP_FACE");

        public const float DEFAULT_SCALE = 0.6f;

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

        private static readonly Dictionary<P03AnimationController.Face, List<Sprite>> PreparedSprites = new()
        {
            {LOADING_FACE, GetLoadingFace()},
            {LOOKUP_FACE, new() { FromFaceKey("lookup")}}
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

        public static P03FinalBossExtraScreen Create(Transform parent)
        {
            // Set up the TV screen itself
            GameObject retval = Instantiate(SpecialNodeHandler.Instance.buildACardSequencer.screen.gameObject, parent);
            Destroy(retval.transform.Find("Anim/ScreenInteractables").gameObject);
            Destroy(retval.transform.Find("RenderCamera/Content/BuildACardInterface").gameObject);
            retval.transform.Find("Anim/CableStart_1").gameObject.SetActive(false);
            retval.transform.Find("Anim/CableStart_2").gameObject.SetActive(false);
            retval.transform.Find("Anim/CableStart_3").gameObject.SetActive(false);

            retval.transform.localScale = new(DEFAULT_SCALE, DEFAULT_SCALE, DEFAULT_SCALE);

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
            displaytexture.transform.localPosition = new Vector3(-1f, 0f, 7.0601f);
            displaytexture.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            displaytexture.transform.localScale = new Vector3(18f, 20f, 10f);

            SpriteRenderer spriteRenderer = displaytexture.AddComponent<SpriteRenderer>();
            spriteRenderer.SetMaterial(Resources.Load<Material>("art/materials/sprite_coloroverlay"));
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
    }
}