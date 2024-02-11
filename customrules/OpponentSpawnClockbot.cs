using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.CustomRules
{
    public class OpponentSpawnClockbot : Effect
    {
        static OpponentSpawnClockbot()
        {
            CompositeBattleRule.AVAILABLE_EFFECTS.Add(new OpponentSpawnClockbot());
        }

        public override string Description => Localization.Translate("I play a Mr:Clock");

        public override Color Color => GameColors.Instance.darkGold;

        public override bool CanExecute() => BoardManager.Instance.OpponentSlotsCopy.Exists((CardSlot x) => x.Card == null);

        public override IEnumerator Execute(PlayableCard triggeringCard)
        {
            List<CardSlot> validSlots = Singleton<BoardManager>.Instance.OpponentSlotsCopy.FindAll((CardSlot x) => x.Card == null);
            if (validSlots.Count > 0)
            {
                Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.1f);
                CardSlot slot = validSlots[SeededRandom.Range(0, validSlots.Count, P03AscensionSaveData.RandomSeed)];
                yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName($"{ExpansionPackCards_1.EXP_1_PREFIX}_Clockbot"), slot, 0.15f, true);
                yield return new WaitForSeconds(0.25f);
            }
            yield break;
        }
    }
}
