using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class OnboardHoloPortrait : CardAppearanceBehaviour
    {
        private bool portraitSpawned = false;

        public static Appearance ID { get; private set; }

        public bool HoloPortraitSpawned => Card.Anim is DiskCardAnimationController dcac && dcac.holoPortraitParent.childCount > 0 && dcac.holoPortraitParent.GetChild(0).gameObject.active;

        public override void ApplyAppearance()
        {
            if (Card.Anim is DiskCardAnimationController dcac && Card is PlayableCard pCard && pCard.OnBoard && !portraitSpawned)
            {
                dcac.SpawnHoloPortrait(Card.Info.holoPortraitPrefab);
                Card.renderInfo.hidePortrait = true;
                portraitSpawned = true;
            }

            if (Card.Info.holoPortraitPrefab == null)
            {
                Card.renderInfo.hidePortrait = false;
            }
            else
            {
                Card.renderInfo.hidePortrait = HoloPortraitSpawned;
            }
        }

        public override void OnPreRenderCard() => ApplyAppearance();

        static OnboardHoloPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "OnboardHoloPortrait", typeof(OnboardHoloPortrait)).Id;
        }
    }
}