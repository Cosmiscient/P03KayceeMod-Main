using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.CustomRules;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class OnboardWizardCardModel : CardAppearanceBehaviour
    {
        private readonly bool portraitSpawned = false;

        public static Appearance ID { get; private set; }

        private GameObject GetPrefab()
        {
            string key = Card.Info.name;
            if (key.Equals(RandomSalmon.SalmonNames[CardTemple.Wizard]))
                return ResourceBank.Get<GameObject>("p03kcm/prefabs/koi");
            try
            {
                return ResourceBank.Get<GameObject>($"prefabs/finalemagnificus/Wizard3DPortrait_{key}");
            }
            catch
            {
                P03Plugin.Log.LogInfo($"Could not find wizard model for {key}");
                return null;
            }
        }

        public void DestroyModel(float delay = 0.15f)
        {
            if (Card.Anim is WizardCardAnimationController wcac && Card is PlayableCard pCard && pCard.OnBoard)
            {
                Transform animationParent = pCard.transform.Find("CustomAnimationParent");
                if (animationParent != null)
                {
                    animationParent.SetParent(BoardManager.Instance.gameObject.transform, worldPositionStays: true);
                    Tween.LocalScale(animationParent, Vector3.zero, delay, 0f, completeCallback: () => GameObject.Destroy(animationParent.gameObject));
                }
            }
        }

        private bool _startedInQueue = false;

        public override void ApplyAppearance()
        {
            if (Card.Anim is WizardCardAnimationController wcac && Card is PlayableCard pCard)
            {
                if (pCard.InOpponentQueue)
                {
                    _startedInQueue = true;
                    return;
                }
                if (!pCard.OnBoard)
                    return;

                GameObject prefab = GetPrefab();
                if (prefab == null)
                    return;

                Transform animationParent = pCard.transform.Find("CustomAnimationParent");
                if (animationParent == null)
                {
                    GameObject anim = new("CustomAnimationParent");
                    anim.transform.SetParent(pCard.transform);
                    anim.transform.localPosition = Vector3.zero;
                    anim.transform.localScale = new(0.2f, 0.2f, 0.2f);
                    GameObject wizard = Instantiate(prefab, anim.transform);
                    wizard.transform.Find("Anim")?.gameObject.SetActive(true);
                    wcac.WizardPortrait = wizard.GetComponent<WizardBattle3DPortrait>();
                    if (wcac.WizardPortrait == null)
                    {
                        wcac.WizardPortrait = wizard.AddComponent<WizardBattle3DPortrait>();
                        wcac.WizardPortrait.anim = wizard.GetComponent<Animator>();
                        wcac.WizardPortrait.projectileSpawnPoint = wizard.transform;
                        wcac.WizardPortrait.gemType = GemType.Orange;
                        wcac.WizardPortrait.enabled = true;
                    }
                    if (prefab.name.EndsWith("PracticeMage") && !_startedInQueue)
                        wizard.transform.Find("Anim/archeryTarget").localEulerAngles = new(0f, 0f, 40f);
                    //wizard.SetActive(false);
                }
            }
        }

        public override void OnPreRenderCard() => ApplyAppearance();

        static OnboardWizardCardModel()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "OnboardWizardCardModel", typeof(OnboardWizardCardModel)).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TransformIntoCard))]
        [HarmonyPostfix]
        private static IEnumerator KillExistingModel(IEnumerator sequence, PlayableCard __instance)
        {
            __instance.GetComponent<OnboardWizardCardModel>()?.DestroyModel();
            yield return sequence;
        }

        [HarmonyPatch(typeof(WizardCardAnimationController), nameof(WizardCardAnimationController.PlayAttackAnimation), typeof(bool), typeof(CardSlot))]
        [HarmonyPrefix]
        private static bool SpecialWizardAttack(WizardCardAnimationController __instance, bool attackPlayer, CardSlot targetSlot)
        {
            if (__instance.PlayableCard != null && __instance.PlayableCard.Info.appearanceBehaviour.Contains(ID) && __instance.WizardPortrait != null)
            {
                if (targetSlot == null)
                    targetSlot = __instance.PlayableCard.Slot.opposingSlot;
                //__instance.WizardPortrait.gameObject.SetActive(true);
                SpecialPortraitAttack(__instance.WizardPortrait, () => __instance.OnAttackImpact(), attackPlayer, targetSlot);
                //CustomCoroutine.WaitThenExecute(0.3f, () => __instance.WizardPortrait.gameObject.SetActive(false));
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(WizardCardAnimationController), nameof(WizardCardAnimationController.PlayHitAnimation))]
        [HarmonyPrefix]
        private static bool SpecialWizardHit(WizardCardAnimationController __instance)
        {
            if (__instance.PlayableCard != null && __instance.PlayableCard.Info.appearanceBehaviour.Contains(ID) && __instance.WizardPortrait != null)
            {
                //__instance.WizardPortrait.gameObject.SetActive(true);
                __instance.WizardPortrait.PlayHitAnimation();
                CustomCoroutine.WaitThenExecute(0.25f, () => Traverse.Create(__instance).Field("<DoingAttackAnimation>k__BackingField").SetValue(false));
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(WizardCardAnimationController), nameof(WizardCardAnimationController.PlayDeathAnimation))]
        [HarmonyPrefix]
        private static bool SpecialWizardDie(WizardCardAnimationController __instance, bool playSound)
        {
            if (__instance.PlayableCard != null && __instance.PlayableCard.Info.appearanceBehaviour.Contains(ID) && __instance.WizardPortrait != null)
            {
                //__instance.WizardPortrait.gameObject.SetActive(true);
                __instance.Anim.SetBool("dying", true);
                __instance.Anim.Play("death", 0, 0f);
                if (playSound)
                {
                    AudioController.Instance.PlaySound3D("card_death", MixerGroup.TableObjectsSFX, __instance.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), new AudioParams.Repetition(0.05f, ""), null, null, false);
                }
                __instance.WizardPortrait.PlayDeathAnimation(0.15f);
                return false;
            }
            return true;
        }

        private static readonly ConditionalWeakTable<WizardBattle3DPortrait, Transform> LastTargetSlot = new();

        private static void SetLastTargetSlot(WizardBattle3DPortrait portrait, Transform target)
        {
            LastTargetSlot.Remove(portrait);
            LastTargetSlot.Add(portrait, target);
        }

        private static Transform GetLastTargetSlot(WizardBattle3DPortrait portrait) => LastTargetSlot.TryGetValue(portrait, out Transform target) ? target : null;

        private static void SpecialPortraitAttack(WizardBattle3DPortrait portrait, Action impactCallback, bool attackPlayer, CardSlot target)
        {
            portrait.projectileImpactCallback = impactCallback;
            SetLastTargetSlot(portrait, target.transform);
            if (attackPlayer)
            {
                portrait.projectileDestination = target.transform.position + (Vector3.down * 30f);
                portrait.anim.SetTrigger("attack");
                return;
            }
            portrait.projectileDestination = target.transform.position + (Vector3.down * 30f);
            portrait.anim.SetTrigger("attack");
        }

        [HarmonyPatch(typeof(WizardBattle3DPortrait), nameof(WizardBattle3DPortrait.OnFireProjectileKeyframe))]
        [HarmonyPrefix]
        private static bool SpecialProjectileKeyFrame(WizardBattle3DPortrait __instance)
        {
            if (WizardPortraitSlotManager.Instance != null)
                return true;

            P03Plugin.Log.LogInfo($"Executing Projectile Keyframe Code");

            AudioController.Instance.PlaySound3D("wizard_cast", MixerGroup.TableObjectsSFX, __instance.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Medium)).spatialBlend = 0.5f;
            GameObject projectile = Instantiate(__instance.projectilePrefab);
            projectile.transform.position = __instance.projectileSpawnPoint.position;
            projectile.transform.localScale = new(0.2f, 0.2f, 0.2f);
            Color color = __instance.gemType == GemType.Orange ? GameColors.Instance.gold
                          : __instance.gemType == GemType.Blue ? GameColors.Instance.brightBlue
                          : GameColors.Instance.limeGreen;
            projectile.GetComponent<Renderer>().material.color = color;
            Tween.Position(projectile.transform, __instance.projectileDestination, 0.075f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, delegate ()
            {
                __instance.projectileImpactCallback?.Invoke();
                PlayProjectileImpact(__instance, __instance.projectileDestination, __instance.gemType);
                Tween.LocalScale(projectile.transform, Vector3.zero, 0.1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
                Destroy(projectile.gameObject, 0.2f);
            }, true);
            return false;
        }

        private static void PlayProjectileImpact(WizardBattle3DPortrait portrait, Vector3 position, GemType attackerGemType)
        {
            P03Plugin.Log.LogInfo("Playing Projectile Impact Code");
            AudioController.Instance.PlaySound3D("wizard_projectileimpact", MixerGroup.TableObjectsSFX, position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
            // Color color;
            // Color color2;
            // if (attackerGemType != GemType.Orange)
            // {
            //     if (attackerGemType != GemType.Blue)
            //     {
            //         color = GameColors.Instance.darkLimeGreen;
            //         color2 = GameColors.Instance.limeGreen;
            //     }
            //     else
            //     {
            //         color = GameColors.Instance.blue;
            //         color2 = GameColors.Instance.brightBlue;
            //     }
            // }
            // else
            // {
            //     color = GameColors.Instance.gold;
            //     color2 = GameColors.Instance.brightGold;
            // }
            // portrait.projectileSmoke.main.startColor = color;
            // portrait.projectileDust.main.startColor = color2;
            Transform target = GetLastTargetSlot(portrait);
            if (target != null)
            {
                GameObject gameObject = Instantiate(ResourceBank.Get<GameObject>("prefabs/finalemagnificus/WizardProjectileImpactEffects"), target);
                gameObject.transform.position = position;
                gameObject.transform.localScale = new(0.2f, 0.2f, 0.2f);
                gameObject.SetActive(true);
                Destroy(gameObject, 10f);
            }
            CameraEffects.Instance.Shake(0.08f, 0.25f);
        }
    }
}