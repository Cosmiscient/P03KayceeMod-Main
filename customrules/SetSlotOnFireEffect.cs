using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.CustomRules
{
    public class SetSlotOnFire : Effect
    {
        static SetSlotOnFire()
        {
            CompositeBattleRule.AVAILABLE_EFFECTS.Add(new SetSlotOnFire());
        }

        public const int NUMBER_OF_SLOTS = 1;
        public override string Description => Localization.Translate($"{NUMBER_OF_SLOTS} of your lanes catches fire");

        public override Color Color => GameColors.Instance.darkRed;

        public override bool CanExecute() => true;
        public override IEnumerator Execute(PlayableCard triggeringCard)
        {
            List<CardSlot> playerSlots = BoardManager.Instance.GetSlotsCopy(true);

            // Get all slots of yours that are not on fire first
            int randomSeed = P03AscensionSaveData.RandomSeed + (10 * TurnManager.Instance.TurnNumber);
            List<CardSlot> notOnFireSlots = playerSlots.Where(s => !FireBomb.SlotIsOnFire(s)).OrderBy(s => SeededRandom.Value(randomSeed++) * 100).ToList();
            List<CardSlot> onFireSlots = playerSlots.Where(s => FireBomb.SlotIsOnFire(s)).OrderBy(s => SeededRandom.Value(randomSeed++) * 100).ToList();

            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.1f);

            int burned = 0;
            while (burned < Mathf.Min(NUMBER_OF_SLOTS, playerSlots.Count))
            {
                if (notOnFireSlots.Count > 0)
                {
                    yield return FireBomb.SetSlotOnFireBasic(1, notOnFireSlots[0], null);
                    notOnFireSlots.RemoveAt(0);
                }
                else
                {
                    yield return FireBomb.SetSlotOnFireBasic(1, onFireSlots[0], null);
                    onFireSlots.RemoveAt(0);
                }
                burned += 1;
            }
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }
    }
}