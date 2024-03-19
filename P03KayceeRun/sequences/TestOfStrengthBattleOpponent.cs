using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class TestOfStrengthBattleOpponent : DamageRaceOpponent
    {
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
            yield break;
        }
    }
}