using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static Dictionary<CardTemple, string> SalmonNames = new()
        {
            { CardTemple.Tech, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon" },
            { CardTemple.Undead, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Undead" },
            { CardTemple.Wizard, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Wizard" },
            { CardTemple.Nature, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Nature" }
        };

        public override string Description => Localization.Translate("a random card becomes a fish");

        public override Color Color => GameColors.Instance.purple;

        // Token: 0x06002313 RID: 8979 RVA: 0x000703EB File Offset: 0x0006E5EB
        public override bool CanExecute() => BoardManager.Instance.AllSlots.Exists((CardSlot x) => x.Card != null && !x.Card.Dead && !SalmonNames.Values.Contains(x.Card.Info.name));

        public override IEnumerator Execute(PlayableCard triggeringCard)
        {
            List<CardSlot> validSlots = Singleton<BoardManager>.Instance.AllSlots.FindAll((CardSlot x) => x.Card != null && !x.Card.Dead && !SalmonNames.Values.Contains(x.Card.Info.name));
            if (validSlots.Count > 0)
            {
                Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.1f);
                CardSlot cardSlot = validSlots[Random.Range(0, validSlots.Count)];
                CardInfo salmon = CardLoader.GetCardByName(SalmonNames[cardSlot.Card.Info.temple]);
                yield return cardSlot.Card.TransformIntoCard(salmon);
                yield return new WaitForSeconds(0.5f);
            }
            yield break;
        }
    }
}