using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace P03KayceeRun.cards
{
    [HarmonyPatch]
    public static class ExpansionPackCards_2
    {
        internal const string EXP_2_PREFIX = "P03KCMXP2";

        internal const string ZAP_CARD = "P03KCMXP2_ZAP";
        internal const string CHARGE_CARD = "P03KCMXP2_CHARGE";

        internal const string FLAME_CHARMER_CARD = "P03KCMXP2_FlameCharmer";

        static ExpansionPackCards_2()
        {
            // Swapper Latcher
            CardManager.New(EXP_2_PREFIX, "SwapperLatcher", "Swapper Latcher", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_swapper_latcher.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(LatchSwapper.AbilityID);

            // Box Bot
            CardManager.New(EXP_2_PREFIX, "BoxBot", "Box Bot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_boxbot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(Ability.Brittle, VesselHeart.AbilityID);

            // Scrap Bot
            CardManager.New(EXP_2_PREFIX, "ScrapBot", "Scrap Bot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_scrapbot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .AddAbilities(ScrapDumper.AbilityID);

            // Zip Bomb
            CardManager.New(EXP_2_PREFIX, "ZipBomb", "Zip Bomb", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_zipbomb.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 1)
                .SetRare()
                .SetNeutralP03Card()
                .AddAbilities(TakeDamageSigil.AbilityID);

            // Robot Ram
            CardManager.New(EXP_2_PREFIX, "RobotRam", "R4M", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ram.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Overheat.AbilityID);

            // Elektron
            CardManager.New(EXP_2_PREFIX, "Elektron", "Elektron", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_elektron.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetRare()
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitNeighbor.AbilityID);

            // Shovester
            CardManager.New(EXP_2_PREFIX, "Shovester", "Shovester", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_shovester.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(Shove.AbilityID);

            // Librarian
            CardManager.New(EXP_2_PREFIX, "Librarian", "Librarian", 1, 2)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_librarian").ConvertTexture(TextureHelper.SpriteType.CardPortrait))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Reach, DeadByte.AbilityID);

            // Ruby Guardian
            CardManager.New(EXP_2_PREFIX, "RubyGuardian", "Ruby Guardian", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ruby_guardian.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 6)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(RubyWarrior.AbilityID);

            // Emerald Guardian
            CardManager.New(EXP_2_PREFIX, "EmeraldGuardian", "Emerald Guardian", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_emerald_guardian.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 5)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(EmeraldExtraction.AbilityID, EmeraldExtraction.AbilityID, EmeraldExtraction.AbilityID);

            // PyroBot
            CardManager.New(EXP_2_PREFIX, "PyroBot", "Ignitron", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ignitron.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .AddAbilities(FireBomb.AbilityID);

            // GlowBot
            CardManager.New(EXP_2_PREFIX, "GlowBot", "GlowBot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_glowbot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Tech)
                .SetRare()
                .AddAbilities(SolarHeart.AbilityID);

            // M0l0t0v
            CardManager.New(EXP_2_PREFIX, "Molotov", "M0l0t0v", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_m010t0v.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 1)
                .SetNeutralP03Card()
                .AddAbilities(Molotov.AbilityID);

            // Flaming Exeskeleton
            CardManager.New(EXP_2_PREFIX, "FlamingExeskeleton", "Flaming Exeskeleton", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_flaming_exeskeleton.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Brittle, FireBomb.AbilityID, BurntOut.AbilityID);

            // Gas Conduit
            CardManager.New(EXP_2_PREFIX, "GasConduit", "Gas Conduit", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gasconduit.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitGas.AbilityID, BurntOut.AbilityID);

            // Street Sweeper
            CardManager.New(EXP_2_PREFIX, "StreetSweeper", "Street Sweeper", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_streetcleaner.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 6)
                .SetNeutralP03Card()
                .AddAbilities(FireBomb.AbilityID, Ability.Strafe);

            // Give-A-Way
            CardManager.New(EXP_2_PREFIX, "GiveAWay", "Give-A-Way", 2, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_giveaway.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .AddAbilities(EnemyGainShield.AbilityID);

            // Pity Seeker
            CardManager.New(EXP_2_PREFIX, "PitySeeker", "Pity Seeker", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_pity_seeker.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Tech, CardTemple.Undead)
                .AddAbilities(Ability.ConduitNull, LatchNullConduit.AbilityID);

            // Urch1n Cell
            CardManager.New(EXP_2_PREFIX, "UrchinCell", "Urch1n Cell", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_urchin_cell.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetRegionalP03Card(CardTemple.Tech, CardTemple.Nature)
                .AddAbilities(Ability.Sharp, CellDeSubmerge.AbilityID);

            // Rh1n0
            CardManager.New(EXP_2_PREFIX, "Rhino", "Rh1n0", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_rhino.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 6)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(ActivatedGainPower.AbilityID);

            // 3leph4nt
            CardManager.New(EXP_2_PREFIX, "Elephant", "3leph4nt", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_elephant.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 5)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Stomp.AbilityID);

            // Zap!
            CardManager.New(EXP_2_PREFIX, "ZAP", "Zap!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_zap.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .SetSpellAppearanceP03()
                .AddAbilities(DirectDamage.AbilityID);

            // Jimmy Jr
            CardManager.New(EXP_2_PREFIX, "JimmyJr", "Jimmy Jr.", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_jimmy_jr.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 1)
                .SetNeutralP03Card()
                .AddAbilities(DrawTwoZap.AbilityID);

            // OP Bot
            CardManager.New(EXP_2_PREFIX, "OpBot", "OP Bot", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_op_bot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(RandomNegativeAbility.AbilityID);

            // Emerald Squid
            CardManager.New(EXP_2_PREFIX, "EmeraldSquid", "656D6572616C64", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_greengemsquid.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .SetStatIcon(GreenGemPower.AbilityID);

            // Charge!
            CardManager.New(EXP_2_PREFIX, "CHARGE", "Charge!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_charge.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 0)
                .SetGlobalSpell()
                .SetSpellAppearanceP03()
                .AddAbilities(RefillBattery.AbilityID);

            // Weeper
            CardManager.New(EXP_2_PREFIX, "Weeper", "Weeper", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_weeper.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(EnergySiphon.AbilityID);

            // Suicell
            CardManager.New(EXP_2_PREFIX, "Suicell", "Sui-Cell", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_suicell.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Tech, CardTemple.Undead)
                .AddAbilities(CellExplodonate.AbilityID);

            // Kindness Giver
            CardManager.New(EXP_2_PREFIX, "KindnessGiver", "Kindness Giver", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_kindness_giver.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitGemify.AbilityID, Ability.GainGemBlue);

            // Kindness Giver
            CardManager.New(EXP_2_PREFIX, "Gopher", "G0ph3r", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_gopher.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Miner.AbilityID);

            // P00dl3
            CardManager.New(EXP_2_PREFIX, "Poodle", "P00dl3", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_poodle.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 3)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.MoveBeside, Ability.BuffNeighbours);

            // Flame Charmter
            CardManager.New(EXP_2_PREFIX, "FlameCharmer", "Flamecharmer", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_flame_charmer.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .AddAbilities(FireBomb.FlameStokerID, Ability.MadeOfStone);

            // Artillery Droid
            CardManager.New(EXP_2_PREFIX, "ArtilleryDroid", "Artillery Droid", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_artillery_droid.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetNeutralP03Card()
                .AddAbilities(MissileStrike.AbilityID);

            // Ultra Bot
            CardManager.New(EXP_2_PREFIX, "UltraBot", "Ultra Bot", 1, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ultra_bot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 5)
                .SetNeutralP03Card()
                .SetRare()
                .SetExtendedProperty(MissileStrike.NUMBER_OF_MISSILES, 3)
                .AddAbilities(MissileStrike.AbilityID);

            // Hellfire Droid
            CardManager.New(EXP_2_PREFIX, "HellfireDroid", "Hellfire Droid", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_hellfire_droid.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 5)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(MissileStrikeSelf.AbilityID);

            // Energy Vampire
            CardManager.New(EXP_2_PREFIX, "EnergyVampire", "Energy Vampire", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_energy_vampire.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(AbsorbShield.AbilityID);
        }
    }
}