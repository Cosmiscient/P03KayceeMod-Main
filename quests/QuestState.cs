using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;
using InscryptionAPI.Guid;
using Infiniscryption.P03KayceeRun.Sequences;
using System.Collections;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Cards;

namespace Infiniscryption.P03KayceeRun.Quests
{
    /// <summary>
    /// Defines a discrete state that a quest can be in.
    /// </summary>
    [HarmonyPatch]
    public class QuestState
    {
        public enum QuestStateStatus
        {
            NotStarted = 0,
            Active = 1,
            Success = 2,
            Failure = 3
        }

        /// <summary>
        /// A story event flag for completion (success or failure) of this quest state
        /// </summary>
        public StoryEvent StateCompleteEvent { get; private set; }

        /// <summary>
        /// A story event flag for a successful completion of this quest state
        /// </summary>
        public StoryEvent StateSuccessfulEvent { get; private set; }

        /// <summary>
        /// A story event flag for a failed completion of this quest state
        /// </summary>
        public StoryEvent StateFailedEvent { get; private set; }

        /// <summary>
        /// The name of this state
        /// </summary>
        public string StateName { get; private set; }

        /// <summary>
        /// The dialogue that will be spoken when interacting with the NPC that gives this quest when this quest state is active
        /// </summary>
        public string DialogueId { get; private set; }

        /// <summary>
        /// The text that will appear over the NPC's head when you hover over them in this state
        /// </summary>
        public string NPCHoverText { get; set; }

        /// <summary>
        /// The parent quest for this quest state
        /// </summary>
        public QuestDefinition ParentQuest { get; private set; }

        private string SaveKey => String.Format("{0}_{1}", this.ParentQuest.QuestName, this.StateName);

        /// <summary>
        /// Optional. A dynamic state to be calculated at lookup time. 
        /// </summary>
        /// <remarks>If a state status has a dyanmic state, setting its status will do nothing!</remarks>
        public Func<QuestStateStatus> DynamicStatus { get; set; }

        /// <summary>
        /// Indicates if this quest state is completed
        /// </summary>
        public QuestStateStatus Status
        {
            get 
            { 
                // Status cannot be anothing other that not started if the quest has not been generated
                if (!this.ParentQuest.QuestGenerated)
                    return QuestStateStatus.NotStarted;

                // Start by getting the status from the save file
                QuestStateStatus status = (QuestStateStatus)ModdedSaveManager.RunState.GetValueAsInt(this.ParentQuest.ModGuid, $"{this.SaveKey}_Status");
                if (status == QuestStateStatus.NotStarted && this != this.ParentQuest.InitialState) // Only the initial state can be not started
                    status = QuestStateStatus.Active;

                // If the set status is fail or success, or there is no dynamic status, we're done
                if (status == QuestStateStatus.Success || status == QuestStateStatus.Failure || this.DynamicStatus == null)
                    return status;
                else
                    return this.DynamicStatus();
            }
            set { ModdedSaveManager.RunState.SetValue(this.ParentQuest.ModGuid, $"{this.SaveKey}_Status", (int)value); }
        }

        /// <summary>
        /// /// Indicates if this quest state has already given out its rewards or not. This is a safety check.
        /// </summary>
        public bool HasGivenRewwards
        {
            get { return ModdedSaveManager.RunState.GetValueAsBoolean(this.ParentQuest.ModGuid, String.Format("{0}_RewardStatus", this.SaveKey)); }
            set { ModdedSaveManager.RunState.SetValue(this.ParentQuest.ModGuid, String.Format("{0}_RewardStatus", this.SaveKey), value); }
        }

        /// <summary>
        /// This list of rewards granted by this quest
        /// </summary>
        public List<QuestReward> Rewards { get; private set; } = new ();

        /// <summary>
        /// Indicates if this event should automatically complete successfully upon talking to an NPC.
        /// </summary>
        /// <remarks>This is meant specifically for dialogue states, where talking to the NPC moves you to the next
        /// dialogue state automatically.</remarks>
        public bool AutoComplete { get; internal set; }

        private Dictionary<QuestStateStatus, QuestState> childStates = new();

        /// <summary>
        /// Indicates if this is the final state to a quest
        /// </summary>
        public bool IsEndState
        {
            get
            {
                return this.childStates.Count == 0;
            }
        }

