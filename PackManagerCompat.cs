using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using Infiniscryption.PackManagement;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;

namespace Infiniscryption.PackManagerP03Plugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency(PackPluginGuid)]
    [BepInDependency(P03PluginGuid)]
    public class PackPlugin : BaseUnityPlugin
    {
        public const string P03PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
        public const string PackPluginGuid = "zorro.inscryption.infiniscryption.packmanager";

        public const string PluginGuid = "zorro.inscryption.infiniscryption.packmanager.p03plugin";
		public const string PluginName = "Infiniscryption Pack Manager - P03 Plugin";
		public const string PluginVersion = "2.0";

        internal static ManualLogSource Log;

        public static CardTemple ScreenState 
        { 
            get
            {
                string value = ModdedSaveManager.SaveData.GetValue(P03PluginGuid, "ScreenState");
                if (string.IsNullOrEmpty(value))
                    return CardTemple.Nature;

                return (CardTemple)Enum.Parse(typeof(CardTemple), value);
            }
        }

        private void Awake()
        {
            Log = base.Logger;

            // Start by creating the pack:
            PackInfo packInfo = PackManager.GetDefaultPackInfo(CardTemple.Tech);
            packInfo.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);

            // Awesome! Since there hasn't been an error, I can start modifying cards:
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                if (ScreenState == CardTemple.Nature && PackManager.GetActivePacks().Contains(packInfo))
                    foreach (CardInfo card in cards)
                        if (card.temple == CardTemple.Tech)
                            if (!card.metaCategories.Contains(CardMetaCategory.Rare))
                                if (card.metaCategories.Contains(CardMetaCategory.TraderOffer))
                                    card.AddMetaCategories(CardMetaCategory.ChoiceNode);

                return cards;
            };

            // Also tell the pack manager about our metacategories
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "NeutralRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "WizardRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "TechRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "NatureRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "UndeadRegionCards"));

            // Expansion pack
            PackInfo expPack1 = PackManager.GetPackInfo("P03KCMXP1");
            expPack1.Title = "Kaycee's P03 Expansion Pack #1";
            expPack1.Description = "The first expansion pack from the developers of 'P03 in Kaycees Mod' adds [count] new cards across all four regions of Botopia.";
            expPack1.ValidFor.Clear();
            expPack1.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            expPack1.SetTexture(TextureHelper.GetImageAsTexture("expansion1.png", typeof(PackPlugin).Assembly));

            PackInfo expPack2 = PackManager.GetPackInfo("P03KCMXP2");
            expPack2.Title = "Kaycee's P03 Expansion Pack #2 [EARLY ACCESS]";
            expPack2.Description = "Still in development! The second official expansion pack, with [count] new cards that are zanier than ever.";
            expPack2.ValidFor.Clear();
            expPack2.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            expPack2.SetTexture(TextureHelper.GetImageAsTexture("PKCMexpansion2pack.png", typeof(PackPlugin).Assembly));

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}