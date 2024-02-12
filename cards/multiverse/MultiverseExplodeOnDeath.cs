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
    public class MultiverseExplodeOnDeath : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private GameObject bombPrefab;

        private void Awake()
        {
            this.bombPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/DetonatorHoloBomb");
        }

        static MultiverseExplodeOnDeath()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.ExplodeOnDeath);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When [creature] dies, all adjacent cards in all universes are dealt 10 damage";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = original.passive;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseExplodeOnDeath),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_explodeondeath")
            ).Id;
        }

        private static IEnumerator BombCard(PlayableCard target, CardSlot source, GameObject bombPrefab)
        {
            GameObject bomb = Object.Instantiate<GameObject>(bombPrefab);
            bomb.transform.position = source.transform.position + Vector3.up * 0.1f;
            Tween.Position(bomb.transform, target.transform.position + Vector3.up * 0.1f, 0.5f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.5f);
            target.Anim.PlayHitAnimation();
            Object.Destroy(bomb);
            yield return target.TakeDamage(10, null);
            yield break;
        }

        private static IEnumerator ExplodeFromSlot(CardSlot slot, GameObject bombPrefab)
        {
            List<CardSlot> adjacentSlots = BoardManager.Instance.GetAdjacentSlots(slot);
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Index < slot.Index)
            {
                if (adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
                {
                    yield return BombCard(adjacentSlots[0].Card, slot, bombPrefab);
                }
                adjacentSlots.RemoveAt(0);
            }
            if (slot.opposingSlot.Card != null && !slot.opposingSlot.Card.Dead)
            {
                yield return BombCard(slot.opposingSlot.Card, slot, bombPrefab);
            }
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
            {
                yield return BombCard(adjacentSlots[0].Card, slot, bombPrefab);
            }
            yield break;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            Card.Anim.LightNegationEffect();
            yield return PreSuccessfulTriggerSequence();
            yield return ExplodeFromSlot(Card.Slot, this.bombPrefab);
            yield return LearnAbility(0.25f);
            // Set up the multiverse explosion:
            if (MultiverseBattleSequencer.Instance != null)
            {
                int slotIdx = Card.Slot.Index % 10;
                for (int i = 0; i < MultiverseBattleSequencer.Instance.MultiverseGames.Length; i++)
                {
                    if (i == MultiverseBattleSequencer.Instance.GetUniverseId(Card.Slot))
                        continue;

                    var universe = MultiverseBattleSequencer.Instance.MultiverseGames[i];
                    CardSlot newSlot = Card.OpponentCard ? universe.OpponentSlots[slotIdx] : universe.PlayerSlots[slotIdx];

                    universe.RegisterCallback(new OtherUniverseExplode(newSlot, bombPrefab));
                }
            }
            yield break;
        }

        protected class OtherUniverseExplode : IMultiverseDelayedCoroutine
        {
            public CardSlot targetSlot;
            public GameObject bombPrefab;

            public OtherUniverseExplode(CardSlot target, GameObject bomb)
            {
                targetSlot = target;
                bombPrefab = bomb;
            }

            public IEnumerator DoCallback()
            {
                MultiverseBattleSequencer.Instance.TeleportationEffects(targetSlot);
                yield return ExplodeFromSlot(targetSlot, bombPrefab);
            }
        }
    }
}
