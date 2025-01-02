using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivateEverything : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        bool useWeaponAnim => this.Card.Info.GetExtendedPropertyAsBool("WeaponButtonPusher") ?? false;

        static ActivateEverything()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Button Pusher";
            info.rulebookDescription = "When [creature] is played, all activated sigils and sigils that trigger on play or on death are activated.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivateEverything),
                TextureHelper.GetImageAsTexture("ability_activate_everything.png", typeof(ActivateEverything).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        private IEnumerator PreTriggerAnimate(CardSlot targetSlot)
        {
            if (useWeaponAnim && Card.Anim is DiskCardAnimationController dcac)
            {
                dcac.AimWeaponAnim(targetSlot.transform.position);
                dcac.ShowWeaponAnim();
                yield return new WaitForSeconds(0.2f);
            }
            Card.Anim.StrongNegationEffect();
            targetSlot.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.25f);
            if (useWeaponAnim && Card.Anim is DiskCardAnimationController dcac2)
            {
                dcac2.HideWeaponAnim();
            }

        }

        private IEnumerator TriggerHelper(CardSlot slot, Trigger trigger, params object[] otherArgs)
        {
            if (slot == null || slot.Card == null || slot.Card.Dead)
                yield break;

            foreach (var receiver in slot.Card.TriggerHandler.triggeredAbilities.Select(p => p.Item2))
            {
                if (receiver is not ActivateEverything)
                {
                    if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                    {
                        yield return PreTriggerAnimate(slot);
                        yield return GlobalTriggerHandler.Instance.TriggerSequence(trigger, receiver, otherArgs);
                    }
                }
            }
        }

        private IEnumerator ActivatedTriggerHelper(CardSlot slot)
        {
            if (slot == null || slot.Card == null || slot.Card.Dead)
                yield break;

            foreach (var pair in slot.Card.TriggerHandler.triggeredAbilities)
            {
                if (pair.Item2 is FuelActivatedAbilityBehaviour fab)
                {
                    bool didActivatePrevious = fab.HasActivatedThisTurn;
                    fab.HasActivatedThisTurn = false;
                    if (fab.CanActivate())
                    {
                        yield return PreTriggerAnimate(slot);
                        yield return fab.ActivateAfterSpendFuel();
                    }
                    fab.HasActivatedThisTurn = didActivatePrevious;
                }
                else if (pair.Item2 is ActivatedAbilityBehaviour ab)
                {
                    if (ab.CanActivate())
                    {
                        yield return PreTriggerAnimate(slot);
                        yield return ab.Activate();
                    }
                }
            }
        }

        public override IEnumerator OnResolveOnBoard()
        {
            var slots = BoardManager.Instance.GetSlotsCopy(Card.IsPlayerCard())
                        .Concat(BoardManager.Instance.GetSlotsCopy(!Card.IsPlayerCard()))
                        .ToList();

            // Resolve on board
            foreach (var slot in slots)
                yield return TriggerHelper(slot, Trigger.ResolveOnBoard);

            // ACtivated abilities
            foreach (var slot in slots)
                yield return ActivatedTriggerHelper(slot);

            // Pre death animation
            foreach (var slot in slots)
                yield return TriggerHelper(slot, Trigger.PreDeathAnimation, false);

            // Die
            foreach (var slot in slots)
                yield return TriggerHelper(slot, Trigger.Die, false, null);
        }
    }
}
