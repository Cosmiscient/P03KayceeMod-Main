using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using Infiniscryption.P03KayceeRun.Encounters;
using static Infiniscryption.P03KayceeRun.Encounters.EncounterHelper;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    internal static class MultiverseEncounters
    {
        // These blueprints are special; they're for bosses and aren't in the normal pool
        internal static List<EncounterBlueprintData> MultiverseBossPhaseOne;
        internal static List<EncounterBlueprintData> MultiverseBossPhaseTwo;

        // Design spec:
        // P03 1 plays a deck of animals. This includes multiversal moles and dogs
        // P03 2 plays a deck of defensive and reactive cards. This includes a multiversal sentry and multiversal bomb bots.
        // P03 3 plays a deck of powerful cards. This includes multiversal strafing and a multiversal double gunner.
        // P03 1 plays a deck of multiversal gems
        // P03 2 plays a deck of defensive, multiversal conduits

        private static void ContinueOn(this EncounterBlueprintData data, params string[] turns)
        {
            for (int i = 0; i < 10; i++)
            {
                foreach (var turnCard in turns)
                {
                    if (string.IsNullOrEmpty(turnCard))
                        data.AddTurn();
                    else
                        data.AddTurn(Enemy(turnCard));
                }
            }
        }

        internal static void CreateMultiverseEncounters()
        {
            MultiverseBossPhaseOne = new();

            // Encounter: Multiverse 1
            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_1_Sentries", addToPool: false));
            MultiverseBossPhaseOne[0].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[0].turns = new();

            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("P03KCM_MultiverseSentry"),
                Enemy("P03KCM_MultiverseSentry")
            );

            // TURN 2
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("RoboSkeleton")
            );

            // TURN 3
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("RoboSkeleton"),
                Enemy("RoboSkeleton")
            );

            // TURN 4
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("P03KCM_MultiverseSentry")
            );

            // TURN 5
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("P03KCM_MultiverseSentry"),
                Enemy("RoboSkeleton")
            );

            // TURN 6
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("Insectodrone"),
                Enemy("RoboSkeleton")
            );

            // TURN 7
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("RoboSkeleton")
            );

            // TURN 8
            MultiverseBossPhaseOne[0].turns.AddTurn(
                Enemy("P03KCM_MultiverseGunner")
            );

            MultiverseBossPhaseOne[0].ContinueOn("P03KCM_MultiverseSentry", "RoboSkeleton", "RoboSkeleton");

            // Encounter: Multiverse 1-1
            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_2_Firewall", addToPool: false));
            MultiverseBossPhaseOne[1].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[1].turns = new();

            // TURN 1
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("P03KCM_MultiverseFirewall")
            );

            // TURN 2
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("Automaton"),
                Enemy("LeapBot")
            );

            // TURN 3
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("Insectodrone")
            );

            // TURN 4
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("P03KCM_MultiverseFirewall")
            );

            // TURN 5
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("P03KCMXP1_Spyplane")
            );

            // TURN 6
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("LeapBot"),
                Enemy("Automaton")
            );

            // TURN 7
            MultiverseBossPhaseOne[1].turns.AddTurn(
                Enemy("Automaton")
            );

            MultiverseBossPhaseOne[1].ContinueOn("P03KCM_MultiverseFirewall", "Automaton", null, "P03KCMXP1_Spyplane");

            // Encounter: Multiverse 3
            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_3_MineCarts", addToPool: false));
            MultiverseBossPhaseOne[2].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[2].turns = new();

            // TURN 1
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseMineCart")
            );

            // TURN 2
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("LeapBot"),
                Enemy("P03KCM_MultiverseMineCart")
            );

            // TURN 3
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombbot")
            );

            // TURN 4
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("LeapBot"),
                Enemy("P03KCM_MultiverseMineCart")
            );

            // TURN 5
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseMineCart")
            );

            // TURN 5
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombbot"),
                Enemy("P03KCM_MultiverseMineCart")
            );

            // TURN 6
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseMineCart"),
                Enemy("Insectodrone")
            );

            // TURN 7
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseGunner"),
                Enemy("Bombbot")
            );

            // TURN 8
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("Automaton"),
                Enemy("P03KCM_MultiverseMineCart")
            );

            // TURN 9
            MultiverseBossPhaseOne[2].turns.AddTurn(
                Enemy("P03KCM_MultiverseGunner")
            );

            MultiverseBossPhaseOne[2].ContinueOn(null, "LeapBot", "P03KCM_MultiverseGunner");

            // Encounter: Multiverse 3
            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_4_Hounds", addToPool: false));
            MultiverseBossPhaseOne[3].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[3].turns = new();

            // TURN 1
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("LeapBot")
            );

            // TURN 2
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCM_MultiverseBolthound")
            );

            // TURN 3
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCMXP1_WolfBot")
            );

            // TURN 4
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCM_CXformerAlpha"),
                Enemy("LeapBot")
            );

            // TURN 5
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCMXP1_WolfBot")
            );

            // TURN 6
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCM_MultiverseBolthound")
            );

            // TURN 7
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCMXP1_ViperBot")
            );

            // TURN 8
            MultiverseBossPhaseOne[3].turns.AddTurn(
                Enemy("P03KCM_CXformerAlpha")
            );

            MultiverseBossPhaseOne[3].ContinueOn("P03KCM_MultiverseBolthound", "P03KCM_CXformerAlpha", "P03KCMXP1_ViperBot");

            // Encounter: Multiverse 3
            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_5_Conduits", addToPool: false));
            MultiverseBossPhaseOne[4].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[4].turns = new();
            MultiverseBossPhaseOne[4].AddTerrainRepeatRule(5);

            MultiverseBossPhaseOne[4].AddEnemyTerrain(new() {
                Enemy("AttackConduit"),
                null,
                null,
                null,
                Enemy("P03KCM_MultiverseConduitNull"),
            });

            // TURN 1
            MultiverseBossPhaseOne[4].turns.AddTurn(
                Enemy("LeapBot")
            );

            // TURN 2
            MultiverseBossPhaseOne[4].turns.AddTurn(
                Enemy("LeapBot")
            );

            // TURN 3
            MultiverseBossPhaseOne[4].turns.AddTurn(
                Enemy("Insectodrone")
            );

            // TURN 4
            MultiverseBossPhaseOne[4].turns.AddTurn(
                Enemy("LeapBot"),
                Enemy("Automaton")
            );

            // TURN 5
            MultiverseBossPhaseOne[4].turns.AddTurn(
                Enemy("Automaton")
            );

            MultiverseBossPhaseOne[4].ContinueOn("LeapBot", "LeapBot", "Automaton", "Insectodrone", "LeapBot");

            // Encounter: Multiverse 3
            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_6_Latchers", addToPool: false));
            MultiverseBossPhaseOne[5].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[5].turns = new();

            // TURN 1
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombLatcher"),
                Enemy("LeapBot")
            );

            // TURN 2
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombbot"),
                Enemy("P03KCM_MultiverseBrittleLatcher")
            );

            // TURN 3
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombLatcher"),
                Enemy("P03KCM_MultiverseBombbot")
            );

            // TURN 4
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBolthound")
            );

            // TURN 5
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBrittleLatcher")
            );

            // TURN 6
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombLatcher"),
                Enemy("Shieldbot")
            );

            // TURN 7
            MultiverseBossPhaseOne[5].turns.AddTurn(
                Enemy("P03KCM_MultiverseBombLatcher")
            );

            MultiverseBossPhaseOne[5].ContinueOn("Shieldbot", "P03KCM_MultiverseBolthound", "P03KCM_MultiverseBombLatcher", "P03KCM_MultiverseBrittleLatcher");

            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_6_Chaos_1", addToPool: false));
            MultiverseBossPhaseOne[6].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[6].turns = new();

            MultiverseBossPhaseOne[6].turns.AddTurn(
                Enemy("P03KCM_MultiverseFirewall")
            );

            MultiverseBossPhaseOne[6].turns.AddTurn(
                Enemy("P03KCM_MultiverseSentry"),
                Enemy("P03KCM_MultiverseMineCart")
            );

            MultiverseBossPhaseOne[6].turns.AddTurn(
                Enemy("LeapBot"),
                Enemy("P03KCM_MultiverseMineCart")
            );

            MultiverseBossPhaseOne[6].turns.AddTurn(
                Enemy("Automaton"),
                Enemy("LeapBot")
            );

            MultiverseBossPhaseOne[6].turns.AddTurn(
                Enemy("P03KCM_MultiverseBolthound")
            );

            MultiverseBossPhaseOne[6].ContinueOn("P03KCM_MultiverseMineCart", "P03KCM_MultiverseGunner", "P03KCM_MultiverseFirewall", "P03KCM_MultiverseMineCart");

            MultiverseBossPhaseOne.Add(EncounterManager.New("P03KCM_Multiverse_6_Chaos_2", addToPool: false));
            MultiverseBossPhaseOne[7].SetDifficulty(0, 10);
            MultiverseBossPhaseOne[7].AddRandomReplacementCards(
                "P03KCM_MultiverseMole",
                "P03KCM_MultiverseMineCart",
                "P03KCM_MultiverseSentry",
                "P03KCM_MultiverseBombbot",
                "P03KCM_MultiverseBrittleLatcher",
                "P03KCM_MultiverseBombLatcher"
            );
            MultiverseBossPhaseOne[7].turns = new();

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 }, new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.Add(new() { new() { randomReplaceChance = 100 } });

            MultiverseBossPhaseOne[7].turns.AddTurn(Enemy("P03KCM_MultiverseGunner"));


        }
    }
}
