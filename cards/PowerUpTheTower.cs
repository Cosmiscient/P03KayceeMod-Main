using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class PowerUpTheTower : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        private static QuestDefinition Quest => DefaultQuestDefinitions.PowerUpTheTower;

        private bool Active
        {
            get => PlayableCard.OnBoard &&
                       ConduitCircuitManager.Instance != null &&
                       ConduitCircuitManager.Instance.SlotIsWithinCircuit(PlayableCard.Slot);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (!playerUpkeep)
                yield break;

            if (Quest.GetQuestCounter() >= DefaultQuestDefinitions.POWER_TURNS)
                yield break;

            if (Active)
            {
                Quest.IncrementQuestCounter();
                yield return NPCDescriptor.SayDialogue(Quest.EventId, $"P03PowerTower{Quest.GetQuestCounter()}");
            }
            else
            {
                yield return NPCDescriptor.SayDialogue(Quest.EventId, $"P03PowerTowerNeedsCircuit");
            }
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (Quest.GetQuestCounter() >= DefaultQuestDefinitions.POWER_TURNS)
                yield break;

            yield return NPCDescriptor.SayDialogue(Quest.EventId, "P03PowerTowerDied");

            if (Quest.GetQuestCounter() > 0)
                Quest.IncrementQuestCounter(-1);
        }

        static PowerUpTheTower()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "PowerUpTheTower", typeof(PowerUpTheTower)).Id;
        }
    }
}