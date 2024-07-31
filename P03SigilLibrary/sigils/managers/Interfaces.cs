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

    public interface IOnMissileStrike
    {
        public bool RespondsToStrikeQueued(CardSlot targetSlot);
        public IEnumerator OnStrikeQueued(CardSlot targetSlot);

        public bool RespondsToPreStrikeHit(CardSlot targetSlot);
        public IEnumerator OnPreStrikeHit(CardSlot targetSlot);

        public bool RespondsToPostStrikeHit(CardSlot targetSlot);
        public IEnumerator OnPostStrikeHit(CardSlot targetSlot);

        public bool RespondsToPostAllStrike();
        public IEnumerator OnPostAllStrike();
    }
}