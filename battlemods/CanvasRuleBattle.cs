using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.CustomRules;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class CanvasRuleBattle : CompositeRuleTriggerHandler, IBattleModSetup, IBattleModCleanup
    {
        public static BattleModManager.ID ID { get; private set; }

        public const bool SELECTED_PAINTING_TO_SCALES = true;

        static CanvasRuleBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Random Rule",
                new List<string>() { "For this battle, I'm going to add an additional rule." },
                typeof(CanvasRuleBattle),
                difficulty: 5,
                bossValid: true,
                iconPath: "p03kcm/prefabs/frame"
            );

            BattleModManager.SetGlobalActivationRule(ID,
                () => AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.PAINTING_CHALLENGE)
                      && TurnManager.Instance.opponent != null && TurnManager.Instance.opponent is Part3BossOpponent);

            BattleModManager.SetGlobalActivationRule(ID, () => true);
        }

        private GameObject effects = null;
        private GameObject offsitePainting = null;

        public void MakeCanvasRuleDisplayer()
        {
            this.effects = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightQuadTableEffect"));
            Renderer renderer = effects.GetComponentInChildren<Renderer>();
            renderer.enabled = false;
        }

        private IEnumerator SpawnPainting(RulePaintingManager manager, CompositeBattleRule rule)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/RulesPainting"), manager.placementMarkers[manager.currentPlacement]);
            gameObject.transform.localPosition = Vector3.up * 7f;
            gameObject.transform.localRotation = Quaternion.identity;
            GameObject gameObject2 = Object.Instantiate<GameObject>(manager.renderCameraObject, base.transform);
            gameObject2.SetActive(true);
            gameObject2.transform.position += Vector3.left * (float)manager.currentPlacement * 20f;
            RenderTexture renderTexture = new RenderTexture(manager.baseRenderTexture);
            renderTexture.filterMode = FilterMode.Point;
            gameObject2.GetComponentInChildren<Camera>().targetTexture = renderTexture;
            gameObject.GetComponent<RulePainting>().AssignRenderTexture(renderTexture);
            manager.activePaintingCameras.Add(gameObject2);
            gameObject2.GetComponentInChildren<CompositeRuleDisplayer>().DisplayRule(rule);
            rule.painting = gameObject.transform;
            manager.currentPlacement++;
            manager.orbitValue = 360f / (float)manager.currentPlacement + 180f;
            manager.SetPaintingsShown(true);
            yield break;
        }

        [HarmonyPatch(typeof(RulePaintingManager), nameof(RulePaintingManager.ManagedUpdate))]
        [HarmonyPrefix]
        private static bool AlignPaintingsForSelection(RulePaintingManager __instance)
        {
            var receiver = BattleModManager.GetActiveReceiver(ID) as CanvasRuleBattle;
            if (receiver != null && receiver.InSelectionEvent)
            {
                for (int i = 0; i < __instance.placementMarkers.Count; i++)
                {
                    try
                    {
                        __instance.placementMarkers[i].localPosition = new(3f * (i - 1), 0, -6f);
                        __instance.placementMarkers[i].GetChild(0).localEulerAngles = new(0f, 40f * (i - 1), 0f);
                    }
                    catch { }
                }
                return false;
            }
            return TurnManager.Instance.opponent is CanvasBossOpponent || !SELECTED_PAINTING_TO_SCALES;
        }

        private static string BossDialogueKey
        {
            get
            {
                if (TurnManager.Instance.opponent == null)
                    return "P03ChooseCanvasDefault";

                if (TurnManager.Instance.opponent is MycologistsBossOpponent)
                    return "P03ChooseCanvasMycologist";

                if (TurnManager.Instance.opponent is PhotographerBossOpponent)
                    return "P03ChooseCanvasPhotographer";

                if (TurnManager.Instance.opponent is TelegrapherBossOpponent)
                    return "P03ChooseCanvasTelegrapher";

                if (TurnManager.Instance.opponent is CanvasBossOpponent)
                    return "P03ChooseCanvasCanvas";

                if (TurnManager.Instance.opponent is ArchivistBossOpponent)
                    return "P03ChooseCanvasArchivist";

                return "P03ChooseCanvasDefault";
            }
        }

        private bool InSelectionEvent = false;
        private int SelectedBattleRule = -1;

        public IEnumerator OnBattleModSetup()
        {
            // Make sure there is a Rule Painting Manager
            if (RulePaintingManager.Instance == null)
                MakeCanvasRuleDisplayer();

            // Setup the battle rule at random
            List<Effect> validEffects = new(CompositeBattleRule.AVAILABLE_EFFECTS);
            List<Trigger> validTriggers = new(CompositeBattleRule.AVAILABLE_TRIGGERS);

            int randomSeed = P03AscensionSaveData.RandomSeed;

            List<CompositeBattleRule> rules = new();
            InSelectionEvent = true;
            SelectedBattleRule = -1;

            RulePaintingManager.Instance.SetPaintingsShown(true);
            yield return new WaitForSeconds(0.4f);

            for (int i = 0; i < 3; i++)
            {
                CompositeBattleRule currentRule = new();
                currentRule.trigger = validTriggers[SeededRandom.Range(0, validTriggers.Count, randomSeed++)];

                if (currentRule.trigger == Trigger.OtherCardResolve)
                    validEffects = validEffects.Where(r => r is not RandomCardDestroyedEffect and not RandomSalmon).ToList();
                currentRule.effect = validEffects[SeededRandom.Range(0, validEffects.Count, randomSeed++)];

                // TODO: REMOVE
                if (i == 0)
                    currentRule.effect = validEffects.FirstOrDefault(e => e is RandomSalmon);
                validEffects.Remove(currentRule.effect);

                yield return SpawnPainting(RulePaintingManager.Instance, currentRule);
                Transform marker = RulePaintingManager.Instance.placementMarkers[i].GetChild(0);
                marker.localScale = Vector3.zero;
                yield return new WaitForSeconds(0.3f);
                marker.localScale = new(0.5f, 0.5f, 0.5f);
                marker.localPosition = new(0f, 3.7f, 0f);
                Tween.LocalPosition(RulePaintingManager.Instance.placementMarkers[i].GetChild(0), Vector3.zero, 1f, 0f, AnimationCurves.EaseOutBackElastic);//Tween.EaseOutBack);

                // var animFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutElastic);
                // Tween.Value(0f, 1f, (float f) => marker.localPosition = new(0f, animFunc(3.7f, 0f, f), 0f), 1f, 0f);


                rules.Add(currentRule);

                GenericMainInputInteractable mii = RulePaintingManager.Instance.placementMarkers[i].GetComponentInChildren<GenericMainInputInteractable>();
                mii.cursorType = CursorType.Pickup;
                var idx = i;
                mii.CursorSelectStarted += delegate (MainInputInteractable m)
                {
                    var receiver = BattleModManager.GetActiveReceiver(ID) as CanvasRuleBattle;
                    if (receiver != null && receiver.InSelectionEvent)
                    {
                        SelectedBattleRule = idx;
                    }
                };

            }

            yield return new WaitForSeconds(0.3f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(BossDialogueKey, TextDisplayer.MessageAdvanceMode.Input);
            yield return new WaitUntil(() => SelectedBattleRule >= 0);

            for (int i = 0; i < 3; i++)
            {
                if (i != SelectedBattleRule || TurnManager.Instance.opponent is CanvasBossOpponent || !SELECTED_PAINTING_TO_SCALES)
                {
                    Tween.LocalPosition(RulePaintingManager.Instance.placementMarkers[i].GetChild(0), Vector3.up * 10f, 1f, 0f, Tween.EaseInBack);
                    yield return new WaitForSeconds(0.15f);
                }
            }

            yield return new WaitForSeconds(.9f);

            for (int i = 0; i < 3; i++)
            {
                if (i != SelectedBattleRule || TurnManager.Instance.opponent is CanvasBossOpponent || !SELECTED_PAINTING_TO_SCALES)
                {
                    GameObject.Destroy(RulePaintingManager.Instance.activePaintingCameras[i]);
                    GameObject.Destroy(RulePaintingManager.Instance.placementMarkers[i].GetChild(0).gameObject);
                    RulePaintingManager.Instance.placementMarkers[i].localPosition = new(0f, 5f, -1f);
                }
            }

            InSelectionEvent = false;
            RulePaintingManager.Instance.currentPlacement = 0;

            CompositeBattleRule selectedRule = rules[SelectedBattleRule];

            if (TurnManager.Instance.opponent is CanvasBossOpponent || !SELECTED_PAINTING_TO_SCALES)
            {
                yield return RulePaintingManager.Instance.SpawnPainting(selectedRule);
                RulePaintingManager.Instance.SetPaintingsShown(true);
            }
            else
            {
                Transform marker = RulePaintingManager.Instance.placementMarkers[SelectedBattleRule];
                Transform painting = marker.GetChild(0);
                //Tween.LocalPosition(painting, Vector3.zero, .8f, 0f, Tween.EaseInOut);
                Tween.LocalScale(painting, new(0.4f, 0.4f, 0.4f), .2f, .2f);
                Tween.LocalRotation(painting, new Vector3(0f, 300f, 0f), .2f, .2f);
                Tween.LocalPosition(marker, new Vector3(1f, .6f, -9f), .3f, 0f, Tween.EaseInOut, completeCallback: () => AudioController.Instance.PlaySound3D("metal_object_up#1", MixerGroup.TableObjectsSFX, marker.position, 1f));
                Tween.LocalPosition(marker, new Vector3(-3.9726f, -0.6236f, -6.0578f), .3f, .3f, Tween.EaseInBack, completeCallback: () => TableVisualEffectsManager.Instance.ThumpTable(0.7f));
                yield return new WaitForSeconds(1f);
                marker.SetParent(LifeManager.Instance.Scales3D.gameObject.transform, worldPositionStays: true);
                this.offsitePainting = marker.gameObject;
            }

            if (TurnManager.Instance.SpecialSequencer is CanvasBattleSequencer cbs)
            {
                cbs.rulesHandler.Rules.Add(selectedRule);
            }
            else
            {
                Rules.Clear();
                Rules.Add(selectedRule);
            }
        }

        public IEnumerator OnBattleModCleanup()
        {
            if (RulePaintingManager.Instance != null)
            {
                try
                {
                    RulePaintingManager.Instance.ShowMostRecentPaintingCancelled();
                }
                catch { }
                RulePaintingManager.Instance.SetPaintingsShown(false);
            }

            if (this.offsitePainting != null)
            {
                Tween.LocalPosition(this.offsitePainting.transform, Vector3.up * 10f, .35f, 0f, Tween.EaseInBack);
            }

            if (this.effects != null || this.offsitePainting != null)
            {
                yield return new WaitForSeconds(0.4f);

                if (this.effects != null)
                    GameObject.Destroy(this.effects);

                if (this.offsitePainting != null)
                    GameObject.Destroy(this.offsitePainting);
            }

            yield break;
        }

        [HarmonyPatch(typeof(CompositeRuleTriggerHandler), nameof(CompositeRuleTriggerHandler.BreakInfiniteLoop))]
        [HarmonyPostfix]
        private static IEnumerator DontDeleteRuleIfNotCanvasBoss(IEnumerator sequence, CompositeRuleTriggerHandler __instance)
        {
            if (TurnManager.Instance.opponent is CanvasBossOpponent)
            {
                yield return sequence;
                yield break;
            }

            // This is the same sequnece as the original, except it does not remove the most recent rule
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("CanvasStopInfiniteLoop", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            __instance.triggersThisTurn = 0;
            __instance.triggeringDisabled = true;
            CustomCoroutine.WaitOnConditionThenExecute(() => Singleton<GlobalTriggerHandler>.Instance.StackSize == 0, delegate
            {
                __instance.triggeringDisabled = false;
            });
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        [HarmonyPatch(typeof(CanvasBossOpponent), nameof(CanvasBossOpponent.FadeInAudioLayer))]
        [HarmonyPrefix]
        private static bool FixAudioWithExtraRule(int index)
        {
            if (!P03AscensionSaveData.IsP03Run || !BattleModManager.RuleIsActive(ID))
                return true;

            AudioController.Instance.SetLoopVolume(0.4f, 0.5f, Mathf.Clamp(index - 1, 1, 3), true);
            return false;
        }
    }
}