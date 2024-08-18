using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03ExpansionPack3.Sigils;
using Infiniscryption.P03SigilLibrary.Sigils;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Cards.Stickers;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03ExpansionPack3
{
    public static class Cards
    {
        public const string BLAST_CARD = "P03KCMXP3_BLAST";

        public static readonly Trait GunbotSwapTrait = GuidManager.GetEnumValue<Trait>(P03Pack3Plugin.PluginGuid, "GunbotSwap");

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

            // Brain DRoid
            CardManager.New(P03Pack3Plugin.CardPrefix, "BrainDroid", "Brain Droid", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_brain_droid.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetNeutralP03Card();

            // Danny
            CardManager.New(P03Pack3Plugin.CardPrefix, "Danny", "D.A.N.N.Y.", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_danny.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .AddAbilities(SummonGunbots.AbilityID)
                .AddSpecialAbilities(SwapWithGunbot.ID);

            // Butane Launcher
            CardManager.New(P03Pack3Plugin.CardPrefix, "ButaneCaster", "Butane Caster", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_butane_caster.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 4)
                .SetNeutralP03Card()
                .SetWeaponMesh(
                    "p03kcm/prefabs/flamethrower",
                    localPosition: new Vector3(0f, 0f, 0f),
                    localRotation: new Vector3(0f, 90f, 0f),
                    localScale: new Vector3(0.75f, 0.75f, 0.75f)
                )
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
                .SetWeaponMesh(
                    "p03kcm/prefabs/XCom_laserRifle_obj",
                    localPosition: new Vector3(0f, -0.66f, 0f),
                    localRotation: new Vector3(0f, 0f, 0f),
                    localScale: new Vector3(0.03f, 0.03f, 0.03f)
                )
                .AddAbilities(Ability.ActivatedDealDamage, ActivatedStrafeSelf.AbilityID);

            // Green Energy Bot
            CardManager.New(P03Pack3Plugin.CardPrefix, "GreenEnergyBot", "Green Energy Bot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_battery_mage.png", typeof(Cards).Assembly))
                .AddPart3Decal(TextureHelper.GetImageAsTexture("decal_battery_mage.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Green)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(Ability.GainBattery, Ability.GemDependant);

            // Mortar Droid
            CardManager.New(P03Pack3Plugin.CardPrefix, "MortarDroid", "Mortar Droid", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mortar_droid.png", typeof(Cards).Assembly))
                //.SetPixelPortrait(TextureHelpeer.GetImageAsTexture("pixelportrait_viper.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 5)
                .SetNeutralP03Card()
                .SetStartingFuel(3)
                .SetWeaponMesh(
                    "p03kcm/prefabs/flamethrower",
                    localPosition: new Vector3(0f, 0f, 0f),
                    localRotation: new Vector3(0f, 90f, 0f),
                    localScale: new Vector3(0.75f, 0.75f, 0.75f)
                )
                .AddAbilities(MissileStrike.AbilityID, FireBombWhenFueled.AbilityID);

            // Tow Truck
            CardManager.New(P03Pack3Plugin.CardPrefix, "TowTruck", "Tow Truck", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_tow_truck.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 5)
                .SetNeutralP03Card()
                .SetStartingFuel(2)
                .SetWeaponMesh(
                    "p03kcm/prefabs/Item_Hook",
                    localPosition: new Vector3(0f, -0.6f, 0.7f),
                    localRotation: new Vector3(330f, 0f, 180f),
                    localScale: new Vector3(0.007f, 0.013f, 0.007f),
                    disableMuzzleFlash: true,
                    audioId: "metal_object_hit#3"
                )
                .SetExtendedProperty("WeaponTowHook", true)
                .AddAbilities(ActivatedTemporaryControl.AbilityID);

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
                .SetGemsCost(GemType.Blue)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(LatchAnnoying.AbilityID);

            // Apedroid
            CardManager.New(P03Pack3Plugin.CardPrefix, "Apedroid", "Cat-A-Pult", 1, 1)
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
                .SetGemsCost(GemType.Blue)
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
                .SetGemsCost(GemType.Orange)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(GemBluePurist.AbilityID);

            // Mystery MAchine
            CardManager.New(P03Pack3Plugin.CardPrefix, "MysteryMachine", "Mystery Machine", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mystery_machine.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.RandomAbility, RandomRareAbility.AbilityID);

            // Sticker King
            CardManager.New(P03Pack3Plugin.CardPrefix, "StickerKing", "Sticker King", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_sticker_king.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .SetRare()
                .SetExtendedProperty("Stickers.Forced", "Stickers[StickerPositions:sticker_muscles,0.18,-0.35,0.00|sticker_revolver,0.47,-0.39,0.00|sticker_altcat,-0.09,0.00,0.00|sticker_winged_shoes,-0.18,0.13,0.00|sticker_battery,0.14,0.32,0.00|sticker_companion_cube,0.04,0.01,0.00|sticker_binary_ribbon,0.22,0.04,0.00|sticker_annoy_face,0.39,0.00,0.00|sticker_cowboy_hat,0.48,0.01,0.00][StickerRotations:sticker_muscles,0.00,180.00,100.86|sticker_revolver,0.00,180.00,133.56|sticker_battery,0.00,180.00,0][StickerScales:][StickerAbility:]")
                .AddAbilities(GiveStickers.AbilityID);

            // Custom event stuff for sticker king
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                // You have to have at least three of the king's stickers to see him:
                CardInfo king = cards.CardByName(P03Pack3Plugin.CardPrefix + "_StickerKing");
                Stickers.CardStickerData data = king.GetStickerData();
                data.FilterToUnlocked();
                if (data.Positions.Count < 3)
                    king.metaCategories.Clear();
                return cards;
            };

            // Booger Barrel
            CardManager.New(P03Pack3Plugin.CardPrefix, "BoogerBarrel", "Booger Barrel", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_booger_barrel.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 2)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(SacrificeSlime.AbilityID);

            // Artificer
            CardManager.New(P03Pack3Plugin.CardPrefix, "Artificer", "Artificer", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_artificer.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Orange, GemType.Blue)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(DrawThreeCommand.AbilityID);

            // Gear Shifter
            CardManager.New(P03Pack3Plugin.CardPrefix, "GearShifter", "Gear Shifter", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gear_shifter.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Blue, GemType.Green)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(Ability.StrafeSwap);

            // Shrinker
            CardManager.New(P03Pack3Plugin.CardPrefix, "Shrinker", "Shrinker", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_shrinker.png", typeof(Cards).Assembly))
                .SetAltPortrait(TextureHelper.GetImageAsTexture("portrait_shrinker_swap.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Green, GemType.Orange)
                .SetNeutralP03Card()
                .AddAbilities(Ability.SwapStats);

            // Magnus God
            CardManager.New(P03Pack3Plugin.CardPrefix, "MagnusGod", "Magnus God", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_magnus_god.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Orange, GemType.Blue, GemType.Green)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.TriStrike, Ability.Flying);

            // Open Sorcerer
            CardManager.New(P03Pack3Plugin.CardPrefix, "OpenSorcerer", "Open Sorcerer", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_open_sorcerer.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Blue, GemType.Green)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(ActivatedCopySigils.AbilityID);

            // Solar Ignitron
            CardManager.New(P03Pack3Plugin.CardPrefix, "SolarIgnitron", "Solar Ignitron", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_solar_ignitron.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Orange)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(FriendlyGemRevignite.AbilityID);

            // Shield Projector
            CardManager.New(P03Pack3Plugin.CardPrefix, "ShieldProjector", "Shield Projector", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_shield_projector.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Green)
                .SetNeutralP03Card()
                .AddAbilities(ActivatedDrawDefend.AbilityID);

            // Blood Vessel
            CardManager.New(P03Pack3Plugin.CardPrefix, "BloodVessel", "Blood Vessel", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_blood_vessel.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 1)
                .SetNeutralP03Card()
                .AddAbilities(FriendliesFullOfBlood.AbilityID);

            // Dredger Vessel
            CardManager.New(P03Pack3Plugin.CardPrefix, "DredgerVessel", "Dredger Vessel", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_dredger_vessel.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 1)
                .SetNeutralP03Card()
                .AddAbilities(Ability.BoneDigger);

            // Fuel Attendant
            CardManager.New(P03Pack3Plugin.CardPrefix, "FuelAttendant", "Fuel Attendant", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_fuel_attendant.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(MoveBesideAndFuel.AbilityID, FriendliesStinkyWhenFueled.AbilityID);

            // Hot Rod
            CardManager.New(P03Pack3Plugin.CardPrefix, "HotRod", "Hot Rod", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_hot_rod.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 7)
                .SetNeutralP03Card()
                .SetStartingFuel(0)
                .AddAbilities(FuelSiphon.AbilityID, Ability.Strafe, FuelShield.AbilityID);

            // Fuel Attendant
            CardManager.New(P03Pack3Plugin.CardPrefix, "BoneCracker", "Bone Cracker", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_bone_cracker.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 1)
                .SetNeutralP03Card()
                .AddAbilities(SacrificeQuadrupleBones.AbilityID);

            // Big Monster
            CardManager.New(P03Pack3Plugin.CardPrefix, "BigEffingThing", "Dobhar-Chu", 5, 10)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_big_blood_card.png", typeof(Cards).Assembly))
                .SetCost(bloodCost: 5)
                .SetNeutralP03Card()
                .AddAbilities(Ability.AllStrike);

            // Ramshackle
            CardManager.New(P03Pack3Plugin.CardPrefix, "Ramshackle", "Ramshackle", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ramshackle.png", typeof(Cards).Assembly))
                .SetGemsCost(GemType.Green, GemType.Orange)
                .SetNeutralP03Card()
                .AddAbilities(Ability.DropRubyOnDeath, EmeraldShard.AbilityID);

            // N-GINN
            CardManager.New(P03Pack3Plugin.CardPrefix, "Engine", "N-GINN", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_engine.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 2)
                .SetNeutralP03Card()
                .SetStartingFuel(4)
                .AddAbilities(ActivatedStrafe.AbilityID);

            // Iterator
            CardManager.New(P03Pack3Plugin.CardPrefix, "Iterator", "Iterator", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_iterator.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .AddAbilities(DrawCopyAltCost.AbilityID);

            // Gas Generator
            CardManager.New(P03Pack3Plugin.CardPrefix, "GasGenerator", "Gas Generator", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gas_generator.png", typeof(Cards).Assembly))
                .SetCost(bonesCost: 2)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitNeighborWhenFueled.AbilityID);

            // Neutral Tentacle
            CardManager.New(P03Pack3Plugin.CardPrefix, "Technicle", "544543484E49434C45", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_empty_tentacle.png", typeof(Cards).Assembly))
                .AddAbilities(Ability.Submerge);

            AbilityManager.ModifyAbilityList += delegate (List<AbilityManager.FullAbility> abilities)
            {
                if (!P03AscensionSaveData.IsP03Run)
                    return abilities;

                // Some additional bone support
                abilities.AbilityByID(Ability.BoneDigger).Info.AddMetaCategories(AbilityMetaCategory.Part3Modular);
                abilities.AbilityByID(Ability.QuadrupleBones).Info.AddMetaCategories(AbilityMetaCategory.Part3Modular);
                abilities.AbilityByID(Ability.OpponentBones).Info.AddMetaCategories(AbilityMetaCategory.Part3Modular);

                return abilities;
            };
        }
    }
}