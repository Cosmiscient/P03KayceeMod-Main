using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
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
                if (CardManager.AllCardsCopy.Any(c => c.name.Equals(name)))
                    return CardLoader.GetCardByName(name);
                return null;
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
            var retval = ScriptableObject.CreateInstance<StarterDeckInfo>();
            retval.name = $"P03_{title}";
            retval.title = title;
            retval.iconSprite = Sprite.Create(icon, new Rect(0f, 0f, 35f, 44f), new Vector2(0.5f, 0.5f));
            retval.cards = cards.Select(FixCardName).ToList();
            return retval;
        }

        public static void RegisterStarterDecks()
        {
            DEFAULT_STARTER_DECK = StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Vanilla", "starterdeck_icon_vanilla", new string[] { "BatteryBot", "Sniper", "Shieldbot", "CloserBot" })).Info.name;
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Bomb", "starterdeck_icon_bomb", new string[] { "Bombbot", "Bombbot", "LatcherBomb", "BombMaiden" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Shield", "starterdeck_icon_shield", new string[] { "LatcherShield", "Shieldbot", "Shieldbot", "Steambot" }), unlockLevel: 2);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("FullDraft", "starterdeck_icon_token", new string[] { CustomCards.DRAFT_TOKEN, CustomCards.DRAFT_TOKEN, CustomCards.DRAFT_TOKEN, CustomCards.UNC_TOKEN }), unlockLevel: 4);

            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Movement", "starterdeck_icon_strafe", new string[] { "ConveyorLatcher", "SeedBot", "MineCart", "BoltHound" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Annoying", "starterdeck_icon_annoying", new string[] { "AlarmBot", "AlarmBot", "Clockbot", "Clockbot" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Conduit", "starterdeck_icon_conduit", new string[] { "StarterConduitTower", "CellGift", "FrankenBot", "HealerConduit" }), unlockLevel: 2);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Evolve", "starterdeck_icon_evolve", new string[] { "XformerBatBot", "XformerPorcupineBot", "ViperBot", "XformerGrizzlyBot" }), unlockLevel: 4);

            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Fire", "starterdeck_icon_fire", new string[] { "Molotov", "FlamingExeskeleton", "PyroBot", "StreetSweeper" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Stall", "starterdeck_icon_slowdown", new string[] { "SentryBot", "BustedPrinter", "RobotRam", "Spyplane" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Recharge", "starterdeck_icon_recharge", new string[] { "Gopher", "Weeper", "Encapsulator", "RoboMice" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("NoCharge", "starterdeck_icon_glow", new string[] { "GlowBot", "GlowBot", "CellBuff", "Suicell" }), unlockLevel: 8);

            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Swap", "starterdeck_icon_swap", new string[] { "AmmoBot", "Poodle", "SwapperLatcher", "SwapBot" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Nefarious", "starterdeck_icon_combo", new string[] { "RoboSkeleton", "EnergyVampire", "LatcherBrittle", "GiveAWay" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Cheapo", "starterdeck_icon_cheap", new string[] { "BoxBot", $"{ExpansionPackCards_2.EXP_2_PREFIX}_Librarian", "JimmyJr", "PitySeeker" }), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Trash", "starterdeck_icon_doctor", new string[] { $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon", "LeapBot", "RoboRingworm", "DrZambot" }), unlockLevel: 8);


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

