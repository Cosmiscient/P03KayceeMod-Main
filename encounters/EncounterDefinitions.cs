using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;

namespace Infiniscryption.P03KayceeRun.Encounters
{
    public static class EncounterHelper
    {
        /// <summary>
        /// The maximum difficulty level that an an encounter can go to
        /// </summary>
        public const int MAX_DIFFICULTY = 6;

        /// <summary>
        /// Creates a series of card blueprints that satisfy the given behavior
        /// </summary>
        /// <param name="cardName">The default card for the blueprint</param>
        /// <param name="replacement">The card that should replace the default when the condition is met</param>
        /// <param name="difficulty">The default card is replaced with the replacement at this difficulty level or higher.</param>
        /// <param name="random">Gives a random chance for the default card to be replaced with one of the blueprint's random replacement cards. Does not apply to the difficulty replacement.</param>
        /// <param name="overclock">At this difficulty level or higher, the card is overclocked.</param>
        public static List<EncounterBlueprintData.CardBlueprint> Enemy(string cardName, string replacement = null, int difficulty = 0, int random = 0, int overclock = 0, Ability? overclockAbility = null)
        {
            P03Plugin.Log.LogDebug($"Generating enemy defn: {cardName} becomes {replacement} at {difficulty} and gets overclocked at {overclock}");
            List<EncounterBlueprintData.CardBlueprint> retval = new();

            EncounterBlueprintData.CardBlueprint baseBp = new()
            {
                card = String.IsNullOrEmpty(cardName) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(cardName))
            };

            if (overclock > 0 && overclock < difficulty && String.IsNullOrEmpty(cardName))
                overclock = difficulty;

            baseBp.maxDifficulty = overclock == 0 && difficulty == 0
                ? MAX_DIFFICULTY
                : overclock == 0 && difficulty > 0
                ? difficulty - 1
                : overclock > 0 && difficulty == 0 ? overclock - 1 : Math.Min(overclock, difficulty) - 1;

            if (random > 0)
                baseBp.randomReplaceChance = random;

            retval.Add(baseBp);

            if (difficulty > 0 && replacement != null)
            {
                EncounterBlueprintData.CardBlueprint diff = new()
                {
                    card = String.IsNullOrEmpty(replacement) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(replacement)),
                    minDifficulty = difficulty,
                    maxDifficulty = overclock > difficulty ? overclock - 1 : MAX_DIFFICULTY
                };

                if (difficulty >= overclock && overclock > 0)
                {
                    diff.card.mods ??= new();

                    if (overclockAbility.HasValue)
                        diff.card.Mods.Add(new(overclockAbility.Value) { fromOverclock = true });
                    else
                        diff.card.Mods.Add(new(1, 0) { fromOverclock = true });
                }

                retval.Add(diff);
            }

