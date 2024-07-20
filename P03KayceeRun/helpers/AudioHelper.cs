using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Sound;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    [HarmonyPatch]
    public static class AudioHelper
    {
        // I'm keeping all of my audio clips in a dictionary. Here. To try to prevent memory leaks
        private static Dictionary<string, AudioClip> audioCache = new();

        private static AudioClip GetAudioClip(string clipname)
        {
            if (audioCache.ContainsKey(clipname))
                return audioCache[clipname];
            audioCache[clipname] = SoundManager.LoadAudioClip(P03Plugin.PluginGuid, clipname);
            return audioCache[clipname];
        }

        internal static void FlushAudioClipCache()
        {
            foreach (var key in audioCache.Keys)
                GameObject.Destroy(audioCache[key]);
            audioCache.Clear();
        }

        public struct AudioState
        {
            public int sourceNum;
            public string clipName;
            public float position;
            public bool isPlaying;
            public float volume;
        }

        public static List<AudioState> PauseAllLoops()
        {
            Traverse controller = Traverse.Create(AudioController.Instance);
            List<AudioSource> sources = controller.Field("loopSources").GetValue<List<AudioSource>>();

            List<AudioState> retval = new();
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];

                if (source == null || source.clip == null)
                {
                    retval.Add(new AudioState
                    {
                        sourceNum = i,
                        position = 0f,
                        clipName = default,
                        isPlaying = false
                    });
                    continue;
                }

                retval.Add(new AudioState
                {
                    sourceNum = i,
                    position = source.isPlaying ? source.time / source.clip.length : 0f,
                    clipName = source.clip.name,
                    isPlaying = source.isPlaying,
                    volume = source.volume
                });
            }

            AudioController.Instance.StopAllLoops();
            return retval;
        }

        public static void ResumeAllLoops(List<AudioState> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].isPlaying)
                {
                    AudioController.Instance.SetLoopAndPlay(states[i].clipName, i, true, true);
                    AudioController.Instance.SetLoopVolumeImmediate(0f, i);
                    AudioController.Instance.SetLoopTimeNormalized(states[i].position, i);
                    AudioController.Instance.FadeInLoop(1f, states[i].volume, new int[] { i });
                }
                else
                {
                    AudioController.Instance.StopLoop(i);
                }
            }
        }

        [HarmonyPatch(typeof(AudioController), nameof(AudioController.TrySetLoop))]
        [HarmonyPrefix]
        internal static void LoadMyLoops(string loopName, AudioController __instance)
        {
            AudioClip loop = __instance.GetLoop(loopName);
            if (loop == null)
            {
                // Please dont' break anything...
                foreach (string clipName in new string[] { "P03_Phase1", "P03_Phase2", "P03_Phase3", "spooky_background", "dark_mist" })
                {
                    if (!__instance.Loops.Any(ac => ac.name.Equals(clipName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        AudioClip expl = GetAudioClip($"{clipName}.mp3");
                        if (expl != null)
                        {
                            expl.name = clipName;
                            __instance.Loops.Add(expl);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AudioController), nameof(AudioController.Awake))]
        [HarmonyPostfix]
        internal static void LoadMyCustomAudio(ref AudioController __instance)
        {
            // Please dont' break anything...
            foreach (string clipName in new string[] { "speechblip_jamescobb_internal", "speechblip_melter", "speechblip_sawyerpatel", "speechblip_jamescobb", "cam_switch", "anime_sword_hit_2", "multiverse_teleport", "bottle_break", "static", "ufo", "big_tv_break", "small_tv_break" })
            {
                if (!__instance.SFX.Any(ac => ac.name.Equals(clipName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    AudioClip expl = GetAudioClip($"{clipName}.wav");
                    if (expl != null)
                    {
                        expl.name = clipName;
                        __instance.SFX.Add(expl);
                    }
                }
            }

            LoadMyLoops("P03_Phase1", __instance);
        }
    }
}