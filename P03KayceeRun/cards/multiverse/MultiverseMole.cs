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
    public class MultiverseMole : AbilityBehaviour, IMultiverseAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MultiverseMole()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.WhackAMole);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When an empty space would be struck, [creature] will cross universes to move to that space to receive the strike instead.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = original.passive;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseMole),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_whackamole")
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            if (attacker != null)
            {
                bool flag = attacker.HasAbility(Ability.Flying);
                return slot.Card == null && base.Card.Slot.IsPlayerSlot == slot.IsPlayerSlot && (!flag || base.Card.HasAbility(Ability.Reach));
            }
            return false;
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            bool shouldMultiverseTeleport = !MultiverseBattleSequencer.Instance.InSameUniverse(Card.Slot, slot);
            bool shouldTeleportOut = shouldMultiverseTeleport && MultiverseBattleSequencer.Instance.GetUniverseId(Card.slot) == MultiverseBattleSequencer.Instance.CurrentMultiverseId;
            bool shouldTeleportIn = shouldMultiverseTeleport && MultiverseBattleSequencer.Instance.GetUniverseId(slot) == MultiverseBattleSequencer.Instance.CurrentMultiverseId;

            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.05f);
            yield return base.PreSuccessfulTriggerSequence();
            Vector3 a = base.Card.Slot.IsPlayerSlot ? Vector3.back : Vector3.forward;

            if (shouldTeleportOut)
                MultiverseBattleSequencer.Instance.TeleportationEffects(slot);

            Tween.Position(base.Card.transform, base.Card.transform.position + a * 2f + Vector3.up * 0.25f, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.15f);
            Tween.Position(base.Card.transform, new Vector3(slot.transform.position.x, base.Card.transform.position.y, base.Card.transform.position.z), 0.1f, 0f, null, Tween.LoopType.None, null, null, true);

            if (shouldTeleportIn)
                MultiverseBattleSequencer.Instance.TeleportationEffects(slot);

            yield return new WaitForSeconds(0.1f);
            yield return BoardManager.Instance.AssignCardToSlot(base.Card, slot, 0.1f, null, true);
            yield return new WaitForSeconds(0.05f);
            yield return base.LearnAbility(0f);
            yield break;
        }
    }
}