            if (overclock > 0)
            {
                EncounterBlueprintData.CardBlueprint ov = new()
                {
                    minDifficulty = overclock
                };
                if (overclock > difficulty && difficulty > 0)
                {
                    ov.card = String.IsNullOrEmpty(replacement) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(replacement));
                    ov.maxDifficulty = MAX_DIFFICULTY;
                }
                else
                {
                    ov.card = String.IsNullOrEmpty(cardName) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(cardName));
                    ov.maxDifficulty = difficulty > overclock ? difficulty - 1 : MAX_DIFFICULTY;
                }
                if (ov.card != null)
                {
                    ov.card.mods ??= new();
                    if (overclockAbility.HasValue)
                        ov.card.Mods.Add(new(overclockAbility.Value) { fromOverclock = true });
                    else
                        ov.card.Mods.Add(new(1, 0) { fromOverclock = true });
                    retval.Add(ov);
                }
            }

            return retval;
        }

        // These blueprints are special; they're for bosses and aren't in the normal pool
        internal static EncounterBlueprintData CanvasBossPX;
        internal static EncounterBlueprintData P03FinalBoss;
        internal static EncounterBlueprintData PhotographerBossP1;
        internal static EncounterBlueprintData PhotographerBossP2;
        internal static EncounterBlueprintData GeneratorDamageRace;

        internal static void BuildEncounters()
        {
            // Encounter: CanvasBossPX
            CanvasBossPX = EncounterManager.New("P03KCM_CanvasBossPX", addToPool: false);
            CanvasBossPX.SetDifficulty(0, 6);
            CanvasBossPX.AddRandomReplacementCards("Automaton", "MineCart", "AlarmBot", "Insectodrone");
            CanvasBossPX.turns = new();

            // TURN 1
            CanvasBossPX.turns.AddTurn(
                Enemy(null, replacement: "SwapBot", difficulty: 2),
                Enemy("Automaton")
            );

            // TURN 2
            CanvasBossPX.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 1)
            );

            // TURN 3
            CanvasBossPX.turns.AddTurn(
                Enemy("Automaton")
            );

            // TURN 4
            CanvasBossPX.turns.AddTurn(
                Enemy("Automaton"),
                Enemy(null, replacement: "Automaton", difficulty: 3)
            );

            // TURN 5
            CanvasBossPX.turns.AddTurn(
                Enemy(null, replacement: "SwapBot", difficulty: 4)
            );

            // TURN 6
            CanvasBossPX.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 6)
            );

            // TURN 7
            CanvasBossPX.turns.AddTurn(
                Enemy("LeapBot")
            );

            // TURN 8
            CanvasBossPX.turns.AddTurn(
                Enemy("Automaton"),
                Enemy(null, replacement: "MineCart", difficulty: 3)
            );


            // Encounter: Nature_BatTransformers
            EncounterBlueprintData natureBatTransformers = EncounterManager.New("P03KCM_Nature_BatTransformers", addToPool: true);
            natureBatTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureBatTransformers.turns = new();

            // TURN 1
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBot", replacement: "XformerBatBeast", difficulty: 2)
            );

            // TURN 2
            natureBatTransformers.turns.AddTurn(
                Enemy(null, replacement: "XformerBatBot", difficulty: 4)
            );

            // TURN 3
            natureBatTransformers.turns.AddTurn(
                Enemy("CXformerAdder", replacement: "XformerBatBot", difficulty: 3),
                Enemy(null, replacement: "P03KCM_CXformerMole", difficulty: 2, overclock: 4)
            );

            // TURN 4
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBot", replacement: "XformerBatBeast", difficulty: 1),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 5
            natureBatTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "P03KCMXP1_BeastMaster", difficulty: 5),
                Enemy("Shieldbot", replacement: null, difficulty: 6)
            );

            // TURN 6
            natureBatTransformers.turns.AddTurn(
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 4)
            );

            // TURN 7
            natureBatTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "BoltHound", difficulty: 6),
                Enemy(null, replacement: "P03KCMXP1_BeastMaster", difficulty: 6)
            );

            // TURN 8
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBeast", replacement: "XformerGrizzlyBot", difficulty: 6)
            );


            // Encounter: Nature_BearTransformers
            EncounterBlueprintData natureBearTransformers = EncounterManager.New("P03KCM_Nature_BearTransformers", addToPool: true);
            natureBearTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureBearTransformers.turns = new();

            // TURN 1
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerGrizzlyBot"),
                Enemy(null, replacement: "P03KCMXP1_SeedBot", difficulty: 1, overclock: 2, overclockAbility: Ability.Reach)
            );

            // TURN 2
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerPorcupineBot", replacement: "XformerBatBeast", difficulty: 4)
            );

            // TURN 3
            natureBearTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "P03KCMXP1_MantisBot", difficulty: 5)
            );

            // TURN 4
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerGrizzlyBot", replacement: "XformerGrizzlyBeast", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_SeedBot", difficulty: 3, overclock: 4, overclockAbility: Ability.Reach)
            );

            // TURN 5
            natureBearTransformers.turns.AddTurn(
                Enemy("Shieldbot", replacement: "XformerGrizzlyBot", difficulty: 6),
                Enemy(null, replacement: "CXformerAdder", difficulty: 4)
            );

            // TURN 6
            natureBearTransformers.turns.AddTurn(
                Enemy("MineCart", replacement: "P03KCMXP1_RubberDuck", difficulty: 6),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 7
            natureBearTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "BoltHound", difficulty: 4)
            );

            // TURN 8
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerGrizzlyBot", replacement: "XformerGrizzlyBeast", difficulty: 6)
            );


            // Encounter: Nature_Hounds
            EncounterBlueprintData natureHounds = EncounterManager.New("P03KCM_Nature_Hounds", addToPool: true);
            natureHounds.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureHounds.turns = new();

            // TURN 1
            natureHounds.turns.AddTurn(
                Enemy("LeapBot")
            );

            // TURN 2
            natureHounds.turns.AddTurn(
                Enemy("BoltHound")
            );

            // TURN 3
            natureHounds.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_WolfBot", difficulty: 1)
            );

            // TURN 4
            natureHounds.turns.AddTurn(
                Enemy("P03KCM_CXformerAlpha", overclock: 5),
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 3, overclock: 6)

            );

            // TURN 5
            natureHounds.turns.AddTurn(
                Enemy("P03KCMXP1_WolfBot", replacement: "P03KCMXP1_WolfBeast", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_BuckingBull", difficulty: 5)
            );

            // TURN 6
            natureHounds.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_MantisBot", difficulty: 6)
            );

            // TURN 7
            natureHounds.turns.AddTurn(
                Enemy("Bombbot", replacement: "BoltHound", difficulty: 3),
                Enemy(null, replacement: "CXformerAdder", difficulty: 2)
            );

            // TURN 8
            natureHounds.turns.AddTurn(
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 2),
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 4)
            );

            // TURN 9
            natureHounds.turns.AddTurn(
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 3),
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 5)
            );


            // Encounter: Nature_Zoo
            EncounterBlueprintData natureZoo = EncounterManager.New("P03KCM_Nature_Zoo", addToPool: true);
            natureZoo.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureZoo.turns = new();

            // TURN 1
            natureZoo.turns.AddTurn(
                Enemy("P03KCM_CXformerRabbit", replacement: "P03KCM_CXformerOpossum", difficulty: 3)
            );

            // TURN 2
            natureZoo.turns.AddTurn(
                Enemy("P03KCM_CXformerOpossum", replacement: "CXformerAdder", difficulty: 2),
                Enemy(null, replacement: "P03KCM_CXformerMole", difficulty: 1)
            );

            // TURN 3
            natureZoo.turns.AddTurn(
                Enemy("P03KCM_CXformerAlpha", overclock: 5),
                Enemy("P03KCM_CXformerRabbit", replacement: "P03KCM_CXformerOpossum", difficulty: 3)
            );

            // TURN 4
            natureZoo.turns.AddTurn(
                Enemy("P03KCM_CXformerRiverSnapper", replacement: "CXformerElk", difficulty: 6),
                Enemy("P03KCM_CXformerRabbit", replacement: "P03KCM_CXformerOpossum", difficulty: 2)
            );

            // TURN 5
            natureZoo.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_BeastMaster", difficulty: 5),
                Enemy("CXformerRaven")
            );

            // TURN 6
            natureZoo.turns.AddTurn(
                Enemy(null, replacement: "P03KCM_CXformerAlpha", difficulty: 1),
                Enemy(null, replacement: "P03KCM_CXformerMantis", difficulty: 3)
            );

            // TURN 7
            natureZoo.turns.AddTurn(
                Enemy("CXformerWolf", replacement: "P03KCMXP1_BeastMaster", difficulty: 6),
                Enemy("P03KCM_CXformerRabbit")
            );

            // TURN 8
            natureZoo.turns.AddTurn(
                Enemy(null, "P03KCM_CXformerOpossum", difficulty: 2),
                Enemy(null, "P03KCM_CXformerAlpha", difficulty: 3)
            );

            // TURN 9
            natureZoo.turns.AddTurn(
                Enemy("P03KCM_CXformerRabbit", "P03KCM_CXformerOpossum", difficulty: 2)
            );

            // TURN 10
            natureZoo.turns.AddTurn(
                Enemy(null, "P03KCM_CXformerAlpha", difficulty: 3)
            );


            // Encounter: Nature_SnakeTransformers
            EncounterBlueprintData natureSnakeTransformers = EncounterManager.New("P03KCM_Nature_SnakeTransformers", addToPool: true);
            natureSnakeTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureSnakeTransformers.turns = new();

            // TURN 1
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot", replacement: "P03KCMXP1_ViperBeast", difficulty: 3, overclock: 2, overclockAbility: SnakeStrafe.AbilityID)
            );

            // TURN 2
            natureSnakeTransformers.turns.AddTurn(
                Enemy("LeapBot", replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 3
            natureSnakeTransformers.turns.AddTurn(
                Enemy("CXformerAdder", replacement: null, difficulty: 6, overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy("CXformerAdder", replacement: "P03KCMXP1_ViperBot", difficulty: 3, overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy(null, replacement: "P03KCMXP1_MantisBot", difficulty: 5)
            );

            // TURN 4
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot", overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy(null, replacement: "P03KCMXP1_ViperBot", difficulty: 1, overclock: 2, overclockAbility: SnakeStrafe.AbilityID)
            );

            // TURN 5
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "CXformerAdder", difficulty: 4, overclock: 4, overclockAbility: SnakeStrafe.AbilityID),
                Enemy("CXformerAdder", replacement: "P03KCMXP1_ViperBot", difficulty: 3, overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy(null, replacement: "P03KCMXP1_ViperBot", difficulty: 1, overclock: 2, overclockAbility: SnakeStrafe.AbilityID)
            );

            // TURN 6
            natureSnakeTransformers.turns.AddTurn(
                Enemy("CXformerAdder", overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy("CXformerAdder", replacement: "P03KCMXP1_ViperBeast", difficulty: 4, overclock: 2, overclockAbility: SnakeStrafe.AbilityID)
            );

            // TURN 7
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot", replacement: null, difficulty: 6, overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy(null, replacement: "P03KCMXP1_MantisBeast", difficulty: 6)
            );

            // TURN 8
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot", overclock: 2, overclockAbility: SnakeStrafe.AbilityID),
                Enemy("P03KCMXP1_ViperBot", replacement: "P03KCMXP1_ViperBeast", difficulty: 6, overclock: 2, overclockAbility: SnakeStrafe.AbilityID)
            );

            // TURN 9
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "CXformerAdder", difficulty: 3, overclock: 3, overclockAbility: SnakeStrafe.AbilityID),
                Enemy("P03KCMXP1_ViperBot", replacement: "P03KCMXP1_ViperBeast", difficulty: 4, overclock: 2, overclockAbility: SnakeStrafe.AbilityID)
            );

            // TURN 10
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_ViperBeast", difficulty: 3, overclock: 3, overclockAbility: SnakeStrafe.AbilityID)
            );


            // Encounter: Neutral_Alarmbots
            EncounterBlueprintData neutralAlarmbots = EncounterManager.New("P03KCM_Neutral_Alarmbots", addToPool: true);
            neutralAlarmbots.SetDifficulty(0, 6).SetP03Encounter();
            neutralAlarmbots.turns = new();

            // TURN 1
            neutralAlarmbots.turns.AddTurn(
                Enemy("AlarmBot", overclock: 3)
            );

            // TURN 2
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 6),
                Enemy(null, replacement: "AlarmBot", difficulty: 4)
            );

            // TURN 3
            neutralAlarmbots.turns.AddTurn(
                Enemy("AlarmBot", replacement: "SwapBot", difficulty: 5)
            );

            // TURN 4
            neutralAlarmbots.turns.AddTurn(
                Enemy("Bombbot"),
                Enemy(null, replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 5
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 2),
                Enemy("Insectodrone", replacement: "P03KCMXP1_RubberDuck", difficulty: 5)
            );

            // TURN 6
            neutralAlarmbots.turns.AddTurn(
                Enemy("MineCart", replacement: "P03KCMXP1_EmeraldTitan", difficulty: 6),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 7
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 3),
                Enemy(null, replacement: "AlarmBot", difficulty: 4)
            );

            // TURN 8
            neutralAlarmbots.turns.AddTurn(
                Enemy("Automaton", replacement: "Insectodrone", difficulty: 4),
                Enemy(null, replacement: "AlarmBot", difficulty: 6)
            );

            // TURN 9
            neutralAlarmbots.turns.AddTurn(

            );

            // TURN 10
            neutralAlarmbots.turns.AddTurn(
                Enemy("Automaton", replacement: "CloserBot", difficulty: 4)
            );


            // Encounter: Neutral_BombsAndShields
            EncounterBlueprintData neutralBombsAndShields = EncounterManager.New("P03KCM_Neutral_BombsAndShields", addToPool: true);
            neutralBombsAndShields.SetDifficulty(0, 6).SetP03Encounter();
            neutralBombsAndShields.AddRandomReplacementCards("Shieldbot", "Bombbot", "SentryBot");
            neutralBombsAndShields.turns = new();

            // TURN 1
            neutralBombsAndShields.turns.AddTurn(
                Enemy("Shieldbot"),
                Enemy(null, replacement: "SentryBot", difficulty: 4),
                Enemy(null, replacement: "Bombbot", difficulty: 6)
            );

            // TURN 2
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_AmmoBot", difficulty: 6),
                Enemy("Bombbot"),
                Enemy(null, replacement: "Shieldbot", difficulty: 2, overclock: 4)
            );

            // TURN 3
            neutralBombsAndShields.turns.AddTurn(
                Enemy("P03KCMXP2_Molotov", replacement: "P03KCMXP2_PyroBot", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP2_Molotov", difficulty: 3, overclock: 4)
            );

            // TURN 4
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy("Bombbot", replacement: "BombMaiden", difficulty: 5),
                Enemy(null, replacement: "Bombbot", difficulty: 3)
            );

            // TURN 5
            neutralBombsAndShields.turns.AddTurn(
                Enemy("Shieldbot", replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP2_PyroBot", difficulty: 3)
            );

            // TURN 6
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 4, overclock: 6, overclockAbility: Ability.DeathShield),
                Enemy("Shieldbot", replacement: "P03KCMXP2_SirBlast", difficulty: 6)
            );

            // TURN 7
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 5)
            );

            // TURN 8
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // // Encounter: Neutral_BridgeBattle
            // EncounterBlueprintData neutralBridgeBattle = EncounterManager.New("P03KCM_Neutral_BridgeBattle", addToPool: true);
            // neutralBridgeBattle.SetDifficulty(0, 6).SetP03Encounter();
            // neutralBridgeBattle.AddRandomReplacementCards("Automaton", "Bombbot", "MineCart", "AlarmBot");
            // neutralBridgeBattle.turns = new();

            // // TURN 1
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy("AlarmBot", replacement: "Shieldbot", difficulty: 4),
            //     Enemy(null, replacement: "SentryBot", difficulty: 3),
            //     Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            // );

            // // TURN 2
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy(null, replacement: "Bombbot", difficulty: 2),
            //     Enemy(null, replacement: "SentryBot", difficulty: 4)
            // );

            // // TURN 3
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy("Automaton", replacement: "CloserBot", difficulty: 4),
            //     Enemy(null, replacement: "SentryBot", difficulty: 3)
            // );

            // // TURN 4
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy(null, replacement: "Bombbot", difficulty: 2),
            //     Enemy(null, replacement: "SentryBot", difficulty: 3)
            // );

            // // TURN 5
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy("SwapBot", replacement: "CloserBot", difficulty: 2)
            // );

            // // TURN 6
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy(null)
            // );

            // // TURN 7
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy("SwapBot", replacement: "CloserBot", difficulty: 4)
            // );

            // // TURN 8
            // neutralBridgeBattle.turns.AddTurn(
            //     Enemy("SwapBot", replacement: "CloserBot", difficulty: 6)
            // );


            // Encounter: Neutral_Minecarts
            EncounterBlueprintData neutralMinecarts = EncounterManager.New("P03KCM_Neutral_Minecarts", addToPool: true);
            neutralMinecarts.SetDifficulty(0, 6).SetP03Encounter();
            neutralMinecarts.turns = new();

            // TURN 1
            neutralMinecarts.turns.AddTurn(
                Enemy("MineCart"),
                Enemy(null, replacement: "MineCart", difficulty: 2)
            );

            // TURN 2
            neutralMinecarts.turns.AddTurn(
                Enemy("LeapBot", replacement: "MineCart", difficulty: 3),
                Enemy("Bombbot", replacement: "MineCart", difficulty: 2, overclock: 4)
            );

            // TURN 3
            neutralMinecarts.turns.AddTurn(
                Enemy("Insectodrone", replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "MineCart", difficulty: 3)
            );

            // TURN 4
            neutralMinecarts.turns.AddTurn(
                Enemy("LeapBot", replacement: "MineCart", difficulty: 2, overclock: 5),
                Enemy(null, replacement: "P03KCMXP1_CopyPasta", difficulty: 5)
            );

            // TURN 5
            neutralMinecarts.turns.AddTurn(
                Enemy("Bombbot", replacement: "P03KCMXP1_CopyPasta", difficulty: 6),
                Enemy("MineCart", overclock: 2)
            );

            // TURN 6
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "MineCart", difficulty: 1, overclock: 2),
                Enemy(null, replacement: "MineCart", difficulty: 3, overclock: 4),
                Enemy(null, replacement: "Insectodrone", difficulty: 6)
            );

            // TURN 7
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 2),
                Enemy("Bombbot", replacement: "LeapBot", difficulty: 3)
            );

            // TURN 8
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 3),
                Enemy(null, replacement: "MineCart", difficulty: 1, overclock: 4)
            );

            // TURN 9
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 3)
            );


            // Encounter: Neutral_SentryWall
            EncounterBlueprintData neutralSentryWall = EncounterManager.New("P03KCM_Neutral_SentryWall", addToPool: true);
            neutralSentryWall.SetDifficulty(0, 6).SetP03Encounter();
            neutralSentryWall.AddRandomReplacementCards("Shieldbot", "Insectodrone", "Bombbot");
            neutralSentryWall.turns = new();

            // TURN 1
            neutralSentryWall.turns.AddTurn(
                Enemy("SentryBot", replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6),
                Enemy("SentryBot", replacement: "Shutterbug", difficulty: 5),
                Enemy("SentryBot", replacement: "RoboSkeleton", difficulty: 4),
                Enemy("SentryBot"),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 2
            neutralSentryWall.turns.AddTurn(
                Enemy("Automaton", replacement: "Shieldbot", difficulty: 3),
                Enemy("RoboSkeleton", replacement: "P03KCMXP2_FlamingExeskeleton", difficulty: 2)
            );

            // TURN 3
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 1),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 3),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 4
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 2),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 5),
                Enemy("SentryBot", replacement: "Shutterbug", difficulty: 5)
            );

            // TURN 5
            neutralSentryWall.turns.AddTurn(
                Enemy("SentryBot", replacement: "Shutterbug", difficulty: 6),
                Enemy(null, replacement: "Automaton", difficulty: 2, overclock: 5, overclockAbility: Ability.Sharp),
                Enemy(null, replacement: "Automaton", difficulty: 4, overclock: 5, overclockAbility: Ability.Sharp),
                Enemy(null, replacement: "Shutterbug", difficulty: 6),
                Enemy(null, replacement: "Automaton", difficulty: 3, overclock: 5, overclockAbility: Ability.Sharp)
            );

            // TURN 6
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 7
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 8
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP1_Spyplane", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 9
            neutralSentryWall.turns.AddTurn(
                Enemy("CloserBot")
            );


            // Encounter: Neutral_Swapbots
            EncounterBlueprintData neutralSwapbots = EncounterManager.New("P03KCM_Neutral_Swapbots", addToPool: true);
            neutralSwapbots.SetDifficulty(0, 6).SetP03Encounter();
            neutralSwapbots.AddRandomReplacementCards("Automaton", "Bombbot", "MineCart");
            neutralSwapbots.turns = new();

            // TURN 1
            neutralSwapbots.turns.AddTurn(
                Enemy("P03KCMXP1_OilJerry"),
                Enemy(null, replacement: "SwapBot", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_OilJerry", difficulty: 2)
            );

            // TURN 2
            neutralSwapbots.turns.AddTurn(
                Enemy("SwapBot"),
                Enemy(null, replacement: "Shutterbug", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP1_OilJerry", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP1_OilJerry", difficulty: 3)
            );

            // TURN 3
            neutralSwapbots.turns.AddTurn(
                Enemy("LeapBot", replacement: "SwapBot", difficulty: 2),
                Enemy("Bombbot", replacement: "SwapBot", difficulty: 4)
            );

            // TURN 4
            neutralSwapbots.turns.AddTurn(
                Enemy(null, replacement: "Shutterbug", difficulty: 6),
                Enemy(null, replacement: "Insectodrone", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP1_OilJerry", difficulty: 3)
            );

            // TURN 5
            neutralSwapbots.turns.AddTurn(
                Enemy("SwapBot"),
                Enemy(null, replacement: "SwapBot", difficulty: 6),
                Enemy(null, replacement: "P03KCMXP1_OilJerry", difficulty: 4)
            );

            // TURN 6
            neutralSwapbots.turns.AddTurn(
                Enemy(null, replacement: "SwapBot", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 6)
            );

            // TURN 7
            neutralSwapbots.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 6)
            );

            // TURN 8
            neutralSwapbots.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 4),
                Enemy(null, replacement: "Automaton", difficulty: 6)
            );


            // Encounter: Neutral_Clockbots
            EncounterBlueprintData neutralClockbots = EncounterManager.New("P03KCM_Neutral_Clockbots", addToPool: true);
            neutralClockbots.SetDifficulty(0, 6).SetP03Encounter();
            neutralClockbots.turns = new();

            // TURN 1
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot", replacement: "P03KCMXP1_Clockbot_Down", difficulty: 4)
            );

            // TURN 2
            neutralClockbots.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Right", difficulty: 2),
                Enemy("P03KCMXP1_Clockbot", replacement: "P03KCMXP1_Clockbot_Left", difficulty: 1)
            );

            // TURN 3
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot", overclock: 3),
                Enemy(null, replacement: "P03KCMXP1_Clockbot", difficulty: 5)
            );

            // TURN 4
            neutralClockbots.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Down", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Right", difficulty: 5)
            );

            // TURN 5
            neutralClockbots.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Clockbot", difficulty: 3, overclock: 5),
                Enemy("P03KCMXP1_Clockbot", replacement: "P03KCMXP1_Clockbot_Left", difficulty: 2)
            );

            // TURN 6
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot_Down", overclock: 2)
            );

            // TURN 7
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot", overclock: 1),
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Right", difficulty: 3)
            );

            // TURN 8
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot", overclock: 4),
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Down", difficulty: 6)
            );

            // TURN 9
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot", overclock: 3)
            );


            // Encounter: Neutral_SpyPlanes
            EncounterBlueprintData neutralSpyPlanes = EncounterManager.New("P03KCM_Neutral_SpyPlanes", addToPool: true);
            neutralSpyPlanes.SetDifficulty(0, 6).SetP03Encounter();
            neutralSpyPlanes.turns = new();

            // TURN 1
            neutralSpyPlanes.turns.AddTurn(
                Enemy("LeapBot")
            );

            // TURN 2
            neutralSpyPlanes.turns.AddTurn(
                Enemy("Automaton"),
                Enemy(null, replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 3
            neutralSpyPlanes.turns.AddTurn(
                Enemy("LeapBot", replacement: "P03KCMXP1_CopyPasta", difficulty: 5),
                Enemy(null, replacement: "LeapBot", difficulty: 1)
            );

            // TURN 4
            neutralSpyPlanes.turns.AddTurn(
                Enemy("Insectodrone", replacement: "P03KCMXP1_Spyplane", difficulty: 1),
                Enemy(null, replacement: "Insectodrone", difficulty: 6)
            );

            // TURN 5
            neutralSpyPlanes.turns.AddTurn(
                Enemy("P03KCMXP1_Spyplane", replacement: "Insectodrone", difficulty: 1)
            );

            // TURN 6
            neutralSpyPlanes.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Spyplane", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP1_EmeraldTitan", difficulty: 6)
            );

            // TURN 7
            neutralSpyPlanes.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Spyplane", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 8
            neutralSpyPlanes.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Spyplane", difficulty: 6)
            );

            // TURN 9
            neutralSpyPlanes.turns.AddTurn(
                Enemy("P03KCMXP1_Spyplane")
            );


            // Encounter: P03FinalBoss
            P03FinalBoss = EncounterManager.New("P03KCM_P03FinalBoss", addToPool: false);
            P03FinalBoss.SetDifficulty(0, 10);
            P03FinalBoss.AddRandomReplacementCards("Automaton", "Thickbot", "AlarmBot", "Insectodrone");
            P03FinalBoss.turns = new();

            // TURN 1
            P03FinalBoss.turns.AddTurn(
                Enemy("SentryBot"),
                Enemy("RoboSkeleton"),
                Enemy("SentryBot"),
                Enemy("RoboSkeleton")
            );

            // TURN 2
            P03FinalBoss.turns.AddTurn(
                Enemy("LatcherBrittle"),
                Enemy("Thickbot"),
                Enemy("Steambot")
            );

            // TURN 3
            P03FinalBoss.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton")
            );

            // TURN 4
            P03FinalBoss.turns.AddTurn(
                Enemy("Steambot"),
                Enemy("Thickbot")
            );

            // TURN 5
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton")
            );

            // TURN 6
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton"),
                Enemy("Automaton")
            );

            // TURN 7
            P03FinalBoss.turns.AddTurn(
                Enemy("GemRipper")
            );

            // TURN 8
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot")
            );

            // TURN 9
            P03FinalBoss.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton")
            );

            // TURN 10
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton")
            );

            // TURN 11
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton"),
                Enemy("Automaton")
            );

            // TURN 12
            P03FinalBoss.turns.AddTurn(
                Enemy("GemRipper")
            );

            // TURN 13
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot")
            );

            // TURN 14
            P03FinalBoss.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton")
            );

            // TURN 15
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton")
            );

            // TURN 16
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton"),
                Enemy("Automaton")
            );

            // TURN 17
            P03FinalBoss.turns.AddTurn(
                Enemy("GemRipper")
            );

            // TURN 18
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot")
            );

            // TURN 19
            P03FinalBoss.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton")
            );

            // TURN 20
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton")
            );

            // TURN 21
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton"),
                Enemy("Automaton")
            );

            // TURN 22
            P03FinalBoss.turns.AddTurn(
                Enemy("GemRipper")
            );

            // TURN 23
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot")
            );

            // TURN 24
            P03FinalBoss.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton")
            );

            // TURN 25
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton")
            );

            // TURN 26
            P03FinalBoss.turns.AddTurn(
                Enemy("Automaton"),
                Enemy("Automaton")
            );

            // TURN 27
            P03FinalBoss.turns.AddTurn(
                Enemy("GemRipper")
            );

            // TURN 28
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot")
            );

            // TURN 29
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 30
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 31
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 32
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 33
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 34
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 35
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 36
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 37
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );

            // TURN 38
            P03FinalBoss.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot"),
                Enemy("CloserBot")
            );


            // Encounter: PhotographerBossP1
            PhotographerBossP1 = EncounterManager.New("P03KCM_PhotographerBossP1", addToPool: false);
            PhotographerBossP1.SetDifficulty(0, 6);
            PhotographerBossP1.turns = new();

            // TURN 1
            PhotographerBossP1.turns.AddTurn(
                Enemy("Shutterbug"),
                Enemy("Shutterbug"),
                Enemy("Shutterbug"),
                Enemy("Shutterbug"),
                Enemy("Shutterbug")
            );

            // TURN 2
            PhotographerBossP1.turns.AddTurn(
                Enemy(null, replacement: "Shutterbug", difficulty: 5),
                Enemy(null, replacement: "Shutterbug", difficulty: 4)
            );

            // TURN 3
            PhotographerBossP1.turns.AddTurn(
                Enemy("BoltHound"),
                Enemy(null, replacement: "AlarmBot", difficulty: 3)
            );

            // TURN 4
            PhotographerBossP1.turns.AddTurn(
                Enemy("XformerBatBot", replacement: "XformerBatBeast", difficulty: 1),
                Enemy("XformerBatBeast"),
                Enemy(null, replacement: "Shutterbug", difficulty: 3)
            );

            // TURN 5
            PhotographerBossP1.turns.AddTurn(
                Enemy("XformerPorcupineBeast"),
                Enemy("XformerPorcupineBot", replacement: "XformerPorcupineBeast", difficulty: 1)
            );

            // TURN 6
            PhotographerBossP1.turns.AddTurn(
                Enemy(null, replacement: "Shutterbug", difficulty: 4)
            );

            // TURN 7
            PhotographerBossP1.turns.AddTurn(
                Enemy("Shutterbug"),
                Enemy("Shutterbug"),
                Enemy(null, replacement: "Shutterbug", difficulty: 3)
            );

            // TURN 8
            PhotographerBossP1.turns.AddTurn(
                Enemy("BoltHound", replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: PhotographerBossP2
            PhotographerBossP2 = EncounterManager.New("P03KCM_PhotographerBossP2", addToPool: false);
            PhotographerBossP2.SetDifficulty(10, 10);
            PhotographerBossP2.turns = new();

            // TURN 1
            PhotographerBossP2.turns.AddTurn(
                Enemy("XformerGrizzlyBot", replacement: "XformerGrizzlyBeast", difficulty: 2),
                Enemy("XformerGrizzlyBot")
            );

            // TURN 2
            PhotographerBossP2.turns.AddTurn(
                Enemy(null, replacement: "XformerPorcupineBot", difficulty: 6)
            );

            // TURN 3
            PhotographerBossP2.turns.AddTurn(
                Enemy("Shutterbug"),
                Enemy(null, replacement: "Shutterbug", difficulty: 3)
            );

            // TURN 4
            PhotographerBossP2.turns.AddTurn(
                Enemy("Shutterbug"),
                Enemy(null, replacement: "Shutterbug", difficulty: 2)
            );

            // TURN 5
            PhotographerBossP2.turns.AddTurn(
                Enemy("Shutterbug"),
                Enemy(null, replacement: "Shutterbug", difficulty: 5)
            );

            // TURN 6
            PhotographerBossP2.turns.AddTurn(
                Enemy(null, replacement: "Shutterbug", difficulty: 4)
            );

            // TURN 7
            PhotographerBossP2.turns.AddTurn(
                Enemy("XformerGrizzlyBot", replacement: "XformerGrizzlyBeast", difficulty: 1),
                Enemy("XformerGrizzlyBot")
            );

            // Encounter: Tech_AttackConduits
            EncounterBlueprintData techObnoxiousConduits = EncounterManager.New("P03KCM_Tech_ObnoxiousConduits", addToPool: true);
            techObnoxiousConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techObnoxiousConduits.turns = new();

            // TURN 1
            techObnoxiousConduits.turns.AddTurn(
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 1),
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 1),
                Enemy("P03KCMXP2_UrchinCell", overclock: 3)
            );

            // TURN 2
            techObnoxiousConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "CellGift", difficulty: 2)
            );

            // TURN 3
            techObnoxiousConduits.turns.AddTurn(
                Enemy("P03KCMXP2_Suicell"),
                Enemy(null, replacement: "Shieldbot", difficulty: 4),
                Enemy(null, replacement: "ConduitTower", difficulty: 3, overclock: 4)
            );

            // TURN 4
            techObnoxiousConduits.turns.AddTurn(
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 1),
                Enemy("P03KCMXP2_UrchinCell", overclock: 3),
                Enemy(null, replacement: "CellBuff", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP2_LockjawCell", difficulty: 5)
            );

            // TURN 5
            techObnoxiousConduits.turns.AddTurn(
                Enemy("NullConduit", replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 6),
                Enemy("P03KCMXP2_UrchinCell", replacement: "P03KCMXP2_LockjawCell", difficulty: 5)
            );

            // TURN 6
            techObnoxiousConduits.turns.AddTurn(
                Enemy("P03KCMXP2_UrchinCell", replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP2_Suicell", difficulty: 2)
            );

            // TURN 7
            techObnoxiousConduits.turns.AddTurn(
                Enemy("ConduitTower", replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP2_LockjawCell", difficulty: 6)
            );

            // TURN 8
            techObnoxiousConduits.turns.AddTurn(
                Enemy(null, replacement: "CellBuff", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 9
            techObnoxiousConduits.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 3)
            );


            // Encounter: Tech_AttackConduits
            EncounterBlueprintData techAttackConduits = EncounterManager.New("P03KCM_Tech_AttackConduits", addToPool: true);
            techAttackConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techAttackConduits.turns = new();

            // TURN 1
            techAttackConduits.turns.AddTurn(
                Enemy("AttackConduit"),
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 1),
                Enemy("LeapBot", replacement: "CellGift", difficulty: 3)
            );

            // TURN 2
            techAttackConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "CellGift", difficulty: 2)
            );

            // TURN 3
            techAttackConduits.turns.AddTurn(
                Enemy("Insectodrone", replacement: "Shieldbot", difficulty: 4),
                Enemy("LeapBot", replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "ConduitTower", difficulty: 3, overclock: 4)
            );

            // TURN 4
            techAttackConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "CellBuff", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP1_EmeraldTitan", difficulty: 5)
            );

            // TURN 5
            techAttackConduits.turns.AddTurn(
                Enemy("NullConduit", replacement: "AttackConduit", difficulty: 2),
                Enemy(null, replacement: "AttackConduit", difficulty: 6),
                Enemy(null, replacement: "LeapBot", difficulty: 4)
            );

            // TURN 6
            techAttackConduits.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 7
            techAttackConduits.turns.AddTurn(
                Enemy("ConduitTower", replacement: "AttackConduit", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_EmeraldTitan", difficulty: 6)
            );

            // TURN 8
            techAttackConduits.turns.AddTurn(
                Enemy(null, replacement: "CellBuff", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 9
            techAttackConduits.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 3)
            );


            // // Encounter: Tech_GiftCells
            // EncounterBlueprintData techGiftCells = EncounterManager.New("P03KCM_Tech_GiftCells", addToPool: true);
            // techGiftCells.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            // techGiftCells.turns = new();

            // // TURN 1
            // techGiftCells.turns.AddTurn(
            //     Enemy("AttackConduit"),
            //     Enemy("NullConduit", replacement: "AttackConduit", difficulty: 3),
            //     Enemy("CellGift"),
            //     Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            // );

            // // TURN 2
            // techGiftCells.turns.AddTurn(
            //     Enemy("CellGift", replacement: "Shieldbot", difficulty: 6),
            //     Enemy(null, replacement: "NullConduit", difficulty: 3)
            // );

            // // TURN 3
            // techGiftCells.turns.AddTurn(
            //     Enemy("NullConduit"),
            //     Enemy(null, replacement: "CellGift", difficulty: 2),
            //     Enemy(null, replacement: "FactoryConduit", difficulty: 3),
            //     Enemy(null, replacement: "AttackConduit", difficulty: 5)
            // );

            // // TURN 4
            // techGiftCells.turns.AddTurn(
            //     Enemy("CellGift", replacement: "Automaton", difficulty: 1),
            //     Enemy(null, replacement: "Bombbot", difficulty: 6),
            //     Enemy(null, replacement: "NullConduit", difficulty: 4)
            // );

            // // TURN 5
            // techGiftCells.turns.AddTurn(
            //     Enemy(null, replacement: "Insectodrone", difficulty: 5),
            //     Enemy("CellGift")
            // );

            // // TURN 6
            // techGiftCells.turns.AddTurn(
            //     Enemy(null, replacement: "AttackConduit", difficulty: 4),
            //     Enemy("AttackConduit"),
            //     Enemy(null, replacement: "Insectodrone", difficulty: 2),
            //     Enemy(null, replacement: "NullConduit", difficulty: 3)
            // );

            // // TURN 7
            // techGiftCells.turns.AddTurn(
            //     Enemy("CellGift", replacement: "CellBuff", difficulty: 4)
            // );

            // // TURN 8
            // techGiftCells.turns.AddTurn(
            //     Enemy("GiftBot", replacement: "CellGift", difficulty: 2),
            //     Enemy("LeapBot", replacement: "MineCart", difficulty: 4),
            //     Enemy(null, replacement: "AttackConduit", difficulty: 3)
            // );

            // // TURN 9
            // techGiftCells.turns.AddTurn(
            //     Enemy(null, replacement: "CloserBot", difficulty: 6)
            // );


            // Encounter: Tech_SplinterCells
            EncounterBlueprintData techSplinterCells = EncounterManager.New("P03KCM_Tech_SplinterCells", addToPool: true);
            techSplinterCells.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techSplinterCells.turns = new();

            // TURN 1
            techSplinterCells.turns.AddTurn(
                Enemy("CellTri"),
                Enemy(null, replacement: "Automaton", difficulty: 6),
                Enemy(null, replacement: "NullConduit", difficulty: 3)
            );

            // TURN 2
            techSplinterCells.turns.AddTurn(
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 4),
                Enemy("NullConduit")
            );

            // TURN 3
            techSplinterCells.turns.AddTurn(
                Enemy("Automaton", replacement: "CellBuff", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP2_Elektron", difficulty: 5)
            );

            // TURN 4
            techSplinterCells.turns.AddTurn(
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 1),
                Enemy(null, replacement: "CellBuff", difficulty: 4),
                Enemy(null, replacement: "AttackConduit", difficulty: 3)
            );

            // TURN 5
            techSplinterCells.turns.AddTurn(
                Enemy(null, replacement: "CellTri", difficulty: 5)
            );

            // TURN 6
            techSplinterCells.turns.AddTurn(
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP2_Elektron", difficulty: 6)
            );

            // TURN 7
            techSplinterCells.turns.AddTurn(
                Enemy(null, replacement: "CellBuff", difficulty: 3)
            );

            // TURN 8
            techSplinterCells.turns.AddTurn(
                Enemy("Automaton", replacement: "CellBuff", difficulty: 3)
            );

            // TURN 9
            techSplinterCells.turns.AddTurn(
                Enemy(null, replacement: "CellTri", difficulty: 4)
            );


            // Encounter: Tech_ProtectConduits
            EncounterBlueprintData techProtectConduits = EncounterManager.New("P03KCM_Tech_ProtectConduits", addToPool: true);
            techProtectConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techProtectConduits.turns = new();

            // TURN 1
            techProtectConduits.turns.AddTurn(
                Enemy("P03KCMXP1_ConduitProtector")
            );

            // TURN 2
            techProtectConduits.turns.AddTurn(
                Enemy("NullConduit", replacement: "ConduitTower", difficulty: 1),
                Enemy("HealerConduit")
            );

            // TURN 3
            techProtectConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Thickbot", difficulty: 1, overclock: 2),
                Enemy("Thickbot", replacement: "SwapBot", difficulty: 5)
            );

            // TURN 4
            techProtectConduits.turns.AddTurn(
                Enemy("HealerConduit", replacement: "AttackConduit", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_ConduitProtector", difficulty: 1)
            );

            // TURN 5
            techProtectConduits.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_EmeraldTitan", difficulty: 5)
            );

            // TURN 6
            techProtectConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Thickbot", difficulty: 1, overclock: 2)
            );

            // TURN 7
            techProtectConduits.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_ConduitProtector", difficulty: 2),
                Enemy("LeapBot", replacement: "Thickbot", difficulty: 1, overclock: 2)
            );

            // TURN 8
            techProtectConduits.turns.AddTurn(
                Enemy(null, replacement: "NullConduit", difficulty: 3, overclock: 5),
                Enemy(null, replacement: "HealerConduit", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_EmeraldTitan", difficulty: 6)
            );


            // Encounter: Tech_StinkyConduits
            EncounterBlueprintData techStinkyConduits = EncounterManager.New("P03KCM_Tech_StinkyConduits", addToPool: true);
            techStinkyConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techStinkyConduits.turns = new();

            // TURN 1
            techStinkyConduits.turns.AddTurn(
                Enemy("P03KCMXP1_ConduitDebuffEnemy", overclock: 3, overclockAbility: Ability.DebuffEnemy),
                Enemy("NullConduit", replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 5, overclock: 4, overclockAbility: Ability.DebuffEnemy),
                Enemy("LeapBot", replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 2
            techStinkyConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "MineCart", difficulty: 3),
                Enemy("LeapBot", replacement: "CellGift", difficulty: 4)
            );

            // TURN 3
            techStinkyConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Shieldbot", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 3, overclock: 4, overclockAbility: Ability.DebuffEnemy)
            );

            // TURN 4
            techStinkyConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "MineCart", difficulty: 2, overclock: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 5
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "AttackConduit", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 6),
                Enemy(null, replacement: "LeapBot", difficulty: 4)
            );

            // TURN 6
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "LeapBot", difficulty: 3),
                Enemy(null, replacement: "P03KCMXP1_RubyTitan", difficulty: 5)
            );

            // TURN 7
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 6),
                Enemy("Insectodrone", replacement: "LeapBot", difficulty: 5)
            );

            // TURN 8
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 9
            techStinkyConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Automaton", difficulty: 4),
                Enemy("LeapBot", replacement: "MineCart", difficulty: 2)
            );

            // TURN 10
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_RubyTitan", difficulty: 6)
            );


            // Encounter: Undead_BombLatchers
            EncounterBlueprintData undeadBombLatchers = EncounterManager.New("P03KCM_Undead_BombLatchers", addToPool: true);
            undeadBombLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadBombLatchers.turns = new();

            // TURN 1
            undeadBombLatchers.turns.AddTurn(
                Enemy("LatcherBomb"),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2)
            );

            // TURN 2
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP1_RoboAngel", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 2, overclock: 4)
            );

            // TURN 3
            undeadBombLatchers.turns.AddTurn(
                Enemy("LatcherBomb", overclock: 2),
                Enemy("Bombbot"),
                Enemy(null, replacement: "Shieldbot", difficulty: 3)
            );

            // TURN 4
            undeadBombLatchers.turns.AddTurn(
                Enemy("BoltHound", replacement: "CloserBot", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP1_Necrobot", difficulty: 6),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "P03KCMXP1_Executor", difficulty: 3),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 6
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "LatcherBomb", difficulty: 2, overclock: 3),
                Enemy(null, replacement: "Shieldbot", difficulty: 4)
            );

            // TURN 7
            undeadBombLatchers.turns.AddTurn(
                Enemy("LatcherBomb", overclock: 1)
            );

            // TURN 8
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 4)
            );

            // TURN 9
            undeadBombLatchers.turns.AddTurn(
                Enemy("BoltHound", replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Undead_ShieldLatchers
            EncounterBlueprintData undeadShieldLatchers = EncounterManager.New("P03KCM_Undead_ShieldLatchers", addToPool: true);
            undeadShieldLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadShieldLatchers.turns = new();

            // TURN 1
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield"),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2)
            );

            // TURN 2
            undeadShieldLatchers.turns.AddTurn(
                Enemy("MineCart"),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 2, overclock: 3)
            );

            // TURN 3
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield", overclock: 1),
                Enemy("Bombbot", replacement: "Insectodrone", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_RoboAngel", difficulty: 5)
            );

            // TURN 4
            undeadShieldLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_Executor", replacement: "Thickbot", difficulty: 4),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 2, overclock: 3),
                Enemy("MineCart", replacement: "P03KCMXP1_Necrobot", difficulty: 6)
            );

            // TURN 5
            undeadShieldLatchers.turns.AddTurn(
                Enemy("MineCart", replacement: "Insectodrone", difficulty: 6),
                Enemy(null, replacement: "LatcherShield", difficulty: 3),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 1, overclock: 3)
            );

            // TURN 6
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield", overclock: 4),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 7
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield", overclock: 0, replacement: null, difficulty: 4),
                Enemy(null, replacement: "CloserBot", difficulty: 4)
            );

            // TURN 8
            undeadShieldLatchers.turns.AddTurn(
                Enemy(null, replacement: "LatcherShield", difficulty: 2, overclock: 2)
            );

            // TURN 9
            undeadShieldLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_Executor", replacement: "CloserBot", difficulty: 6)
            );

            // Encounter: Undead_FlamingSkeleSwarm
            EncounterBlueprintData undeadFlamingSkeleSwarm = EncounterManager.New("P03KCM_Undead_Flaming_SkeleSwarm", addToPool: true);
            undeadFlamingSkeleSwarm.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadFlamingSkeleSwarm.turns = new();

            // TURN 1
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy("P03KCMXP2_FlamingExeskeleton"),
                Enemy("P03KCMXP2_FlamingExeskeleton"),
                Enemy(null, replacement: "SentryBot", difficulty: 1),
                Enemy(null, replacement: "Shutterbug", difficulty: 5)
            );

            // TURN 2
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy("P03KCMXP2_FlamingExeskeleton", replacement: "P03KCMXP1_ZombieProcess", difficulty: 6, overclock: 6, overclockAbility: FireBomb.AbilityID),
                Enemy("P03KCMXP2_FlamingExeskeleton", replacement: "P03KCMXP2_PyroBot", difficulty: 3)
            );

            // TURN 4
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP2_FlamingExeskeleton", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy("P03KCMXP2_FlamingExeskeleton", overclock: 2),
                Enemy("RoboSkeleton", replacement: "P03KCMXP2_PyroBot", difficulty: 6),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 2)
            );

            // TURN 6
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy("Insectodrone"),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 7
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy("P03KCMXP2_FlamingExeskeleton", replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 8
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP2_FlamingExeskeleton", difficulty: 4)
            );

            // TURN 9
            undeadFlamingSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Undead_SkeleSwarm
            EncounterBlueprintData undeadSkeleSwarm = EncounterManager.New("P03KCM_Undead_SkeleSwarm", addToPool: true);
            undeadSkeleSwarm.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadSkeleSwarm.turns = new();

            // TURN 1
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton"),
                Enemy(null, replacement: "SentryBot", difficulty: 1),
                Enemy(null, replacement: "Shutterbug", difficulty: 5)
            );

            // TURN 2
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_ZombieProcess", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton", replacement: "P03KCMXP1_ZombieProcess", difficulty: 6),
                Enemy("RoboSkeleton", replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 4
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton", overclock: 2),
                Enemy("RoboSkeleton", replacement: "Insectodrone", difficulty: 6),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 2)
            );

            // TURN 6
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("Insectodrone"),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 7
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton", replacement: "Insectodrone", difficulty: 4),
                Enemy("RoboSkeleton", replacement: "Insectodrone", difficulty: 6, overclock: 3)
            );

            // TURN 8
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 4)
            );

            // TURN 9
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Sanity check
            EncounterManager.SyncEncounterList();
            EncounterBlueprintData test = EncounterManager.AllEncountersCopy.First(ebd => ebd.name == undeadSkeleSwarm.name);
            foreach (EncounterBlueprintData.CardBlueprint cbp in test.turns[6].Where(c => c.card != null))
                P03Plugin.Log.LogInfo($"Turn 7 of Skeleswarm. Copy of {cbp.card.name} has {cbp.card.mods.Count} mods");
            foreach (EncounterBlueprintData.CardBlueprint cbp in undeadSkeleSwarm.turns[6].Where(c => c.card != null))
                P03Plugin.Log.LogInfo($"Turn 7 of Skeleswarm. ORIGINAL of {cbp.card.name} has {cbp.card.mods.Count} mods");

            // Encounter: Undead_WingLatchers
            EncounterBlueprintData undeadWingLatchers = EncounterManager.New("P03KCM_Undead_WingLatchers", addToPool: true);
            undeadWingLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadWingLatchers.turns = new();

            // TURN 1
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_FlyingLatcher", overclock: 2)
            );

            // TURN 2
            undeadWingLatchers.turns.AddTurn(
                Enemy("Automaton", replacement: "P03KCMXP1_RoboAngel", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP1_FlyingLatcher", difficulty: 1, overclock: 3)
            );

            // TURN 3
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_FlyingLatcher", overclock: 6),
                Enemy("Automaton", replacement: "Thickbot", difficulty: 2),
                Enemy("Bombbot")
            );

            // TURN 4
            undeadWingLatchers.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Executor", difficulty: 1),
                Enemy(null, replacement: "P03KCMXP1_FlyingLatcher", difficulty: 5, overclock: 6),
                Enemy(null, replacement: "P03KCMXP1_Necrobot", difficulty: 6)
            );

            // TURN 5
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_FlyingLatcher", overclock: 3),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4, overclock: 5)
            );

            // TURN 6
            undeadWingLatchers.turns.AddTurn(
                Enemy("Thickbot", overclock: 3),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 7
            undeadWingLatchers.turns.AddTurn(
                Enemy("Thickbot", replacement: "CloserBot", difficulty: 6),
                Enemy("MineCart")
            );


            // Encounter: Undead_StrafeLatchers
            EncounterBlueprintData undeadStrafeLatchers = EncounterManager.New("P03KCM_Undead_StrafeLatchers", addToPool: true);
            undeadStrafeLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadStrafeLatchers.turns = new();

            // TURN 1
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher", overclock: 2),
                Enemy(null, "SentryBot", difficulty: 2, overclock: 5, overclockAbility: Ability.Sharp)
            );

            // TURN 2
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher", replacement: "P03KCMXP1_RoboAngel", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP1_ConveyorLatcher", difficulty: 1, overclock: 2),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy("MineCart", replacement: "P03KCMXP1_RoboAngel", difficulty: 6),
                Enemy(null, "SentryBot", difficulty: 2, overclock: 5, overclockAbility: Ability.Sharp)
            );

            // TURN 4
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher", overclock: 6),
                Enemy("MineCart"),
                Enemy(null, replacement: "MineCart", difficulty: 2),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy("MineCart"),
                Enemy(null, replacement: "P03KCMXP1_ConveyorLatcher", difficulty: 1)
            );

            // TURN 6
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("MineCart", replacement: "CloserBot", difficulty: 3),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 7
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("MineCart", replacement: "CloserBot", difficulty: 6),
                Enemy("MineCart")
            );


            // Encounter: Wizard_BigRipper
            EncounterBlueprintData wizardBigRipper = EncounterManager.New("P03KCM_Wizard_BigRipper", addToPool: true);
            wizardBigRipper.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardBigRipper.turns = new();

            // TURN 1
            wizardBigRipper.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "EmptyVessel_GreenGem", difficulty: 4),
                Enemy("GemRipper")
            );

            // TURN 2
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 5)
            );

            // TURN 3
            wizardBigRipper.turns.AddTurn(
                Enemy("Bombbot", replacement: null, difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 4),
                Enemy("SentinelGreen", replacement: "TechMoxTriple", difficulty: 6, overclock: 6)
            );

            // TURN 4
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 2),
                Enemy(null, replacement: "SentinelBlue", difficulty: 3),
                Enemy("GemRipper")
            );

            // TURN 5
            wizardBigRipper.turns.AddTurn(
                Enemy("SwapBot"),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 1),
                Enemy(null, replacement: "TechMoxTriple", difficulty: 5, overclock: 5)
            );

            // TURN 6
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "SentinelGreen", difficulty: 3)
            );

            // TURN 7
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 3)
            );

            // TURN 8
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 3),
                Enemy(null, replacement: "SentinelGreen", difficulty: 3)
            );

            // TURN 9
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 4)
            );


            // Encounter: Wizard_GemExploder
            EncounterBlueprintData wizardGemExploder = EncounterManager.New("P03KCM_Wizard_GemExploder", addToPool: true);
            wizardGemExploder.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardGemExploder.turns = new();

            // TURN 1
            wizardGemExploder.turns.AddTurn(
                Enemy("Automaton", replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 1),
                Enemy(null, replacement: "SentinelOrange", difficulty: 3)
            );

            // TURN 2
            wizardGemExploder.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "SentinelGreen", difficulty: 4),
                Enemy("GemExploder")
            );

            // TURN 3
            wizardGemExploder.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "SentinelGreen", difficulty: 3),
                Enemy("Bombbot", replacement: "GemExploder", difficulty: 1),
                Enemy(null, replacement: "BombMaiden", difficulty: 6)
            );

            // TURN 4
            wizardGemExploder.turns.AddTurn(
                Enemy("SentinelGreen", replacement: "GemRipper", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3),
                Enemy(null, replacement: "GemExploder", difficulty: 2)
            );

            // TURN 5
            wizardGemExploder.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "SentinelGreen", difficulty: 3),
                Enemy(null, replacement: "TechMoxTriple", difficulty: 5)
            );

            // TURN 6
            wizardGemExploder.turns.AddTurn(
                Enemy("SentinelGreen"),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 2),
                Enemy(null, replacement: "GemExploder", difficulty: 3)
            );

            // TURN 7
            wizardGemExploder.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 2)
            );

            // TURN 8
            wizardGemExploder.turns.AddTurn(
                Enemy(null, replacement: "SentinelGreen", difficulty: 3),
                Enemy(null, replacement: "GemExploder", difficulty: 4)
            );

            // TURN 9
            wizardGemExploder.turns.AddTurn(
                Enemy("GemRipper"),
                Enemy(null, replacement: "SentinelOrange", difficulty: 5)
            );


            // Encounter: Wizard_ShieldGems
            EncounterBlueprintData wizardShieldGems = EncounterManager.New("P03KCM_Wizard_ShieldGems", addToPool: true);
            wizardShieldGems.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardShieldGems.turns = new();

            // TURN 1
            wizardShieldGems.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", overclock: 5),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3)
            );

            // TURN 2
            wizardShieldGems.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "SentinelGreen", difficulty: 3),
                Enemy("Bombbot", replacement: "GemShielder", difficulty: 3, overclock: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 2, overclock: 3)
            );

            // TURN 3
            wizardShieldGems.turns.AddTurn(
                Enemy("GemShielder", overclock: 2),
                Enemy(null, replacement: "GemShielder", difficulty: 3, overclock: 4),
                Enemy(null, replacement: "TechMoxTriple", difficulty: 5),
                Enemy(null, replacement: "SentinelGreen", difficulty: 3)
            );

            // TURN 4
            wizardShieldGems.turns.AddTurn(
                Enemy("SentinelGreen", replacement: "GemRipper", difficulty: 4),
                Enemy(null, replacement: "SentinelGreen", difficulty: 2),
                Enemy(null, replacement: "Automaton", difficulty: 5)
            );

            // TURN 5
            wizardShieldGems.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "GemShielder", difficulty: 3, overclock: 6)
            );

            // TURN 6
            wizardShieldGems.turns.AddTurn(
                Enemy(null, replacement: "SentinelGreen", difficulty: 1, overclock: 3),
                Enemy(null, replacement: "TechMoxTriple", difficulty: 5)
            );

            // TURN 7
            wizardShieldGems.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 3),
                Enemy("SentinelGreen", replacement: "SentinelBlue", difficulty: 4)
            );

            // TURN 8
            wizardShieldGems.turns.AddTurn(
                Enemy("SentinelGreen", overclock: 5),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 4)
            );

            // TURN 9
            wizardShieldGems.turns.AddTurn(
                Enemy("GemRipper"),
                Enemy(null, replacement: "GemRipper", difficulty: 6),
                Enemy(null, replacement: "GemShielder", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3)
            );

            // Damage Race
            GeneratorDamageRace = EncounterManager.New("P03KCM_GeneratorDamageRace", addToPool: false);
            GeneratorDamageRace.SetDifficulty(0, 6);
            GeneratorDamageRace.turns = new();
            GeneratorDamageRace.turns.AddTurn(
                Enemy("SentryBot")
            );

            // Encounter: Wizard_Guardians
            EncounterBlueprintData wizardGuardians = EncounterManager.New("P03KCM_Wizard_Guardians", addToPool: true);
            wizardGuardians.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardGuardians.turns = new();

            // TURN 1
            wizardGuardians.turns.AddTurn(
                Enemy("EmptyVessel_GreenGem", overclock: 4, overclockAbility: Ability.GainGemOrange)
            );

            // TURN 2
            wizardGuardians.turns.AddTurn(
                Enemy("EmptyVessel_GreenGem", overclock: 4, overclockAbility: Ability.GainGemOrange),
                Enemy("EmptyVessel_GreenGem", overclock: 4, overclockAbility: Ability.GainGemOrange)
            );

            // TURN 3
            wizardGuardians.turns.AddTurn(
                Enemy("P03KCMXP2_EmeraldSquid", overclock: 3, overclockAbility: Ability.GainGemGreen)
            );

            // TURN 4
            wizardGuardians.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP2_EmeraldSquid", difficulty: 1, overclock: 3, overclockAbility: Ability.GainGemGreen)
            );

            // TURN 5
            wizardGuardians.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP2_EmeraldGuardian", difficulty: 2, overclock: 5),
                Enemy(null, replacement: "P03KCMXP2_RubyGuardian", difficulty: 3, overclock: 5)
            );

            // TURN 6
            wizardGuardians.turns.AddTurn(
                Enemy("P03KCMXP2_EmeraldSquid", overclock: 3, overclockAbility: Ability.GainGemGreen),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 2, overclock: 4, overclockAbility: Ability.GainGemOrange)
            );

            // TURN 7
            wizardGuardians.turns.AddTurn(
                Enemy("P03KCMXP2_EmeraldSquid", overclock: 3, overclockAbility: Ability.GainGemGreen),
                Enemy("Automaton", replacement: "P03KCMXP2_GemAugur", difficulty: 5, overclock: 6),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 2, overclock: 4, overclockAbility: Ability.GainGemOrange)
            );

            // TURN 8
            wizardGuardians.turns.AddTurn(
                Enemy("P03KCMXP2_EmeraldSquid", overclock: 4, overclockAbility: Ability.Submerge),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 1, overclock: 4, overclockAbility: Ability.GainGemOrange)
            );

            // TURN 9
            wizardGuardians.turns.AddTurn(
                Enemy("Automaton", replacement: "P03KCMXP2_GemAugur", difficulty: 5, overclock: 6),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 2, overclock: 4, overclockAbility: Ability.GainGemOrange)
            );

        }
    }
}
