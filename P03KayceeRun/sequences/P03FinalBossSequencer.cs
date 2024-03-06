using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.LightningBolt;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class P03FinalBossSequencer : BossBattleSequencer
    {
        public override Opponent.Type BossType => BossManagement.P03FinalBossOpponent;

        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_P03;

        public static readonly string[] MODS = new string[] { "Special Hammer Mod", "Incredible Drafting Mod", "The Community API", "Super-Duper Unity Editor" };

        public P03AscensionOpponent P03AscensionOpponent => TurnManager.Instance.opponent as P03AscensionOpponent;

        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            EncounterData data = base.BuildCustomEncounter(nodeData);
            data.aiId = BossManagement.P03FinalBossAI;
            return data;
        }

        private int upkeepCounter = -1;

        public override IEnumerator PlayerUpkeep()
        {
            if (P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("multiverse"))
            {
                yield return LifeManager.Instance.ShowDamageSequence(20, 1, false);
                P03AscensionOpponent.NumLives = 1;
                yield break;
            }
        }

        public override IEnumerator OpponentUpkeep()
        {
            upkeepCounter += 1;
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default);

            if (TurnManager.Instance.opponent.NumLives == 1)
                yield break;

            int sequenceNumber = upkeepCounter <= 8 ? upkeepCounter : ((upkeepCounter - 1) % 6) + 1;

            switch (upkeepCounter)
            {
                case 1:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[0], upkeepCounter == 1, upkeepCounter <= 8);
                    yield break;

                case 2:
                    yield return P03AscensionOpponent.HammerSequence();
                    yield break;

                case 3:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[1], false, upkeepCounter <= 8);
                    yield break;

                case 4:
                    yield return P03AscensionOpponent.ExchangeTokensSequence();
                    yield break;

                case 5:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[2], false, upkeepCounter <= 8);
                    yield break;

                case 6:
                    yield return P03AscensionOpponent.APISequence();
                    yield break;

                case 7:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[3], false, upkeepCounter <= 8);
                    yield break;

                case 8:
                    yield return P03AscensionOpponent.UnityEngineSequence();
                    yield break;
                default:
                    break;
            }
        }

        private readonly List<GameObject> antennaLightning = new();
        private void AddAntennaLightning(Vector3 offset)
        {
            GameObject lightning = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"));
            lightning.GetComponent<LightningBoltScript>().StartObject = P03AnimationController.Instance.antenna;
            lightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 0.5f;
            lightning.GetComponent<LightningBoltScript>().EndPosition = P03AnimationController.Instance.antenna.transform.position + offset + (Vector3.up * 2f);
            antennaLightning.Add(lightning);
        }

        private void DeleteAllLightning()
        {
            foreach (var obj in antennaLightning)
                GameObject.Destroy(obj);

            antennaLightning.Clear();
        }

        private IEnumerator BlinkInOutInner(GameObject obj)
        {
            if (obj.activeSelf)
            {
                obj.SetActive(false);
                float waitTime = 0.125f;
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(waitTime);
                    obj.SetActive(true);
                    yield return new WaitForSeconds(waitTime);
                    obj.SetActive(false);
                    waitTime /= 2f;
                }
                yield break;
            }
            else
            {
                obj.SetActive(true);
                float waitTime = 0.125f;
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(waitTime);
                    obj.SetActive(false);
                    yield return new WaitForSeconds(waitTime);
                    obj.SetActive(true);
                    waitTime /= 2f;
                }
                yield break;
            }
        }

        private void BlinkInOut(GameObject obj)
        {
            StartCoroutine(BlinkInOutInner(obj));
        }

        public override IEnumerator GameEnd(bool playerWon)
        {
            OpponentAnimationController.Instance.ClearLookTarget();
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            TableVisualEffectsManager.Instance.ResetTableColors();
            InteractionCursor.Instance.InteractionDisabled = true;
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            if (playerWon)
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03BeatFinalBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Happy, true, true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03NothingMatters", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.DefaultUpwards, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ThreeMovesAhead", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
                FactoryScrybes scrybes = FactoryManager.Instance.Scrybes;
                scrybes.Show();
                yield return new WaitForSeconds(0.2f);
                P03AnimationController.Instance.SetHeadTrigger("neck_snap");

                if (!MultiverseBossOpponent.IsActiveForRun)
                {
                    CustomCoroutine.WaitOnConditionThenExecute(() => P03AnimationController.Instance.CurrentFace == P03AnimationController.Face.Choking, delegate
                    {
                        AchievementManager.Unlock(P03AchievementManagement.FIRST_WIN);
                        if (AscensionChallengeManagement.SKULL_STORM_ACTIVE)
                            AchievementManager.Unlock(P03AchievementManagement.SKULLSTORM);
                        AudioController.Instance.PlaySound3D("p03_head_off", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.transform.position, 1f, 0f, null, null, null, null, false);
                    });
                    yield return new WaitForSeconds(12f);
                    P03AnimationController.Instance.gameObject.SetActive(false);
                    scrybes.leshy.SetEyesAnimated(true);
                    yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyFinalBossDialogue", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    yield return new WaitForSeconds(0.5f);
                    StoryEventsData.SetEventCompleted(EventManagement.HAS_DEFEATED_P03);
                    AscensionStatsData.TryIncrementStat(AscensionStat.Type.BossesDefeated);
                    EventManagement.FinishAscension(true);
                    yield break;
                }

                // We need to transition to the multiverse boss
                yield return new WaitForSeconds(1.8f);
                P03AnimationController.Instance.SwitchToFace(P03TrollFace.ID);
                yield return new WaitForSeconds(1.0f);

                GameObject leftArmDuplicate = MaterialHelper.CreateMatchingAnimatedObject(P03AnimationController.Instance.headAnim.transform.Find("LeftLeshyArm").gameObject, this.transform);
                GameObject rightArmDuplicate = MaterialHelper.CreateMatchingAnimatedObject(P03AnimationController.Instance.headAnim.transform.Find("RightLeshyArm").gameObject, this.transform);

                leftArmDuplicate.SetActive(true);
                rightArmDuplicate.SetActive(true);

                var leftTween = Tween.Shake(leftArmDuplicate.transform, leftArmDuplicate.transform.position, new Vector3(0.1f, 0.1f, 0.1f), 1f, 0f, Tween.LoopType.Loop);
                var rightTween = Tween.Shake(rightArmDuplicate.transform, rightArmDuplicate.transform.position, new Vector3(0.1f, 0.1f, 0.1f), 1f, 0f, Tween.LoopType.Loop);

                P03AnimationController.Instance.headAnim.Rebind();

                PauseMenu.pausingDisabled = true;
                AudioController.Instance.SetLoopAndPlay($"spooky_background", 0, true, true);
                AudioController.Instance.SetLoopVolumeImmediate(0.4f, 0);

                yield return new WaitForSeconds(0.5f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseIntro", TextDisplayer.MessageAdvanceMode.Input);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default);
                scrybes.leshy.SetEyesAnimated(true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyMultiverseIntro", TextDisplayer.MessageAdvanceMode.Input);
                scrybes.leshy.SetEyesAnimated(false);

                Tween.LocalPosition(ViewManager.Instance.cameraParent, new Vector3(0f, 7.65f, -1.66f), 3f, 0f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseSmug", TextDisplayer.MessageAdvanceMode.Input);

                string username = IntroSequence.GetUserName();
                yield return string.IsNullOrEmpty(username)
                    ? TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseSmugWithoutName", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null)
                    : TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseSmugWithName", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] { username }, null);

                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseSmugPartTwo", TextDisplayer.MessageAdvanceMode.Input);

                scrybes.leshy.SetEyesAnimated(true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyMultiversePlease", TextDisplayer.MessageAdvanceMode.Auto);
                yield return new WaitForSeconds(0.5f);
                P03AnimationController.Instance.SwitchToFace(P03TrollFace.ID);
                TextDisplayer.Instance.Interrupt();
                AudioController.Instance.SetLoopPaused(true);

                leftTween.Stop();
                rightTween.Stop();
                GlitchOutAssetEffect.GlitchModel(leftArmDuplicate.transform);
                GlitchOutAssetEffect.GlitchModel(rightArmDuplicate.transform);

                scrybes.leshy.gameObject.SetActive(false);
                P03AscensionSaveData.SetLeshyDead(true, true);

                yield return new WaitForSeconds(2f);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Happy);
                ViewManager.Instance.CurrentView = View.Default;
                ViewManager.Instance.SwitchToView(View.DefaultUpwards, false, false);

                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseConnect", TextDisplayer.MessageAdvanceMode.Input);
                P03AnimationController.Instance.SetAntennaShown(true);
                //P03AnimationController.Instance.antenna.transform.Find("WifiParticles").gameObject.SetActive(false);
                P03AnimationController.Instance.SetWifiColor(GameColors.Instance.blue);
                yield return new WaitForSeconds(0.25f);

                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseConnectTwo", TextDisplayer.MessageAdvanceMode.Input);
                //AddAntennaLightning(Vector3.up * 4f);

                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);
                AudioController.Instance.SetLoopAndPlay($"dark_mist", 0, true, true);
                AudioController.Instance.SetLoopVolumeImmediate(0.4f, 0);

                GameObject cyberspace = GameObject.Instantiate(AssetBundleManager.Prefabs["Cyberspace_Particles"], TurnManager.Instance.transform);
                cyberspace.name = "Cyberspace_Particles";
                cyberspace.transform.localPosition = new(0f, 10f, 16f);
                cyberspace.transform.localEulerAngles = new(0f, 180f, 0f);

                yield return new WaitForSeconds(1.5f);
                ViewManager.Instance.SwitchToView(View.HandCuff);
                HandcuffArmAnimationController.Instance.ShowAttemptEscape();
                yield return new WaitForSeconds(0.3f);
                CameraEffects.Instance.Shake(0.05f, 0.2f);
                yield return new WaitForSeconds(0.4f);

                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseEscape", TextDisplayer.MessageAdvanceMode.Input);

                ViewManager.Instance.SwitchToView(View.DefaultUpwards);

                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseEscapeTwo", TextDisplayer.MessageAdvanceMode.Input);
                ViewManager.Instance.SwitchToView(View.P03FaceClose);
                yield return new WaitForSeconds(0.25f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03GreaterTranscendence", TextDisplayer.MessageAdvanceMode.Input);

                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);
                ViewManager.Instance.SwitchToView(View.DefaultUpwards);

                // yield return new WaitForSeconds(0.2f);
                // AddAntennaLightning(Vector3.right * 50f);
                // yield return new WaitForSeconds(0.2f);
                // AddAntennaLightning(new Vector3(5f, 1f, -2f) * 2f);
                // yield return new WaitForSeconds(0.2f);
                // AddAntennaLightning(new Vector3(-5f, 1f, 2f) * 2f);
                // yield return new WaitForSeconds(0.2f);
                // AddAntennaLightning(new Vector3(-5f, 1f, -2f) * 2f);
                // yield return new WaitForSeconds(0.2f);
                // AddAntennaLightning(new Vector3(5f, 1f, 2f) * 2f);

                GameObject grimora = GameObject.Instantiate(FactoryManager.Instance.scrybes.grimora.gameObject, FactoryManager.Instance.scrybes.grimora.transform.parent);
                GameObject magnificus = GameObject.Instantiate(FactoryManager.Instance.scrybes.magnificus.gameObject, FactoryManager.Instance.scrybes.magnificus.transform.parent);

                OnboardDynamicHoloPortrait.HolofyGameObject(grimora, GameColors.Instance.darkLimeGreen);
                OnboardDynamicHoloPortrait.HolofyGameObject(magnificus, GameColors.Instance.darkRed);

                grimora.SetActive(false);
                magnificus.SetActive(false);


                yield return new WaitForSeconds(1.75f);

                BlinkInOut(grimora);
                yield return TextDisplayer.Instance.PlayDialogueEvent("GrimorasPlan", TextDisplayer.MessageAdvanceMode.Input);
                BlinkInOut(magnificus);
                yield return TextDisplayer.Instance.PlayDialogueEvent("MagnificusResponseToPlan", TextDisplayer.MessageAdvanceMode.Input);
                yield return TextDisplayer.Instance.PlayDialogueEvent("GrimorasPlanTwo", TextDisplayer.MessageAdvanceMode.Input);

                yield return new WaitForSeconds(0.5f);
                BlinkInOut(grimora);
                BlinkInOut(magnificus);
                yield return new WaitForSeconds(1.0f);

                for (int i = 0; i < cyberspace.transform.childCount; i++)
                {
                    if (cyberspace.transform.GetChild(i).gameObject.activeSelf)
                    {
                        ParticleSystem particles = cyberspace.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                        ParticleSystem.EmissionModule emission = particles.emission;
                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(10f);
                    }
                    else
                    {
                        cyberspace.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }

                yield return TextDisplayer.Instance.PlayDialogueEvent("P03MultiverseFound", TextDisplayer.MessageAdvanceMode.Input);

                InteractionCursor.Instance.InteractionDisabled = false;
                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                PauseMenu.pausingDisabled = false;
            }
            else
            {
                P03AscensionOpponent.ScreenArray.EndLoadingFaces(P03AnimationController.Face.Happy);
                yield return new WaitForSeconds(1.5f);

                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03LostFinalBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(1.5f);
                EventManagement.FinishAscension(false);
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.TransitionToNextGameState))]
        [HarmonyPrefix]
        [HarmonyPriority(HarmonyLib.Priority.VeryHigh)]
        private static bool TransitionDirectlyToMultiverseBoss(TurnManager __instance)
        {
            if (MultiverseBossOpponent.IsActiveForRun && __instance.SpecialSequencer is P03FinalBossSequencer)
            {
                CardBattleNodeData data = new()
                {
                    difficulty = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty),
                    specialBattleId = BossBattleSequencer.GetSequencerIdForBoss(BossManagement.P03MultiverseOpponent)
                };
                GameFlowManager.Instance.TransitionToGameState(GameState.CardBattle, data);
                return false;
            }
            return true;
        }

        public override IEnumerator DamageAddedToScale(int amount, bool playerIsAttacker)
        {
            if (playerIsAttacker)
            {
                P03AscensionOpponent.ScreenArray.ShowFace(
                    P03AnimationController.Face.Angry,
                    P03AnimationController.Face.Choking,
                    P03FinalBossExtraScreen.LOOKUP_FACE
                );
            }
            else
            {
                P03AscensionOpponent.ScreenArray.ShowFace(
                    P03AnimationController.Face.Happy,
                    P03AnimationController.Face.Bored,
                    P03AnimationController.Face.Happy
                );
            }
            yield return base.DamageAddedToScale(amount, playerIsAttacker);
        }
    }
}