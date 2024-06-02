using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using GBC;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Items;
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

        public static readonly AscensionChallenge BOUNTY_HUNTER = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "HigherBounties");
        public static readonly AscensionChallenge ENERGY_HAMMER = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "EnergyHammer");
        public static readonly AscensionChallenge TRADITIONAL_LIVES = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "Traditional Lives");
        public static readonly AscensionChallenge BROKEN_BRIDGE = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "BrokenBridge");
        public static readonly AscensionChallenge BATTLE_MODIFIERS = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "BattleModifiers");
        public static readonly AscensionChallenge LEEPBOT_SIDEDECK = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "LeepbotSidedeck");
        public static readonly AscensionChallenge TURBO_VESSELS = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "TurboVessels");
        public static readonly AscensionChallenge PAINTING_CHALLENGE = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "PaintingChallenge");
        public static readonly AscensionChallenge ALL_CONVEYOR = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "Overactive Factory");
        public static readonly AscensionChallenge BOMB_CHALLENGE = GuidManager.GetEnumValue<AscensionChallenge>(P03Plugin.PluginGuid, "Explosive Bots");

        internal static List<ChallengeManager.FullChallenge> PageOneChallenges = new();

        internal static readonly int[] ChallengePointsPerLevel = new int[] { 5, 20, 45, 70, 100 };

        internal static readonly Sprite SKULL_SPRITE = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_skull.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon);
        internal static readonly Sprite SKULL_EYES_SPRITE = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_skull_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon);
        internal static AscensionChallengeInfo FAKE_FINAL_BOSS_INFO { get; private set; }

        public static bool SKULL_STORM_ACTIVE
        {
            get
            {
                foreach (ChallengeManager.FullChallenge fc in PageOneChallenges)
                {
                    if (fc.UnlockLevel > 13)
                        continue;
                    int count = fc.AppearancesInChallengeScreen;
                    int active = AscensionSaveData.Data.GetNumChallengesOfTypeActive(fc.Challenge.challengeType);
                    P03Plugin.Log.LogInfo($"Scarlet Skull Check. Expecting {count} {fc.Challenge.title}, found {active}");
                    if (active < count)
                        return false;
                }
                return true;
            }
        }

        internal static bool TurboVesselsUIPlayed
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(P03Plugin.PluginGuid, "TurboVesselsUIPlayed");
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "TurboVesselsUIPlayed", value);
        }

        internal static bool LeapingSidedeckUIPlayed
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(P03Plugin.PluginGuid, "LeapingSidedeckUIPlayed");
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "LeapingSidedeckUIPlayed", value);
        }

        internal static bool TradLivesUIPlayed
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(P03Plugin.PluginGuid, "TradLivesUIPlayed");
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "TradLivesUIPlayed", value);
        }

        internal static bool ExpensiveRespawnUIPlayed
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(P03Plugin.PluginGuid, "ExpensiveRespawnUIPlayed");
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "ExpensiveRespawnUIPlayed", value);
        }

        private static string CompatibleChallengeList => ModdedSaveManager.SaveData.GetValue(P03Plugin.PluginGuid, "P03CompatibleChallenges");

        public const int HAMMER_ENERGY_COST = 2;

        public static void UpdateP03Challenges()
        {
            FAKE_FINAL_BOSS_INFO = new()
            {
                challengeType = AscensionChallenge.FinalBoss,
                title = "Final Boss",
                description = "Description for the Final Boss challenge",
                iconSprite = SKULL_SPRITE,
                activatedSprite = SKULL_EYES_SPRITE,
                pointValue = 15
            };

            // Page 2+ challenges, managed entirely by the challenge manager
            // BOMB_CHALLENGE = ChallengeManager.AddSpecific(P03Plugin.PluginGuid,
            // "Explosive Bots",
            // "All non-vessel bots self destruct when they die",
            // 0,
            // TextureHelper.GetImageAsTexture("ascensionicon_bomb.png", typeof(AscensionChallengeManagement).Assembly),
            // TextureHelper.GetImageAsTexture("ascensionicon_bombactivated.png", typeof(AscensionChallengeManagement).Assembly),
            // 6).SetFlags("p03", NO_LESHY);

            // ALL_CONVEYOR = ChallengeManager.AddSpecific(P03Plugin.PluginGuid,
            // "Overactive Factory",
            // "All regular battles are conveyor battles",
            // 0,
            // TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle.png", typeof(AscensionChallengeManagement).Assembly),
            // TextureHelper.GetImageAsTexture("ascensionicon_conveyorbattle_active.png", typeof(AscensionChallengeManagement).Assembly),
            // 6).SetFlags("p03", NO_LESHY);

            // TRADITIONAL_LIVES = ChallengeManager.AddSpecific(P03Plugin.PluginGuid,
            // "Traditional Lives",
            // "You have two lives per region.",
            // 10,
            // TextureHelper.GetImageAsTexture("ascensionicon_tradLives.png", typeof(AscensionChallengeManagement).Assembly),
            // TextureHelper.GetImageAsTexture("ascensionicon_tradLives_activated.png", typeof(AscensionChallengeManagement).Assembly),
            // 6).SetFlags("p03", NO_LESHY).Challenge.challengeType;

            // Page 1 Challenges
            PageOneChallenges.AddRange(new List<ChallengeManager.FullChallenge>()
            {
                // Backpack, first instance
                new() {
                    Challenge = ChallengeManager.BaseGameChallenges.First(fc => fc.Challenge.challengeType == AscensionChallenge.LessConsumables),
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 1
                },

                // Backpack, second instance
                new() {
                    Challenge = new() {
                        challengeType = AscensionChallenge.NoHook,
                        title = "Missing Remote",
                        description = "You do not start with Mrs. Bomb's Remote",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_nohook.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_nohook_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 5
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 1
                },

                // Turbo Vessels
                new () {
                    Challenge = new() {
                        challengeType = TURBO_VESSELS,
                        title = "Turbo Vessels",
                        description = "Your vessels have the Double Sprinter sigil.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_turbovessel.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_turbovessel_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 5
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 1
                },

                // Broken Bridge
                new() {
                    Challenge = new () {
                        challengeType = BROKEN_BRIDGE,
                        title = "Broken Bridges",
                        description = "Only two regions are available at the start of each run",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_broken_bridge.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_broken_bridge_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 5
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 1
                },

                // Pricey Unlocks
                new() {
                    Challenge = new() {
                        challengeType = AscensionChallenge.ExpensivePelts,
                        title = "Pricey Upgrades",
                        description = "All upgrades cost more",
                        iconSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_expensivepelts"), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 15
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 2
                },

                // Tipped Scales
                new() {
                    Challenge = ChallengeManager.BaseGameChallenges.First(fc => fc.Challenge.challengeType == AscensionChallenge.StartingDamage),
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 2
                },

                // Eccentric Painter
                new () {
                    Challenge = new() {
                        challengeType = PAINTING_CHALLENGE,
                        title = "Eccentric Painter",
                        description = "All bosses start with a random canvas rule.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_eccentricpainter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_eccentricpainter_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 25
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 3
                },

                // Energy Hammer
                new() {
                    Challenge = new() {
                        challengeType = ENERGY_HAMMER,
                        title = "Energy Hammer",
                        description = "The hammer now costs 2 energy to use",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_energyhammer_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 10
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 3
                },

                // More Difficulty
                new() {
                    Challenge = ChallengeManager.BaseGameChallenges.First(fc => fc.Challenge.challengeType == AscensionChallenge.BaseDifficulty),
                    AppearancesInChallengeScreen = 2,
                    UnlockLevel = 4
                },

                // new() {
                //     Challenge = ChallengeManager.BaseGameChallenges.First(fc => fc.Challenge.challengeType == AscensionChallenge.BaseDifficulty),
                //     AppearancesInChallengeScreen = 1,
                //     UnlockLevel = 4
                // },

                // Expensive Respawns
                new() {
                    Challenge = new() {
                        challengeType = AscensionChallenge.LessLives,
                        title = "Costly Respawn",
                        description = "The cost for additional respawns is tripled",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_oneup.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 25
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 5,
                },

                // Leapbot Sidedeck
                new() {
                    Challenge = new() {
                        challengeType = LEEPBOT_SIDEDECK,
                        title = "Leaping Side Deck",
                        description = "Replace your Empty Vessels with L33pbots.",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_leepbot.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_leepbot_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 15
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 5
                },

                // Bounty Hunters
                new() {
                    Challenge = new() {
                        challengeType = BOUNTY_HUNTER,
                        title = "Wanted Fugitive",
                        description = "Your bounty level is permanently increased by 2",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_bounthunter.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 20
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 6
                },

                new() {
                    Challenge = new() {
                        challengeType = BATTLE_MODIFIERS,
                        title = "Strange Encounters",
                        description = "Some battles on each map will have additional effects",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_modifiers.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_modifiers_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 20
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 6
                },

                // Final Boss
                new() {
                    Challenge = new() {
                        challengeType = AscensionChallenge.FinalBoss,
                        title = "The Great Transcendence",
                        description = "Unlock the secrets of the Great Transcendence",
                        iconSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_multiverse.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        activatedSprite = TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("ascensionicon_multiverse_activated.png", typeof(AscensionChallengeManagement).Assembly), TextureHelper.SpriteType.ChallengeIcon),
                        pointValue = 15
                    },
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = 8,
                },
            });

            ChallengeManager.ModifyChallenges += delegate (List<ChallengeManager.FullChallenge> challenges)
            {
                if (P03AscensionSaveData.IsP03Run)
                {
                    // Find the index of the final boss challenge
                    int fbIdx = challenges.FindIndex(f => f.Challenge.challengeType == AscensionChallenge.FinalBoss);
                    for (int i = 0; i <= fbIdx; i++)
                        challenges.RemoveAt(0);
                    for (int i = 0; i < PageOneChallenges.Count; i++)
                        challenges.Insert(i, PageOneChallenges[i]);
                }

                return challenges;
            };
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), nameof(AscensionChallengeScreen.OnEnable))]
        [HarmonyPostfix]
        private static void HideLockedBossIcon(AscensionChallengeScreen __instance) => __instance.gameObject.GetComponentInChildren<ChallengeIconGrid>().Start();

        [HarmonyPatch(typeof(ChallengeIconGrid), nameof(ChallengeIconGrid.Start))]
        [HarmonyPrefix]
        private static void DynamicSwapSize(ChallengeIconGrid __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            __instance.finalBossIcon.SetActive(false);
            if (!AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel))
            {
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
                if (PageOneChallenges.Any(fc => fc.Challenge.challengeType == challenge))
                {
                    __result = fullChallenge.UnlockLevel <= AscensionSaveData.Data.challengeLevel;
                    return;
                }

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

        private static void EnsureSingletonFinalBoss()
        {
            while (P03AscensionSaveData.P03Data.activeChallenges.Where(c => c == AscensionChallenge.FinalBoss).Count() > 1)
            {
                P03AscensionSaveData.P03Data.activeChallenges.Remove(AscensionChallenge.FinalBoss);
                AscensionChallengeScreen.Instance?.challengeLevelText?.UpdateText();
            }
        }

        [HarmonyPatch(typeof(AscensionIconInteractable), nameof(AscensionIconInteractable.OnCursorSelectStart))]
        [HarmonyPrefix]
        private static bool SpecialFinalBossClick(AscensionIconInteractable __instance)
        {
            if (P03AscensionSaveData.IsP03Run &&
                __instance.challengeInfo.challengeType == AscensionChallenge.FinalBoss
                && !P03AscensionSaveData.P03Data.conqueredChallenges.Contains(AscensionChallenge.FinalBoss)
                && AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(__instance.challengeInfo.challengeType, P03AscensionSaveData.P03Data.challengeLevel)
                && !P03AscensionSaveData.LeshyIsDead
                && __instance.clickable)
            {
                __instance.clickable = false;

                if (__instance.iconRenderer != null)
                    __instance.iconRenderer.enabled = false;

                if (__instance.activatedRenderer != null)
                    __instance.activatedRenderer.enabled = false;

                __instance.gameObject.SetActive(false);
                P03AscensionSaveData.P03Data.activeChallenges.Add(AscensionChallenge.FinalBoss);

                GBCUIManager.Instance.transform.parent.GetComponentInChildren<ScreenGlitchEffect>().SetIntensity(1f, .4f);
                GBC.CameraEffects.Instance.Shake(0.1f, .4f);
                AudioController.Instance.PlaySound2D("glitch", MixerGroup.VideoCam, 1f);

                EnsureSingletonFinalBoss();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionIconInteractable), nameof(AscensionIconInteractable.AssignInfo))]
        [HarmonyPrefix]
        private static bool SpecialFinalBossIcon(AscensionChallengeInfo info, AscensionIconInteractable __instance)
        {
            if (P03AscensionSaveData.IsP03Run &&
                info.challengeType == AscensionChallenge.FinalBoss
                && !P03AscensionSaveData.P03Data.conqueredChallenges.Contains(AscensionChallenge.FinalBoss)
                && AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(info.challengeType, P03AscensionSaveData.P03Data.challengeLevel))
            {
                __instance.challengeInfo = info;
                __instance.gameObject.SetActive(true);
                if (P03AscensionSaveData.LeshyIsDead)
                {
                    __instance.clickable = false;

                    if (__instance.iconRenderer != null)
                    {
                        __instance.iconRenderer.sprite = info.iconSprite;
                        __instance.iconRenderer.enabled = true;
                    }

                    if (__instance.activatedRenderer != null)
                    {
                        __instance.activatedRenderer.sprite = info.activatedSprite;
                        __instance.activatedRenderer.enabled = true;
                    }
                    AscensionChallengeScreen.Instance?.SetChallengeActivated(info, true);
                }
                else if (P03AscensionSaveData.P03Data.activeChallenges.Contains(info.challengeType))
                {
                    __instance.clickable = false;
                    __instance.gameObject.SetActive(false);

                    if (__instance.iconRenderer != null)
                        __instance.iconRenderer.enabled = false;

                    if (__instance.activatedRenderer != null)
                        __instance.activatedRenderer.enabled = false;
                }
                else
                {
                    __instance.clickable = true;
                    __instance.challengeInfo = FAKE_FINAL_BOSS_INFO;

                    if (__instance.iconRenderer != null)
                    {
                        __instance.iconRenderer.enabled = true;
                        __instance.iconRenderer.sprite = SKULL_SPRITE;
                    }

                    if (__instance.activatedRenderer != null)
                        __instance.activatedRenderer.sprite = SKULL_EYES_SPRITE;
                }
                EnsureSingletonFinalBoss();
                return false;
            }
            return true;
        }


        private static readonly Texture2D TURBO_SPRINTER_TEXTURE = TextureHelper.GetImageAsTexture("portrait_turbovessel.png", typeof(AscensionChallengeManagement).Assembly);
        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        private static void UpdateSidedeckMod(CardInfo info)
        {
            if (info == null)
                return;

            // The side deck card cannot be sacrificable
            if (P03AscensionSaveData.IsP03Run)
            {
                info.traits ??= new();
                info.traits.Add(CustomCards.Unsackable);
            }

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
            private static void Prefix(ref HammerItem __instance, CardSlot targetSlot, ref HammerItem __state)
            {
                ItemSlotPatches.LastSlotHammered = targetSlot;
                __state = __instance;
            }

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

                ItemSlotPatches.LastSlotHammered = null;
                yield break;
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