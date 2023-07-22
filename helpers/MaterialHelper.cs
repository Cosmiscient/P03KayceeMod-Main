using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class MaterialHelper
    {
        public static void RecolorAllMaterials(GameObject obj, Color color, string shaderKey = null, bool emissive = false, string[] shaderKeywords = null, bool forceEnable = false, bool makeTransparent = false)
        {
            Color halfMain = new Color(color.r, color.g, color.b);
            halfMain.a = 0.5f;

            if (makeTransparent)
                shaderKey = "Standard";

            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (forceEnable)
                    renderer.enabled = true;

                foreach (Material material in renderer.materials)
                {
                    if (!string.IsNullOrEmpty(shaderKey))
                        material.shader = Shader.Find(shaderKey);

                    if (emissive)
                    {
                        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                        material.EnableKeyword("_EMISSION");
                    }

                    if (shaderKeywords != null)
                        material.SetShaderKeywords(shaderKeywords);         

                    if (material.HasProperty("_EmissionColor"))
                        material.SetColor("_EmissionColor", color * 0.5f);

                    if (material.HasProperty("_MainColor"))
                        material.SetColor("_MainColor", color);
                    if (material.HasProperty("_RimColor"))
                        material.SetColor("_RimColor", color);

                    if (!makeTransparent)
                    {
                        if (material.HasProperty("_Color"))
                            material.SetColor("_Color", halfMain); 
                    }
                    else
                    {
                        material.SetColor("_Color", color); 
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetFloat("_ZWrite", 0.0f);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                }
            }
        }

        internal static readonly string[] TextureNames = new string[]
        {
            "_MainTex",
            "_DetailAlbedoMap",
        };

        private static Dictionary<string, Dictionary<string, bool>> _rendererCache = new ();

        private static bool CardComponentHasTargetTexture(Renderer renderer, string textureName, string matchKey)
        {
            if (!_rendererCache.ContainsKey(renderer.gameObject.name))
                _rendererCache[renderer.gameObject.name] = new ();

            if (_rendererCache[renderer.gameObject.name].ContainsKey(textureName))
                return _rendererCache[renderer.gameObject.name][textureName];

            Texture tex = renderer.material.GetTexture(textureName);
            if (tex != null && tex.name.ToLowerInvariant().Contains(matchKey))
                _rendererCache[renderer.gameObject.name][textureName] = true;
            else
                _rendererCache[renderer.gameObject.name][textureName] = false;

            return _rendererCache[renderer.gameObject.name][textureName];
        }

        public static void RetextureAllRenderers(GameObject gameObject, Texture texture, string originalTextureKey = null)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                try
                {
                    foreach (string textureName in TextureNames)
                        if (String.IsNullOrEmpty(originalTextureKey) || CardComponentHasTargetTexture(renderer, textureName, originalTextureKey.ToLowerInvariant()))
                            foreach (var material in renderer.materials)
                                material.SetTexture(textureName, texture);
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        public static Material GetBakedEmissiveMaterial(Texture texture, Texture emissionTexture = null)
        {
            GameObject screenFront = CardSpawner.Instance.playableCardPrefab.transform.Find("Anim/CardBase/Top/EnergyCostLights").gameObject;
            Renderer screenRenderer = screenFront.GetComponent<Renderer>();
            Material mat = new(screenRenderer.material);
            mat.SetTexture("_MainTex", texture ?? emissionTexture);

            if (emissionTexture != null)
                mat.SetTexture("_EmissionMap", emissionTexture);
            return mat;
        }

        public static Texture2D DuplicateTexture(Texture2D texture)
        {
            // https://support.unity.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
            // Create a temporary RenderTexture of the same size as the texture

            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it

            Texture2D myTexture2D = new(texture.width, texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            return myTexture2D;
        }
    }
}