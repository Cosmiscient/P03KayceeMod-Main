using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using TMPro;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BuildACardPatcher
    {
        public static readonly Ability[] AscensionAbilities = new Ability[] {
            Ability.Strafe,
            Ability.CellBuffSelf,
            Ability.CellTriStrike,
            Ability.ConduitNull,
            Ability.DeathShield,
            Ability.ExplodeOnDeath,
            Ability.LatchBrittle,
            Ability.LatchDeathShield,
            Ability.RandomAbility,
            Ability.ConduitFactory,
            Ability.ConduitSpawnGems,
            Ability.DrawVesselOnHit,
            FullOfOil.AbilityID,
            FullyLoaded.AbilityID,
            Ability.GainGemBlue,
            Ability.GainGemGreen,
            Ability.GainGemOrange,
            TreeStrafe.AbilityID,
            Ability.ExplodeGems,
            LatchRampage.AbilityID,
            LatchSwapper.AbilityID,
            Ability.GemDependant,
            VesselHeart.AbilityID,
            SnakeStrafe.AbilityID,
            Ability.DrawCopy,
            BurntOut.AbilityID,
            Molotov.AbilityID,
            FireBomb.AbilityID,
            MissileStrike.AbilityID
        };

        [HarmonyPatch(typeof(BuildACardInfo), nameof(BuildACardInfo.GetValidAbilities))]
        [HarmonyPostfix]
        public static void NoRecursionForAscension(ref List<Ability> __result)
        {
            if (SaveFile.IsAscension)
            {
                __result.Remove(Ability.DrawCopyOnDeath);
                __result.Remove(Ability.GainBattery);
                foreach (Ability ab in AscensionAbilities)
                {
                    if (!__result.Contains(ab))
                        __result.Add(ab);
                }

                __result = __result.Distinct().Randomize().Take(8).ToList();
            }
        }

        [HarmonyPatch(typeof(AbilityScreenButton), nameof(AbilityScreenButton.OnAddPressed))]
        [HarmonyPrefix]
        private static bool OnAddPressedFix(ref AbilityScreenButton __instance)
        {
            if (__instance.Ability == Ability.None)
            {
                __instance.SetIconShown(true);
                __instance.Ability = __instance.abilityChoices[0];
            }
            __instance.OnLeftOrRightPressed(false);
            return false;
        }

        public class BuildACardNameInputHandler : KeyboardInputHandler
        {
            public List<TextMeshPro> Target = new();
            public BuildACardInfo Info;

            private new void Awake()
            {
                // Do nthing
            }

            public override void OnEnable()
            {
                base.OnEnable();
                EnteredInput = false;
            }

            public override void ManagedUpdate()
            {
                base.ManagedUpdate();
                Target.ForEach(t => t.SetText(KeyboardInput));

                if (Info != null && Info.mod != null)
                    Info.mod.nameReplacement = KeyboardInput;
            }
        }

        [HarmonyPatch(typeof(BuildACardScreen), nameof(BuildACardScreen.Initialize))]
        [HarmonyPostfix]
        private static void SetupManualEntry(BuildACardScreen __instance)
        {
            foreach (var btn in __instance.nameButtons)
            {
                btn.upButton.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
                btn.downButton.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
                btn.upSprite.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
                btn.downSprite.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
            }
            __instance.nameTexts[0].gameObject.SetActive(!P03AscensionSaveData.IsP03Run);

            if (P03AscensionSaveData.IsP03Run)
            {
                __instance.nameTexts[1].SetText(Localization.Translate("ENTER NAME HERE"));
                __instance.nameTexts[1].gameObject.GetComponent<RectTransform>().sizeDelta = new(15f, 2.2f);
            }

            __instance.nameTexts[2].gameObject.SetActive(!P03AscensionSaveData.IsP03Run);

            var nameScreen = __instance.stageInteractableParents[(int)BuildACardScreen.Stage.Name];
            var handler = nameScreen.GetComponent<BuildACardNameInputHandler>();
            if (handler == null)
            {
                handler = nameScreen.AddComponent<BuildACardNameInputHandler>();
                handler.EnterPressed = () => __instance.OnRightArrowPressed();
                handler.Target.Add(__instance.nameTexts[1]);
                handler.Target.Add(__instance.confirmScreenTitle);
                handler.Info = __instance.info;
                handler.KeyboardInput = "ENTER NAME HERE";
                handler.maxInputLength = 25;
            }

            __instance.info.nameIndices = new int[] { -1, -1, -1 };
        }

        [HarmonyPatch(typeof(BuildACardScreen), nameof(BuildACardScreen.ShowStage))]
        [HarmonyPostfix]
        private static void TurnOffUpDownNameButtons(BuildACardScreen __instance)
        {
            foreach (var btn in __instance.nameButtons)
            {
                btn.upButton.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
                btn.downButton.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
                btn.upSprite.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
                btn.downSprite.gameObject.SetActive(!P03AscensionSaveData.IsP03Run);
            }
        }

        [HarmonyPatch(typeof(BuildACardInfo), nameof(BuildACardInfo.GetName))]
        [HarmonyPrefix]
        private static bool DontMakeNameSometimes(ref BuildACardInfo __instance, ref string __result)
        {
            if (__instance.nameIndices == null || __instance.nameIndices[0] < 0)
            {
                __result = __instance.mod.nameReplacement;
                return false;
            }
            return true;
        }
    }
}