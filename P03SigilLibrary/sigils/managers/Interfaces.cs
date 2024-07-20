using System.Collections;
using DiskCardGame;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public interface IAbsorbSacrifices
    {
        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice);

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice);
    }

    public interface IRespondToEverything
    {
        public IEnumerator OnEverything();
    }
}