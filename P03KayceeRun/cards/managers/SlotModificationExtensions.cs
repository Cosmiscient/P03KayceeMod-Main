// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using DiskCardGame;
// using HarmonyLib;
// using Infiniscryption.P03KayceeRun.Patchers;
// using InscryptionAPI.Guid;
// using InscryptionAPI.Helpers.Extensions;
// using InscryptionAPI.Triggers;
// using Sirenix.Serialization.Utilities;
// using UnityEngine;

// namespace Infiniscryption.P03KayceeRun.Cards
// {
//     [HarmonyPatch]
//     public static class SlotModificationExtensions
//     {
//         public static IEnumerator SetSlotModification(this CardSlot slot, SlotModificationManager.ModificationType modType)
//         {
//             if (slot == null)
//                 yield break;

//             SlotModificationManager.ModificationType oldModification = SlotModificationManager.ModificationType.NoModification;
//             if (SlotModificationManager.Instance.SlotModification.ContainsKey(slot))
//             {
//                 SlotModificationManager.Instance.AllModifiedSlots[SlotModificationManager.Instance.SlotModification[slot]].Remove(slot);
//                 oldModification = SlotModificationManager.Instance.SlotModification[slot];
//             }

//             SlotModificationManager.Instance.SlotModification[slot] = modType;
//             SlotModificationManager.Instance.AllModifiedSlots[modType].Add(slot);

//             SlotModificationManager.Info defn = SlotModificationManager.AllSlotModifications.FirstOrDefault(m => m.ModificationType == modType);
//             if (defn == null || defn.Texture == null)
//                 slot.ResetSlotTexture();
//             else
//                 slot.SetTexture(defn.Texture);

//             // if (defn != null && defn.SlotBehaviour != null)
//             // {
//             //     Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
//             //     if (triggerComponent == null)
//             //         BoardManager.Instance.gameObject.AddComponent(defn.SlotBehaviour);
//             // }

//             // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
//             // foreach (NonCardTriggerReceiver trigger in receivers)
//             // {
//             //     if (!trigger.SafeIsUnityNull() && trigger is ISlotModificationChanged ismc)
//             //     {
//             //         if (ismc.RespondsToSlotModificationChanged(slot, oldModification, modType))
//             //             yield return ismc.OnSlotModificationChanged(slot, oldModification, modType);
//             //     }
//             // }

//             // TODO: Go back to this when custom trigger finder is fixed
//             yield return CustomTriggerFinder.TriggerAll(
//                 false,
//                 delegate (ISlotModificationChanged t)
//                 {
//                     P03Plugin.Log.LogInfo($"About to trigger {t}");
//                     return t.RespondsToSlotModificationChanged(slot, oldModification, modType);
//                 },
//                 t => t.OnSlotModificationChanged(slot, oldModification, modType)
//             );
//             P03Plugin.Log.LogInfo("Finished triggering for slot modification changes");
//         }

//         public static SlotModificationManager.ModificationType GetSlotModification(this CardSlot slot)
//         {
//             return slot == null
//                 ? SlotModificationManager.ModificationType.NoModification
//                 : SlotModificationManager.Instance.SlotModification.ContainsKey(slot) ? SlotModificationManager.Instance.SlotModification[slot] : SlotModificationManager.ModificationType.NoModification;
//         }

//         public static void ResetSlotTexture(this CardSlot slot)
//         {
//             CardTemple temple = CardTemple.Tech;
//             if (SaveManager.SaveFile.IsPart1)
//                 temple = CardTemple.Nature;
//             if (SaveManager.SaveFile.IsGrimora)
//                 temple = CardTemple.Undead;
//             if (SaveManager.SaveFile.IsMagnificus)
//                 temple = CardTemple.Wizard;

//             Dictionary<CardTemple, List<Texture>> lookup = slot.IsOpponentSlot() ? SlotModificationManager.Instance.OpponentOverrideSlots : SlotModificationManager.Instance.PlayerOverrideSlots;
//             if (lookup.ContainsKey(temple))
//             {
//                 try
//                 {
//                     slot.SetTexture(lookup[temple][slot.Index]);
//                 }
//                 catch
//                 {
//                     slot.SetTexture(SlotModificationManager.DefaultSlotTextures[temple]);
//                 }
//             }
//             else
//             {
//                 slot.SetTexture(SlotModificationManager.DefaultSlotTextures[temple]);
//             }
//         }
//     }
// }