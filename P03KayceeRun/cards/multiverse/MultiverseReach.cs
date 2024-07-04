using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseReach : AbilityBehaviour, IMultiverseAbility, IOnPostSingularSlotAttackSlot
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;
        public override int Priority => -10000;
        private CardSlot returnToSlot = null;

        static MultiverseReach()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.Reach);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "[creature] will leap across universes to block attacks that occur in this lane in any universe.";
            info.canStack = original.canStack;
            info.powerLevel = 1;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = Color.black;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseReach),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_reach")
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPostfix]
        private static void ThisIsReachToo(PlayableCard __instance, Ability ability, ref bool __result)
        {
            if (!__result && ability == Ability.Reach && __instance.HasAbility(AbilityID))
                __result = true;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            returnToSlot = null;
            yield break;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            if (!Card.OnBoard)
                return false;

            if (attacker != null)
                return slot.Card == null && base.Card.Slot.IsPlayerSlot == slot.IsPlayerSlot && ((slot.Index % 10) == (Card.slot.Index % 10));
            return false;
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            bool shouldMultiverseTeleport = !MultiverseBattleSequencer.Instance.InSameUniverse(Card.Slot, slot);
            bool shouldTeleportOut = shouldMultiverseTeleport && MultiverseBattleSequencer.Instance.GetUniverseId(Card.slot) == MultiverseBattleSequencer.Instance.CurrentMultiverseId;
            bool shouldTeleportIn = shouldMultiverseTeleport && MultiverseBattleSequencer.Instance.GetUniverseId(slot) == MultiverseBattleSequencer.Instance.CurrentMultiverseId;

            returnToSlot = Card.Slot;

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
            yield return BoardManager.Instance.AssignCardToSlot(base.Card, slot, 0.1f, null, false);
            yield return new WaitForSeconds(0.05f);
            yield break;
        }

        public bool RespondsToPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot) => returnToSlot != null;

        public IEnumerator OnPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot)
        {
            bool shouldMultiverseTeleport = !MultiverseBattleSequencer.Instance.InSameUniverse(Card.Slot, returnToSlot);
            bool shouldTeleportOut = shouldMultiverseTeleport && MultiverseBattleSequencer.Instance.GetUniverseId(Card.slot) == MultiverseBattleSequencer.Instance.CurrentMultiverseId;
            bool shouldTeleportIn = shouldMultiverseTeleport && MultiverseBattleSequencer.Instance.GetUniverseId(returnToSlot) == MultiverseBattleSequencer.Instance.CurrentMultiverseId;

            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.05f);
            yield return base.PreSuccessfulTriggerSequence();
            Vector3 a = base.Card.Slot.IsPlayerSlot ? Vector3.back : Vector3.forward;

            if (shouldTeleportOut)
                MultiverseBattleSequencer.Instance.TeleportationEffects(Card.Slot);

            Tween.Position(base.Card.transform, base.Card.transform.position + a * 2f + Vector3.up * 0.25f, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.15f);
            Tween.Position(base.Card.transform, new Vector3(Card.Slot.transform.position.x, base.Card.transform.position.y, base.Card.transform.position.z), 0.1f, 0f, null, Tween.LoopType.None, null, null, true);

            if (shouldTeleportIn)
                MultiverseBattleSequencer.Instance.TeleportationEffects(returnToSlot);

            yield return new WaitForSeconds(0.1f);
            yield return BoardManager.Instance.AssignCardToSlot(base.Card, returnToSlot, 0.1f, null, false);
            yield return new WaitForSeconds(0.05f);
            returnToSlot = null;
            yield break;
        }
    }
}
