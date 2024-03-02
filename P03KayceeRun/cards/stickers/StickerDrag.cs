using System.Collections.Generic;
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

        private readonly List<Bounds> CardBounds = new();
        private readonly List<SelectableCard> Cards = new();
        private SelectableCard LastHoverCard { get; set; }

        [SerializeField]
        public float yOffset = 0f;

        [SerializeField]
        public string StickerName;

        public GameObject StenciledProjector;
        public GameObject UnStenciledProjector;

        private bool isDragging = false;
        private bool hasInitialized = false;

        public override int ExecutionOrder => -100;

        protected new void OnEnable() => base.OnEnable();

        internal void Initialize()
        {
            bool attachedToCard = transform.parent.gameObject.GetComponent<DiskRenderStatsLayer>() != null;
            StenciledProjector.SetActive(attachedToCard);
            UnStenciledProjector.SetActive(!attachedToCard);
            hasInitialized = true;
        }

        public override CursorType CursorType => CursorType.Pickup;

        private Vector3 CursorWorldPoint
        {
            get
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
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

            offset = gameObject.transform.position - CursorWorldPoint;

            isDragging = true;
        }

        internal static bool IsOverCardBounds(Bounds b, Vector3 p) => p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z;

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

            if (!hasInitialized && transform.parent != null)
                Initialize();

            if (!isDragging)
                return;

            Vector3 curPosition = CursorWorldPoint + offset;

            LastHoverCard = null;
            curPosition.y = StickerInterfaceManager.STICKER_HOME_POSITION.y + StickerInterfaceManager.Instance.transform.position.y;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (IsOverCardBounds(CardBounds[i], CursorWorldPoint))
                {
                    LastHoverCard = Cards[i];
                    curPosition.y = Cards[i].StatsLayer.transform.position.y + yOffset;
                    continue;
                }
            }

            if (LastHoverCard == null)
            {
                UnStenciledProjector.SetActive(true);
                StenciledProjector.SetActive(false);

                if (gameObject.transform.parent != StickerInterfaceManager.Instance.transform)
                    gameObject.transform.SetParent(StickerInterfaceManager.Instance.transform);
            }
            else
            {
                UnStenciledProjector.SetActive(false);
                StenciledProjector.SetActive(true);

                if (gameObject.transform.parent.gameObject != LastHoverCard.StatsLayer.gameObject)
                {
                    gameObject.transform.SetParent(LastHoverCard.StatsLayer.gameObject.transform);

                }
            }

            transform.localPosition = transform.parent.InverseTransformPoint(curPosition);
        }

        public override void OnCursorSelectEnd()
        {
            base.OnCursorSelectEnd();

            if (LastHoverCard == null)
            {
                if (gameObject == StickerInterfaceManager.Instance.LastPrintedSticker)
                {
                    Vector3 position = transform.position;
                    transform.SetParent(StickerInterfaceManager.Instance.transform);
                    transform.localPosition = transform.parent.InverseTransformPoint(position);
                    Tween.LocalPosition(transform, StickerInterfaceManager.STICKER_HOME_POSITION, 0.1f, 0f);
                }
                else
                {
                    Tween.Position(transform, transform.position + new Vector3(0f, -2f, 0f), 0.2f, 0f, completeCallback: () => Destroy(gameObject));
                }
                Stickers.ClearStickerAppearance(StickerName);
            }
            else
            {
                LastHoverCard.Info.UpdateStickerPosition(StickerName, transform.localPosition);
            }

            isDragging = false;
        }
    }
}