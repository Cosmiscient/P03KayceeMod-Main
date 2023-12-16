using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class CanvasAscensionSequencer : CanvasBattleSequencer
    {
        private static readonly int[] INDICES = new int[] { 1, 2, 3 };

        public override IEnumerator PreHandDraw()
        {
            P03Plugin.Log.LogInfo($"Ascension Canvas Sequencer - is Ascension {SaveFile.IsAscension}");
            if (SaveFile.IsAscension && P03Plugin.Instance.SkipCanvasFace)
            {
                // This skips the part of the battle where you pick the boss face.
                if (!Part3SaveData.Data.ValidCanvasBossFace)
                {
                    canvasFace = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.CanvasBlank, true, true).GetComponent<P03CanvasFace>();
                    Part3SaveData.Data.canvasBossEyeIndex = INDICES[UnityEngine.Random.Range(0, INDICES.Length)];
                    Part3SaveData.Data.canvasBossMouthIndex = INDICES[UnityEngine.Random.Range(0, INDICES.Length)];
                    canvasFace.UpdateFace();
                }
            }
            yield return base.PreHandDraw();
        }

        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            EncounterData encounterData = base.BuildCustomEncounter(nodeData);
            EncounterBlueprintData blueprint = EncounterHelper.CanvasBossPX;
            P03Plugin.Log.LogInfo($"Building Canvas turn plan with difficulty {EventManagement.EncounterDifficulty}");
            encounterData.opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(blueprint, EventManagement.EncounterDifficulty, false);
            foreach (List<CardInfo> turn in encounterData.opponentTurnPlan)
                P03Plugin.Log.LogInfo($"Turn: {string.Join(",", turn.Select(i => i == null ? "NONE" : i.name))}");
            return encounterData;
        }

        private readonly bool chooseFirstRule = EventManagement.CompletedZones.Count <= 3;
        private readonly bool chooseSecondRule = EventManagement.CompletedZones.Count <= 1;
        private readonly bool haveThirdRule = EventManagement.CompletedZones.Count >= 2;

        private void ShowArrowButtons(bool upperActive, Action leftUpperPressedCallback, Action rightUpperPressedCallback, Action leftLowerPressedCallback, Action rightLowerPressedCallback)
        {
            P03ScreenInteractables.Instance.leftUpperArrowButton.gameObject.SetActive(upperActive);
            P03ScreenInteractables.Instance.rightUpperArrowButton.gameObject.SetActive(upperActive);
            P03ScreenInteractables.Instance.leftLowerArrowButton.gameObject.SetActive(!upperActive);
            P03ScreenInteractables.Instance.rightLowerArrowButton.gameObject.SetActive(!upperActive);
            HighlightedInteractable highlightedInteractable = P03ScreenInteractables.Instance.leftUpperArrowButton;
            highlightedInteractable.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(highlightedInteractable.CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
            {
                leftUpperPressedCallback();
            }));
            HighlightedInteractable highlightedInteractable2 = P03ScreenInteractables.Instance.rightUpperArrowButton;
            highlightedInteractable2.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(highlightedInteractable2.CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
            {
                rightUpperPressedCallback();
            }));
            HighlightedInteractable highlightedInteractable3 = P03ScreenInteractables.Instance.leftLowerArrowButton;
            highlightedInteractable3.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(highlightedInteractable3.CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
            {
                leftLowerPressedCallback();
            }));
            HighlightedInteractable highlightedInteractable4 = P03ScreenInteractables.Instance.rightLowerArrowButton;
            highlightedInteractable4.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(highlightedInteractable4.CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
            {
                rightLowerPressedCallback();
            }));
        }

        private IEnumerator RandomRuleSequence()
        {
            RulePaintingManager.Instance.SetPaintingsShown(false);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            ruleDisplayer = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.CreateRule, true, true).GetComponentInChildren<CompositeRuleDisplayer>();
            ruleDisplayer.ResetPainting();
            currentRule = new CompositeBattleRule();
            ruleDisplayer.DisplayRule(currentRule);

            yield return new WaitForSeconds(1f);

            int seed = P03AscensionSaveData.RandomSeed + (123 * TurnManager.Instance.TurnNumber);

            bool chooseUpper = SeededRandom.Bool(seed++);

            int numberOfTicks = SeededRandom.Range(2, 9, seed++);

            for (int i = 0; i < numberOfTicks; i++)
            {
                if (chooseUpper) IncrementRuleEffect();
                else IncrementRuleTrigger();
                yield return new WaitForSeconds(.25f);
            }

            yield return new WaitForSeconds(1.5f);

            ShowArrowButtons(chooseUpper, new Action(DecrementRuleTrigger), new Action(IncrementRuleTrigger), new Action(DecrementRuleEffect), new Action(IncrementRuleEffect));

            yield return new WaitUntil(() => currentRule.IsValid());
            bool ruleConfirmed = false;
            P03ScreenInteractables.Instance.AssignFaceMainInteractable(CursorType.Point, new Action<MainInputInteractable>(OnCursorEnterScreenForRule), new Action<MainInputInteractable>(OnCursorExitScreenForRule), null, delegate (MainInputInteractable i)
            {
                ruleConfirmed = true;
            });
            yield return new WaitUntil(() => ruleConfirmed);

            P03ScreenInteractables.Instance.ClearFaceInteractables();
            P03ScreenInteractables.Instance.HideArrowButtons();
            ruleDisplayer.MovingPaintingOffscreen();
            yield return new WaitForSeconds(0.15f);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.CanvasBlank, true, true);
            yield return new WaitForSeconds(0.1f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            CanvasBoss.FadeInAudioLayer(rulesHandler.NumRules + 1);
            yield return RulePaintingManager.Instance.SpawnPainting(currentRule, 1f);
            rulesHandler.AddRule(currentRule);
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (!SaveFile.IsAscension)
            {
                yield return base.OnUpkeep(playerUpkeep);
                yield break;
            }

            if (TurnManager.Instance.TurnNumber > 0 && TurnManager.Instance.Opponent.NumLives == 2 && rulesHandler.NumRules == 0)
            {
                if (chooseFirstRule)
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("CanvasChooseRule1", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    yield return CreateRuleSequence();
                    yield return TextDisplayer.Instance.PlayDialogueEvent("CanvasChooseRule2", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                }
                else
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03RandomCanvas", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    yield return RandomRuleSequence();
                }
            }
            if (TurnManager.Instance.Opponent.NumLives == 1 && !made3rdRule)
            {
                turnsSincePhase2++;
                if (turnsSincePhase2 > 2)
                {
                    if (haveThirdRule)
                    {
                        yield return TextDisplayer.Instance.PlayDialogueEvent("CanvasChooseThirdRule", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                        yield return CreateRuleSequence();
                    }
                    made3rdRule = true;
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(CanvasBattleSequencer), nameof(CreatePhase2Rule))]
        public static class RandomPhase2Rule
        {
            [HarmonyPrefix]
            public static void Prefix(ref CanvasBattleSequencer __instance, ref CanvasBattleSequencer __state) => __state = __instance;

            [HarmonyPostfix]
            public static IEnumerator Postfix(IEnumerator enumerator, CanvasBattleSequencer __state)
            {
                if (__state is CanvasAscensionSequencer ascnSeq)
                {
                    if (!ascnSeq.chooseSecondRule)
                    {
                        yield return TextDisplayer.Instance.PlayDialogueEvent("P03RandomCanvas", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                        yield return ascnSeq.RandomRuleSequence();
                        yield break;
                    }
                    else
                    {
                        yield return TextDisplayer.Instance.PlayDialogueEvent("CanvasPhase2Start", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                        yield return ascnSeq.CreateRuleSequence();
                        yield break;
                    }
                }
                else
                {
                    yield return enumerator;
                    yield break;
                }
            }
        }

    }
}