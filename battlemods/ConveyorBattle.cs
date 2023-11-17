using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class ConveyorBattle : NonCardTriggerReceiver, IBattleModSetup, IBattleModCleanup, IBattleModSimulator
    {
        public static BattleModManager.ID ID { get; private set; }

        static ConveyorBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Conveyor",
                new List<string>() { "It appears someone activated the [c:bR]conveyor belt[c:]", "Cards will rotate clockwise every turn" },
                typeof(ConveyorBattle),
                difficulty: 1,
                new() { CardTemple.Tech },
                iconPath: "p03kcm/prefabs/arrow-repeat"
            );
            BattleModManager.SetGlobalActivationRule(ID,
                () => AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.ALL_CONVEYOR.challengeType)
                      || DefaultQuestDefinitions.Conveyors.IsDefaultActive());
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => TurnManager.Instance.TurnNumber > 1 && playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (TurnManager.Instance.TurnNumber > 1 && playerUpkeep)
            {
                if (TurnManager.Instance.TurnNumber == 2 && AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.ALL_CONVEYOR.challengeType))
                    ChallengeActivationUI.Instance.ShowActivation(AscensionChallengeManagement.ALL_CONVEYOR.challengeType);

                yield return BoardManager.Instance.MoveAllCardsClockwise();
            }
        }

        public static readonly Texture2D UP_CONVEYOR_SLOT = TextureHelper.GetImageAsTexture("cadslot_up.png", typeof(AscensionChallengeManagement).Assembly);

        private static readonly List<Texture> PLAYER_CONVEYOR_SLOTS = new()
        {
            UP_CONVEYOR_SLOT,
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            Resources.Load<Texture2D>("art/cards/card_slot_left")
        };

        private static readonly List<Texture> OPPONENT_CONVEYOR_SLOTS = new()
        {
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            Resources.Load<Texture2D>("art/cards/card_slot_left"),
            UP_CONVEYOR_SLOT
        };

        public IEnumerator OnBattleModSetup()
        {
            SlotModificationManager.OverrideDefaultSlotTexture(CardTemple.Tech, PLAYER_CONVEYOR_SLOTS, OPPONENT_CONVEYOR_SLOTS);
            BoardManager.Instance.AllSlotsCopy.ForEach(s => s.ResetSlot());
            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Board);
            yield break;
        }

        public IEnumerator OnBattleModCleanup()
        {
            SlotModificationManager.ResetDefaultSlotTexture(CardTemple.Tech);
            BoardManager.Instance.AllSlotsCopy.ForEach(s => s.ResetSlot());
            yield break;
        }

        public void DoBoardStateAdjustment(BoardState board, bool playerIsAttacker)
        {
            // We need to rotate the board
            BoardState.CardState anchorCard = board.playerSlots[0].card;
            for (int i = 1; i < board.playerSlots.Count; i++)
                board.playerSlots[i - 1].card = board.playerSlots[i].card;
            board.playerSlots[board.playerSlots.Count - 1].card = board.opponentSlots[board.opponentSlots.Count - 1].card;
            for (int i = board.opponentSlots.Count - 1; i > 0; i--)
                board.opponentSlots[i].card = board.opponentSlots[i - 1].card;
            board.opponentSlots[0].card = anchorCard;
        }

        public bool HasBoardStateAdjustment(BoardState board, bool playerIsAttacker) => true;
        public bool HasCardEvaluationAdjustment(BoardState.CardState card, BoardState board) => true;
        public int DoCardEvaluationAdjustment(BoardState.CardState card, BoardState board)
        {
            if (board.opponentSlots.Contains(card.slot))
            {
                if (card.info.mods.Any(m => m.bountyHunterInfo != null))
                {
                    int bestSlot = card.HasAbility(Ability.SplitStrike) ? 1 : 0;
                    return -Math.Abs(board.opponentSlots.IndexOf(card.slot) - bestSlot);
                }
            }
            return 0;
        }
    }
}