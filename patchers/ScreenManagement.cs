using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class ScreenManagement
    {
        private const string GRIMORA_MOD = "arackulele.inscryption.grimoramod";
        private const string MAGNIFICUS_MOD = "silenceman.inscryption.magnificusmod";

        internal static Dictionary<string, string> AcceptedScreenStates = new()
        {
            { P03Plugin.PluginGuid, P03Plugin.PluginGuid },
            { GRIMORA_MOD, GRIMORA_MOD },
            { MAGNIFICUS_MOD, $"{MAGNIFICUS_MOD}starterdecks" }
        };

        internal static CardTemple ScreenState
        {
            get
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene != null && !string.IsNullOrEmpty(activeScene.name))
                {
                    string sceneName = activeScene.name.ToLowerInvariant();
                    if (sceneName.Contains("magnificus"))
                        return CardTemple.Wizard;
                    if (sceneName.Contains("part3"))
                        return CardTemple.Tech;
                    if (sceneName.Contains("grimora"))
                        return CardTemple.Undead;
                    if (sceneName.Contains("part1"))
                        return CardTemple.Nature;
                }

                foreach (string guid in AcceptedScreenStates.Keys)
                {
                    if (!Chainloader.PluginInfos.ContainsKey(guid))
                        continue;

                    string value = ModdedSaveManager.SaveData.GetValue(AcceptedScreenStates[guid], "ScreenState");
                    if (string.IsNullOrEmpty(value))
                        continue;

                    return (CardTemple)Enum.Parse(typeof(CardTemple), value);
                }

                return CardTemple.Nature;
            }
            set
            {
                CardTemple oldValue = ScreenState;

                if (value == CardTemple.Nature)
                    ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "ScreenState", default(string));
                else
                    ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "ScreenState", value);

                P03Plugin.Log.LogInfo($"Changing screenstate from {oldValue} to {value}");

                if (value != oldValue && SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("ascension"))
                {
                    P03Plugin.Log.LogInfo($"Reconfiguring the post game screens");
                    AscensionMenuScreens.Instance.ConfigurePostGameScreens();
                }
            }
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.ConfigurePostGameScreens))]
        [HarmonyPrefix]
        private static bool P03ConfigurePostGameScreens(AscensionMenuScreens __instance)
        {
            if (ScreenState == CardTemple.Tech || P03AscensionSaveData.ReturningFromP03Run)
            {
                if (AscensionMenuScreens.ReturningFromFailedRun || AscensionMenuScreens.ReturningFromSuccessfulRun)
                {
                    __instance.startScreen.gameObject.SetActive(false);
                    __instance.runEndScreen.gameObject.SetActive(true);
                    __instance.runEndScreen.GetComponent<AscensionRunEndScreen>().Initialize(AscensionMenuScreens.ReturningFromSuccessfulRun);
                    __instance.runEndScreen.GetComponent<AscensionStatsScreen>().PreFillStatsText();
                    if (AscensionStatsData.GetStatValue(AscensionStat.Type.Misplays, false) >= 10)
                    {
                        AchievementManager.Unlock(Achievement.KMOD_SPECIAL1);
                    }
                    if (AscensionMenuScreens.ReturningFromSuccessfulRun && AscensionSaveData.Data.ChallengeLevelIsMet() && AscensionSaveData.Data.challengeLevel <= 6)
                    {
                        //AscensionUnlockSchedule.UnlockTier unlocksForLevel = AscensionUnlockSchedule.GetUnlocksForLevel(AscensionSaveData.Data.challengeLevel);
                        AscensionSaveData.Data.IncrementChallengeLevel();
                    }
                    AscensionMenuScreens.ReturningFromCredits = false;
                    AscensionMenuScreens.ReturningFromSuccessfulRun = false;
                    AscensionMenuScreens.ReturningFromFailedRun = false;
                    AscensionSaveData.Data.EndRun();
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        [HarmonyBefore("zorro.inscryption.infiniscryption.packmanager")]
        private static void InitializeP03SaveData(ref AscensionMenuScreens __instance, bool newRun)
        {
            P03Plugin.Log.LogInfo($"New Run? {newRun} Screen State {ScreenState} IsP03? {P03AscensionSaveData.IsP03Run}");
            if (newRun)
            {
                if (ScreenState == CardTemple.Tech)
                {
                    // Ensure the old part 3 save data gets saved if it needs to be
                    P03AscensionSaveData.EnsureRegularSave();
                    //P03AscensionSaveData.IsP03Run = true;
                }
                else
                {
                    //P03AscensionSaveData.IsP03Run = false;
                }
            }
            SaveManager.SaveToFile();
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), nameof(AscensionChallengeScreen.OnEnable))]
        [HarmonyPostfix]
        private static void LogScreenInfo() => P03Plugin.Log.LogInfo($"Challenge screen. State {ScreenState}. Challenge level (P03) {P03AscensionSaveData.P03Data.challengeLevel} (Standard) {AscensionSaveData.Data.challengeLevel}");

        [HarmonyPatch(typeof(AscensionStartScreen), nameof(AscensionStartScreen.ManagedUpdate))]
        [HarmonyPostfix]
        private static void UnlockEverything()
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                && Input.GetKey(KeyCode.P)
                && (Input.GetKey(KeyCode.Alpha0) || Input.GetKey(KeyCode.RightParen) || Input.GetKey(KeyCode.Keypad0))
                && (Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Hash) || Input.GetKey(KeyCode.Keypad3)))
            {
                P03Plugin.Log.LogInfo("Maxing out P03 Challenge Level");
                P03AscensionSaveData.P03Data.challengeLevel = 13;
            }
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.E))
            {
                P03Plugin.Log.LogInfo("Maxing out P03 Challenge Level");
                P03AscensionSaveData.P03Data.challengeLevel = 13;
            }
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.E))
            {
                P03Plugin.Log.LogInfo("Resetting P03 Challenge Level");
                P03AscensionSaveData.P03Data.challengeLevel = 1;
            }
        }

        [HarmonyPatch(typeof(MenuController), nameof(MenuController.LoadGameFromMenu))]
        [HarmonyPrefix]
        private static bool LoadGameFromMenu(bool newGameGBC)
        {
            if (!newGameGBC && P03AscensionSaveData.IsP03Run)
            {

                SaveManager.LoadFromFile();
                LoadingScreenManager.LoadScene("Part3_Cabin");
                SaveManager.savingDisabled = false;
                return false;
            }
            return true;
        }

        private static void ClearP03Data()
        {
            if (!AscensionMenuScreens.ReturningFromFailedRun && !AscensionMenuScreens.ReturningFromSuccessfulRun)
                ScreenState = CardTemple.Nature;

            RunBasedHoloMap.ClearWorldData();
            SaveManager.SaveToFile();
        }

        [HarmonyPatch(typeof(AscensionStartScreen), nameof(AscensionStartScreen.OnEnable))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static void ClearP03SaveOnNewRun() => ClearP03Data();

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPrefix]
        private static void ClearScreenStatePrefix() => ClearP03Data();

        [HarmonyPatch(typeof(AscensionStartScreen), nameof(AscensionStartScreen.UpdateContinueTextEnabled))]
        [HarmonyPostfix]
        private static void P03ContinueButtons(AscensionStartScreen __instance)
        {
            Transform continueButton = __instance.transform.Find("Center/MenuItems/Menu_Continue_P03/Menu_Continue");
            Transform disabledButton = __instance.transform.Find("Center/MenuItems/Menu_Continue_P03/Menu_Continue_DISABLED");
            P03Plugin.Log.LogInfo($"Continue {continueButton}, disabled {disabledButton}, P03 Save {P03AscensionSaveData.P03Data}, Run exists {P03AscensionSaveData.P03RunExists}");
            continueButton?.gameObject.SetActive(P03AscensionSaveData.P03RunExists);
            disabledButton?.gameObject.SetActive(!P03AscensionSaveData.P03RunExists);
        }

        private static readonly string[] menuItems = new string[] { "Menu_New", "Continue", "Menu_Stats", "Menu_Unlocks", "Menu_Exit", "Menu_QuitApp" };
        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPostfix]
        private static void AddP03StartOption()
        {
            GameObject startScreen = AscensionMenuScreens.Instance.startScreen.gameObject;

            GameObject newButton = startScreen.transform.Find($"Center/MenuItems/{menuItems[0]}").gameObject;
            newButton.GetComponentInChildren<PixelText>().SetText("- NEW LESHY RUN -");
            AscensionMenuInteractable newButtonController = newButton.GetComponent<AscensionMenuInteractable>();

            Vector3 newP03RunPos = startScreen.transform.Find($"Center/MenuItems/{menuItems[1]}").localPosition;
            float ygap = newP03RunPos.y - newButton.transform.localPosition.y;

            // Make room for the new menu option
            for (int i = 1; i < menuItems.Length; i++)
            {
                Transform item = startScreen.transform.Find($"Center/MenuItems/{menuItems[i]}");
                item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y + ygap, item.localPosition.z);
            }

            // Clone the new button
            GameObject newP03Button = UnityEngine.Object.Instantiate(newButton, newButton.transform.parent);
            newP03Button.transform.localPosition = newP03RunPos;
            newP03Button.name = "Menu_New_P03";
            AscensionMenuInteractable newP03ButtonController = newP03Button.GetComponent<AscensionMenuInteractable>();
            newP03ButtonController.CursorSelectStarted = delegate (MainInputInteractable i)
            {
                ScreenState = CardTemple.Tech;
                newButtonController.CursorSelectStart();
            };
            newP03Button.GetComponentInChildren<PixelText>().SetText("- NEW P03 RUN -");

            // Setup continue button
            GameObject continueButton = startScreen.transform.Find($"Center/MenuItems/{menuItems[1]}").gameObject;
            continueButton.GetComponentInChildren<PixelText>().SetText("- CONTINUE LESHY RUN -");
            AscensionMenuInteractable continueButtonController = continueButton.transform.Find("Menu_Continue").GetComponentInChildren<AscensionMenuInteractable>();

            Vector3 continueP03RunPos = startScreen.transform.Find($"Center/MenuItems/{menuItems[2]}").localPosition;
            ygap = continueP03RunPos.y - continueButton.transform.localPosition.y;

            // Make room for the continue menu option
            for (int i = 2; i < menuItems.Length; i++)
            {
                Transform item = startScreen.transform.Find($"Center/MenuItems/{menuItems[i]}");
                item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y + ygap, item.localPosition.z);
            }

            // Clone the continue button
            GameObject continueP03Button = UnityEngine.Object.Instantiate(continueButton, continueButton.transform.parent);
            continueP03Button.transform.localPosition = continueP03RunPos;
            continueP03Button.name = "Menu_Continue_P03";
            AscensionMenuInteractable continueP03ButtonController = continueP03Button.transform.Find("Menu_Continue").GetComponent<AscensionMenuInteractable>();
            continueP03ButtonController.CursorSelectStarted = delegate (MainInputInteractable i)
            {
                ScreenState = CardTemple.Tech;
                continueButtonController.CursorSelectStart();
            };

            continueP03Button.transform.Find("Menu_Continue").GetComponentInChildren<PixelText>().SetText("- CONTINUE P03 RUN -");
            continueP03Button.transform.Find("Menu_Continue_DISABLED").GetComponentInChildren<PixelText>().SetText("- CONTINUE P03 RUN -");

            // Add to transition
            AscensionMenuScreenTransition transitionController = startScreen.GetComponent<AscensionMenuScreenTransition>();

            transitionController.onEnableRevealedObjects.Insert(transitionController.onEnableRevealedObjects.IndexOf(newButton) + 1, newP03Button);
            transitionController.onEnableRevealedObjects.Insert(transitionController.onEnableRevealedObjects.IndexOf(continueButton) + 1, continueP03Button);
            transitionController.screenInteractables.Insert(transitionController.screenInteractables.IndexOf(newButtonController) + 1, newP03ButtonController);
            transitionController.screenInteractables.Insert(transitionController.screenInteractables.IndexOf(continueButtonController) + 1, continueP03ButtonController);

            AscensionMenuScreens.Instance.startScreen.GetComponent<AscensionStartScreen>().UpdateContinueTextEnabled();
        }
    }
}