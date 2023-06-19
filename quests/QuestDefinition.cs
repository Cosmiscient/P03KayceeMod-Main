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
using Infiniscryption.P03KayceeRun.Patchers;

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
        /// The starting state for the quest
        /// </summary>
        public QuestState InitialState { get; set; }

        /// <summary>
        /// Any additional conditions about whether or not this particular quest is allowed to be generated.
        /// </summary>
        public Func<bool> GenerateCondition { get; set; }

        /// <summary>
        /// Indicates if the quest has been generated
        /// </summary>
        public bool QuestGenerated
        {
            get { return ModdedSaveManager.RunState.GetValueAsBoolean(this.ModGuid, $"{this.QuestName}_GENERATED"); }
            set { ModdedSaveManager.RunState.SetValue(this.ModGuid, $"{this.QuestName}_GENERATED", value); }
        }

        /// <summary>
        /// Any special conditions about which rooms this particular quest giver can live in
        /// </summary>
        public Predicate<HoloMapBlueprint> ValidRoomCondition { get; set; }

        internal Predicate<HoloMapBlueprint> GenerateRoomFilter()
        {
            // Special rules for special quests
            if (this.EventId == DefaultQuestDefinitions.FindGoobert.EventId)
            {
                if (EventManagement.CompletedZones.Count == 0)
                    return (HoloMapBlueprint bp) => bp.isSecretRoom;
                else
                    return (HoloMapBlueprint bp) => !bp.isSecretRoom && bp.color != 1;
            }

            if (this.EventId == DefaultQuestDefinitions.BrokenGenerator.EventId)
                return (HoloMapBlueprint bp) => bp.isSecretRoom;

            if (this.EventId == DefaultQuestDefinitions.Prospector.EventId)
                return (HoloMapBlueprint bp) => !bp.isSecretRoom && bp.color != 1;

            // Use special conditions if necessary
            if (this.ValidRoomCondition != null)
            {
                Predicate<HoloMapBlueprint> special = this.ValidRoomCondition;
                return (HoloMapBlueprint bp) => !bp.isSecretRoom && special(bp);
            }

            // If this is a generic quest
            if (this.PriorEventId != SpecialEvent.None || this.CurrentState.Status != QuestState.QuestStateStatus.NotStarted)
                return (HoloMapBlueprint bp) => !bp.isSecretRoom && bp.color != 1;
            else
                return (HoloMapBlueprint bp) => !bp.isSecretRoom && bp.color == 1;
        }

        /// <summary>
        /// Indicates if this is one of the special quests that are part of the special secrets
        /// </summary>
        public bool IsSpecialQuest
        {
            get
            {
                return this.EventId == DefaultQuestDefinitions.FindGoobert.EventId ||
                    this.EventId == DefaultQuestDefinitions.BrokenGenerator.EventId ||
                    this.EventId == DefaultQuestDefinitions.Prospector.EventId;
            }
        }

        /// <summary>
        /// Indicates if this quest is allowed to be selected as the random quest on the current map
        /// </summary>
        public bool ValidForRandomSelection
        {
            get
            {
                // Special quests can't be here
                if (this.IsSpecialQuest)
                    return false;

                // Is this quest completed? If so, we can't generate it again
                QuestState current = this.CurrentState;
                if (current.Status == QuestState.QuestStateStatus.Success || current.Status == QuestState.QuestStateStatus.Failure)
                    return false;

                // Does this quest have a prior? If so, no. You can't randomly select this
                if (this.PriorEventId != SpecialEvent.None)
                    return false;

                // Calculate my quest size. There has to be enough time to finish this quest
                if ((4 - EventManagement.CompletedZones.Count) < QuestManager.CalculateQuestSize(this.EventId))
                    return false;

                // If there's a special condition on this quest, figure that out now
                if (this.GenerateCondition != null)
                    return this.GenerateCondition();
                else
                    return true;
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
                if (this.EventId == DefaultQuestDefinitions.FindGoobert.EventId)
                {
                    if (EventManagement.CompletedZones.Count == 0)
                        return true; // Always generated on map one
                    if (EventManagement.CompletedZones.Count == 1)
                        return DefaultQuestDefinitions.FindGoobert.CurrentState.Status == QuestState.QuestStateStatus.Active;
                    return false;
                }

                if (this.EventId == DefaultQuestDefinitions.BrokenGenerator.EventId)
                    return EventManagement.CompletedZones.Count == 1; // Always generated on map 2

                if (this.EventId == DefaultQuestDefinitions.Prospector.EventId)
                    return Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.BRAIN); // Always generated if you have a bounty hunter brain in your deck

                // Does this quest have a prior? Is that prior successfully complete? If so, this must
                // be generated so the quest can continue
                if (this.PriorEventId != SpecialEvent.None)
                {
                    QuestDefinition priorQuest = QuestManager.Get(this.PriorEventId);
                    if (priorQuest.IsCompleted && priorQuest.CurrentState.Status == QuestState.QuestStateStatus.Success)
                        return true;
                }

                // Is this quest currently active? That means it wasn't finished and needs to be generated again
                return this.CurrentState.Status == QuestState.QuestStateStatus.Active;
            }
        }

        /// <summary>
        /// Gets a reference to the current state of the quest
        /// </summary>
        public QuestState CurrentState
        {
            get
            {
                QuestState currentState = this.InitialState;
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
                QuestState currentState = this.CurrentState;
                if (!currentState.IsEndState)
                    return false;
                return currentState.Status == QuestState.QuestStateStatus.Success || currentState.Status == QuestState.QuestStateStatus.Failure;
            }
        }

        /// <summary>
        /// Forces a quest to run all the way through its states, ending in the given state. Useful for forcing a quest to complete.
        /// </summary>
        public void ForceComplete(QuestState.QuestStateStatus status)
        {
            // If this status is "success," we just run forward until we run out
            if (status == QuestState.QuestStateStatus.Success)
            {
                while (this.CurrentState.Status != status)
                    this.CurrentState.Status = status;
                
                return;
            } 
            else 
            {
                // Some quests force you through a 'success' state
                // before getting to a fail state.
                while (this.CurrentState.Status != status)
                {
                    // If there are NO next states at all
                    if (this.CurrentState.IsEndState)
                    {
                        this.CurrentState.Status = QuestState.QuestStateStatus.Failure;
                        return;
                    }
                    else if (this.CurrentState.GetNextState(QuestState.QuestStateStatus.Failure) != null)
                        this.CurrentState.Status = QuestState.QuestStateStatus.Failure;
                    else
                        this.CurrentState.Status = QuestState.QuestStateStatus.Success;            
                }
            }
        }

        /// <summary>
        /// Runs all necessary logic to advance the quest 
        /// </summary>
        public IEnumerator GrantAllUngrantedRewards()
        {
            QuestState currentState = this.InitialState;
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
            this.ModGuid = modGuid;
            this.QuestName = questName;
            this.EventId = GuidManager.GetEnumValue<SpecialEvent>(modGuid, questName);
        }
    }
}