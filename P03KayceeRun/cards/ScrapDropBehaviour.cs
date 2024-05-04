using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ScrapDropBehaviour : SpecialCardBehaviour
    {
        public static readonly SpecialTriggeredAbility ID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "DropScrapBehaviour", typeof(ScrapDropBehaviour)).Id;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
        {
            return !wasSacrifice && base.PlayableCard.OnBoard;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return new WaitForSeconds(0.3f);

            var possibles = ScriptableObjectLoader<CardInfo>.AllData.FindAll(TradeChipsSequencer.IsValidDraftCard);
            possibles.RemoveAll(ci => ci.energyCost < 2 || ci.energyCost > 4);
            var targetCard = possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed)];
            yield return BoardManager.Instance.CreateCardInSlot(targetCard, base.PlayableCard.Slot, 0.15f, true);
            yield break;
        }
    }
}