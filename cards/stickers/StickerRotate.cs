using DiskCardGame;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Renderer))]
    public class StickerRotate : AlternateInputInteractable
    {
        public override CursorType CursorType => CursorType.Rotate;

        private static Vector3 ROTATION_AXIS = Vector3.up;
        private static Vector3 FORWARD_REFERENCE = Vector3.forward;

        public StickerDrag Dragger => this.GetComponent<StickerDrag>();

        public string StickerName => this.Dragger?.StickerName;

        private Vector3? StartingDirectionalVector { get; set; } = null;
        private Vector3? StartingEulerAngles { get; set; } = null;

        private Vector3 CursorWorldPoint
        {
            get
            {
                var screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                return Camera.main.ScreenToWorldPoint(new(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }
        }

        private Vector3 CurrentDirectionalVector
        {
            get
            {
                var retval = CursorWorldPoint - this.transform.position;
                retval.y = 0;
                return retval;
            }
        }

        float SignedAngleBetween(Vector3 a, Vector3 b)
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

            if (!this.StartingDirectionalVector.HasValue)
                return;

            if (!InputButtons.GetButton(Button.AltSelect))
            {
                P03Plugin.Log.LogInfo("Ending because alt select is not down");
                OnAlternateSelectEnded();
                return;
            }

            this.transform.eulerAngles = StartingEulerAngles.Value;
            float angle = SignedAngleBetween(this.StartingDirectionalVector.Value, this.CurrentDirectionalVector);
            P03Plugin.Log.LogInfo($"Rotation angle is {angle}");
            this.transform.Rotate(0f, angle, 0f, Space.World);
        }

        public override void OnAlternateSelectStarted()
        {
            StartingDirectionalVector = CurrentDirectionalVector;
            StartingEulerAngles = this.transform.eulerAngles;
            P03Plugin.Log.LogInfo($"Starting direction: {StartingDirectionalVector}, Angles {StartingEulerAngles}");
        }

        public override void OnAlternateSelectEnded()
        {
            StartingDirectionalVector = null;
            StartingEulerAngles = null;

            SelectableCard attachedCard = this.Dragger.LastHoverCard;
            if (attachedCard == null)
            {
                Stickers.UpdateStickerRotation(null, this.StickerName, null);
            }
            else
            {
                Stickers.UpdateStickerRotation(attachedCard.Info, this.StickerName, this.transform.localEulerAngles);
            }
        }
    }
}