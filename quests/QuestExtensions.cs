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
    public static class QuestExtensions
    {
        /// <summary>
        /// Sets the prior stage of this quest, if this is a follow-up to a multi-part quest
        /// </summary>
        /// <param name="prior">The prior event</param>
        public static QuestDefinition SetPriorQuest(this QuestDefinition parent, SpecialEvent prior)
        {
            parent.PriorEventId = prior;
            return parent;
        }

        /// <summary>
        /// Sets the prior stage of this quest, if this is a follow-up to a multi-part quest
        /// </summary>
        /// <param name="prior">The prior event</param>
        public static QuestDefinition SetPriorQuest(this QuestDefinition parent, QuestDefinition prior)
        {
            return parent.SetPriorQuest(prior.EventId);
        }

        /// <summary>
        /// Sets the NPC Descriptor override
        /// </summary>
        public static QuestDefinition OverrideNPCDescriptor(this QuestDefinition parent, NPCDescriptor npc)
        {
            parent.ForcedNPCDescriptor = npc;
            return parent;
        }

        /// <summary>
        /// Sets a condition (restriction) for the quest to be valid for generation.
        /// </summary>
        public static QuestDefinition SetGenerateCondition(this QuestDefinition parent, Func<bool> condition)
        {
            parent.GenerateCondition = condition;
            return parent;
        }

        /// <summary>
        /// Restricts a quest to only be generated in a specific zone
        /// </summary>
        /// <param name="parent"></param>
        public static QuestDefinition SetRegionCondition(this QuestDefinition parent, RunBasedHoloMap.Zone region)
        {
            return parent.SetGenerateCondition(() => {
                return EventManagement.CurrentZone == region || EventManagement.CompletedZones.Any(z => z.ToLowerInvariant().EndsWith(region.ToString().ToLowerInvariant()));
            });
        }

        public static QuestState AddDummyStartingState(this QuestDefinition parent, Func<QuestState.QuestStateStatus> status)
        {
            return parent.AddDialogueState("DUMMY", "DUMMY").SetDynamicStatus(status);
        }

        public static QuestState AddDummyStartingState(this QuestDefinition parent, Func<bool> status)
        {
            return parent.AddDialogueState("DUMMY", "DUMMY").SetDynamicStatus(() => status() ? QuestState.QuestStateStatus.Success : QuestState.QuestStateStatus.Failure);
        }

        /// <summary>
        /// Adds an opening dialogue state to a quest. Most quests will start this way.
        /// </summary>
        /// <param name="hoverText">The text that shows over the NPC's head when you hover over them</param>
        /// <param name="dialogueId">The dialogue that the NPC will say when you first meet them</param>
        /// <returns></returns>
        public static QuestState AddDialogueState(this QuestDefinition parent, string hoverText, string dialogueId)
        {
            QuestState state = new (parent, "InitialDialogue", hoverText, dialogueId, autoComplete:true);
            parent.InitialState = state;
            return state;
        }

        /// <summary>
        /// Adds an opening state to a quest.
        /// </summary>
        /// <param name="hoverText">The text that shows over the NPC's head when you hover over them</param>
        /// <param name="dialogueId">The dialogue that the NPC will say when you first meet them</param>
        /// <returns></returns>
        public static QuestState AddState(this QuestDefinition parent, string hoverText, string dialogueId)
        {
            QuestState state = new (parent, "InitialState", hoverText, dialogueId);
            parent.InitialState = state;
            return state;
        }

        /// <summary>
        /// Sets a dynamic status function on a quest state
        /// </summary>
        /// <param name="status">The function calculating the status of the state</param>
        /// <returns></returns>
        public static QuestState SetDynamicStatus(this QuestState state, Func<QuestState.QuestStateStatus> status)
        {
            state.DynamicStatus = status;
            return state;
        }

        /// <summary>
        /// Sets the quest state to monitor story event flags to track its progress
        /// </summary>
        /// <param name="successState">The story event that, if set, will cause this quest state to be successful</param>
        /// <param name="failState">The story event that, if set, will cause this quest state to be failed</param>
        /// <remarks>The fail state has priority over the success state</remarks>
        public static QuestState SetStoryEventStatus(this QuestState state, StoryEvent successState, StoryEvent? failState)
        {
            if (failState.HasValue)
            {
                StoryEvent actualFailState = failState.Value;
                return state.SetDynamicStatus(() => {
                    if (StoryEventsData.EventCompleted(actualFailState))
                        return QuestState.QuestStateStatus.Failure;
                    if (StoryEventsData.EventCompleted(successState))
                        return QuestState.QuestStateStatus.Success;
                    return QuestState.QuestStateStatus.Active;
                });
            }
            else
            {
                return state.SetDynamicStatus(() => {
                    if (StoryEventsData.EventCompleted(successState))
                        return QuestState.QuestStateStatus.Success;
                    return QuestState.QuestStateStatus.Active;
                });
            }
        }

        /// <summary>
        /// Sets the autocomplete flag on the quest state
        /// </summary>
        public static QuestState SetAutoComplete(this QuestState state, bool autoComplete = true)
        {
            state.AutoComplete = autoComplete;
            return state;
        }

        /// <summary>
        /// Sets some arbitrary code (as an Action) to execute when this state ends successfully
        /// </summary>
        public static QuestState AddSuccessAction(this QuestState state, Action action)
        {
            state.Rewards.Add(new QuestRewardAction() { RewardAction = action });
            return state;
        }

        /// <summary>
        /// Adds a passthrough dialogue state to a quest state.
        /// </summary>
        /// <param name="hoverText">The text that shows over the NPC's head when you hover over them</param>
        /// <param name="dialogueId">The dialogue that the NPC will say</param>
        public static QuestState AddDialogueState(this QuestState parent, string hoverText, string dialogueId, QuestState.QuestStateStatus status = QuestState.QuestStateStatus.Success)
        {
            QuestState state = new (parent.ParentQuest, String.Format("Dialogue_{0}", dialogueId), hoverText, dialogueId, autoComplete:true);
            parent.SetNextState(status, state);
            return state;
        }

        /// <summary>
        /// Adds a passthrough dialogue state to a quest state.
        /// </summary>
        /// <param name="hoverText">The text that shows over the NPC's head when you hover over them</param>
        /// <param name="dialogueId">The dialogue that the NPC will say</param>
        /// <param name="status">The status of the parent state that will trigger this state. Usually this is "success"</param>
        public static QuestState AddNamedState(this QuestState parent, string name, string hoverText, string dialogueId, QuestState.QuestStateStatus status = QuestState.QuestStateStatus.Success)
        {
            QuestState state = new (parent.ParentQuest, name, hoverText, dialogueId, autoComplete:false);
            parent.SetNextState(status, state);
            return state;
        }

        /// <summary>
        /// Adds a "default active" state. Most quests will fall into a default "active" state and wait until the player does something to advance. This creates one of those.
        /// </summary>
        /// <param name="hoverText">The text that shows over the NPC's head when you hover over them</param>
        /// <param name="dialogueId">The dialogue that the NPC will say</param>
        /// <param name="threshold">A convenience variable. If you want to use the dummy quest counter to track something
        /// alongside this quest and automatically complete this state when that counter reaches a threshold, set the threshold to
        /// a non-zero value.</param>
        public static QuestState AddDefaultActiveState(this QuestState parent, string hoverText, string dialogueId, int threshold = 0)
        {
            QuestState retval = parent.AddNamedState("DEFAULTACTIVESTATE", hoverText, dialogueId);
            if (threshold > 0)
            {
                string saveKey = String.Format("{0}_Counter", parent.ParentQuest.QuestName);
                string modGuid = parent.ParentQuest.ModGuid;
                retval.DynamicStatus = () => 
                {
                    if (ModdedSaveManager.RunState.GetValueAsInt(modGuid, saveKey) >= threshold)
                        return QuestState.QuestStateStatus.Success;
                    else
                        return QuestState.QuestStateStatus.Active;
                };
            }
            return retval;
        }

        /// <summary>
        /// This convenience method increments a dummy count variable that is tracked alongside every quest.
        /// </summary>
        /// <remarks>This is a convenience function. Many quests functionally just track how many times *something* happens
        /// over the course of the run. This function creates a dummy variable associated with the quests and increments it
        /// by one. There is a related helper function to create a quest state that automatically complets itself successfully
        /// whenever this quest counter reaches a certain threshold, which is the primary use case for this helper</remarks>
        public static void IncrementQuestCounter(this QuestDefinition defn, int incrementBy = 1)
        {
            string saveKey = String.Format("{0}_Counter", defn.QuestName);
            int curValue = ModdedSaveManager.RunState.GetValueAsInt(defn.ModGuid, saveKey);
            ModdedSaveManager.RunState.SetValue(defn.ModGuid, saveKey, curValue + incrementBy);
        }

        /// <summary>
        /// This convenience method gets the current value of the dummy count variable tracked alongside the quest
        /// </summary>
        public static int GetQuestCounter(this QuestDefinition defn)
        {
            return ModdedSaveManager.RunState.GetValueAsInt(defn.ModGuid, String.Format("{0}_Counter", defn.QuestName));   
        }

        /// <summary>
        /// Helper to determine if this particular quest state is the "default" active state when the quest is active.
        /// </summary>
        /// <returns>True if the state represents the default active state of the quest</returns>
        public static bool IsDefaultState(this QuestState state)
        {
            return state.StateName == "DEFAULTACTIVESTATE";
        }

        /// <summary>
        /// Helper to determine if the active state of the question is the "default" active state.
        /// </summary>
        public static bool IsDefaultActive(this QuestDefinition defn)
        {
            return defn.CurrentState.IsDefaultState();
        }

        public static QuestState AddMonetaryReward(this QuestState state, int amount)
        {
            state.Rewards.Add(new QuestRewardCoins() { Amount = amount });
            return state;
        }

        public static QuestState AddDynamicMonetaryReward(this QuestState state)
        {
            state.Rewards.Add(new QuestRewardDynamicCoins());
            return state;
        }

        public static QuestState AddGainCardReward(this QuestState state, string cardName)
        {
            state.Rewards.Add(new QuestRewardCard() { CardName  = cardName });
            return state;
        }

        public static QuestState AddGainItemReward(this QuestState state, string itemName)
        {
            state.Rewards.Add(new QuestRewardItem() { ItemName = itemName});
            return state;
        }

        public static QuestState AddLoseCardReward(this QuestState state, string cardName)
        {
            state.Rewards.Add(new QuestRewardLoseCard { CardName  = cardName });
            return state;
        }

        public static QuestState AddLoseItemReward(this QuestState state, string itemName)
        {
            state.Rewards.Add(new QuestRewardLoseItem() { ItemName = itemName});
            return state;
        }

        public static QuestState AddGemifyCardsReward(this QuestState state, int count)
        {
            state.Rewards.Add(new QuestRewardModifyRandomCards() { NumberOfCards = count, Gemify = true });
            return state;
        }

        public static QuestState AddGainAbilitiesReward(this QuestState state, int count, Ability ability)
        {
            state.Rewards.Add(new QuestRewardModifyRandomCards() { NumberOfCards = count, Ability = ability });
            return state;
        }
    }
}