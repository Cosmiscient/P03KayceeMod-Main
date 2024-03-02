using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class OnboardHoloPortrait : CardAppearanceBehaviour
    {
        private bool portraitSpawned = false;

        public static Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
            if (Card.Anim is DiskCardAnimationController dcac && Card is PlayableCard pCard && pCard.OnBoard && !portraitSpawned)
            {
                dcac.SpawnHoloPortrait(Card.Info.holoPortraitPrefab);
                Card.renderInfo.hidePortrait = true;
                portraitSpawned = true;
            }
        }

        public override void OnPreRenderCard() => ApplyAppearance();

        static OnboardHoloPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "OnboardHoloPortrait", typeof(OnboardHoloPortrait)).Id;
        }
    }
}