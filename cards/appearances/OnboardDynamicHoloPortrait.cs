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
        public const string IN_HAND = "HoloPortrait.InHand";

        private bool portraitSpawned = false;

        public static Appearance ID { get; private set; }

        private GameObject GetPrefab()
        {
            string key = Card.Info.GetExtendedProperty(PORTRAIT_KEY);
            if (string.IsNullOrEmpty(key))
            {
                key = Card.Info.GetExtendedProperty(PREFAB_KEY);

                if (string.IsNullOrEmpty(key))
                    return null;

                P03Plugin.Log.LogDebug($"Getting prefab for card portrait: {key}");
                return Resources.Load<GameObject>(key);
            }

            P03Plugin.Log.LogDebug($"Getting game object from holomap for card portrait: {key}");
            return RunBasedHoloMap.GetGameObject(key);
        }

        private Vector3 GetVector3(string key, bool zeroDefault = true)
        {
            string offset = Card.Info.GetExtendedProperty(key);
            if (string.IsNullOrEmpty(offset))
                return zeroDefault ? Vector3.zero : Vector3.one;

            string[] offsetSplit = offset.Split(',');
            return offsetSplit.Length != 3
                ? zeroDefault ? Vector3.zero : Vector3.one
                : new Vector3(float.Parse(offsetSplit[0], CultureInfo.InvariantCulture),
                               float.Parse(offsetSplit[1], CultureInfo.InvariantCulture),
                               float.Parse(offsetSplit[2], CultureInfo.InvariantCulture));
        }

        public static void HolofyGameObject(GameObject obj, Color color, string shaderKey = "SFHologram/HologramShader", bool inChildren = true)
        {
            List<Component> compsToDestroy = new();
            compsToDestroy.AddRange(obj.GetComponentsInChildren<Rigidbody>());
            compsToDestroy.AddRange(obj.GetComponentsInChildren<AutoRotate>());
            compsToDestroy.AddRange(obj.GetComponentsInChildren<Animator>());
            compsToDestroy.AddRange(obj.GetComponentsInChildren<InteractableBase>());
            compsToDestroy.AddRange(obj.GetComponentsInChildren<Item>());

            foreach (Component c in compsToDestroy.Where(c => !c.SafeIsUnityNull()))
                Destroy(c);

            Color halfMain = new(color.r, color.g, color.b, 0.5f);

            // Get reference material
            Material refMat = CardLoader.GetCardByName("BridgeRailing").holoPortraitPrefab.GetComponentInChildren<Renderer>().material;

            var allRenderers = inChildren ? obj.GetComponentsInChildren<Renderer>() : obj.GetComponents<Renderer>();
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

        private void CleanGameObject(GameObject obj)
        {
            string colorKey = Card.Info.GetExtendedProperty(COLOR);
            Color color = GameColors.Instance.brightBlue;
            if (!string.IsNullOrEmpty(colorKey))
            {
                string[] colorSplit = colorKey.Split(',');
                if (colorSplit.Length == 3)
                    color = new Color(float.Parse(colorSplit[0]), float.Parse(colorSplit[1]), float.Parse(colorSplit[2]));
            }

            string shaderKey = Card.Info.GetExtendedProperty(SHADER_KEY);
            if (string.IsNullOrEmpty(shaderKey))
                shaderKey = "SFHologram/HologramShader";

            HolofyGameObject(obj, color, shaderKey);
        }

        private void SpawnHoloPortrait(DiskCardAnimationController dcac)
        {
            GameObject prefab = GetPrefab();

            if (prefab == null)
                return;

            GameObject gameObject = Instantiate(prefab, dcac.holoPortraitParent);
            CleanGameObject(gameObject);
            gameObject.transform.localPosition = GetVector3(OFFSET_KEY);
            gameObject.transform.localEulerAngles = GetVector3(ROTATION_KEY);
            gameObject.transform.localScale = GetVector3(SCALE_KEY, false);
            gameObject.SetActive(true);
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