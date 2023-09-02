using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Guid;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class SlotModificationManager
    {
        public enum ModificationType
        {
            NoModification = 0
        }

        public class Info
        {
            public Texture2D Texture { get; internal set; }
            public ModificationType ModificationType { get; internal set; }
            public Type SlotBehaviour { get; internal set; }
        }

        internal static List<Info> AllSlotModifications = new() {
            new () {
                Texture = null,
                ModificationType = ModificationType.NoModification,
                SlotBehaviour = null
            }
        };

        private static readonly Dictionary<ModificationType, List<CardSlot>> AllModifiedSlots = new()
        {
            { ModificationType.NoModification, new() }
        };

        private static readonly Dictionary<CardSlot, ModificationType> SlotModification = new();

        public static ModificationType New(string modGuid, string modificationName, Type behaviour, Texture2D slotTexture)
        {
            if (!behaviour.IsSubclassOf(typeof(NonCardTriggerReceiver)))
                throw new InvalidOperationException("The slot behavior must be a subclass of NonCardTriggerReceiver");

            ModificationType mType = GuidManager.GetEnumValue<ModificationType>(modGuid, modificationName);
            AllModifiedSlots[mType] = new();
            AllSlotModifications.Add(new()
            {
                Texture = slotTexture,
                SlotBehaviour = behaviour,
                ModificationType = mType
            });
            return mType;
        }

        public static ModificationType New(string modGuid, string modificationName, Type behaviour)
        {
            return New(modGuid, modificationName, behaviour, null);
        }

        public static IEnumerator SetSlotModification(this CardSlot slot, ModificationType modType)
        {
            if (slot == null)
                yield break;

            var oldModification = ModificationType.NoModification;
            if (SlotModification.ContainsKey(slot))
            {
                AllModifiedSlots[SlotModification[slot]].Remove(slot);
                oldModification = SlotModification[slot];
            }

            SlotModification[slot] = modType;
            AllModifiedSlots[modType].Add(slot);

            var defn = AllSlotModifications.FirstOrDefault(m => m.ModificationType == modType);
            if (defn == null || defn.Texture == null)
                ResetSlot(slot);
            else
                slot.SetTexture(defn.Texture);

            if (defn != null && defn.SlotBehaviour != null)
            {
                var triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
                if (triggerComponent == null)
                    BoardManager.Instance.gameObject.AddComponent(defn.SlotBehaviour);
            }

            yield return CustomTriggerFinder.TriggerAll<ISlotModificationChanged>(
                false,
                t => t.RespondsToSlotModificationChanged(slot, oldModification, modType),
                t => t.OnSlotModificationChanged(slot, oldModification, modType)
            );
        }

        public static ModificationType GetSlotModification(this CardSlot slot)
        {
            if (slot == null)
                return ModificationType.NoModification;

            if (SlotModification.ContainsKey(slot))
                return SlotModification[slot];

            return ModificationType.NoModification;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static void ResetBuffedSlots()
        {
            SlotModification.Clear();
            foreach (var slotList in AllModifiedSlots.Values)
            {
                slotList.Clear();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static IEnumerator CleanUpBuffedSlots(IEnumerator sequence)
        {
            foreach (var slot in BoardManager.Instance.AllSlots)
                if (slot.GetSlotModification() != ModificationType.NoModification)
                    yield return slot.SetSlotModification(ModificationType.NoModification);

            foreach (var defn in AllSlotModifications)
            {
                var comp = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
                if (comp != null)
                    GameObject.Destroy(comp);
            }

            yield return sequence;
        }

        private static void ResetSlot(CardSlot slot)
        {
            if (SaveManager.SaveFile.IsPart1)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot"));
            if (SaveManager.SaveFile.IsPart3)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot_tech"));
            if (SaveManager.SaveFile.IsGrimora)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot_undead"));
            if (SaveManager.SaveFile.IsMagnificus)
                slot.SetTexture(ResourceBank.Get<Texture>("Art/Cards/card_slot_wizard"));
        }
    }
}