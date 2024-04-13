using System.Collections;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public static class ElevatorArrowPatches
    {
        public static readonly LookDirection ELEVATOR_UP = (LookDirection)2000;
        public static readonly LookDirection ELEVATOR_DOWN = (LookDirection)3000;
        public static readonly LookDirection MIRROR_UP = (LookDirection)4000;

        private static Animator HatchAnim
        {
            get
            {
                return HoloGameMap.Instance.ScreenCamera.CurrentAreaBackground.GetComponentInChildren<Animator>();
            }
        }

        private static GameObject Elevator
        {
            get
            {
                return HoloMapAreaManager.Instance.CurrentArea.transform.Find("Scenery/LiftPillar")?.gameObject
                    ?? HoloMapAreaManager.Instance.CurrentArea.transform.Find("Scenery/LiftPillar(Clone)")?.gameObject;
            }
        }

        [HarmonyPatch(typeof(MoveHoloMapAreaNode), nameof(MoveHoloMapAreaNode.OnCursorSelectEnd))]
        [HarmonyPrefix]
        private static bool ArrowRunsElevator(MoveHoloMapAreaNode __instance)
        {
            if (!HoloMapAreaManager.Instance.MovingAreas && HoloGameMap.Instance.FullyUnrolled)
            {
                if (__instance.direction == ELEVATOR_UP)
                {
                    CustomCoroutine.Instance.StartCoroutine(TakeElevatorUp());
                    return false;
                }
                if (__instance.direction == ELEVATOR_DOWN)
                {
                    CustomCoroutine.Instance.StartCoroutine(TakeElevatorDown());
                    return false;
                }
                if (__instance.direction == MIRROR_UP)
                {
                    CustomCoroutine.Instance.StartCoroutine(MirrorSequence());
                    return false;
                }
            }
            return true;
        }

        private static void ActionByName(MainInputInteractable mii)
        {
            string name = mii.gameObject.name;
            bool forward = name.Contains("Right");
            if (name.Contains("Arms"))
                CompositeFigurineManager.RotateArms(forward);
            if (name.Contains("Body"))
                CompositeFigurineManager.RotateBody(forward);
            if (name.Contains("Head"))
                CompositeFigurineManager.RotateHead(forward);
        }

        private static IEnumerator MirrorSequence()
        {
            HoloMapAreaManager.Instance.CurrentArea.SetNodesHidden(true);
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            GameFlowManager.Instance.Transitioning = true;
            float time = 0.4f;

            Tween.Position(PlayerMarker.Instance.transform, PlayerMarker.Instance.transform.position + Vector3.forward * 1.5f, time, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Tween.LocalRotation(PlayerMarker.Instance.transform, new Vector3(0f, 180f, 0f), time, 0f);
            Vector3 cameraLocationOffset = new Vector3(0.38f, 7.7f, -2.6f) - ViewManager.GetViewInfo(ViewManager.Instance.CurrentView).camPosition;
            Vector3 cameraTargetRotation = new Vector3(24.5f, 0f, 0f) - ViewManager.GetViewInfo(ViewManager.Instance.CurrentView).camRotation;
            ViewManager.Instance.OffsetPosition(cameraLocationOffset, time);
            ViewManager.Instance.OffsetRotation(cameraTargetRotation, time);
            yield return new WaitForSeconds(time);

            var nodes = HoloMapAreaManager.Instance.CurrentArea.transform.Find("Nodes");
            for (int i = 0; i < nodes.childCount; i++)
            {
                if (nodes.GetChild(i).gameObject.name.StartsWith("Figurine"))
                {
                    nodes.GetChild(i).gameObject.SetActive(true);
                    nodes.GetChild(i).GetComponent<MainInputInteractable>().CursorSelectEnded = ActionByName;
                }
            }

            yield return new WaitUntil(() => InputButtons.GetButton(Button.LookDown));

            Tween.Position(PlayerMarker.Instance.transform, PlayerMarker.Instance.transform.position - Vector3.forward * 1.5f, time, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            ViewManager.Instance.OffsetPosition(Vector3.zero, time);
            ViewManager.Instance.OffsetRotation(Vector3.zero, time);

            yield return new WaitForSeconds(time);

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            GameFlowManager.Instance.Transitioning = false;
            for (int i = 0; i < nodes.childCount; i++)
                if (nodes.GetChild(i).gameObject.name.StartsWith("Figurine"))
                    nodes.GetChild(i).gameObject.SetActive(true);
            HoloMapAreaManager.Instance.CurrentArea.SetNodesHidden(false);
        }

        private static IEnumerator TakeElevatorUp()
        {
            HoloMapAreaManager.Instance.CurrentArea.HideDirectionNodes();
            if (!Part3SaveData.Data.techElevatorOn)
            {
                yield return new WaitForSeconds(1f);
                if (Elevator != null)
                {
                    AudioController.Instance.PlaySound3D("holomap_waypoint_activate", MixerGroup.ExplorationSFX, Elevator.transform.position, 1f, 0f, null, null, null, null, false);
                    Elevator?.SetActive(true);
                }
                HoloGameMap.Instance.Jump();
                yield return new WaitForSeconds(1f);
                Part3SaveData.Data.techElevatorOn = true;
            }
            else
            {
                Elevator?.SetActive(true);
                yield return new WaitForSeconds(0.5f);
            }
            Tween.Position(PlayerMarker.Instance.transform, PlayerMarker.Instance.transform.position + Vector3.up * 10f, 1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.25f);
            CustomCoroutine.Instance.StartCoroutine(WaitThenMoveToNewArea(true));
        }

        private static IEnumerator TakeElevatorDown()
        {
            HoloMapAreaManager.Instance.CurrentArea.HideDirectionNodes();
            HatchAnim?.Play("open", 0, 0f);
            yield return new WaitForSeconds(0.2f);
            Tween.Position(PlayerMarker.Instance.transform, PlayerMarker.Instance.transform.position + Vector3.down * 1f, 0.5f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.1f);
            yield return WaitThenMoveToNewArea(false);
            Vector3 position = PlayerMarker.Instance.transform.position;
            PlayerMarker.Instance.transform.position += Vector3.up * 6f;
            Tween.Position(PlayerMarker.Instance.transform, position, 0.75f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            yield break;
        }

        private static IEnumerator WaitThenMoveToNewArea(bool goingUp)
        {
            yield return new WaitWhile(() => HoloMapAreaManager.Instance.MovingAreas);
            HoloMapAreaManager.Instance.CurrentArea.HideDirectionNodes();
            Tween.Cancel(PlayerMarker.Instance.transform.GetInstanceID());

            Part3SaveData.WorldPosition targetPos = new(
                HoloMapAreaManager.Instance.CurrentWorld.Id,
                HoloMapAreaManager.Instance.CurrentArea.GridX,
                goingUp ? HoloMapAreaManager.Instance.CurrentArea.GridY - 1 : HoloMapAreaManager.Instance.CurrentArea.GridY + 1
            );

            HoloMapAreaManager.Instance.MoveToAreaDirectly(targetPos);
            PlayerMarker.Instance.transform.position = HoloMapAreaManager.Instance.CurrentArea.CenterPosition;
            yield break;
        }
    }

    // public abstract class AscensionElevatorBase : HoloAreaSpecialSequencer
    // {
    //     protected Animator HatchAnim
    //     {
    //         get
    //         {
    //             return HoloGameMap.Instance.ScreenCamera.CurrentAreaBackground.GetComponentInChildren<Animator>();
    //         }
    //     }



    //     protected abstract bool GoingUp { get; }
    // }

    // public class AscensionElevatorDown : AscensionElevatorBase
    // {
    //     protected override bool GoingUp => false;

    //     public override IEnumerator PreEnteredSequence()
    //     {
    //         base.GetComponent<HoloMapArea>().HideDirectionNodes();
    //         CustomCoroutine.Instance.StartCoroutine(this.TakeElevatorDown());
    //         yield break;
    //     }

    //     private IEnumerator TakeElevatorDown()
    //     {
    //         base.HatchAnim.Play("open", 0, 0f);
    //         yield return new WaitForSeconds(0.2f);
    //         Tween.Position(PlayerMarker.Instance.transform, PlayerMarker.Instance.transform.position + Vector3.down * 1f, 0.5f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
    //         yield return new WaitForSeconds(0.1f);
    //         yield return base.WaitThenMoveToNewArea();
    //         Vector3 position = PlayerMarker.Instance.transform.position;
    //         PlayerMarker.Instance.transform.position += Vector3.up * 6f;
    //         Tween.Position(PlayerMarker.Instance.transform, position, 0.75f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
    //         yield break;
    //     }
    // }

    // public class AscensionElevatorUp : AscensionElevatorBase
    // {
    //     [SerializeField]
    //     internal GameObject elevatorBeam;

    //     protected override bool GoingUp => true;

    //     public override IEnumerator PreEnteredSequence()
    //     {
    //         base.GetComponent<HoloMapArea>().HideDirectionNodes();
    //         if (!Part3SaveData.Data.techElevatorOn)
    //         {
    //             yield return new WaitForSeconds(1f);
    //             AudioController.Instance.PlaySound3D("holomap_waypoint_activate", MixerGroup.ExplorationSFX, base.transform.position, 1f, 0f, null, null, null, null, false);
    //             elevatorBeam?.SetActive(true);
    //             HoloGameMap.Instance.Jump();
    //             yield return new WaitForSeconds(1f);
    //             Part3SaveData.Data.techElevatorOn = true;
    //         }
    //         else
    //         {
    //             elevatorBeam?.SetActive(true);
    //             yield return new WaitForSeconds(0.5f);
    //         }
    //         Tween.Position(PlayerMarker.Instance.transform, PlayerMarker.Instance.transform.position + Vector3.up * 10f, 1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
    //         yield return new WaitForSeconds(0.25f);
    //         CustomCoroutine.Instance.StartCoroutine(this.WaitThenMoveToNewArea());
    //     }
    // }
}