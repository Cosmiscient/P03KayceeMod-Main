using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class MaterialHelper
    {
        public static void RecolorAllMaterials(GameObject obj, Color color, string shaderKey = null, bool emissive = false, string[] shaderKeywords = null, bool forceEnable = false, bool makeTransparent = false)
        {
            Color halfMain = new(color.r, color.g, color.b)
            {
                a = 0.5f
            };

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

        private static readonly Dictionary<string, Dictionary<string, bool>> _rendererCache = new();

        private static bool CardComponentHasTargetTexture(Renderer renderer, string textureName, string matchKey)
        {
            if (!_rendererCache.ContainsKey(renderer.gameObject.name))
                _rendererCache[renderer.gameObject.name] = new();

            if (_rendererCache[renderer.gameObject.name].ContainsKey(textureName))
                return _rendererCache[renderer.gameObject.name][textureName];

            Texture tex = renderer.material.GetTexture(textureName);
            _rendererCache[renderer.gameObject.name][textureName] = tex != null && tex.name.ToLowerInvariant().Contains(matchKey);

            return _rendererCache[renderer.gameObject.name][textureName];
        }

        public static void RetextureAllRenderers(GameObject gameObject, Texture texture, string originalTextureKey = null, string textureName = null)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                try
                {
                    if (String.IsNullOrEmpty(textureName))
                    {
                        foreach (string texName in TextureNames)
                        {
                            if (String.IsNullOrEmpty(originalTextureKey) || CardComponentHasTargetTexture(renderer, textureName, originalTextureKey.ToLowerInvariant()))
                            {
                                foreach (Material material in renderer.materials)
                                    material.SetTexture(texName, texture);
                            }
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(originalTextureKey) || CardComponentHasTargetTexture(renderer, textureName, originalTextureKey.ToLowerInvariant()))
                        {
                            foreach (Material material in renderer.materials)
                                material.SetTexture(textureName, texture);
                        }
                    }
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        public static void HolofyAllRenderers(GameObject gameObject, Color color)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                OnboardDynamicHoloPortrait.HolofyGameObject(renderer.gameObject, color);
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

        private static void AlignReferenceObjects(Transform newObject, Vector3 newParentScale, Transform referenceObject, Vector3 refParentScale)
        {
            newObject.eulerAngles = referenceObject.eulerAngles;
            newObject.position = referenceObject.position;

            Vector3 refActualScale = Vector3.Scale(referenceObject.localScale, refParentScale);
            newObject.localScale = new Vector3(
                refActualScale.x / newParentScale.x,
                refActualScale.y / newParentScale.y,
                refActualScale.z / newParentScale.z
            );

            newObject.gameObject.SetActive(referenceObject.gameObject.activeSelf);

            var renderer = newObject.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                P03Plugin.Log.LogDebug($"Setting renderer enabled for {newObject.gameObject.name} to {referenceObject.gameObject.activeSelf}");
                renderer.enabled = referenceObject.gameObject.activeSelf;
            }

            for (int i = 0; i < newObject.childCount; i++)
                AlignReferenceObjects(newObject.GetChild(i), refActualScale, referenceObject.GetChild(i), refActualScale);
        }

        internal static void AlignReferenceObjects(Transform newObject, Transform referenceObject)
        {
            AlignReferenceObjects(newObject, AggregateScale(newObject.parent), referenceObject, AggregateScale(referenceObject.parent));
        }

        private static Vector3 AggregateScale(Transform t)
        {
            Vector3 retval = t.localScale;
            if (t.parent != null)
                retval = Vector3.Scale(retval, AggregateScale(t.parent));
            return retval;
        }

        public static GameObject CreateMatchingAnimatedObject(GameObject refObj, Transform newParent)
        {
            GameObject newObj = GameObject.Instantiate(refObj, refObj.transform.parent);

            foreach (var t in new List<Type>() { typeof(Animator), typeof(TableAnimationKeyframeEvents), typeof(OpponentArmController) })
                foreach (var anim in newObj.GetComponentsInChildren(t).ToList())
                    GameObject.Destroy(anim);

            newObj.transform.SetParent(newParent, true);
            AlignReferenceObjects(newObj.transform, refObj.transform);

            return newObj;
        }
    }
}