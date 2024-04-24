using System;
using System.Collections;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Quests
{
    public class QuestDefinition
    {
        /// <summary>
        /// The prefix of the mod that owns this quest
        /// </summary>
        public string ModGuid { get; private set; }

        /// <summary>
        /// The unique identification name of this quest
        /// </summary>
        public string QuestName { get; private set; }

        /// <summary>
        /// The ID for this quest
        /// </summary>
        public SpecialEvent EventId { get; private set; }

        /// <summary>
        /// Optional. If set, this will force this quest to always have this specific NPC
        /// </summary>
        public NPCDescriptor ForcedNPCDescriptor { get; set; }

        /// <summary>
        /// The prior event Id. If set, this will cause this quest to function as "part 2" of the prior quest.
        /// </summary>
        /// <value></value>
        public SpecialEvent PriorEventId { get; set; } = SpecialEvent.None;

        /// <summary>
        /// The partner event. If set, this will cause this quest to function as a "sibling" of the partner quest.
        /// It will only get generated if the partner gets generated, and it will get generated on the same map.
        /// </summary>
        public SpecialEvent PartnerOfEventId { get; set; } = SpecialEvent.None;

        /// <summary>
        /// The starting state for the quest
        /// </summary>
        public QuestState InitialState { get; set; }

        /// <summary>
        /// Any additional conditions about whether or not this particular quest is allowed to be generated.
        /// </summary>
        public Func<bool> GenerateCondition { get; set; }

        /// <summary>
        /// If true, the quest can appear in a secret room.
        /// To force it in a secret room, set this true and then use GenerateCondition to force it in a secret room.
        /// </summary>
        public bool AllowSecretRoom { get; set; } = false;

        /// <summary>
        /// If true, the quest can appear in a landmark room.
        /// </summary>
        public bool AllowLandmarks { get; set; } = false;

        /// <summary>
        /// If true, the quest can continue across map changes
        /// </summary>
        public bool QuestCanContinue { get; set; } = true;

        /// <summary>
        /// Calculates the priority of this quest
        /// </summary>
        public Func<int> CalculatedPriority { get; set; } = () => 1;

        /// <summary>
        /// Any additional conditions to force the quest to be generated
        /// </summary>
        public Func<bool> MustBeGeneratedConditon { get; set; }

        /// <summary>
        /// Any code to run on the NPC after it's generated
        /// </summary>
        public Action<GameObject> PostGenerateAction { get; set; }

        /// <summary>
        /// Indicates if the quest has been generated
        /// </summary>
        public bool QuestGenerated
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(ModGuid, $"{QuestName}_GENERATED");
            internal set => P03AscensionSaveData.RunStateData.SetValue(ModGuid, $"{QuestName}_GENERATED", value);
        }

        /// <summary>
        /// Any special conditions about which rooms this particular quest giver can live in
        /// </summary>
        public Predicate<HoloMapBlueprint> ValidRoomCondition { get; set; }

        internal Predicate<HoloMapBlueprint> GenerateRoomFilter()
        {
            int startingQuad = RunBasedHoloMap.GetStartingQuadrant(EventManagement.CurrentZone);
            // Special rules for special quests
            if (EventId == DefaultQuestDefinitions.FindGoobert.EventId)
            {
                return EventManagement.CompletedZones.Count == 0
                    ? ((HoloMapBlueprint bp) => bp.isSecretRoom)
                    : ((HoloMapBlueprint bp) => !bp.isSecretRoom && bp.color != startingQuad);
            }

            if (EventId == DefaultQuestDefinitions.BrokenGenerator.EventId)
                return (HoloMapBlueprint bp) => bp.isSecretRoom;

            if (EventId == DefaultQuestDefinitions.Prospector.EventId)
                return (HoloMapBlueprint bp) => !bp.isSecretRoom && bp.color != startingQuad && (bp.specialTerrain & HoloMapBlueprint.LANDMARKER) == 0;

            Predicate<HoloMapBlueprint> landmark = AllowLandmarks ? (bp) => true : (bp) => (bp.specialTerrain & HoloMapBlueprint.LANDMARKER) == 0;
            Predicate<HoloMapBlueprint> secret = AllowSecretRoom ? (bp) => true : (bp) => !bp.isSecretRoom;

            // Use special conditions if necessary
            if (ValidRoomCondition != null)
            {
                Predicate<HoloMapBlueprint> special = ValidRoomCondition;
                return (HoloMapBlueprint bp) => landmark(bp) && secret(bp) && special(bp);
            }

            // If this is a generic quest
            return PriorEventId != SpecialEvent.None || CurrentState.Status != QuestState.QuestStateStatus.NotStarted
                ? ((HoloMapBlueprint bp) => secret(bp) && landmark(bp) && bp.color != startingQuad)
                : ((HoloMapBlueprint bp) => secret(bp) && landmark(bp) && bp.color == startingQuad);
        }

        /// <summary>
        /// Indicates if this is one of the special quests that are part of the special secrets
        /// </summary>
        public bool IsSpecialQuest
        {
            get => EventId == DefaultQuestDefinitions.FindGoobert.EventId ||
                    EventId == DefaultQuestDefinitions.BrokenGenerator.EventId ||
                    EventId == DefaultQuestDefinitions.Prospector.EventId ||
                    EventId == DefaultQuestDefinitions.Rebecha.EventId;
        }

        /// <summary>
        /// Indicates if this is the end of a quest line (i.e., if there are no other quests that depend on it)
        /// </summary>
        public bool IsEndOfQuest => QuestManager.AllQuestDefinitions.Count(q => q.PriorEventId == EventId) == 0;

        /// <summary>
        /// Indicates if this quest is allowed to be selected as the random quest on the current map
        /// </summary>
        public bool ValidForRandomSelection
        {
            get
            {
                // Special quests can't be here
                if (IsSpecialQuest)
                    return false;

                // Is this quest completed? If so, we can't generate it again
                if (QuestGenerated)
                    return false;

                // Does this quest have a prior? If so, no. You can't randomly select this
                if (PriorEventId != SpecialEvent.None)
                    return false;

                // Does this quest have a partner? If so, no. You can't randomly select this
                if (PartnerOfEventId != SpecialEvent.None)
                    return false;

                // Calculate my quest size. There has to be enough time to finish this quest
                if ((4 - EventManagement.CompletedZones.Count) < QuestManager.CalculateQuestSize(EventId))
                    return false;

                // If there's a special condition on this quest, figure that out now
                return GenerateCondition == null || GenerateCondition();
            }
        }

        /// <summary>
        /// Indicates if this quest MUST be generated on the current map
        /// </summary>
        public bool MustBeGenerated
        {
            get
            {
                // There are special rules for the hardcoded story quests
                if (EventId == DefaultQuestDefinitions.FindGoobert.EventId)
                {
                    if (EventManagement.CompletedZones.Count == 0)
                        return true; // Always generated on map one

                    return EventManagement.CompletedZones.Count == 1
                            && DefaultQuestDefinitions.FindGoobert.CurrentState.Status == QuestState.QuestStateStatus.Active
                            && EventManagement.CurrentZone == DefaultQuestDefinitions.GoobertDropoffZone;
                }

                if (EventId == DefaultQuestDefinitions.BrokenGenerator.EventId)
                    return EventManagement.CompletedZones.Count == 1; // Always generated on map 2

                if (EventId == DefaultQuestDefinitions.Prospector.EventId)
                    return Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.BRAIN); // Always generated if you have a bounty hunter brain in your deck

                if (EventId == DefaultQuestDefinitions.Rebecha.EventId)
                    return false;

                // If this is completed but there are still rewards to give?
                if (IsCompleted && HasUngrantedRewards && QuestCanContinue)
                    return true;

                // Oh duh, this cannot be in a "must be generated" state if it has been completed
                if (IsCompleted)
                    return false;

                // Does this quest have a prior? Is that prior successfully complete? If so, this must
                // be generated so the quest can continue
                if (PriorEventId != SpecialEvent.None)
                {
                    QuestDefinition priorQuest = QuestManager.Get(PriorEventId);
                    if (priorQuest.IsCompleted && priorQuest.CurrentState.Status == QuestState.QuestStateStatus.Success)
                        return true;
                }

                if (PartnerOfEventId != SpecialEvent.None)
                {
                    QuestDefinition partnerQuest = QuestManager.Get(PartnerOfEventId);
                    if (partnerQuest.QuestGenerated && !partnerQuest.IsCompleted)
                        return true;
                }


                // Does this have a must be generated condition?
                if (MustBeGeneratedConditon != null)
                    return MustBeGeneratedConditon();

                // Is this quest currently active? That means it wasn't finished and needs to be generated again
                return CurrentState.Status == QuestState.QuestStateStatus.Active && QuestCanContinue;
            }
        }

        /// <summary>
        /// Gets a reference to the current state of the quest
        /// </summary>
        public QuestState CurrentState
        {
            get
            {
                QuestState currentState = InitialState;
                while (currentState.GetNextState() != null)
                    currentState = currentState.GetNextState();
                return currentState;
            }
        }

        /// <summary>
        /// Indicates whether or not the quest is completed
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                QuestState currentState = CurrentState;
                return currentState.IsEndState
