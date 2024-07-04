using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03SigilLibrary.Helpers
{
    public static class CardExtensions
    {
        public static bool EligibleForGemBonus(this PlayableCard card, GemType gem) => card != null && GameFlowManager.IsCardBattle && (card.OpponentCard ? OpponentGemsManager.Instance.HasGem(gem) : ResourcesManager.Instance.HasGem(gem));
    }
}