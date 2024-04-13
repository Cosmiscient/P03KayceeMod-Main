using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class FilePaperworkInOrder : SpecialCardBehaviour
    {
        public const string TEMPORARY_MOD_ID = "FILE_PAPERWORK_MOD";
        public static readonly SpecialTriggeredAbility ID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "FilePaperworkInOrder", typeof(FilePaperworkInOrder)).Id;
        internal static readonly List<string> ALL_PAPERWORK = new() { CustomCards.PAPERWORK_A, CustomCards.PAPERWORK_B, CustomCards.PAPERWORK_C };

        public List<CardSlot> GetPaperwork() => BoardManager.Instance.AllSlotsCopy.Where(s => s.Card != null && ALL_PAPERWORK.Contains(s.Card.Info.name)).ToList();

        public bool IsAllPaperworkFiledInOrder(List<CardSlot> paperwork)
        {
            // Need three cards, where the names are in order
            if (paperwork.Count != 3)
                return false;
            for (int i = 0; i < 3; i++)
            {
                if (paperwork[i].Card.OpponentCard)
                    return false;
                if (paperwork[i].Card.Info.name != ALL_PAPERWORK[i])
                    return false;
            }
            return true;
        }

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;

        private void AddStampAbility(PlayableCard card)
        {
            var mod = card.temporaryMods.FirstOrDefault(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(TEMPORARY_MOD_ID));
            if (mod == null)
                card.AddTemporaryMod(new(FilePaperworkStamp.AbilityID) { singletonId = TEMPORARY_MOD_ID });
        }

        private void RemoveStampAbility(PlayableCard card)
        {
            var mod = card.temporaryMods.FirstOrDefault(m => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(TEMPORARY_MOD_ID));
            if (mod != null)
                card.RemoveTemporaryMod(mod);
        }

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            var paperwork = GetPaperwork();
            if (IsAllPaperworkFiledInOrder(paperwork))
            {
                var stampedPaperwork = FilePaperworkStamp.StampedPaperwork;
                foreach (var slot in paperwork)
                    if (!stampedPaperwork.Contains(slot.Card.Info.name))
                        AddStampAbility(slot.Card);
            }
            else
            {
                foreach (var slot in paperwork)
                    RemoveStampAbility(slot.Card);
            }
            yield break;
        }
    }
}