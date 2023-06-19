using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class PowerUpTheTower : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        private bool Active
        {
            get
            {
                return this.PlayableCard.OnBoard &&
                       ConduitCircuitManager.Instance != null &&
                       ConduitCircuitManager.Instance.SlotIsWithinCircuit(this.PlayableCard.Slot);
            }
        }

        private IEnumerator SayDialogue(string dialogueCode)
        {
            string faceCode = NPCDescriptor.GetDescriptorForNPC(DefaultQuestDefinitions.PowerUpTheTower.EventId).faceCode;
            P03ModularNPCFace.Instance.SetNPCFace(faceCode);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return new WaitForSeconds(0.1f);
            P03AnimationController.Instance.SwitchToFace(P03ModularNPCFace.ModularNPCFace, true, true);
            yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueCode, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.15f);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (!playerUpkeep)
                yield break;

            if (DefaultQuestDefinitions.PowerUpTheTower.GetQuestCounter() >= DefaultQuestDefinitions.POWER_TURNS)
                yield break;

            if (Active)
            {
                DefaultQuestDefinitions.PowerUpTheTower.IncrementQuestCounter();
                yield return SayDialogue($"P03PowerTower{DefaultQuestDefinitions.PowerUpTheTower.GetQuestCounter()}");
            }
            else
            {
                yield return SayDialogue($"P03PowerTowerNeedsCircuit");
            }
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (DefaultQuestDefinitions.PowerUpTheTower.GetQuestCounter() >= DefaultQuestDefinitions.POWER_TURNS)
                yield break;

            yield return SayDialogue("P03PowerTowerDied");

            if (DefaultQuestDefinitions.PowerUpTheTower.GetQuestCounter() > 0)
                DefaultQuestDefinitions.PowerUpTheTower.IncrementQuestCounter(-1);
        }

        static PowerUpTheTower()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "PowerUpTheTower", typeof(PowerUpTheTower)).Id;
        }
    }
}