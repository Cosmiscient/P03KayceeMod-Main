using System;
using System.Collections.Generic;
using System.IO;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class RareDiscCardAppearance : DiscCardColorAppearance
    {
        private static bool RGBIsActive => SaveManager.SaveFile.unlockedAchievements.Contains(P03AchievementManagement.SKULLSTORM) || P03Plugin.Instance.DebugCode.Contains("rgb");

        private static readonly Dictionary<string, Color?> configColors = new();
        private static readonly Dictionary<string, Color> defaultColors = new()
        {
            {"BorderColor", new(1f, 0f, 0f) },
            {"PortraitColor", new(1f, 0.33984375f, 0.19921875f) },
            {"AttackColor", new(1f, 0.33984375f, 0.19921875f) },
            {"HealthColor", new(1f, 0.33984375f, 0.19921875f) },
            {"EnergyColor", new(1f, 0.33984375f, 0.19921875f) },
            {"DefaultAbilityColor", new(1f, 0.33984375f, 0.19921875f) },
            {"NameBannerColor", new(0.5f, 0.5f, 0.5f)},
            {"NameTextColor", new Color(0f, 0f, 0f, 1f)}
        };
        private static readonly Dictionary<string, Color> defaultRGBColors = new()
        {
            {"BorderColor", Color.black },
            {"PortraitColor", new(1f, 1f, 1f) },
            {"AttackColor", new(1f, 1f, 1f) },
            {"HealthColor", new(1f, 1f, 1f) },
            {"EnergyColor", new(1f, 1f, 1f) },
            {"DefaultAbilityColor", new(1f, 1f, 1f) },
            {"NameBannerColor", new(0.5f, 0.5f, 0.5f)},
            {"NameTextColor", new Color(0f, 0f, 0f, 1f)}
        };

        public override bool EmissiveBorderTexture { get => true; set => base.EmissiveBorderTexture = value; }

        private static Color? ConfigFileColor(string key)
        {
            if (configColors.ContainsKey(key))
                return configColors[key];

            key = $"rare{key.ToLowerInvariant()}=[";

            try
            {
                int start = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf(key) + key.Length;
                int end = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf("]", start);
                configColors[key] = GetColorFromString(P03Plugin.Instance.DebugCode.ToLowerInvariant().Substring(start, end - start));
            }
            catch (Exception e)
            {
                P03Plugin.Log.LogDebug($"Could not find {key} in {P03Plugin.Instance.DebugCode}: {e.Message}");
                configColors[key] = null;
            }
            return configColors[key];
        }

        protected override Color? LookupColor(string key)
        {
            Color? c = ConfigFileColor(key);
            if (c.HasValue)
                return c;

            var lookup = RGBIsActive ? defaultRGBColors : defaultColors;
            return lookup.ContainsKey(key) ? lookup[key] : null;
        }

        public static new Appearance ID { get; private set; }

        static RareDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RareDiscAppearance", typeof(RareDiscCardAppearance)).Id;
        }

        public void ReRenderCard() => Card.RenderCard();

        public void SetBorderColor(Color color)
        {
            BorderColor = color;
            ReRenderCard();
        }

        public void SetPortraitColor(Color color)
        {
            PortraitColor = color;
            ReRenderCard();
        }

        public void SetEnergyColor(Color color)
        {
            EnergyColor = color;
            ReRenderCard();
        }

        public void SetNameTextColor(Color color)
        {
            NameTextColor = color;
            ReRenderCard();
        }

        public void SetNameBannerColor(Color color)
        {
            NameBannerColor = color;
            ReRenderCard();
        }

        public void SetAttackColor(Color color)
        {
            AttackColor = color;
            ReRenderCard();
        }

        public void SetHealthColor(Color color)
        {
            HealthColor = color;
            ReRenderCard();
        }

        public void SetDefaultAbilityColor(Color color)
        {
            DefaultAbilityColor = color;
            ReRenderCard();
        }

        internal static readonly Gradient RGB_GRADIENT = new Gradient()
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(new (1f, 0f, 0f), 0f),
                new GradientColorKey(new (1f, 1f, 0f), 0.3f),
                new GradientColorKey(new (0f, 1f, 0f), 0.5f),
                new GradientColorKey(new (0f, 1f, 1f), 0.7f),
                new GradientColorKey(new (0f, 0f, 1f), 0.9f),
                new GradientColorKey(new (1f, 0f, 1f), 1f),
            }
        };

        private const float FULL_SIZE = 250f + 365f;

        private static readonly Texture2D SPECULAR_MAP = TextureHelper.GetImageAsTexture("rare_specular_fractal.png", typeof(RareDiscCardAppearance).Assembly, FilterMode.Trilinear);

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.AssignCopiedRenderTexture))]
        [HarmonyPostfix]
        private static void SpecularCard(RenderStatsLayer __instance, Texture tex, bool emission)
        {
            if (RGBIsActive && emission && __instance is DiskRenderStatsLayer drsl && tex is Texture2D texture)
            {
                Card card = drsl.gameObject.GetComponentInParent<Card>();
                if (card != null && card.Info.appearanceBehaviour.Contains(ID))
                {
                    drsl.Material.SetTexture("_MetallicGlossMap", SPECULAR_MAP);
                }
            }
        }

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.AssignCopiedRenderTexture))]
        [HarmonyPrefix]
        private static void RBGifyCard(RenderStatsLayer __instance, Texture tex, bool emission)
        {
            if (RGBIsActive && emission && __instance is DiskRenderStatsLayer drsl && tex is Texture2D texture)
            {
                Card card = drsl.gameObject.GetComponentInParent<Card>();
                if (card != null && card.Info.appearanceBehaviour.Contains(ID))
                {
                    for (int x = 0; x < tex.width; x++)
                    {
                        for (int y = 0; y < tex.height; y++)
                        {
                            Color ex = texture.GetPixel(x, y);
                            if (ex != Color.black)
                            {
                                Color newColor = RGB_GRADIENT.Evaluate(((float)(x + y)) / FULL_SIZE);
                                newColor *= ex;
                                newColor.a = ex.a;
                                texture.SetPixel(x, y, newColor);
                            }
                        }
                    }
                    texture.Apply();
                }
                // string filename = $"cardexports/{tex.name}.png";
                // File.WriteAllBytes(filename, ImageConversion.EncodeToPNG(tex as Texture2D));
            }
        }

    }
}