using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InscryptionAPI.Sound;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Helpers
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
            audioCache[clipname] = SoundManager.LoadAudioClip(P03SigilLibraryPlugin.PluginGuid, clipname);
            return audioCache[clipname];
        }

        internal static void FlushAudioClipCache()
        {
            foreach (var key in audioCache.Keys)
                GameObject.Destroy(audioCache[key]);
            audioCache.Clear();
        }

        [HarmonyPatch(typeof(AudioController), nameof(AudioController.Awake))]
        [HarmonyPostfix]
        internal static void LoadMyCustomAudio(ref AudioController __instance)
        {
            // Please dont' break anything...
            foreach (string clipName in new string[] { "angel_reveal", "fireball", "missile_explosion", "missile_launch", "molotov", "shred" })
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
        }
    }
}