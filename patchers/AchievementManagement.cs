using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Achievements;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class P03AchievementManagement
    {
        public static Achievement SKULLSTORM { get; private set; }
        public static Achievement FIRST_WIN { get; private set; }
        public static Achievement CONTROL_NFT { get; private set; }
        public static Achievement SURVIVE_SIX_ARCHIVIST { get; private set; }
        public static Achievement DONT_USE_CAMERA { get; private set; }
        public static Achievement CANVAS_ENOUGH { get; private set; }
        public static Achievement KILL_30_BOUNTY_HUNTERS { get; private set; }
        public static Achievement MYCOLOGISTS_COMPLETED { get; private set; }
        public static Achievement ALL_QUESTS_COMPLETED { get; private set; }
        public static Achievement KILL_QUEST_CARD { get; private set; }
        public static Achievement TURBO_RAMP { get; private set; }
        public static Achievement MASSIVE_OVERKILL { get; private set; }
        public static Achievement FULLY_UPGRADED { get; private set; }
        public static Achievement MAX_SP_CARD { get; private set; }
        public static Achievement SIX_SHOOTER { get; private set; }
        public static Achievement SCALES_TILTED_3X { get; private set; }
        public static Achievement AVOID_BOUNTY_HUNTERS { get; private set; }
        public static Achievement PLASMA_JIMMY_CRAZY { get; private set; }
        public static Achievement FULLY_OVERCLOCKED { get; private set; }
        public static Achievement FAST_GENERATOR { get; private set; }

        private static int BountyHuntersKilled
        {
            get => ModdedSaveManager.SaveData.GetValueAsInt(P03Plugin.PluginGuid, "BountyHuntersKilledLifetime");
            set => ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "BountyHuntersKilledLifetime", value);
        }

        private static int BattlesWithoutBountyHuntersKilled
        {
            get => ModdedSaveManager.SaveData.GetValueAsInt(P03Plugin.PluginGuid, "BattlesWithoutBountyHuntersKilled");
            set => ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "BattlesWithoutBountyHuntersKilled", value);
        }

        static P03AchievementManagement()
        {
            ModdedAchievementManager.AchievementGroupDefinition grp = ModdedAchievementManager.NewGroup(
                P03Plugin.PluginGuid,
                "P03 In Kaycee's Mod",
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            );

            FIRST_WIN = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Uber Apocalypse",
                "Beat \"P03 in Kaycee's Mod\" for the first time",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [🚫] sticker

            SKULLSTORM = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Scarlet Skull",
                "Win a run with every challenge skull from the first page enabled",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_skull.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [red skull] sticker

            CONTROL_NFT = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Avatar",
                "Take ownership of one of G0lly!'s beloved trinkets",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [an ages dead cat meme, like 2010] sticker

            SURVIVE_SIX_ARCHIVIST = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Book of the Dead",
                "During The Archivist's boss fight, survive 6 turns during phase 2",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [101010 strip] sticker

            DONT_USE_CAMERA = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Feral Spirit",
                "Defeat the Photographer without using the camera",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [broken camera] sticker

            CANVAS_ENOUGH = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Gallery Showcase",
                "Create an infinite loop in the Unfinished Boss fight",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [P03's annoyed face as an a rageface] sticker

            KILL_30_BOUNTY_HUNTERS = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Battle Frenzy",
                "Destroy 30 Bounty Hunters in your lifetime",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [cowboy hat] sticker

            ALL_QUESTS_COMPLETED = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Calling for Backup",
                "Complete all NPC quests that appear in a single run",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [smiley face with a wink and thumbs up] sticker

            KILL_QUEST_CARD = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Accelerated Fuse",
                "Skeleclock and destroy a card given to you by an NPC",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [scorched companion cube] sticker

            TURBO_RAMP = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Mana Blink",
                "Have six energy available on turn two",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [leaky battery] sticker

            MASSIVE_OVERKILL = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Aimless Assault",
                "Earn 15 Robobucks in a single battle",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [tophat and monocle] sticker

            FULLY_UPGRADED = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Rebuild From Scrap",
                "Have a card in your deck that is gemified, skeleclocked, transformable, and can complete a circuit",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [💪] sticker

            MAX_SP_CARD = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Buy Your Buddy",
                "Give a card with the maximum amount of SP to the Bot Builder",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [Sir Fire, Esquire] sticker

            SIX_SHOOTER = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Me Smash",
                "Strike P03 six separate times in a single turn",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [smoking revolver] sticker

            SCALES_TILTED_3X = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Absolution",
                "Have the scales tiled 4 points against you on three different turns",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [that one "hang in there!" cat poster] sticker

            AVOID_BOUNTY_HUNTERS = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "The Perfect Crime",
                "Win five battles with bounty hunters without killing them",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [a pig with an apple in its mouth] sticker

            PLASMA_JIMMY_CRAZY = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Blastenheimer 5000 Ultra Cannon",
                "Deal 6 damage with Plasma Jimmy in a single turn",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_plasma.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [orange wizard hat] sticker

            FULLY_OVERCLOCKED = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Double Time",
                "Skeleclock every card in your deck",
                false,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [guillotine] sticker

            MYCOLOGISTS_COMPLETED = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "One And The Same",
                "Complete the Mycologist's experiment and create an abomination of science",
                true,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [trippy rainbow shrooms] sticker

            FAST_GENERATOR = ModdedAchievementManager.New(
                P03Plugin.PluginGuid,
                "Burning Adrenaline",
                "Repair the generator in three turns or less",
                true,
                grp.ID,
                TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(P03AchievementManagement).Assembly)
            ).ID; // [hermes boots, shoe with wings on it] sticker
        }

        private class CardBattleAchievementMonitor : Singleton<CardBattleAchievementMonitor>
        {
            private bool BattleActive = false;

            private bool UnlockedApe = false;
            private bool UnlockedCurrency = false;
            private int ArchivistTurns = 0;
            internal bool UsedCamera = false;
            private int InitialCurrency = 0;
            private int StrikesThisTurn = 0;
            private int TippedScalesTurns = 0;

            internal int PlasmaJimmyAttacks = 0;

            internal bool BountyHunterEntered = false;
            internal bool BountyHunterDied = true;

            public override void ManagedUpdate()
            {
                if (!BattleActive)
                    return;

                foreach (CardSlot slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
                {
                    if (!UnlockedApe && slot.Card != null && !slot.Card.OpponentCard && slot.Card.Info.name == CustomCards.NFT)
                    {
                        AchievementManager.Unlock(CONTROL_NFT);
                        UnlockedApe = true;
                    }
                }

                if (!UnlockedCurrency && (Part3SaveData.Data.currency - InitialCurrency) >= 15)
                {
                    AchievementManager.Unlock(MASSIVE_OVERKILL);
                    UnlockedCurrency = true;
                }
            }

            internal void TurnStart(bool playerTurn)
            {
                StrikesThisTurn = 0;
                PlasmaJimmyAttacks = 0;

                if (playerTurn
                    && TurnManager.Instance.opponent is ArchivistBossOpponent
                    && (TurnManager.Instance.opponent.StartingLives - TurnManager.Instance.opponent.NumLives) == 1)
                {
                    ArchivistTurns += 1;
                    if (ArchivistTurns >= 7) // Need to survive six opponent turns, which means start your seventh turn.
                        AchievementManager.Unlock(SURVIVE_SIX_ARCHIVIST);
                }

                if (playerTurn
                    && ResourcesManager.Instance.PlayerMaxEnergy >= 5
                    && TurnManager.Instance.TurnNumber == 2)
                {
                    AchievementManager.Unlock(TURBO_RAMP);
                }

                if (playerTurn && LifeManager.Instance.DamageUntilPlayerWin == 9)
                {
                    TippedScalesTurns += 1;
                    if (TippedScalesTurns == 3)
                        AchievementManager.Unlock(SCALES_TILTED_3X);
                }
            }

            internal void SlotAttackSlot(CardSlot attackingSlot, CardSlot opposingSlot)
            {
                if (attackingSlot.Card != null
                    && attackingSlot.Card.Attack > 0
                    && opposingSlot.Card == null
                    && !attackingSlot.Card.OpponentCard)
                {
                    StrikesThisTurn += 1;
                    if (StrikesThisTurn >= 6)
                        AchievementManager.Unlock(SIX_SHOOTER);
                }
            }

            internal void CleanUp()
            {
                BattleActive = false;

                if (TurnManager.Instance.opponent is PhotographerBossOpponent && !UsedCamera)
                    AchievementManager.Unlock(DONT_USE_CAMERA);

                if (BountyHunterEntered && !BountyHunterDied)
                {
                    BattlesWithoutBountyHuntersKilled += 1;
                    if (BattlesWithoutBountyHuntersKilled >= 5)
                        AchievementManager.Unlock(AVOID_BOUNTY_HUNTERS);
                }
            }

            internal void Reset()
            {
                BattleActive = true;

                UnlockedApe = ModdedAchievementManager.AchievementById(CONTROL_NFT).IsUnlocked;
                ArchivistTurns = 0;
                UsedCamera = false;
                UnlockedCurrency = ModdedAchievementManager.AchievementById(MASSIVE_OVERKILL).IsUnlocked;
                InitialCurrency = Part3SaveData.Data.currency;
                TippedScalesTurns = 0;
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPrefix]
        private static void CreateCardBattleAchievementMonitor()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (CardBattleAchievementMonitor.Instance == null)
                TurnManager.Instance.gameObject.AddComponent<CardBattleAchievementMonitor>();

            CardBattleAchievementMonitor.Instance.Reset();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.OpponentTurn))]
        [HarmonyPrefix]
        private static void TurnStart() => CardBattleAchievementMonitor.Instance?.TurnStart(false);

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
        [HarmonyPrefix]
        private static void TurnStartPlayer() => CardBattleAchievementMonitor.Instance?.TurnStart(true);

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        private static void Cleanup() => CardBattleAchievementMonitor.Instance?.CleanUp();

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static void SlotAttack(CardSlot attackingSlot, CardSlot opposingSlot) => CardBattleAchievementMonitor.Instance?.SlotAttackSlot(attackingSlot, opposingSlot);

        [HarmonyPatch(typeof(ActivatedDealDamage), nameof(ActivatedDealDamage.Activate))]
        [HarmonyPrefix]
        private static void JimmyShot()
        {
            if (CardBattleAchievementMonitor.Instance != null)
            {
                CardBattleAchievementMonitor.Instance.PlasmaJimmyAttacks += 1;
                if (CardBattleAchievementMonitor.Instance.PlasmaJimmyAttacks == 6)
                    AchievementManager.Unlock(PLASMA_JIMMY_CRAZY);
            }
        }

        [HarmonyPatch(typeof(PhotographerUI), nameof(PhotographerUI.OnSnapshotPressed))]
        [HarmonyPrefix]
        private static void UsedCamera()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (CardBattleAchievementMonitor.Instance != null)
                CardBattleAchievementMonitor.Instance.UsedCamera = true;
        }

        [HarmonyPatch(typeof(CompositeRuleTriggerHandler), nameof(CompositeRuleTriggerHandler.BreakInfiniteLoop))]
        [HarmonyPrefix]
        private static void BreakInfiniteLoop()
        {
            if (P03AscensionSaveData.IsP03Run)
                AchievementManager.Unlock(CANVAS_ENOUGH);
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.IntroductionSequence))]
        [HarmonyPrefix]
        private static void BountyHunterEntered()
        {
            if (P03AscensionSaveData.IsP03Run && CardBattleAchievementMonitor.Instance != null)
                CardBattleAchievementMonitor.Instance.BountyHunterEntered = true;
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.OnDie))]
        [HarmonyPrefix]
        private static void KillBountyHunter()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (CardBattleAchievementMonitor.Instance != null)
                CardBattleAchievementMonitor.Instance.BountyHunterDied = true;

            BountyHuntersKilled++;
            if (BountyHuntersKilled >= 30)
                AchievementManager.Unlock(KILL_30_BOUNTY_HUNTERS);
        }

        [HarmonyPatch(typeof(BuildACardScreen), nameof(BuildACardScreen.Initialize))]
        [HarmonyPrefix]
        private static void CheckingForBACAchievement(int baseStatPoints)
        {
            if (baseStatPoints >= 5 && P03AscensionSaveData.IsP03Run)
                AchievementManager.Unlock(MAX_SP_CARD);
        }

        [HarmonyPatch(typeof(ViewManager), nameof(ViewManager.SwitchToView))]
        [HarmonyPrefix]
        private static void CheckForAllUpgradedCards(View view)
        {
            if (P03AscensionSaveData.IsP03Run && view == View.MapDefault)
            {
                if (Part3SaveData.Data.deck.Cards.Where(
                    c => (c.HasAbility(Ability.PermaDeath) || c.HasAbility(NewPermaDeath.AbilityID))
                         && c.Gemified
                         && c.Abilities.Where(a => AbilitiesUtil.GetInfo(a).conduit).Count() > 0
                         && c.HasAbility(Ability.Transformer)).Count() > 0
                )
                {
                    AchievementManager.Unlock(FULLY_UPGRADED);
                }

                if (Part3SaveData.Data.deck.Cards.Where(c => c.HasAbility(Ability.PermaDeath) || c.HasAbility(NewPermaDeath.AbilityID)).Count() == 0)
                    AchievementManager.Unlock(FULLY_OVERCLOCKED);
            }
        }
    }
}