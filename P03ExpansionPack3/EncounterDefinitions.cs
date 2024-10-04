using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Sequences;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using Infiniscryption.P03KayceeRun.Encounters;
using static Infiniscryption.P03KayceeRun.Encounters.EncounterHelper;

namespace Infiniscryption.P03ExpansionPack3
{
    public static class Pack3EncounterHelper
    {
        internal static EncounterBlueprintData UFOBattle;

        internal static void BuildEncounters()
        {
            UFOBattle = EncounterManager.New("P03KCMXP3_UFOBattle", addToPool: false);
            UFOBattle.SetDifficulty(0, 6);
            UFOBattle.turns = new();

            UFOBattle.turns.AddTurn(
                Enemy("P03KCMXP3_UFO_Hopper")
            );

            UFOBattle.turns.AddTurn();

            UFOBattle.turns.AddTurn(
                Enemy("P03KCMXP3_UFO_Hopper")
            );

            UFOBattle.turns.AddTurn(
                Enemy("P03KCMXP3_UFO_Sniper")
            );

            UFOBattle.turns.AddTurn(
                Enemy("P03KCMXP3_UFO_Hopper"), Enemy("P03KCMXP3_UFO_Hopper")
            );

            UFOBattle.turns.AddTurn();

            UFOBattle.turns.AddTurn(
                Enemy("P03KCMXP3_UFO_Hopper"), Enemy("P03KCMXP3_UFO_Sniper")
            );

            UFOBattle.ContinueOn("P03KCMXP3_UFO_Hopper", "P03KCMXP3_UFO_Sniper", null);

            // Synthesiod
            var synthBattle = EncounterManager.New("P03KCMXP3_Synths", addToPool: true);
            synthBattle.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            synthBattle.turns = new();

            // Turn 1
            synthBattle.turns.AddTurn(
                Enemy("Automaton", replacement: "P03KCMXP1_AmmoBot", difficulty: 3),
                Enemy("EmptyVessel_GreenGem", replacement: null, difficulty: 3)
            );

            // Turn 2
            synthBattle.turns.AddTurn(
                Enemy(null, replacement: "EmptyVessel_GreenGem", overclock: 3, difficulty: 3),
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy(null, replacement: "P03KCMXP1_ConveyorLatcher", difficulty: 2)
            );

            // Turn 3
            synthBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Synthesioid", overclock: 4)
            );

            // Turn 4
            synthBattle.turns.AddTurn(
                Enemy("EmptyVessel_GreenGem"),
                Enemy(null, replacement: "P03KCMXP1_CopyPasta", difficulty: 5)
            );

            // Turn 5
            synthBattle.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP2_EmeraldGuardian", difficulty: 3),
                Enemy("Automaton", replacement: "Insectodrone", difficulty: 1),
                Enemy("P03KCMXP1_FlyingLatcher")
            );

