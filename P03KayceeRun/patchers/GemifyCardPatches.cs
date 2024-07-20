using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class GemifyCardPatches
    {
        private static bool IsOnGemifyFallback
        {
            get
            {
                return SpecialNodeHandler.Instance != null &&
                       SpecialNodeHandler.Instance.attachGemSequencer != null &&
                       (
                            (SpecialNodeHandler.Instance.attachGemSequencer.modList != null &&
                            SpecialNodeHandler.Instance.attachGemSequencer.modList.Count == 3)
                            ||
                            (SpecialNodeHandler.Instance.attachGemSequencer.selectedCard != null &&
                            SpecialNodeHandler.Instance.attachGemSequencer.selectedCard.Info.Gemified)
                       );
            }
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.GetValidCardsFromDeck))]
        [HarmonyPostfix]
        private static void FallbackToAddGemAbilities(ref List<CardInfo> __result)
        {
            if (__result.Count == 0 && P03AscensionSaveData.IsP03Run)
            {
                __result = new List<CardInfo>(Part3SaveData.Data.deck.Cards);
                __result.RemoveAll((CardInfo c) => c.Abilities.Count >= 4);
            }
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.UpdateModChoices))]
        [HarmonyPrefix]
        private static bool SelectGemAbilities(AttachGemSequencer __instance, CardInfo selectedCard)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (selectedCard.Gemified)
            {
                if (__instance.modChoices == null)
                {
                    __instance.modChoices = new List<CardModificationInfo>();
                    __instance.modChoices.Add(new CardModificationInfo(Ability.GainGemOrange));
                    __instance.modChoices.Add(new CardModificationInfo(Ability.GainGemBlue));
                    __instance.modChoices.Add(new CardModificationInfo(Ability.GainGemGreen));
                }
                __instance.currentValidModChoices = new List<CardModificationInfo>(__instance.modChoices);
                __instance.currentValidModChoices.RemoveAll((CardModificationInfo x) => selectedCard.HasAbility(x.abilities[0]));
                return false;
            }
            return true;
        }

        private static P03AbilityFace _abilityFace = null;

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.DisplayMod))]
        [HarmonyPrefix]
        private static bool DisplayMod(AttachGemSequencer __instance, CardModificationInfo mod, bool fromCursorExit = false)
        {
            if (!P03AscensionSaveData.IsP03Run || !IsOnGemifyFallback)
                return true;

            _abilityFace?.SetAbility(mod.abilities[0]);
            if (RuleBookController.Instance.Shown && !fromCursorExit)
                RuleBookController.Instance.OpenToAbilityPage(mod.abilities[0].ToString(), null, true);
            return false;
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.OnStartModSelection))]
        [HarmonyPrefix]
        private static bool OnStartModSelection(AttachGemSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run || !IsOnGemifyFallback)
                return true;

            _abilityFace = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AbilityIcon, true, true).GetComponent<P03AbilityFace>();
            P03ScreenInteractables.Instance.AssignFaceAltInteractable(CursorType.Inspect, new Action<AlternateInputInteractable>(OnAltSelectScreen), null);
            return false;
        }

        private static void OnAltSelectScreen(AlternateInputInteractable screen)
        {
            var instance = SpecialNodeHandler.Instance?.attachGemSequencer;
            if (instance != null)
                RuleBookController.Instance.OpenToAbilityPage(instance.currentValidModChoices[instance.currentModIndex].abilities[0].ToString(), null, RuleBookController.Instance.Shown);
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.ShowOverviewOnScreen))]
        [HarmonyPrefix]
        private static bool ShowOverviewOnScreen(AttachGemSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run || !IsOnGemifyFallback)
                return true;

            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AbilityIcon, false, true);
            return false;
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.ShowDetailsOnScreen))]
        [HarmonyPrefix]
        private static bool ShowDetailsOnScreen(AttachGemSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run || !IsOnGemifyFallback)
                return true;

            var instance = SpecialNodeHandler.Instance?.attachGemSequencer;
            if (instance != null)
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AddMod, false, true).GetComponent<P03AddModFace>().DisplayCardWithMod(instance.selectedCard.Info, instance.currentValidModChoices[instance.currentModIndex]);
            return false;
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.GetCompletedDialogueId))]
        [HarmonyPrefix]
        private static bool GetCompletedDialogueId(ref string __result)
        {
            if (!P03AscensionSaveData.IsP03Run || !IsOnGemifyFallback)
                return true;

            __result = "P03BuiltInGems";
            return false;
        }

        [HarmonyPatch(typeof(AttachGemSequencer), nameof(AttachGemSequencer.EnterDiskDrive))]
        [HarmonyPostfix]
        private static IEnumerator EnterDiskDrive(IEnumerator sequence)
        {
            yield return sequence;

            if (!P03AscensionSaveData.IsP03Run)
                yield break;

            List<CardInfo> list = new(Part3SaveData.Data.deck.Cards);
            list.RemoveAll((CardInfo c) => c.Mods.Exists((CardModificationInfo m) => m.gemify));

            if (list.Count > 0)
                yield break;

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03CannotGemify", TextDisplayer.MessageAdvanceMode.Input);
        }
    }
}