using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class EnergyConduitAppearnace : CardAppearanceBehaviour
    {
        public static Appearance ID { get; private set; }

        private static readonly Sprite[] PORTRAITS = new Sprite[] {
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy_1.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy_2.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(TextureHelper.GetImageAsTexture("portrait_conduitenergy_3.png", typeof(EnergyConduitAppearnace).Assembly), TextureHelper.SpriteType.CardPortrait)
        };

        // This is a bit of a hack to deal with an infinite loop that can happen with this appearance
        private int renderStackSize = 0;

        public override void ApplyAppearance()
        {
            if (Card is PlayableCard pCard)
            {
                if (pCard.slot == null)
                    return;

                NewConduitEnergy behaviour = Card.GetComponent<NewConduitEnergy>();
                if (behaviour == null)
                    return;

                renderStackSize += 1;

                if (renderStackSize <= 2)
                {
                    pCard.renderInfo.portraitOverride = !behaviour.CompletesCircuit() ? PORTRAITS[0] : PORTRAITS[behaviour.RemainingEnergy];
                }

                renderStackSize -= 1;
            }
        }

        public override void OnPreRenderCard() => ApplyAppearance();

        static EnergyConduitAppearnace()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "EnergyConduitAppearance", typeof(EnergyConduitAppearnace)).Id;
        }
    }
}