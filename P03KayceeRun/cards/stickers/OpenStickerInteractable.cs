using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    internal class OpenStickerInteractable : MainInputInteractable
    {
        internal Part3DeckReviewSequencer deckReviewSequencer => this.gameObject.GetComponentInParent<Part3DeckReviewSequencer>();

        public override CursorType CursorType => CursorType.Pickup;

        public override void OnCursorSelectStart()
        {
            base.OnCursorSelectStart();
            Stickers.OnStickerBookClicked(deckReviewSequencer);
        }
    }
}