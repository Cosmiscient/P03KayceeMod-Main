using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using DiskCardGame;
using InscryptionAPI.Resource;
using Sirenix.Utilities;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Helpers
{
    internal static class AssetBundleManager
    {
        internal static List<Shader> Shaders;

        internal static Dictionary<string, GameObject> Prefabs = new();

        static AssetBundleManager()
        {
            string[] files = Directory.GetFiles(Paths.PluginPath, "p03sigilbundle", SearchOption.AllDirectories);
            var pathToAssetBundle = files.FirstOrDefault();
            var bundle = AssetBundle.LoadFromFile(pathToAssetBundle);

            Shaders = new(bundle.LoadAllAssets<Shader>());

            Prefabs = bundle.LoadAllAssets<GameObject>().ToDictionary(o => o.name);
            Prefabs.ForEach(kvp => ResourceBankManager.Add(P03SigilLibraryPlugin.PluginGuid, $"p03kcm/prefabs/{kvp.Key}", kvp.Value, true));

            bundle.Unload(false);
        }

        internal static void CleanUp()
        {
            foreach (string key in Prefabs.Keys)
                GameObject.Destroy(Prefabs[key]);
        }

        public static void HolofyGameObject(GameObject obj, Color color, string shaderKey = "SFHologram/HologramShader", bool inChildren = true, Material reference = null, bool destroyComponents = false, float? brightness = null)
        {
            if (destroyComponents)
            {
                List<Component> compsToDestroy = new();
                foreach (Type c in new List<Type>() { typeof(Rigidbody), typeof(AutoRotate), typeof(Animator), typeof(InteractableBase), typeof(Item) })
                    compsToDestroy.AddRange(inChildren ? obj.GetComponentsInChildren(c) : obj.GetComponents(c));

                foreach (Component c in compsToDestroy.Where(c => !c.SafeIsUnityNull()))
                    GameObject.Destroy(c);
            }

            Color halfMain = new(color.r, color.g, color.b, 0.5f);

            // Get reference material
            Material refMat = reference ?? CardLoader.GetCardByName("BridgeRailing").holoPortraitPrefab.GetComponentInChildren<Renderer>().material;

            Renderer[] allRenderers = inChildren ? obj.GetComponentsInChildren<Renderer>() : obj.GetComponents<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = Shader.Find(shaderKey);
                    material.CopyPropertiesFromMaterial(refMat);
                    //material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.;
                    //material.EnableKeyword("_EMISSION");

                    // _METALLICGLOSSMAP
                    // _DETAIL_MULX2

                    if (material.HasProperty("_EmissionColor"))
                        material.SetColor("_EmissionColor", color * 0.5f);

                    if (material.HasProperty("_MainColor"))
                        material.SetColor("_MainColor", color);
                    if (material.HasProperty("_RimColor"))
                        material.SetColor("_RimColor", color);
                    if (material.HasProperty("_Color"))
                        material.SetColor("_Color", halfMain);

                    if (brightness.HasValue && material.HasProperty("_Brightness"))
                        material.SetFloat("_Brightness", brightness.Value);
                }
            }
        }
    }
}