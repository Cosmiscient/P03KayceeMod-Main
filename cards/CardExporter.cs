using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class CardExporter : ManagedBehaviour
    {

        [HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.ValidStatsLayer))]
        [HarmonyPostfix]
        private static void AttachExporter(ref CardRenderCamera __instance)
        {
            if (__instance.gameObject.GetComponent<CardExporter>() == null)
            {
                P03Plugin.Log.LogDebug("Adding Card Exporter!");
                __instance.gameObject.AddComponent<CardExporter>();
            }
        }

        public void StartCardExport()
        {
            inRender = true;
            base.StartCoroutine(ExportAllCards());
        }

        [SerializeField]        
        private GameObject temporaryHolding;

        [SerializeField]
        private PlayableCard dummyCard;

        private static RenderStatsLayer statsLayer = null;

        private bool IsTalkingCard(CardInfo info)
        {
            return info.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.DynamicPortrait) || info.animatedPortrait != null;
        }

        [SerializeField]
        public float xOffset = 0.4f;

        internal static readonly string[] GameObjectPaths = new string[]
        {
            "Anim/CardBase/Rails",
            "Anim/CardBase/Top",
            "Anim/CardBase/Bottom"
        };

        private bool inRender = false;

        public override void ManagedUpdate()
        {
            if (!inRender && 
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && 
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && 
                Input.GetKey(KeyCode.X))
                StartCardExport();
        }

        private static Bounds GetMaxBounds(GameObject g) {

            List<Renderer> renderers = new ();
            foreach (string p in GameObjectPaths)
            {
                Transform t = g.transform.Find(p);
                if (t != null)
                    renderers.AddRange(t.gameObject.GetComponents<Renderer>());
            }

            if (renderers.Count == 0) return new Bounds(g.transform.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (Renderer r in renderers) {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

        public IEnumerator ExportAllCards()
        {
            ViewManager.Instance.SwitchToView(View.MapDeckReview);
            yield return new WaitForSeconds(0.25f);
            Tween.LocalRotation(ViewManager.Instance.cameraParent, new Vector3(90f, 0f, 0f), 0f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
            ViewManager.Instance.controller.LockState = ViewLockState.Locked;

            Color originalHangingLightColor = ExplorableAreaManager.Instance.hangingLight.color;
            Color originalHangingLightCardColor = ExplorableAreaManager.Instance.hangingCardsLight.color;

            bool noiseEnabled = GameOptions.optionsData.noiseEnabled;
            GameOptions.optionsData.noiseEnabled = false;

            bool flickeringDisabled = GameOptions.optionsData.flickeringDisabled;
            GameOptions.optionsData.flickeringDisabled = true;

            bool screenshakeDisabled = GameOptions.optionsData.screenshakeDisabled;
            GameOptions.optionsData.screenshakeDisabled = true;

            //ExplorableAreaManager.Instance.SetHangingLightColors(GameColors.instance.brightSeafoam, GameColors.instance.brightSeafoam);

            yield return new WaitForSeconds(.15f);

            if (!Directory.Exists("cardexports"))
                Directory.CreateDirectory("cardexports");

            Camera camera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();

            Texture2D screenshot = new Texture2D(Screen.currentResolution.width, Screen.currentResolution.height);
            screenshot.filterMode = FilterMode.Trilinear;
            Texture2D finalTexture = null;

            List<CardInfo> cardsToRender = CardManager.AllCardsCopy.Where(ci => ci.temple == CardTemple.Tech && ci.name[0] != '!').ToList();

            while (cardsToRender.Count > 0)
            {
                List<PlayableCard> currentBatch = new ();

                Vector3 renderPosition = new(0f,0f,0f);
                while (cardsToRender.Count > 0 && currentBatch.Count < 20)
                {
                    CardInfo info = cardsToRender[0];
                    cardsToRender.RemoveAt(0);
                    
                    PlayableCard card = CardSpawner.SpawnPlayableCard(info);
                    card.gameObject.transform.localPosition = new Vector3(card.gameObject.transform.localPosition.x + xOffset, card.gameObject.transform.localPosition.y, card.gameObject.transform.localPosition.z);
                    renderPosition = card.gameObject.transform.localPosition;
                    card.gameObject.transform.localPosition = card.gameObject.transform.localPosition + new Vector3(0, 10, 0);
                    currentBatch.Add(card);
                    yield return new WaitForEndOfFrame();
                }

                yield return new WaitForSeconds(.5f);

                for (int i = 0; i < currentBatch.Count; i++)
                {
                    PlayableCard card = currentBatch[i];
                    string filename = $"cardexports/{card.Info.name}.png";

                    card.gameObject.transform.localPosition = renderPosition;
                    yield return new WaitForSeconds(0.1f);
                    yield return new WaitForEndOfFrame();

                    try
                    {
                        screenshot.ReadPixels(new (0, 0, Screen.currentResolution.width, Screen.currentResolution.height), 0, 0, false);
                        screenshot.Apply();

                        Bounds cardBounds = GetMaxBounds(card.gameObject);
                        Vector2 lower = camera.WorldToScreenPoint(cardBounds.min);
                        Vector2 upper = camera.WorldToScreenPoint(cardBounds.max);
                        int width = Mathf.RoundToInt(Mathf.Abs(lower.x - upper.x));
                        int height = Mathf.RoundToInt(Mathf.Abs(lower.y - upper.y));
                        int xMin = Mathf.RoundToInt(Mathf.Min(lower.x, upper.x));
                        int yMin = Mathf.RoundToInt(Mathf.Min(lower.y, upper.y));

                        if (finalTexture == null)
                        {
                            finalTexture = new (width, height);
                            finalTexture.filterMode = FilterMode.Trilinear;
                        }

                        for (int x = 0; x < width; x++)
                            for (int y = 0; y < height; y++)
                                finalTexture.SetPixel(x, y, screenshot.GetPixel(x + xMin, y + yMin));


                        P03Plugin.Log.LogDebug("Writing file");
                        File.WriteAllBytes(filename, ImageConversion.EncodeToPNG(finalTexture));
                    }
                    catch (Exception ex)
                    {
                        P03Plugin.Log.LogError(ex);
                    }

                    card.transform.localPosition = card.transform.localPosition + new Vector3(0, 10, 0);
                    yield return new WaitForEndOfFrame();
                    if (card != null)
                        GameObject.DestroyImmediate(card.gameObject);
                    yield return new WaitForEndOfFrame();
                }                
            }

            GameObject.Destroy(finalTexture);
            GameObject.Destroy(screenshot);
            ExplorableAreaManager.Instance.SetHangingLightColors(originalHangingLightColor, originalHangingLightCardColor);

            GameOptions.optionsData.noiseEnabled = noiseEnabled;
            GameOptions.optionsData.flickeringDisabled = flickeringDisabled;
            GameOptions.optionsData.screenshakeDisabled = screenshakeDisabled;

            ViewManager.Instance.controller.LockState = ViewLockState.Unlocked;

            inRender = false;

            yield break;
        }
    }
}
