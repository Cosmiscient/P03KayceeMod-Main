using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GooDiscCardAppearance : DiscCardColorAppearance
    {
        public static new Appearance ID { get; private set; }

        public override Color? BorderColor => GameColors.Instance.darkLimeGreen;
        public override Color? PortraitColor => GameColors.Instance.darkLimeGreen;
        public override Color? DefaultAbilityColor => GameColors.Instance.darkLimeGreen;
        public override Color? AttackColor => GameColors.Instance.darkLimeGreen;
        public override Color? HealthColor => GameColors.Instance.darkLimeGreen;
        public override Color? EnergyColor => GameColors.Instance.darkLimeGreen;

        static GooDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "GooDiscCardAppearance", typeof(GooDiscCardAppearance)).Id;
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            if (Card is PlayableCard pCard && pCard.OnBoard)
                Card.RenderInfo.hidePortrait = true;
        }

        public override void ApplyAppearance()
        {
            base.ApplyAppearance();
            if (Card is PlayableCard pCard && pCard.OnBoard)
                Card.RenderInfo.hidePortrait = true;
        }
    }
}