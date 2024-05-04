using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03KayceeRun.Quests
{
    [HarmonyPatch]
    public static class QuestManager
    {
        private static readonly Dictionary<SpecialEvent, QuestDefinition> AllQuests = new();

        public static IEnumerable<QuestDefinition> AllQuestDefinitions = AllQuests.Values;

        internal static int CalculateQuestSize(SpecialEvent eventId)
        {
            QuestDefinition nextQuest = AllQuests.Values.FirstOrDefault(q => q.PriorEventId == eventId);
            return nextQuest == null ? 1 : 1 + CalculateQuestSize(nextQuest.EventId);
        }

        /// <summary>
        /// Gets the definition for a given quest based on its unique ID
        /// </summary>
        /// <param name="eventId">The event ID for the quest</param>
        public static QuestDefinition Get(SpecialEvent eventId) => AllQuests[eventId];

        /// <summary>
        /// Creates a new blank quest and adds it to the quest pool
        /// </summary>
        /// <param name="modGuid"></param>
        /// <param name="questName"></param>
        /// <returns></returns>
        public static QuestDefinition Add(string modGuid, string questName)
        {
            SpecialEvent eventId = GuidManager.GetEnumValue<SpecialEvent>(modGuid, questName);
            if (AllQuests.ContainsKey(eventId))
                throw new InvalidOperationException(String.Format("A quest with the name {0} in mod {1} was defined twice!", questName, modGuid));

            QuestDefinition defn = new(modGuid, questName);
            AllQuests.Add(eventId, defn);
            return defn;
        }

        internal static List<Tuple<SpecialEvent, Predicate<HoloMapBlueprint>>> GetSpecialEventForZone(RunBasedHoloMap.Zone zone)
        {
            List<Tuple<SpecialEvent, Predicate<HoloMapBlueprint>>> events = new();

            // Need to generate all the must adds first. This is because sometimes simply generating
            // a quest randomly is enough to move it to an active state. And then the "must be generated" flag
            // is no longer accurate.
            //
            // This could also be handled by setting the generated flag inside of the map generator instead of setting it
            // here, but this is just easier. Always generate the MUST GENERATE quests first, then then RANDOM GENERATE
            // quests next to make sure you don't double generate.
            foreach (QuestDefinition quest in AllQuests.Values.Where(q => q.MustBeGenerated))
            {
                quest.QuestGenerated = true;
                events.Add(new(quest.EventId, quest.GenerateRoomFilter()));
            }

            // Randomized special events
            List<SpecialEvent> possibles = AllQuests.Values.Where(q => q.ValidForRandomSelection).Select(q => q.EventId).ToList();

            if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("event"))
            {
                try
                {
                    int idx = P03Plugin.Instance.DebugCode.ToLowerInvariant().IndexOf("event[");
                    int eidx = P03Plugin.Instance.DebugCode.ToUpperInvariant().IndexOf("]");
                    string substr = P03Plugin.Instance.DebugCode.Substring(idx + 6, eidx - idx - 6);
                    P03Plugin.Log.LogWarning($"Parsing override debug event! {substr}");
                    string[] guidParts = substr.Split('_');
                    SpecialEvent dEvent = GuidManager.GetEnumValue<SpecialEvent>(guidParts[0], guidParts[1]);

                    QuestDefinition selected = Get(dEvent);
                    selected.QuestGenerated = true;
                    events.Add(new(dEvent, selected.GenerateRoomFilter())); // randomly selected events should appear in the first color
                    possibles.Clear();
                }
                catch (Exception ex)
                {
                    P03Plugin.Log.LogWarning($"Could not parse special event from debug string! {ex}");
                }
            }

            if (possibles.Count > 0)
            {
                int maxPriority = possibles.Max(p => Get(p).CalculatedPriority());
                possibles.RemoveAll(se => Get(se).CalculatedPriority() != maxPriority);
                SpecialEvent randomEvent = possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed)];
                QuestDefinition selected = Get(randomEvent);
                selected.QuestGenerated = true;
                events.Add(new(randomEvent, selected.GenerateRoomFilter()));
            }

            // Now we check all the must be generateds AGAIN - just in case we triggered any partner quests
            foreach (QuestDefinition quest in AllQuests.Values.Where(q => q.MustBeGenerated))
            {
                // Make sure we don't generate twice of course
                if (!events.Any(e => e.Item1 == quest.EventId))
                {
                    quest.QuestGenerated = true;
                    events.Add(new(quest.EventId, quest.GenerateRoomFilter()));
                }
            }

            return events;
        }
    }
}