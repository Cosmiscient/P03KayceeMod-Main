using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using InscryptionAPI.Helpers;
using System.Collections.Generic;
using HarmonyLib;
using System.Collections;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class DiscCardColorAppearance : CardAppearanceBehaviour
    {
        private Dictionary<string, Color?> _setColors = new();

        protected virtual Color? LookupColor(string key)
        {
            return null;
        }

        private Color? GetColor(string key)
        {
            if (_setColors.ContainsKey(key))
                return _setColors[key];

            _setColors[key] = LookupColor(key);

            if (!_setColors[key].HasValue)
                _setColors[key] = GetColorFromString(this.Card.Info.GetExtendedProperty(key));

            return _setColors[key];
        }

        public virtual Color? BorderColor { get => GetColor("BorderColor"); set => _setColors["BorderColor"] = value; }
        public virtual Color? PortraitColor { get => GetColor("PortraitColor"); set => _setColors["PortraitColor"] = value; }
        public virtual Color? EnergyColor { get => GetColor("EnergyColor"); set => _setColors["EnergyColor"] = value; }
        public virtual Color? NameTextColor { get => GetColor("NameTextColor"); set => _setColors["NameTextColor"] = value; }
        public virtual Color? NameBannerColor { get => GetColor("NameBannerColor"); set => _setColors["NameBannerColor"] = value; }
        public virtual Color? AttackColor { get => GetColor("AttackColor"); set => _setColors["AttackColor"] = value; }
        public virtual Color? HealthColor { get => GetColor("HealthColor"); set => _setColors["HealthColor"] = value; }
        public virtual Color? DefaultAbilityColor { get => GetColor("DefaultAbilityColor"); set => _setColors["DefaultAbilityColor"] = value; }

        internal static Texture2D RedTexture { get; set; }
        internal static Texture2D GoldTexture { get; set; }
        internal static Texture2D BlueTexture { get; set; }
        internal static Texture2D WhiteTextTexture { get; set; }
        internal Texture2D currentTexture { get; set; }

        private static Dictionary<string, Color> GameColorsCache;

        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        static DiscCardColorAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ColoredDiscAppearance", typeof(DiscCardColorAppearance)).Id;

            // I'm going to manually load this texture; it seems to have some issues
            GoldTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            byte[] imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Gold_2048.png", typeof(DiscCardColorAppearance).Assembly);
            bool isLoaded = GoldTexture.LoadImage(imgBytes);
            GoldTexture.filterMode = FilterMode.Point;

            RedTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Red_2048.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = RedTexture.LoadImage(imgBytes);
            RedTexture.filterMode = FilterMode.Point;

            BlueTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_2048.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = BlueTexture.LoadImage(imgBytes);
            BlueTexture.filterMode = FilterMode.Point;

            WhiteTextTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("heavyweight-white.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = WhiteTextTexture.LoadImage(imgBytes);
            WhiteTextTexture.filterMode = FilterMode.Bilinear;
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

        private static void PopulateGameColorsCache()
        {
            if (GameColorsCache != null)
                return;

            GameColorsCache = new ();

            foreach (var field in typeof(GameColors).GetFields())
            {
                if (field.FieldType == typeof(Color))
                {
                    P03Plugin.Log.LogDebug($"Adding {field.Name} to game colors cache");
                    GameColorsCache[field.Name.ToLowerInvariant()] = (Color)field.GetValue(GameColors.Instance);
                }
                else
                {
                    P03Plugin.Log.LogDebug($"Could not add {field.Name} to game colors cache");
                }
            }

            GameColorsCache["white"] = new Color(1f, 1f, 1f, 1f);
            GameColorsCache["black"] = new Color(0f, 0f, 0f, 1f);
        }

        private void CalcRareTexture()
        {
            if (!this.BorderColor.HasValue)
            {
                currentTexture = BlueTexture;
                return;
            }

            float red = ColorDistance(GameColors.Instance.darkRed, this.BorderColor.Value);
            float gold = ColorDistance(GameColors.Instance.darkGold, this.BorderColor.Value);
            float blue = ColorDistance(GameColors.Instance.blue, this.BorderColor.Value);

            if (red < gold && red < blue)
                currentTexture = RedTexture;
            else if (gold < red && gold < blue)
                currentTexture = GoldTexture;
            else
                currentTexture = BlueTexture;
        }

        internal static Color? GetColorFromString(string colorKey)
        {
            try
            {
                string[] keyComponents = colorKey.Split(',');
                
                if (keyComponents.Length == 1)
                {
                    if (keyComponents[0][0] == '#')
                    {
                        Color htmlColor;
                        if (ColorUtility.TryParseHtmlString(keyComponents[0], out htmlColor))
                            return htmlColor;
                        else
                            return null;
                    }
                    PopulateGameColorsCache();
                    return GameColorsCache[keyComponents[0].ToLowerInvariant()];
                }
                else if (keyComponents.Length == 2)
                {
                    return GetColorFromString(keyComponents[0]) * float.Parse(keyComponents[1]);
                }
                else if (keyComponents.Length == 3)
                {
                    return new Color(float.Parse(keyComponents[0]), float.Parse(keyComponents[1]), float.Parse(keyComponents[2]));
                }
                else if (keyComponents.Length == 4)
                {
                    return new Color(float.Parse(keyComponents[0]), float.Parse(keyComponents[1]), float.Parse(keyComponents[2]), float.Parse(keyComponents[3]));
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        internal static readonly string[] BorderObjectPaths = new string[]
        {
            "Anim/CardBase/Rails",
            "Anim/CardBase/Top",
            "Anim/CardBase/Bottom"
        };

        internal static readonly string[] StickerObjectPaths = new string[]
        {
            "Anim/CardBase/Top/Stickers/Sticker_1",
            "Anim/CardBase/Top/Stickers/Sticker_2",
            "Anim/CardBase/Top/Stickers/Sticker_3",
            "Anim/CardBase/Top/Stickers/Sticker_4",
            "Anim/CardBase/Top/Stickers/Sticker_5",
        };

        internal static readonly string[] TextureNames = new string[]
        {
            "_MainTex",
            "_DetailAlbedoMap",
        };

        private void ApplyColorSafe(string path, Color color, bool emission)
        {
            try
            {
                Transform tComp = this.gameObject.transform.Find(path);
                if (tComp != null && tComp.gameObject != null)
                {
                    GameObject component = tComp.gameObject;
                    MeshRenderer renderer = component.GetComponent<MeshRenderer>();
                    Material material = renderer.material;
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    if (emission)
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", color);
                    } else {
                        material.SetColor("_Color", color);
                    }
                }
            }
            catch
            {
                // Do nothing
            }
        }

        private static Dictionary<string, Dictionary<string, bool>> _rendererCache = new ();

        private static bool CardComponentHasTargetTexture(Renderer renderer, string textureName)
        {
            if (!_rendererCache.ContainsKey(renderer.gameObject.name))
                _rendererCache[renderer.gameObject.name] = new ();

            if (_rendererCache[renderer.gameObject.name].ContainsKey(textureName))
                return _rendererCache[renderer.gameObject.name][textureName];

            Texture tex = renderer.material.GetTexture(textureName);
            if (tex != null && tex.name.ToLowerInvariant().Contains("floppydisc"))
                _rendererCache[renderer.gameObject.name][textureName] = true;
            else
                _rendererCache[renderer.gameObject.name][textureName] = false;

            return _rendererCache[renderer.gameObject.name][textureName];
        }

        public override void ApplyAppearance()
        {
            // Swap out textures as appropriate
            foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>())
            {
                try
                {
                    if (currentTexture == null)
                        CalcRareTexture();

                    foreach (string textureName in TextureNames)
                        if (CardComponentHasTargetTexture(renderer, textureName))
                            foreach (var material in renderer.materials)
                                material.SetTexture(textureName, currentTexture);
                }
                catch
                {
                    // Do nothing
                }
            }

            // Apply the color to the border
            if (this.BorderColor.HasValue)
                foreach (string key in BorderObjectPaths)
                    ApplyColorSafe(key, this.BorderColor.Value, true);

            // Apply the color to the name background
            if (this.NameBannerColor.HasValue)
                foreach (string key in StickerObjectPaths)
                    ApplyColorSafe(key, this.NameBannerColor.Value, false);


            if (this.NameTextColor.HasValue && this.Card.StatsLayer is DiskRenderStatsLayer drsl)
            {
                if (this.EnergyColor.HasValue)
                    drsl.lightColor = this.EnergyColor.Value;

                if (drsl.labelText.fontMaterial.shader.name.EndsWith("(Surface)"))
                    drsl.labelText.fontMaterial.shader = Shader.Find(drsl.labelText.fontMaterial.shader.name.Replace(" (Surface)", ""));

                if (drsl.labelText.renderer.material.shader.name.EndsWith("(Surface)"))
                    drsl.labelText.renderer.material.shader = Shader.Find(drsl.labelText.renderer.material.shader.name.Replace(" (Surface)", ""));

                drsl.labelText.fontMaterial.SetTexture("_MainTex", WhiteTextTexture);
                drsl.labelText.renderer.material.SetTexture("_MainTex", WhiteTextTexture);
                string hexcode = ColorUtility.ToHtmlStringRGBA(this.NameTextColor.Value);
                drsl.labelText.SetText($"<color=#{hexcode}>{this.Card.Info.DisplayedNameLocalized}</color>", true);
            }
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            this.ApplyAppearance();
        }

        //private static readonly Color _defaultBarColor = new Color(.4858f, .8751f, 1f, 1f);

        [HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.UpdateTextureWhenReady))]
        [HarmonyPostfix]
        private static IEnumerator FixMiddleColor(IEnumerator sequence, CardRenderCamera __instance, RenderStatsLayer layer, CardRenderInfo info, PlayableCard playableCard, bool updateMain, bool updateEmission)
        {
            if (!(layer is DiskRenderStatsLayer drsl))
            {
                yield return sequence;
                yield break;
            }

            DiscCardColorAppearance appearance = drsl.gameObject.transform.parent.parent.gameObject.GetComponent<DiscCardColorAppearance>();

            Color myBarColor = drsl.defaultLightColor;
            if (appearance != null && appearance.BorderColor.HasValue)
                myBarColor = appearance.BorderColor.Value * 3;

            // The only way I can figure out how to do this is to just copy/paste the original method straight from dnSpy.
            // Sorry. I kinda hate doing it this way but I couldn't figure out a better way to do it.
            CardRenderCamera.renderQueue.Add(layer);
			while (CardRenderCamera.renderQueue.Count == 0 || CardRenderCamera.renderQueue[0] != layer)
			{
				CardRenderCamera.renderQueue.RemoveAll((RenderStatsLayer x) => !__instance.ValidStatsLayer(x));
				if (!__instance.ValidStatsLayer(layer))
				{
					yield break;
				}
				yield return new WaitForEndOfFrame();
			}
			if (__instance.ValidStatsLayer(layer))
			{
				__instance.cardDisplayer.DisplayInfo(info, playableCard);

                // Also change the bar color here.
                GameObject delimiter = __instance.transform.Find("CardsPlane/Base/Delimiter").gameObject;
                SpriteRenderer renderer = delimiter.GetComponent<SpriteRenderer>();
                renderer.color = myBarColor;
				yield return new WaitForEndOfFrame();
			}

			if (__instance.ValidStatsLayer(layer))
			{
				if (updateMain)
				{
					__instance.SetRenderLayerMainTexture(layer, RenderTextureSnapshotter.CopyRenderTexture(__instance.snapshotRenderTexture, FilterMode.Point));
				}
				if (updateEmission)
				{
					__instance.SetRenderLayerEmissionTexture(layer, RenderTextureSnapshotter.CopyRenderTexture(__instance.snapshotEmissionRenderTexture, FilterMode.Point));
				}
				CardRenderCamera.renderQueue.Remove(layer);
			}

            yield break;
        }

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.RenderCard))]
        [HarmonyPrefix]
        private static void RenderDiscCardAdjustColors(ref RenderStatsLayer __instance, CardRenderInfo info)
        {
            if (!(__instance is DiskRenderStatsLayer drsl))
                return;

            DiscCardColorAppearance appearance = drsl.gameObject.transform.parent.parent.gameObject.GetComponent<DiscCardColorAppearance>();

            if (appearance == null)
                return;

            if (appearance.AttackColor.HasValue)
                info.attackTextColor = appearance.AttackColor.Value;
            else
                info.attackTextColor = drsl.defaultLightColor;

            if (appearance.HealthColor.HasValue)
                info.healthTextColor = appearance.HealthColor.Value;
            else
                info.healthTextColor = drsl.defaultLightColor;

            if (appearance.DefaultAbilityColor.HasValue)
                info.defaultAbilityColor = appearance.DefaultAbilityColor.Value;
            else
                info.defaultAbilityColor = drsl.defaultLightColor;

            if (appearance.PortraitColor.HasValue)
                info.portraitColor = appearance.PortraitColor.Value;
            else
                info.portraitColor = drsl.defaultLightColor;
            
            if (appearance.NameTextColor.HasValue)
            {
                info.nameTextColor = appearance.NameTextColor.Value; 

                // Do it here too?
                drsl.labelText.fontMaterial.SetTexture("_MainTex", WhiteTextTexture);
                drsl.labelText.renderer.material.SetTexture("_MainTex", WhiteTextTexture);
                string hexcode = ColorUtility.ToHtmlStringRGBA(appearance.NameTextColor.Value);
                drsl.labelText.SetText($"<color=#{hexcode}>{appearance.Card.Info.DisplayedNameLocalized}</color>", true);
            }
        }
    }
}