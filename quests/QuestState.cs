using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;

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
        /// A function to get the dialogue ID in a dynamic fashion
        /// </summary>
        public Func<string> DynamicDialogueId { get; private set; }

        /// <summary>
        /// The text that will appear over the NPC's head when you hover over them in this state
        /// </summary>
        public string NPCHoverText { get; set; }

        /// <summary>
        /// The parent quest for this quest state
        /// </summary>
        public QuestDefinition ParentQuest { get; private set; }

        private string SaveKey => String.Format("{0}_{1}", ParentQuest.QuestName, StateName);

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
                if (!ParentQuest.QuestGenerated)
                    return QuestStateStatus.NotStarted;

                // Start by getting the status from the save file
                QuestStateStatus status = (QuestStateStatus)P03AscensionSaveData.RunStateData.GetValueAsInt(ParentQuest.ModGuid, $"{SaveKey}_Status");
                if (status == QuestStateStatus.NotStarted && this != ParentQuest.InitialState) // Only the initial state can be not started
                    status = QuestStateStatus.Active;

                // If the set status is fail or success, or there is no dynamic status, we're done
                return status == QuestStateStatus.Success || status == QuestStateStatus.Failure || DynamicStatus == null
                    ? status
                    : DynamicStatus();
            }
            set => P03AscensionSaveData.RunStateData.SetValue(ParentQuest.ModGuid, $"{SaveKey}_Status", (int)value);
        }

        /// <summary>
        /// /// Indicates if this quest state has already given out its rewards or not. This is a safety check.
        /// </summary>
        public bool HasGivenRewwards
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(ParentQuest.ModGuid, String.Format("{0}_RewardStatus", SaveKey));
            set => P03AscensionSaveData.RunStateData.SetValue(ParentQuest.ModGuid, String.Format("{0}_RewardStatus", SaveKey), value);
        }

        /// <summary>
        /// This list of rewards granted by this quest
        /// </summary>
        public List<QuestReward> Rewards { get; private set; } = new();

        /// <summary>
        /// Indicates if this event should automatically complete successfully upon talking to an NPC.
        /// </summary>
        /// <remarks>This is meant specifically for dialogue states, where talking to the NPC moves you to the next
        /// dialogue state automatically.</remarks>
        public bool AutoComplete { get; internal set; }

        private readonly Dictionary<QuestStateStatus, QuestState> childStates = new();

        /// <summary>
        /// Indicates if this is the final state to a quest
        /// </summary>
        public bool IsEndState => childStates.Count == 0;

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
            if (IsFailState || statusCondition == QuestStateStatus.Failure)
                nextState.IsFailState = true;

            if (childStates.ContainsKey(statusCondition))
                childStates[statusCondition] = nextState;
            else
                childStates.Add(statusCondition, nextState);
            return nextState;
        }

        /// <summary>
        /// Defines what state comes next based on how this state concludes. Overrides any previous definition.
        /// </summary>
        /// <param name="statusCondition">The status trigger for the next state</param>
        public QuestState SetNextState(QuestStateStatus statusCondition, string hoverText, string dialogueId, bool autoComplete = false)
        {
            QuestState nextState = new(ParentQuest, String.Format("{0}_{1}", StateName, statusCondition.ToString()), hoverText, dialogueId, autoComplete: autoComplete);
            return SetNextState(statusCondition, nextState);
        }

        /// <summary>
        /// Gets the next state in the state chain based on a given condition
        /// </summary>
        /// <param name="statusCondition">The status condition</param>
        public QuestState GetNextState(QuestStateStatus statusCondition) => childStates.ContainsKey(statusCondition) ? childStates[statusCondition] : null;

        /// <summary>
        /// Gets the next state based on the current state's condition. Useful for traveling the state tree quickly.
        /// </summary>
        public QuestState GetNextState() => GetNextState(Status);

        /// <summary>
        /// Grants all rewards associated with this quest state. Only happens if the quest is in a success state and rewards have not been given before.
        /// </summary>
        public IEnumerator GrantRewards()
        {
            // The moment we are asked to grant rewards, we lock our state to where it can't change again.
            // This makes it where the dyamic status can't change anymore; if we are in a failure or success status at this
            // point, we're always going to be in that status moving forward.
            Status = Status;

            if (Status != QuestStateStatus.Success || HasGivenRewwards)
                yield break;

            HasGivenRewwards = true;

            foreach (QuestReward reward in Rewards)
                yield return reward.GrantReward();

            yield break;
        }

        private static readonly Dictionary<StoryEvent, QuestState> storyEventReverseLookup = new();

        internal QuestState(QuestDefinition parentQuest, string stateName, string hoverText, string dialogueId, bool autoComplete = false)
        {
            StateName = stateName;
            ParentQuest = parentQuest;
            NPCHoverText = hoverText;
            AutoComplete = autoComplete;
            DialogueId = dialogueId;

            StateCompleteEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Complete", parentQuest.QuestName, StateName));
            StateSuccessfulEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Success", parentQuest.QuestName, StateName));
            StateFailedEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Failure", parentQuest.QuestName, StateName));

            storyEventReverseLookup[StateCompleteEvent] = this;
            storyEventReverseLookup[StateSuccessfulEvent] = this;
            storyEventReverseLookup[StateFailedEvent] = this;
        }

        internal QuestState(QuestDefinition parentQuest, string stateName, string hoverText, Func<string> dialogueId, bool autoComplete = false)
        {
            StateName = stateName;
            ParentQuest = parentQuest;
            NPCHoverText = hoverText;
            AutoComplete = autoComplete;
            DialogueId = null;
            DynamicDialogueId = dialogueId;

            StateCompleteEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Complete", parentQuest.QuestName, StateName));
            StateSuccessfulEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Success", parentQuest.QuestName, StateName));
            StateFailedEvent = GuidManager.GetEnumValue<StoryEvent>(parentQuest.ModGuid, String.Format("{0}_{1}_Failure", parentQuest.QuestName, StateName));

            storyEventReverseLookup[StateCompleteEvent] = this;
            storyEventReverseLookup[StateSuccessfulEvent] = this;
            storyEventReverseLookup[StateFailedEvent] = this;
        }

        [HarmonyPatch(typeof(StoryEventsData), "EventCompleted")]
        [HarmonyPrefix]
        internal static bool QuestBasedStoryFlags(ref bool __result, StoryEvent storyEvent)
        {
            if (!storyEventReverseLookup.ContainsKey(storyEvent))
                return true;

            QuestState matchingState = storyEventReverseLookup[storyEvent];

            if (matchingState.Status is QuestStateStatus.NotStarted or QuestStateStatus.Active)
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