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
    public class MultiverseGuardian : AbilityBehaviour, IMultiverseAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MultiverseGuardian()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.GuardDog);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When an opposing creature is placed opposite to an empty space in any universe, [creature] will move to that empty space.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = original.passive;
            info.hasColorOverride = true;
            info.colorOverride = Color.black;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseGuardian),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_guarddog")
            ).Id;
        }

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return Card.OnBoard && otherCard != null && otherCard.Slot != null && (otherCard.OpponentCard != Card.OpponentCard) && otherCard.Slot != Card.Slot.opposingSlot;
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            bool shouldMultiverseTeleport = MultiverseBattleSequencer.Instance != null && !MultiverseBattleSequencer.Instance.InSameUniverse(Card.Slot, otherCard.Slot);
            ViewManager.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.15f);
            CardSlot targetSlot = otherCard.Slot.opposingSlot;
            if (targetSlot.Card != null)
            {
                Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                yield return PreSuccessfulTriggerSequence();
                Vector3 a = (Card.Slot.transform.position + targetSlot.transform.position) / 2f;
                Tween.Position(Card.transform, a + Vector3.up * 0.5f, 0.1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);

                if (shouldMultiverseTeleport)
                    MultiverseBattleSequencer.Instance.TeleportationEffects(targetSlot);

                yield return BoardManager.Instance.AssignCardToSlot(Card, targetSlot, 0.1f, null, true);
                yield return new WaitForSeconds(0.3f);
                yield return LearnAbility(0.1f);
            }
            yield break;
        }
    }
}
