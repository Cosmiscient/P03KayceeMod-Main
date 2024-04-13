using System.Collections;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class DummyBreak : SpecialCardBehaviour
    {
        public static readonly SpecialTriggeredAbility ID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "DummyBreak", typeof(DummyBreak)).Id;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => !wasSacrifice && killer != null && !Part3SaveData.Data.deck.Cards.Any(ci => ci.name.Equals("BlueMage_Talking", System.StringComparison.InvariantCultureIgnoreCase));

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            ViewManager.Instance.SwitchToView(View.Default);
            Part3SaveData.Data.deck.RemoveCardByName(PlayableCard.Info.name);
            yield return QuestRewardCard.ImmediateReward("BlueMage_Talking");
            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName("BlueMage_Talking"), null);
            yield return new WaitForSeconds(0.45f);
        }
    }
}