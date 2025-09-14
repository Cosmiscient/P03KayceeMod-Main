using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Guid;
using InscryptionAPI.Resource;
using InscryptionAPI.Saves;
using InscryptionAPI.Triggers;
using Sirenix.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public static class BattleModManager
    {
        public enum ID
        {
            Nothing = 0
        }

        public const string DEFAULT_ICON_PATH = "battlemodicons/default";

        internal class BattleModDefinition
        {
            internal ID ID { get; set; }
            internal string Title { get; set; }
            internal string DialogueId { get; set; }
            internal Type Behavior { get; set; }
            internal List<CardTemple> Regions { get; set; }
            internal bool BossValid { get; set; }
            internal Func<bool> SpecialRule { get; set; }
            internal string IconPath { get; set; }
            internal int Difficulty { get; set; }
        }

        internal static readonly List<BattleModDefinition> AllBattleMods = new();
        private static readonly Dictionary<string, List<ID>> AssignedBattleMods = new();
        private static readonly Dictionary<ID, NonCardTriggerReceiver> ActiveReceivers = new();
        private static bool CanShowActivation = false;

        static BattleModManager()
        {
            GameObject hazard = UnityEngine.Object.Instantiate(ResourceBank.Get<GameObject>("art/assets3d/misccustomholomap/damage_race_icon"));
            hazard.transform.localScale = new(0.4f, 0.4f, 0.4f);
            hazard.transform.localEulerAngles = new(0f, 0f, 180f);
            OnboardDynamicHoloPortrait.HolofyGameObject(hazard, GameColors.Instance.brightRed);
            hazard.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(hazard);
            ResourceBankManager.Add(P03Plugin.PluginGuid, DEFAULT_ICON_PATH, hazard, true);
        }

        public static bool RuleIsActive(ID ruleId)
        {
            var modInfo = AllBattleMods.First(bmd => bmd.ID == ruleId);
            if (!BoardManager.Instance.SafeIsUnityNull())
            {
                var component = BoardManager.Instance.gameObject.GetComponent(modInfo.Behavior);
                if (component != null)
                    return true;
            }
            return false;
        }

        public static NonCardTriggerReceiver GetActiveReceiver(ID id)
        {
            if (ActiveReceivers.ContainsKey(id))
                return ActiveReceivers[id];

            return null;
        }

        public static ID New(string modGuid, string title, List<string> introDialogue, Type behaviour, int difficulty = 1, List<CardTemple> regions = null, bool bossValid = false, string iconPath = DEFAULT_ICON_PATH)
        {
            if (!behaviour.IsSubclassOf(typeof(NonCardTriggerReceiver)))
                throw new InvalidOperationException("The battle modification behavior must be a subclass of NonCardTriggerReceiver");

            BattleModDefinition defn = new()
            {
                ID = GuidManager.GetEnumValue<ID>(modGuid, title),
                Title = title,
                DialogueId = DialogueManager.Add(modGuid, new()
                {
                    id = $"{modGuid}_battlemodintro_{title}",
                    speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, DialogueEvent.Speaker.P03 },
                    mainLines = new(introDialogue.Select(line => new DialogueEvent.Line() { text = line, specialInstruction = "", speakerIndex = 1, emotion = Emotion.Neutral, p03Face = P03AnimationController.Face.NoChange, }).ToList()),
                }).DialogueEvent.id,
                Behavior = behaviour,
                Regions = regions ?? new() { CardTemple.Nature, CardTemple.Tech, CardTemple.Undead, CardTemple.Wizard },
                BossValid = bossValid,
                IconPath = iconPath,
                Difficulty = difficulty != 0 ? difficulty : 1
            };

            AllBattleMods.Add(defn);
            return defn.ID;
        }

        public static void SetGlobalActivationRule(ID id, Func<bool> rule) => AllBattleMods.First(b => b.ID == id).SpecialRule = rule;

        internal static void ResetAllBlueprintRules()
        {
            AssignedBattleMods.Clear();
        }

        internal static void SetBlueprintRule(string blueprintId, ID battleModId)
        {
            if (!AssignedBattleMods.ContainsKey(blueprintId))
                AssignedBattleMods[blueprintId] = new();

            AssignedBattleMods[blueprintId].Add(battleModId);
        }

        internal static List<ID> GetBlueprintMods(CardBattleNodeData data)
        {
            List<ID> retval = new();
            if (data.blueprint != null)
            {
                if (AssignedBattleMods.ContainsKey(data.blueprint.name))
                {
                    if (AssignedBattleMods[data.blueprint.name] != null)
                        retval.AddRange(AssignedBattleMods[data.blueprint.name]);
                }
            }

            foreach (BattleModDefinition defn in AllBattleMods)
            {
                if (defn.SpecialRule != null && defn.SpecialRule() && (defn.BossValid || data is not BossBattleNodeData))
                    retval.Add(defn.ID);
            }

            return retval.Distinct().ToList();
        }

        public static IEnumerator GiveOneTimeIntroduction(ID id, View targetView = View.P03Face)
        {
            if (CanShowActivation)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BATTLE_MODIFIERS))
                    ChallengeActivationUI.TryShowActivation(AscensionChallengeManagement.BATTLE_MODIFIERS);
                CanShowActivation = false;
            }
            string hasSeenIntroId = $"HasSeenBattleModIntro_{id}";
            if (ModdedSaveManager.SaveData.GetValueAsBoolean(P03Plugin.PluginGuid, hasSeenIntroId))
                yield break;

            BattleModDefinition defn = AllBattleMods.FirstOrDefault(d => d.ID == id);
            if (defn == null)
                yield break;

            View currentView = ViewManager.Instance.CurrentView;
            if (currentView != targetView)
            {
                ViewManager.Instance.SwitchToView(View.P03Face);
                yield return new WaitForSeconds(0.2f);
            }
            yield return TextDisplayer.Instance.PlayDialogueEvent(defn.DialogueId, TextDisplayer.MessageAdvanceMode.Input);
            if (currentView != targetView)
            {
                ViewManager.Instance.SwitchToView(currentView);
                yield return new WaitForSeconds(0.2f);
            }

            ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, hasSeenIntroId, true);
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static IEnumerator CreateBattleModReceivers(IEnumerator sequence)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            CanShowActivation = true;

            // Guarantee we can't accidentally carry over a battlemod from a previous run
            foreach (BattleModDefinition defn in AllBattleMods)
            {
                Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.Behavior);
                if (!triggerComponent.SafeIsUnityNull())
                {
                    UnityEngine.Object.Destroy(triggerComponent);
                }
            }

            yield return new WaitForEndOfFrame();

            List<Component> allMods = new();
            foreach (ID id in GetBlueprintMods(TurnManager.Instance.BattleNodeData))
            {
                BattleModDefinition defn = AllBattleMods.FirstOrDefault(d => d.ID == id);
                if (defn == null)
                    continue;

                var component = BoardManager.Instance.gameObject.GetComponent(defn.Behavior)
                            ?? BoardManager.Instance.gameObject.AddComponent(defn.Behavior);
                ActiveReceivers[id] = component as NonCardTriggerReceiver;

                allMods.Add(component);
            }

            yield return sequence;

            foreach (Component comp in allMods)
            {
                if (comp is not null and IBattleModSetup ibms)
                    yield return ibms.OnBattleModSetup();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlacePreSetCards))]
        [HarmonyPostfix]
        private static IEnumerator ModifyTerrainByBattleMods(IEnumerator sequence, EncounterData encounterData)
        {
            foreach (EncounterData.StartCondition condition in encounterData.startConditions)
            {
                if (condition.cardsInPlayerSlots != null)
                    CustomTriggerFinder.CallAll<IModifyTerrain>(false, t => true, t => t.ModifyPlayerTerrain(condition.cardsInPlayerSlots));
                if (condition.cardsInOpponentSlots != null)
                    CustomTriggerFinder.CallAll<IModifyTerrain>(false, t => true, t => t.ModifyOpponentTerrain(condition.cardsInOpponentSlots));
                if (condition.cardsInOpponentQueue != null)
                    CustomTriggerFinder.CallAll<IModifyTerrain>(false, t => true, t => t.ModifyOpponentQueuedTerrain(condition.cardsInOpponentQueue));
            }
            // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
            // foreach (NonCardTriggerReceiver trigger in receivers)
            // {
            //     if (!trigger.SafeIsUnityNull() && trigger is IModifyTerrain imt)
            //     {
            //         foreach (EncounterData.StartCondition condition in encounterData.startConditions)
            //         {
            //             if (condition.cardsInPlayerSlots != null)
            //                 imt.ModifyPlayerTerrain(condition.cardsInPlayerSlots);
            //             if (condition.cardsInOpponentSlots != null)
            //                 imt.ModifyOpponentTerrain(condition.cardsInOpponentSlots);
            //             if (condition.cardsInOpponentQueue != null)
            //                 imt.ModifyOpponentQueuedTerrain(condition.cardsInOpponentQueue);
            //         }
            //     }
            // }

            yield return sequence;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static IEnumerator CleanupBattleModReceivers(IEnumerator sequence)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            CanShowActivation = false;

            // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
            // foreach (NonCardTriggerReceiver trigger in receivers)
            // {
            //     if (!trigger.SafeIsUnityNull() && trigger is IBattleModCleanup ibmc)
            //         yield return ibmc.OnBattleModCleanup();
            // }

            // // TODO: Go back to the custom trigger finder when it gets fixed
            // yield return CustomTriggerFinder.TriggerAll(
            //     false,
            //     delegate (IBattleModCleanup t)
            //     {
            //         P03Plugin.Log.LogInfo($"About to clean up {t}");
            //         return true;
            //     },
            //     t => t.OnBattleModCleanup()
            // );

            // yield return new WaitUntil(() => InputButtons.GetButton(Button.EndTurn));

            // Fuck it, do it this way:
            foreach (BattleModDefinition defn in AllBattleMods)
            {
                Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.Behavior);
                if (!triggerComponent.SafeIsUnityNull())
                {
                    if (triggerComponent is IBattleModCleanup ibmc)
                    {
                        P03Plugin.Log.LogInfo($"About to clean up {triggerComponent}");
                        yield return ibmc.OnBattleModCleanup();
                    }
                }
            }

            yield return new WaitForEndOfFrame();

            P03Plugin.Log.LogInfo($"About to destroy battle mods");

            foreach (BattleModDefinition defn in AllBattleMods)
            {
                Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.Behavior);
                if (!triggerComponent.SafeIsUnityNull())
                {
                    UnityEngine.Object.Destroy(triggerComponent);
                    yield return new WaitForEndOfFrame();
                }
            }

            yield return sequence;
        }

        // [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.VeryLow)]
        // private static void DeleteBattleModReceivers()
        // {
        //     foreach (BattleModDefinition defn in AllBattleMods)
        //     {
        //         Component triggerComponent = BoardManager.Instance.gameObject.GetComponent(defn.Behavior);
        //         if (!triggerComponent.SafeIsUnityNull())
        //             UnityEngine.Object.Destroy(triggerComponent);
        //     }
        // }

        [HarmonyPatch(typeof(BoardStateSimulator), nameof(BoardStateSimulator.SimulateCombatPhase))]
        [HarmonyPrefix]
        private static void MakeAIRecognizeBattleState(BoardState board, bool playerIsAttacker)
        {
            try
            {
                // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
                // foreach (NonCardTriggerReceiver trigger in receivers)
                // {
                //     if (!trigger.SafeIsUnityNull() && trigger is IBattleModSimulator ibms && ibms.HasBoardStateAdjustment(board, playerIsAttacker))
                //         ibms.DoBoardStateAdjustment(board, playerIsAttacker);
                // }

                // TODO: Replace when the custom trigger finder is fixed
                CustomTriggerFinder.CallAll<IBattleModSimulator>(
                    false,
                    t => t.HasBoardStateAdjustment(board, playerIsAttacker),
                    t => t.DoBoardStateAdjustment(board, playerIsAttacker)
                );
            }
            catch (Exception ex)
            {
                P03Plugin.Log.LogError(ex);
            }
        }

        [HarmonyPatch(typeof(BoardStateEvaluator), nameof(BoardStateEvaluator.EvaluateCard))]
        [HarmonyPostfix]
        private static void MakeAIEvaluateBattleState(BoardState.CardState card, BoardState board, ref int __result)
        {
            try
            {
                // int sum = 0;
                // List<NonCardTriggerReceiver> receivers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
                // foreach (NonCardTriggerReceiver trigger in receivers)
                // {
                //     if (!trigger.SafeIsUnityNull() && trigger is IBattleModSimulator ibms && ibms.HasCardEvaluationAdjustment(card, board))
                //         sum += ibms.DoCardEvaluationAdjustment(card, board);
                // }
                // TODO: Replace when trigger finder is fixe
                List<(TriggerReceiver, int)> result = CustomTriggerFinder.CollectDataAll<IBattleModSimulator, int>(
                    false,
                    t => t.HasCardEvaluationAdjustment(card, board),
                    t => t.DoCardEvaluationAdjustment(card, board)
                );
                __result += result.Sum(t => t.Item2);
            }
            catch (Exception ex)
            {
                P03Plugin.Log.LogError(ex);
            }
        }

        internal const float X_OFFSET = 0.5f;
        internal const float Y_OFFSET = 0.5f;

        internal static readonly Dictionary<char, List<Vector3>> OFFSETS = new()
        {
            { 'W', new()
                {
                    new(0f, 0f, -Y_OFFSET),
                    new(0f, 0f, Y_OFFSET),
                    new(X_OFFSET, 0f, 0f),
                    new(X_OFFSET, 0f, -Y_OFFSET),
                    new(X_OFFSET, 0f, Y_OFFSET),
                    new(-X_OFFSET, 0f, -Y_OFFSET),
                    new(-X_OFFSET, 0f, Y_OFFSET),
                }
            },
            { 'E', new()
                {
                    new(0f, 0f, -Y_OFFSET),
                    new(0f, 0f, Y_OFFSET),
                    new(-X_OFFSET, 0f, 0f),
                    new(-X_OFFSET, 0f, -Y_OFFSET),
                    new(-X_OFFSET, 0f, Y_OFFSET),
                    new(X_OFFSET, 0f, -Y_OFFSET),
                    new(X_OFFSET, 0f, Y_OFFSET),
                }
            },
            { 'N', new()
                {
                    new(-X_OFFSET, 0f, 0f),
                    new(X_OFFSET, 0f, 0f),
                    new(0f, 0f, -Y_OFFSET),
                    new(-X_OFFSET, 0f, -Y_OFFSET),
                    new(X_OFFSET, 0f, -Y_OFFSET),
                    new(-X_OFFSET, 0f, Y_OFFSET),
                    new(X_OFFSET, 0f, Y_OFFSET),
                }
            },
            { 'S', new()
                {
                    new(-X_OFFSET, 0f, 0f),
                    new(X_OFFSET, 0f, 0f),
                    new(0f, 0f, Y_OFFSET),
                    new(-X_OFFSET, 0f, Y_OFFSET),
                    new(X_OFFSET, 0f, Y_OFFSET),
                    new(-X_OFFSET, 0f, -Y_OFFSET),
                    new(X_OFFSET, 0f, -Y_OFFSET),
                }
            }
        };

        [Obsolete]
        private static void ActivateIconSet(HoloMapNode skull)
        {
            if (skull.Data is not CardBattleNodeData cbnd)
                return;

            HoloMapNode node = skull.GetComponent<HoloMapNode>();
            Transform parent = skull.transform.Find("RendererParent");

            string blueprintId = skull.blueprintData.name;
            bool isBossBattle = skull.Data is BossBattleNodeData;
            List<ID> ids = GetBlueprintMods(cbnd);

            List<GameObject> oldContainers = new();
            foreach (Transform t in parent)
            {
                if (t.gameObject.name.StartsWith("BattleModIcons"))
                    oldContainers.Add(t.gameObject);
            }

            foreach (GameObject g in oldContainers)
            {
                Renderer[] renderers = g.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    if (!r.SafeIsUnityNull() && skull.nodeRenderers.Contains(r))
                        skull.nodeRenderers.Remove(r);
                }

                UnityEngine.Object.Destroy(g);
            }

            Material refMat = parent.Find("ShortArrow").GetComponent<Renderer>().material;

            Transform additional = parent.Find("AdditionalIcon");
            bool active = additional != null && additional.gameObject.active;

            for (int i = 0; i < ids.Count; i++)
            {
                GameObject obj = new($"BattleModIcons_{ids[i]}");
                obj.transform.SetParent(parent);
                obj.transform.position = additional.position + OFFSETS[skull.name.Last()][i];
                obj.transform.eulerAngles = new(-85f, -180f, 0f);
                obj.transform.localScale = new(1f, 1f, 1f);

                GameObject prefab = ResourceBank.Get<GameObject>(AllBattleMods.Find(d => d.ID == ids[i]).IconPath);
                GameObject icon = UnityEngine.Object.Instantiate(prefab, obj.transform);
                OnboardDynamicHoloPortrait.HolofyGameObject(icon, GameColors.Instance.gold, reference: refMat);
                icon.transform.localPosition = Vector3.zero;
                icon.transform.localEulerAngles = new(-90f, 0f, 0f);
                icon.SetActive(true);

                Renderer[] renderers = icon.GetComponentsInChildren<Renderer>();
                skull.nodeRenderers.AddRange(renderers);

                obj.SetActive(active);
            }
        }

        [HarmonyPatch(typeof(HoloMapNode), nameof(HoloMapNode.Awake))]
        [HarmonyPostfix]
        [Obsolete]
        private static void HoveringEffectsForBattleMod(HoloMapNode __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (__instance.blueprintData != null)
                ActivateIconSet(__instance);
        }

        [HarmonyPatch(typeof(MoveHoloMapAreaNode), nameof(MoveHoloMapAreaNode.SetAdditionalIconShown))]
        [HarmonyPostfix]
        private static void UpdateAdditionalSpecialModIcons(MoveHoloMapAreaNode __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            Transform parent = __instance.transform.Find("RendererParent");
            foreach (Transform child in parent)
            {
                if (child.gameObject.name.StartsWith("BattleModIcons"))
                    child.gameObject.SetActive(__instance.additionalIconRenderer.gameObject.activeSelf);
            }
        }
    }
}