using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Encounters
{
    [HarmonyPatch]
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
        public static List<EncounterBlueprintData.CardBlueprint> Enemy(string cardName, string replacement = null, int difficulty = 0, int random = 0, int overclock = 0)
        {
            List<EncounterBlueprintData.CardBlueprint> retval = new();

            EncounterBlueprintData.CardBlueprint baseBp = new();
            baseBp.card = String.IsNullOrEmpty(cardName) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(cardName));

            if (overclock == 0 && difficulty == 0)
                baseBp.maxDifficulty = MAX_DIFFICULTY;
            else if (overclock == 0 && difficulty > 0)
                baseBp.maxDifficulty = difficulty - 1;
            else if (overclock > 0 && difficulty == 0)
                baseBp.maxDifficulty = overclock - 1;
            else
                baseBp.maxDifficulty = Math.Min(overclock, difficulty) - 1;
            
            if (random > 0)
                baseBp.randomReplaceChance = random;
            
            retval.Add(baseBp);

            if (difficulty > 0 && replacement != null)
            {
                EncounterBlueprintData.CardBlueprint diff = new();
                diff.card = String.IsNullOrEmpty(replacement) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(replacement));
                diff.minDifficulty = difficulty;
                if (overclock > difficulty)
                    diff.maxDifficulty = overclock - 1;
                else
                    diff.maxDifficulty = MAX_DIFFICULTY;

                if (difficulty > overclock && overclock > 0)
                    diff.card.Mods.Add(new (1, 0) { fromOverclock = true });
                    
                retval.Add(baseBp);
            }

            if (overclock > 0)
            {
                EncounterBlueprintData.CardBlueprint ov = new();
                ov.minDifficulty = overclock;
                if (overclock > difficulty && difficulty > 0)
                {
                    ov.card = String.IsNullOrEmpty(replacement) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(replacement));
                    ov.maxDifficulty = MAX_DIFFICULTY;
                }
                else
                {
                    ov.card = String.IsNullOrEmpty(cardName) ? null : CardLoader.Clone(CardManager.AllCardsCopy.CardByName(cardName));
                    if (difficulty > overclock)
                        ov.maxDifficulty = difficulty - 1;
                    else
                        ov.maxDifficulty = MAX_DIFFICULTY;
                }
                ov.card.Mods.Add(new (1, 0) { fromOverclock = true });
                retval.Add(baseBp);
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
            CanvasBossPX = EncounterManager.New("CanvasBossPX", addToPool: false);
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
            EncounterBlueprintData natureBatTransformers = EncounterManager.New("Nature_BatTransformers", addToPool: true);
            natureBatTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureBatTransformers.turns = new();

            // TURN 1
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBot", replacement: "XformerBatBeast", difficulty: 2),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBot", replacement: "XformerBatBeast", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 3
            natureBatTransformers.turns.AddTurn(
                Enemy(null, replacement: "XformerBatBeast", difficulty: 4),
                Enemy(null, replacement: "Bombbot", difficulty: 6),
                Enemy(null, replacement: "CXformerAdder", difficulty: 4)
            );

            // TURN 4
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBot", replacement: "XformerBatBeast", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 5
            natureBatTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "XformerPorcupineBeast", difficulty: 2),
                Enemy("Shieldbot", replacement: "XformerGrizzlyBot", difficulty: 6)
            );

            // TURN 6
            natureBatTransformers.turns.AddTurn(
                Enemy(null, replacement: "XformerPorcupineBeast", difficulty: 4)
            );

            // TURN 7
            natureBatTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "BoltHound", difficulty: 6)
            );

            // TURN 8
            natureBatTransformers.turns.AddTurn(
                Enemy("XformerBatBeast", replacement: "XformerGrizzlyBot", difficulty: 6)
            );


            // Encounter: Nature_BearTransformers
            EncounterBlueprintData natureBearTransformers = EncounterManager.New("Nature_BearTransformers", addToPool: true);
            natureBearTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureBearTransformers.turns = new();

            // TURN 1
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerGrizzlyBot"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerPorcupineBot", replacement: "XformerBatBeast", difficulty: 4)
            );

            // TURN 3
            natureBearTransformers.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 4
            natureBearTransformers.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 4)
            );

            // TURN 5
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerGrizzlyBot", replacement: "XformerGrizzlyBeast", difficulty: 4),
                Enemy("Shieldbot", replacement: "XformerGrizzlyBot", difficulty: 6),
                Enemy(null, replacement: "CXformerAdder", difficulty: 4)
            );

            // TURN 6
            natureBearTransformers.turns.AddTurn(
                Enemy(null, replacement: "XformerGrizzlyBot", difficulty: 6),
                Enemy(null, replacement: "Shieldbot", difficulty: 2),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 7
            natureBearTransformers.turns.AddTurn(
                Enemy("Bombbot", replacement: "BoltHound", difficulty: 4)
            );

            // TURN 8
            natureBearTransformers.turns.AddTurn(
                Enemy("XformerBatBeast", replacement: "XformerGrizzlyBeast", difficulty: 6)
            );


            // Encounter: Nature_Hounds
            EncounterBlueprintData natureHounds = EncounterManager.New("Nature_Hounds", addToPool: true);
            natureHounds.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureHounds.turns = new();

            // TURN 1
            natureHounds.turns.AddTurn(
                Enemy("XformerPorcupineBeast", replacement: "XformerPorcupineBot", difficulty: 2),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            natureHounds.turns.AddTurn(
                Enemy("BoltHound"),
                Enemy(null, replacement: "AlarmBot", difficulty: 4)
            );

            // TURN 3
            natureHounds.turns.AddTurn(
                Enemy(null, replacement: "XformerBatBot", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 4
            natureHounds.turns.AddTurn(
                Enemy("XformerPorcupineBeast", replacement: "XformerPorcupineBot", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 4)
            );

            // TURN 5
            natureHounds.turns.AddTurn(
                Enemy("Bombbot", replacement: "XformerPorcupineBeast", random: 0),
                Enemy("Shieldbot", replacement: "XformerGrizzlyBot", difficulty: 4)
            );

            // TURN 6
            natureHounds.turns.AddTurn(
                Enemy(null, replacement: "XformerPorcupineBeast", difficulty: 6)
            );

            // TURN 7
            natureHounds.turns.AddTurn(
                Enemy("Bombbot", replacement: "BoltHound", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 8
            natureHounds.turns.AddTurn(
                Enemy("XformerPorcupineBeast", replacement: "XformerGrizzlyBot", difficulty: 6)
            );


            // Encounter: Nature_WolfTransformers
            EncounterBlueprintData natureWolfTransformers = EncounterManager.New("Nature_WolfTransformers", addToPool: true);
            natureWolfTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureWolfTransformers.turns = new();

            // TURN 1
            natureWolfTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_SeedBot"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            natureWolfTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_WolfBot"),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 3
            natureWolfTransformers.turns.AddTurn(
                Enemy(null, replacement: "XformerPorcupineBot", difficulty: 2)
            );

            // TURN 4
            natureWolfTransformers.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_WolfBot", difficulty: 1),
                Enemy(null, replacement: "Shieldbot", difficulty: 2),
                Enemy(null, replacement: "CXformerAdder", difficulty: 4)
            );

            // TURN 5
            natureWolfTransformers.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_WolfBot", difficulty: 4),
                Enemy(null, replacement: "CXformerAdder", difficulty: 3)
            );

            // TURN 6
            natureWolfTransformers.turns.AddTurn(
                Enemy("XformerBatBeast")
            );

            // TURN 7
            natureWolfTransformers.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_WolfBeast", difficulty: 6)
            );


            // Encounter: Nature_SnakeTransformers
            EncounterBlueprintData natureSnakeTransformers = EncounterManager.New("Nature_SnakeTransformers", addToPool: true);
            natureSnakeTransformers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Nature);
            natureSnakeTransformers.turns = new();

            // TURN 1
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 2)
            );

            // TURN 3
            natureSnakeTransformers.turns.AddTurn(
                Enemy("CXformerAdder"),
                Enemy("CXformerAdder", replacement: "P03KCMXP1_ViperBeast", difficulty: 2)
            );

            // TURN 4
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 1)
            );

            // TURN 5
            natureSnakeTransformers.turns.AddTurn(
                Enemy("CXformerAdder"),
                Enemy(null, replacement: "Shieldbot", difficulty: 2)
            );

            // TURN 6
            natureSnakeTransformers.turns.AddTurn(
                Enemy("CXformerAdder"),
                Enemy("CXformerAdder", replacement: "P03KCMXP1_ViperBeast", difficulty: 4)
            );

            // TURN 7
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot", replacement: "P03KCMXP1_ViperBeast", difficulty: 5)
            );

            // TURN 8
            natureSnakeTransformers.turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot"),
                Enemy("P03KCMXP1_ViperBot", replacement: "P03KCMXP1_ViperBeast", difficulty: 6)
            );

            // TURN 9
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 4)
            );

            // TURN 10
            natureSnakeTransformers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 2)
            );


            // Encounter: Neutral_Alarmbots
            EncounterBlueprintData neutralAlarmbots = EncounterManager.New("Neutral_Alarmbots", addToPool: true);
            neutralAlarmbots.SetDifficulty(0, 6).SetP03Encounter();
            neutralAlarmbots.turns = new();

            // TURN 1
            neutralAlarmbots.turns.AddTurn(
                Enemy("AlarmBot", replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 6),
                Enemy(null, replacement: "AlarmBot", difficulty: 4)
            );

            // TURN 3
            neutralAlarmbots.turns.AddTurn(
                Enemy("AlarmBot", replacement: "SwapBot", difficulty: 5),
                Enemy(null, replacement: "Bombbot", difficulty: 3)
            );

            // TURN 4
            neutralAlarmbots.turns.AddTurn(
                Enemy("Bombbot"),
                Enemy(null, replacement: "Bombbot", difficulty: 6),
                Enemy(null, replacement: "Bombbot", difficulty: 3),
                Enemy(null, replacement: "Bombbot", difficulty: 4)
            );

            // TURN 5
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 2),
                Enemy(null, replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "Bombbot", difficulty: 3),
                Enemy(null, replacement: "Bombbot", difficulty: 4)
            );

            // TURN 6
            neutralAlarmbots.turns.AddTurn(
                Enemy("MineCart"),
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 7
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "AlarmBot", difficulty: 3),
                Enemy(null, replacement: "AlarmBot", difficulty: 4)
            );

            // TURN 8
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 4),
                Enemy(null, replacement: "AlarmBot", difficulty: 6),
                Enemy(null, replacement: "CloserBot", difficulty: 4)
            );

            // TURN 9
            neutralAlarmbots.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6),
                Enemy(null, replacement: "CloserBot", difficulty: 3)
            );

            // TURN 10
            neutralAlarmbots.turns.AddTurn(
                Enemy("Automaton", replacement: "AlarmBot", difficulty: 4)
            );


            // Encounter: Neutral_BombsAndShields
            EncounterBlueprintData neutralBombsAndShields = EncounterManager.New("Neutral_BombsAndShields", addToPool: true);
            neutralBombsAndShields.SetDifficulty(0, 6).SetP03Encounter();
            neutralBombsAndShields.AddRandomReplacementCards("Shieldbot", "Bombbot", "SentryBot");
            neutralBombsAndShields.turns = new();

            // TURN 1
            neutralBombsAndShields.turns.AddTurn(
                Enemy("Shieldbot"),
                Enemy(null, replacement: "SentryBot", difficulty: 4),
                Enemy(null, replacement: "Bombbot", difficulty: 6),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralBombsAndShields.turns.AddTurn(
                Enemy("Automaton", replacement: "Shieldbot", difficulty: 6),
                Enemy("Bombbot"),
                Enemy(null, replacement: "Shieldbot", difficulty: 2)
            );

            // TURN 3
            neutralBombsAndShields.turns.AddTurn(
                Enemy("Bombbot", replacement: "CloserBot", difficulty: 6),
                Enemy(null, replacement: "Bombbot", difficulty: 1)
            );

            // TURN 4
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "Bombbot", difficulty: 3)
            );

            // TURN 5
            neutralBombsAndShields.turns.AddTurn(
                Enemy("Shieldbot", replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "Shieldbot", difficulty: 3)
            );

            // TURN 6
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 2),
                Enemy("Shieldbot")
            );

            // TURN 7
            neutralBombsAndShields.turns.AddTurn(

            );

            // TURN 8
            neutralBombsAndShields.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Neutral_BridgeBattle
            EncounterBlueprintData neutralBridgeBattle = EncounterManager.New("Neutral_BridgeBattle", addToPool: true);
            neutralBridgeBattle.SetDifficulty(0, 6).SetP03Encounter();
            neutralBridgeBattle.AddRandomReplacementCards("Automaton", "Bombbot", "MineCart", "AlarmBot");
            neutralBridgeBattle.turns = new();

            // TURN 1
            neutralBridgeBattle.turns.AddTurn(
                Enemy("AlarmBot", replacement: "Shieldbot", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 3),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralBridgeBattle.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 3
            neutralBridgeBattle.turns.AddTurn(
                Enemy("Automaton", replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 4
            neutralBridgeBattle.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 5
            neutralBridgeBattle.turns.AddTurn(
                Enemy("SwapBot", replacement: "CloserBot", difficulty: 2)
            );

            // TURN 6
            neutralBridgeBattle.turns.AddTurn(
                Enemy(null)
            );

            // TURN 7
            neutralBridgeBattle.turns.AddTurn(
                Enemy("SwapBot", replacement: "CloserBot", difficulty: 4)
            );

            // TURN 8
            neutralBridgeBattle.turns.AddTurn(
                Enemy("SwapBot", replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Neutral_Minecarts
            EncounterBlueprintData neutralMinecarts = EncounterManager.New("Neutral_Minecarts", addToPool: true);
            neutralMinecarts.SetDifficulty(0, 6).SetP03Encounter();
            neutralMinecarts.turns = new();

            // TURN 1
            neutralMinecarts.turns.AddTurn(
                Enemy("MineCart", replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralMinecarts.turns.AddTurn(
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2),
                Enemy("Bombbot", replacement: "MineCart", difficulty: 2)
            );

            // TURN 3
            neutralMinecarts.turns.AddTurn(
                Enemy("Insectodrone", replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "MineCart", difficulty: 3)
            );

            // TURN 4
            neutralMinecarts.turns.AddTurn(
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 6)
            );

            // TURN 5
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 1),
                Enemy(null, replacement: "MineCart", difficulty: 4)
            );

            // TURN 6
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "MineCart", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "Insectodrone", difficulty: 6)
            );

            // TURN 7
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 2),
                Enemy(null, replacement: "MineCart", difficulty: 3)
            );

            // TURN 8
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 4),
                Enemy(null, replacement: "MineCart", difficulty: 4)
            );

            // TURN 9
            neutralMinecarts.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Neutral_SentryWall
            EncounterBlueprintData neutralSentryWall = EncounterManager.New("Neutral_SentryWall", addToPool: true);
            neutralSentryWall.SetDifficulty(0, 6).SetP03Encounter();
            neutralSentryWall.AddRandomReplacementCards("Shieldbot", "Insectodrone", "Bombbot");
            neutralSentryWall.turns = new();

            // TURN 1
            neutralSentryWall.turns.AddTurn(
                Enemy("SentryBot"),
                Enemy("SentryBot", replacement: "RoboSkeleton", difficulty: 6),
                Enemy("SentryBot", replacement: "RoboSkeleton", difficulty: 4),
                Enemy("SentryBot"),
                Enemy(null, replacement: "SentryBot", difficulty: 4),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralSentryWall.turns.AddTurn(
                Enemy("RoboSkeleton", replacement: "Shieldbot", difficulty: 1),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 3
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 3),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 6),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 4
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 2),
                Enemy(null, replacement: "RoboSkeleton", difficulty: 5),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 5
            neutralSentryWall.turns.AddTurn(
                Enemy("SentryBot"),
                Enemy(null, replacement: "SentryBot", difficulty: 2),
                Enemy(null, replacement: "SentryBot", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 6),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 6
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 7
            neutralSentryWall.turns.AddTurn(
                Enemy(null),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 8
            neutralSentryWall.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 9
            neutralSentryWall.turns.AddTurn(
                Enemy("CloserBot")
            );


            // Encounter: Neutral_Swapbots
            EncounterBlueprintData neutralSwapbots = EncounterManager.New("Neutral_Swapbots", addToPool: true);
            neutralSwapbots.SetDifficulty(0, 6).SetP03Encounter();
            neutralSwapbots.AddRandomReplacementCards("Automaton", "Bombbot", "MineCart");
            neutralSwapbots.turns = new();

            // TURN 1
            neutralSwapbots.turns.AddTurn(
                Enemy("SentryBot"),
                Enemy(null, replacement: "SwapBot", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 1),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralSwapbots.turns.AddTurn(
                Enemy("SwapBot", replacement: "CloserBot", difficulty: 6),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 3
            neutralSwapbots.turns.AddTurn(
                Enemy("Automaton"),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2),
                Enemy("Automaton", replacement: "SwapBot", difficulty: 4)
            );

            // TURN 4
            neutralSwapbots.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "SentryBot", difficulty: 3)
            );

            // TURN 5
            neutralSwapbots.turns.AddTurn(
                Enemy("SwapBot"),
                Enemy(null, replacement: "SwapBot", difficulty: 6),
                Enemy(null, replacement: "SentryBot", difficulty: 4)
            );

            // TURN 6
            neutralSwapbots.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 2),
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
            EncounterBlueprintData neutralClockbots = EncounterManager.New("Neutral_Clockbots", addToPool: true);
            neutralClockbots.SetDifficulty(0, 6).SetP03Encounter();
            neutralClockbots.turns = new();

            // TURN 1
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot", replacement: "P03KCMXP1_Clockbot_Down", difficulty: 4),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            neutralClockbots.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Right", difficulty: 2),
                Enemy("P03KCMXP1_Clockbot", replacement: "P03KCMXP1_Clockbot_Left", difficulty: 1)
            );

            // TURN 3
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot"),
                Enemy(null, replacement: "P03KCMXP1_Clockbot", difficulty: 5),
                Enemy(null, replacement: "P03KCMXP1_Clockbot", difficulty: 3)
            );

            // TURN 4
            neutralClockbots.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Down", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Right", difficulty: 5)
            );

            // TURN 5
            neutralClockbots.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_Clockbot", difficulty: 3),
                Enemy("P03KCMXP1_Clockbot", replacement: "P03KCMXP1_Clockbot_Left", difficulty: 2)
            );

            // TURN 6
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot_Down")
            );

            // TURN 7
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot"),
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Right", difficulty: 3)
            );

            // TURN 8
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot"),
                Enemy(null, replacement: "P03KCMXP1_Clockbot_Down", difficulty: 6)
            );

            // TURN 9
            neutralClockbots.turns.AddTurn(
                Enemy("P03KCMXP1_Clockbot")
            );


            // Encounter: Neutral_SpyPlanes
            EncounterBlueprintData neutralSpyPlanes = EncounterManager.New("Neutral_SpyPlanes", addToPool: true);
            neutralSpyPlanes.SetDifficulty(0, 6).SetP03Encounter();
            neutralSpyPlanes.turns = new();

            // TURN 1
            neutralSpyPlanes.turns.AddTurn(
                Enemy("LeapBot"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6),
                Enemy(null, replacement: "Insectodrone", difficulty: 2)
            );

            // TURN 2
            neutralSpyPlanes.turns.AddTurn(
                Enemy("Insectodrone"),
                Enemy(null, replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 3
            neutralSpyPlanes.turns.AddTurn(
                Enemy("LeapBot"),
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
                Enemy(null, replacement: "Insectodrone", difficulty: 1),
                Enemy(null, replacement: "P03KCMXP1_Spyplane", difficulty: 3),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 7
            neutralSpyPlanes.turns.AddTurn(
                Enemy("P03KCMXP1_Spyplane"),
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
            P03FinalBoss = EncounterManager.New("P03FinalBoss", addToPool: false);
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
            PhotographerBossP1 = EncounterManager.New("PhotographerBossP1", addToPool: false);
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
            PhotographerBossP2 = EncounterManager.New("PhotographerBossP2", addToPool: false);
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
            EncounterBlueprintData techAttackConduits = EncounterManager.New("Tech_AttackConduits", addToPool: true);
            techAttackConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techAttackConduits.turns = new();

            // TURN 1
            techAttackConduits.turns.AddTurn(
                Enemy("AttackConduit"),
                Enemy("NullConduit", replacement: "AttackConduit", difficulty: 5),
                Enemy("Automaton", replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            techAttackConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Shieldbot", difficulty: 6),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2)
            );

            // TURN 3
            techAttackConduits.turns.AddTurn(
                Enemy("Insectodrone", replacement: "Shieldbot", difficulty: 4),
                Enemy(null, replacement: "AttackConduit", difficulty: 3)
            );

            // TURN 4
            techAttackConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 5
            techAttackConduits.turns.AddTurn(
                Enemy(null, replacement: "AttackConduit", difficulty: 2),
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
                Enemy(null, replacement: "AttackConduit", difficulty: 6),
                Enemy("NullConduit"),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 8
            techAttackConduits.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 9
            techAttackConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Automaton", difficulty: 4),
                Enemy("LeapBot", replacement: "MineCart", difficulty: 2)
            );

            // TURN 10
            techAttackConduits.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Tech_GiftCells
            EncounterBlueprintData techGiftCells = EncounterManager.New("Tech_GiftCells", addToPool: true);
            techGiftCells.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techGiftCells.turns = new();

            // TURN 1
            techGiftCells.turns.AddTurn(
                Enemy("AttackConduit"),
                Enemy("NullConduit", replacement: "AttackConduit", difficulty: 3),
                Enemy("CellGift"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            techGiftCells.turns.AddTurn(
                Enemy("CellGift", replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "NullConduit", difficulty: 3)
            );

            // TURN 3
            techGiftCells.turns.AddTurn(
                Enemy("NullConduit"),
                Enemy(null, replacement: "CellGift", difficulty: 2),
                Enemy(null, replacement: "FactoryConduit", difficulty: 3),
                Enemy(null, replacement: "AttackConduit", difficulty: 5)
            );

            // TURN 4
            techGiftCells.turns.AddTurn(
                Enemy("CellGift", replacement: "Automaton", difficulty: 1),
                Enemy(null, replacement: "Bombbot", difficulty: 6),
                Enemy(null, replacement: "NullConduit", difficulty: 4)
            );

            // TURN 5
            techGiftCells.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 5),
                Enemy("CellGift")
            );

            // TURN 6
            techGiftCells.turns.AddTurn(
                Enemy(null, replacement: "AttackConduit", difficulty: 4),
                Enemy("AttackConduit"),
                Enemy(null, replacement: "Insectodrone", difficulty: 2),
                Enemy(null, replacement: "NullConduit", difficulty: 3)
            );

            // TURN 7
            techGiftCells.turns.AddTurn(
                Enemy("CellGift", replacement: "CellBuff", difficulty: 4)
            );

            // TURN 8
            techGiftCells.turns.AddTurn(
                Enemy("GiftBot", replacement: "CellGift", difficulty: 2),
                Enemy("LeapBot", replacement: "MineCart", difficulty: 4),
                Enemy(null, replacement: "AttackConduit", difficulty: 3)
            );

            // TURN 9
            techGiftCells.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Tech_SplinterCells
            EncounterBlueprintData techSplinterCells = EncounterManager.New("Tech_SplinterCells", addToPool: true);
            techSplinterCells.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techSplinterCells.turns = new();

            // TURN 1
            techSplinterCells.turns.AddTurn(
                Enemy("CellTri"),
                Enemy(null, replacement: "Automaton", difficulty: 6),
                Enemy(null, replacement: "NullConduit", difficulty: 3),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            techSplinterCells.turns.AddTurn(
                Enemy("NullConduit", replacement: "AttackConduit", difficulty: 4),
                Enemy("NullConduit")
            );

            // TURN 3
            techSplinterCells.turns.AddTurn(
                Enemy("Automaton", replacement: "CellBuff", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 3),
                Enemy(null, replacement: "NullConduit", difficulty: 3)
            );

            // TURN 4
            techSplinterCells.turns.AddTurn(
                Enemy("NullConduit", replacement: "AttackConduit", difficulty: 1),
                Enemy(null, replacement: "CellBuff", difficulty: 4),
                Enemy(null, replacement: "AttackConduit", difficulty: 3)
            );

            // TURN 5
            techSplinterCells.turns.AddTurn(
                Enemy(null, replacement: "CellTri", difficulty: 5)
            );

            // TURN 6
            techSplinterCells.turns.AddTurn(
                Enemy("NullConduit")
            );

            // TURN 7
            techSplinterCells.turns.AddTurn(
                Enemy(null, replacement: "CellBuff", difficulty: 3)
            );

            // TURN 8
            techSplinterCells.turns.AddTurn(
                Enemy("Automaton", replacement: "CellBuff", difficulty: 3),
                Enemy("LeapBot", replacement: "Shieldbot", difficulty: 2)
            );

            // TURN 9
            techSplinterCells.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Tech_ProtectConduits
            EncounterBlueprintData techProtectConduits = EncounterManager.New("Tech_ProtectConduits", addToPool: true);
            techProtectConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techProtectConduits.turns = new();

            // TURN 1
            techProtectConduits.turns.AddTurn(
                Enemy("P03KCMXP1_ConduitProtector")
            );

            // TURN 2
            techProtectConduits.turns.AddTurn(
                Enemy("NullConduit", replacement: "HealerConduit", difficulty: 1),
                Enemy("HealerConduit")
            );

            // TURN 3
            techProtectConduits.turns.AddTurn(
                Enemy("Thickbot"),
                Enemy("Thickbot", replacement: "SwapBot", difficulty: 5)
            );

            // TURN 4
            techProtectConduits.turns.AddTurn(
                Enemy("HealerConduit", replacement: "AttackConduit", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_ConduitProtector", difficulty: 2)
            );

            // TURN 5
            techProtectConduits.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Tech_StinkyConduits
            EncounterBlueprintData techStinkyConduits = EncounterManager.New("Tech_StinkyConduits", addToPool: true);
            techStinkyConduits.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Tech);
            techStinkyConduits.turns = new();

            // TURN 1
            techStinkyConduits.turns.AddTurn(
                Enemy("P03KCMXP1_ConduitDebuffEnemy"),
                Enemy("NullConduit", replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 5),
                Enemy("Automaton", replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            techStinkyConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Shieldbot", difficulty: 6),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2)
            );

            // TURN 3
            techStinkyConduits.turns.AddTurn(
                Enemy("Insectodrone", replacement: "Shieldbot", difficulty: 4),
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 3)
            );

            // TURN 4
            techStinkyConduits.turns.AddTurn(
                Enemy("LeapBot", replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "Bombbot", difficulty: 4),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 5
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 2),
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 6),
                Enemy(null, replacement: "LeapBot", difficulty: 4)
            );

            // TURN 6
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 5),
                Enemy(null, replacement: "LeapBot", difficulty: 3)
            );

            // TURN 7
            techStinkyConduits.turns.AddTurn(
                Enemy(null, replacement: "P03KCMXP1_ConduitDebuffEnemy", difficulty: 6),
                Enemy("NullConduit"),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
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
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Undead_BombLatchers
            EncounterBlueprintData undeadBombLatchers = EncounterManager.New("Undead_BombLatchers", addToPool: true);
            undeadBombLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadBombLatchers.turns = new();

            // TURN 1
            undeadBombLatchers.turns.AddTurn(
                Enemy("LatcherBomb"),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 1),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "Automaton", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadBombLatchers.turns.AddTurn(
                Enemy("LatcherBomb"),
                Enemy("Bombbot", replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 4
            undeadBombLatchers.turns.AddTurn(
                Enemy("BoltHound", replacement: "CloserBot", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "LatcherBomb", difficulty: 4),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 6
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "LatcherBomb", difficulty: 2),
                Enemy(null, replacement: "Shieldbot", difficulty: 4)
            );

            // TURN 7
            undeadBombLatchers.turns.AddTurn(
                Enemy("LatcherBomb")
            );

            // TURN 8
            undeadBombLatchers.turns.AddTurn(
                Enemy(null, replacement: "Shieldbot", difficulty: 4)
            );

            // TURN 9
            undeadBombLatchers.turns.AddTurn(
                Enemy("LeapBot", replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Undead_ShieldLatchers
            EncounterBlueprintData undeadShieldLatchers = EncounterManager.New("Undead_ShieldLatchers", addToPool: true);
            undeadShieldLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadShieldLatchers.turns = new();

            // TURN 1
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield"),
                Enemy("LeapBot", replacement: "Automaton", difficulty: 1),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            undeadShieldLatchers.turns.AddTurn(
                Enemy("MineCart"),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield", replacement: "Shieldbot", difficulty: 5),
                Enemy("Bombbot", replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 4
            undeadShieldLatchers.turns.AddTurn(
                Enemy("CloserBot"),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadShieldLatchers.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 6),
                Enemy(null, replacement: "LatcherShield", difficulty: 4),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 6
            undeadShieldLatchers.turns.AddTurn(
                Enemy(null, replacement: "LatcherShield", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 4)
            );

            // TURN 7
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LatcherShield", replacement: "CloserBot", difficulty: 4)
            );

            // TURN 8
            undeadShieldLatchers.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 2)
            );

            // TURN 9
            undeadShieldLatchers.turns.AddTurn(
                Enemy("LeapBot", replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Undead_SkeleSwarm
            EncounterBlueprintData undeadSkeleSwarm = EncounterManager.New("Undead_SkeleSwarm", addToPool: true);
            undeadSkeleSwarm.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadSkeleSwarm.turns = new();

            // TURN 1
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton"),
                Enemy(null, replacement: "SentryBot", difficulty: 1),
                Enemy(null, replacement: "SentryBot", difficulty: 3),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton", replacement: "Insectodrone", difficulty: 3)
            );

            // TURN 4
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadSkeleSwarm.turns.AddTurn(
                Enemy("RoboSkeleton"),
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
                Enemy("RoboSkeleton", replacement: "Insectodrone", difficulty: 6)
            );

            // TURN 8
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "RoboSkeleton", difficulty: 4)
            );

            // TURN 9
            undeadSkeleSwarm.turns.AddTurn(
                Enemy(null, replacement: "CloserBot", difficulty: 6)
            );


            // Encounter: Undead_WingLatchers
            EncounterBlueprintData undeadWingLatchers = EncounterManager.New("Undead_WingLatchers", addToPool: true);
            undeadWingLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadWingLatchers.turns = new();

            // TURN 1
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_FlyingLatcher"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6),
                Enemy(null, replacement: "Bombbot", difficulty: 2)
            );

            // TURN 2
            undeadWingLatchers.turns.AddTurn(
                Enemy("Automaton"),
                Enemy(null, replacement: "P03KCMXP1_FlyingLatcher", difficulty: 1),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_FlyingLatcher"),
                Enemy("Automaton", replacement: "Thickbot", difficulty: 2),
                Enemy("Bombbot")
            );

            // TURN 4
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_Executor"),
                Enemy(null, replacement: "P03KCMXP1_FlyingLatcher", difficulty: 5),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 5
            undeadWingLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_FlyingLatcher"),
                Enemy("Thickbot")
            );

            // TURN 6
            undeadWingLatchers.turns.AddTurn(
                Enemy("Thickbot", replacement: "CloserBot", difficulty: 3),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 4)
            );

            // TURN 7
            undeadWingLatchers.turns.AddTurn(
                Enemy("Thickbot", replacement: "CloserBot", difficulty: 6),
                Enemy("MineCart")
            );


            // Encounter: Undead_StrafeLatchers
            EncounterBlueprintData undeadStrafeLatchers = EncounterManager.New("Undead_StrafeLatchers", addToPool: true);
            undeadStrafeLatchers.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Undead);
            undeadStrafeLatchers.turns = new();

            // TURN 1
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6),
                Enemy(null, replacement: "Bombbot", difficulty: 2)
            );

            // TURN 2
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy(null, replacement: "P03KCMXP1_ConveyorLatcher", difficulty: 1),
                Enemy(null, replacement: "LatcherBrittle", difficulty: 3)
            );

            // TURN 3
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy("Automaton", replacement: "Shieldbot", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 1),
                Enemy("Bombbot")
            );

            // TURN 4
            undeadStrafeLatchers.turns.AddTurn(
                Enemy("P03KCMXP1_ConveyorLatcher"),
                Enemy("MineCart"),
                Enemy(null, replacement: "MineCart", difficulty: 2),
                Enemy(null, replacement: "Insectodrone", difficulty: 5),
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
            EncounterBlueprintData wizardBigRipper = EncounterManager.New("Wizard_BigRipper", addToPool: true);
            wizardBigRipper.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardBigRipper.turns = new();

            // TURN 1
            wizardBigRipper.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem", replacement: "EmptyVessel_OrangeGem", difficulty: 2),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 4),
                Enemy("GemRipper"),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 5),
                Enemy(null, replacement: "SentinelBlue", difficulty: 3)
            );

            // TURN 3
            wizardBigRipper.turns.AddTurn(
                Enemy("Bombbot", replacement: "Automaton", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 3),
                Enemy(null, replacement: "SentinelGreen", difficulty: 1)
            );

            // TURN 4
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 2),
                Enemy(null, replacement: "GemRipper", difficulty: 4)
            );

            // TURN 5
            wizardBigRipper.turns.AddTurn(
                Enemy("SwapBot"),
                Enemy(null, replacement: "EmptyVessel_GreenGem", difficulty: 1),
                Enemy(null, replacement: "SentinelBlue", difficulty: 4)
            );

            // TURN 6
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "SentinelGreen", difficulty: 3)
            );

            // TURN 7
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 5)
            );

            // TURN 8
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "AlarmBot", difficulty: 3),
                Enemy(null, replacement: "SentinelGreen", difficulty: 3)
            );

            // TURN 9
            wizardBigRipper.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 6)
            );


            // Encounter: Wizard_GemExploder
            EncounterBlueprintData wizardGemExploder = EncounterManager.New("Wizard_GemExploder", addToPool: true);
            wizardGemExploder.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardGemExploder.turns = new();

            // TURN 1
            wizardGemExploder.turns.AddTurn(
                Enemy("Automaton", replacement: "Shieldbot", difficulty: 6),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 1),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 3),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            wizardGemExploder.turns.AddTurn(
                Enemy("EmptyVessel_OrangeGem"),
                Enemy("GemExploder")
            );

            // TURN 3
            wizardGemExploder.turns.AddTurn(
                Enemy("EmptyVessel_OrangeGem"),
                Enemy("Bombbot", replacement: "GemExploder", difficulty: 2)
            );

            // TURN 4
            wizardGemExploder.turns.AddTurn(
                Enemy("Shieldbot", replacement: "GemRipper", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 3),
                Enemy(null, replacement: "GemExploder", difficulty: 3)
            );

            // TURN 5
            wizardGemExploder.turns.AddTurn(
                Enemy("EmptyVessel_OrangeGem", replacement: "Bombbot", difficulty: 2)
            );

            // TURN 6
            wizardGemExploder.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 3),
                Enemy(null, replacement: "GemExploder", difficulty: 3)
            );

            // TURN 7
            wizardGemExploder.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 2)
            );

            // TURN 8
            wizardGemExploder.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 4),
                Enemy(null, replacement: "GemExploder", difficulty: 4)
            );

            // TURN 9
            wizardGemExploder.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 6),
                Enemy(null, replacement: "GemExploder", difficulty: 6),
                Enemy(null, replacement: "EmptyVessel_OrangeGem", difficulty: 6)
            );


            // Encounter: Wizard_ShieldGems
            EncounterBlueprintData wizardShieldGems = EncounterManager.New("Wizard_ShieldGems", addToPool: true);
            wizardShieldGems.SetDifficulty(0, 6).SetP03Encounter(CardTemple.Wizard);
            wizardShieldGems.turns = new();

            // TURN 1
            wizardShieldGems.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem"),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3),
                Enemy(null, replacement: "P03KCM_FIREWALL_BATTLE", difficulty: 6)
            );

            // TURN 2
            wizardShieldGems.turns.AddTurn(
                Enemy("EmptyVessel_BlueGem"),
                Enemy("Bombbot"),
                Enemy(null, replacement: "GemShielder", difficulty: 3),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3)
            );

            // TURN 3
            wizardShieldGems.turns.AddTurn(
                Enemy("GemShielder"),
                Enemy(null, replacement: "GemShielder", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 4)
            );

            // TURN 4
            wizardShieldGems.turns.AddTurn(
                Enemy("Shieldbot", replacement: "GemRipper", difficulty: 4)
            );

            // TURN 5
            wizardShieldGems.turns.AddTurn(
                Enemy("EmptyVessel_OrangeGem", replacement: "Bombbot", difficulty: 2),
                Enemy(null, replacement: "GemShielder", difficulty: 3)
            );

            // TURN 6
            wizardShieldGems.turns.AddTurn(
                Enemy(null, replacement: "Automaton", difficulty: 2),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3)
            );

            // TURN 7
            wizardShieldGems.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 4)
            );

            // TURN 8
            wizardShieldGems.turns.AddTurn(
                Enemy(null, replacement: "Insectodrone", difficulty: 6),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 4)
            );

            // TURN 9
            wizardShieldGems.turns.AddTurn(
                Enemy(null, replacement: "GemRipper", difficulty: 6),
                Enemy(null, replacement: "GemShielder", difficulty: 4),
                Enemy(null, replacement: "EmptyVessel_BlueGem", difficulty: 3)
            );

            // Damage Race
            GeneratorDamageRace = EncounterManager.New("GeneratorDamageRace", addToPool: false);
            GeneratorDamageRace.SetDifficulty(0, 6);
            GeneratorDamageRace.turns = new();
            GeneratorDamageRace.turns.AddTurn(
                Enemy("SentryBot")
            );
        }
    }
}