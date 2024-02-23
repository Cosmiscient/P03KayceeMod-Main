using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public interface IRestoreFromSnapshot
    {
        public void RestoreFromSnapshot(BoardState.CardState state);
    }
}