using System.Collections;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    public interface IBattleModSetup
    {
        /// <summary>
        /// Action taken when it's time to setup the battle mod
        /// </summary>
        public IEnumerator OnBattleModSetup();
    }

    public interface IBattleModCleanup
    {
        /// <summary>
        /// Action taken when it's time to setup the battle mod
        /// </summary>
        public IEnumerator OnBattleModCleanup();
    }

    public interface IModifyTerrain
    {
        public void ModifyPlayerTerrain(CardInfo[] terrain);
        public void ModifyOpponentTerrain(CardInfo[] terrain);
        public void ModifyOpponentQueuedTerrain(CardInfo[] terrain);
    }

    public interface IBattleModSimulator
    {
        public bool HasBoardStateAdjustment(BoardState board, bool playerIsAttacker);
        public void DoBoardStateAdjustment(BoardState board, bool playerIsAttacker);

        public bool HasCardEvaluationAdjustment(BoardState.CardState card, BoardState board);
        public int DoCardEvaluationAdjustment(BoardState.CardState card, BoardState board);
    }
}