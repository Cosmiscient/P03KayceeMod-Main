using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Saves;
using TMPro;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class TestOfStrengthBattleSequencer : DamageRaceBattleSequencer
    {
        public static int HighScore
        {
            get { return ModdedSaveManager.SaveData.GetValueAsInt(P03Plugin.PluginGuid, "DamageRaceHighScore"); }
            set { ModdedSaveManager.SaveData.SetValue(P03Plugin.PluginGuid, "DamageRaceHighScore", value); }
        }

        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            return new EncounterData
            {
                opponentType = BossManagement.TestOfStrengthOpponent,
                opponentTurnPlan = new()
            };
        }

        public override IEnumerator DamageAddedToScale(int amount, bool playerIsAttacker)
        {
            if (playerIsAttacker)
            {
                this.damageDealt += amount;
                ViewManager.Instance.SwitchToView(View.Scales);
                yield return new WaitForSeconds(0.1f);
                int amount2 = Mathf.Min(amount, 12 - this.damageDealt + amount);
                yield return DamageRaceGenerator.Instance.ShowAddDamage(amount2, 0.1f);
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }

        public override IEnumerator OpponentUpkeep()
        {
            this.turnsRemaining--;
            DamageRaceGenerator.Instance.SetAlarmPitch(1f + 0.1f * (float)(6 - this.turnsRemaining));
            ViewManager.Instance.SwitchToView(View.P03Face, false, true);
            GameObject gameObject = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Countdown, true, true);
            TextMeshPro countdownText = gameObject.GetComponentInChildren<TextMeshPro>();
            countdownText.text = (this.turnsRemaining + 1).ToString();
            yield return new WaitForSeconds(1f);
            AudioController.Instance.PlaySound3D("p03_cord_unplug", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.transform.position, 1f, 0f, null, null, null, null, false);
            P03AnimationController.Instance.SetHeadTrigger("twitch_right");
            countdownText.text = this.turnsRemaining.ToString();
            yield return new WaitForSeconds(1f);

            if (this.turnsRemaining == 0)
            {
                yield return new WaitForSeconds(1f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03TestOfStrengthOver", TextDisplayer.MessageAdvanceMode.Input, variableStrings: new string[1] { this.damageDealt.ToString() });

                if (this.damageDealt > HighScore)
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03TestOfStrengthOverNewRecord", TextDisplayer.MessageAdvanceMode.Input);
                    HighScore = this.damageDealt;

                    if (HighScore >= 50)
                        AchievementManager.Unlock(P03AchievementManagement.TEST_OF_STRENGTH);
                }
                else
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03TestOfStrengthOverNoRecord", TextDisplayer.MessageAdvanceMode.Input, variableStrings: new string[1] { HighScore.ToString() });
                }

                LifeManager.Instance.OpponentDamage += LifeManager.Instance.DamageUntilPlayerWin;
            }
            else
            {
                P03AnimationController.Instance.SwitchToFace((this.turnsRemaining <= 3) ? P03AnimationController.Face.Happy : P03AnimationController.Face.Default, true, true);
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }
    }
}