using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using InscryptionAPI.Sound;
using UnityEngine.Networking;
using System;
using BepInEx;
using System.Linq;

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
            foreach (string clipName in new string[] { "bottle_break", "angel_reveal" })
            {
                if (!__instance.SFX.Any(ac => ac.name.Equals(clipName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    AudioClip expl = SoundManager.LoadAudioClip(P03Plugin.PluginGuid, $"{clipName}.wav");
                    expl.name = clipName;
                    __instance.SFX.Add(expl);
                }
            }
        }
    }
}