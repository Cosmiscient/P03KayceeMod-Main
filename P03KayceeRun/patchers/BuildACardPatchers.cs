using System.Collections.Generic;
using System.Linq;
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
            if (P03AscensionSaveData.IsP03Run)
            {
                __result.Remove(Ability.DrawCopyOnDeath);
                __result.Remove(Ability.GainBattery);
                foreach (Ability ab in AscensionAbilities)
                {
                    if (!__result.Contains(ab))
                        __result.Add(ab);
                }

                int randomSeed = P03AscensionSaveData.RandomSeed + 25;
                __result = __result.Distinct().OrderBy(a => SeededRandom.Value(randomSeed++) * 1000).Take(8).ToList();
            }
        }

        [HarmonyPatch(typeof(AbilityScreenButton), nameof(AbilityScreenButton.OnAddPressed))]
        [HarmonyPrefix]
        private static bool OnAddPressedFix(ref AbilityScreenButton __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (__instance.Ability == Ability.None)
            {
                __instance.SetIconShown(true);
                if (__instance.abilityChoices == null || __instance.abilityChoices.Count < 8)
                    __instance.abilityChoices = BuildACardInfo.GetValidAbilities();
                __instance.Ability = __instance.abilityChoices[0];
            }
            __instance.OnLeftOrRightPressed(false);
            return false;
        }

        [HarmonyPatch(typeof(RecycleCardSequencer), nameof(RecycleCardSequencer.GetCardStatPointsValue))]
        [HarmonyPostfix]
        private static void AdjustSPBasedOnUselessMods(CardInfo info, ref int __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            // New formula.
            // 1 base. +1 per ability. +1 for having been gemified by the player. Just simplify this shit.
            // It's simpler to process and fix by a lot
            if (info.name.Equals(ExpansionPackCards_2.RINGWORM_CARD))
            {
                __result = 6;
            }
            else
            {
                __result = 1 + info.Abilities.Count + (info.Gemified && !CardLoader.GetCardByName(info.name).Gemified ? 1 : 0);
            }
        }


        public class BuildACardNameInputHandler : KeyboardInputHandler
        {
            public List<TextMeshPro> Target = new();
            public BuildACardScreen ScreenParent;

            private new void Awake()
            {
                // Do nothing
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

                if (ScreenParent.info != null && ScreenParent.info.mod != null)
                    ScreenParent.info.mod.nameReplacement = KeyboardInput;
            }
        }

        [HarmonyPatch(typeof(BuildACardScreen), nameof(BuildACardScreen.Initialize))]
        [HarmonyPostfix]
        private static void SetupManualEntry(BuildACardScreen __instance)
        {
            foreach (UpDownScreenButtons btn in __instance.nameButtons)
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

            if (P03AscensionSaveData.IsP03Run)
            {
                GameObject nameScreen = __instance.stageInteractableParents[(int)BuildACardScreen.Stage.Name];
                BuildACardNameInputHandler handler = nameScreen.GetComponent<BuildACardNameInputHandler>();
                if (handler == null)
                {
                    handler = nameScreen.AddComponent<BuildACardNameInputHandler>();
                    handler.EnterPressed = () => __instance.OnRightArrowPressed();
                    handler.Target.Add(__instance.nameTexts[1]);
                    handler.Target.Add(__instance.confirmScreenTitle);
                    handler.maxInputLength = 25;
                    handler.ScreenParent = __instance;
                }
                handler.KeyboardInput = "ENTER NAME HERE";
                __instance.info.nameIndices = new int[] { -1, -1, -1 };
            }
        }

        [HarmonyPatch(typeof(BuildACardScreen), nameof(BuildACardScreen.ShowStage))]
        [HarmonyPostfix]
        private static void TurnOffUpDownNameButtons(BuildACardScreen __instance)
        {
            foreach (UpDownScreenButtons btn in __instance.nameButtons)
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