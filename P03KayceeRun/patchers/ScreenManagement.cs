using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private static float Y_GAP => 0.11f;

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
                if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("scarletskull"))
                    AchievementManager.Unlock(P03AchievementManagement.SKULLSTORM);
            }
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.E))
            {
                P03Plugin.Log.LogInfo("Maxing out P03 Challenge Level");
                P03AscensionSaveData.P03Data.challengeLevel = 13;
                if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("scarletskull"))
                    AchievementManager.Unlock(P03AchievementManagement.SKULLSTORM);
            }
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.N))
            {
                P03Plugin.Log.LogInfo("Resetting P03 Challenge Level");
                P03AscensionSaveData.P03Data.challengeLevel = 1;
            }
        }

        private static readonly Color KAYCEE_RED_COLOR = new Color(0.6196f, 0.149f, 0.1882f, 1f);
        private static readonly Color P03_BLUE_COLOR = new Color(0.1098f, 0.8863f, 1f, 1f);
        private static bool IsColorMatch(Color color_a, Color color_b)
        {
            // Target color: 0.6196 0.149 0.1882 1
            if (Math.Abs(color_a.r - color_b.r) > 0.1)
                return false;
            if (Math.Abs(color_a.g - color_b.g) > 0.1)
                return false;
            if (Math.Abs(color_a.b - color_b.b) > 0.1)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPostfix]
        private static void RecolorForP03OnStart(AscensionMenuScreens __instance)
        {
            RecolorForP03(__instance);
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.SwitchToScreen))]
        [HarmonyPostfix]
        private static void RecolorForP03(AscensionMenuScreens __instance)
        {
            if (P03AscensionSaveData.LeshyIsDead)
            {
                foreach (SpriteRenderer renderer in __instance.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (IsColorMatch(renderer.color, KAYCEE_RED_COLOR))
                        renderer.color = P03_BLUE_COLOR;
                }
                foreach (AscensionMenuBlinkEffect blinker in __instance.GetComponentsInChildren<AscensionMenuBlinkEffect>(true))
                {
                    if (IsColorMatch(blinker.blinkOffColor, KAYCEE_RED_COLOR))
                        blinker.blinkOffColor = P03_BLUE_COLOR;
                    if (IsColorMatch(blinker.blinkOnColor, KAYCEE_RED_COLOR))
                        blinker.blinkOnColor = P03_BLUE_COLOR;
                }
                foreach (PixelText text in __instance.GetComponentsInChildren<PixelText>(true))
                {
                    if (IsColorMatch(text.defaultColor, KAYCEE_RED_COLOR))
                        text.defaultColor = P03_BLUE_COLOR;
                    if (IsColorMatch(text.mainText.color, KAYCEE_RED_COLOR))
                        text.mainText.color = P03_BLUE_COLOR;
                    if (text.mainText.text.ToLowerInvariant().Contains("kaycee's mod"))
                        text.SetText("P03'S MOD");
                }
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
        [HarmonyPatch(typeof(AscensionStartScreen), nameof(AscensionStartScreen.Start))]
        [HarmonyBefore(GRIMORA_MOD, MAGNIFICUS_MOD)]
        [HarmonyPrefix]
        private static void AddP03StartOption(AscensionStartScreen __instance)
        {
            if (__instance.transform.Find("Center/MenuItems/Menu_Continue_P03") != null)
                return;

            GameObject startScreen = __instance.gameObject;

            GameObject newButton = startScreen.transform.Find($"Center/MenuItems/{menuItems[0]}").gameObject;
            newButton.GetComponentInChildren<PixelText>().SetText("- NEW LESHY RUN -");
            AscensionMenuInteractable newButtonController = newButton.GetComponent<AscensionMenuInteractable>();

            Vector3 newP03RunPos = startScreen.transform.Find($"Center/MenuItems/{menuItems[1]}").localPosition;

            // Make room for the new menu option
            for (int i = 1; i < menuItems.Length; i++)
            {
                Transform item = startScreen.transform.Find($"Center/MenuItems/{menuItems[i]}");
                item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y - Y_GAP, item.localPosition.z);
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

            // Make room for the continue menu option
            for (int i = 2; i < menuItems.Length; i++)
            {
                Transform item = startScreen.transform.Find($"Center/MenuItems/{menuItems[i]}");
                item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y - Y_GAP, item.localPosition.z);
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
            // AscensionMenuScreenTransition transitionController = startScreen.GetComponent<AscensionMenuScreenTransition>();
            RemoveDisabledContinueButtons(__instance);
            // transitionController.onEnableRevealedObjects.Insert(transitionController.onEnableRevealedObjects.IndexOf(newButton) + 1, newP03Button);
            // transitionController.onEnableRevealedObjects.Insert(transitionController.onEnableRevealedObjects.IndexOf(continueButton) + 1, continueP03Button);
            // transitionController.screenInteractables.Insert(transitionController.screenInteractables.IndexOf(newButtonController) + 1, newP03ButtonController);
            // transitionController.screenInteractables.Insert(transitionController.screenInteractables.IndexOf(continueButtonController) + 1, continueP03ButtonController);
        }

        private static readonly Dictionary<string, int> SORT_KEYS = new() {
            {"new", 10 }, {"continue", 20 }, {"stats", 30 },
            {"unlocks", 40 }, {"exit", 50 }, {"quit", 60 }
        };
        private static readonly Dictionary<string, int> SCRYBE_SORT_KEYS = new() {
            {"p03", 2 }, {"grim", 4 }, {"mag", 6 }
        };
        private static int MenuItemSortKey(GameObject item)
        {
            PixelText text = item.GetComponent<PixelText>();
            string itemKey = item.name.ToLowerInvariant();
            int sortValue = 1000;
            foreach (KeyValuePair<string, int> kvp in SORT_KEYS)
            {
                if (itemKey.Contains(kvp.Key))
                {
                    sortValue = kvp.Value;
                    break;
                }
            }
            foreach (KeyValuePair<string, int> kvp in SCRYBE_SORT_KEYS)
            {
                if (itemKey.Contains(kvp.Key))
                {
                    sortValue += kvp.Value;
                    break;
                }
                if (text != null && text.Text.ToLowerInvariant().Contains(kvp.Key))
                {
                    sortValue += kvp.Value;
                    break;
                }
            }
            return sortValue;
        }

        private static bool IsMenuItem(GameObject obj) => MenuItemSortKey(obj) < 1000;

        private static Transform FindLeshyContinueButton(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals("Continue", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (Transform grandchild in child)
                    {
                        if (!grandchild.gameObject.name.ToLowerInvariant().Contains("grim"))
                            return child;
                    }
                }
            }

            return null;
        }

        private static void RemoveDisabledContinueButtons(AscensionStartScreen __instance)
        {
            Transform itemsParent = __instance.transform.Find("Center/MenuItems");
            if (itemsParent == null)
                return;

            Transform leshyNewTransform = itemsParent.Find("Menu_New");
            Transform leshyContinueTransform = FindLeshyContinueButton(itemsParent);
            Transform p03ContinueTransform = itemsParent.Find("Menu_Continue_P03");

            if (leshyContinueTransform == null || p03ContinueTransform == null)
                return;

            if (P03AscensionSaveData.LeshyIsDead)
            {
                leshyNewTransform.gameObject.SetActive(false);
                leshyContinueTransform.gameObject.SetActive(false);
            }
            else
            {
                leshyContinueTransform.Find("Menu_Continue").gameObject.SetActive(__instance.RunExists);
                leshyContinueTransform.Find("Menu_Continue_DISABLED").gameObject.SetActive(!__instance.RunExists);
                leshyContinueTransform.gameObject.SetActive(__instance.RunExists);
            }

            p03ContinueTransform.Find("Menu_Continue").gameObject.SetActive(P03AscensionSaveData.P03RunExists);
            p03ContinueTransform.Find("Menu_Continue_DISABLED").gameObject.SetActive(!P03AscensionSaveData.P03RunExists);
            p03ContinueTransform.gameObject.SetActive(P03AscensionSaveData.P03RunExists);

            List<Transform> allItems = new();
            foreach (Transform child in itemsParent)
                allItems.Add(child);
            allItems = allItems.Where(t => t == leshyContinueTransform ? __instance.RunExists : t != p03ContinueTransform || P03AscensionSaveData.P03RunExists)
                               .OrderBy(t => MenuItemSortKey(t.gameObject)).ToList();

            for (int i = 0; i < allItems.Count; i++)
            {
                allItems[i].localPosition = new(
                    allItems[i].localPosition.x,
                    -i * Y_GAP,
                    allItems[i].localPosition.z
                );
            }

            AscensionMenuScreenTransition transitionController = __instance.GetComponent<AscensionMenuScreenTransition>();
            if (transitionController == null || transitionController.onEnableRevealedObjects == null)
                return;

            int firstMenu = 0;

            for (int i = 0; i < transitionController.onEnableRevealedObjects.Count; i++)
            {
                if (IsMenuItem(transitionController.onEnableRevealedObjects[i]))
                {
                    firstMenu = i;
                    break;
                }
            }

            transitionController.onEnableRevealedObjects = transitionController.onEnableRevealedObjects.Where(o => !IsMenuItem(o)).ToList();
            foreach (Transform item in allItems)
                transitionController.onEnableRevealedObjects.Insert(firstMenu, item.gameObject);
            transitionController.onEnableRevealedObjects = transitionController.onEnableRevealedObjects.Distinct().ToList();

            transitionController.screenInteractables = transitionController.onEnableRevealedObjects.SelectMany(g => g.GetComponentsInChildren<MainInputInteractable>()).ToList();

            if (P03AscensionSaveData.LeshyIsDead)
            {
                transitionController.onEnableRevealedObjects.Remove(leshyContinueTransform.gameObject);
                transitionController.onEnableRevealedObjects.Remove(leshyNewTransform.gameObject);

                foreach (var mii in leshyContinueTransform.GetComponentsInChildren<MainInputInteractable>())
                    transitionController.screenInteractables.Remove(mii);

                foreach (var mii in leshyNewTransform.GetComponentsInChildren<MainInputInteractable>())
                    transitionController.screenInteractables.Remove(mii);
            }
        }

        [HarmonyPatch(typeof(AscensionMenuScreenTransition), nameof(AscensionMenuScreenTransition.SequentiallyRevealContents))]
        [HarmonyPostfix]
        private static IEnumerator EnsureSetupBeforeInitialize(IEnumerator sequence, AscensionMenuScreenTransition __instance)
        {
            AscensionStartScreen startScreen = __instance.GetComponent<AscensionStartScreen>();
            if (startScreen != null)
            {
                yield return new WaitForEndOfFrame();
                RemoveDisabledContinueButtons(startScreen);
                yield return new WaitForEndOfFrame();
            }
            yield return sequence;
        }
    }
}