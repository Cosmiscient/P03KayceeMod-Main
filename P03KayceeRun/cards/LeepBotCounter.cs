using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class LeepBotCounter : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID => SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "LeepBotCounter", typeof(LeepBotCounter)).Id;

        public override bool RespondsToTakeDamage(PlayableCard source) => !PlayableCard.OpponentCard && source != null;

        public override IEnumerator OnTakeDamage(PlayableCard source)
        {
            DefaultQuestDefinitions.LeapBotNeo.IncrementQuestCounter();
            yield break;
        }
    }
}