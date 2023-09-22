using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Shred : ActivatedAbilityBehaviour, IPassiveAttackBuff
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int ActivationsThisTurn = 0;

        static Shred()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Shred";
            info.rulebookDescription = "Shred a card from your hand to get +1 Attack (until end of turn). The shredded card counts as having died.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Shred),
                TextureHelper.GetImageAsTexture("ability_shred.png", typeof(Shred).Assembly)
            ).Id;
        }

        private static CardInfo FromPlayable(PlayableCard card)
        {
            CardInfo retval = CardLoader.Clone(card.Info);
            if (card.temporaryMods != null && card.temporaryMods.Count > 0)
                retval.mods.AddRange(card.temporaryMods);
            return retval;
        }

        private PlayableCard GetMatchFromHand(SelectableCard card)
        {
            return PlayerHand.Instance
                                  .CardsInHand
                                  .FirstOrDefault(
                                      c => c.Info.name == card.Info.name
                                         && card.Info.Abilities.All(a => c.HasAbility(a))
                                  );
        }

        private IEnumerator FakeDeath(PlayableCard shredded)
        {
            if (!shredded.Dead)
            {
                shredded.Dead = true;
                shredded.slot = Card.slot;
                if (shredded.Info != null && shredded.Info.name.ToLower().Contains("squirrel"))
                    AscensionStatsData.TryIncrementStat(AscensionStat.Type.SquirrelsKilled);

                if (shredded.TriggerHandler.RespondsToTrigger(Trigger.PreDeathAnimation, true))
                    yield return shredded.TriggerHandler.OnTrigger(Trigger.PreDeathAnimation, true);

                yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.OtherCardPreDeath, false, Card.Slot, false, Card);

                if (shredded.TriggerHandler.RespondsToTrigger(Trigger.Die, true, Card))
                    yield return shredded.TriggerHandler.OnTrigger(Trigger.Die, true, Card);

                yield return GlobalTriggerHandler.Instance.TriggerAll(Trigger.OtherCardDie, false, shredded, Card.Slot, false, Card);
                Destroy(shredded.gameObject);
            }
        }

        public override IEnumerator Activate()
        {
            // If the player has no cards, do nothing
            if (PlayerHand.Instance.CardsInHand.Count == 0)
                yield break;

            SelectableCard selectedCard = null;

            ViewManager.Instance.SwitchToView(View.DeckSelection, false, true);
            yield return BoardManager.Instance.CardSelector.SelectCardFrom(
                PlayerHand.Instance.CardsInHand.Select(FromPlayable).ToList(),
                null,
                s => selectedCard = s
            );

            ViewManager.Instance.SwitchToView(View.Default, false, false);

            if (selectedCard == null)
                yield break;

            // Remove the card from their hand
            PlayableCard playableCard = GetMatchFromHand(selectedCard);
            Destroy(selectedCard.gameObject);

            if (playableCard == null)
                yield break;

            // Hover the card into position
            PlayerHand.Instance.RemoveCardFromHand(playableCard);
            playableCard.transform.SetParent(Card.Slot.transform);
            playableCard.transform.eulerAngles = new(0f, 0f, 0f);
            Tween.Position(playableCard.transform, Card.Slot.transform.position + (Vector3.up * 2f), 0.25f, 0f);
            AudioController.Instance.PlaySound2D("shred", MixerGroup.TableObjectsSFX, volume: 2f);
            yield return new WaitForSeconds(0.25f);

            // Fly it through the board
            Tween.Position(playableCard.transform, Card.Slot.transform.position + (2 * Vector3.down), 0.85f, 0f);
            yield return new WaitForSeconds(0.85f);

            yield return FakeDeath(playableCard);

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            ActivationsThisTurn += 1;

            selectedCard = null;
            playableCard = null;
            yield break;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            ActivationsThisTurn = 0;
            yield break;
        }

        public int GetPassiveAttackBuff(PlayableCard target) => target == Card ? ActivationsThisTurn : 0;
    }
}
