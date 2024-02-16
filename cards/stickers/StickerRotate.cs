using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Renderer))]
    public class StickerRotate : AlternateInputInteractable
    {
        private readonly List<Bounds> CardBounds = new();
        private readonly List<SelectableCard> Cards = new();
        public SelectableCard LastHoverCard { get; private set; }

        public override CursorType CursorType => CursorType.Rotate;

        private static Vector3 ROTATION_AXIS = Vector3.up;
        private static Vector3 FORWARD_REFERENCE = Vector3.forward;

        public StickerDrag Dragger => GetComponent<StickerDrag>();

        public string StickerName => Dragger?.StickerName;

        private Vector3? StartingDirectionalVector { get; set; } = null;
        private Vector3? StartingEulerAngles { get; set; } = null;

        private Vector3 CursorWorldPoint
        {
            get
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                return Camera.main.ScreenToWorldPoint(new(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }
        }

        private Vector3 CurrentDirectionalVector
        {
            get
            {
                Vector3 retval = CursorWorldPoint - transform.position;
                retval.y = 0;
                return retval;
            }
        }

        private float SignedAngleBetween(Vector3 a, Vector3 b)
        {
            // https://stackoverflow.com/questions/19675676/calculating-actual-angle-between-two-vectors-in-unity3d
            // angle in [0,180]
            float angle = Vector3.Angle(a, b);
            float sign = Mathf.Sign(Vector3.Dot(ROTATION_AXIS, Vector3.Cross(a, b)));

            // angle in [-179,180]
            float signed_angle = angle * sign;

            // angle in [0,360] (not used but included here for completeness)
            //float angle360 =  (signed_angle + 180) % 360;

            return signed_angle;
        }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (!StartingDirectionalVector.HasValue)
                return;

            LastHoverCard = null;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (StickerDrag.IsOverCardBounds(CardBounds[i], CursorWorldPoint))
                {
                    LastHoverCard = Cards[i];
                    continue;
                }
            }

            if (!InputButtons.GetButton(Button.AltSelect))
            {
                P03Plugin.Log.LogInfo("Ending because alt select is not down");
                OnAlternateSelectEnded();
                return;
            }

            transform.eulerAngles = StartingEulerAngles.Value;
            float angle = SignedAngleBetween(StartingDirectionalVector.Value, CurrentDirectionalVector);
            P03Plugin.Log.LogInfo($"Rotation angle is {angle}");
            transform.Rotate(0f, angle, 0f, Space.World);
        }

        public override void OnAlternateSelectStarted()
        {
            StartingDirectionalVector = CurrentDirectionalVector;
            StartingEulerAngles = transform.eulerAngles;
            P03Plugin.Log.LogInfo($"Starting direction: {StartingDirectionalVector}, Angles {StartingEulerAngles}");

            if (DeckReviewSequencer.Instance != null
                && DeckReviewSequencer.Instance is Part3DeckReviewSequencer
                && StickerInterfaceManager.Instance.LastDisplayedCard != null)
            {
                CardBounds.Clear();
                Cards.Clear();

                CardBounds.Add(CardExporter.GetMaxBounds(StickerInterfaceManager.Instance.LastDisplayedCard.gameObject));
                Cards.Add(StickerInterfaceManager.Instance.LastDisplayedCard);
            }
        }

        public override void OnAlternateSelectEnded()
        {
            StartingDirectionalVector = null;
            StartingEulerAngles = null;

            SelectableCard attachedCard = LastHoverCard;
            if (attachedCard == null)
            {
                Stickers.ClearStickerAppearance(StickerName);
            }
            else
            {
                attachedCard.Info.UpdateStickerRotation(StickerName, transform.localEulerAngles);
            }
        }
    }
}