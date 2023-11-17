using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class AscensionChallengeManagement
    {
        public static string NO_LESHY = "noleshy";

        public static AscensionChallenge BOUNTY_HUNTER { get; private set; }
        public static AscensionChallenge ENERGY_HAMMER { get; private set; }
        public static AscensionChallenge TRADITIONAL_LIVES { get; private set; }

        public static AscensionChallenge LEEPBOT_SIDEDECK { get; private set; }
        public static AscensionChallenge TURBO_VESSELS { get; private set; }
        public static AscensionChallenge PAINTING_CHALLENGE { get; private set; }

        public static bool SKULL_STORM_ACTIVE =>
            AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessConsumables) >= 2
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(TURBO_VESSELS) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.ExpensivePelts) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(PAINTING_CHALLENGE) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(ENERGY_HAMMER) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(LEEPBOT_SIDEDECK) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty) >= 2
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(TRADITIONAL_LIVES) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessLives) > 0
            && AscensionSaveData.Data.GetNumChallengesOfTypeActive(BOUNTY_HUNTER) >= 2;

        internal static bool TurboVesselsUIPlayed
        {
            get => ModdedSaveManager.RunState.GetValueAsBoolean(P03Plugin.PluginGuid, "TurboVesselsUIPlayed");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "TurboVesselsUIPlayed", value);
        }

        internal static bool LeapingSidedeckUIPlayed
        {
            get => ModdedSaveManager.RunState.GetValueAsBoolean(P03Plugin.PluginGuid, "LeapingSidedeckUIPlayed");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "LeapingSidedeckUIPlayed", value);
        }

        internal static bool TradLivesUIPlayed
        {
            get => ModdedSaveManager.RunState.GetValueAsBoolean(P03Plugin.PluginGuid, "TradLivesUIPlayed");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "TradLivesUIPlayed", value);
        }

        internal static bool ExpensiveRespawnUIPlayed
        {
            get => ModdedSaveManager.RunState.GetValueAsBoolean(P03Plugin.PluginGuid, "ExpensiveRespawnUIPlayed");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "ExpensiveRespawnUIPlayed", value);
        }

        private static CanvasBossOpponent CanvasBoss => Singleton<TurnManager>.Instance.Opponent as CanvasBossOpponent;
        private static readonly CompositeRuleTriggerHandler rulesHandler;
        private static readonly CompositeBattleRule currentRule;
        private static readonly CompositeRuleDisplayer ruleDisplayer;

        public static Part3BossOpponent dummyCanvasBoss;

        private static string CompatibleChallengeList => ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, "P03CompatibleChallenges");

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
            0).SetFlags("p03", NO_LESHY);
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
            0).SetFlags("p03", NO_LESHY);
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

            PatchedChallengesReference = new() {
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

                {
                    AscensionChallenge.NoClover,
                    new() {
                        challengeType = TURBO_VESSELS,
                        title = "Turbo Vessels",
                        description = "Your vessels have the Double Sprinter sigil.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_turbovessel.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_turbovessel_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 5
                    }
                },

                {
                    AscensionChallenge.SubmergeSquirrels,
                    new() {
                        challengeType = BOUNTY_HUNTER,
                        title = "Wanted Fugitive",
                        description = "Your bounty level is permanently increased by 1",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 10
                    }
                },

                {
                    AscensionChallenge.GrizzlyMode,
                    new() {
                        challengeType = BOUNTY_HUNTER,
                        title = "Wanted Fugitive",
                        description = "Your bounty level is permanently increased by 1",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 10
                    }
                },

                {
                    AscensionChallenge.BossTotems,
                    new() {
                        challengeType = PAINTING_CHALLENGE,
                        title = "Eccentric Painter",
                        description = "All bosses start with a random canvas rule.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_eccentricpainter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_eccentricpainter_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 35
                    }
                },

                {
                    AscensionChallenge.AllTotems,
                    new() {
                        challengeType = ENERGY_HAMMER,
                        title = "Energy Hammer",
                        description = "The hammer now costs 2 energy to use",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 10
                    }
                },

                {
                    AscensionChallenge.NoHook,
                    ChallengeManager.BaseGameChallenges.First(fc => fc.Challenge.challengeType == AscensionChallenge.LessConsumables)
                },

                {
                    AscensionChallenge.ExpensivePelts,
                    new() {
                        challengeType = AscensionChallenge.ExpensivePelts,
                        title = "Pricey Upgrades",
                        description = "All upgrades cost more",
                        iconSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_expensivepelts"), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 5
                    }
                },

                {
                    AscensionChallenge.LessLives,
                    new() {
                        challengeType = AscensionChallenge.LessLives,
                        title = "Costly Respawn",
                        description = "Respawn Cost is 15. With Traditional Lives, you've one chance.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_oneup.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 20
                    }
                },

                {
                    AscensionChallenge.WeakStarterDeck,
                    new() {
                        challengeType = TRADITIONAL_LIVES,
                        title = "Traditional Lives",
                        description = "You have two lives per region.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_tradLives.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_tradLives_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 10
                    }
                },

                {
                    AscensionChallenge.NoBossRares,
                    new() {
                        challengeType = LEEPBOT_SIDEDECK,
                        title = "Leaping Side Deck",
                        description = "Replace your Empty Vessels with L33pbots.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_leepbot.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_leepbot_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 15
                    }
                }
            };

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
                {
                    for (int i = 0; i < challenges.Count; i++)
                    {
                        if (PatchedChallengesReference.ContainsKey(challenges[i].Challenge.challengeType))
                        {
                            //challenges[i] = PatchedChallengesReference[challenges[i].challengeType];
                            challenges[i] = new()
                            {
                                Challenge = PatchedChallengesReference[challenges[i].Challenge.challengeType],
                                AppearancesInChallengeScreen = 1,
                                UnlockLevel = challenges[i].UnlockLevel
                            };
                        }
                    }
                }

                return challenges;
            };
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), nameof(AscensionChallengeScreen.OnEnable))]
        [HarmonyPostfix]
        private static void HideLockedBossIcon(AscensionChallengeScreen __instance) => __instance.gameObject.GetComponentInChildren<ChallengeIconGrid>().Start();

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
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (!AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel))
            {
                __instance.finalBossIcon.SetActive(false);
                float xStart = -1.65f;
                for (int i = 0; i < __instance.topRowIcons.Count; i++)
                    __instance.topRowIcons[i].localPosition = new Vector2(xStart + (i * 0.55f), __instance.topRowIcons[i].localPosition.y);

                for (int j = 0; j < __instance.bottomRowIcons.Count; j++)
                    __instance.bottomRowIcons[j].localPosition = new Vector2(xStart + (j * 0.55f), __instance.bottomRowIcons[j].localPosition.y);
            }
        }


        [HarmonyPatch(typeof(AscensionUnlockSchedule), nameof(AscensionUnlockSchedule.ChallengeIsUnlockedForLevel))]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        [HarmonyPostfix]
        public static void ValidP03Challenges(ref bool __result, AscensionChallenge challenge, int level)
        {
            ChallengeManager.FullChallenge fullChallenge = ChallengeManager.AllChallenges.FirstOrDefault(fc => fc.Challenge.challengeType == challenge);
            if (fullChallenge == null)
                return;

            if (ScreenManagement.ScreenState == CardTemple.Tech)
            {
                if (PatchedChallengesReference.Any(kvp => kvp.Value.challengeType == challenge))
                {
                    KeyValuePair<AscensionChallenge, AscensionChallengeInfo> kvp = PatchedChallengesReference.First(kvp => kvp.Value.challengeType == challenge);
                    if (kvp.Value.challengeType != kvp.Key)
                    {
                        __result = AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(kvp.Key, level);
                        return;
                    }
                }

                if (ValidChallenges.Contains(challenge))
                    return;


                if (fullChallenge.Flags == null)
                {
                    __result = false;
                    return;
                }

                if (!fullChallenge.Flags.Any(f => f != null && f.ToString().Equals("p03", StringComparison.InvariantCultureIgnoreCase)))
                {
                    __result = false;
                    return;
                }
            }
            else if (ScreenManagement.ScreenState == CardTemple.Nature) // Make sure the noleshy challenges are disabled
            {
                if (fullChallenge.Flags != null)
                {
                    if (fullChallenge.Flags.Any(f => f != null && f.ToString().Equals(NO_LESHY, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }

        private static readonly Texture2D TURBO_SPRINTER_TEXTURE = TextureHelper.GetImageAsTexture("portrait_turbovessel.png", typeof(AscensionChallengeManagement).Assembly);
        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        private static void UpdateSidedeckMod(CardInfo info)
        {
            if (info == null)
                return;

            if (AscensionSaveData.Data.ChallengeIsActive(TURBO_VESSELS) && info.name.StartsWith("EmptyVessel"))
            {
                CardModificationInfo mod = new()
                {
                    abilities = new() { DoubleSprint.AbilityID },
                    nameReplacement = "Turbo Vessel"
                };
                info.mods.Add(mod);
                info.SetPortrait(TURBO_SPRINTER_TEXTURE);
            }

            if (AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK))
            {
                CardModificationInfo antiMod = new()
                {
                    negateAbilities = new() { Ability.ConduitNull }
                };
                info.mods.Add(antiMod);
            }
        }

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.CreateVesselDeck))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        private static bool BuildPart3SideDeck(ref List<CardInfo> __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (SaveFile.IsAscension)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK))
                    ChallengeActivationUI.Instance.ShowActivation(LEEPBOT_SIDEDECK);

                if (AscensionSaveData.Data.ChallengeIsActive(TURBO_VESSELS))
                    ChallengeActivationUI.Instance.ShowActivation(TURBO_VESSELS);
            }

            // Start by getting all of the card names
            IEnumerable<string> cardNames = Enumerable.Empty<string>();
            if (AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK))
            {
                cardNames = AscensionSaveData.Data.ChallengeIsActive(TURBO_VESSELS)
                    ? cardNames.Concat(Enumerable.Repeat("P03KCM_TURBO_LEAPBOT", 10))
                    : cardNames.Concat(Enumerable.Repeat("LeapBot", 10));
            }
            else if (StoryEventsData.EventCompleted(StoryEvent.GemsModuleFetched))
            {
                foreach (GemType gem in Enum.GetValues(typeof(GemType))) // TODO: Consider support for custom gems?
                {
                    int gemCount = Part3SaveData.Data.deckGemsDistribution[(int)gem];
                    string gemCardName = $"EmptyVessel_{gem}Gem";

                    cardNames = cardNames.Concat(Enumerable.Repeat(gemCardName, gemCount));
                }
            }
            else
            {
                cardNames = cardNames.Concat(Enumerable.Repeat("EmptyVessel", 10));
            }

            // And now get each card
            __result = cardNames.Select(CardLoader.GetCardByName).ToList();
            __result.ForEach(Part3CardDrawPiles.AddModsToVessel);
            return false;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.BuildSideDeck))]
        [HarmonyPrefix]
        private static bool ReplaceBuildSideDeck(Part3DeckReviewSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            __instance.sideDeck.Clear();
            __instance.sideDeck.AddRange(Part3CardDrawPiles.CreateVesselDeck());
            return false;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.ApplySideDeckAbilitiesToCard))]
        [HarmonyPrefix]
        private static bool ReplaceAddSideDeck(CardInfo cardInfo)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            string currentAbilityString = String.Join(", ", cardInfo.Abilities);
            P03Plugin.Log.LogDebug($"Before applying side deck abilities card {cardInfo.DisplayedNameEnglish} has {currentAbilityString}");
            Part3CardDrawPiles.AddModsToVessel(cardInfo);
            currentAbilityString = String.Join(", ", cardInfo.Abilities);
            P03Plugin.Log.LogDebug($"After applying side deck abilities card {cardInfo.DisplayedNameEnglish} has {currentAbilityString}");
            return false;
        }

        [HarmonyPatch(typeof(TargetSlotItem), nameof(TargetSlotItem.ActivateSequence))]
        public static class HammerPatch
        {
            [HarmonyPrefix]
            private static void Prefix(ref TargetSlotItem __instance, ref TargetSlotItem __state) => __state = __instance;

            [HarmonyPostfix]
            private static IEnumerator Postfix(IEnumerator sequence, TargetSlotItem __state)
            {
                if (P03AscensionSaveData.IsP03Run && AscensionSaveData.Data.ChallengeIsActive(ENERGY_HAMMER) && __state is HammerItem && TurnManager.Instance.IsPlayerTurn)
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
            private static void Prefix(ref HammerItem __instance, ref HammerItem __state) => __state = __instance;

            [HarmonyPostfix]
            private static IEnumerator SpendHammerEnergy(IEnumerator sequence, HammerItem __state)
            {
                if (P03AscensionSaveData.IsP03Run && AscensionSaveData.Data.ChallengeIsActive(ENERGY_HAMMER) && TurnManager.Instance.IsPlayerTurn)
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

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.Start))]
        [HarmonyPostfix]
        private static void AddLeepBot(Part3DeckReviewSequencer __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.vesselFigurine.transform.Find("LeepBot") == null)
                {
                    GameObject lpBot = UnityEngine.Object.Instantiate(AssetBundleManager.Prefabs["LeepBot"], __instance.vesselFigurine.transform);
                    lpBot.transform.localPosition = new(-.4f, .6f, -.2f);
                    lpBot.transform.localEulerAngles = new(0f, 261f, 350f);
                    lpBot.name = "LeepBot";
                }
            }
        }

        [HarmonyPatch(typeof(CardPile), nameof(CardPile.SetFigurineShown))]
        [HarmonyPrefix]
        private static void AddLeepBotToBattle(CardPile __instance)
        {
            if (P03AscensionSaveData.IsP03Run)
            {
                if (__instance.figurine != null)
                {
                    Transform lpBot = __instance.figurine.transform.Find("LeepBot");

                    if (lpBot == null)
                    {
                        lpBot = UnityEngine.Object.Instantiate(AssetBundleManager.Prefabs["LeepBot"], __instance.figurine.transform).transform;
                        lpBot.localPosition = new(1.5f, 0f, 0f);
                        __instance.defaultFigurinePos = lpBot.localPosition;
                        lpBot.localEulerAngles = new(0f, 287f, 350f);
                        lpBot.gameObject.name = "LeepBot";
                    }

                    __instance.figurine.transform.Find("Anim").gameObject.SetActive(!AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK));
                    lpBot.gameObject.SetActive(AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK));
                }
            }
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.SpawnDeckPiles))]
        [HarmonyPostfix]
        private static IEnumerator ToggleLeepBot(IEnumerator sequence, Part3DeckReviewSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            __instance.vesselFigurine.transform.Find("Anim").gameObject.SetActive(!AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK));
            __instance.vesselFigurine.transform.Find("LeepBot").gameObject.SetActive(AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK));

            yield return sequence;
        }
    }
}