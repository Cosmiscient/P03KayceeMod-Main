using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Items.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Tools
{
    public static Assembly _assembly;
    public static Assembly CurrentAssembly => _assembly ??= Assembly.GetExecutingAssembly();

    public static GameObject Particle;

    public static Texture2D LoadTexture(string name)
    {
        if (name == null)
        {
            return null;
        }
        return TextureHelper.GetImageAsTexture(name + (name.EndsWith(".png") ? "" : ".png"), CurrentAssembly);
    }
}
