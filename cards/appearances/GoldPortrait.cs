using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GoldPortrait : CardAppearanceBehaviour
    {
        public static Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
            Card.RenderInfo.portraitColor = GameColors.Instance.gold;

            if (Card.StatsLayer is DiskRenderStatsLayer drsl)
                drsl.lightColor = GameColors.Instance.gold;
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            ApplyAppearance();
        }

        static GoldPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "GoldPortrait", typeof(GoldPortrait)).Id;
        }
    }
}