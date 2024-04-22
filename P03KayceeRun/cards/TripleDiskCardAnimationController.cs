using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class TripleDiskCardAnimationController : DiskCardAnimationController
    {
        [HarmonyPatch(typeof(CardAnimationController3D), nameof(Awake))]
        [HarmonyPrefix]
        private static bool StopAwake(ref CardAnimationController3D __instance) => __instance is not TripleDiskCardAnimationController;

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(Expand))]
        [HarmonyPrefix]
        private static bool NewExpand(ref DiskCardAnimationController __instance, bool immediate = false)
        {
            if (__instance is TripleDiskCardAnimationController tdcac)
            {
                __instance.ShowScreenOnKeyframe();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(Contract))]
        [HarmonyPrefix]
        private static bool NewContract(ref DiskCardAnimationController __instance)
        {
            return __instance is TripleDiskCardAnimationController
                ? throw new InvalidOperationException("A card with the TripleDiskCardAnimationController should never be asked to contract")
                : true;
        }

        private readonly List<TweenBase> activeTweens = new();
        private readonly bool windupComplete = true;

        [HarmonyPatch(typeof(CardAnimationController), nameof(SetAnimationPaused))]
        [HarmonyPrefix]
        private static bool TweenAnimationPause(ref CardAnimationController __instance, bool paused)
        {
            if (__instance is TripleDiskCardAnimationController tdcac)
            {
                if (paused)
                {
                    foreach (TweenBase item in tdcac.activeTweens)
                        item.Stop();
                }

                if (!paused)
                {
                    foreach (TweenBase item in tdcac.activeTweens)
                        item.Resume();
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(ShowWeaponAnim))]
        [HarmonyPrefix]
        private static bool DontShowWeaponAnim(ref DiskCardAnimationController __instance) => __instance is not TripleDiskCardAnimationController;

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(HideWeaponAnim))]
        [HarmonyPrefix]
        private static bool DontHideWeaponAnim(ref DiskCardAnimationController __instance) => __instance is not TripleDiskCardAnimationController;

        private bool attacking = false;
        public override void PlayAttackAnimation(bool attackPlayer, CardSlot targetSlot)
        {
            DoingAttackAnimation = true;
            Vector3 targetSpot = targetSlot.transform.position;

            GameObject mushroomContainer = new("mushroomContainer");
            mushroomContainer.transform.SetParent(gameObject.transform);
            mushroomContainer.transform.localPosition = Vector3.zero;
            GameObject mushroom = Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloMushroom_1"), mushroomContainer.transform);
            OnboardDynamicHoloPortrait.HolofyGameObject(mushroom, GameColors.Instance.brightLimeGreen);
            mushroom.transform.localPosition = Vector3.zero;
            AudioController.Instance.PlaySound3D("mushroom_large_hit", MixerGroup.TableObjectsSFX, mushroom.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            void callback()
            {
                DoingAttackAnimation = false;
                AudioController.Instance.PlaySound3D("small_mushroom_hit", MixerGroup.TableObjectsSFX, mushroom.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
                impactKeyframeCallback?.Invoke();
                Destroy(mushroomContainer, 0.35f);
                activeTweens.Clear();
                attacking = false;
            }

            attacking = true;
            activeTweens.Add(Tween.LocalPosition(mushroom.transform, Vector3.up * 1.5f, 0.2f, 0f, Tween.EaseOut));
            activeTweens.Add(Tween.LocalPosition(mushroom.transform, Vector3.zero, 0.5f, 0.2f, Tween.EaseIn, completeCallback: callback));
            activeTweens.Add(Tween.LocalRotation(mushroom.transform, new Vector3(180f, 200f, 180f), .4f, 0f));
            activeTweens.Add(Tween.Position(mushroomContainer.transform, targetSpot, .4f, 0f));
        }

        private static IEnumerator UnrollWithWait(IEnumerator sequence, TripleDiskCardAnimationController controller)
        {
            while (sequence.MoveNext())
            {
                if (sequence.Current is IEnumerator ies)
                {
                    yield return UnrollWithWait(ies, controller);
                    continue;
                }

                if (sequence.Current is WaitForSeconds wfs && wfs.m_Seconds == 0.05f)
                    yield return new WaitUntil(() => !controller.attacking);
                else
                    yield return sequence.Current;
            }
        }

        [HarmonyPatch(typeof(CombatPhaseManager3D), nameof(CombatPhaseManager3D.InitializePhase))]
        [HarmonyPrefix]
        private static void DoCombatPhaseLogger(List<CardSlot> attackingSlots, bool playerIsAttacker)
        {
            // Remove duplicates
            attackingSlots.RemoveAll(x => x.Card == null || x.Card.Slot != x);
            P03Plugin.Log.LogInfo($"Starting combat phase - player? {playerIsAttacker} - turn {TurnManager.Instance.TurnNumber}. Number of attacking slots {attackingSlots.Count}");
        }

        [HarmonyPatch(typeof(CombatPhaseManager3D), nameof(CombatPhaseManager3D.InitializePhase))]
        [HarmonyPostfix]
        private static void DoCombatPhasePostLogger(List<CardSlot> attackingSlots, bool playerIsAttacker)
        {
            attackingSlots.RemoveAll(x => x.Card == null || x.Card.Slot != x);
            P03Plugin.Log.LogInfo($"Starting combat phase - player? {playerIsAttacker} - turn {TurnManager.Instance.TurnNumber}. Number of attacking slots {attackingSlots.Count}");
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSequence))]
        [HarmonyPrefix]
        private static void DoSlotAttackLogger(CardSlot slot)
        {
            List<CardSlot> opposingSlots = slot.Card.GetOpposingSlots();
            P03Plugin.Log.LogInfo($"Starting slot attack - {slot.Card}. Number of opposing slots {opposingSlots.Count}");
        }


        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator SmarterSlotAttackSlot(IEnumerator sequence, CombatPhaseManager __instance, CardSlot attackingSlot, CardSlot opposingSlot, float waitAfter = 0f)
        {
            P03Plugin.Log.LogInfo($"Attacking card {attackingSlot.Card}. Defending card {opposingSlot.Card}");
            if (opposingSlot.Card != null)// && GoobertCenterCardBehaviour.Instance != null && opposingSlot.Card == GoobertCenterCardBehaviour.Instance)
            {
                var gcs = opposingSlot.Card.GetComponent<GoobertCenterCardBehaviour>();
                if (gcs != null)
                {
                    if (opposingSlot != gcs.PlayableCard.slot)
                    {
                        P03Plugin.Log.LogInfo("Redirecting attack to another slot");
                        yield return __instance.SlotAttackSlot(attackingSlot, gcs.PlayableCard.slot, waitAfter);
                    }
                }
            }

            if (attackingSlot.Card == null || attackingSlot.Card.Anim is not TripleDiskCardAnimationController tdcac)
            {
                P03Plugin.Log.LogInfo("Running normal attack sequence");
                yield return sequence;
                yield break;
            }

            P03Plugin.Log.LogInfo("Unrolling attack sequence by goobert");
            yield return new WaitForEndOfFrame();
            yield return UnrollWithWait(sequence, tdcac);
        }

        public override void PlayTransformAnimation() => AudioController.Instance.PlaySound3D("disk_card_transform", MixerGroup.CardPaperSFX, transform.position, 1f, 0f, null, null, null, null, false);

        public override void PlayHitAnimation()
        {
            AudioController.Instance.PlaySound3D("disk_card_hit", MixerGroup.TableObjectsSFX, transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
            Singleton<TableVisualEffectsManager>.Instance.ThumpTable(0.075f);
            StartCoroutine(FlickerScreen(0.075f));
        }

        public override void PlayDeathAnimation(bool playSound = true)
        {
            if (playSound)
            {
                AudioController.Instance.PlaySound3D("disk_card_death", MixerGroup.CardPaperSFX, transform.position, 1f, 0f, null, null, null, null, false);
            }
            PlayHitAnimation();
            StopAllCoroutines();
            StartCoroutine(Die());
            ShowScreenOffKeyframe();
        }

        private List<Renderer> lastSetOff;
        private void SetRenderers(bool visible)
        {
            if (visible && lastSetOff != null)
            {
                foreach (Renderer r in lastSetOff)
                    r.enabled = true;
            }

            if (!visible)
            {
                lastSetOff = new();
                foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
                {
                    if (r.enabled)
                    {
                        lastSetOff.Add(r);
                        r.enabled = false;
                    }
                }
            }
        }

        private IEnumerator Die()
        {
            for (float dur = 0.3f; dur > 0f; dur -= 0.25f)
            {
                SetRenderers(false);
                yield return new WaitForSeconds(dur);
                SetRenderers(true);
                yield return new WaitForSeconds(dur);
            }
            SetRenderers(false);
            gameObject.transform.localPosition += Vector3.down * 3;
        }

        public override void PlayPermaDeathAnimation(bool playSound = true)
        {
            if (playSound)
            {
                AudioController.Instance.PlaySound3D("disk_card_overload", MixerGroup.CardPaperSFX, transform.position, 1f, 0.15f, null, null, null, null, false);
            }
            PlayHitAnimation();
            StopAllCoroutines();
            StartCoroutine(Die());
            gameObject.transform.localPosition += Vector3.down * 3;
        }

        public override void SetHovering(bool hovering) => transform.localPosition = hovering ? new(0f, 0f, -.1f) : new(0f, 0f, 0f);

        public override void ExitBoard(float tweenLength, Vector3 destinationOffset) => StartCoroutine(ClearEffectsThenExit());

        public override void SetCardRendererFlipped(bool flipped) => cardRenderer.transform.localRotation = Quaternion.Euler(0f, -180f, flipped ? -90f : 90f);

        public override void NegationEffect(bool strong)
        {
            AudioController.Instance.PlaySound3D("disk_card_flicker", MixerGroup.CardPaperSFX, transform.position, 1f, 0f, null, null, null, null, false);
            StartCoroutine(FlickerScreen(0.05f));
        }

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.ClearEffectsThenExit))]
        [HarmonyPostfix]
        private static IEnumerator ClearEffectsThenExit(IEnumerator sequence, DiskCardAnimationController __instance)
        {
            if (__instance is TripleDiskCardAnimationController tdcac)
            {
                yield return tdcac.ClearLatchAbility();
                tdcac.ShowScreenOffKeyframe();
                tdcac.gameObject.transform.localPosition += Vector3.down * 3;
                yield break;
            }
            yield return sequence;
        }

        public void InitializeWith(DiskCardAnimationController controller)
        {
            anim = controller.anim;
            statsLayer = controller.statsLayer;
            cracks = controller.cracks;
            weaponAnim = controller.weaponAnim;
            weaponRenderer = controller.weaponRenderer;
            weaponMeshFilter = controller.weaponMeshFilter;
            weaponMeshes = controller.weaponMeshes;
            weaponMeshOffsets = controller.weaponMeshOffsets;
            weaponScales = controller.weaponScales;
            weaponRotations = controller.weaponRotations;
            weaponMaterials = controller.weaponMaterials;
            redHologramMaterial = controller.redHologramMaterial;
            blueHologramMaterial = controller.blueHologramMaterial;
            renderersToHologram = controller.renderersToHologram;
            disableForHologram = controller.disableForHologram;
            holoPortraitParent = controller.holoPortraitParent;
            shieldRenderer = controller.shieldRenderer;
            latchModule = controller.latchModule;
            lightningParent = controller.lightningParent;
            fuseParent = controller.fuseParent;
            toHologramRenderersDefaultMats = controller.toHologramRenderersDefaultMats;
            unusedCracks = controller.unusedCracks;
            cardRenderer = controller.cardRenderer;
            impactKeyframeCallback = controller.impactKeyframeCallback;
            sacrificeHoveringMarker = controller.sacrificeHoveringMarker;
            sacrificeMarker = controller.sacrificeMarker;
            intendedRendererYPos = cardRenderer.transform.localPosition.y;

            // Lots of wonkiness happens with what I'm doing to make the triple card work
            // This solves a problem with the screen disappearing after blinking in and out
            Transform screen = gameObject.transform.Find("Anim/CardBase/ScreenFront");
            screen.localPosition = new Vector3(screen.localPosition.x, screen.localPosition.y, 0.01f);


            anim.enabled = false;
        }
    }
}
