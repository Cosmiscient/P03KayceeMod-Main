using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class HighResAlternatePortrait : CardAppearanceBehaviour
    {
        private static Sprite TributeSprite = TextureHelper.GetImageAsTexture("portrait_poodle_photo.png", typeof(CustomCards).Assembly, FilterMode.Trilinear).ConvertTexture(TextureHelper.SpriteType.CardPortrait, FilterMode.Trilinear);

        public static Appearance ID { get; private set; }

        public class DynamicPortrait : DynamicCardPortrait
        {
            public override void ApplyCardInfo(CardInfo card)
            {
                SpriteRenderer renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
                if (card.name.EndsWith("Poodle"))
                    renderer.sprite = TributeSprite;
                else
                    renderer.sprite = card.alternatePortrait;
            }
        }

        private static GameObject prefabPortrait = null;

        internal static GameObject CloneSpecialPortrait()
        {
            CardInfo mole = CardLoader.GetCardByName("Mole_Telegrapher");
            GameObject myObj = Instantiate(mole.AnimatedPortrait);
            SpriteRenderer rend = myObj.GetComponentInChildren<SpriteRenderer>();
            myObj.AddComponent<DynamicPortrait>();
            return myObj;
        }

        public override void ApplyAppearance()
        {
            if (prefabPortrait == null)
            {
                prefabPortrait = CloneSpecialPortrait();
            }

            Card.RenderInfo.prefabPortrait = prefabPortrait;
            Card.RenderInfo.hidePortrait = true;
            Card.renderInfo.hiddenCost = true;
        }

        static HighResAlternatePortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "HighResAlternatePortrait", typeof(HighResAlternatePortrait)).Id;
        }
    }
}