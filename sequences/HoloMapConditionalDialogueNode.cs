using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class HoloMapConditionalDialogueNode : HoloMapDialogueNode
    {

        [HarmonyPatch(typeof(HoloFloatingLabel), nameof(HoloFloatingLabel.ManagedUpdate))]
        [HarmonyPrefix]
        private static bool DontIfLabelIsNull(HoloFloatingLabel __instance)
        {
            return __instance.line != null;
        }

        public override void OnCursorSelectEnd()
        {
            this.SetHoveringEffectsShown(false);
            this.OnSelected();
            base.StartCoroutine(this.DialogueThenStorySequence());
        }

        public override void OnCursorEnter()
        {
            label.gameObject.SetActive(true);
            QuestDefinition quest = QuestManager.Get(this.eventId);
            this.label.SetText(Localization.Translate(quest.CurrentState.NPCHoverText));
            base.OnCursorEnter();
        }

        public override void OnCursorExit()
        {
            label.gameObject.SetActive(false);
            base.OnCursorExit();
        }

        private IEnumerator DialogueThenStorySequence()
        {
            // Go ahead and get a reference to the quest
            QuestDefinition quest = QuestManager.Get(this.eventId);

            MapNodeManager.Instance.SetAllNodesInteractable(false);
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = true;
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            NPCDescriptor npc = NPCDescriptor.GetDescriptorForNPC(this.eventId);
            P03ModularNPCFace.Instance.SetNPCFace(npc.faceCode);

            yield return HoloGameMap.Instance.FlickerHoloElements(false, 1);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return new WaitForSeconds(0.1f);
            P03AnimationController.Instance.SwitchToFace(this.face, true, true);
            yield return new WaitForSeconds(0.1f);

            // Need to play the dialogue associated with the current state of the quest
            yield return TextDisplayer.Instance.PlayDialogueEvent(quest.CurrentState.DialogueId, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);

            // Now we advance the quest if necessary (if this is an autocomplete quest state)
            if (quest.CurrentState.AutoComplete)
                quest.CurrentState.Status = QuestState.QuestStateStatus.Success;

            // Now we play all quest rewards that haven't yet been granted
            ViewManager.Instance.SwitchToView(View.Default);
            yield return quest.GrantAllUngrantedRewards();

            if (quest.IsCompleted && quest.CurrentState.Status == QuestState.QuestStateStatus.Success)
                AscensionStatsData.TryIncrementStat(StatManagement.QUESTS_COMPLETED);

            // Reset back to normal game state
            ViewManager.Instance.SwitchToView(View.MapDefault, false, false);
            yield return new WaitForSeconds(0.15f);
            HoloGameMap.Instance.StartCoroutine(HoloGameMap.Instance.FlickerHoloElements(true, 2));
            MapNodeManager.Instance.SetAllNodesInteractable(true);
            (GameFlowManager.Instance as Part3GameFlowManager).DisableTransitionToFirstPerson = false;
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            P03Plugin.Log.LogDebug($"After dialogue, the current state of the quest is {quest.CurrentState.StateName} with a status of {quest.CurrentState.Status}. Is the quest completed? {quest.IsCompleted}");

            if (quest.IsCompleted)
            {
                this.SetCompleted();
                this.npc?.SetActive(false);

                // Check to see if we should unlock the "every quest" achievement
                // This happens if you've completed four full quests and you're in the final zone
                if (EventManagement.CompletedZones.Count == 3)
                {
                    if (QuestManager.AllQuestDefinitions
                                    .Where(q => !q.IsSpecialQuest
                                           && q.IsCompleted
                                           && q.CurrentState.Status == QuestState.QuestStateStatus.Success
                                           && q.IsEndOfQuest)
                                    .Count() == 4)
                    {
                        AchievementManager.Unlock(P03AchievementManagement.ALL_QUESTS_COMPLETED);
                    }
                }
            }
            else
                this.SetHidden(false, false);

            yield break;
        }

        [SerializeField]
        public SpecialEvent eventId;

        [SerializeField]
        public HoloFloatingLabel label;
    }
}