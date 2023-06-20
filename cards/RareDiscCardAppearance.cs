using System;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RareDiscCardAppearance : EmissiveDiscBorderBase
    {
        public static Texture2D RareTexture { get; private set; }

        protected override Color EmissionColor => GameColors.Instance.darkRed;
        protected Color PortraitColor => GameColors.Instance.red;
        protected override float Intensity => 0.25f;

        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        static RareDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RareDiscAppearance", typeof(RareDiscCardAppearance)).Id;

            // I'm going to manually load this texture; it seems to have some issues
            RareTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            
            byte[] imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Red_2048.png", typeof(RareDiscCardAppearance).Assembly);
            bool isLoaded = RareTexture.LoadImage(imgBytes);
            RareTexture.filterMode = FilterMode.Point;
        }

        public override void ApplyAppearance()
        {
			// Let's try this nonsense just by guessing
            foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>())
            {
                try
                {
                    if (renderer.material.name.ToLowerInvariant().Contains("floppydisc_albedo"))
                        renderer.material.SetTexture("_MainTex", RareTexture);
                }
                catch (Exception ex)
                {
                    // Do nothing
                }
            }

            base.ApplyAppearance();

			this.Card.RenderInfo.portraitColor = this.PortraitColor;

            if (this.Card.StatsLayer is DiskRenderStatsLayer drsl)
                drsl.lightColor = this.PortraitColor;
        }
    }
}