using System;
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

        internal static bool CardIsPlayingAttackAnimation(this PlayableCard card)
        {
            UnityEngine.AnimatorStateInfo info;
            if (card.Anim is DiskCardAnimationController dcac)
                info = dcac.weaponAnim.GetCurrentAnimatorStateInfo(0);
            else
                info = card.Anim.Anim.GetCurrentAnimatorStateInfo(0);
            return info.IsName("attack_player") || info.IsName("attack_creature") || info.IsName("attack_inair");
        }

        internal static void SpecialPatchDamageTrigger(Harmony harmony)
        {
            var allCombatPhaseManagers = AppDomain.CurrentDomain.GetAssemblies()
                                         .Where(a => a != null)
                                         .SelectMany(a => a.GetTypes())
                                         .Where(t => typeof(CombatPhaseManager).IsAssignableFrom(t))
                                         .ToList();

            var patchMethod = typeof(CustomTriggerPatches).GetMethod("TakeDirectDamageAttackAnimTrigger", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            foreach (var cpm in allCombatPhaseManagers)
            {
                P03SigilLibraryPlugin.Log.LogInfo($"Evaluating {cpm} for patching");
                var targetMethod = cpm.GetMethod(nameof(CombatPhaseManager.VisualizeCardAttackingDirectly), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (targetMethod == null)
                {
                    P03SigilLibraryPlugin.Log.LogInfo($"Could not find VisualizeCardAttackingDirectly");
                    continue;
                }
                if (targetMethod.DeclaringType != cpm)
                {
                    P03SigilLibraryPlugin.Log.LogInfo($"Does not directly implement VisualizeCardAttackingDirectly");
                    continue;
                }
                harmony.Patch(targetMethod, postfix: new HarmonyMethod(patchMethod));
            }
        }

        private static IEnumerator TakeDirectDamageAttackAnimTrigger(IEnumerator sequence, CardSlot attackingSlot, CardSlot targetSlot, int damage)
        {
            yield return sequence;
            if (attackingSlot.Card != null)
            {
                foreach (var t in attackingSlot.Card.TriggerHandler.FindTriggersOnCard<IOnDealDamageWithAttackAnimation>())
                {
                    if (t.RespondsToDealDamageWithAttackAnimation(targetSlot, damage))
                    {
                        yield return t.OnDealDamageWithAttackAnimation(targetSlot, damage);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
        [HarmonyPostfix]
        [HarmonyPriority(HarmonyLib.Priority.VeryLow)]
        private static IEnumerator TakeDamageAttackAnimTrigger(IEnumerator sequence, PlayableCard __instance, int damage, PlayableCard attacker)
        {
            if (attacker != null)
            {
                if (attacker.CardIsPlayingAttackAnimation())
                {
                    foreach (var t in attacker.TriggerHandler.FindTriggersOnCard<IOnDealDamageWithAttackAnimation>())
                    {
                        if (t.RespondsToDealDamageWithAttackAnimation(__instance.Slot, damage))
                        {
                            yield return t.OnDealDamageWithAttackAnimation(__instance.Slot, damage);
                        }
                    }
                }
            }
            yield return sequence;
        }


    }
}