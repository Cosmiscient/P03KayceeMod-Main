using System.Collections;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MultiverseBossOpponent : Part3BossOpponent
    {
        public override string PreIntroDialogueId => "P03RoyalBossInto";
        public override string PostDefeatedDialogueId => "P03RoyalBossOuttro";

        public override IEnumerator StartBattleSequence()
        {
            NumLives = 1;
            yield break;
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