using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class GoobertCenterCardBehaviourHelpers
    {
        public static bool IsOnBoard(this IEnumerable<GoobertCenterCardBehaviour> items) => items.OnBoard().Any();

        public static IEnumerable<GoobertCenterCardBehaviour> OnBoard(this IEnumerable<GoobertCenterCardBehaviour> items)
        {
            foreach (var b in items)
            {
                if (b.SafeIsUnityNull())
                    continue;

                PlayableCard p = b.PlayableCard;
                if (p != null && p.OnBoard)
                    yield return b;
            }
        }

        public static bool CanFireOnAllSlots(Trigger trigger, TriggerReceiver receiver)
        {
            if (trigger != Trigger.Die && trigger != Trigger.PreDeathAnimation)
                return false;

            if (receiver is Latch)
                return false;

            return true;
        }

        [HarmonyPatch(typeof(ActivatedDealDamage), nameof(ActivatedDealDamage.CanActivate))]
        [HarmonyPrefix]
        private static bool GoobertGunCanActivate(ActivatedDealDamage __instance, ref bool __result)
        {
            if (!__instance.Card.HasSpecialAbility(GoobertCenterCardBehaviour.AbilityID))
                return true;

            __result = __instance.Card.GetOpposingSlots().Any(cs => cs.Card != null);
            return false;
        }

        [HarmonyPatch(typeof(ActivatedDealDamage), nameof(ActivatedDealDamage.Activate))]
        [HarmonyPostfix]
        private static IEnumerator GoobertGun(IEnumerator sequence, ActivatedDealDamage __instance)
        {
            if (!__instance.Card.HasSpecialAbility(GoobertCenterCardBehaviour.AbilityID))
            {
                yield return sequence;
                yield break;
            }

            foreach (var slot in __instance.Card.GetOpposingSlots().Where(cs => cs.Card != null))
            {
                if (__instance.Card.Dead)
                    break;

                bool impactFrameReached = false;
                __instance.Card.Anim.PlayAttackAnimation(false, slot, delegate ()
                {
                    impactFrameReached = true;
                });
                yield return new WaitUntil(() => impactFrameReached);
                yield return slot.Card.TakeDamage(1, __instance.Card);
                yield return new WaitForSeconds(0.25f);
            }
            yield break;
        }
    }
}