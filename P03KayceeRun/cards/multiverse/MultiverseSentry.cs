using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseSentry : AbilityBehaviour, IMultiverseAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int Priority => -1;

        private int lastShotTurn = -1;
        private PlayableCard lastShotCard;

        private int NumShots => Mathf.Max(Card.Info.Abilities.FindAll((Ability x) => x == AbilityID).Count, 1);

        private void Awake()
        {
            if (Card.Anim is DiskCardAnimationController)
            {
                (Card.Anim as DiskCardAnimationController).SetWeaponMesh(DiskCardWeapon.Turret);
            }
        }

        static MultiverseSentry()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.Sentry);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When a creature moves into the space opposing [creature] in any universe, they are dealt 1 damage.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = original.passive;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseSentry),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_sentry")
            ).Id;
        }

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return !Card.Dead && !otherCard.Dead &&
            (
                otherCard.Slot == Card.Slot.opposingSlot ||
                (MultiverseBattleSequencer.Instance != null && MultiverseBattleSequencer.Instance.OpposesInAnyUniverse(Card.Slot, otherCard.Slot))
            );
        }

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            return this.RespondsToOtherCardResolve(otherCard);
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            yield return this.FireAtOpposingSlot(otherCard);
            yield break;
        }

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            yield return this.FireAtOpposingSlot(otherCard);
            yield break;
        }

        private IEnumerator FireAtOpposingSlot(PlayableCard otherCard)
        {
            if (otherCard != this.lastShotCard || TurnManager.Instance.TurnNumber != this.lastShotTurn)
            {
                this.lastShotCard = otherCard;
                this.lastShotTurn = TurnManager.Instance.TurnNumber;
                ViewManager.Instance.SwitchToView(View.Default, false, true);
                yield return new WaitForSeconds(0.25f);

                bool sameUniverse = MultiverseBattleSequencer.Instance.InSameUniverse(Card, otherCard);

                for (int i = 0; i < this.NumShots; i++)
                {
                    if (otherCard != null && !otherCard.Dead)
                    {
                        Card.Anim.LightNegationEffect();

                        bool impactFrameReached = false;
                        if (!sameUniverse)
                        {
                            yield return MultiverseBattleSequencer.Instance.VisualizeMultiversalAttack(Card, otherCard.slot, () => impactFrameReached = true);
                        }
                        else
                        {
                            if (Card.Anim is DiskCardAnimationController dcac)
                            {
                                dcac.SetWeaponMesh(DiskCardWeapon.Turret);
                                dcac.AimWeaponAnim(otherCard.slot.transform.position);
                                dcac.ShowWeaponAnim();
                            }
                            yield return new WaitForSeconds(0.5f);

                            Card.Anim.PlayAttackAnimation(Card.IsFlyingAttackingReach(), otherCard.slot, () => impactFrameReached = true);
                        }
                        yield return new WaitUntil(() => impactFrameReached);

                        yield return otherCard.TakeDamage(1, Card);
                    }
                }
                yield return LearnAbility(0.5f);
                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            }
            yield break;
        }
    }
}
