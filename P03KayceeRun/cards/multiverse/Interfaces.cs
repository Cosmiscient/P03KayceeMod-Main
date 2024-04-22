using System.Collections;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    public interface IMultiverseAbility
    {

    }

    public interface IMultiverseDelayedCoroutine
    {
        public IEnumerator DoCallback();
    }

    public static class MultiverseHelpers
    {
        public static bool SafeIsUnityNull(this IMultiverseDelayedCoroutine co)
        {
            if (co is not Component comp)
                return false;

            return comp.SafeIsUnityNull();
        }
    }
}