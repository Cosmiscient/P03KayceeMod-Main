using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class TestOfStrengthBattleOpponent : DamageRaceOpponent
    {
        private List<string> currentItems;

        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            AudioController.Instance.FadeOutLoop(0.1f, new int[] { 0, 1 });
            DamageRaceGenerator.Instance.SetShown(true);
            yield return new WaitForSeconds(0.1f);
            this.SetSceneEffectsShown(true);
            HoloMapAreaManager.Instance.MarkAudioConfigChanged();
            AudioController.Instance.SetLoopAndPlay("part3_damagerace");
            AudioController.Instance.SetLoopVolumeImmediate(0.8f, 0);
            if (TestOfStrengthBattleSequencer.HighScore == 0)
            {
                yield return new WaitForSeconds(1f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03TestOfStrengthIntro", TextDisplayer.MessageAdvanceMode.Input);
            }
            currentItems = new();
            currentItems.AddRange(Part3SaveData.Data.items);
            P03Plugin.Log.LogDebug("Saved the following item data: " + string.Join(", ", currentItems));
            yield break;
        }

        public void RestoreItems()
        {
            if (currentItems != null)
            {
                P03Plugin.Log.LogDebug("Retrieved the following item data: " + string.Join(", ", currentItems));
                Part3SaveData.Data.items.Clear();
                Part3SaveData.Data.items.AddRange(currentItems);
                ItemsManager.Instance.UpdateItems(false);
                var currentItemLog = ItemsManager.Instance.consumableSlots.Select(s => s.Item.Data.name);
                P03Plugin.Log.LogDebug("Items manager has: " + string.Join(", ", currentItemLog));
            }
            else
            {
                P03Plugin.Log.LogDebug("Current item data is null");
            }
        }

        public override IEnumerator OutroSequence(bool wasDefeated)
        {
            RestoreItems();
            yield return base.OutroSequence(wasDefeated);
        }
    }
}