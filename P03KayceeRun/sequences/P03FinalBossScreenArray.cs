using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class P03FinalBossScreenArray : MonoBehaviour
    {
        private const float X_SPACING = 2.8f;
        private const float Y_SPACING = 2f;
        private const float Z_SPACING = 0.75f;
        private const float VERTICAL_ROTATION = 9f;
        private const int RENDER_SPACING = 20;
        private const float SECONDS_PER_BEAT = 60f / 45f;

        private readonly List<P03FinalBossExtraScreen> AllScreens = new();
        private P03FinalBossExtraScreen BigMoonScreen = null;

        private static readonly List<Tuple<int, int>> LOADING_SCREENS = new()
        {
            new(-1, 0),
            new(-4, 0),
            new(-2, 1),
            new(-3, 2),
            new(1, 0),
            new(3, 0),
            new(3, 1),
            new(2, 2)
        };

        private static bool IsLoadingScreen(Tuple<int, int> pt) => LOADING_SCREENS.Any(t => t.Item1 == pt.Item1 && t.Item2 == pt.Item2);

        private bool _initialized = false;
        private bool _inFaceDisplay = false;

        // Stuff for the volume brightness
        private const float UPDATE_STEP = 0.2f;
        private const int VOLUME_SAMPLE_SIZE = 1024;
        //private readonly float[] clipSampleData = new float[VOLUME_SAMPLE_SIZE];
        private float currentUpdateTime = 0f;
        private readonly float clipLoudness = 0f;

        private static readonly List<Texture> Gears = new() {
            TextureHelper.GetImageAsTexture("card_slot_spin1.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin2.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin3.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin4.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin5.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin6.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin7.png", typeof(P03FinalBossScreenArray).Assembly),
            TextureHelper.GetImageAsTexture("card_slot_spin8.png", typeof(P03FinalBossScreenArray).Assembly),
        };

        private static int GearIndex = 0;

        // Stuff for face management
        private bool showingLoadingFaces = false;
        private readonly bool showingMoonFace = false;

        private void Update()
        {
            currentUpdateTime += Time.deltaTime;
            if (currentUpdateTime >= UPDATE_STEP)
            {
                currentUpdateTime = 0f;

                foreach (AudioSource loop in AudioController.Instance.loopSources)
                {
                    if (loop.isPlaying)
                    {
                        // Rotation of gears
                        int beatCount = Mathf.RoundToInt((loop.time - (SECONDS_PER_BEAT / 2f)) / SECONDS_PER_BEAT);
                        int gearTarget = beatCount % Gears.Count;
                        if (GearIndex != gearTarget)
                        {
                            GearIndex = gearTarget;
                            foreach (CardSlot slot in BoardManager.Instance.AllSlots)
                            {
                                if (slot.GetSlotModification() == SlotModificationManager.ModificationType.NoModification)
                                    slot.SetTexture(Gears[GearIndex]);
                            }

                            foreach (P03FinalBossExtraScreen screen in AllScreens.Where(s => s.PulsesWithMusic))
                            {
                                Tween.LocalScale(screen.transform, P03FinalBossExtraScreen.BUMP_SCALE_VECTOR, 0.2f, 0f);
                                Tween.LocalScale(screen.transform, P03FinalBossExtraScreen.DEFAULT_SCALE_VECTOR, 0.2f, 0.2f);
                            }
                        }
                        break;

                        // Loudness pop of screens
                        // if (!showingMoonFace)
                        // {
                        //     loop.clip.GetData(clipSampleData, loop.timeSamples);
                        //     clipLoudness = 0f;
                        //     foreach (float sample in clipSampleData)
                        //     {
                        //         clipLoudness += Mathf.Abs(sample);
                        //     }
                        //     clipLoudness /= VOLUME_SAMPLE_SIZE; //clipLoudness is what you are looking for

                        //     float newScale = P03FinalBossExtraScreen.DEFAULT_SCALE + (clipLoudness / 10f);
                        //     Vector3 newScaleVec = new(newScale, newScale, newScale);
                        //     foreach (P03FinalBossExtraScreen screen in AllScreens.Where(s => s.PulsesWithMusic))
                        //         screen.transform.localScale = newScaleVec;
                        // }
                    }
                }
            }
        }

        private IEnumerator BuildScreens()
        {
            // Build them in a random order
            List<Tuple<int, int>> screenPoints = new();
            for (int x = 1; x < 5; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    screenPoints.Add(new(x, y));

                    if (x > 0)
                        screenPoints.Add(new(-x, y));
                }
            }

            while (screenPoints.Count > 0)
            {
                Tuple<int, int> curPt = screenPoints[UnityEngine.Random.RandomRangeInt(0, screenPoints.Count)];
                screenPoints.Remove(curPt);
                int x = curPt.Item1;
                int y = curPt.Item2;

                P03FinalBossExtraScreen lScreen = P03FinalBossExtraScreen.Create(transform);
                lScreen.xIndex = x;
                lScreen.yIndex = y;
                lScreen.gameObject.name = $"Screen_{x}_{y}";
                lScreen.transform.localEulerAngles = new(0f, VERTICAL_ROTATION * x, 0f);
                lScreen.transform.localPosition = new((Math.Sign(x) * (3f - X_SPACING)) + (X_SPACING * x), 0.5f + (Y_SPACING * y), -Z_SPACING * (Math.Abs(x) - 1));

                // Some special screens
                lScreen.PulsesWithMusic = true;
                lScreen.RespondsToDownloading = IsLoadingScreen(curPt);

                Transform lCamera = lScreen.transform.Find("RenderCamera");
                lCamera.transform.position = new(Math.Sign(x) * RENDER_SPACING * Math.Abs(x), RENDER_SPACING * (y + 1), -50);
                AllScreens.Add(lScreen);

                yield return new WaitForSeconds(0.05f);
            }

            _initialized = true;
        }

        public static P03FinalBossScreenArray Create(Transform parent)
        {
            GameObject screens = new("ScreenArray");
            screens.transform.SetParent(parent);
            screens.transform.localPosition = new(0f, 0f, 7f);

            P03FinalBossScreenArray array = screens.AddComponent<P03FinalBossScreenArray>();
            array.StartCoroutine(array.BuildScreens());
            return array;
        }

        private bool CanShowFace(P03FinalBossExtraScreen screen)
        {
            if (showingMoonFace)
            {
                if (screen.xIndex is not (< (-1) or > 1))
                    return false;
            }
            return !showingLoadingFaces || !screen.RespondsToDownloading;
        }

        public void Collapse()
        {
            for (int i = 0; i < AllScreens.Count; i++)
            {
                P03FinalBossExtraScreen screen = AllScreens[i];
                Tween.LocalPosition(screen.transform, screen.transform.localPosition + (Vector3.down * 5f), 0.4f, 0.02f * i, completeCallback: () => Destroy(screen.gameObject));
            }

            if (BigMoonScreen != null)
                Tween.LocalPosition(BigMoonScreen.transform, BigMoonScreen.transform.localPosition + (Vector3.down * 7f), 0.35f, 0f);

            CustomCoroutine.WaitThenExecute(0.5f, () => AudioController.Instance.PlaySound2D("big_tv_break", MixerGroup.TableObjectsSFX, 1f));
            AllScreens.Clear();
            enabled = false;
        }

        public void ShowBigMoon()
        {
            if (BigMoonScreen != null)
                return;

            // Go ahead and create the new
            BigMoonScreen = P03FinalBossExtraScreen.Create(transform, hugeScreen: true);
            BigMoonScreen.GetComponentInChildren<Animator>().enabled = false;
            BigMoonScreen.transform.localPosition = new(0f, -16.4f, 0.2f); // have it sit just in front of the other
            BigMoonScreen.ShowFace(P03FinalBossExtraScreen.BIG_MOON_FACE);
            BigMoonScreen.name = "Big_Moon";
            BigMoonScreen.transform.localEulerAngles = Vector3.zero;
            BigMoonScreen.FrameRenderer.material.SetColor("_EmissionColor", P03FinalBossExtraScreen.RedFrameColor);

            Tween.LocalPosition(BigMoonScreen.transform, new Vector3(0f, -8.1f, 0f), 6f, 0f);
            foreach (P03FinalBossExtraScreen s in AllScreens.Where(s => Math.Abs(s.xIndex) == 1))
            {
                CustomCoroutine.WaitThenExecute(1.8f * s.yIndex, delegate ()
                {
                    P03FinalBossExtraScreen.Orbiter orbiter = s.StartRotation(transform);
                    if (s.xIndex > 0)
                        orbiter.RotationSpeed *= -1;
                    orbiter.Stop(immediate: false, destroyAfter: true, stopTheta: 0f);
                });
            }
        }

        private IEnumerator ShowFaceSequence(float delay, Func<P03FinalBossExtraScreen, bool> screenFilter, params P03AnimationController.Face[] face)
        {
            if (!_initialized)
                yield return new WaitUntil(() => _initialized);

            yield return new WaitUntil(() => !_inFaceDisplay);

            _inFaceDisplay = true;

            List<P03FinalBossExtraScreen> screensCopy = new(AllScreens.Where(screenFilter ?? CanShowFace));

            while (screensCopy.Count > 0)
            {
                if (delay > 0)
                    yield return new WaitForSeconds(delay);

                P03FinalBossExtraScreen screen = screensCopy[UnityEngine.Random.RandomRangeInt(0, screensCopy.Count)];
                screensCopy.Remove(screen);

                if (face.Length == 1)
                    screen.ShowFace(face[0]);
                else
                    screen.ShowFace(face[UnityEngine.Random.RandomRangeInt(0, face.Length)]);
            }

            _inFaceDisplay = false;
        }

        public void ShowFaceWithDelay(P03AnimationController.Face face, float delay = 0.04f) => StartCoroutine(ShowFaceSequence(delay, null, face));

        public void ShowFace(params P03AnimationController.Face[] face) => StartCoroutine(ShowFaceSequence(0.04f, null, face));

        public void ShowFaceImmediate(params P03AnimationController.Face[] face) => StartCoroutine(ShowFaceSequence(0.00f, null, face));

        public void StartLoadingFaces()
        {
            showingLoadingFaces = true;
            StartCoroutine(ShowFaceSequence(0.04f, s => s.RespondsToDownloading, P03FinalBossExtraScreen.LOADING_FACE));
        }

        public void EndLoadingFaces(params P03AnimationController.Face[] face)
        {
            try
            {
                showingLoadingFaces = false;

                if (face.Length == 0)
                    StartCoroutine(ShowFaceSequence(0.01f, null, P03AnimationController.Face.Happy, P03AnimationController.Face.Bored, P03AnimationController.Face.Default));
                else
                    StartCoroutine(ShowFaceSequence(0.01f, null, face));
            }
            catch
            {
                // Do nothing for now
            }
        }

        public void RecolorFrames(Color color)
        {
            foreach (P03FinalBossExtraScreen screen in AllScreens)
                screen.FrameRenderer.material.SetColor("_EmissionColor", color);
        }
    }
}