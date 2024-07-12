using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03ExpansionPack3.Sigils;
using Infiniscryption.P03SigilLibrary.Sigils;

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

            // Stimulator
            CardManager.New(P03Pack3Plugin.CardPrefix, "Stimulator", "Stimulator", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_stimulator.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 3)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(ActivateEverything.AbilityID);

            // Gamer
            CardManager.New(P03Pack3Plugin.CardPrefix, "Gamer", "GameKid", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gamer.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 5)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.ActivatedDealDamage, ActivatedStrafeSelf.AbilityID);

            // Green Energy Bot
            CardManager.New(P03Pack3Plugin.CardPrefix, "GreenEnergyBot", "Green Energy Bot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_battery_mage.png", typeof(Cards).Assembly))
                .AddPart3Decal(TextureHelper.GetImageAsTexture("decal_battery_mage.png", typeof(Cards).Assembly))
                .SetCost(gemsCost: new() { GemType.Green })
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(Ability.GainBattery, Ability.GemDependant);

            // Mortar Droid
            CardManager.New(P03Pack3Plugin.CardPrefix, "MortarDroid", "Mortar Droid", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mortar_droid.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 3)
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

            // Mucus Launcher
            CardManager.New(P03Pack3Plugin.CardPrefix, "MucusLauncher", "Mucus Launcher", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mucus_launcher.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(ThrowSlime.AbilityID);

            // General Gunk
            CardManager.New(P03Pack3Plugin.CardPrefix, "GeneralGunk", "General Gunk", 2, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_general_gunk.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 2)
                .SetRegionalP03Card(CardTemple.Nature)
                .SetRare()
                .AddAbilities(ThrowSlimeAll.AbilityID);

            // Gachabomb
            CardManager.New(P03Pack3Plugin.CardPrefix, "Gachabomb", "Gachabomb", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gachabomb.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(Explodonate.AbilityID, Lootbox.AbilityID);

            // Submariner
            CardManager.New(P03Pack3Plugin.CardPrefix, "Submariner", "Submariner", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_submariner.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 5)
                .SetNeutralP03Card()
                .SetStartingFuel(2)
                .AddAbilities(ActivatedSubmerge.AbilityID);

            // Rot Latcher
            CardManager.New(P03Pack3Plugin.CardPrefix, "RotLatcher", "Rot Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_rot_latcher.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 2)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(LatchStinky.AbilityID);

            // Time Latcher
            CardManager.New(P03Pack3Plugin.CardPrefix, "TimeLatcher", "Time Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_time_latcher.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(gemsCost: new() { GemType.Blue })
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(LatchAnnoying.AbilityID);

            // Apedroid
            CardManager.New(P03Pack3Plugin.CardPrefix, "Apedroid", "Apedroid", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_apedroid.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Fling.AbilityID);

            // Synthesioid
            CardManager.New(P03Pack3Plugin.CardPrefix, "Synthesioid", "Synthesioid", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_synthesioid.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(SacrificeMorsel.AbilityID);

            // RemoteDetonator
            CardManager.New(P03Pack3Plugin.CardPrefix, "RemoteDetonator", "Remote Detonator", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_remote_detonator.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetNeutralP03Card()
                .AddAbilities(SacrificeExplode.AbilityID);

            // Scavenger
            CardManager.New(P03Pack3Plugin.CardPrefix, "Scavenger", "Scavenger", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_scavenger.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 3)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(ScrapSalvage.AbilityID);

            // Skin Droid
            CardManager.New(P03Pack3Plugin.CardPrefix, "SkinDroid", "Skin Droid", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skin_droid.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddSpecialAbilities(CopySacrificeIceCube.AbilityID)
                .AddAbilities(Ability.IceCube);

            // Skin Droid
            CardManager.New(P03Pack3Plugin.CardPrefix, "EmergenceLatcher", "Emergence Latcher", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_emergence_latcher.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(SacrificeLatch.AbilityID);

            // Cottagecog
            CardManager.New(P03Pack3Plugin.CardPrefix, "Cottagecog", "Cottagecog", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_cottagecog.png", typeof(Cards).Assembly))
                .AddDecal(
                    TextureHelper.GetImageAsTexture("decal_cottagecog_sapphire.png", typeof(Cards).Assembly),
                    TextureHelper.GetImageAsTexture("decal_cottagecog_ruby.png", typeof(Cards).Assembly)
                )
                .AddAppearances(ReplicaAppearanceBehavior.ID)
                .SetExtendedProperty(ReplicaAppearanceBehavior.REPLICA_TYPE, "orange")
                .SetCost(gemsCost: new() { GemType.Blue })
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(GemOrangePrinter.AbilityID);

            // Vessel Tentacle
            CardManager.New(P03Pack3Plugin.CardPrefix, "VesselTentacle", "56455353454C", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_vessel_tentacle.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .SetStatIcon(SideDeckPower.AbilityID);

            // Baristabot
            CardManager.New(P03Pack3Plugin.CardPrefix, "Baristabot", "Baristabot", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_baristabot.png", typeof(Cards).Assembly))
                .SetCost(gemsCost: new() { GemType.Orange })
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(GemBluePurist.AbilityID);

            // Mystery MAchine
            CardManager.New(P03Pack3Plugin.CardPrefix, "MysteryMachine", "Mystery Machine", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mystery_machine.png", typeof(Cards).Assembly))
                .SetCost(gemsCost: new() { GemType.Blue, GemType.Green })
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.RandomAbility, RandomRareAbility.AbilityID);
        }
    }
}