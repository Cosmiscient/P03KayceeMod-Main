using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
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

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            Card.Anim.StrongNegationEffect();
            TableVisualEffectsManager.Instance.ThumpTable(1f);
            yield return new WaitForSeconds(0.15f);
            List<CardSlot> slotsToShuffle = BoardManager.Instance.GetSlotsCopy(Card.OpponentCard);
            List<PlayableCard> cardsToAssign = slotsToShuffle.Where(s => s.Card != null).Select(s => s.Card).ToList();

            Dictionary<PlayableCard, CardSlot> assignments = new();
            int randomSeed = P03AscensionSaveData.RandomSeed;
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
                if (kvp.Key.slot != kvp.Value)
                    yield return BoardManager.Instance.AssignCardToSlot(kvp.Key, kvp.Value);
            }
        }
    }
}