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
    public class SlotModificationManager : MonoBehaviour
    {
        private static SlotModificationManager m_instance;
        public static SlotModificationManager Instance
        {
            get
            {
                if (m_instance != null)
                    return m_instance;
                Instantiate();
                return m_instance;
            }
            set => m_instance = value;
        }

        private static void Instantiate()
        {
            if (m_instance != null)
                return;

            GameObject slotModManager = new("SlotModificationManager");
            slotModManager.transform.SetParent(BoardManager.Instance.gameObject.transform);
            m_instance = slotModManager.AddComponent<SlotModificationManager>();

            foreach (var info in AllSlotModifications)
                m_instance.AllModifiedSlots.Add(info.ModificationType, new());
        }

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

        internal readonly Dictionary<ModificationType, List<CardSlot>> AllModifiedSlots = new();

        internal readonly Dictionary<CardSlot, ModificationType> SlotModification = new();

        public static ModificationType New(string modGuid, string modificationName, Type behaviour, Texture2D slotTexture)
        {
            if (!behaviour.IsSubclassOf(typeof(NonCardTriggerReceiver)))
                throw new InvalidOperationException("The slot behavior must be a subclass of NonCardTriggerReceiver");

            ModificationType mType = GuidManager.GetEnumValue<ModificationType>(modGuid, modificationName);
            AllSlotModifications.Add(new()
            {
                Texture = slotTexture,
                SlotBehaviour = behaviour,
                ModificationType = mType
            });
            return mType;
        }

        public static ModificationType New(string modGuid, string modificationName, Type behaviour) => New(modGuid, modificationName, behaviour, null);

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static void ResetBuffedSlots()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            Instance.SlotModification.Clear();
            foreach (List<CardSlot> slotList in Instance.AllModifiedSlots.Values)
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

        internal static readonly Dictionary<CardTemple, Texture> DefaultSlotTextures = new()
        {
            { CardTemple.Nature, ResourceBank.Get<Texture>("Art/Cards/card_slot") },
            { CardTemple.Tech, ResourceBank.Get<Texture>("Art/Cards/card_slot_tech") },
            { CardTemple.Wizard, ResourceBank.Get<Texture>("Art/Cards/card_slot_undead") },
            { CardTemple.Undead, ResourceBank.Get<Texture>("Art/Cards/card_slot_wizard") }
        };

        internal readonly Dictionary<CardTemple, List<Texture>> PlayerOverrideSlots = new();
        internal readonly Dictionary<CardTemple, List<Texture>> OpponentOverrideSlots = new();

        internal void OverrideDefaultSlotTexture(CardTemple temple, List<Texture> playerSlots, List<Texture> opponentSlots)
        {
            PlayerOverrideSlots[temple] = new(playerSlots);
            OpponentOverrideSlots[temple] = new(opponentSlots);
        }

        internal void ResetDefaultSlotTexture(CardTemple temple)
        {
            if (PlayerOverrideSlots.ContainsKey(temple))
                PlayerOverrideSlots.Remove(temple);
            if (OpponentOverrideSlots.ContainsKey(temple))
                OpponentOverrideSlots.Remove(temple);
        }
    }
}