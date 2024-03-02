using System.Collections;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    public interface IMultiverseAbility
    {

    }

    public interface IMultiverseDelayedCoroutine
    {
        public IEnumerator DoCallback();
    }
}