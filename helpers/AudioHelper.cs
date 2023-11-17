using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InscryptionAPI.Sound;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    [HarmonyPatch]
    public static class AudioHelper
    {
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
            if (loop == null && loopName.StartsWith("P03"))
            {
                // Please dont' break anything...
                foreach (string clipName in new string[] { "P03_Phase1", "P03_Phase2", "P03_Phase3" })
                {
                    if (!__instance.Loops.Any(ac => ac.name.Equals(clipName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        AudioClip expl = SoundManager.LoadAudioClip(P03Plugin.PluginGuid, $"{clipName}.mp3");
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
            foreach (string clipName in new string[] { "bottle_break", "angel_reveal", "fireball", "molotov", "static", "missile_launch", "missile_explosion", "shred", "ufo", "big_tv_break", "small_tv_break" })
            {
                if (!__instance.SFX.Any(ac => ac.name.Equals(clipName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    AudioClip expl = SoundManager.LoadAudioClip(P03Plugin.PluginGuid, $"{clipName}.wav");
                    if (expl != null)
                    {
                        expl.name = clipName;
                        __instance.SFX.Add(expl);
                    }
                }
            }
        }

        // public static string FindResourceName(string key, string type, Assembly target)
        // {
        //     string lowerKey = $".{key.ToLowerInvariant()}.{type.ToLowerInvariant()}";
        //     foreach (string resourceName in target.GetManifestResourceNames())
        //         if (resourceName.ToLowerInvariant().EndsWith(lowerKey))
        //             return resourceName;

        //     return default(string);
        // }

        // private static byte[] GetResourceBytes(string key, string type, Assembly target)
        // {
        //     string resourceName = FindResourceName(key, type, target);

        //     if (string.IsNullOrEmpty(resourceName))
        //     {
        //         string errorHelp = "";
        //         foreach (string testName in target.GetManifestResourceNames())
        //             errorHelp += "," + testName;
        //         throw new InvalidDataException($"Could not find resource matching {key}. This is what I have: {errorHelp}");
        //     }

        //     using (Stream resourceStream = target.GetManifestResourceStream(resourceName))
        //     {
        //         using (MemoryStream memStream = new MemoryStream())
        //         {
        //             resourceStream.CopyTo(memStream);
        //             return memStream.ToArray();
        //         }
        //     }
        // }

        // private static string WriteWavToFile(string wavname)
        // {
        //     byte[] wavBytes = GetResourceBytes(wavname, "wav", Assembly.GetExecutingAssembly());
        //     string tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{wavname}.wav");
        //     File.WriteAllBytes(tempPath, wavBytes);
        //     return tempPath;
        // }

        // public static string FindAudioClip(string clipName)
        // {
        //     string fname = $"{clipName}.wav";
        //     string[] found = Directory.GetFiles(Paths.PluginPath, $"{clipName}.wav", SearchOption.AllDirectories);
        //     if (found.Length > 0)
        //         return found[0];

        //     throw new InvalidOperationException($"Could not find any file matching {clipName}");
        // }

        // public static void LoadAudioClip(string clipname, string group = "Loops")
        // {

        //     // Is this a hack?
        //     // Hell yes, this is a hack.

        //     Traverse audioController = Traverse.Create(AudioController.Instance);
        //     List<AudioClip> clips = audioController.Field(group).GetValue<List<AudioClip>>();

        //     if (clips.Find(clip => clip.name.Equals(clipname)) != null)
        //         return;

        //     //string manualPath = WriteWavToFile(clipname);
        //     string manualPath = FindAudioClip(clipname);

        //     string url = $"file://{manualPath.Replace("#", "%23")}";

        //     P03Plugin.Log.LogInfo($"About to get audio clip at {url}");

        //     using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        //     {
        //         request.SendWebRequest();
        //         while (request.IsExecuting()) ; // Wait for this thing to finish

        //         if (request.isHttpError)
        //         {
        //             throw new InvalidOperationException($"Bad request getting audio clip {request.error}");
        //         }
        //         else
        //         {
        //             AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        //             clip.name = clipname;

        //             clips.Add(clip);
        //         }
        //     }
        // }
    }
}