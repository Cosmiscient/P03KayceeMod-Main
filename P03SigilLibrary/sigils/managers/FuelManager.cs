using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class FuelManager : ManagedBehaviour
    {
        public static readonly Color GreenFuel = new Color(0.18f, .78f, .22f) * 0.75f;
        public static readonly Color RedFuel = new Color(0.8f, .2f, .2f) * 0.25f;

        private Texture2D _scratchedTexture;
        private Texture2D ScratchedTexture
        {
            get
            {
                if (_scratchedTexture != null)
                    return _scratchedTexture;
                _scratchedTexture = TextureHelper.GetImageAsTexture("scratched_large.png", typeof(FuelManager).Assembly);
                return _scratchedTexture;
            }
        }

        private static List<Sprite> PixelFuelSprites = new()
        {
            TextureHelper.GetImageAsSprite("pixel_fuel_0.png", typeof(FuelManager).Assembly, TextureHelper.SpriteType.PixelPortrait),
            TextureHelper.GetImageAsSprite("pixel_fuel_1.png", typeof(FuelManager).Assembly, TextureHelper.SpriteType.PixelPortrait),
            TextureHelper.GetImageAsSprite("pixel_fuel_2.png", typeof(FuelManager).Assembly, TextureHelper.SpriteType.PixelPortrait),
            TextureHelper.GetImageAsSprite("pixel_fuel_3.png", typeof(FuelManager).Assembly, TextureHelper.SpriteType.PixelPortrait),
            TextureHelper.GetImageAsSprite("pixel_fuel_4.png", typeof(FuelManager).Assembly, TextureHelper.SpriteType.PixelPortrait),
        };

        private static List<Texture2D> FuelTextures = new()
        {
            TextureHelper.GetImageAsTexture("fuel_gauge_0.png", typeof(FuelManager).Assembly),
            TextureHelper.GetImageAsTexture("fuel_gauge_1.png", typeof(FuelManager).Assembly),
            TextureHelper.GetImageAsTexture("fuel_gauge_2.png", typeof(FuelManager).Assembly),
            TextureHelper.GetImageAsTexture("fuel_gauge_3.png", typeof(FuelManager).Assembly),
            TextureHelper.GetImageAsTexture("fuel_gauge_4.png", typeof(FuelManager).Assembly),
        };

        private static FuelManager m_instance;
        public static FuelManager Instance
        {
            get
            {
                if (m_instance != null)
                    return m_instance;

                if (BoardManager.Instance == null)
                    return null;

                m_instance = BoardManager.Instance.gameObject.AddComponent<FuelManager>();
                return m_instance;
            }
        }

        public class Status
        {
            public int StartingFuel { get; internal set; }
            public int CurrentFuel { get; internal set; }
        }

        [HarmonyPatch(typeof(Card), nameof(Card.SetInfo))]
        [HarmonyPrefix]
        private static void EstablishFuelReserves(Card __instance, CardInfo info)
        {
            Status status = FuelExtensions.FuelStatus.GetOrCreateValue(__instance);
            if (status.StartingFuel == 0 && info.GetStartingFuel() > 0)
            {
                status.StartingFuel = info.GetStartingFuel();
                status.CurrentFuel = info.GetStartingFuel();
            }
        }

        internal void Render3DFuelGauge(Card card)
        {
            DiskRenderStatsLayer stats = card.StatsLayer as DiskRenderStatsLayer;
            if (stats == null)
            {
                card.RenderCard();
                return;
            }

            var railsParent = stats.gameObject.transform.Find("Rails");

            Transform gauge = railsParent.Find("FuelGauge");
            if (gauge == null)
            {
                var fuelGauge = GameObject.Instantiate(ResourceBank.Get<GameObject>("p03kcm/prefabs/fuelgauge"), railsParent);
                fuelGauge.name = "FuelGauge";
                fuelGauge.transform.localEulerAngles = new(0f, 359.68f, 270f);
                fuelGauge.transform.localPosition = new(1.63f, 0f, 0f);
                fuelGauge.transform.localScale = new(1f, 1f, 0.43f);
                gauge = fuelGauge.transform;

                var mainBody = gauge.Find("default").gameObject;

                mainBody.AddComponent<BoxCollider>();
                var aii = mainBody.AddComponent<GenericAltInputInteractable>();
                aii.cursorType = CursorType.Inspect;
                aii.AlternateSelectEnded = aii => FuelRulebookManager.OpenToFuelRulebookPage(card as PlayableCard);
            }

            PlayableCard playableCard = card as PlayableCard;
            int fuelToDisplay = playableCard == null ? card.Info.GetStartingFuel() : (playableCard.GetCurrentFuel() ?? -1);

            for (int i = 0; i <= 4; i++)
            {
                string name = $"fueltank_{i}";
                GameObject obj = gauge.Find(name).gameObject;
                obj.SetActive(fuelToDisplay == i);
            }
        }

        internal void RenderCurrentFuel(Card card)
        {
            // Currently this is just a stub until we get the full 3D renderer
            if (card.StatsLayer is DiskRenderStatsLayer)
                Render3DFuelGauge(card);
            else
                card.RenderCard();
        }

        private static Renderer GetDefaultDecalRenderer(CardDisplayer3D displayer)
        {
            if (displayer == null)
                return null;

            if (displayer.decalRenderers == null)
                return null;

            if (displayer.decalRenderers.Count == 0)
                return null;

            return displayer.decalRenderers[0];
        }

        private static Transform GetDecalParent(CardDisplayer3D displayer)
        {
            return GetDefaultDecalRenderer(displayer)?.gameObject.transform.parent;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPostfix]
        private static void Render3DFuel(Card __instance)
        {
            if (__instance is PixelSelectableCard || __instance.StatsLayer is not DiskRenderStatsLayer)
                return;

            PlayableCard playableCard = __instance as PlayableCard;
            int fuelToDisplay = playableCard == null ? __instance.Info.GetStartingFuel() : (playableCard.GetCurrentFuel() ?? -1);
            bool displayFuel = playableCard == null ? fuelToDisplay > 0 : playableCard.HasFuel();
            if (displayFuel)
                FuelManager.Instance.Render3DFuelGauge(__instance);
        }

        [HarmonyPatch(typeof(CardAnimationController), nameof(CardAnimationController.SetFaceDown))]
        [HarmonyPostfix]
        private static void ManageFlippedFuelGauge(CardAnimationController __instance, bool faceDown)
        {
            if (__instance is not DiskCardAnimationController dcac)
                return;

            if (dcac.Card == null)
                return;

            var railsParent = dcac.Card.StatsLayer.gameObject.transform.Find("Rails");

            Transform gauge = railsParent.Find("FuelGauge");

            if (gauge != null)
            {
                CustomCoroutine.WaitThenExecute(0.22f, () => gauge.gameObject.SetActive(!faceDown));
            }
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPrefix]
        private static void AddFuelDecal(ref CardDisplayer3D __instance)
        {
            if (__instance is DiskScreenCardDisplayer)
                return;

            var decalParent = GetDecalParent(__instance);
            if (decalParent == null)
                return;

            if (decalParent.Find("Decal_Fuel") == null)
            {
                GameObject portrait = __instance.portraitRenderer.gameObject;
                GameObject decalFull = GetDefaultDecalRenderer(__instance)?.gameObject;
                if (decalFull == null)
                    return;

                GameObject decalGood = UnityEngine.Object.Instantiate(decalFull, decalParent);
                decalGood.name = "Decal_Fuel";
                decalGood.transform.localPosition = portrait.transform.localPosition + new Vector3(0f, 0f, -0.0001f);
                decalGood.transform.localScale = new(1f, 1f, 1f);
            }
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPostfix]
        private static void DecalsForFuel(CardDisplayer3D __instance, CardRenderInfo renderInfo, PlayableCard playableCard)
        {
            if (__instance is DiskScreenCardDisplayer)
                return;

            var decalParent = GetDecalParent(__instance);
            var fuelDecalObj = decalParent?.Find("Decal_Fuel");
            if (fuelDecalObj == null)
                return;

            int fuelToDisplay = playableCard == null ? (renderInfo?.baseInfo?.GetStartingFuel() ?? -1) : (playableCard.GetCurrentFuel() ?? -1);
            bool displayFuel = playableCard == null ? fuelToDisplay > 0 : playableCard.HasFuel();
            var fuelDecal = fuelDecalObj.GetComponent<Renderer>();
            if (displayFuel)
            {
                fuelDecal.enabled = true;
                fuelDecal.material.mainTexture = FuelTextures[fuelToDisplay];
            }
            else
            {
                fuelDecal.enabled = false;
            }
        }

        [HarmonyPatch(typeof(PixelCardDisplayer), nameof(PixelCardDisplayer.DisplayInfo))]
        [HarmonyPostfix]
        private static void PixelFuelDisplayer(PixelCardDisplayer __instance, CardRenderInfo renderInfo, PlayableCard playableCard)
        {
            if (__instance.cardElements == null)
                return;

            P03SigilLibraryPlugin.Log.LogInfo("Starting");

            int fuelToDisplay = playableCard == null ? (renderInfo?.baseInfo?.GetStartingFuel() ?? -1) : (playableCard?.GetCurrentFuel() ?? -1);
            bool displayFuel = playableCard == null ? fuelToDisplay > 0 : playableCard?.HasFuel() ?? false;

            P03SigilLibraryPlugin.Log.LogInfo($"Should display fuel? {displayFuel}");

            Transform fuelSprite = __instance.cardElements.transform.Find("FuelPortrait");
            if (fuelSprite == null)
            {
                P03SigilLibraryPlugin.Log.LogInfo("Fuel sprite is missing!");
                GameObject fuelSpriteObject = GameObject.Instantiate(__instance.cardElements.transform.Find("Portrait").gameObject, __instance.cardElements.transform);
                fuelSpriteObject.name = "FuelPortrait";
                fuelSprite = fuelSpriteObject.transform;
            }

            if (!displayFuel)
            {
                P03SigilLibraryPlugin.Log.LogInfo("Disable the fuel sprite!");
                fuelSprite.gameObject.SetActive(false);
                return;
            }

            fuelSprite.gameObject.SetActive(true);
            P03SigilLibraryPlugin.Log.LogInfo("Get the renderer");
            SpriteRenderer renderer = fuelSprite.gameObject.GetComponent<SpriteRenderer>();
            P03SigilLibraryPlugin.Log.LogInfo("Set the sprite");
            renderer.sprite = PixelFuelSprites[fuelToDisplay];
            renderer.enabled = true;
            P03SigilLibraryPlugin.Log.LogInfo("Set the sort orderer");
            renderer.sortingOrder = 20;
        }
    }
}