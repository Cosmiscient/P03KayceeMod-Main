using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RerenderOnBoard : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        public override int Priority => 100000;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            Card.SetInfo(Card.Info);
            yield break;
        }

        static RerenderOnBoard()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "RerenderOnBoard", typeof(RerenderOnBoard)).Id;
        }
    }
}