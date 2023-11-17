using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class BeastNodeExtensions
    {
        /// <summary>
        /// Sets this card to be a possible choice in the beast node
        /// </summary>
        /// <param name="healthChange">The increase/decrease in health</param>
        /// <param name="energyChange">The increase/decrease in energy cost</param>
        public static void SetNewBeastTransformer(this CardInfo card, int healthChange = 0, int energyChange = 0)
        {
            card.AddMetaCategories(CustomCards.NewBeastTransformers);
            card.temple = CardTemple.Tech;

            AscensionTransformerNew.beastInfoList.Add(card.name, healthChange, energyChange);
        }
    }
}