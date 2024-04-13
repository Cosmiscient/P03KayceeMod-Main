using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class PaperworkDecalAppearance : CardAppearanceBehaviour
    {
        private static readonly Texture2D STAMPED = TextureHelper.GetImageAsTexture("portrait_paperwork_filed.png", typeof(PaperworkDecalAppearance).Assembly);

        public static Appearance ID { get; private set; }

        public override void ApplyAppearance() { } // Actually does nothing

        public override void OnPreRenderCard() => ApplyAppearance();

        static PaperworkDecalAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "PaperworkDecalAppearance", typeof(EnergyConduitAppearnace)).Id;
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPostfix]
        private static void ReplaceDecalsForReplicas(CardDisplayer3D __instance, CardRenderInfo renderInfo, PlayableCard playableCard)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (renderInfo.baseInfo.appearanceBehaviour.Contains(ID))
            {
                List<Texture> decals = new() { CustomCards.DUMMY_DECAL, CustomCards.DUMMY_DECAL_2 };

                if (FilePaperworkStamp.StampedPaperwork.Contains(renderInfo.baseInfo.name))
                    decals.Add(STAMPED);
                else
                    decals.Add(CustomCards.DUMMY_DECAL);

                __instance.DisplayDecals(decals);
            }
        }
    }
}