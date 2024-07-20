using DiskCardGame;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public interface IRestoreFromSnapshot
    {
        public void RestoreFromSnapshot(BoardState.CardState state);
    }
}