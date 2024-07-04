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
            { CardTemple.Tech, "P03SIG_Salmon" },
            { CardTemple.Undead, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Undead" },
            { CardTemple.Wizard, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Wizard" },
            { CardTemple.Nature, $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Nature" }
        };

        public override string Description => Localization.Translate("a random card becomes a fish");

        public override Color Color => GameColors.Instance.purple;

        private string GetSalmonName(CardTemple temple)
        {
            if (temple == CardTemple.Tech && TurnManager.Instance.opponent is TelegrapherBossOpponent)
                return $"{ExpansionPackCards_1.EXP_1_PREFIX}_Salmon_Golly";
            return SalmonNames[temple];
        }

        public override bool CanExecute() => BoardManager.Instance.AllSlots.Exists((CardSlot x) => x.Card != null && !x.Card.Dead && !SalmonNames.Values.Contains(x.Card.Info.name));

        private bool GollyHasReacted = false;

        public override IEnumerator Execute(PlayableCard triggeringCard)
        {
            List<CardSlot> validSlots = Singleton<BoardManager>.Instance.AllSlots.FindAll((CardSlot x) => x.Card != null && !x.Card.Dead && !SalmonNames.Values.Contains(x.Card.Info.name));
            if (validSlots.Count > 0)
            {
                Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.1f);
                CardSlot cardSlot = validSlots[Random.Range(0, validSlots.Count)];
                CardInfo salmon = CardLoader.GetCardByName(GetSalmonName(cardSlot.Card.Info.temple));
                cardSlot.Card.ExitBoard(0.25f, -Vector3.up * 3f - Vector3.right * 2f + Vector3.down);
                yield return BoardManager.Instance.CreateCardInSlot(salmon, cardSlot, resolveTriggers: false);

                if (TurnManager.Instance.opponent is TelegrapherBossOpponent && !GollyHasReacted)
                {
                    GollyHasReacted = true;
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03SalmonTelegrapher", TextDisplayer.MessageAdvanceMode.Input);
                }

                // yield return cardSlot.Card.TransformIntoCard(salmon, preTransformCallback: delegate ()
                // {
                //     cardSlot.Card.RenderInfo.prefabPortrait = null;
                //     cardSlot.Card.RenderInfo.portraitOverride = null;

                //     List<CardModificationInfo> tempMods = new(cardSlot.Card.TemporaryMods);
                //     foreach (var mod in tempMods)
                //     {
                //         cardSlot.Card.RemoveTemporaryMod(mod);
                //     }
                // });
                ResourcesManager.Instance.ForceGemsUpdate();
                yield return new WaitForSeconds(0.5f);
            }
            yield break;
        }
    }
}