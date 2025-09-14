using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
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
    public class MultiverseStrafe : AbilityBehaviour, IMultiverseDelayedCoroutine
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private bool hasStrafed = false;

        private CardSlot strafeToSlot = null;

        static MultiverseStrafe()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.Strafe);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "At the end of the owner's turn, [creature] will move to the next universe.";
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
                typeof(MultiverseStrafe),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_strafe")
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => MultiverseBattleSequencer.Instance != null && Card.OnBoard && playerUpkeep != Card.OpponentCard;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            hasStrafed = false;
            yield break;
        }

        private CardSlot GetNextSlot()
        {
            CardSlot currentSlot = this.Card.Slot;
            int activeSlotIdx = currentSlot.Index % 10;
            int multiverseIndex = MultiverseBattleSequencer.Instance.CurrentMultiverseId;

            int nextUniverse = multiverseIndex + 1;
            if (nextUniverse >= MultiverseBattleSequencer.Instance.MultiverseGames.Length)
                nextUniverse = 0;

            while (nextUniverse != multiverseIndex)
            {
                var targetUniverse = MultiverseBattleSequencer.Instance.MultiverseGames[nextUniverse];
                List<CardSlot> slots = this.Card.OpponentCard ? targetUniverse.OpponentSlots : targetUniverse.PlayerSlots;
                CardSlot destination = slots[activeSlotIdx];

                if (destination.Card == null)
                    return destination;

                nextUniverse += 1;
                if (nextUniverse >= MultiverseBattleSequencer.Instance.MultiverseGames.Length)
                    nextUniverse = 0;
            }

            return null;
        }
        public override bool RespondsToTurnEnd(bool playerTurnEnd)
        {
            return MultiverseBattleSequencer.Instance != null && !hasStrafed && Card != null && Card.OpponentCard != playerTurnEnd && !Card.Dead;
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            strafeToSlot = GetNextSlot();
            if (strafeToSlot != null)
            {
                hasStrafed = true;
                MultiverseBattleSequencer.Instance.TeleportationEffects(Card.Slot);
                Tween.Position(Card.transform, Card.transform.position + Vector3.down, .5f, 0f);
                yield return new WaitForSeconds(0.4f);
                Card.UnassignFromSlot();
                int targetUniverse = MultiverseBattleSequencer.Instance.GetUniverseId(strafeToSlot);

                var phase = Card.OpponentCard ? MultiverseGameState.Phase.OpponentEnd : MultiverseGameState.Phase.PlayerEnd;
                MultiverseBattleSequencer.Instance.MultiverseGames[targetUniverse].RegisterCallback(phase, this);
            }
            else
            {
                Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.15f);
            }
        }

        public IEnumerator DoCallback()
        {
            if (strafeToSlot != null)
            {
                MultiverseBattleSequencer.Instance.TeleportationEffects(strafeToSlot);
                if (strafeToSlot.Card != null)
                {
                    yield return strafeToSlot.Card.Die(false, null);
                    MultiverseBattleSequencer.Instance.TeleportationEffects(strafeToSlot);
                }
                yield return BoardManager.Instance.AssignCardToSlot(Card, strafeToSlot);
                yield return new WaitForSeconds(0.25f);
                strafeToSlot = null;
            }
            yield break;
        }
    }
}
