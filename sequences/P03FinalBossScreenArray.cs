using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
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

        private readonly List<P03FinalBossExtraScreen> AllScreens = new();

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
        private const float UPDATE_STEP = 0.075f;
        private const int VOLUME_SAMPLE_SIZE = 1024;
        private readonly float[] clipSampleData = new float[VOLUME_SAMPLE_SIZE];
        private float currentUpdateTime = 0f;
        private float clipLoudness = 0f;
        private Color currentColor = P03FinalBossExtraScreen.FrameColor;

        // Stuff for face management
        private bool showingLoadingFaces = false;

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
                        loop.clip.GetData(clipSampleData, loop.timeSamples);
                        clipLoudness = 0f;
                        foreach (float sample in clipSampleData)
                        {
                            clipLoudness += Mathf.Abs(sample);
                        }
                        clipLoudness /= VOLUME_SAMPLE_SIZE; //clipLoudness is what you are looking for

                        float newScale = P03FinalBossExtraScreen.DEFAULT_SCALE + (clipLoudness / 10f);
                        Vector3 newScaleVec = new(newScale, newScale, newScale);
                        foreach (P03FinalBossExtraScreen screen in AllScreens)
                            screen.transform.localScale = newScaleVec;
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
                lScreen.gameObject.name = $"Screen_{x}_{y}";
                lScreen.transform.localEulerAngles = new(0f, VERTICAL_ROTATION * x, 0f);
                lScreen.transform.localPosition = new((Math.Sign(x) * (3f - X_SPACING)) + (X_SPACING * x), 0.5f + (Y_SPACING * y), -Z_SPACING * (Math.Abs(x) - 1));

                // Some special screens
                lScreen.PulsesWithMusic = screenPoints.Count > 12;
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

        private bool CanShowFace(P03FinalBossExtraScreen screen) => !showingLoadingFaces || !screen.RespondsToDownloading;

        private IEnumerator ShowFaceSequence(float delay, Func<P03FinalBossExtraScreen, bool> screenFilter, params P03AnimationController.Face[] face)
        {
            if (!_initialized)
                yield return new WaitUntil(() => _initialized);

            yield return new WaitUntil(() => !_inFaceDisplay);

            _inFaceDisplay = true;

            List<P03FinalBossExtraScreen> screensCopy = new(AllScreens.Where(screenFilter ?? CanShowFace));

            while (screensCopy.Count > 0)
            {
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

        public void StartLoadingFaces()
        {
            showingLoadingFaces = true;
            StartCoroutine(ShowFaceSequence(0.04f, s => s.RespondsToDownloading, P03FinalBossExtraScreen.LOADING_FACE));
        }

        public void EndLoadingFaces(params P03AnimationController.Face[] face)
        {
            showingLoadingFaces = false;

            if (face.Length == 0)
                StartCoroutine(ShowFaceSequence(0.04f, null, P03AnimationController.Face.Happy, P03AnimationController.Face.Bored, P03AnimationController.Face.Default));
            else
                StartCoroutine(ShowFaceSequence(0.04f, null, face));
        }

        public void RecolorFrames(Color color)
        {
            currentColor = color;
            foreach (P03FinalBossExtraScreen screen in AllScreens)
                screen.FrameRenderer.material.SetColor("_EmissionColor", color);
        }
    }
}