            // Turn 6
            synthBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Synthesioid", overclock: 4)
            );

            // Turn 7
            synthBattle.turns.AddTurn(
                Enemy("EmptyVessel_GreenGem"),
                Enemy(null, replacement: "P03KCMXP1_CopyPasta", difficulty: 6)
            );

            // Turn 8
            synthBattle.turns.AddTurn(
                Enemy("Automaton", replacement: "Insectodrone", difficulty: 2),
                Enemy("P03KCMXP3_RotLatcher"),
                Enemy(null, replacement: "P03KCMXP3_RotLatcher", difficulty: 3)
            );

            // Turn 9
            synthBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Synthesioid", overclock: 4)
            );

            // Turn 10
            synthBattle.turns.AddTurn(
                Enemy("EmptyVessel_GreenGem")
            );

            // Turn 11
            synthBattle.turns.AddTurn(
                Enemy("Automaton", replacement: "Insectodrone", difficulty: 1),
                Enemy("P03KCMXP3_RotLatcher"),
                Enemy(null, replacement: "P03KCMXP3_RotLatcher", difficulty: 2)
            );

            // Turn 12
            synthBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Synthesioid", overclock: 3)
            );

            // Gems Battle
            var gemsBattle = EncounterManager.New("P03KCMXP3_Gems", addToPool: true);
            gemsBattle.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            gemsBattle.turns = new();

            // Turn 1
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Ramshackle", overclockAbility: Ability.BuffGems, overclock: 2)
            );

            // Turn 2
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_TimeLatcher"),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 1)
            );

            // Turn 3
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_GearShifter", overclock: 4),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 3)
            );

            // Turn 4
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_TimeLatcher"),
                Enemy(null, replacement: "P03KCMXP3_TimeLatcher", difficulty: 2)
            );

            // Turn 5
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Shrinker", replacement: "P03KCMXP3_MagnusGod", difficulty: 6)
            );

            // Turn 6
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Ramshackle", overclockAbility: Ability.BuffGems, overclock: 2),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 1)
            );

            // Turn 7
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Kiln"),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 4)
            );

            // Turn 8
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_TimeLatcher", replacement: "P03KCMXP3_GearShifter", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP3_MagnusGod", difficulty: 5),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 3)
            );

            // Turn 9
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Ramshackle", overclockAbility: Ability.BuffGems, overclock: 2)
            );

            // Turn 10
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Kiln"),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 4)
            );

            // Turn 11
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_TimeLatcher", replacement: "P03KCMXP3_GearShifter", difficulty: 3),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 1)
            );

            // Turn 12
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Ramshackle", overclockAbility: Ability.BuffGems, overclock: 2)
            );

            // Turn 13
            gemsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_Kiln"),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 4)
            );

            // Fuel Generator Battle
            var fuelConduitsBattle = EncounterManager.New("P03KCMXP3_FuelConduits", addToPool: true);
            fuelConduitsBattle.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            fuelConduitsBattle.turns = new();

            // Turn 1
            fuelConduitsBattle.turns.AddTurn(
                Enemy("P03SIG_UrchinCell", replacement: "CellTri", difficulty: 4)
            );

            // Turn 2
            fuelConduitsBattle.turns.AddTurn(
                Enemy("P03KCMXP3_GasGenerator", replacement: "P03KCMXP3_GasGenerator_4", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP3_FuelAttendant", difficulty: 2)
            );

            // Turn 3
            fuelConduitsBattle.turns.AddTurn(
                Enemy(null, replacement: "P03SIG_UrchinCell", difficulty: 1),
                Enemy("P03KCMXP1_FrankenBot", replacement: "P03KCMXP1_FrankenBeast", difficulty: 4)
            );

            // Turn 4
            fuelConduitsBattle.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP3_FuelAttendant", difficulty: 2)
            );

            // Turn 5
            fuelConduitsBattle.turns.AddTurn(
                Enemy("P03SIG_UrchinCell", replacement: "CellGift", difficulty: 3),
                Enemy(null, "P03KCMXP3_NitrousTanker", difficulty: 6)
            );

            // Turn 6
            fuelConduitsBattle.turns.AddTurn(
                Enemy(null, "P03KCMXP3_NitrousTanker", difficulty: 5),
                Enemy("CellBuff")
            );

            // Turn 7
            fuelConduitsBattle.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP3_GasGenerator_4", difficulty: 2)
            );

            // Turn 8
            fuelConduitsBattle.turns.AddTurn(
                Enemy("P03KCMXP1_FrankenBot", replacement: "P03KCMXP1_FrankenBeast", difficulty: 4)
            );

            // Turn 9
            fuelConduitsBattle.turns.AddTurn(
                Enemy("CellTri")
            );

            // Turn 10
            fuelConduitsBattle.turns.AddTurn(
                Enemy(null, replacement: "CellGift", difficulty: 3)
            );

            // Turn 11
            fuelConduitsBattle.turns.AddTurn(
                Enemy("P03KCMXP1_FrankenBot", replacement: "P03KCMXP1_FrankenBeast", difficulty: 5)
            );

            // Turn 12
            fuelConduitsBattle.turns.AddTurn(
                Enemy("CellTri")
            );

            // Booger Battle
            var slimeBattle = EncounterManager.New("P03KCMXP3_Slime", addToPool: true);
            slimeBattle.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            slimeBattle.turns = new();

            // Turn 1
            slimeBattle.turns.AddTurn(
                Enemy("P03KCMXP3_MucusLauncher", overclock: 4, overclockAbility: Ability.DebuffEnemy)
            );

            // Turn 2
            slimeBattle.turns.AddTurn(
                Enemy("LeapBot", replacement: "P03KCM_FIREWALL", difficulty: 3)
            );

            // Turn 3
            slimeBattle.turns.AddTurn(
                Enemy("LeapBot", replacement: "Insectodrone", difficulty: 1)
            );

            // Turn 4
            slimeBattle.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP3_MysteryMachine", difficulty: 6)
            );

            // Turn 5
            slimeBattle.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP3_MucusLauncher", difficulty: 3, overclock: 4, overclockAbility: Ability.DebuffEnemy),
                Enemy("LeapBot", replacement: "P03KCMXP2_RobotRam", difficulty: 1)
            );

            // Turn 6
            slimeBattle.turns.AddTurn(
                Enemy("P03KCMXP3_MucusLauncher", replacement: null, difficulty: 3),
                Enemy(null, replacement: "P03KCMXP3_MysteryMachine", difficulty: 5)
            );

            // Turn 7
            slimeBattle.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP2_RobotRam", difficulty: 2)
            );

            // Turn 8
            slimeBattle.turns.AddTurn(
            );

            // Turn 9
            slimeBattle.turns.AddTurn(
                Enemy("P03KCMXP3_MucusLauncher")
            );
        }
    }
}
