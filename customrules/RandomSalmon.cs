using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using Infiniscryption.P03KayceeRun.Cards;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.CustomRules
{
    public class RandomSalmon : Effect
    {
        static RandomSalmon()
        {
            CompositeBattleRule.AVAILABLE_EFFECTS.Add(new RandomSalmon());
        }

        public readonly string SALMON_NAME = $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon";
        public override string Description => Localization.Translate("a random card becomes a fish");

        public override Color Color => GameColors.Instance.purple;

        // Token: 0x06002313 RID: 8979 RVA: 0x000703EB File Offset: 0x0006E5EB
        public override bool CanExecute() => BoardManager.Instance.AllSlots.Exists((CardSlot x) => x.Card != null && !x.Card.Dead && !x.Card.Info.name.Equals(SALMON_NAME));

        public override IEnumerator Execute(PlayableCard triggeringCard)
        {
            List<CardSlot> validSlots = Singleton<BoardManager>.Instance.AllSlots.FindAll((CardSlot x) => x.Card != null && !x.Card.Dead && !x.Card.Info.name.Equals(SALMON_NAME));
            if (validSlots.Count > 0)
            {
                Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.1f);
                CardSlot cardSlot = validSlots[Random.Range(0, validSlots.Count)];
                CardInfo salmon = CardLoader.GetCardByName(SALMON_NAME);
                yield return cardSlot.Card.TransformIntoCard(salmon);
                yield return new WaitForSeconds(0.5f);
            }
            yield break;
        }
    }
}