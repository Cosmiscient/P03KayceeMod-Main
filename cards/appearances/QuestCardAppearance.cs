using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class QuestCardAppearance : EmissiveDiscBorderBase
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public override Color EmissionColor { get { return GameColors.Instance.darkBlue; } }

        static QuestCardAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "QuestCardAppearance", typeof(QuestCardAppearance)).Id;
        }
    }
}