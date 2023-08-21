using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Achievements;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using Pixelplacement;
using TMPro;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    [HarmonyPatch]
    internal static class Stickers
    {
        #region Sticker Textures

        internal static readonly Color TRANSPARENT_COLOR = new(0f, 0f, 0f, 0f);
        internal static readonly Texture2D CARBOARD_TEXTURE = TextureHelper.GetImageAsTexture("cardboard_texture.png", typeof(Stickers).Assembly);

        internal static readonly Shader STENCIL_SHADER = AssetBundleManager.Shaders.Find(sh => sh.name.Equals("P03/Projector/StickerStencilApply"));

        private static Texture2D _transparentTexture;
        internal static Texture2D TRANSPARENT_TEXTURE
        {
            get
            {
                if (_transparentTexture != null)
                    return _transparentTexture;

                _transparentTexture = new(2, 2, TextureFormat.RGBA32, false);
                _transparentTexture.SetPixels(new Color[] { TRANSPARENT_COLOR, TRANSPARENT_COLOR, TRANSPARENT_COLOR, TRANSPARENT_COLOR });
                _transparentTexture.Apply();

                return _transparentTexture;
            }
        }

        private static Texture2D _falloffTexture;
        internal static Texture2D FALLOFF_TEXTURE
        {
            get
            {
                if (_falloffTexture != null)
                    return _falloffTexture;

                _falloffTexture = new(6, 6, TextureFormat.RGBA32, false);
                for (int x = 0; x < _falloffTexture.width; x++)
                    for (int y = 0; y < _falloffTexture.height; y++)
                        _falloffTexture.SetPixel(x, y, x == 0 ? Color.black : TRANSPARENT_COLOR);

                _falloffTexture.Apply();

                return _falloffTexture;
            }
        }

        public enum StickerStyle
        {
            Standard = 0,
            Faded = 1,
            Shadow = 2
        }

        internal static Dictionary<string, Achievement> StickerRewards = new() {
            { "sticker_altcat", Achievement.KMOD_CHALLENGELEVEL1 },
            { "sticker_null", Achievement.KMOD_CHALLENGELEVEL2 },
            { "sticker_altcat_3", Achievement.KMOD_SPECIAL3 },
        };

        internal readonly static List<string> AllStickerKeys = new(StickerRewards.Keys);

        private static Texture2D GetStickerTexture(string keyName)
        {
            Texture2D tempTexture = TextureHelper.GetImageAsTexture($"{keyName}.png", typeof(Stickers).Assembly);
            Texture2D retval = new(tempTexture.width + 2, tempTexture.height + 2, TextureFormat.RGBA32, false);
            retval.name = tempTexture.name;
            retval.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < retval.width; x++)
            {
                retval.SetPixel(x, 0, TRANSPARENT_COLOR);
                retval.SetPixel(x, retval.height - 1, TRANSPARENT_COLOR);
            }
            for (int y = 0; y < retval.height; y++)
            {
                retval.SetPixel(0, y, TRANSPARENT_COLOR);
                retval.SetPixel(retval.width - 1, y, TRANSPARENT_COLOR);
            }
            retval.SetPixels(1, 1, tempTexture.width, tempTexture.height, tempTexture.GetPixels());
            retval.Apply();
            return retval;
        }

        private readonly static List<Texture2D> AllStickers = new(AllStickerKeys.Select(GetStickerTexture));

        private static Color Transparency(Color c)
        {
            return new(c.r, c.g, c.b, c.a * 0.3f);
        }

        private static Texture2D MakeFadedTexture(Texture2D texture)
        {
            Texture2D newTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false);
            newTexture.name = texture.name;
            for (int x = 0; x < texture.width; x++)
                for (int y = 0; y < texture.height; y++)
                    newTexture.SetPixel(x, y, Transparency(texture.GetPixel(x, y)));
            newTexture.Apply();
            return newTexture;
        }

        private readonly static List<Texture2D> AllFadedStickers = new(AllStickers.Select(MakeFadedTexture));

        private static Color Shadow(Color c)
        {
            Color refC = GameColors.instance.darkGold;
            return new(refC.r, refC.g, refC.b, c.a == 0 ? 0f : 1f);
        }

        private static Texture2D MakeShadowTexture(Texture2D texture)
        {
            Texture2D newTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false);
            newTexture.name = texture.name;
            for (int x = 0; x < texture.width; x++)
                for (int y = 0; y < texture.height; y++)
                    newTexture.SetPixel(x, y, Shadow(texture.GetPixel(x, y)));
            newTexture.Apply();
            return newTexture;
        }

        private readonly static List<Texture2D> AllShadowStickers = new(AllStickers.Select(MakeShadowTexture));

        private readonly static Dictionary<StickerStyle, List<Texture2D>> AllStickerTypes = new()
        {
            { StickerStyle.Standard, AllStickers },
            { StickerStyle.Faded, AllFadedStickers },
            { StickerStyle.Shadow, AllShadowStickers }
        };

        #endregion

        #region Saved Sticker Positions

        private static Dictionary<string, Vector3> ParseVectorMap(string parsed)
        {
            Dictionary<string, Vector3> retval = new();
            if (String.IsNullOrEmpty(parsed))
                return retval;

            foreach (var p in parsed.Split('|').Select(s => s.Split(',')))
                if (p.Length == 4)
                    retval.Add(p[0], new(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));

            return retval;
        }

        private static Dictionary<string, Dictionary<string, Vector3>> ParseVectorMapOfMaps(string parsed)
        {
            Dictionary<string, Dictionary<string, Vector3>> retval = new();
            if (String.IsNullOrEmpty(parsed))
                return retval;

            foreach (var p in parsed.Split('@').Select(s => s.Split('/')))
                retval.Add(p[0], ParseVectorMap(p[1]));

            return retval;
        }

        private static string FormatVectorMap(Dictionary<string, Vector3> value)
        {
            return String.Join("|", value.Select(kvp => $"{kvp.Key},{kvp.Value.x},{kvp.Value.y},{kvp.Value.z}"));
        }

        private static string FormatVectorMapOfMaps(Dictionary<string, Dictionary<string, Vector3>> value)
        {
            return String.Join("@", value.Select(kvp => $"{kvp.Key}/{FormatVectorMap(kvp.Value)}"));
        }

        private static string GetCardKey(CardInfo card)
        {
            if (card == null)
                return null;

            string retval = card.name;
            string cardKey = CustomCards.ConvertCardToCompleteCode(card);
            int duplicates = 0;
            foreach (var deckCard in Part3SaveData.Data.deck.Cards)
            {
                if (deckCard.name.Equals(retval))
                {
                    if (cardKey.Equals(CustomCards.ConvertCardToCompleteCode(deckCard)))
                        break;
                    else
                        duplicates += 1;
                }
            }
            return $"{retval}{duplicates}";
        }

        private static Dictionary<string, Dictionary<string, Vector3>> UpdateVectorHelper(this Dictionary<string, Dictionary<string, Vector3>> dictionary, CardInfo card, string stickerName, Vector3? vector)
        {
            if (card == null)
            {
                foreach (var key in dictionary.Keys)
                {
                    if (dictionary[key].ContainsKey(stickerName))
                        dictionary[key].Remove(stickerName);
                }
                return dictionary;
            }

            string cardKey = GetCardKey(card);

            if (!dictionary.ContainsKey(cardKey))
                dictionary[cardKey] = new();

            if (String.IsNullOrEmpty(stickerName))
            {
                dictionary[cardKey] = new();
                return dictionary;
            }

            if (vector.HasValue)
                dictionary[cardKey][stickerName] = vector.Value;
            else if (dictionary[cardKey].ContainsKey(stickerName))
                dictionary[cardKey].Remove(stickerName);

            return dictionary;
        }

        private static Dictionary<string, Dictionary<string, Vector3>> AppliedStickerPositions
        {
            get => ParseVectorMapOfMaps(ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "AppliedStickerPositions"));
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "AppliedStickerPositions", FormatVectorMapOfMaps(value));
        }

        internal static void UpdateStickerPosition(CardInfo card, string stickerName, Vector3? position)
        {
            AppliedStickerPositions = AppliedStickerPositions.UpdateVectorHelper(card, stickerName, position);
        }

        private static Dictionary<string, Dictionary<string, Vector3>> AppliedStickerRotations
        {
            get => ParseVectorMapOfMaps(ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "AppliedStickerRotations"));
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "AppliedStickerRotations", FormatVectorMapOfMaps(value));
        }

        internal static void UpdateStickerRotation(CardInfo card, string stickerName, Vector3? eulerAngles)
        {
            AppliedStickerRotations = AppliedStickerRotations.UpdateVectorHelper(card, stickerName, eulerAngles);
        }

        private static Dictionary<string, Dictionary<string, Vector3>> AppliedStickerScales
        {
            get => ParseVectorMapOfMaps(ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "AppliedStickerScales"));
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "AppliedStickerScales", FormatVectorMapOfMaps(value));
        }

        internal static void UpdateStickerScale(CardInfo card, string stickerName, Vector3? scale)
        {
            AppliedStickerScales = AppliedStickerScales.UpdateVectorHelper(card, stickerName, scale);
        }

        internal static bool IsStickerApplied(string stickerName)
        {
            var stickerDict = AppliedStickerPositions;
            foreach (var cardKey in stickerDict.Keys)
                if (stickerDict[cardKey].ContainsKey(stickerName))
                    return true;

            return false;
        }

        #endregion

        #region Game Objects

        internal static void PrepareStickerRenderer(Renderer textureRenderer, Texture texture)
        {
            textureRenderer.material.EnableKeyword("_EMISSION");
            textureRenderer.material.SetTexture("_MainTex", texture ?? TRANSPARENT_TEXTURE);
            textureRenderer.material.SetTexture("_DetailAlbedoMap", texture ?? TRANSPARENT_TEXTURE);
            textureRenderer.material.SetTexture("_EmissionMap", texture ?? TRANSPARENT_TEXTURE);
            textureRenderer.material.SetColor("_EmissionColor", Color.white);

            textureRenderer.material.SetFloat("_Mode", 2);
            textureRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            textureRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            textureRenderer.material.SetInt("_ZWrite", 0);
            textureRenderer.material.DisableKeyword("_ALPHATEST_ON");
            textureRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            textureRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            textureRenderer.material.renderQueue = 3000;
        }

        internal static GameObject GetSticker(string stickerName, bool interactable, bool project, StickerStyle style)
        {
            Texture2D texture = AllStickerTypes[style].FirstOrDefault(t => t.name.Equals(stickerName));
            if (texture == null)
            {
                P03Plugin.Log.LogWarning($"Tried to create a sticker that doesn't exist: {stickerName}");
                return null;
            }

            GameObject sticker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sticker.name = stickerName;

            Renderer textureRenderer = sticker.GetComponent<Renderer>();
            PrepareStickerRenderer(textureRenderer, texture);

            textureRenderer.sortingOrder = style == StickerStyle.Standard ? 500 : -500;

            float widthOverHeight = ((float)texture.width) / ((float)texture.height);
            float width = 0.5f;
            float height = width / widthOverHeight;
            sticker.transform.localScale = new(width, height, 1f);

            if (project)
            {
                textureRenderer.enabled = false;

                GameObject projectorObject = new("Projector");
                projectorObject.transform.SetParent(sticker.transform);
                projectorObject.transform.localPosition = new(0f, 0f, -1.25f);

                var projector = projectorObject.AddComponent<Projector>();
                string shaderName = interactable ? "P03/Projector/UnStenciledSticker" : "P03/Projector/StenciledSticker";
                Shader lightShader = AssetBundleManager.Shaders.Find(sh => sh.name.Equals(shaderName));
                projector.material = new(lightShader);
                projector.material.SetColor("_Color", Color.white);
                projector.material.SetTexture("_ShadowTex", texture);
                projector.farClipPlane = interactable ? 1.3121f : 1.26f;
                projector.nearClipPlane = interactable ? 0f : 1.23f;
                projector.fieldOfView = 25;
                projector.ignoreLayers = 1 << 2;
                projector.orthographic = true;
                projector.orthographicSize = 0.25f;

                if (!interactable)
                {
                    projectorObject.AddComponent<Camera>().depth = -5;
                    projectorObject.AddComponent<StickerProjector>();
                }
            }

            if (interactable)
            {
                StickerDrag dragger = sticker.AddComponent<StickerDrag>();
                dragger.StickerName = stickerName;
                sticker.AddComponent<StickerRotate>();
            }
            else
            {
                GameObject.Destroy(sticker.GetComponent<MeshCollider>());
            }

            return sticker;
        }

        internal static readonly Part3DeckReviewSequencer.State StickerState = GuidManager.GetEnumValue<Part3DeckReviewSequencer.State>(P03Plugin.PluginGuid, "StickerView");

        #endregion

        internal static void OnStickerBookClicked(Part3DeckReviewSequencer sequencer)
        {
            // This just wipes out the stuff on the on the table and then kicks off the sticker management sequence
            sequencer.state = StickerState;
            sequencer.SetDeckPilesEnabled(false);
            sequencer.StartCoroutine(sequencer.CleanUpDeckPiles(true, false));
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            CustomCoroutine.WaitThenExecute(0.2f, delegate
            {
                ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
                sequencer.StartCoroutine(StickerInterfaceManager.Instance.ShowStickerInterfaceUntilCancelled(sequencer));
            }, false);
        }


        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.SpawnDeckPiles))]
        [HarmonyPostfix]
        private static IEnumerator SpawnStickerTablet(IEnumerator sequence, Part3DeckReviewSequencer __instance)
        {
            yield return sequence;

            if (!P03AscensionSaveData.IsP03Run)
                yield break;

            if (StickerRewards.Where(kvp => ModdedAchievementManager.AchievementById(kvp.Value).IsUnlocked).Count() == 0)
                yield break;

            if (__instance.gameObject.GetComponent<StickerInterfaceManager>() == null)
                __instance.gameObject.AddComponent<StickerInterfaceManager>();

            GameObject stickerButton = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/rulebook/TableTablet"), __instance.transform);
            GameObject.Destroy(stickerButton.GetComponentInChildren<TableRuleBook>());

            stickerButton.transform.localScale = new(0.5f, 0.5f, 0.5f);
            stickerButton.transform.localEulerAngles = new(0f, 90f, 0f);

            var previousInteractable = stickerButton.GetComponentInChildren<OpenRulebookInteractable>();
            var osi = previousInteractable.gameObject.AddComponent<OpenStickerInteractable>();
            GameObject.Destroy(previousInteractable);
            osi.SetEnabled(true);

            stickerButton.name = "StickerBook";

            Vector3 targetPosition = new(0f, 0.1f, -1.75f);
            stickerButton.transform.localPosition = targetPosition + new Vector3(0f, 0f, -2f);

            stickerButton.SetActive(true);

            Tween.LocalPosition(stickerButton.transform, targetPosition, 0.2f, 0f);

            yield break;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.CleanUpDeckPiles))]
        [HarmonyPostfix]
        private static IEnumerator CleanUpStickerBook(IEnumerator sequence, SelectableCardArray __instance)
        {
            Transform tablet = __instance.transform.Find("StickerBook");
            if (tablet != null)
            {
                Tween.Position(tablet, tablet.transform.position + new Vector3(0f, 0f, -2f), 0.2f, 0f, completeCallback: () => GameObject.Destroy(tablet.gameObject));
            }
            yield return sequence;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPostfix]
        private static void ApplyStickersToCard(ref Card __instance)
        {
            if (__instance.StatsLayer is not DiskRenderStatsLayer)
                return;

            for (int i = 1; i <= 5; i++)
                __instance.StatsLayer.transform.Find($"Top/Stickers/Sticker_{i}").gameObject.layer = 2;

            foreach (var proj in __instance.GetComponentsInChildren<Projector>())
                GameObject.Destroy(proj.transform.parent.gameObject);

            string cardKey = GetCardKey(__instance.Info);
            var positions = AppliedStickerPositions;
            var scales = AppliedStickerScales;
            var rotations = AppliedStickerRotations;
            var activeInterface = StickerInterfaceManager.Instance != null && StickerInterfaceManager.Instance.StickerInterfaceActive;

            if (positions.ContainsKey(cardKey))
            {
                if (!activeInterface)
                {
                    GameObject stencil = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stencil.name = "Stencil";
                    stencil.transform.SetParent(__instance.StatsLayer.transform);
                    stencil.transform.localScale = new(1.945f, 1.385f, 0.055f);
                    stencil.transform.localPosition = new(0f, -0.05f, 0f);
                    stencil.transform.localEulerAngles = new(0f, 0f, 0f);
                    stencil.GetComponent<Renderer>().material = new Material(STENCIL_SHADER);
                    GameObject.Destroy(stencil.GetComponent<Collider>());
                }

                foreach (var stickerKey in positions[cardKey].Keys)
                {
                    var sticker = GetSticker(stickerKey, activeInterface, true, StickerStyle.Standard);
                    sticker.transform.SetParent(__instance.StatsLayer.transform);
                    sticker.transform.localPosition = positions[cardKey][stickerKey];
                    sticker.transform.localEulerAngles = new(0f, 180f, 90f);

                    if (scales.ContainsKey(cardKey) && scales[cardKey].ContainsKey(stickerKey))
                        sticker.transform.localScale = scales[cardKey][stickerKey];

                    if (rotations.ContainsKey(cardKey) && rotations[cardKey].ContainsKey(stickerKey))
                        sticker.transform.localEulerAngles = rotations[cardKey][stickerKey];

                    // Reparent
                    sticker.transform.SetParent(__instance.StatsLayer.transform.Find(sticker.transform.localPosition.x > 0 ? "Top" : "Bottom"), true);
                }
            }
        }
    }
}