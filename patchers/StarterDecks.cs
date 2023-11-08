using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Ascension;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class StarterDecks
    {
        public static string DEFAULT_STARTER_DECK { get; private set; }
        private static string[] PREFIXES = {
            P03Plugin.CardPrefx,
            ExpansionPackCards_1.EXP_1_PREFIX,
            ExpansionPackCards_2.EXP_2_PREFIX
        };

        private static CardInfo FriendlyGet(string name)
        {
            try
            {
                return CardLoader.GetCardByName(name);
            }
            catch
            {
                return null;
            }
        }

        private static CardInfo FixCardName(string name)
        {
            var nextCard = FriendlyGet(name);
            if (nextCard != null)
                return nextCard;

            foreach (var prefix in PREFIXES)
            {
                nextCard = FriendlyGet($"{prefix}_{name}");
                if (nextCard != null)
                    return nextCard;
            }
            throw new KeyNotFoundException($"Cannot find a card named {name}");
        }

        private static StarterDeckInfo CreateStarterDeckInfo(string title, string iconKey, string[] cards)
        {
            Texture2D icon = TextureHelper.GetImageAsTexture($"{iconKey}.png", typeof(StarterDecks).Assembly);
            return new()
            {
                name = $"P03_{title}",
                title = title,
                iconSprite = Sprite.Create(icon, new Rect(0f, 0f, 35f, 44f), new Vector2(0.5f, 0.5f)),
                cards = cards.Select(FixCardName).ToList()
            };


        }

        public static void RegisterStarterDecks()
        {
            DEFAULT_STARTER_DECK = StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Snipers", "starterdeck_icon_snipers", new string[] { "Sniper", "BustedPrinter", "SentryBot" })).Info.name;
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Random", "starterdeck_icon_random", new string[] { "Amoebot", "GiftBot", "GiftBot" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Shield", "starterdeck_icon_shield", new string[] { "GemShielder", "Shieldbot", "LatcherShield" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Energy", "starterdeck_icon_energy", new string[] { "CloserBot", "BatteryBot", "BatteryBot" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Conduit", "starterdeck_icon_conduit", new string[] { "CellTri", "CellBuff", "HealerConduit" }), unlockLevel: 4);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Nature", "starterdeck_icon_evolve", new string[] { "XformerGrizzlyBot", "XformerBatBot", "XformerPorcupineBot" }), unlockLevel: 4);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Gems", "starterdeck_icon_gems", new string[] { "SentinelBlue", "SentinelGreen", "SentinelOrange" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("FullDraft", "starterdeck_icon_token", new string[] { CustomCards.UNC_TOKEN, CustomCards.DRAFT_TOKEN, CustomCards.DRAFT_TOKEN }), unlockLevel: 8);

            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Fire", "starterdeck_icon_fire", new string[] { "PyroBot", "Molotov", "StreetSweeper" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Bomb", "starterdeck_icon_bomb", new string[] { "Bombbot", "LatcherBomb", "Suicell" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Annoying", "starterdeck_icon_annoying", new string[] { "AlarmBot", "Clockbot", "Gopher" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Recharge", "starterdeck_icon_recharge", new string[] { "Weeper", "Encapsulator", "RoboMice" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Helpers", "starterdeck_icon_helpers", new string[] { "AmmoBot", "GlowBot", "PitySeeker" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Cheapo", "starterdeck_icon_cheap", new string[] { "Librarian", "BoxBot", "JimmyJr" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Latchers", "starterdeck_icon_latchers", new string[] { "ConveyorLatcher", "LatcherBrittle", "SwapperLatcher" }), unlockLevel: 8);

            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Replicas", "starterdeck_icon_replicas", new string[] { "BleeneAcolyte", "OrluAcolyte", "GoranjAcolyte" }), unlockLevel: 8);


            StarterDeckManager.ModifyDeckList += delegate (List<StarterDeckManager.FullStarterDeck> decks)
            {
                CardTemple acceptableTemple = ScreenManagement.ScreenState;

                // Only keep decks where at least one card belongs to this temple
                decks.RemoveAll(info => info.Info.cards.FirstOrDefault(ci => ci.temple == acceptableTemple) == null);

                return decks;
            };
        }
    }
}

