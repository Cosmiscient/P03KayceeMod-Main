using System.Collections;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MultiverseBossOpponent : Part3BossOpponent
    {
        public override string PreIntroDialogueId => "P03RoyalBossIntro";
        public override string PostDefeatedDialogueId => "NO_DIALOGUE";
        public override string BossLoopID => "P03_Phase3";

        public static bool IsActiveForRun => AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.FinalBoss) || P03AscensionSaveData.LeshyIsDead;

        public override bool GiveCurrencyOnDefeat => false;

        public override IEnumerator StartBattleSequence()
        {
            NumLives = 1;
            yield break;
        }

        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            this.SetSceneEffectsShown(true);
            AudioController.Instance.StopAllLoops();
            AudioController.Instance.SetLoopAndPlay(this.BossLoopID, 0, true, true);
            AudioController.Instance.SetLoopVolumeImmediate(0.4f, 0);
            yield return this.StartBattleSequence();
            yield return TextDisplayer.Instance.PlayDialogueEvent(PreIntroDialogueId, TextDisplayer.MessageAdvanceMode.Input);
            yield break;
        }

        public override void SetSceneEffectsShown(bool shown)
        {
            if (shown)
            {
                MultiverseGameState.LightColorState.GetPreset(GameColors.Instance.blue).RestoreState();

                float durationOfEffect = 0.45f;

                Transform itemTrans = ItemsManager.Instance.gameObject.transform;
                Vector3 newItemPos = new(6.75f, itemTrans.localPosition.y, itemTrans.localPosition.z + 1.3f);
                Tween.LocalPosition(itemTrans, newItemPos, durationOfEffect, 0f);

                Transform hammerTrans = ItemsManager.Instance.Slots.FirstOrDefault(s => s.name.ToLowerInvariant().StartsWith("hammer")).gameObject.transform;
                Vector3 newHammerPos = new(-9.5f, hammerTrans.localPosition.y, hammerTrans.localPosition.z - 1.3f);
                Tween.LocalPosition(hammerTrans, newHammerPos, durationOfEffect, 0f);

                Transform bellTrans = (BoardManager.Instance as BoardManager3D).bell.gameObject.transform;
                Vector3 newBellPos = new(-5f, bellTrans.localPosition.y, bellTrans.localPosition.z);
                Tween.LocalPosition(bellTrans, newBellPos, durationOfEffect, 0f);

                Transform scaleTrans = LifeManager.Instance.Scales3D.gameObject.transform;
                Vector3 newScalePos = new(-6, scaleTrans.localPosition.y, scaleTrans.localPosition.z);
                Tween.LocalPosition(scaleTrans, newScalePos, durationOfEffect, 0f);

                if (MultiverseBattleSequencer.Instance.CyberspaceParticles == null)
                {
                    // This means we have to do a lot of the setup again 
                    GameObject cyberspace = GameObject.Instantiate(AssetBundleManager.Prefabs["Cyberspace_Particles"], TurnManager.Instance.transform);
                    cyberspace.name = "Cyberspace_Particles";
                    cyberspace.transform.localPosition = new(0f, 10f, 16f);
                    cyberspace.transform.localEulerAngles = new(0f, 180f, 0f);

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

                    P03AnimationController.Instance.SetAntennaShown(true);
                    P03AnimationController.Instance.SetWifiColor(GameColors.Instance.blue);
                }
            }
        }
    }
}