&& currentState.Status is QuestState.QuestStateStatus.Success or QuestState.QuestStateStatus.Failure;
            }
        }

        /// <summary>
        /// Indicates whether or not the quest has ungranted rewards
        /// </summary>
        public bool HasUngrantedRewards => CurrentState.HasUngrantedRewards;

        /// <summary>
        /// Forces a quest to run all the way through its states, ending in the given state. Useful for forcing a quest to complete.
        /// </summary>
        public void ForceComplete(QuestState.QuestStateStatus status)
        {
            // If this status is "success," we just run forward until we run out
            if (status == QuestState.QuestStateStatus.Success)
            {
                while (CurrentState.Status != status)
                    CurrentState.Status = status;

                return;
            }
            else
            {
                // Some quests force you through a 'success' state
                // before getting to a fail state.
                while (CurrentState.Status != status)
                {
                    // If there are NO next states at all
                    if (CurrentState.IsEndState)
                    {
                        CurrentState.Status = QuestState.QuestStateStatus.Failure;
                        return;
                    }
                    else
                    {
                        CurrentState.Status = CurrentState.GetNextState(QuestState.QuestStateStatus.Failure) != null
                        ? QuestState.QuestStateStatus.Failure
                        : QuestState.QuestStateStatus.Success;
                    }
                }
            }
        }

        /// <summary>
        /// Runs all necessary logic to advance the quest 
        /// </summary>
        public IEnumerator GrantAllUngrantedRewards()
        {
            QuestState currentState = InitialState;
            yield return currentState.GrantRewards(); // note that this only happens if it needs to

            while (currentState.GetNextState() != null)
            {
                currentState = currentState.GetNextState();
                yield return currentState.GrantRewards(); // note that this only happens if it needs to
            }
            yield break;
        }

        internal QuestDefinition(string modGuid, string questName)
        {
            ModGuid = modGuid;
            QuestName = questName;
            EventId = GuidManager.GetEnumValue<SpecialEvent>(modGuid, questName);
        }
    }
}