using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RareDiscCardAppearance : EmissiveDiscBorderBase
    {
        public static Texture2D RareTexture { get; private set; }
        public static Texture2D AltRareTexture { get; private set; }
        private static Texture2D OriginalRareTexture { get; set; }
        private Texture2D currentRareTexture { get; set; }

        private const string RARE_BORDER_KEY = "rareborder=[";
        private const string RARE_PORTRAIT_KEY = "rareportrait=[";
        private static Dictionary<string, Color?> configColors = new ();

        private void CalcRareTexture()
        {
            float red = ColorDistance(GameColors.Instance.darkRed, this.EmissionColor);
            float gold = ColorDistance(GameColors.Instance.darkGold, this.EmissionColor);
            float blue = ColorDistance(GameColors.Instance.blue, this.EmissionColor);
            float min = Mathf.Min(red, gold, blue);

            if (red == min)
                currentRareTexture = AltRareTexture;
            else if (gold == min)
                currentRareTexture = RareTexture;
            else
                currentRareTexture = OriginalRareTexture;
        }

        private static float ColorDistance(Color a, Color b)
        {
            float fbar = 0.5f * ((float)a.r + (float)b.r);
            float deltar = (float)(a.r - b.r);
            float deltag = (float)(a.g - b.g);
            float deltab = (float)(a.b - b.b);
            if (fbar < 128)
                return Mathf.Sqrt(2 * deltar * deltar + 4 * deltag * deltag + 3 * deltab * deltab);
            else
                return Mathf.Sqrt(3 * deltar * deltar + 4 * deltag * deltag + 2 * deltab * deltab);
        }

        private static Color? GetColorFromConfig(string key)
        {
            if (configColors.ContainsKey(key))
                return configColors[key];

            try
            {
                int start = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf(key) + key.Length;
                int end = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf("]", start);
                string colorKey = P03Plugin.Instance.DebugCode.ToLowerInvariant().Substring(start, end-start);
                string[] keyComponents = colorKey.Split(',');
                if (keyComponents.Length == 3)
                    configColors[key] = new Color(float.Parse(keyComponents[0]), float.Parse(keyComponents[1]), float.Parse(keyComponents[2]));
                else if (keyComponents.Length == 4)
                    configColors[key] = new Color(float.Parse(keyComponents[0]), float.Parse(keyComponents[1]), float.Parse(keyComponents[2]), float.Parse(keyComponents[3]));
                else
                    configColors[key] = null;
            } 
            catch 
            {
                configColors[key] = null;
            }

            return configColors[key];
        }

        private Color? overrideColor = null;

        [SerializeField]
        public override Color EmissionColor
        {
            get
            {
                if (overrideColor.HasValue)
                    return overrideColor.Value;
                else if (GetColorFromConfig(RARE_BORDER_KEY).HasValue)
                    return GetColorFromConfig(RARE_BORDER_KEY).Value;
                else if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("red"))
                    return GameColors.Instance.darkRed;
                else
                    return GameColors.Instance.darkGold;
            }
            set
            {
                overrideColor = value;
                CalcRareTexture();
                this.ApplyAppearance();
                this.Card.RenderCard();
            }
        }

        private Color? overridePortraitColor = null;

        [SerializeField]
        public Color PortraitColor
        {
            get
            {
                if (overridePortraitColor.HasValue)
                    return overridePortraitColor.Value;
                else if (GetColorFromConfig(RARE_PORTRAIT_KEY).HasValue)
                    return GetColorFromConfig(RARE_PORTRAIT_KEY).Value;
                else if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("red"))
                    return GameColors.Instance.glowRed;
                else
                    return GameColors.Instance.blue;
            }
            set
            {
                overridePortraitColor = value;
                this.ApplyAppearance();
                this.Card.RenderCard();
            }
        }

        [SerializeField]
        public override float Intensity
        { 
            get => base.Intensity;
            set
            {
                base.Intensity = value;
                this.ApplyAppearance();
                this.Card.RenderCard();
            }
        }

        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        static RareDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RareDiscAppearance", typeof(RareDiscCardAppearance)).Id;

            // I'm going to manually load this texture; it seems to have some issues
            RareTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            byte[] imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Gold_2048.png", typeof(RareDiscCardAppearance).Assembly);
            bool isLoaded = RareTexture.LoadImage(imgBytes);
            RareTexture.filterMode = FilterMode.Point;

            AltRareTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Red_2048.png", typeof(RareDiscCardAppearance).Assembly);
            isLoaded = AltRareTexture.LoadImage(imgBytes);
            AltRareTexture.filterMode = FilterMode.Point;
        }

        public override void ApplyAppearance()
        {
			// Let's try this nonsense just by guessing
            foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>())
            {
                try
                {
                    if (OriginalRareTexture == null)
                        OriginalRareTexture = renderer.material.GetTexture("_MainTex") as Texture2D;

                    if (currentRareTexture == null)
                        CalcRareTexture();

                    if (renderer.material.name.ToLowerInvariant().Contains("floppydisc_albedo"))
                        renderer.material.SetTexture("_MainTex", currentRareTexture);
                }
                catch (Exception ex)
                {
                    // Do nothing
                }
            }

			this.Card.RenderInfo.portraitColor = this.PortraitColor;

            if (this.Card.StatsLayer is DiskRenderStatsLayer drsl)
                drsl.lightColor = this.PortraitColor;

            

            base.ApplyAppearance();
        }
    }
}