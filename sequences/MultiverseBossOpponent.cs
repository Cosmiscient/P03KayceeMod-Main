using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MultiverseBossOpponent : Part3BossOpponent
    {
        public override string PreIntroDialogueId => "P03RoyalBossInto";
        public override string PostDefeatedDialogueId => "P03RoyalBossOuttro";

        public override bool GiveCurrencyOnDefeat => false;

        public override IEnumerator StartBattleSequence()
        {
            NumLives = 1;
            yield break;
        }

        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            yield return base.IntroSequence(encounter);
            P03AnimationController.Instance.SwitchToFace(P03TrollFace.ID);
        }

        public override void SetSceneEffectsShown(bool shown)
        {
            if (shown)
            {
                MultiverseGameState.LightColorState.GetPreset(GameColors.Instance.blue).RestoreState();
            }
        }
    }
}