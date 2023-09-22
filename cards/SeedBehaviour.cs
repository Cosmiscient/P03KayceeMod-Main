using System.Collections;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class SeedBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID => SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "SeedBehaviour", typeof(SeedBehaviour)).Id;

        private int triggerPriority;
        public override int Priority => triggerPriority;

        public override bool RespondsToUpkeep(bool playerUpkeep) => PlayableCard.OpponentCard != playerUpkeep && PlayableCard.FaceDown;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return OnTurnEnd(!PlayableCard.OpponentCard);
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            ViewManager.Instance.SwitchToView(View.Board, false, true);
            yield return new WaitForSeconds(0.15f);
            PlayableCard.SetFaceDown(false, false);
            PlayableCard.UpdateFaceUpOnBoardEffects();

            CardInfo newCard = CardLoader.GetCardByName("Tree_Hologram");
            newCard.mods = new(Card.Info.Mods.Select(m => (CardModificationInfo)m.Clone()));
            Card.SetInfo(newCard);

            yield return new WaitForSeconds(0.3f);
            triggerPriority = int.MinValue;
            yield break;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => PlayableCard.OpponentCard != playerTurnEnd && !PlayableCard.FaceDown;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            ViewManager.Instance.SwitchToView(View.Board, false, true);
            yield return new WaitForSeconds(0.15f);
            Card.SetCardbackSubmerged();
            Card.SetFaceDown(true, false);
            yield return new WaitForSeconds(0.3f);
            triggerPriority = int.MaxValue;
            yield break;
        }
    }
}