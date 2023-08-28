using System;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
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

        public static event Action<CardSlot, ModificationType> OnSlotModificationSet;

        private static readonly Dictionary<ModificationType, Texture2D> SlotTextures = new();

        private static readonly Dictionary<ModificationType, List<CardSlot>> AllModifiedSlots = new();
        private static readonly Dictionary<CardSlot, ModificationType> SlotModification = new();

        public static ModificationType New(string modGuid, string modificationName, Texture2D slotTexture)
        {
            ModificationType mType = GuidManager.GetEnumValue<ModificationType>(modGuid, modificationName);
            SlotTextures[mType] = slotTexture;
            AllModifiedSlots[mType] = new();
            return mType;
        }

        public static void SetSlotModification(this CardSlot slot, ModificationType modType)
        {
            if (SlotModification.ContainsKey(slot))
                AllModifiedSlots[SlotModification[slot]].Remove(slot);

            SlotModification[slot] = modType;
            AllModifiedSlots[modType].Add(slot);

            if (modType == ModificationType.NoModification)
                ResetSlot(slot);
            else
                slot.SetTexture(SlotTextures[modType]);

            OnSlotModificationSet?.Invoke(slot, modType);
        }

        public static ModificationType GetSlotModification(this CardSlot slot)
        {
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
                foreach (var slot in slotList)
                    OnSlotModificationSet?.Invoke(slot, ModificationType.NoModification);

                slotList.Clear();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static void CleanUpBuffedSlots()
        {
            SlotModification.Clear();
            foreach (var slotList in AllModifiedSlots.Values)
            {
                foreach (var slot in slotList)
                {
                    ResetSlot(slot);
                    OnSlotModificationSet?.Invoke(slot, ModificationType.NoModification);
                }

                slotList.Clear();
            }
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