using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Quests;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class DredgerBossOpponent : Part3BossOpponent
    {
        public override string PreIntroDialogueId => "DredgerPreBoss";
        public override string PostDefeatedDialogueId => string.Empty;

        [HarmonyPatch(typeof(Part3BossOpponent), nameof(Part3BossOpponent.StartingLives), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool StartingLivesForDredger(Part3BossOpponent __instance, ref int __result)
        {
            if (__instance is DredgerBossOpponent)
            {
                __result = 1;
                return false;
            }
            return true;
        }

        public override void SetSceneEffectsShown(bool shown)
        {
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Dredger);
            base.SetSceneEffectsShown(true);
        }

        public override IEnumerator DefeatedPlayerSequence()
        {
            ViewManager.Instance.SwitchToView(View.Default, lockAfter: true);
            yield return new WaitForSeconds(0.25f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("DredgerPostBossLose", TextDisplayer.MessageAdvanceMode.Input);
            yield return new WaitForSeconds(0.1f);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            DefaultQuestDefinitions.DredgerBattle.CurrentState.Status = QuestState.QuestStateStatus.Failure;
        }

        public override IEnumerator PreDefeatedSequence()
        {
            ViewManager.Instance.SwitchToView(View.Default, lockAfter: true);
            yield return new WaitForSeconds(0.25f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("DredgerPostBossWin", TextDisplayer.MessageAdvanceMode.Input);
            yield return new WaitForSeconds(0.1f);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            DefaultQuestDefinitions.DredgerBattle.CurrentState.Status = QuestState.QuestStateStatus.Success;
        }
    }
}