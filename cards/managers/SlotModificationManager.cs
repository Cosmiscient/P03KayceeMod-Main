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
using Sirenix.Serialization.Utilities;
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
                ResetSlotTexture(slot);
            else
                slot.SetTexture(defn.Texture);

            // if (defn != null && defn.SlotBehaviour != null)
            // {
            //     Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
            //     if (triggerComponent == null)
            //         BoardManager.Instance.gameObject.AddComponent(defn.SlotBehaviour);
            // }

            // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
            // foreach (NonCardTriggerReceiver trigger in receivers)
            // {
            //     if (!trigger.SafeIsUnityNull() && trigger is ISlotModificationChanged ismc)
            //     {
            //         if (ismc.RespondsToSlotModificationChanged(slot, oldModification, modType))
            //             yield return ismc.OnSlotModificationChanged(slot, oldModification, modType);
            //     }
            // }

            // TODO: Go back to this when custom trigger finder is fixed
            yield return CustomTriggerFinder.TriggerAll(
                false,
                delegate (ISlotModificationChanged t)
                {
                    P03Plugin.Log.LogInfo($"About to trigger {t}");
                    return t.RespondsToSlotModificationChanged(slot, oldModification, modType);
                },
                t => t.OnSlotModificationChanged(slot, oldModification, modType)
            );
            P03Plugin.Log.LogInfo("Finished triggering for slot modification changes");
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

            // Ensure all of the various receivers are set up
            foreach (Info item in AllSlotModifications.Where(m => m.SlotBehaviour != null))
            {
                if (BoardManager.Instance.GetComponent(item.SlotBehaviour).SafeIsUnityNull())
                    BoardManager.Instance.gameObject.AddComponent(item.SlotBehaviour);
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static IEnumerator ResetSlots(IEnumerator sequence)
        {
            foreach (CardSlot slot in BoardManager.Instance.AllSlots)
            {
                if (slot.GetSlotModification() != ModificationType.NoModification)
                    yield return slot.SetSlotModification(ModificationType.NoModification);
            }

            yield return sequence;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.CleanUp))]
        [HarmonyPostfix]
        private static IEnumerator CleanUpBuffedSlots(IEnumerator sequence)
        {
            foreach (Info defn in AllSlotModifications.Where(m => m.SlotBehaviour != null))
            {
                Component comp = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
                if (!comp.SafeIsUnityNull())
                    UnityEngine.Object.Destroy(comp);
            }

            yield return sequence;
        }

        private static readonly Dictionary<CardTemple, Texture> DefaultSlotTextures = new()
        {
            { CardTemple.Nature, ResourceBank.Get<Texture>("Art/Cards/card_slot") },
            { CardTemple.Tech, ResourceBank.Get<Texture>("Art/Cards/card_slot_tech") },
            { CardTemple.Wizard, ResourceBank.Get<Texture>("Art/Cards/card_slot_undead") },
            { CardTemple.Undead, ResourceBank.Get<Texture>("Art/Cards/card_slot_wizard") }
        };

        private static readonly Dictionary<CardTemple, List<Texture>> PlayerOverrideSlots = new();
        private static readonly Dictionary<CardTemple, List<Texture>> OpponentOverrideSlots = new();

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

        public static void ResetSlotTexture(this CardSlot slot)
        {
            CardTemple temple = CardTemple.Tech;
            if (SaveManager.SaveFile.IsPart1)
                temple = CardTemple.Nature;
            if (SaveManager.SaveFile.IsGrimora)
                temple = CardTemple.Undead;
            if (SaveManager.SaveFile.IsMagnificus)
                temple = CardTemple.Wizard;

            Dictionary<CardTemple, List<Texture>> lookup = slot.IsOpponentSlot() ? OpponentOverrideSlots : PlayerOverrideSlots;
            if (lookup.ContainsKey(temple))
            {
                try
                {
                    slot.SetTexture(lookup[temple][slot.Index]);
                }
                catch
                {
                    slot.SetTexture(DefaultSlotTextures[temple]);
                }
            }
            else
            {
                slot.SetTexture(DefaultSlotTextures[temple]);
            }
        }
    }
}