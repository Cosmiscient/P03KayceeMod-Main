using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    internal static class CustomTriggerPatches
    {
        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Sacrifice))]
        [HarmonyPostfix]
        private static IEnumerator TriggerSacrificeListener(IEnumerator sequence, PlayableCard __instance)
        {
            var sacrificeCard = BoardManager.Instance.CurrentSacrificeDemandingCard;
            if (sacrificeCard != null)
            {
                foreach (var trigger in sacrificeCard.TriggerHandler.FindTriggersOnCard<IAbsorbSacrifices>())
                {
                    if (trigger.RespondsToCardSacrificedAsCost(__instance))
                        yield return trigger.OnCardSacrificedAsCost(__instance);
                }
            }
            yield return sequence;
        }

        private static IEnumerator TriggerEverythingTriggers()
        {
            List<IRespondToEverything> triggers = CustomTriggerFinder.FindTriggersOnBoard<IRespondToEverything>().ToList();
            foreach (var e in triggers)
                yield return e.OnEverything();
        }

        [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.OnTrigger))]
        [HarmonyPostfix]
        private static IEnumerator EverythingTriggerCardTriggerHandler(IEnumerator sequence)
        {
            yield return sequence;
            yield return TriggerEverythingTriggers();
        }

        [HarmonyPatch(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator EverythingTriggerGlobalTrigger(IEnumerator sequence)
        {
            yield return sequence;
            yield return TriggerEverythingTriggers();
        }
    }
}