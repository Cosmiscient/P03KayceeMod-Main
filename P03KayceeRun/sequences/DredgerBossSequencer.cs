using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class DredgerBattleSequencer : BossBattleSequencer
    {
        public override Opponent.Type BossType => BossManagement.DredgerOpponent;
        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_DREDGER;

        public override bool RespondsToUpkeep(bool playerUpkeep) => !playerUpkeep && TurnManager.Instance.TurnNumber % 3 == 0;

        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            return new()
            {
                opponentType = BossManagement.DredgerOpponent,
                opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(EncounterHelper.DredgerBattle, 1, false)
            };
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            // Ideal: replace an urchin opposing a player card
            List<CardSlot> possibles = BoardManager.Instance.OpponentSlotsCopy.Where(s => s.Card != null && s.Card.name == "P03KCMXP2_UrchinCell" && s.opposingSlot.Card != null).ToList();
            if (possibles.Count == 0)
                // Second best: replace an empty slot opposing a player card
                possibles = BoardManager.Instance.OpponentSlotsCopy.Where(s => s.Card == null && s.opposingSlot.Card != null).ToList();

            if (possibles.Count == 0)
                // Third best: fill an empty slot
                possibles = BoardManager.Instance.OpponentSlotsCopy.Where(s => s.Card == null).ToList();

            if (possibles.Count == 0) // Guess we can't
                yield break;

            CardSlot targetSlot = possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed)];
            ViewManager.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.2f);

            if (targetSlot.Card != null)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("DredgerScrapDropUrchin", TextDisplayer.MessageAdvanceMode.Input);
                targetSlot.Card.ExitBoard(0.4f, Vector3.zero);
                yield return new WaitForSeconds(0.4f);
            }
            else
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("DredgerScrapDrop", TextDisplayer.MessageAdvanceMode.Input);
            }
            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName(CustomCards.PILE_OF_SCRAP), targetSlot);
        }
    }
}