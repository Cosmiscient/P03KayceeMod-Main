using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers.Extensions;
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

        public static ModificationType New(string modGuid, string modificationName, Type behaviour) => New(modGuid, modificationName, behaviour, null);

        public static IEnumerator SetSlotModification(this CardSlot slot, ModificationType modType)
        {
            if (slot == null)
                yield break;

            ModificationType oldModification = ModificationType.NoModification;
            if (SlotModification.ContainsKey(slot))
            {
                AllModifiedSlots[SlotModification[slot]].Remove(slot);
                oldModification = SlotModification[slot];
            }

            SlotModification[slot] = modType;
            AllModifiedSlots[modType].Add(slot);

            Info defn = AllSlotModifications.FirstOrDefault(m => m.ModificationType == modType);
            if (defn == null || defn.Texture == null)
                ResetSlot(slot);
            else
                slot.SetTexture(defn.Texture);

            if (defn != null && defn.SlotBehaviour != null)
            {
                Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
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
            return slot == null
                ? ModificationType.NoModification
                : SlotModification.ContainsKey(slot) ? SlotModification[slot] : ModificationType.NoModification;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static void ResetBuffedSlots()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            SlotModification.Clear();
            foreach (List<CardSlot> slotList in AllModifiedSlots.Values)
            {
                slotList.Clear();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static IEnumerator CleanUpBuffedSlots(IEnumerator sequence)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            foreach (CardSlot slot in BoardManager.Instance.AllSlots)
            {
                if (slot.GetSlotModification() != ModificationType.NoModification)
                    yield return slot.SetSlotModification(ModificationType.NoModification);
            }

            foreach (Info defn in AllSlotModifications.Where(m => m.SlotBehaviour != null))
            {
                Component comp = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
                if (comp != null)
                    UnityEngine.Object.Destroy(comp);
            }

            yield return sequence;
        }

        private static Dictionary<CardTemple, Texture> DefaultSlotTextures = new()
        {
            { CardTemple.Nature, ResourceBank.Get<Texture>("Art/Cards/card_slot") },
            { CardTemple.Tech, ResourceBank.Get<Texture>("Art/Cards/card_slot_tech") },
            { CardTemple.Wizard, ResourceBank.Get<Texture>("Art/Cards/card_slot_undead") },
            { CardTemple.Undead, ResourceBank.Get<Texture>("Art/Cards/card_slot_wizard") }
        };

        private static Dictionary<CardTemple, List<Texture>> PlayerOverrideSlots = new();
        private static Dictionary<CardTemple, List<Texture>> OpponentOverrideSlots = new();

        internal static void OverrideDefaultSlotTexture(CardTemple temple, List<Texture> playerSlots, List<Texture> opponentSlots)
        {
            PlayerOverrideSlots[temple] = playerSlots;
            OpponentOverrideSlots[temple] = opponentSlots;
        }

        internal static void ResetDefaultSlotTexture(CardTemple temple)
        {
            if (PlayerOverrideSlots.ContainsKey(temple))
                PlayerOverrideSlots.Remove(temple);
            if (OpponentOverrideSlots.ContainsKey(temple))
                OpponentOverrideSlots.Remove(temple);
        }

        public static void ResetSlot(this CardSlot slot)
        {
            CardTemple temple = CardTemple.Tech;
            if (SaveManager.SaveFile.IsPart1)
                temple = CardTemple.Nature;
            if (SaveManager.SaveFile.IsGrimora)
                temple = CardTemple.Undead;
            if (SaveManager.SaveFile.IsMagnificus)
                temple = CardTemple.Wizard;

            var lookup = slot.IsOpponentSlot() ? OpponentOverrideSlots : PlayerOverrideSlots;
            if (lookup.ContainsKey(temple))
                slot.SetTexture(lookup[temple][slot.Index]);
            else
                slot.SetTexture(DefaultSlotTextures[temple]);
        }
    }
}