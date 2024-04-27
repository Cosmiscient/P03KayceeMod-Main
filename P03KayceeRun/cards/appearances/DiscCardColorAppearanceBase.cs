using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class DiscCardColorAppearance : CardAppearanceBehaviour
    {
        private readonly Dictionary<string, Color?> _setColors = new();

        private readonly Dictionary<bool, Dictionary<string, Color?>> _cachedColors = new() { { true, new() }, { false, new() } };

        protected virtual Color? LookupColor(string key) => null;

        private Card _cacheCard = null;
        private PlayableCard _cachedPlayableCard = null;
        protected bool IsOpponentAppearance
        {
            get
            {
                if (_cacheCard == null)
                {
                    _cacheCard = this.Card;
                    _cachedPlayableCard = this.Card as PlayableCard;
                }
                return _cachedPlayableCard != null && _cachedPlayableCard.OpponentCard;
            }
        }

        private Color? GetColor(string key)
        {
            if (_setColors.ContainsKey(key))
                return _setColors[key];

            var cache = _cachedColors[IsOpponentAppearance];

            if (cache.ContainsKey(key))
                return cache[key];

            cache[key] = LookupColor(key);

            if (!cache[key].HasValue)
                cache[key] = GetColorFromString(Card.Info.GetExtendedProperty(key));

            return cache[key];
        }

        private bool? _holofy = null;
        private bool? _emissiveBorderTex = null;

        public virtual Color? BorderColor { get => GetColor("BorderColor"); set => _setColors["BorderColor"] = value; }
        public virtual Color? PortraitColor { get => GetColor("PortraitColor"); set => _setColors["PortraitColor"] = value; }
        public virtual Color? EnergyColor { get => GetColor("EnergyColor"); set => _setColors["EnergyColor"] = value; }
        public virtual Color? NameTextColor { get => GetColor("NameTextColor"); set => _setColors["NameTextColor"] = value; }
        public virtual Color? NameBannerColor { get => GetColor("NameBannerColor"); set => _setColors["NameBannerColor"] = value; }
        public virtual Color? AttackColor { get => GetColor("AttackColor"); set => _setColors["AttackColor"] = value; }
        public virtual Color? HealthColor { get => GetColor("HealthColor"); set => _setColors["HealthColor"] = value; }
        public virtual Color? DefaultAbilityColor { get => GetColor("DefaultAbilityColor"); set => _setColors["DefaultAbilityColor"] = value; }
        public virtual bool HolofyBorder { get => _holofy.GetValueOrDefault(Card.Info.GetExtendedPropertyAsBool("Holofy").GetValueOrDefault(false)); set => _holofy = value; }
        public virtual bool EmissiveBorderTexture { get => _emissiveBorderTex.GetValueOrDefault(Card.Info.GetExtendedPropertyAsBool("EmissiveBorderTexture").GetValueOrDefault(false)); set => _emissiveBorderTex = value; }

        internal static readonly Dictionary<Color, Texture2D> DiskTextures = new();

        internal static Texture2D WhiteTextTexture { get; set; }
        internal Texture2D CurrentTexture { get; set; }

        private static Dictionary<string, Color> GameColorsCache;

        public static Appearance ID { get; private set; }

        static DiscCardColorAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ColoredDiscAppearance", typeof(DiscCardColorAppearance)).Id;

            // I'm going to manually load this texture; it seems to have some issues
            DiskTextures[GameColors.Instance.darkGold] = new Texture2D(2, 2, TextureFormat.DXT1, false);
            byte[] imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Gold_2048.png", typeof(DiscCardColorAppearance).Assembly);
            bool isLoaded = DiskTextures[GameColors.Instance.darkGold].LoadImage(imgBytes);
            DiskTextures[GameColors.Instance.darkGold].filterMode = FilterMode.Point;
            DiskTextures[GameColors.Instance.darkGold].name = "GoldDiskTexture";

            DiskTextures[GameColors.Instance.darkRed] = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Red_2048.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = DiskTextures[GameColors.Instance.darkRed].LoadImage(imgBytes);
            DiskTextures[GameColors.Instance.darkRed].filterMode = FilterMode.Point;
            DiskTextures[GameColors.Instance.darkRed].name = "RedDiskTexture";

            DiskTextures[GameColors.Instance.blue] = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_2048.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = DiskTextures[GameColors.Instance.blue].LoadImage(imgBytes);
            DiskTextures[GameColors.Instance.blue].filterMode = FilterMode.Point;
            DiskTextures[GameColors.Instance.blue].name = "BlueDiskTexture";

            DiskTextures[Color.black] = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("FloppyDisc_AlbedoTransparency_Black_2048.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = DiskTextures[Color.black].LoadImage(imgBytes);
            DiskTextures[Color.black].filterMode = FilterMode.Point;
            DiskTextures[Color.black].name = "BlackDiskTexture";

            WhiteTextTexture = new Texture2D(2, 2, TextureFormat.DXT1, false);
            imgBytes = TextureHelper.GetResourceBytes("heavyweight-white.png", typeof(DiscCardColorAppearance).Assembly);
            isLoaded = WhiteTextTexture.LoadImage(imgBytes);
            WhiteTextTexture.filterMode = FilterMode.Bilinear;
        }

        private static float ColorDistance(Color a, Color b)
        {
            float fbar = 0.5f * (a.r + b.r);
            float deltar = (float)(a.r - b.r);
            float deltag = (float)(a.g - b.g);
            float deltab = (float)(a.b - b.b);
            return fbar < 128
                ? Mathf.Sqrt((2 * deltar * deltar) + (4 * deltag * deltag) + (3 * deltab * deltab))
                : Mathf.Sqrt((3 * deltar * deltar) + (4 * deltag * deltag) + (2 * deltab * deltab));
        }

        private static void PopulateGameColorsCache()
        {
            if (GameColorsCache != null)
                return;

            GameColorsCache = new();

            foreach (System.Reflection.FieldInfo field in typeof(GameColors).GetFields())
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
            if (!BorderColor.HasValue)
            {
                CurrentTexture = DiskTextures[GameColors.Instance.blue];
                return;
            }

            var key = DiskTextures.Keys.OrderBy(k => ColorDistance(k, BorderColor.Value)).First();
            CurrentTexture = DiskTextures[key];
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
                        return ColorUtility.TryParseHtmlString(keyComponents[0], out Color htmlColor) ? htmlColor : null;
                    }
                    PopulateGameColorsCache();
                    return GameColorsCache[keyComponents[0].ToLowerInvariant()];
                }
                else
                {
                    return keyComponents.Length == 2
                        ? GetColorFromString(keyComponents[0]) * float.Parse(keyComponents[1], CultureInfo.InvariantCulture)
                        : keyComponents.Length == 3
                                            ? new Color(float.Parse(keyComponents[0], CultureInfo.InvariantCulture), float.Parse(keyComponents[1], CultureInfo.InvariantCulture), float.Parse(keyComponents[2], CultureInfo.InvariantCulture))
                                            : keyComponents.Length == 4
                                                                ? new Color(float.Parse(keyComponents[0], CultureInfo.InvariantCulture), float.Parse(keyComponents[1], CultureInfo.InvariantCulture), float.Parse(keyComponents[2], CultureInfo.InvariantCulture), float.Parse(keyComponents[3], CultureInfo.InvariantCulture))
                                                                : null;
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
            "Anim/CardBase/Top/MetalSlider",
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

        private void ApplyColorSafe(string path, Color color, bool emission, bool holofy = false, Texture retexture = null, bool emissiveTexture = false)
        {
            try
            {
                Transform tComp = gameObject.transform.Find(path);
                if (tComp != null && tComp.gameObject != null)
                {
                    if (holofy)
                    {
                        OnboardDynamicHoloPortrait.HolofyGameObject(tComp.gameObject, color, inChildren: false, destroyComponents: false);
                        return;
                    }
                    GameObject component = tComp.gameObject;
                    MeshRenderer renderer = component.GetComponent<MeshRenderer>();
                    Material material = renderer.material;
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

                    if (retexture != null)
                    {
                        material.mainTexture = retexture;
                    }

                    if (emission)
                    {
                        material.EnableKeyword("_EMISSION");

                        if (emissiveTexture)
                        {
                            if (retexture == null)
                                material.SetTexture("_EmissionMap", material.GetTexture("_MainTex"));
                            else
                                material.SetTexture("_EmissionMap", retexture);
                        }
                        else
                        {
                            material.SetColor("_EmissionColor", color);
                        }

                        if (retexture != null)
                            material.SetColor("_Color", Color.black);
                    }
                    else
                    {
                        material.SetColor("_Color", color);
                    }
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public override void ApplyAppearance()
        {
            // Swap out textures as appropriate
            if (CurrentTexture == null)
                CalcRareTexture();

            MaterialHelper.RetextureAllRenderers(gameObject, CurrentTexture, originalTextureKey: "floppydisc");

            // Apply the color to the border
            if (BorderColor.HasValue)
            {
                foreach (string key in BorderObjectPaths)
                    ApplyColorSafe(key, BorderColor.Value, true, HolofyBorder, emissiveTexture: EmissiveBorderTexture);
            }

            // Apply the color to the name background
            if (NameBannerColor.HasValue)
            {
                foreach (string key in StickerObjectPaths)
                    ApplyColorSafe(key, NameBannerColor.Value, true, retexture: DiskTextures[GameColors.Instance.blue]);
            }

            if (NameTextColor.HasValue && Card.StatsLayer is DiskRenderStatsLayer drsl)
            {
                if (EnergyColor.HasValue)
                    drsl.lightColor = EnergyColor.Value;

                if (drsl.labelText.fontMaterial.shader.name.EndsWith("(Surface)"))
                    drsl.labelText.fontMaterial.shader = Shader.Find(drsl.labelText.fontMaterial.shader.name.Replace(" (Surface)", ""));

                if (drsl.labelText.renderer.material.shader.name.EndsWith("(Surface)"))
                    drsl.labelText.renderer.material.shader = Shader.Find(drsl.labelText.renderer.material.shader.name.Replace(" (Surface)", ""));

                drsl.labelText.fontMaterial.SetTexture("_MainTex", WhiteTextTexture);
                drsl.labelText.renderer.material.SetTexture("_MainTex", WhiteTextTexture);
                string hexcode = ColorUtility.ToHtmlStringRGBA(NameTextColor.Value);
                drsl.labelText.SetText($"<color=#{hexcode}>{Card.Info.DisplayedNameLocalized}</color>", true);
            }
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            ApplyAppearance();
        }

        //private static readonly Color _defaultBarColor = new Color(.4858f, .8751f, 1f, 1f);

        [HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.UpdateTextureWhenReady))]
        [HarmonyPostfix]
        private static IEnumerator FixMiddleColor(IEnumerator sequence, CardRenderCamera __instance, RenderStatsLayer layer, CardRenderInfo info, PlayableCard playableCard, bool updateMain, bool updateEmission)
        {
            if (!P03AscensionSaveData.IsP03Run || layer == null || layer is not DiskRenderStatsLayer drsl)
            {
                yield return sequence;
                yield break;
            }

            DiscCardColorAppearance appearance = drsl.GetComponentInParent<DiscCardColorAppearance>();//drsl.transform.parent.parent.GetComponent<DiscCardColorAppearance>();

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
                Transform delimiter = __instance.transform.Find("CardsPlane/Base/Delimiter");
                if (delimiter != null)
                {
                    SpriteRenderer renderer = delimiter.GetComponent<SpriteRenderer>();
                    renderer.color = myBarColor;
                }
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
            if (!P03AscensionSaveData.IsP03Run || __instance is not DiskRenderStatsLayer drsl)
                return;

            DiscCardColorAppearance appearance = drsl.GetComponentInParent<DiscCardColorAppearance>();//drsl.gameObject.transform.parent.parent.gameObject.GetComponent<DiscCardColorAppearance>();

            if (appearance == null)
                return;

            info.attackTextColor = appearance.AttackColor ?? drsl.defaultLightColor;

            info.healthTextColor = appearance.HealthColor ?? drsl.defaultLightColor;

            info.defaultAbilityColor = appearance.DefaultAbilityColor ?? drsl.defaultLightColor;

            info.portraitColor = appearance.PortraitColor ?? drsl.defaultLightColor;

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