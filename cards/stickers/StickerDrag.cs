using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Renderer))]
    public class StickerDrag : MainInputInteractable
    {
        private Vector3 offset;

        private List<Bounds> CardBounds = new();
        private List<SelectableCard> Cards = new();
        public SelectableCard LastHoverCard { get; private set; }

        [SerializeField]
        public float yOffset = 0f;

        [SerializeField]
        public string StickerName;

        private bool isDragging = false;

        public override int ExecutionOrder => -100;

        protected new void OnEnable()
        {
            base.OnEnable();
        }

        public override CursorType CursorType => CursorType.Pickup;

        private Vector3 CursorWorldPoint
        {
            get
            {
                var screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                return Camera.main.ScreenToWorldPoint(new(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }
        }

        public override void OnCursorSelectStart()
        {
            base.OnCursorSelectStart();

            if (DeckReviewSequencer.Instance != null
                && DeckReviewSequencer.Instance is Part3DeckReviewSequencer
                && StickerInterfaceManager.Instance.LastDisplayedCard != null)
            {
                CardBounds.Clear();
                Cards.Clear();

                CardBounds.Add(CardExporter.GetMaxBounds(StickerInterfaceManager.Instance.LastDisplayedCard.gameObject));
                Cards.Add(StickerInterfaceManager.Instance.LastDisplayedCard);
            }

            offset = gameObject.transform.position - this.CursorWorldPoint;

            this.isDragging = true;
        }

        private bool IsOverCardBounds(Bounds b, Vector3 p)
        {
            return (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z);
        }

        // public override void OnCursorDrag()
        // {
        //     base.OnCursorDrag();
        //     this.OnCursorStay();
        // }

        // public override void OnCursorExit()
        // {
        //     base.OnCursorExit();
        //     this.OnCursorStay();
        // }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (!this.isDragging)
                return;

            Vector3 curPosition = this.CursorWorldPoint + offset;

            LastHoverCard = null;
            curPosition.y = StickerInterfaceManager.STICKER_HOME_POSITION.y + StickerInterfaceManager.Instance.transform.position.y;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (IsOverCardBounds(CardBounds[i], this.CursorWorldPoint))
                {
                    LastHoverCard = Cards[i];
                    curPosition.y = Cards[i].StatsLayer.transform.position.y + yOffset;
                    continue;
                }
            }

            if (LastHoverCard == null)
            {
                if (gameObject.transform.parent != StickerInterfaceManager.Instance.transform)
                    gameObject.transform.SetParent(StickerInterfaceManager.Instance.transform);
            }
            else
            {
                if (gameObject.transform.parent.gameObject != LastHoverCard.StatsLayer.gameObject)
                    gameObject.transform.SetParent(LastHoverCard.StatsLayer.gameObject.transform);
            }

            transform.localPosition = transform.parent.InverseTransformPoint(curPosition);
        }

        public override void OnCursorSelectEnd()
        {
            base.OnCursorSelectEnd();

            if (LastHoverCard == null)
            {
                if (this.gameObject == StickerInterfaceManager.Instance.LastPrintedSticker)
                {
                    Vector3 position = this.transform.position;
                    this.transform.SetParent(StickerInterfaceManager.Instance.transform);
                    this.transform.localPosition = this.transform.parent.InverseTransformPoint(position);
                    Tween.LocalPosition(this.transform, StickerInterfaceManager.STICKER_HOME_POSITION, 0.1f, 0f);
                }
                else
                {
                    Tween.Position(this.transform, this.transform.position + new Vector3(0f, -2f, 0f), 0.2f, 0f, completeCallback: () => GameObject.Destroy(this.gameObject));
                }
                Stickers.UpdateStickerPosition(null, this.StickerName, null);
            }
            else
            {
                Stickers.UpdateStickerPosition(LastHoverCard.Info, this.StickerName, this.transform.localPosition);
            }

            this.isDragging = false;
        }
    }
}