using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ReplicatingFirewallBehavior : SpecialCardBehaviour
    {
        public const string NUMBER_OF_ADDITIONAL_COPIES = "ReplicatingFirewallBehavior.NumberOfCopies";

        public static SpecialTriggeredAbility AbilityID => SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "ReplicatingFirewallBehavior", typeof(ReplicatingFirewallBehavior)).Id;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        private CardSlot FindEmptySlot(CardSlot startingSlot)
        {
            List<CardSlot> slots = BoardManager.Instance
                                               .GetSlotsCopy(PlayableCard.IsPlayerCard())
                                               .Where(s => s.Card == null)
                                               .OrderBy(s => Mathf.Abs(startingSlot.Index - s.Index))
                                               .ToList();

            if (slots.Count > 0)
                return slots[0];

            return null;
        }

        private CardInfo GetReplicant(int targetHealth)
        {
            CardInfo replicant = CardLoader.GetCardByName(PlayableCard.Info.name);
            CardModificationInfo mod = new();
            mod.healthAdjustment = targetHealth - replicant.Health;
            replicant.mods.Add(mod);
            return replicant;
        }

        private int GetNumCopies()
        {
            try
            {
                return Card.Info.GetExtendedPropertyAsInt(NUMBER_OF_ADDITIONAL_COPIES).GetValueOrDefault(1);
            }
            catch
            {
                return 1;
            }
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return new WaitForSeconds(0.3f);
            if (this.PlayableCard == null)
                yield break;
            CardSlot startingSlot = PlayableCard.Slot;
            if (PlayableCard.Info.iceCubeParams != null && PlayableCard.Info.iceCubeParams.creatureWithin != null)
            {
                name = PlayableCard.Info.iceCubeParams.creatureWithin.name;
                yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName(name), PlayableCard.Slot, 0.15f);

                int copies = GetNumCopies();

                for (int i = 0; i < copies; i++)
                {
                    CardSlot emptySlot = FindEmptySlot(startingSlot);
                    if (emptySlot != null)
                        yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName(name), emptySlot, 0.15f);
                }
            }
            else
            {
                int numAdditionalCards = 1;
                int targetHealth = Mathf.FloorToInt(Card.Info.Health / 2f);
                if (targetHealth == 0)
                    yield break;

                if (Card.Info.Health % 3 == 0)
                {
                    numAdditionalCards = 2;
                    targetHealth = Mathf.FloorToInt(Card.Info.Health / 3f);
                }

                yield return BoardManager.Instance.CreateCardInSlot(GetReplicant(targetHealth), PlayableCard.Slot, 0.15f);

                for (int i = 0; i < numAdditionalCards; i++)
                {
                    CardSlot emptySlot = FindEmptySlot(startingSlot);
                    if (emptySlot != null)
                        yield return BoardManager.Instance.CreateCardInSlot(GetReplicant(targetHealth), emptySlot, 0.15f);
                }
            }
            yield break;
        }
    }
}