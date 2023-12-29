using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Achievements;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    [HarmonyPatch]
    public static class Stickers
    {
        #region Sticker Textures

        internal static bool DebugStickers => P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("sticker");

        internal static readonly Color TRANSPARENT_COLOR = new(0f, 0f, 0f, 0f);
        internal static readonly Texture2D CARBOARD_TEXTURE = TextureHelper.GetImageAsTexture("cardboard_texture.png", typeof(Stickers).Assembly);

        internal static readonly Shader STENCIL_SHADER = AssetBundleManager.Shaders.Find(sh => sh.name.Equals("P03/Projector/StickerStencilApply"));
        internal static readonly Shader STANDARD_STENCIL_SHADER = AssetBundleManager.Shaders.Find(sh => sh.name.Equals("P03/Projector/StandardWithShader"));

        public const string STICKER_PROPERTY_KEY = "AbilityManager.Sticker";

        private static Texture2D _transparentTexture;
        internal static Texture2D TRANSPARENT_TEXTURE
        {
            get
            {
                if (_transparentTexture != null)
                {
                    return _transparentTexture;
                }

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
                {
                    return _falloffTexture;
                }

                _falloffTexture = new(6, 6, TextureFormat.RGBA32, false);
                for (int x = 0; x < _falloffTexture.width; x++)
                {
                    for (int y = 0; y < _falloffTexture.height; y++)
                    {
                        _falloffTexture.SetPixel(x, y, x == 0 ? Color.black : TRANSPARENT_COLOR);
                    }
                }

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
            { "sticker_null", P03AchievementManagement.FIRST_WIN },
            { "sticker_skull", P03AchievementManagement.SKULLSTORM },
            { "sticker_ceiling_cat_border", P03AchievementManagement.CONTROL_NFT },
            { "sticker_binary_ribbon", P03AchievementManagement.SURVIVE_SIX_ARCHIVIST },
            { "sticker_camera_photog", P03AchievementManagement.DONT_USE_CAMERA },
            { "sticker_annoy_face", P03AchievementManagement.CANVAS_ENOUGH },
            { "sticker_cowboy_hat", P03AchievementManagement.KILL_30_BOUNTY_HUNTERS },
            { "sticker_pokerchips", P03AchievementManagement.ALL_QUESTS_COMPLETED },
            { "sticker_companion_cube", P03AchievementManagement.KILL_QUEST_CARD },
            { "sticker_revolver", P03AchievementManagement.SIX_SHOOTER },
            { "sticker_altcat", P03AchievementManagement.SCALES_TILTED_3X },
            { "sticker_muscles", P03AchievementManagement.FULLY_UPGRADED },
            { "sticker_dr_fire_esq_2", P03AchievementManagement.MAX_SP_CARD },
            { "sticker_battery", P03AchievementManagement.TURBO_RAMP },
            { "sticker_tophat", P03AchievementManagement.MASSIVE_OVERKILL },
            { "sticker_rainbow_peace", P03AchievementManagement.AVOID_BOUNTY_HUNTERS },
            { "sticker_wizardhat", P03AchievementManagement.PLASMA_JIMMY_CRAZY },
            { "sticker_guillotine", P03AchievementManagement.FULLY_OVERCLOCKED },
            { "sticker_mushroom", P03AchievementManagement.MYCOLOGISTS_COMPLETED },
            { "sticker_winged_shoes", P03AchievementManagement.FAST_GENERATOR }
        };

        internal static string AddAbilitySticker(AbilityManager.FullAbility ability)
        {
            string newName = "abilitysticker_" + ability.Info.rulebookName.Replace(" ", "_");
            if (AllStickers.Any(t => t.name == newName))
                return newName;

            Texture2D stickerTexture = GetStickerTexture(ability.Id);
            stickerTexture.name = newName;
            ability.Info.SetExtendedProperty(STICKER_PROPERTY_KEY, newName);

            AllStickers.Add(stickerTexture);
            AllFadedStickers.Add(MakeFadedTexture(stickerTexture));
            AllShadowStickers.Add(MakeShadowTexture(stickerTexture));
            return newName;
        }

        /// <summary>
        /// Adds a new sticker to the sticker book
        /// </summary>
        /// <param name="pluginGuid">Plugin/mod guid</param>
        /// <param name="stickerName">Name of the sticker</param>
        /// <param name="stickerTexture">Sticker texture (500x500 pixels please!)</param>
        /// <param name="achievement">The associated achievement that unlocks the sticker</param>
        public static void Add(string pluginGuid, string stickerName, Texture2D stickerTexture, Achievement achievement)
        {
            string newName = $"{pluginGuid}_{stickerName}";
            if (StickerRewards.ContainsKey(newName))
                return;
            stickerTexture.name = newName;
            StickerRewards[newName] = achievement;
            AllStickerKeys.Add(newName);
            AllStickers.Add(stickerTexture);
            AllFadedStickers.Add(MakeFadedTexture(stickerTexture));
            AllShadowStickers.Add(MakeShadowTexture(stickerTexture));
        }

        internal static readonly List<string> AllStickerKeys = new(StickerRewards.Keys);

        private static bool HasWithin(Texture2D compTexture, int x, int y, int d)
        {
            for (int i = x - d; i <= x + d; i++)
            {
                for (int j = y - d; j <= y + d; j++)
                {
                    if (i < 0)
                        continue;
                    if (i >= compTexture.width)
                        continue;
                    if (j < 0)
                        continue;
                    if (j >= compTexture.height)
                        continue;
                    if (compTexture.GetPixel(i, j).a > 0)
                        return true;
                }
            }
            return false;
        }

        internal static Texture2D GetStickerTexture(Ability ability)
        {
            int border = 5;
            Texture2D abilityTexture = TextureHelper.DuplicateTexture(AbilitiesUtil.LoadAbilityIcon(ability.ToString(), false, false) as Texture2D);
            Texture2D retval = new(abilityTexture.width + (border * 2), abilityTexture.height + (border * 2), TextureFormat.RGBA32, false)
            {
                name = abilityTexture.name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear
            };

            // Ensure it all starts transparent
            for (int x = 0; x < retval.width; x++)
            {
                for (int y = 0; y < retval.height; y++)
                {
                    retval.SetPixel(x, y, TRANSPARENT_COLOR);
                }
            }

            // If there is a pixel within 2, set a black pixel
            for (int x = 0; x < retval.width; x++)
            {
                for (int y = 0; y < retval.height; y++)
                {
                    if (HasWithin(abilityTexture, x - border, y - border, 2))
                        retval.SetPixel(x, y, Color.black);
                }
            }

            // If there is a pixel within 1, set a beige pixel
            for (int x = 0; x < retval.width; x++)
            {
                for (int y = 0; y < retval.height; y++)
                {
                    if (HasWithin(abilityTexture, x - border, y - border, 1))
                        retval.SetPixel(x, y, new(0.921875f, 0.81640625f, 0.76171875f, 1f));
                }
            }

            // Copy the original
            for (int x = 0; x < abilityTexture.width; x++)
            {
                for (int y = 0; y < abilityTexture.height; y++)
                {
                    if (abilityTexture.GetPixel(x, y).a > 0)
                        retval.SetPixel(x + border, y + border, new(0.49609375f, 0f, 0f, 1f));
                }
            }

            retval.Apply();
            GameObject.Destroy(abilityTexture);
            return retval;
        }

        internal static Texture2D GetStickerTexture(string keyName)
        {
            Texture2D tempTexture = TextureHelper.GetImageAsTexture($"{keyName}.png", typeof(Stickers).Assembly);
            Texture2D retval = new(tempTexture.width + 2, tempTexture.height + 2, TextureFormat.RGBA32, false)
            {
                name = tempTexture.name,
                wrapMode = TextureWrapMode.Clamp
            };
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

        private static readonly List<Texture2D> AllStickers = new(AllStickerKeys.Select(GetStickerTexture));

        private static Color Transparency(Color c) => new(c.r, c.g, c.b, c.a * 0.3f);

        private static Texture2D MakeFadedTexture(Texture2D texture)
        {
            Texture2D newTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false)
            {
                name = texture.name
            };
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    newTexture.SetPixel(x, y, Transparency(texture.GetPixel(x, y)));
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        private static readonly List<Texture2D> AllFadedStickers = new(AllStickers.Select(MakeFadedTexture));

        private static Color Shadow(Color c)
        {
            Color refC = GameColors.instance.darkGold;
            return new(refC.r, refC.g, refC.b, c.a == 0 ? 0f : 1f);
        }

        private static Texture2D MakeShadowTexture(Texture2D texture)
        {
            Texture2D newTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false)
            {
                name = texture.name
            };
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    newTexture.SetPixel(x, y, Shadow(texture.GetPixel(x, y)));
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        private static readonly List<Texture2D> AllShadowStickers = new(AllStickers.Select(MakeShadowTexture));

        private static readonly Dictionary<StickerStyle, List<Texture2D>> AllStickerTypes = new()
        {
            { StickerStyle.Standard, AllStickers },
            { StickerStyle.Faded, AllFadedStickers },
            { StickerStyle.Shadow, AllShadowStickers }
        };

        #endregion

        #region Saved Sticker Positions

        public class CardStickerData
        {
            public Dictionary<string, Vector3> Positions { get; set; } = new();
            public Dictionary<string, Vector3> Rotations { get; set; } = new();
            public Dictionary<string, Vector3> Scales { get; set; } = new();
            public Dictionary<string, Ability> Ability { get; set; } = new();

            public override string ToString()
            {
                string retval = "Stickers";
                retval += $"[StickerPositions:{FormatVectorMap(Positions)}]";
                retval += $"[StickerRotations:{FormatVectorMap(Rotations)}]";
                retval += $"[StickerScales:{FormatVectorMap(Scales)}]";

                string stickerAbilities = String.Join(",", Ability.Select(kvp => $"{kvp.Key}:{(int)kvp.Value}"));
                retval += $"[StickerAbility:{stickerAbilities}]";
                return retval;
            }
        }

        private static Dictionary<string, Vector3> ParseVectorMap(string parsed)
        {
            Dictionary<string, Vector3> retval = new();
            if (String.IsNullOrEmpty(parsed))
            {
                return retval;
            }

            foreach (string[] p in parsed.Split('|').Select(s => s.Split(',')))
            {
                if (p.Length == 4)
                {
                    retval.Add(p[0], new(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));
                }
            }

            return retval;
        }

        private static Dictionary<string, Dictionary<string, Vector3>> ParseVectorMapOfMaps(string parsed)
        {
            Dictionary<string, Dictionary<string, Vector3>> retval = new();
            if (String.IsNullOrEmpty(parsed))
            {
                return retval;
            }

            foreach (string[] p in parsed.Split('@').Select(s => s.Split('/')))
            {
                retval.Add(p[0], ParseVectorMap(p[1]));
            }

            return retval;
        }

        private static string FormatVectorMap(Dictionary<string, Vector3> value) => String.Join("|", value.Select(kvp => $"{kvp.Key},{kvp.Value.x},{kvp.Value.y},{kvp.Value.z}"));

        internal static bool IsStickerApplied(string stickerName) => Part3SaveData.Data.deck.Cards.Any(ci => ci.GetStickerData().Positions.ContainsKey(stickerName));

        private static CardModificationInfo GetStickerMod(this PlayableCard card)
        {
            foreach (CardModificationInfo cardMod in card.TemporaryMods)
            {
                if (string.IsNullOrEmpty(cardMod.singletonId))
                    continue;

                if (cardMod.singletonId.StartsWith("Stickers"))
                    return cardMod;
            }
            return card.Info.GetStickerMod();
        }

        private static CardModificationInfo GetStickerMod(this CardInfo info, bool force = false)
        {
            foreach (CardModificationInfo cardMod in info.Mods)
            {
                if (string.IsNullOrEmpty(cardMod.singletonId))
                    continue;

                if (cardMod.singletonId.StartsWith("Stickers"))
                    return cardMod;
            }
            if (!force)
                return null;
            CardModificationInfo stickerMod = new()
            {
                singletonId = "Stickers"
            };
            Part3SaveData.Data.deck.ModifyCard(info, stickerMod);
            return stickerMod;
        }

        private static Dictionary<string, Vector3> GetStickerVectors(this CardModificationInfo stickerMod, string vectorKey)
        {
            string vectorKeyStart = $"[Sticker{vectorKey}:";

            if (stickerMod.singletonId.Contains($"[Sticker{vectorKey}:"))
            {
                int startIndex = stickerMod.singletonId.IndexOf(vectorKeyStart);
                string vectorData = stickerMod.singletonId.Substring(startIndex).Replace(vectorKeyStart, "");
                if (vectorData.Contains("]"))
                {
                    int endIndex = vectorData.IndexOf("]");
                    vectorData = vectorData.Substring(0, endIndex).Replace("]", "");

                    return ParseVectorMap(vectorData);
                }
            }
            return null;
        }

        private static Dictionary<string, Ability> GetStickerAbility(this CardModificationInfo stickerMod)
        {
            string keyStart = "[StickerAbility:";
            Dictionary<string, Ability> retval = new();
            if (stickerMod.singletonId.Contains(keyStart))
            {
                int startIndex = stickerMod.singletonId.IndexOf(keyStart);
                string vectorData = stickerMod.singletonId.Substring(startIndex).Replace(keyStart, "");
                if (vectorData.Contains("]"))
                {
                    int endIndex = vectorData.IndexOf("]");
                    vectorData = vectorData.Substring(0, endIndex).Replace("]", "");

                    string[] stickerMatches = vectorData.Split(',');

                    foreach (string pair in stickerMatches)
                    {
                        string[] pairParts = pair.Split(':');
                        if (pairParts.Length != 2)
                            continue;
                        if (int.TryParse(pairParts[1], out int abilityNumber))
                        {
                            retval[pairParts[0]] = (Ability)abilityNumber;
                        }
                        else
                        {
                            string[] splits = pairParts[1].Split('_');
                            if (splits.Length != 2)
                                continue;
                            retval[pairParts[0]] = GuidManager.GetEnumValue<Ability>(splits[0], splits[1]);
                        }
                    }
                }
            }
            return retval;
        }

        internal static CardStickerData GetStickerData(this Card card)
        {
            if (card is PlayableCard pCard)
            {
                CardModificationInfo stickerMod = pCard.GetStickerMod();
                return new CardStickerData()
                {
                    Positions = stickerMod?.GetStickerVectors("Positions") ?? new(),
                    Rotations = stickerMod?.GetStickerVectors("Rotations") ?? new(),
                    Scales = stickerMod?.GetStickerVectors("Scales") ?? new(),
                    Ability = stickerMod?.GetStickerAbility() ?? new()
                };
            }
            else
            {
                return card.Info.GetStickerData();
            }
        }

        internal static CardStickerData GetStickerData(this CardInfo info)
        {
            CardModificationInfo stickerMod = info.GetStickerMod();
            return new CardStickerData()
            {
                Positions = stickerMod?.GetStickerVectors("Positions") ?? new(),
                Rotations = stickerMod?.GetStickerVectors("Rotations") ?? new(),
                Scales = stickerMod?.GetStickerVectors("Scales") ?? new(),
                Ability = stickerMod?.GetStickerAbility() ?? new()
            };
        }

        internal static CardInfo SetStickerData(this CardInfo info, CardStickerData data)
        {
            CardModificationInfo stickerMod = info.GetStickerMod(force: true);
            stickerMod.singletonId = data.ToString();
            return info;
        }

        internal static void UpdateStickerPosition(this CardInfo info, string stickerKey, Vector3 position)
        {
            CardStickerData data = info.GetStickerData();
            data.Positions[stickerKey] = position;
            info.SetStickerData(data);
        }

        internal static void UpdateStickerRotation(this CardInfo info, string stickerKey, Vector3 rotation)
        {
            CardStickerData data = info.GetStickerData();
            data.Rotations[stickerKey] = rotation;
            info.SetStickerData(data);
        }

        internal static void UpdateStickerScale(this CardInfo info, string stickerKey, Vector3 scale)
        {
            CardStickerData data = info.GetStickerData();
            data.Scales[stickerKey] = scale;
            info.SetStickerData(data);
        }

        internal static void ClearStickerAppearance(string stickerKey)
        {
            foreach (CardInfo card in Part3SaveData.Data.deck.Cards)
            {
                CardStickerData data = card.GetStickerData();
                if (data.Positions.ContainsKey(stickerKey))
                {
                    data.Positions.Remove(stickerKey);
                    data.Rotations.Remove(stickerKey);
                    data.Scales.Remove(stickerKey);
                    card.SetStickerData(data);
                }
            }
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

        internal static GameObject GetSticker(string stickerName, bool interactable, bool project, StickerStyle style, int stencilNumber = StickerInterfaceManager.INTERFACE_STENCIL_NUMBER, Ability ability = Ability.None, PlayableCard card = null)
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

            float widthOverHeight = texture.width / ((float)texture.height);
            float width = 0.5f;
            float height = width / widthOverHeight;
            sticker.transform.localScale = new(width, height, 1f);

            if (project)
            {
                textureRenderer.enabled = false;

                GameObject projectorObject = new("Projector");
                projectorObject.transform.SetParent(sticker.transform);
                projectorObject.transform.localPosition = new(0f, 0f, -0.35f);

                Projector projector = projectorObject.AddComponent<Projector>();
                projector.material = new(AssetBundleManager.Shaders.Find(sh => sh.name.Equals("P03/Projector/StenciledSticker")));
                projector.material.SetColor("_Color", Color.white);
                projector.material.SetTexture("_ShadowTex", texture);
                projector.material.SetInt("_StencilNumber", interactable ? StickerInterfaceManager.INTERFACE_STENCIL_NUMBER : stencilNumber);
                projector.farClipPlane = 10f;
                projector.nearClipPlane = 0f;
                projector.fieldOfView = 25;
                projector.ignoreLayers = 1 << 2;
                projector.orthographic = true;
                projector.orthographicSize = 0.25f;

                // GameObject projectorSphere = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                // projectorSphere.transform.SetParent(sticker.transform);
                // projectorSphere.transform.localPosition = projector.transform.localPosition;
                // projectorSphere.transform.localScale = new(0.05f, 0.05f, 0.05f);

                if (interactable)
                {
                    GameObject intProjectorObject = new("InteractiveProjector");
                    intProjectorObject.transform.SetParent(sticker.transform);
                    intProjectorObject.transform.localPosition = new(0f, 0f, -0.35f);

                    Projector intProjector = intProjectorObject.AddComponent<Projector>();
                    intProjector.material = new(AssetBundleManager.Shaders.Find(sh => sh.name.Equals("P03/Projector/UnStenciledSticker")));
                    intProjector.material.SetColor("_Color", Color.white);
                    intProjector.material.SetTexture("_ShadowTex", texture);
                    intProjector.farClipPlane = 10f;
                    intProjector.nearClipPlane = 0f;
                    intProjector.fieldOfView = 25;
                    intProjector.ignoreLayers = 1 << 2;
                    intProjector.orthographic = true;
                    intProjector.orthographicSize = 0.25f;

                    StickerDrag dragger = sticker.AddComponent<StickerDrag>();
                    dragger.StickerName = stickerName;
                    dragger.StenciledProjector = projectorObject;
                    dragger.UnStenciledProjector = intProjectorObject;
                    sticker.AddComponent<StickerRotate>();

                }
                else
                {
                    projectorObject.AddComponent<Camera>().depth = -5;
                    projectorObject.AddComponent<StickerProjector>();

                    if (ability != Ability.None)
                    {
                        StickerRulebook rulebook = sticker.AddComponent<StickerRulebook>();
                        rulebook.Ability = ability;
                        rulebook.Card = card;
                    }
                }
            }

            if (!interactable && ability == Ability.None)
            {
                UnityEngine.Object.Destroy(sticker.GetComponent<MeshCollider>());
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
            {
                yield break;
            }

            if (!DebugStickers && StickerRewards.Where(kvp => ModdedAchievementManager.AchievementById(kvp.Value).IsUnlocked).Count() == 0)
            {
                yield break;
            }

            if (__instance.gameObject.GetComponent<StickerInterfaceManager>() == null)
            {
                __instance.gameObject.AddComponent<StickerInterfaceManager>();
            }

            GameObject stickerButton = UnityEngine.Object.Instantiate(ResourceBank.Get<GameObject>("prefabs/rulebook/TableTablet"), __instance.transform);
            UnityEngine.Object.Destroy(stickerButton.GetComponentInChildren<TableRuleBook>());

            stickerButton.transform.localScale = new(0.5f, 0.5f, 0.5f);
            stickerButton.transform.localEulerAngles = new(0f, 90f, 0f);

            OpenRulebookInteractable previousInteractable = stickerButton.GetComponentInChildren<OpenRulebookInteractable>();
            OpenStickerInteractable osi = previousInteractable.gameObject.AddComponent<OpenStickerInteractable>();
            UnityEngine.Object.Destroy(previousInteractable);
            osi.SetEnabled(true);

            stickerButton.name = "StickerBook";

            Vector3 targetPosition = new(0f, 0.1f, -1.75f);
            stickerButton.transform.localPosition = targetPosition + new Vector3(0f, 0f, -2f);

            stickerButton.SetActive(true);

            Tween.LocalPosition(stickerButton.transform, targetPosition, 0.2f, 0f);

            yield return EventManagement.SayDialogueOnce("P03StickerBook", EventManagement.SAW_STICKER_BOOK);

            yield break;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.CleanUpDeckPiles))]
        [HarmonyPostfix]
        private static IEnumerator CleanUpStickerBook(IEnumerator sequence, SelectableCardArray __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            Transform tablet = __instance.transform.Find("StickerBook");
            if (tablet != null)
            {
                Tween.Position(tablet, tablet.transform.position + new Vector3(0f, 0f, -2f), 0.2f, 0f, completeCallback: () => UnityEngine.Object.Destroy(tablet.gameObject));
            }
            yield return sequence;
        }

        private static int LAST_STENCIL_NUMBER = StickerInterfaceManager.INTERFACE_STENCIL_NUMBER + 1;

        private static void CreateStencilDuplicate(GameObject target, int stencilNumber)
        {
            bool canCreate = true;
            foreach (Transform child in target.transform)
            {
                if (child.gameObject.name.Equals("PortraitStencil"))
                {
                    child.GetComponent<Renderer>().material.SetInt("_StencilNumber", stencilNumber);
                    canCreate = false;
                }
            }
            if (canCreate)
            {
                GameObject stencilPortrait = UnityEngine.Object.Instantiate(target, target.transform.parent);
                stencilPortrait.name = "PortraitStencil";

                List<Transform> children = new();
                foreach (Transform t in stencilPortrait.transform)
                    children.Add(t);
                foreach (Transform t in children)
                    UnityEngine.Object.Destroy(t.gameObject);

                Renderer stencilRenderer = stencilPortrait.GetComponent<Renderer>();
                stencilRenderer.material = new(STENCIL_SHADER);
                stencilRenderer.material.SetInt("_StencilNumber", stencilNumber);
                stencilPortrait.transform.SetParent(target.transform);
                stencilPortrait.transform.localPosition = Vector3.zero;
                stencilPortrait.transform.localScale = new(1f, 1f, 1f);
                stencilPortrait.transform.localEulerAngles = Vector3.zero;
            }
        }

        private static readonly List<string> STENCIL_PATHS = new() { "ScreenFront", "Rails", "Bottom", "Top", "Top/MetalSlider" };

        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPostfix]
        private static void ApplyStickersToCard(ref Card __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (__instance.StatsLayer is not DiskRenderStatsLayer)
            {
                return;
            }

            for (int i = 1; i <= 5; i++)
            {
                __instance.StatsLayer.transform.Find($"Top/Stickers/Sticker_{i}").gameObject.layer = 2;
            }

            foreach (Projector proj in __instance.GetComponentsInChildren<Projector>().ToList())
            {
                UnityEngine.Object.Destroy(proj.transform.parent.gameObject);
            }

            CardStickerData data = __instance.GetStickerData();
            bool activeInterface = StickerInterfaceManager.Instance != null && StickerInterfaceManager.Instance.StickerInterfaceActive;
            bool cardHasStickers = data.Positions.Count > 0;

            // Figure out the appropriate stencil number for the card
            int stencilNumber = StickerInterfaceManager.INTERFACE_STENCIL_NUMBER;
            if (!activeInterface && cardHasStickers)
            {
                if (LAST_STENCIL_NUMBER == 255)
                    LAST_STENCIL_NUMBER = StickerInterfaceManager.INTERFACE_STENCIL_NUMBER + 1;
                else
                    LAST_STENCIL_NUMBER++;
                stencilNumber = LAST_STENCIL_NUMBER;
            }

            // We create a little stencil world around each card
            foreach (string path in STENCIL_PATHS)
                CreateStencilDuplicate(__instance.StatsLayer.transform.Find(path).gameObject, stencilNumber);

            // // The card art face also needs to set the stencil buffer, but that uses the
            // // uber shader, and I can't recompile that one. So I have to duplicate it
            // GameObject portraitObj = __instance.StatsLayer.transform.Find("ScreenFront").gameObject;
            // bool canCreate = true;
            // foreach (Transform child in portraitObj.transform)
            // {
            //     if (child.gameObject.name.Equals("PortraitStencil"))
            //     {
            //         child.GetComponent<Renderer>().material.SetInt("_StencilNumber", activeInterface || !positions.ContainsKey(cardKey) ? StickerInterfaceManager.INTERFACE_STENCIL_NUMBER : LAST_STENCIL_NUMBER);
            //         canCreate = false;
            //     }
            // }
            // if (canCreate)
            // {
            //     GameObject stencilPortrait = UnityEngine.Object.Instantiate(portraitObj, portraitObj.transform.parent);
            //     stencilPortrait.name = "PortraitStencil";
            //     UnityEngine.Object.Destroy(stencilPortrait.transform.Find("ScreenOverlay").gameObject);
            //     UnityEngine.Object.Destroy(stencilPortrait.transform.Find("Cracks").gameObject);
            //     Renderer stencilRenderer = stencilPortrait.GetComponent<Renderer>();
            //     stencilRenderer.material = new(STENCIL_SHADER);
            //     stencilRenderer.material.SetInt("_StencilNumber", activeInterface || !positions.ContainsKey(cardKey) ? StickerInterfaceManager.INTERFACE_STENCIL_NUMBER : LAST_STENCIL_NUMBER);
            //     stencilPortrait.transform.SetParent(portraitObj.transform);
            //     stencilPortrait.transform.localPosition = Vector3.zero;
            //     stencilPortrait.transform.localScale = new(1f, 1f, 1.01f);
            //     stencilPortrait.transform.localEulerAngles = Vector3.zero;
            // }

            if (cardHasStickers)
            {
                // // Okay - step one - we need to replace the standard shader with our shader
                // // Our shader functions the same as the standard shader, except it will set the stencil buffer
                // foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
                // {
                //     foreach (Material mat in renderer.materials.Where(m => m.shader.name.Equals("P03/Projector/StandardWithShader")))
                //     {
                //         renderer.material.SetInt("_StencilNumber", activeInterface ? StickerInterfaceManager.INTERFACE_STENCIL_NUMBER : LAST_STENCIL_NUMBER);
                //     }
                //     Material[] materials = renderer.materials;
                //     for (int i = 0; i < materials.Length; i++)
                //     {
                //         if (!materials[i].shader.name.Equals("Standard"))
                //             continue;

                //         // Material newMat = new(STANDARD_STENCIL_SHADER);
                //         // newMat.CopyPropertiesFromMaterial(materials[i]);
                //         // newMat.SetInt("_StencilNumber", activeInterface ? StickerInterfaceManager.INTERFACE_STENCIL_NUMBER : LAST_STENCIL_NUMBER);
                //         // materials[i] = newMat;
                //         materials[i].shader = STANDARD_STENCIL_SHADER;
                //         materials[i].SetInt("_StencilNumber", activeInterface ? StickerInterfaceManager.INTERFACE_STENCIL_NUMBER : LAST_STENCIL_NUMBER);
                //     }
                //     renderer.materials = materials;
                // }

                foreach (string stickerKey in data.Positions.Keys)
                {
                    Ability ability = data.Ability.ContainsKey(stickerKey) ? data.Ability[stickerKey] : Ability.None;
                    GameObject sticker = GetSticker(stickerKey, activeInterface, true, StickerStyle.Standard, LAST_STENCIL_NUMBER, ability, __instance as PlayableCard);
                    sticker.transform.SetParent(__instance.StatsLayer.transform);
                    if (activeInterface)
                        sticker.GetComponent<StickerDrag>().Initialize();
                    sticker.transform.localPosition = data.Positions[stickerKey] + new Vector3(0f, 0f, 0.1f);
                    sticker.transform.localEulerAngles = new(0f, 180f, 90f);

                    if (data.Scales.ContainsKey(stickerKey))
                    {
                        sticker.transform.localScale = data.Scales[stickerKey];
                    }

                    if (data.Rotations.ContainsKey(stickerKey))
                    {
                        sticker.transform.localEulerAngles = data.Rotations[stickerKey];
                    }

                    // Reparent
                    if ((__instance is PlayableCard pCard && pCard.OnBoard) || __instance is SelectableCard)
                        sticker.transform.SetParent(__instance.StatsLayer.transform.Find(sticker.transform.localPosition.x > 0 ? "Top" : "Bottom"), true);
                }
            }
        }
    }
}
