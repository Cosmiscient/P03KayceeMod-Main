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

        static AssetBundleManager()
        {
            string[] files = Directory.GetFiles(Paths.PluginPath, "p03assetbundle", SearchOption.AllDirectories);
            var pathToAssetBundle = files.FirstOrDefault();
            var bundle = AssetBundle.LoadFromFile(pathToAssetBundle);

            Shaders = new(bundle.LoadAllAssets<Shader>());
            foreach (var sh in Shaders)
                P03Plugin.Log.LogInfo($"Loaded Shader {sh.name} from Asset Bundle");

            bundle.Unload(false);
        }
    }
}