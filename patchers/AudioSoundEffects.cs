using System;
using System.Linq;
using HarmonyLib;
using InscryptionAPI.Sound;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    internal static class AudioSoundEffects
    {
        [HarmonyPatch(typeof(AudioController), nameof(AudioController.Awake))]
        [HarmonyPostfix]
        internal static void LoadMyCustomAudio(ref AudioController __instance)
        {
            // Please dont' break anything...
            foreach (string clipName in new string[] { "bottle_break", "angel_reveal", "fireball", "molotov" })
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
    }
}