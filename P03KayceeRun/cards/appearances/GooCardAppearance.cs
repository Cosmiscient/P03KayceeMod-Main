using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GooDiscCardAppearance : DiscCardColorAppearance
    {
        public static new Appearance ID { get; private set; }

        public override Color? BorderColor => GameColors.Instance.limeGreen;

        static GooDiscCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "GooDiscCardAppearance", typeof(GooDiscCardAppearance)).Id;
        }
    }
}