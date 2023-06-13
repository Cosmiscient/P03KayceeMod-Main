using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using InscryptionAPI.Helpers;
using System.Linq;
using UnityEngine;
using InscryptionAPI.Ascension;
using InscryptionAPI.Guid;
using System.Collections;
using System;
using InscryptionAPI.Saves;
using InscryptionAPI.Card;
using Infiniscryption.P03KayceeRun.Cards;
using DiskCardGame.CompositeRules;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class AscensionChallengeManagement
    {
        public static AscensionChallenge BOUNTY_HUNTER { get; private set; }
        public static AscensionChallenge ENERGY_HAMMER { get; private set; }
        public static AscensionChallenge TRADITIONAL_LIVES { get; private set; }

        public static AscensionChallenge LEEPBOT_SIDEDECK { get; private set; }
        public static AscensionChallenge TURBO_VESSELS { get; private set; }
        public static AscensionChallenge PAINTING_CHALLENGE { get; private set; }

        public static bool turboVesselsUIPlayed = false;
        public static bool LeapingSidedeckUIPlayed = false;
        public static bool tradLivesUIPlayed = false;
        public static bool expensiveRespawnUIPlayed = false;

        private static CanvasBossOpponent CanvasBoss => Singleton<TurnManager>.Instance.Opponent as CanvasBossOpponent;
        private static CompositeRuleTriggerHandler rulesHandler;
        private static CompositeBattleRule currentRule;
        private static CompositeRuleDisplayer ruleDisplayer;

        public static Part3BossOpponent dummyCanvasBoss;

        private static string CompatibleChallengeList
        {
            get 
            {
                return ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, "P03CompatibleChallenges");
            }
        }

        public const int HAMMER_ENERGY_COST = 2;

        public static Dictionary<AscensionChallenge, AscensionChallengeInfo> PatchedChallengesReference;
        public static List<AscensionChallenge> ValidChallenges;

        public static AscensionChallengeInfo BOMB_CHALLENGE;

        private static void AddBombChallenge()
        {
            BOMB_CHALLENGE = ChallengeManager.AddSpecific(P03Plugin.PluginGuid,
            "Explosive Bots",
            "All non-vessel bots self destruct when they die",
            0,
            TextureHelper.GetImageAsTexture("ascensionicon_bomb.png", typeof(AscensionChallengeManagement).Assembly),
            TextureHelper.GetImageAsTexture("ascensionicon_bombactivated.png", typeof(AscensionChallengeManagement).Assembly),
            0).SetFlags("p03");
        }

        public static AscensionChallengeInfo ALL_CONVEYOR;

        private static void AddConveyorChallenge()
        {
            ALL_CONVEYOR = ChallengeManager.AddSpecific(P03Plugin.PluginGuid,
            "Overactive Factory",
            "All regular battles are conveyor battles",
            0,
            TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle.png", typeof(AscensionChallengeManagement).Assembly),
            TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle_active.png", typeof(AscensionChallengeManagement).Assembly),
            0).SetFlags("p03");
        }

        public static void UpdateP03Challenges()
        {
            //Add challenges
            AddBombChallenge();
            AddConveyorChallenge();

            BOUNTY_HUNTER = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "HigherBounties");
            //BOMB_CHALLENGE = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "ExplodingBots");
            ENERGY_HAMMER = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "EnergyHammer");
            //ALL_CONVEYOR = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "AllConveyor");
            PAINTING_CHALLENGE = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "PaintingChallenge");
            TRADITIONAL_LIVES = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "TraditionalLives");
            TURBO_VESSELS = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "TurboVessels");
            LEEPBOT_SIDEDECK = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "LeepbotSidedeck");

            PatchedChallengesReference = new();

            //PatchedChallengesReference.Add(
            //    AscensionChallenge.NoClover,
            //    new() {
            //        challengeType = ALL_CONVEYOR,
            //        title = "Overactive Factory",
            //        description = "All regular battles are conveyor battles",
            //        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
            //        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle_active.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
            //        pointValue = 0
            //    }
            //);

            PatchedChallengesReference.Add(
                AscensionChallenge.NoClover,
                new()
                {
                    challengeType = TURBO_VESSELS,
                    title = "Turbo Vessels",
                    description = "Your vessels have the Double Sprinter sigil.",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_turbovessel.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_turbovessel_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 5
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.SubmergeSquirrels,
                new()
                {
                    challengeType = BOUNTY_HUNTER,
                    title = "Wanted Fugitive",
                    description = "Your bounty level is permanently increased by 1",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.GrizzlyMode,
                new()
                {
                    challengeType = BOUNTY_HUNTER,
                    title = "Wanted Fugitive",
                    description = "Your bounty level is permanently increased by 1",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.BossTotems,
                new()
                {
                    challengeType = PAINTING_CHALLENGE,
                    title = "Eccentric Painter",
                    description = "All bosses start with a random canvas rule.",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_eccentricpainter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_eccentricpainter_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 35
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.AllTotems,
                new()
                {
                    challengeType = ENERGY_HAMMER,
                    title = "Energy Hammer",
                    description = "The hammer now costs 2 energy to use",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.NoHook,
                ChallengeManager.BaseGameChallenges.First(fc => fc.Challenge.challengeType == AscensionChallenge.LessConsumables)
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.ExpensivePelts,
                new()
                {
                    challengeType = AscensionChallenge.ExpensivePelts,
                    title = "Pricey Upgrades",
                    description = "All upgrades cost more",
                    iconSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_expensivepelts"), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 5
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.LessLives,
                new()
                {
                    challengeType = AscensionChallenge.LessLives,
                    title = "Costly Respawn",
                    description = "Respawn Cost is 15. With Traditional Lives, you've one chance.",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_oneup.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 20
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.WeakStarterDeck,
                new()
                {
                    challengeType = TRADITIONAL_LIVES,
                    title = "Traditional Lives",
                    description = "You have two lives per region.",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_tradLives.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_tradLives_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 10
                }
            );

            PatchedChallengesReference.Add(
                AscensionChallenge.NoBossRares,
                new()
                {
                    challengeType = LEEPBOT_SIDEDECK,
                    title = "Leaping Side Deck",
                    description = "Replace your Empty Vessels with L33pbots.",
                    iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_leepbot.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_leepbot_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                    pointValue = 15
                }
            );

            ValidChallenges = new()
            {
                AscensionChallenge.BaseDifficulty,
                AscensionChallenge.ExpensivePelts,
                AscensionChallenge.LessConsumables,
                AscensionChallenge.LessLives,
                AscensionChallenge.NoBossRares,
                LEEPBOT_SIDEDECK,

                AscensionChallenge.NoHook,
                AscensionChallenge.StartingDamage,
                AscensionChallenge.WeakStarterDeck,
                TRADITIONAL_LIVES,

                AscensionChallenge.SubmergeSquirrels, // This gets replaced by BOUNTY_HUNTER - we mark it as valid so that we can calculate its unlock level properly
                BOUNTY_HUNTER,
                //Put the challenge that will replace the bomb challenge here

                AscensionChallenge.BossTotems, // This gets replaced by PAINTING_CHALLENGE - we mark it as valid so that we can calculate its unlock level properly
                PAINTING_CHALLENGE,
                AscensionChallenge.AllTotems, // This gets replaced by ENERGY_HAMMER - we mark it as valid so that we can calculate its unlock level properly
                ENERGY_HAMMER,
                AscensionChallenge.NoClover,
                TURBO_VESSELS
            };

            ChallengeManager.ModifyChallenges += delegate (List<ChallengeManager.FullChallenge> challenges)
            {
                if (P03AscensionSaveData.IsP03Run)
                    for (int i = 0; i < challenges.Count; i++)
                        if (PatchedChallengesReference.ContainsKey(challenges[i].Challenge.challengeType))
                            //challenges[i] = PatchedChallengesReference[challenges[i].challengeType];
                            challenges[i] = new()
                            {
                                Challenge = PatchedChallengesReference[challenges[i].Challenge.challengeType],
                                AppearancesInChallengeScreen = 1,
                                UnlockLevel = challenges[i].UnlockLevel
                            };

                return challenges;
            };
        }

        //Runs after the part 3 boss intro sequence
        public static IEnumerator RandomCanvasRule(IEnumerator sequence)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(PAINTING_CHALLENGE))
            {
                dummyCanvasBoss = (Part3BossOpponent)Singleton<TurnManager>.Instance.Opponent;

                //If the canvas boss exists, delete it. Then, either way, create a new one.
                dummyCanvasBoss.DestroyScenery();
                dummyCanvasBoss.SpawnScenery("LightQuadTableEffect");
                GameObject CanvasBackground = GameObject.Find("LightQuadTableEffect(Clone)");
                Renderer renderer = CanvasBackground.GetComponentInChildren<Renderer>();
                renderer.enabled = false;

                //Singleton<RulePaintingManager>.Instance.SetPaintingsShown(shown: false);
                //Part3BossOpponent boss = (Part3BossOpponent)Singleton<Part3BossOpponent>.Instance;
                //if (Singleton<TurnManager>.Instance.Opponent is CanvasBossOpponent canvasBoss)
                {
                    //ruleDisplayer = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.CreateRule).GetComponentInChildren<CompositeRuleDisplayer>();
                    //ruleDisplayer.ResetPainting();

                    ChallengeActivationUI.Instance.ShowActivation(PAINTING_CHALLENGE);

                    rulesHandler = dummyCanvasBoss.gameObject.AddComponent<CompositeRuleTriggerHandler>();
                    //Debug.Log("rulesHandler is fine");
                    currentRule = new CompositeBattleRule();
                    int randomEffectID = UnityEngine.Random.Range(0, CompositeBattleRule.AVAILABLE_EFFECTS.Count);
                    int randomTriggerID = UnityEngine.Random.Range(0, CompositeBattleRule.AVAILABLE_TRIGGERS.Count);
                    //Debug.Log("Effects: "+ CompositeBattleRule.AVAILABLE_EFFECTS.Count);
                    //Debug.Log("Triggers: " + CompositeBattleRule.AVAILABLE_TRIGGERS.Count);

                    //Get rid of damage effects for archivist
                    if (Singleton<TurnManager>.Instance.Opponent is ArchivistBossOpponent archivistBossOpponent)
                    {
                        // The player is facing the Archivist boss
                        Debug.Log("Facing Archivist boss");
                        Debug.Log("EffectID: " + randomEffectID);
                        //1 is 5 damage to random card, 4 is all cards damaged by 1
                        if ((randomEffectID == 1) || (randomEffectID == 4))
                        {
                            //Player take damage, 1 damage on scales
                            randomEffectID = 0;
                        }
                    }
                    else if (Singleton<TurnManager>.Instance.Opponent is Part3BossOpponent anotherBossOpponent)
                    {
                        // The player is facing another boss
                        Debug.Log("Facing another boss");
                    }
                    else
                    {
                        // The player is not facing any boss
                        Debug.Log("Not facing a boss");
                    }

                    currentRule.effect = CompositeBattleRule.AVAILABLE_EFFECTS.ElementAt(randomEffectID);
                    currentRule.trigger = CompositeBattleRule.AVAILABLE_TRIGGERS.ElementAt(randomTriggerID);

                    //ruleDisplayer.DisplayRule(currentRule);
                    //ruleDisplayer.MovingPaintingOffscreen();
                    //Singleton<RulePaintingManager>.Instance.SetPaintingsShown(shown: true);
                    yield return Singleton<RulePaintingManager>.Instance.SpawnPainting(currentRule);
                    rulesHandler.AddRule(currentRule);
                    Singleton<RulePaintingManager>.Instance.SetPaintingsShown(shown: true);
                }
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), nameof(AscensionChallengeScreen.OnEnable))]
        [HarmonyPostfix]
        private static void HideLockedBossIcon(AscensionChallengeScreen __instance)
        {
            __instance.gameObject.GetComponentInChildren<ChallengeIconGrid>().Start();
        }

        // [HarmonyPatch(typeof(AscensionChallengePaginator), nameof(AscensionChallengePaginator.ShowVisibleChallenges))]
        // [HarmonyPostfix]
        // private static void MakeIconGridRecalc(AscensionChallengePaginator __instance)
        // {
        //     if (!AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel))
        //         __instance.gameObject.GetComponentInChildren<ChallengeIconGrid>().finalBossIcon.SetActive(false);
        // }

        [HarmonyPatch(typeof(ChallengeIconGrid), nameof(ChallengeIconGrid.Start))]
        [HarmonyPrefix]
        private static void DynamicSwapSize(ChallengeIconGrid __instance)
        {
            if (!AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel))
			{
				__instance.finalBossIcon.SetActive(false);
				float xStart = -1.65f;
				for (int i = 0; i < __instance.topRowIcons.Count; i++)
					__instance.topRowIcons[i].localPosition = new Vector2(xStart + (float)i * 0.55f, __instance.topRowIcons[i].localPosition.y);

				for (int j = 0; j < __instance.bottomRowIcons.Count; j++)
					__instance.bottomRowIcons[j].localPosition = new Vector2(xStart + (float)j * 0.55f, __instance.bottomRowIcons[j].localPosition.y);
			}
        }


        [HarmonyPatch(typeof(AscensionUnlockSchedule), nameof(AscensionUnlockSchedule.ChallengeIsUnlockedForLevel))]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        [HarmonyPostfix]
        public static void ValidP03Challenges(ref bool __result, AscensionChallenge challenge, int level)
        {
            if (ScreenManagement.ScreenState == CardTemple.Tech)
            {
                if (PatchedChallengesReference.Any(kvp => kvp.Value.challengeType == challenge))
                {
                    var kvp = PatchedChallengesReference.First(kvp => kvp.Value.challengeType == challenge);
                    if (kvp.Value.challengeType != kvp.Key)
                    {
                        __result = AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(kvp.Key, level);
                        return;
                    }
                }

                if (ValidChallenges.Contains(challenge))
                    return;

                var fullChallenge = ChallengeManager.AllChallenges.FirstOrDefault(fc => fc.Challenge.challengeType == challenge);
                if (fullChallenge != null)
                {
                    if (fullChallenge.Flags == null)
                    {
                        __result = false;
                        return;
                    }

                    if (!fullChallenge.Flags.Any(f => f != null && f.ToString().ToLowerInvariant().Equals("p03")))
                    {
                        __result = false;
                        return;
                    }
                }                
            }
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.BuildSideDeck))]
        [HarmonyPostfix]
        private static void PostfixBuildSideDeck(Part3DeckReviewSequencer __instance)
        {
            // Access the variables from the original method using the __instance parameter
            List<CardInfo> sideDeck = __instance.sideDeck;

            // Clear the side deck
            sideDeck.Clear();

            // Modify the side deck building code as needed
            for (int i = 0; i < Part3SaveData.Data.deckGemsDistribution[0]; i++)
            {
                sideDeck.Add(__instance.GetGemCard("EmptyVessel_GreenGem"));
            }
            for (int j = 0; j < Part3SaveData.Data.deckGemsDistribution[1]; j++)
            {
                sideDeck.Add(__instance.GetGemCard("EmptyVessel_OrangeGem"));
            }
            for (int k = 0; k < Part3SaveData.Data.deckGemsDistribution[2]; k++)
            {
                sideDeck.Add(__instance.GetGemCard("EmptyVessel_BlueGem"));
            }

            for (int i = 0; i < 10; i++)
            {
                sideDeck[i].RemoveAbilities(DoubleSprint.AbilityID);
            }

            if (SaveFile.IsAscension)
            {


                //If the turbo vessels challenge is enabled, turn the sidedeck into turbo vessels.
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        //Debug.Log("VESSELS MODIFIED");
                        sideDeck[i].SetPortrait(TextureHelper.GetImageAsTexture("portrait_turbovessel.png", typeof(CustomCards).Assembly));
                        sideDeck[i].SetDisplayedName("Turbo Vessel");
                        sideDeck[i].AddAbilities(DoubleSprint.AbilityID);
                    }
                }

                //If the leapbot challenge is enabled, turn the sidedeck into leapbots.
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
                {
                    // Clear the side deck
                    sideDeck.Clear();

                    for (int i = 0; i < 10; i++)
                    {
                        sideDeck.Add(CardLoader.GetCardByName("LeapBot"));
                    }
                }

                //If turbo vessels are enabled as well, set the text to turbo leapbots instead.
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS) && (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK)))
                {
                    // Clear the side deck
                    sideDeck.Clear();

                    for (int i = 0; i < 10; i++)
                    {
                        sideDeck.Add(CardLoader.GetCardByName("P03KCM_TURBO_LEAPBOT"));
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.GetGemCard))]
        //[HarmonyPostfix]
        //private static void PostfixGetGemCard(string cardId, ref CardInfo __result)
        //{
        //    CardInfo cardByName = __result;

        //    // Don't apply side deck abilities only if turbo vessels is on
        //    if (!((SaveFile.IsAscension) && (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS))))
        //    {
        //        ApplySideDeckAbilitiesToCard(cardByName);
        //    }
        //}

        //private static void ApplySideDeckAbilitiesToCard(CardInfo cardInfo)
        //{
        //    CardModificationInfo cardModificationInfo = new CardModificationInfo();
        //    cardModificationInfo.abilities.AddRange(Part3SaveData.Data.sideDeckAbilities);
        //    cardModificationInfo.sideDeckMod = true;
        //    cardInfo.Mods.Add(cardModificationInfo);
        //}



        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.CreateVesselDeck))]
        [HarmonyPostfix]
        private static List<CardInfo> CreateVesselDeck(List<CardInfo> originalList)
        {
            List<CardInfo> list = new List<CardInfo>();
            for (int i = 0; i < 10; i++)
            {
                string text = "EmptyVessel";
                if (StoryEventsData.EventCompleted(StoryEvent.GemsModuleFetched))
                {
                    text = ((i >= Part3SaveData.Data.deckGemsDistribution[0]) ? ((i >= Part3SaveData.Data.deckGemsDistribution[0] + Part3SaveData.Data.deckGemsDistribution[1]) ? "EmptyVessel_BlueGem" : "EmptyVessel_OrangeGem") : "EmptyVessel_GreenGem");
                }

                if (SaveFile.IsAscension)
                {
                    //If the leapbot challenge is enabled, turn the sidedeck into leapbots.
                    if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
                    {
                        text = "LeapBot";

                        //If turbo vessels are enabled as well, set the text to turbo leapbots instead.
                        if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS))
                        {
                            text = "P03KCM_TURBO_LEAPBOT";
                        }
                    }
                }

                CardInfo cardByName = CardLoader.GetCardByName(text);
                AddSideDeckAbilitiesWithoutMesh(cardByName);
                list.Add(cardByName);
            }
            return list;
        }

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        private static void AddSideDeckAbilitiesWithoutMesh(CardInfo info)
        {
            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels))
            {
                if (info != null && !info.HasAbility(Ability.Submerge))
                {
                    CardModificationInfo abMod = new(Ability.Submerge);
                    abMod.sideDeckMod = true;
                    info.mods.Add(abMod);
                }
            }

            if ((info != null) && (SaveFile.IsAscension) && (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK)))
            {
                foreach (Ability ab in info.Abilities)
                {
                    CardModificationInfo abMod = new(ab);
                    abMod.sideDeckMod = true;

                    //Add the conduit
                    abMod.abilities.Add(Ability.ConduitNull);

                    info.mods.Add(abMod);
                }
            }

            //If turbo vessels are enabled but not leepbot sidedeck, change the side deck to be turbo vessels
            if ((SaveFile.IsAscension) && (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS)) && (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK)))
            {
                if (info != null)
                {
                    //If one side deck card doesn't have double sprint sigil, play challenge activation
                    //This method makes sure it only triggers once per run
                    if (!(turboVesselsUIPlayed))
                    {
                        ChallengeActivationUI.Instance.ShowActivation(TURBO_VESSELS);
                        turboVesselsUIPlayed = true;
                    }

                    CardModificationInfo abMod = new CardModificationInfo();
                    abMod.sideDeckMod = true;
                    abMod.nameReplacement = "Turbo Vessel";
                    abMod.abilities.Add(DoubleSprint.AbilityID);
                    info.SetPortrait(TextureHelper.GetImageAsTexture("portrait_turbovessel.png", typeof(CustomCards).Assembly));
                    info.mods.Add(abMod);
                }
            }
        }

        //Resets both variables so the challenge UI can activate again
        public static void ResetChallengeActivationUI()
        {
            turboVesselsUIPlayed = false;
            LeapingSidedeckUIPlayed = false;
            tradLivesUIPlayed = false;
            expensiveRespawnUIPlayed = false;
        }

        private static bool CardShouldExplode(this PlayableCard card)
        {
            return !card.Info.name.ToLowerInvariant().Contains("vessel") && !card.Info.HasTrait(Trait.Terrain);
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.UpdateFaceUpOnBoardEffects))]
        [HarmonyPostfix]
        private static void ShowExplosiveEffect(ref PlayableCard __instance)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(BOMB_CHALLENGE.challengeType) && __instance.CardShouldExplode())
            {
                __instance.Anim.SetExplosive(!__instance.Dead);
            }
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        private static IEnumerator AttachExplosivesToCard(IEnumerator sequence, PlayableCard card)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(BOMB_CHALLENGE.challengeType) && card.CardShouldExplode())
            {
                // Make sure the card has the explosive trigger receiver
                ExplodeOnDeath[] comps = card.gameObject.GetComponentsInChildren<ExplodeOnDeath>();
                if (comps == null || !comps.Any(c => c.GetType() == typeof(ExplodeOnDeath)))
                {
                    //CardTriggerHandler.AddReceiverToGameObject<AbilityBehaviour>(Ability.ExplodeOnDeath.ToString(), card.gameObject);
                    card.TriggerHandler.AddAbility(Ability.ExplodeOnDeath);
                }
            }
            yield return sequence;
        }

        [HarmonyPatch(typeof(TargetSlotItem), nameof(TargetSlotItem.ActivateSequence))]
        public static class HammerPatch
        {
            [HarmonyPrefix]
            private static void Prefix(ref TargetSlotItem __instance, ref TargetSlotItem __state)
            {
                __state = __instance;
            }

            [HarmonyPostfix]
            private static IEnumerator Postfix(IEnumerator sequence, TargetSlotItem __state)
            {
                if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ENERGY_HAMMER) && __state is HammerItem && TurnManager.Instance.IsPlayerTurn)
                {
                    if (ResourcesManager.Instance.PlayerEnergy < HAMMER_ENERGY_COST)
                    {
                        ChallengeActivationUI.Instance.ShowActivation(ENERGY_HAMMER);
                        __state.PlayShakeAnimation();
                        yield return new WaitForSeconds(0.2f);
                        yield break;
                    }
                }

                yield return sequence;
            }
        }

        [HarmonyPatch(typeof(HammerItem), nameof(HammerItem.OnValidTargetSelected))]
        public static class HammerPatchPostSelect
        {
            [HarmonyPrefix]
            private static void Prefix(ref HammerItem __instance, ref HammerItem __state)
            {
                __state = __instance;
            }

            [HarmonyPostfix]
            private static IEnumerator SpendHammerEnergy(IEnumerator sequence, HammerItem __state)
            {
                if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ENERGY_HAMMER) && TurnManager.Instance.IsPlayerTurn)
                {
                    if (ResourcesManager.Instance.PlayerEnergy < HAMMER_ENERGY_COST)
                    {
                        ChallengeActivationUI.Instance.ShowActivation(ENERGY_HAMMER);
                        __state.PlayShakeAnimation();
                        yield return new WaitForSeconds(0.2f);
                        yield break;
                    }

                    ChallengeActivationUI.Instance.ShowActivation(ENERGY_HAMMER);
                    yield return ResourcesManager.Instance.SpendEnergy(HAMMER_ENERGY_COST);
                }

                yield return sequence;
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        private static IEnumerator RotateCards(IEnumerator sequence, bool playerUpkeep)
        {
            yield return sequence;

            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR.challengeType) && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                if (TurnManager.Instance.TurnNumber > 0 && playerUpkeep)
                {
                    if (TurnManager.Instance.TurnNumber == 1)
                        ChallengeActivationUI.Instance.ShowActivation(ALL_CONVEYOR.challengeType);

                    yield return BoardManager.Instance.MoveAllCardsClockwise();
                }
            }
        }

        [HarmonyPatch(typeof(BoardStateSimulator), nameof(BoardStateSimulator.SimulateCombatPhase))]
        [HarmonyPrefix]
        private static void MakeAIRecognizeRotation(BoardState board, bool playerIsAttacker)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR.challengeType) && playerIsAttacker && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                // We need to rotate the board
                var anchorCard = board.playerSlots[0].card;
                for (int i = 1; i < board.playerSlots.Count; i++)
                    board.playerSlots[i-1].card = board.playerSlots[i].card;
                board.playerSlots[board.playerSlots.Count - 1].card = board.opponentSlots[board.opponentSlots.Count - 1].card;
                for (int i = board.opponentSlots.Count - 1; i > 0; i--)
                    board.opponentSlots[i].card = board.opponentSlots[i - 1].card;
                board.opponentSlots[0].card = anchorCard;
            }
        }

        [HarmonyPatch(typeof(BoardStateEvaluator), nameof(BoardStateEvaluator.EvaluateCard))]
        [HarmonyPostfix]
        private static void MakeAIPreferLeftmostBountyHunters(BoardState.CardState card, BoardState board, ref int __result)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR.challengeType) && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                if (board.opponentSlots.Contains(card.slot))
                {
                    if (card.info.mods.Any(m => m.bountyHunterInfo != null))
                    {
                        int bestSlot = card.HasAbility(Ability.SplitStrike) ? 1 : 0;
                        __result -= Math.Abs(board.opponentSlots.IndexOf(card.slot) - bestSlot);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BoardManager3D), nameof(BoardManager3D.ShowSlots))]
        [HarmonyPrefix]
        private static void SetRotationSlotTextures()
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.ChallengeIsActive(ALL_CONVEYOR.challengeType) && TurnManager.Instance.opponent is not Part3BossOpponent)
            {
                for (int i = 0; i < BoardManager.Instance.opponentSlots.Count - 1; i++)
                    BoardManager.Instance.opponentSlots[i].SetTexture(Resources.Load<Texture2D>("art/cards/card_slot_left"));

                BoardManager.Instance.opponentSlots[BoardManager.Instance.opponentSlots.Count - 1].SetTexture(TextureHelper.GetImageAsTexture("cadslot_up.png", typeof(AscensionChallengeManagement).Assembly));

                for (int i = 1; i < BoardManager.Instance.playerSlots.Count; i++)
                    BoardManager.Instance.playerSlots[i].SetTexture(Resources.Load<Texture2D>("art/cards/card_slot_left"));

                BoardManager.Instance.playerSlots[0].SetTexture(TextureHelper.GetImageAsTexture("cadslot_up.png", typeof(AscensionChallengeManagement).Assembly));
            }
        }
    }
}