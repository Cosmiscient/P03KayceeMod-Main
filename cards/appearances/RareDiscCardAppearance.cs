using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RareDiscCardAppearance : DiscCardColorAppearance
    {
        private static Dictionary<string, Color?> configColors = new ();
        private static readonly Dictionary<string, Color> defaultColors = new ()
        {
            {"BorderColor", GameColors.Instance.darkGold * 0.33f},
            {"NameBannerColor", new Color(1f, 1f, 1f, 1f)},
            {"NameTextColor", new Color(0f, 0f, 0f, 1f)}
        };

        private static Color? ConfigFileColor(string key)
        {
            if (configColors.ContainsKey(key))
                return configColors[key];

            key = $"rare{key.ToLowerInvariant()}=[";

            try
            {
                int start = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf(key) + key.Length;
                int end = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf("]", start);
                configColors[key] = GetColorFromString(P03Plugin.Instance.DebugCode.ToLowerInvariant().Substring(start, end-start));
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

            if (defaultColors.ContainsKey(key))
                return defaultColors[key];

            return null;
        }

        public new static CardAppearanceBehaviour.Appearance ID { get; private set; }

        static RareDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RareDiscAppearance", typeof(RareDiscCardAppearance)).Id;
        }

        public void ReRenderCard()
        {
            this.Card.RenderCard();
        }

        public void SetBorderColor(Color color)
        {
            this.BorderColor = color;
            this.ReRenderCard();
        }

        public void SetPortraitColor(Color color)
        {
            this.PortraitColor = color;
            this.ReRenderCard();
        }
        
        public void SetEnergyColor(Color color)
        {
            this.EnergyColor = color;
            this.ReRenderCard();
        }
        
        public void SetNameTextColor(Color color)
        {
            this.NameTextColor = color;
            this.ReRenderCard();
        }
        
        public void SetNameBannerColor(Color color)
        {
            this.NameBannerColor = color;
            this.ReRenderCard();
        }
        
        public void SetAttackColor(Color color)
        {
            this.AttackColor = color;
            this.ReRenderCard();
        }
        
        public void SetHealthColor(Color color)
        {
            this.HealthColor = color;
            this.ReRenderCard();
        }
        
        public void SetDefaultAbilityColor(Color color)
        {
            this.DefaultAbilityColor = color;
            this.ReRenderCard();
        }
        
    }
}