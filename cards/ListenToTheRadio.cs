using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ListenToTheRadio : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        private IEnumerator SayDialogue(string dialogueCode)
        {
            string faceCode = NPCDescriptor.GetDescriptorForNPC(DefaultQuestDefinitions.ListenToTheRadio.EventId).faceCode;
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

            if (DefaultQuestDefinitions.ListenToTheRadio.GetQuestCounter() >= DefaultQuestDefinitions.RADIO_TURNS)
                yield break;

            DefaultQuestDefinitions.ListenToTheRadio.IncrementQuestCounter();

            yield return SayDialogue($"P03RadioTower{DefaultQuestDefinitions.ListenToTheRadio.GetQuestCounter()}");
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            if (DefaultQuestDefinitions.ListenToTheRadio.GetQuestCounter() >= DefaultQuestDefinitions.RADIO_TURNS)
                yield break;

            yield return SayDialogue("P03RadioTowerOnBoard");
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (DefaultQuestDefinitions.ListenToTheRadio.GetQuestCounter() >= DefaultQuestDefinitions.RADIO_TURNS)
                yield break;

            yield return SayDialogue("P03RadioTowerDied");

            if (DefaultQuestDefinitions.ListenToTheRadio.GetQuestCounter() > 0)
                DefaultQuestDefinitions.ListenToTheRadio.IncrementQuestCounter(-1);
        }

        static ListenToTheRadio()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "ListenToTheRadio", typeof(ListenToTheRadio)).Id;
        }
    }
}