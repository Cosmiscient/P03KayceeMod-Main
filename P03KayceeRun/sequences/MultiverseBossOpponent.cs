using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class MultiverseBossOpponent : Part3BossOpponent
    {
        public override string PreIntroDialogueId => "P03RoyalBossIntro";
        public override string PostDefeatedDialogueId => "NO_DIALOGUE";
        public override string BossLoopID => "P03_Phase3";

        public static bool IsActiveForRun => P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("multiverse") || AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.FinalBoss);

        public override bool GiveCurrencyOnDefeat => false;

        public override IEnumerator StartBattleSequence()
        {
            NumLives = 1;
            yield break;
        }

        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            this.SetSceneEffectsShown(true);
            this.StartMainAudioLoop(this.BossLoopID);
            yield return this.StartBattleSequence();
            yield return TextDisplayer.Instance.PlayDialogueEvent(PreIntroDialogueId, TextDisplayer.MessageAdvanceMode.Input);
            yield break;
        }

        public override void SetSceneEffectsShown(bool shown)
        {
            if (shown)
            {
                MultiverseGameState.LightColorState.GetPreset(GameColors.Instance.blue).RestoreState();

                if (MultiverseBattleSequencer.Instance.CyberspaceParticles == null)
                {
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
                }
            }
        }
    }
}