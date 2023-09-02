using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace P03KayceeRun.cards
{
    public class Stomp : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static Stomp()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Overwhelming Entrance";
            info.rulebookDescription = "When [creature] is played, all opposing creatures are tossed into new slots";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Stomp),
                TextureHelper.GetImageAsTexture("ability_stomp.png", typeof(Stomp).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            Card.Anim.StrongNegationEffect();
            TableVisualEffectsManager.Instance.ThumpTable(0.5f);
            List<CardSlot> slotsToShuffle = new(BoardManager.Instance.GetSlots(Card.OpponentCard));
            List<PlayableCard> cardsToAssign = slotsToShuffle.Where(s => s.Card != null).Select(s => s.Card).ToList();

            Dictionary<PlayableCard, CardSlot> assignments = new();
            int randomSeed = P03AscensionSaveData.RandomSeed;
            foreach (PlayableCard card in cardsToAssign)
            {
                CardSlot newTarget = card.Slot;
                while (newTarget == card.Slot)
                {
                    newTarget = slotsToShuffle[SeededRandom.Range(0, slotsToShuffle.Count, randomSeed++)];
                }

                assignments[card] = newTarget;
                slotsToShuffle.Remove(newTarget);
            }
            yield return new WaitForSeconds(0.05f);
            foreach (KeyValuePair<PlayableCard, CardSlot> kvp in assignments)
            {
                yield return BoardManager.Instance.AssignCardToSlot(kvp.Key, kvp.Value);
            }
        }
    }
}