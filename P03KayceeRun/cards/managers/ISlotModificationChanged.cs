// using System.Collections;
// using DiskCardGame;

// namespace Infiniscryption.P03KayceeRun.Cards
// {
//     public interface ISlotModificationChanged
//     {
//         /// <summary>
//         /// Indicates if the trigger wants to respond to the slot modification being changed
//         /// </summary>
//         /// <param name="slot">The card slot that was changed</param>
//         /// <param name="previous">The previous modification</param>
//         /// <param name="current">The current modification</param>
//         /// <returns>True if this trigger wants to respond; false otherwise</returns>
//         public bool RespondsToSlotModificationChanged(CardSlot slot, SlotModificationManager.ModificationType previous, SlotModificationManager.ModificationType current);

//         /// <summary>
//         /// Action taken when the slot modification changes
//         /// </summary>
//         /// <param name="slot">The card slot that was changed</param>
//         /// <param name="previous">The previous modification</param>
//         /// <param name="current">The current modification</param>
//         public IEnumerator OnSlotModificationChanged(CardSlot slot, SlotModificationManager.ModificationType previous, SlotModificationManager.ModificationType current);
//     }
// }