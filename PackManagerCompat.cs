using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
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
                return string.IsNullOrEmpty(value) ? CardTemple.Nature : (CardTemple)Enum.Parse(typeof(CardTemple), value);
            }
        }

        private void Awake()
        {
            Log = Logger;

            // Start by creating the pack:
            PackInfo packInfo = PackManager.GetDefaultPackInfo(CardTemple.Tech);
            packInfo.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);

            // Awesome! Since there hasn't been an error, I can start modifying cards:
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                if (ScreenState == CardTemple.Nature && PackManager.GetActivePacks().Contains(packInfo))
                {
                    foreach (CardInfo card in cards)
                    {
                        if (card.temple == CardTemple.Tech)
                        {
                            if (!card.metaCategories.Contains(CardMetaCategory.Rare))
                            {
                                if (card.metaCategories.Contains(CardMetaCategory.TraderOffer))
                                    card.AddMetaCategories(CardMetaCategory.ChoiceNode);
                            }
                        }
                    }
                }

                return cards;
            };

            // Also tell the pack manager about our metacategories
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "NeutralRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "WizardRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "TechRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "NatureRegionCards"));
            PackManager.TempleMetacategories[CardTemple.Tech].Add(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "UndeadRegionCards"));

            // Protect the NewBeastTransformer metacategory
            PackManager.AddProtectedMetacategory(GuidManager.GetEnumValue<CardMetaCategory>(P03PluginGuid, "NewBeastTransformers"));
            PackManager.AddProtectedMetacategory(CustomCards.MultiverseAbility);

            // Expansion pack
            PackInfo expPack1 = PackManager.GetPackInfo("P03KCMXP1");
            expPack1.Title = "Kaycee's P03 Expansion Pack #1";
            expPack1.Description = "The first expansion pack from the developers of 'P03 in Kaycees Mod' adds [count] new cards across all four regions of Botopia.";
            expPack1.ValidFor.Clear();
            expPack1.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            expPack1.SetTexture(TextureHelper.GetImageAsTexture("expansion1.png", typeof(PackPlugin).Assembly));

            PackInfo expPack2 = PackManager.GetPackInfo("P03KCMXP2");
            expPack2.Title = "Kaycee's P03 Expansion Pack #2";
            expPack2.Description = "The second official expansion pack, with [count] firey new cards that command an explosive reaction!";
            expPack2.ValidFor.Clear();
            expPack2.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            expPack2.SetTexture(TextureHelper.GetImageAsTexture("PKCMexpansion2pack.png", typeof(PackPlugin).Assembly));

            EncounterPackInfo defEncPack = PackManager.GetDefaultPackInfo<EncounterPackInfo>(CardTemple.Tech);
            defEncPack.Title = "Inscryption: Rogue Bots of Botopia";
            defEncPack.Description = "Botopia has become overrun with rogue bots! These [count] encounters have been rebalanced for Kaycee's Mod.";
            defEncPack.ValidFor.Clear();
            defEncPack.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            defEncPack.SetTexture(TextureHelper.GetImageAsTexture("p03_encounter_pack.png", typeof(PackPlugin).Assembly));

            EncounterPackInfo xp1EncPack = PackManager.GetPackInfo<EncounterPackInfo>("P03KCMXP1");
            xp1EncPack.Title = "Kaycee's P03 Encounter Expansion #1";
            xp1EncPack.Description = "[count] additional encounters that feature cards from the first official P03 expansion pack.";
            xp1EncPack.ValidFor.Clear();
            xp1EncPack.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            xp1EncPack.SetTexture(TextureHelper.GetImageAsTexture("p03_encounter_pack1.png", typeof(PackPlugin).Assembly));

            EncounterPackInfo xp2EncPack = PackManager.GetPackInfo<EncounterPackInfo>("P03KCMXP2");
            xp2EncPack.Title = "Kaycee's P03 Encounter Expansion #2";
            xp2EncPack.Description = "[count] additional encounters that feature cards from the second official P03 expansion pack.";
            xp2EncPack.ValidFor.Clear();
            xp2EncPack.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
            xp2EncPack.SetTexture(TextureHelper.GetImageAsTexture("p03_encounter_pack2.png", typeof(PackPlugin).Assembly));

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}