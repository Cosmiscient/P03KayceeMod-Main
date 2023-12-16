using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using Sirenix.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class OnboardDynamicHoloPortrait : CardAppearanceBehaviour
    {
        public const string PORTRAIT_KEY = "HoloPortrait.Key";
        public const string PREFAB_KEY = "HoloPortrait.PrefabKey";
        public const string OFFSET_KEY = "HoloPortrait.Transform.LocalPosition";
        public const string ROTATION_KEY = "HoloPortrait.Transform.LocalEulerAngles";
        public const string SCALE_KEY = "HoloPortrait.Transform.LocalScale";
        public const string COLOR = "HoloPortrait.Color";
        public const string SHADER_KEY = "HoloPortrait.ShaderKey";
        public const string HIDE_CHILDREN = "HoloPortrait.HideChildren";
        public const string IN_HAND = "HoloPortrait.InHand";

        private bool portraitSpawned = false;

        public static Appearance ID { get; private set; }

        private GameObject GetPrefab(string portraitKey, string prefabKey)
        {
            if (string.IsNullOrEmpty(portraitKey))
            {
                if (string.IsNullOrEmpty(prefabKey))
                    return null;

                P03Plugin.Log.LogDebug($"Getting prefab for card portrait: {prefabKey}");
                return ResourceBank.Get<GameObject>(prefabKey);
            }

            P03Plugin.Log.LogDebug($"Getting game object from holomap for card portrait: {portraitKey}");
            return RunBasedHoloMap.GetGameObject(portraitKey);
        }

        private List<GameObject> GetPrefab()
        {
            string allPortraitKey = Card.Info.GetExtendedProperty(PORTRAIT_KEY);
            string allPrefabKey = Card.Info.GetExtendedProperty(PREFAB_KEY);

            List<string> portraitKeys = String.IsNullOrEmpty(allPortraitKey) ? new() : allPortraitKey.Split('|').ToList();
            List<string> prefabKeys = String.IsNullOrEmpty(allPrefabKey) ? new() : allPrefabKey.Split('|').ToList();

            int numberOfObjects = Mathf.Max(portraitKeys.Count, prefabKeys.Count);

            List<GameObject> retval = new();

            for (int i = 0; i < numberOfObjects; i++)
            {
                string portraitKey = i < portraitKeys.Count ? portraitKeys[i] : string.Empty;
                string prefabKey = i < prefabKeys.Count ? prefabKeys[i] : string.Empty;

                GameObject child = GetPrefab(portraitKey, prefabKey);
                retval.Add(child);
            }

            return retval;

        }

        private Vector3 GetVector3(string key, int index, bool zeroDefault = true)
        {
            string offset = Card.Info.GetExtendedProperty(key);
            if (string.IsNullOrEmpty(offset))
                return zeroDefault ? Vector3.zero : Vector3.one;

            string[] allOffsets = offset.Split('|');
            if (allOffsets.Length <= index)
                return zeroDefault ? Vector3.zero : Vector3.one;

            string[] offsetSplit = allOffsets[index].Split(',');
            return offsetSplit.Length != 3
                ? zeroDefault ? Vector3.zero : Vector3.one
                : new Vector3(float.Parse(offsetSplit[0], CultureInfo.InvariantCulture),
                               float.Parse(offsetSplit[1], CultureInfo.InvariantCulture),
                               float.Parse(offsetSplit[2], CultureInfo.InvariantCulture));
        }

        public static void HolofyGameObject(GameObject obj, Color color, string shaderKey = "SFHologram/HologramShader", bool inChildren = true, Material reference = null, bool destroyComponents = false)
        {
            if (destroyComponents)
            {
                List<Component> compsToDestroy = new();
                foreach (Type c in new List<Type>() { typeof(Rigidbody), typeof(AutoRotate), typeof(Animator), typeof(InteractableBase), typeof(Item) })
                    compsToDestroy.AddRange(inChildren ? obj.GetComponentsInChildren(c) : obj.GetComponents(c));

                foreach (Component c in compsToDestroy.Where(c => !c.SafeIsUnityNull()))
                    Destroy(c);
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
                }
            }
        }

        private void CleanGameObject(GameObject obj, int index)
        {
            string colorKeyAll = Card.Info.GetExtendedProperty(COLOR);
            string[] colorKeyAllSplit = string.IsNullOrEmpty(colorKeyAll) ? new string[0] : colorKeyAll.Split('|');
            string colorKey = index < colorKeyAllSplit.Length ? colorKeyAllSplit[index] : null;
            Color color = GameColors.Instance.brightBlue;
            if (!string.IsNullOrEmpty(colorKey))
            {
                string[] colorSplit = colorKey.Split(',');
                if (colorSplit.Length == 3)
                    color = new Color(float.Parse(colorSplit[0]), float.Parse(colorSplit[1]), float.Parse(colorSplit[2]));
            }

            string shaderKeyAll = Card.Info.GetExtendedProperty(SHADER_KEY);
            string[] shaderKeyAllSplit = string.IsNullOrEmpty(shaderKeyAll) ? new string[0] : shaderKeyAll.Split('|');
            string shaderKey = index < shaderKeyAllSplit.Length ? shaderKeyAllSplit[index] : null;
            if (string.IsNullOrEmpty(shaderKey))
                shaderKey = "SFHologram/HologramShader";

            if (!shaderKey.Equals("default", StringComparison.InvariantCultureIgnoreCase))
            {
                HolofyGameObject(obj, color, shaderKey);
            }
        }

        private void SpawnHoloPortrait(DiskCardAnimationController dcac)
        {
            List<GameObject> prefab = GetPrefab();

            if (prefab == null || prefab.Count == 0)
                return;

            GameObject gameObject = new("DynamicHoloPortraitParent");
            gameObject.transform.SetParent(dcac.holoPortraitParent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;

            string hideChildrenAll = Card.Info.GetExtendedProperty(HIDE_CHILDREN);
            List<string> hideChildren = string.IsNullOrEmpty(hideChildrenAll) ? new() : hideChildrenAll.Split('|').ToList();
            List<List<string>> childrenToHide = hideChildren.Select(s => string.IsNullOrEmpty(s) ? new() : s.Split(',').ToList()).ToList();

            for (int i = 0; i < prefab.Count; i++)
            {
                GameObject child = Instantiate(prefab[i], gameObject.transform);
                CleanGameObject(child, i);
                child.transform.localPosition = GetVector3(OFFSET_KEY, i);
                child.transform.localEulerAngles = GetVector3(ROTATION_KEY, i);
                child.transform.localScale = GetVector3(SCALE_KEY, i, false);
                child.SetActive(true);

                if (i < childrenToHide.Count)
                {
                    List<string> children = childrenToHide[i];
                    if (children != null)
                    {
                        foreach (string childKey in children)
                        {
                            if (!string.IsNullOrEmpty(childKey))
                            {
                                Transform t = child.transform.Find(childKey);
                                t?.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            CustomCoroutine.FlickerSequence(delegate
            {
                dcac.holoPortraitParent.gameObject.SetActive(true);
            }, delegate
            {
                dcac.holoPortraitParent.gameObject.SetActive(false);
            }, false, true, 0.1f, 3, null);

            portraitSpawned = true;
        }

        public override void ApplyAppearance()
        {
            bool showInHand = Card.Info.GetExtendedPropertyAsBool(IN_HAND).GetValueOrDefault(false);
            if (Card.Anim is DiskCardAnimationController dcac && Card is PlayableCard pCard && (pCard.OnBoard || showInHand) && !portraitSpawned)
            {
                SpawnHoloPortrait(dcac);
                Card.renderInfo.hidePortrait = portraitSpawned;
            }
        }

        public override void OnPreRenderCard() => ApplyAppearance();

        static OnboardDynamicHoloPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "OnboardDynamicHoloPortrait", typeof(OnboardDynamicHoloPortrait)).Id;
        }
    }
}