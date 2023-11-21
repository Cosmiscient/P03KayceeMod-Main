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
            set => ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "ScreenState", value);
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "TransitionToGame")]
        [HarmonyPrefix]
        [HarmonyBefore("zorro.inscryption.infiniscryption.packmanager")]
        public static void InitializeP03SaveData(ref AscensionMenuScreens __instance, bool newRun)
        {
            if (newRun)
            {
                if (ScreenState == CardTemple.Tech)
                {
                    // Ensure the old part 3 save data gets saved if it needs to be
                    P03AscensionSaveData.EnsureRegularSave();
                    P03AscensionSaveData.IsP03Run = true;
                    SaveManager.SaveToFile();
                }
                else
                {
                    P03AscensionSaveData.IsP03Run = false;
                }
            }
            if (P03AscensionSaveData.IsP03Run)
                ScreenState = CardTemple.Tech;
        }

        [HarmonyPatch(typeof(MenuController), "LoadGameFromMenu")]
        [HarmonyPrefix]
        public static bool LoadGameFromMenu(bool newGameGBC)
        {
            if (!newGameGBC && SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
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
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "SwitchToScreen")]
        [HarmonyPrefix]
        public static void ClearP03SaveOnNewRun(AscensionMenuScreens.Screen screen)
        {
            if (screen == AscensionMenuScreens.Screen.Start) // At the main screen, you can't be in any style of run. Not yet.
            {
                ClearP03Data();
            }
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "Start")]
        [HarmonyPrefix]
        public static void ClearScreenStatePrefix() => ClearP03Data();

        private static readonly string[] menuItems = new string[] { "Menu_New", "Continue", "Menu_Stats", "Menu_Unlocks", "Menu_Exit", "Menu_QuitApp" };
        [HarmonyPatch(typeof(AscensionMenuScreens), "Start")]
        [HarmonyPostfix]
        public static void AddP03StartOption()
        {
            Traverse menuScreens = Traverse.Create(AscensionMenuScreens.Instance);
            GameObject startScreen = menuScreens.Field("startScreen").GetValue<GameObject>();

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

            // Add to transition
            AscensionMenuScreenTransition transitionController = startScreen.GetComponent<AscensionMenuScreenTransition>();
            Traverse transitionTraverse = Traverse.Create(transitionController);
            List<GameObject> onEnableRevealedObjects = transitionTraverse.Field("onEnableRevealedObjects").GetValue<List<GameObject>>();
            List<MainInputInteractable> screenInteractables = transitionTraverse.Field("screenInteractables").GetValue<List<MainInputInteractable>>();

            onEnableRevealedObjects.Insert(onEnableRevealedObjects.IndexOf(newButton) + 1, newP03Button);
            screenInteractables.Insert(screenInteractables.IndexOf(newButtonController) + 1, newP03ButtonController);
        }
    }
}