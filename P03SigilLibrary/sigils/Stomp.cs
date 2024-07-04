using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class Stomp : ActivatedAbilityBehaviour
    {
        private const float STOMP_TIME = 0.3f;

        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private int energyCostTest = 3;
        public override int EnergyCost => energyCostTest;

        static Stomp()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Overwhelming Entrance";
            info.rulebookDescription = "When [creature] is played, all opposing creatures are tossed into new slots. Non-conduit terrain is not affected.";
            info.canStack = false;
            info.powerLevel = 2;
            info.activated = true;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Stomp),
                TextureHelper.GetImageAsTexture("ability_stomp.png", typeof(Stomp).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.RespondsToResolveOnBoard))]
        [HarmonyPostfix]
        [HarmonyPriority(HarmonyLib.Priority.VeryLow)]
        private static void RespondsToResolveOnBoardPatch(ActivatedAbilityBehaviour __instance, ref bool __result)
        {
            if (__instance is Stomp)
                __result = true;
        }

        private static bool SlotCanBeShuffled(CardSlot slot)
        {
            if (slot.Card == null)
                return true;

            if (slot.Card.HasConduitAbility())
                return true;

            if (slot.Card.HasTrait(Trait.Terrain))
                return false;

            return true;
        }

        private static void BounceToSlot(PlayableCard card, CardSlot slot, float height = 0.75f)
        {
            float diffX = slot.transform.position.x - card.transform.position.x;
            Vector3 start = card.transform.position;

            Tween.Value(0f, 1f, delegate (float v)
            {
                float newX = diffX > 0 ? Mathf.Min(diffX * v, diffX) : Mathf.Max(diffX * v, diffX);
                float newY = v <= 0.5
                             ? Tween.EaseOut.Evaluate(v * 2f) * height
                             : height - Tween.EaseIn.Evaluate((v - 0.5f) * 2f);

                newY = Mathf.Max(0f, newY);

                card.transform.position = start + new Vector3(newX, newY, 0);
            }, STOMP_TIME, 0f);
        }

        private static IEnumerator ShuffleQueue()
        {
            List<PlayableCard> queue = new(TurnManager.Instance.Opponent.Queue);
            List<CardSlot> validQueueSlots = BoardManager.Instance.GetSlotsCopy(false);
            int randomSeed = P03SigilLibraryPlugin.RandomSeed + 525;
            Dictionary<PlayableCard, CardSlot> assignments = new();
            foreach (var card in queue)
            {
                CardSlot newTarget = card.QueuedSlot;
                int sanityCheck = 0;
                while (newTarget == card.Slot && sanityCheck < 10)
                {
                    sanityCheck += 1;
                    newTarget = validQueueSlots[SeededRandom.Range(0, validQueueSlots.Count, randomSeed++)];
                }

                assignments[card] = newTarget;
                validQueueSlots.Remove(newTarget);
            }
            foreach (KeyValuePair<PlayableCard, CardSlot> kvp in assignments)
            {
                BounceToSlot(kvp.Key, kvp.Value);
            }
            yield return new WaitForSeconds(0.6f);
            foreach (KeyValuePair<PlayableCard, CardSlot> kvp in assignments)
            {
                if (kvp.Key.QueuedSlot != kvp.Value)
                {
                    BoardManager.Instance.QueueCardForSlot(kvp.Key, kvp.Value, tweenLength: 0.0f, doTween: true);
                }
            }
        }

        [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.OnResolveOnBoard))]
        [HarmonyPostfix]
        [HarmonyPriority(HarmonyLib.Priority.VeryHigh)]
        private static IEnumerator OnResolveOnBoardPatch(IEnumerator sequence, ActivatedAbilityBehaviour __instance)
        {
            if (__instance is not Stomp)
            {
                yield return sequence;
                yield break;
            }

            __instance.Card.Anim.StrongNegationEffect();
            TableVisualEffectsManager.Instance.ThumpTable(0.7f);
            List<CardSlot> slotsToShuffle = BoardManager.Instance.GetSlotsCopy(__instance.Card.OpponentCard)
                                                        .Where(SlotCanBeShuffled)
                                                        .ToList();
            List<PlayableCard> cardsToAssign = slotsToShuffle.Where(s => s.Card != null).Select(s => s.Card).ToList();

            Dictionary<PlayableCard, CardSlot> assignments = new();
            int randomSeed = P03SigilLibraryPlugin.RandomSeed;
            foreach (PlayableCard card in cardsToAssign)
            {
                CardSlot newTarget = card.Slot;
                int sanityCheck = 0;
                while (newTarget == card.Slot && sanityCheck < 10)
                {
                    sanityCheck += 1;
                    newTarget = slotsToShuffle[SeededRandom.Range(0, slotsToShuffle.Count, randomSeed++)];
                }

                assignments[card] = newTarget;
                slotsToShuffle.Remove(newTarget);
            }
            yield return new WaitForSeconds(0.05f);
            foreach (KeyValuePair<PlayableCard, CardSlot> kvp in assignments)
            {
                BounceToSlot(kvp.Key, kvp.Value);
            }

            if (!__instance.Card.OpponentCard)
                yield return ShuffleQueue();
            else
                yield return new WaitForSeconds(0.6f);

            foreach (KeyValuePair<PlayableCard, CardSlot> kvp in assignments)
            {
                if (kvp.Key.slot != kvp.Value)
                    yield return BoardManager.Instance.AssignCardToSlot(kvp.Key, kvp.Value, 0f);
            }
        }

        public override IEnumerator Activate()
        {
            yield return this.OnResolveOnBoard();
        }
    }
}