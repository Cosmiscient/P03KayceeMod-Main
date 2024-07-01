using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03ExpansionPack3.Sigils;
using Infiniscryption.Spells.Sigils;
using Infiniscryption.P03ExpansionPack3.Managers;

namespace Infiniscryption.P03ExpansionPack3
{
    public static class Cards
    {
        public const string BLAST_CARD = "P03KCMXP3_BLAST";

        static Cards()
        {
            // Unpacker
            CardManager.New(P03Pack3Plugin.CardPrefix, "Unpacker", "Unpacker", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_unpacker.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 6)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(ActivatedGainItem.AbilityID);

            // Gunbot
            CardManager.New(P03Pack3Plugin.CardPrefix, "Gunbot", "Gunbot", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gunbot.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(TargetRequired.AbilityID);

            // Kraken
            CardManager.New(P03Pack3Plugin.CardPrefix, "Kraken", "KR4K3N", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_kraken.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Nature)
                .SetRare()
                .AddAbilities(KrakenTransformer.AbilityID);

            // Brain Cell
            CardManager.New(P03Pack3Plugin.CardPrefix, "BrainCell", "Brain Cell", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_brain_cell.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(CellGemDraw.AbilityID);

            // Sticker King
            CardManager.New(P03Pack3Plugin.CardPrefix, "StickerKing", "Sticker King", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_sticker_king.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .AddAbilities(GiveStickers.AbilityID);

            // Brain DRoid
            CardManager.New(P03Pack3Plugin.CardPrefix, "BrainDroid", "Brain Droid", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_brain_droid.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetNeutralP03Card();

            // Butane Launcher
            CardManager.New(P03Pack3Plugin.CardPrefix, "ButaneCaster", "Butane Caster", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_butane_caster.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 4)
                .SetNeutralP03Card()
                .AddAbilities(ThrowFire.AbilityID);

            // Remote Detonator
            CardManager.New(P03Pack3Plugin.CardPrefix, "RemoteDetonator", "Remote Detonator", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_remote_detonator.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 2)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(ActivateEverything.AbilityID);

            // Gamer
            CardManager.New(P03Pack3Plugin.CardPrefix, "Gamer", "Gamer", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gamer.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 5)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.ActivatedDealDamage, ActivatedStrafeSelf.AbilityID);

            // Battery Mage
            CardManager.New(P03Pack3Plugin.CardPrefix, "BatteryMage", "Battery Mage", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_battery_mage.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(gemsCost: new() { GemType.Green })
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(Ability.GainBattery, Ability.GemDependant);

            // Blast
            CardManager.New(P03Pack3Plugin.CardPrefix, "BLAST", "Blast!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_blast.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .SetSpellAppearanceP03()
                .AddAbilities(CatchFire.AbilityID);

            // Mortar Droid
            CardManager.New(P03Pack3Plugin.CardPrefix, "MortarDroid", "Mortar Droid", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mortar_droid.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 5)
                .SetNeutralP03Card()
                .SetStartingFuel(3)
                .AddAbilities(ActivatedDrawBlast.AbilityID);

            // Nitrous Dispenser
            CardManager.New(P03Pack3Plugin.CardPrefix, "NitrousTanker", "Nitrous Tanker", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_nitrous_dispenser.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 5)
                .SetNeutralP03Card()
                .SetStartingFuel(2)
                .AddAbilities(ActivatedBuffTeam.AbilityID);

            // Bone Mill
            CardManager.New(P03Pack3Plugin.CardPrefix, "BoneMill", "Bone Mill", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_bone_mill.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(ActivatedGainBattery.AbilityID);

            // Badass
            CardManager.New(P03Pack3Plugin.CardPrefix, "BadAss", "B4D A55", 2, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_badass.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 4)
                .SetNeutralP03Card()
                .SetStartingFuel(3)
                .AddAbilities(FuelRequired.AbilityID);
        }
    }
}