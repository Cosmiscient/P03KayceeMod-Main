using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    internal static class AssetBundleManager
    {
        internal static List<Shader> Shaders;

        internal static Dictionary<string, GameObject> Prefabs = new();

        static AssetBundleManager()
        {
            string[] files = Directory.GetFiles(Paths.PluginPath, "p03assetbundle", SearchOption.AllDirectories);
            var pathToAssetBundle = files.FirstOrDefault();
            var bundle = AssetBundle.LoadFromFile(pathToAssetBundle);

            Shaders = new(bundle.LoadAllAssets<Shader>());

            Prefabs = bundle.LoadAllAssets<GameObject>().ToDictionary(o => o.name);

            bundle.Unload(false);
        }
    }
}