        /// <summary>
        /// Indicates if this is a failure state of the quest.
        /// </summary>
        /// <remarks>Note that this state could have a status of "Success" but still be a fail state.
        /// A QuestSate is a fail state if is exists in any branch of the quest that you reach by failing
        /// any state. So this state could have a status of success and still be a fail state because it was 
        /// reached by failing a previous state of the quest.</remarks>
        public bool IsFailState { get; private set; }

        /// <summary>
        /// Defines what state comes next based on how this state concludes. Overrides any previous definition.
        /// </summary>
        /// <param name="statusCondition">The status trigger for the next state</param>
        /// <param name="nextState">The next state</param>
        public QuestState SetNextState(QuestStateStatus statusCondition, QuestState nextState)
        {
            if (this.IsFailState || statusCondition == QuestStateStatus.Failure)
                nextState.IsFailState = true;

            if (this.childStates.ContainsKey(statusCondition))
                this.childStates[statusCondition] = nextState;
            else
                this.childStates.Add(statusCondition, nextState);
            return nextState;
        }

        /// <summary>
        /// Defines what state comes next based on how this state concludes. Overrides any previous definition.
        /// </summary>
        /// <param name="statusCondition">The status trigger for the next state</param>
        public QuestState SetNextState(QuestStateStatus statusCondition, string hoverText, string dialogueId, bool autoComplete = false)
        {
            QuestState nextState = new(this.ParentQuest, String.Format("{0}_{1}", this.StateName, statusCondition.ToString()), hoverText, dialogueId, autoComplete:autoComplete);
            return SetNextState(statusCondition, nextState);
        }

        /// <summary>
        /// Gets the next state in the state chain based on a given condition
        /// </summary>
        /// <param name="statusCondition">The status condition</param>
        public QuestState GetNextState(QuestStateStatus statusCondition)
        {
            if (this.childStates.ContainsKey(statusCondition))
                return this.childStates[statusCondition];
            else
                return null;
        }

        /// <summary>
        /// Gets the next state based on the current state's condition. Useful for traveling the state tree quickly.
        /// </summary>
        public QuestState GetNextState()
        {
            return this.GetNextState(this.Status);
        }

        /// <summary>
        /// Grants all rewards associated with this quest state. Only happens if the quest is in a success state and rewards have not been given before.
        /// </summary>
        public IEnumerator GrantRewards()
        {
            // The moment we are asked to grant rewards, we lock our state to where it can't change again.
            // This makes it where the dyamic status can't change anymore; if we are in a failure or success status at this
            // point, we're always going to be in that status moving forward.
            this.Status = this.Status;

            if (this.Status != QuestStateStatus.Success || this.HasGivenRewwards)
                yield break;

            this.HasGivenRewwards = true;

            foreach (QuestReward reward in this.Rewards)
                yield return reward.GrantReward();
            
            yield break;
        }

        private static Dictionary<StoryEvent, QuestState> storyEventReverseLookup = new ();

        internal QuestState(QuestDefinition parentQuest, string stateName, string hoverText, string dialogueId, bool autoComplete = false)
        {
            this.StateName = stateName;
            this.ParentQuest = parentQuest;
            this.NPCHoverText = hoverText;
            this.AutoComplete = autoComplete;
            this.DialogueId = dialogueId;

            this.StateCompleteEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Complete", parentQuest.QuestName, this.StateName));
            this.StateSuccessfulEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Success", parentQuest.QuestName, this.StateName));
            this.StateFailedEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Failure", parentQuest.QuestName, this.StateName));

            storyEventReverseLookup[this.StateCompleteEvent] = this;
            storyEventReverseLookup[this.StateSuccessfulEvent] = this;
            storyEventReverseLookup[this.StateFailedEvent] = this;
        }

        [HarmonyPatch(typeof(StoryEventsData), "EventCompleted")]
        [HarmonyPrefix]
        internal static bool QuestBasedStoryFlags(ref bool __result, StoryEvent storyEvent)
        {
            if (!storyEventReverseLookup.ContainsKey(storyEvent))
                return true;
            
            QuestState matchingState = storyEventReverseLookup[storyEvent];
            
            if (matchingState.Status == QuestStateStatus.NotStarted || matchingState.Status == QuestStateStatus.Active)
                __result = false;
            else if (storyEvent == matchingState.StateCompleteEvent)
                __result = true;
            else if (storyEvent == matchingState.StateFailedEvent)
                __result = matchingState.Status == QuestStateStatus.Failure;
            else if (storyEvent == matchingState.StateSuccessfulEvent)
                __result = matchingState.Status == QuestStateStatus.Success;
            
            return false;
        }
    }
}