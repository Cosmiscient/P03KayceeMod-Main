using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Managers
{
    [HarmonyPatch]
    public class FuelManager : ManagedBehaviour
    {
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

        internal void RenderCurrentFuel(Card card)
        {
            // Currently this is just a stub until we get the full 3D renderer
            card.RenderCard();
        }


        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPrefix]
        private static void AddFuelDecal(ref CardDisplayer3D __instance)
        {
            if (__instance.gameObject.transform.Find("Decal_Fuel") == null)
            {
                GameObject portrait = __instance.portraitRenderer.gameObject;
                GameObject decalFull = __instance.decalRenderers[1].gameObject;
                GameObject decalGood = UnityEngine.Object.Instantiate(decalFull, decalFull.transform.parent);
                decalGood.name = "Decal_Fuel";
                decalGood.transform.localPosition = portrait.transform.localPosition + new Vector3(-.1f, 0f, -0.0001f);
                decalGood.transform.localScale = new(1.2f, 1f, 0f);
            }
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPostfix]
        private static void DecalsForFuel(CardDisplayer3D __instance, CardRenderInfo renderInfo, PlayableCard playableCard)
        {
            int fuelToDisplay = playableCard == null ? renderInfo.baseInfo.GetStartingFuel() : playableCard.GetCurrentFuel() ?? -1;
            bool displayFuel = playableCard == null ? fuelToDisplay > 0 : playableCard.HasFuel();
            var fuelDecal = __instance.gameObject.transform.Find("Decal_Fuel").GetComponent<Renderer>();
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
    }
}