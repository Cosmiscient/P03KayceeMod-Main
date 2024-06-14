using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class ExpansionPackCards_1
    {
        internal const string EXP_1_PREFIX = "P03KCMXP1";
        internal static string SEED_CARD => $"{EXP_1_PREFIX}_SEED";

        static ExpansionPackCards_1()
        {
            // Wolfbeast
            CardInfo wolfBeast = CardManager.New(EXP_1_PREFIX, "WolfBeast", "B30WULF", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_wolfbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 6)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(Ability.Transformer, Ability.DoubleStrike);

            // Wolfbot
            CardInfo wolfBot = CardManager.New(EXP_1_PREFIX, "WolfBot", "B30WULF", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_wolfbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 6)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.Transformer);

            wolfBeast.SetEvolve(wolfBot, 1);
            wolfBot.SetEvolve(wolfBeast, 1);

            // Viperbeast
            CardInfo viperBeast = CardManager.New(EXP_1_PREFIX, "ViperBeast", "PYTH0N", 3, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_transformer_rattler.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 5)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(Ability.Transformer);

            // Viperbot
            CardInfo viperBot = CardManager.New(EXP_1_PREFIX, "ViperBot", "PYTH0N", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_viperbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_viper.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 5)
                .AddMetaCategories(CardMetaCategory.Part3Random)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.Transformer);

            viperBeast.SetEvolve(viperBot, 1);
            viperBot.SetEvolve(viperBeast, 1);

            // Mantisbeast
            CardInfo mantisBeast = CardManager.New(EXP_1_PREFIX, "MantisBeast", "Asmanteus", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mantisbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetCardTemple(CardTemple.Tech)
                .AddAppearances(RareDiscCardAppearance.ID)
                .AddAbilities(Ability.Transformer, Ability.TriStrike);

            // Mantisbot
            CardInfo mantisBot = CardManager.New(EXP_1_PREFIX, "MantisBot", "Asmanteus", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mantisbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetRare()
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.Transformer);

            mantisBeast.SetEvolve(mantisBot, 1);
            mantisBot.SetEvolve(mantisBeast, 1);

            // Seedbot
            CardManager.New(EXP_1_PREFIX, "SeedBot", "SeedBot", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_seedbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_seedbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(TreeStrafe.AbilityID);

            // CardManager.New(EXP_1_PREFIX, "SeedBot_Gems", "SeedBot", 0, 1)
            //     .SetPortrait(TextureHelper.GetImageAsTexture("portrait_seedbot.png", typeof(ExpansionPackCards_1).Assembly))
            //     .SetCost(bonesCost: 6, bloodCost: 2, gemsCost: new() { GemType.Green}, energyCost: 2)
            //     .AddAbilities(TreeStrafe.AbilityID)
            //     .temple = CardTemple.Tech;


            // Rampager Latcher
            CardManager.New(EXP_1_PREFIX, "ConveyorLatcher", "Conveyor Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conveyorlatcher.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddMetaCategories(CardMetaCategory.Part3Random)
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_conveyor_latcher.png", typeof(ExpansionPackCards_1).Assembly))
                .AddAbilities(Ability.StrafeSwap, LatchRampage.AbilityID);

            // flying Latcher
            CardManager.New(EXP_1_PREFIX, "FlyingLatcher", "Sky Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skylatcher.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Flying, LatchFlying.AbilityID);

            // W0om
            CardManager.New(EXP_1_PREFIX, "Worm", "W0rm", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_worm.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(LatchDeathLatch.AbilityID);

            // Mirror Tentacle
            CardManager.New(EXP_1_PREFIX, "MirrorTentacle", "4D4952524F52", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mirrorsquid.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .SetStatIcon(SpecialStatIcon.Mirror);

            // Battery Tentacle
            CardManager.New(EXP_1_PREFIX, "BatteryTentacle", "42415454455259", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_batterysquid.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .SetStatIcon(BatteryPower.AbilityID);

            CardManager.New(EXP_1_PREFIX, "Salmon_Undead", "Fishbones", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_fishbones.png", typeof(ExpansionPackCards_1).Assembly))
                .SetEmissivePortrait(TextureHelper.GetImageAsTexture("portrait_fishbones_emission.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(bonesCost: 1)
                .SetCardTemple(CardTemple.Undead);

            var salmonWizard = CardManager.New(EXP_1_PREFIX, "Salmon_Wizard", "Koi", 0, 1)
                .AddAppearances(OnboardWizardCardModel.ID)
                .SetCardTemple(CardTemple.Wizard);

            salmonWizard.portraitTex = Sprite.Create(
                TextureHelper.GetImageAsTexture("portrait_koifish.png", typeof(ExpansionPackCards_1).Assembly),
                new Rect(0, 0, 125, 190),
                new Vector2(0.5f, 0.5f)
            );

            CardManager.New(EXP_1_PREFIX, "Salmon_Nature", "Salmon", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_salmon_nature.png", typeof(ExpansionPackCards_1).Assembly))
                .SetEmissivePortrait(TextureHelper.GetImageAsTexture("portrait_salmon_nature_emission.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCardTemple(CardTemple.Nature);

            CardManager.New(EXP_1_PREFIX, "Salmon_Golly", "Salmon", 0, 1)
                .SetAltPortrait(TextureHelper.GetImageAsTexture("portrait_golly_salmon.png", typeof(CustomCards).Assembly, FilterMode.Trilinear))
                .AddAppearances(HighResAlternatePortrait.ID)
                .SetWeaponMesh(DiskCardWeapon.Fish)
                .temple = CardTemple.Tech;

            // Salmon and beastmaster
            CardManager.New(EXP_1_PREFIX, "Salmon", "S4LM0N", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_salmon.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_salmon.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 1)
                .AddMetaCategories(CardMetaCategory.Part3Random)
                .SetCardTemple(CardTemple.Tech)
                .SetWeaponMesh(DiskCardWeapon.Fish)
                .SetEvolve("Angler_Fish_More", 1);

            CardManager.BaseGameCards.CardByName("Angler_Fish_More").SetEvolve("Angler_Fish_Good", 1);

            CardInfo bm2 = CardManager.New(EXP_1_PREFIX, "BeastMaster2", "B3A5T GR4ND M4ST3R", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_beastmaster.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(SummonFamiliar.AbilityID, Ability.BuffNeighbours);

            CardManager.New(EXP_1_PREFIX, "BeastMaster", "B3A5T M4ST3R", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_beastmaster.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Nature)
                .SetRare()
                .AddAbilities(SummonFamiliar.AbilityID)
                .SetEvolve(bm2, 1);

            // Bull
            CardManager.New(EXP_1_PREFIX, "BuckingBull", "T4URU5", 1, 8)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_bull.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Nature)
                .SetRare()
                .AddAbilities(BuckWildRework.AbilityID);

            // Googlebot
            CardManager.New(EXP_1_PREFIX, "GoogleBot", "SearchBot", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_googlebot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.Tutor);

            // Ammo Bot
            CardManager.New(EXP_1_PREFIX, "AmmoBot", "AmmoBot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ammobot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_ammobot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .AddMetaCategories(CardMetaCategory.Part3Random)
                .AddAbilities(FullyLoaded.AbilityID);

            // oil Bot
            CardManager.New(EXP_1_PREFIX, "OilJerry", "Oil Jerry", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_oil_jerry.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_oil_jerry.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(FullOfOil.AbilityID);

            // Necrobot
            CardManager.New(EXP_1_PREFIX, "Necrobot", "Necronomaton", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_necrobot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(Necromancer.AbilityID);

            // Zombie Process
            CardInfo zombie = CardManager.New(EXP_1_PREFIX, "ZombieProcess", "Zombie Process", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_zombieprocess.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(Ability.Brittle, Ability.IceCube);

            CardInfo gravestone = CardManager.New(EXP_1_PREFIX, "ZombieGravestone", "Zombie Process", 0, 2)
                .AddAppearances(CardAppearanceBehaviour.Appearance.HologramPortrait)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(Ability.PreventAttack, Ability.Evolve);

            zombie.SetIceCube(gravestone);
            gravestone.SetEvolve(zombie, 3);
            gravestone.holoPortraitPrefab = CardManager.BaseGameCards.CardByName("TombStone").holoPortraitPrefab;

            CardManager.BaseGameCards.CardByName("TombStone").SetEvolve(zombie, 1);

            // Recycle Angel
            CardManager.New(EXP_1_PREFIX, "RoboAngel", "AngelBot", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_recyclenangel.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(AcceleratedLifecycle.AbilityID, Ability.Flying);

            // Conduit protector
            CardManager.New(EXP_1_PREFIX, "ConduitProtector", "Conduit Protector", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conduitprotector.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitProtector.AbilityID);

            // Skelecell
            CardManager.New(EXP_1_PREFIX, "Skelecell", "Skel-E-Cell", 3, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skelecell.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(Ability.Brittle, CellUndying.AbilityID);

            // Flying Conduit
            CardManager.New(EXP_1_PREFIX, "ConduitFlying", "Airspace Conduit", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conduitflying.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitGainFlying.AbilityID);

            // Flying Conduit
            CardManager.New(EXP_1_PREFIX, "ConduitDebuffEnemy", "Foul Conduit", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conduitdebuffenemy.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 2)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitGainDebuffEnemy.AbilityID);

            // Skyplane
            CardManager.New(EXP_1_PREFIX, "Spyplane", "Spyplane", 3, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skyplane.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_spyplane.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 6)
                .SetNeutralP03Card()
                .AddAbilities(Ability.Flying);

            // Executor
            CardManager.New(EXP_1_PREFIX, "Executor", "Executor", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_executor.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 6)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddMetaCategories(CardMetaCategory.Part3Random)
                .AddAbilities(Ability.Deathtouch);

            // Copy Pasta
            CardManager.New(EXP_1_PREFIX, "CopyPasta", "Copypasta", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_copypasta.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(MirrorImage.AbilityID);

            // Frankenbot
            CardInfo frankenBot = CardManager.New(EXP_1_PREFIX, "FrankenBot", "FrankenCell", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_frankenbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_frankenbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddMetaCategories(CustomCards.TechRegion)
                .AddAbilities(CellEvolve.AbilityID);

            CardInfo frankenBeast = CardManager.New(EXP_1_PREFIX, "FrankenBeast", "FrankenCell", 3, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_frankenbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(CellDeEvolve.AbilityID, Electric.AbilityID);

            frankenBot.SetEvolve(frankenBeast, 1);
            frankenBeast.SetEvolve(frankenBot, 1);
            frankenBeast.temple = CardTemple.Tech;
            frankenBeast.metaCategories = new();

            // Clock man
            CardManager.New(EXP_1_PREFIX, "Clockbot", "Mr:Clock", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_clockbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_clockbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .AddMetaCategories(CardMetaCategory.Part3Random)
                .SetNeutralP03Card()
                .AddAbilities(RotatingAlarm.AbilityID);

            CardManager.New(EXP_1_PREFIX, "Clockbot_Right", "Mr:Clock", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_clockbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .AddAbilities(RotatingAlarm.AbilityID)
                .SetExtendedProperty(RotatingAlarm.DEFAULT_STATE_KEY, 1);

            CardManager.New(EXP_1_PREFIX, "Clockbot_Down", "Mr:Clock", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_clockbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .AddAbilities(RotatingAlarm.AbilityID)
                .SetExtendedProperty(RotatingAlarm.DEFAULT_STATE_KEY, 0);

            CardManager.New(EXP_1_PREFIX, "Clockbot_Left", "Mr:Clock", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_clockbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 3)
                .AddAbilities(RotatingAlarm.AbilityID)
                .SetExtendedProperty(RotatingAlarm.DEFAULT_STATE_KEY, 3);

            // Robo Ducky
            CardManager.New(EXP_1_PREFIX, "RubberDuck", "Roboducky", 3, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_roboducky.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .SetRare()
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.BuffEnemy, Ability.Submerge);

            // Titans
            CardManager.New(EXP_1_PREFIX, "RubyTitan", "Ruby Titan", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ruby_titan.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 6)
                .AddPart3Decal(TextureHelper.GetImageAsTexture("decal_ruby_titan.png", typeof(CustomCards).Assembly))
                .SetRegionalP03Card(CardTemple.Wizard)
                .SetRare()
                .AddAbilities(RubyPower.AbilityID);

            CardManager.New(EXP_1_PREFIX, "SapphireTitan", "Sapphire Titan", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_sapphire_titan.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 4)
                .AddPart3Decal(TextureHelper.GetImageAsTexture("decal_sapphire_titan.png", typeof(CustomCards).Assembly))
                .SetRegionalP03Card(CardTemple.Wizard)
                .SetRare()
                .AddAbilities(SapphirePower.AbilityID);

            CardManager.New(EXP_1_PREFIX, "EmeraldTitan", "Emerald Titan", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_emerald_titan.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 5)
                .AddPart3Decal(TextureHelper.GetImageAsTexture("decal_emerald_titan.png", typeof(CustomCards).Assembly))
                .SetRegionalP03Card(CardTemple.Wizard)
                .SetRare()
                .AddAbilities(EmeraldPower.AbilityID);

            // I'm sorry gem rotator you don't work
            // CardManager.New(EXP_1_PREFIX, "GemRotator", "Gem Cycler", 1, 2)
            //     .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gemcycler.png", typeof(ExpansionPackCards_1).Assembly))
            //     .SetCost(energyCost: 5)
            //     //.SetRegionalP03Card(CardTemple.Wizard)
            //     .AddAbilities(GemRotator.AbilityID);

            // Seed
            CardManager.New(EXP_1_PREFIX, "SEED", "Seed", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_seed.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech)
                .AddSpecialAbilities(SeedBehaviour.AbilityID);
        }
    }
}