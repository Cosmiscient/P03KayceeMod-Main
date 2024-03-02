using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class CardExporter : ManagedBehaviour
    {

        [HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.ValidStatsLayer))]
        [HarmonyPostfix]
        private static void AttachExporter(ref CardRenderCamera __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (__instance.gameObject.GetComponent<CardExporter>() == null)
            {
                P03Plugin.Log.LogDebug("Adding Card Exporter!");
                __instance.gameObject.AddComponent<CardExporter>();
            }
        }

        public void StartCardExport()
        {
            inRender = true;
            StartCoroutine(ExportAllCards());
        }

        [SerializeField]
        private readonly GameObject temporaryHolding;

        [SerializeField]
        private readonly PlayableCard dummyCard;

        private static readonly RenderStatsLayer statsLayer = null;

        private bool IsTalkingCard(CardInfo info) => info.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.DynamicPortrait) || info.animatedPortrait != null;

        [SerializeField]
        public float xOffset = 0.4f;

        internal static readonly string[] GameObjectPaths = new string[]
        {
            "Anim/CardBase/Rails",
            "Anim/CardBase/Top",
            "Anim/CardBase/Bottom"
        };

        private static bool inRender = false;
        private static bool skipBulkRender = false;

        public override void ManagedUpdate()
        {
            if (!inRender &&
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                if (Input.GetKey(KeyCode.X))
                {
                    StartCardExport();
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    skipBulkRender = true;
                    StartCardExport();
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    ExportAllSigils();
                }
                else if (Input.GetKey(KeyCode.F))
                {
                    SetupForFaceCamera(true);
                    StartScreenshotAllFaces();
                }
                else if (Input.GetKey(KeyCode.G))
                {
                    SetupForFaceCamera(false);
                    StartScreenshotAllFaces();
                }
            }
        }

        private static void SetupForFaceCamera(bool makeWhite)
        {
            // The camera's depth setting should make it to where we don't have to turn off a bunch
            // of game objects. We mostly just need to set the color, position, and intensity of lights
            // and the position/far clip plane of the camera

            Transform cameraParent = ViewManager.Instance.cameraParent;
            cameraParent.localPosition = new(0f, 8.25f, 0.54f); // 0 8.25 0.54
            cameraParent.localEulerAngles = Vector3.zero;

            Camera camera = ViewManager.Instance.pixelCamera;
            camera.farClipPlane = 7;

            if (makeWhite)
                ExplorableAreaManager.Instance.SetHangingLightColors(Color.white, Color.white);

            var hangingLight = ExplorableAreaManager.Instance.hangingLight;
            hangingLight.intensity = 1;
            hangingLight.type = LightType.Spot;
            hangingLight.spotAngle = 100;
            hangingLight.innerSpotAngle = 100;
            hangingLight.transform.localPosition = new(0.1f, 6.6f, -1.09f); // 0.1 6.6 -1.09
            hangingLight.transform.localEulerAngles = Vector3.zero;

            P03AnimationController.Instance.transform.Find("Body/Chest").gameObject.SetActive(false);
            P03AnimationController.Instance.transform.Find("Body/RotatingHead/Head/HeadAnim/Head-Cable-Left").gameObject.SetActive(false);
            P03AnimationController.Instance.transform.Find("Body/RotatingHead/Head/HeadAnim/Head-Cable-Right").gameObject.SetActive(false);
        }

        public static void CaptureTransparentScreenshot(Camera cam, int width, int height, string screengrabfile_path)
        {
            // https://discussions.unity.com/t/capture-rendered-scene-to-png-with-background-transparent/1705/6
            // This is slower, but seems more reliable.
            var bak_cam_targetTexture = cam.targetTexture;
            var bak_cam_clearFlags = cam.clearFlags;
            var bak_RenderTexture_active = RenderTexture.active;

            var tex_white = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var tex_black = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
            // Must use 24-bit depth buffer to be able to fill background.
            var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var grab_area = new Rect(0, 0, width, height);

            RenderTexture.active = render_texture;
            cam.targetTexture = render_texture;
            cam.clearFlags = CameraClearFlags.SolidColor;

            cam.backgroundColor = Color.black;
            cam.Render();
            tex_black.ReadPixels(grab_area, 0, 0);
            tex_black.Apply();

            cam.backgroundColor = Color.white;
            cam.Render();
            tex_white.ReadPixels(grab_area, 0, 0);
            tex_white.Apply();

            // Create Alpha from the difference between black and white camera renders
            for (int y = 0; y < tex_transparent.height; ++y)
            {
                for (int x = 0; x < tex_transparent.width; ++x)
                {
                    float alpha = tex_white.GetPixel(x, y).r - tex_black.GetPixel(x, y).r;
                    alpha = 1.0f - alpha;
                    Color color;
                    if (alpha == 0)
                    {
                        color = Color.clear;
                    }
                    else
                    {
                        color = tex_black.GetPixel(x, y) / alpha;
                    }
                    color.a = alpha;
                    tex_transparent.SetPixel(x, y, color);
                }
            }

            // Encode the resulting output texture to a byte array then write to the file
            byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
            File.WriteAllBytes(screengrabfile_path, pngShot);

            cam.clearFlags = bak_cam_clearFlags;
            cam.targetTexture = bak_cam_targetTexture;
            RenderTexture.active = bak_RenderTexture_active;
            RenderTexture.ReleaseTemporary(render_texture);

            Texture2D.Destroy(tex_black);
            Texture2D.Destroy(tex_white);
            Texture2D.Destroy(tex_transparent);
        }

        private void StartScreenshotAllFaces()
        {
            inRender = true;
            StartCoroutine(ScreenshotAllFaces());
        }

        private IEnumerator ScreenshotAllFaces()
        {
            yield return new WaitForSeconds(0.3f);

            foreach (P03AnimationController.Face face in Enum.GetValues(typeof(P03AnimationController.Face)))
            {
                if (face == P03AnimationController.Face.NUM_FACES)
                    continue;

                string facecode = face.ToString().ToLowerInvariant();
                P03AnimationController.Instance.headAnim.Rebind();
                yield return new WaitForSeconds(0.1f);
                P03AnimationController.Instance.transform.Find("Body/RotatingHead/Head/HeadAnim/Head-Cable-Left").gameObject.SetActive(false);
                P03AnimationController.Instance.transform.Find("Body/RotatingHead/Head/HeadAnim/Head-Cable-Right").gameObject.SetActive(false);
                P03AnimationController.Instance.SetAntennaShown(face == P03AnimationController.Face.TelegrapherBossOnline);
                P03AnimationController.Instance.ShowInfected(facecode.Contains("myco"));
                P03AnimationController.Instance.SwitchToFace(face, false, false);
                yield return new WaitForSeconds(facecode.Contains("myco") ? 1.0f : 0.1f);

                string outfile = $"cardexports/p03face_{facecode}.png";
                CaptureTransparentScreenshot(ViewManager.Instance.pixelCamera, Screen.width, Screen.height, outfile);
            }

            // And now we need to iterate through all the faces
            P03AnimationController.Instance.headAnim.Rebind();
            yield return new WaitForSeconds(1.5f);
            P03AnimationController.Instance.SwitchToFace(P03ModularNPCFace.ModularNPCFace, false, false);
            foreach (P03ModularNPCFace.FaceSet faceset in Enum.GetValues(typeof(P03ModularNPCFace.FaceSet)))
            {
                string facecode = $"p03face_npc_{faceset}";
                NPCDescriptor npc = new(faceset, CompositeFigurine.FigurineType.SettlerMan);
                P03ModularNPCFace.Instance.SetNPCFace(npc.faceCode);
                yield return new WaitForSeconds(0.1f);

                string outfile = $"cardexports/{facecode}.png";
                CaptureTransparentScreenshot(ViewManager.Instance.pixelCamera, Screen.width, Screen.height, outfile);
                yield return new WaitForSeconds(0.2f);
            }

            inRender = false;
        }

        internal static Bounds GetMaxBounds(GameObject g)
        {
            List<Renderer> renderers = new();
            foreach (string p in GameObjectPaths)
            {
                Transform t = g.transform.Find(p);
                if (t != null)
                {
                    renderers.AddRange(t.gameObject.GetComponents<Renderer>());
                }
            }

            if (renderers.Count == 0)
            {
                return new Bounds(g.transform.position, Vector3.zero);
            }

            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

        private static readonly Dictionary<string, string> imageCache = new();
        private static string GetImageEmbedded(string cardName)
        {
            if (imageCache.Keys.Contains(cardName))
            {
                return imageCache[cardName];
            }

            byte[] sourceBytes = cardName.Equals("queue", StringComparison.InvariantCultureIgnoreCase) ?
                                  TextureHelper.GetResourceBytes("cadslot_down.png", typeof(CardExporter).Assembly) :
                                  File.ReadAllBytes($"cardexports/{cardName}.png");
            string b64string = Convert.ToBase64String(sourceBytes);

            imageCache[cardName] = "div." + cardName + " {\n\tbackground-image: url(data:image/png;base64," + b64string + ");\n\twidth: 62px;\n\theight: 92px;\n\tbackground-size: 62px 92px;\n}\n";

            return imageCache[cardName];
        }

        private static readonly System.Text.RegularExpressions.Regex rgx = new("[^a-zA-Z0-9]");
        private static string GetRepr(CardInfo info) => info.mods == null || info.mods.Count == 0 ? info.name : rgx.Replace(CustomCards.ConvertCardToCompleteCode(info), "");

        private static readonly HashSet<string> GeneratedThisRun = new();
        private static bool Generated(CardInfo info)
        {
            if (File.Exists($"cardexports/{GetRepr(info)}.png"))
                return true;

            if (info.mods == null || info.mods.Count == 0)
            {
                return File.Exists($"cardexports/{GetRepr(info)}.png");
            }

            if (GeneratedThisRun.Contains(GetRepr(info)))
            {
                return true;
            }

            GeneratedThisRun.Add(GetRepr(info));
            return false;
        }

        private IEnumerator GenerateCard(PlayableCard card, Vector3 renderPosition, Texture2D screenshot, Camera camera)
        {
            string filename = $"cardexports/{GetRepr(card.Info)}.png";

            card.gameObject.transform.localPosition = renderPosition;
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForEndOfFrame();

            Texture2D finalTexture = null;

            try
            {
                screenshot.ReadPixels(new(0, 0, Screen.currentResolution.width, Screen.currentResolution.height), 0, 0, false);
                screenshot.Apply();

                Bounds cardBounds = GetMaxBounds(card.gameObject);
                Vector2 lower = camera.WorldToScreenPoint(cardBounds.min);
                Vector2 upper = camera.WorldToScreenPoint(cardBounds.max);
                int width = Mathf.RoundToInt(Mathf.Abs(lower.x - upper.x));
                int height = Mathf.RoundToInt(Mathf.Abs(lower.y - upper.y));
                int xMin = Mathf.RoundToInt(Mathf.Min(lower.x, upper.x));
                int yMin = Mathf.RoundToInt(Mathf.Min(lower.y, upper.y));

                finalTexture = new(width, height)
                {
                    filterMode = FilterMode.Trilinear
                };

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        finalTexture.SetPixel(x, y, screenshot.GetPixel(x + xMin, y + yMin));
                    }
                }

                P03Plugin.Log.LogDebug("Writing file");
                File.WriteAllBytes(filename, ImageConversion.EncodeToPNG(finalTexture));
            }
            catch (Exception ex)
            {
                P03Plugin.Log.LogError(ex);
            }

            card.transform.localPosition = card.transform.localPosition + new Vector3(0, 10, 0);
            yield return new WaitForEndOfFrame();
            if (card != null)
            {
                DestroyImmediate(card.gameObject);
            }

            if (finalTexture != null)
            {
                DestroyImmediate(finalTexture);
            }

            yield return new WaitForEndOfFrame();
        }

        private static List<KeyValuePair<string, string>> GetEnumValues(Type type)
        {
            List<KeyValuePair<string, string>> itemList = new();
            foreach (var item in Enum.GetValues(type))
                itemList.Add(new(((int)item).ToString(), item.ToString()));

            string startKey = type.Name + "_";
            var saveData = Traverse.Create(ModdedSaveManager.SaveData).Field("SaveData").GetValue<Dictionary<string, Dictionary<string, object>>>();
            foreach (var item in saveData[InscryptionAPIPlugin.ModGUID])
            {
                if (item.Key.StartsWith(startKey))
                {
                    itemList.Add(new(item.Value.ToString(), item.Key.Replace(startKey, "")));
                }
            }

            return itemList;
        }

        private void ExportJsons()
        {
            foreach (var name in CardManager.AllCardsCopy.Select(ci => ci.name))
            {
                var cardInfo = CardLoader.GetCardByName(name);
                var json = JsonUtility.ToJson(cardInfo);
                if (cardInfo.evolveParams != null && cardInfo.evolveParams.evolution != null)
                {
                    json = json.Substring(0, json.Length - 1);
                    json += ",\"evolveParams\":{\"evolution\":\"" + cardInfo.evolveParams.evolution.name;
                    json += "\",\"turns\":" + cardInfo.evolveParams.turnsToEvolve + "}}";
                }
                if (cardInfo.iceCubeParams != null && cardInfo.iceCubeParams.creatureWithin != null)
                {
                    json = json.Substring(0, json.Length - 1);
                    json += ",\"iceCubeParams\":{\"creatureWithin\":\"" + cardInfo.iceCubeParams.creatureWithin.name + "\"}}";
                }
                File.WriteAllText($"cardexports/card_{cardInfo.name}.json", json);

                // And the card portrait
                if (cardInfo.portraitTex != null && cardInfo.portraitTex.texture != null)
                {
                    try
                    {
                        Texture2D abTexture = TextureHelper.DuplicateTexture(cardInfo.portraitTex.texture);
                        // for (int x = 0; x < abTexture.width; x++)
                        //     for (int y = 0; y < abTexture.height; y++)
                        //         if (abTexture.GetPixel(x, y).a > 0.1)
                        //             abTexture.SetPixel(x, y, new(0f, 0f, 0f, 1f));
                        //         else
                        //             abTexture.SetPixel(x, y, new(0f, 0f, 0f, 0f));

                        abTexture.Apply();
                        File.WriteAllBytes($"cardexports/card_{cardInfo.name}.png", ImageConversion.EncodeToPNG(abTexture));
                        GameObject.Destroy(abTexture);
                    }
                    catch (Exception ex)
                    {
                        P03Plugin.Log.LogWarning($"Failed to generate portrait export for {cardInfo.name}");
                        P03Plugin.Log.LogWarning(ex);
                    }
                }
                else
                {
                    P03Plugin.Log.LogInfo($"Skipping portrait export for {cardInfo.name} because it is blank");
                }
            }
            foreach (var ab in AbilityManager.AllAbilities)
            {
                var json = JsonUtility.ToJson(ab.Info);
                var name = ab.Info.rulebookName.Replace(" ", "_");
                File.WriteAllText($"cardexports/ability_{name}.json", json);

                // And the ability icon
                Texture2D abTexture = TextureHelper.DuplicateTexture(ab.Texture as Texture2D);
                for (int x = 0; x < abTexture.width; x++)
                    for (int y = 0; y < abTexture.height; y++)
                        if (abTexture.GetPixel(x, y).a > 0.1)
                            abTexture.SetPixel(x, y, new(0f, .88f, 1f, 1f));
                        else
                            abTexture.SetPixel(x, y, new(0f, 0f, 0f, 0f));

                abTexture.Apply();
                File.WriteAllBytes($"cardexports/ability_{name}.png", ImageConversion.EncodeToPNG(abTexture));
                GameObject.Destroy(abTexture);


            }
            // Now the special stuff
            string specialJson = "{\n";
            foreach (var enumType in new List<Type>() { typeof(CardTemple), typeof(SpecialTriggeredAbility), typeof(CardMetaCategory), typeof(AbilityMetaCategory), typeof(CardAppearanceBehaviour.Appearance) })
            {
                specialJson += "\t\"" + enumType.Name + "\": {";
                foreach (var kvp in GetEnumValues(enumType))
                    specialJson += "\t\t\"" + kvp.Key + "\": \"" + kvp.Value + "\",\n";

                specialJson = specialJson.Substring(0, specialJson.Length - 2) + "\n";

                specialJson += "\t},\n";
            }
            specialJson = specialJson.Substring(0, specialJson.Length - 2) + "\n";
            specialJson += "}";
            File.WriteAllText($"cardexports/special_enums.json", specialJson);

            // List<CardInfo> cards = CardManager.AllCardsCopy
            //                                   .Where(ci => !ci.name.StartsWith("!") &&
            //                                                ci.temple == CardTemple.Tech)
            //                                   .ToList();

            // string table = "id,name,artist,expansion,cost,attack,health,sigils,quality,region\n";
            // foreach (var card in cards)
            // {
            //     table += card.name + "," + card.DisplayedNameEnglish + ",,";
            //     if (card.HasAnyOfCardMetaCategories(CardMetaCategory.Rare, CardMetaCategory.ChoiceNode))
            //     {
            //         if (card.name.StartsWith("P03KCMXP2"))
            //             table += "Expansion Pack 2";
            //         else if (card.name.StartsWith("P03KCMXP1"))
            //             table += "Expansion Pack 1";
            //         else if (card.name.StartsWith("P03KCM"))
            //             table += "P03 in Kaycee's Mod";
            //         else if (card.IsBaseGameCard())
            //             table += "Vanilla";
            //     }
            //     table += ",";
            //     if (card.energyCost > 0)
            //         table += $"{card.energyCost} energy,";
            //     table += card.Attack + "," + card.Health + ",";
            //     table += "\"";
            //     List<string> abilityLinks = new();
            //     foreach (var sigil in card.Abilities)
            //     {
            //         var ab = AbilitiesUtil.GetInfo(sigil);
            //         string pageName = ab.rulebookName.Replace(" ", "");
            //         abilityLinks.Add($"[[{pageName}|{ab.rulebookName}]]");
            //     }
            //     table += string.Join(", ", abilityLinks) + "\"";
            //     if (card.HasCardMetaCategory(CardMetaCategory.Rare))
            //         table += "Rare,";
            //     else if (card.HasCardMetaCategory(CardMetaCategory.ChoiceNode))
            //         table += "Common,";
            //     else
            //         table += "Unobtainable,";
            // }
        }

        private void ExportAllSigils()
        {
            if (!Directory.Exists("cardexports"))
            {
                Directory.CreateDirectory("cardexports");
            }

            List<AbilityManager.FullAbility> mySigils = GuidManager.GetValues<Ability>()
                                      .Select(a => AbilityManager.AllAbilities.FirstOrDefault(f => f.Info.ability == a))
                                      .Where(ai => ai != null && ai.Info != null && !string.IsNullOrEmpty(ai.Info.name))
                                      .Where(ai => ai.Info.name.StartsWith(P03Plugin.PluginGuid))
                                      .OrderBy(ai => ai.Info.rulebookName)
                                      .ToList();

            P03Plugin.Log.LogInfo($"Running sigil export for {mySigils.Count} sigils");

            string mdTable = "|Icon|Name|Power Level|Description|\n|---|---|---|---|\n";
            foreach (AbilityManager.FullAbility item in mySigils)
            {
                mdTable += $"|[[https://github.com/Cosmiscient/P03KayceeMod-Main/blob/main/assets/{item.Texture.name}.png]]";
                mdTable += $"|{item.Info.rulebookName}|{item.Info.powerLevel}|{item.Info.rulebookDescription}|\n";
            }

            File.WriteAllText("cardexports/sigil_table.md", mdTable);
        }

        public IEnumerator ExportAllCards()
        {
            ViewManager.Instance.SwitchToView(View.MapDeckReview);
            yield return new WaitForSeconds(0.25f);
            Tween.LocalRotation(ViewManager.Instance.cameraParent, new Vector3(90f, 0f, 0f), 0f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
            ViewManager.Instance.controller.LockState = ViewLockState.Locked;

            Color originalHangingLightColor = ExplorableAreaManager.Instance.hangingLight.color;
            Color originalHangingLightCardColor = ExplorableAreaManager.Instance.hangingCardsLight.color;
            ExplorableAreaManager.Instance.SetHangingLightColors(originalHangingLightColor, originalHangingLightCardColor);

            bool noiseEnabled = GameOptions.optionsData.noiseEnabled;
            GameOptions.optionsData.noiseEnabled = false;

            bool flickeringDisabled = GameOptions.optionsData.flickeringDisabled;
            GameOptions.optionsData.flickeringDisabled = true;

            bool screenshakeDisabled = GameOptions.optionsData.screenshakeDisabled;
            GameOptions.optionsData.screenshakeDisabled = true;

            //ExplorableAreaManager.Instance.SetHangingLightColors(GameColors.instance.brightSeafoam, GameColors.instance.brightSeafoam);

            yield return new WaitForSeconds(.15f);

            if (!Directory.Exists("cardexports"))
            {
                Directory.CreateDirectory("cardexports");
            }

            Camera camera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();
            Vector3 renderPosition = new(0f, 0f, 0f);

            Texture2D screenshot = new(Screen.currentResolution.width, Screen.currentResolution.height)
            {
                filterMode = FilterMode.Trilinear
            };

            List<CardInfo> cardsToRender = CardManager.AllCardsCopy.Where(ci => ci.temple == CardTemple.Tech && ci.name[0] != '!').ToList();
            if (skipBulkRender)
            {
                cardsToRender.Clear();
                skipBulkRender = false;
            }

            while (cardsToRender.Count > 0)
            {
                List<PlayableCard> currentBatch = new();

                while (cardsToRender.Count > 0 && currentBatch.Count < 20)
                {
                    CardInfo info = cardsToRender[0];
                    cardsToRender.RemoveAt(0);

                    PlayableCard card = CardSpawner.SpawnPlayableCard(info);
                    card.gameObject.transform.localPosition = new Vector3(card.gameObject.transform.localPosition.x + xOffset, card.gameObject.transform.localPosition.y, card.gameObject.transform.localPosition.z);
                    renderPosition = card.gameObject.transform.localPosition + new Vector3(0f, 0.5f, 0f);
                    card.gameObject.transform.localPosition = card.gameObject.transform.localPosition + new Vector3(0, 10, 0);
                    currentBatch.Add(card);
                    yield return new WaitForEndOfFrame();
                }

                yield return new WaitForSeconds(.5f);

                for (int i = 0; i < currentBatch.Count; i++)
                {
                    PlayableCard card = currentBatch[i];
                    yield return GenerateCard(card, renderPosition, screenshot, camera);
                }
            }

            List<EncounterBlueprintData> encountersToExport = EncounterManager.AllEncountersCopy.Where(ebd => Encounters.EncounterExtensions.P03OnlyEncounters.Contains(ebd.name)).ToList();

            // We also want to export all the base game encounters for act 3
            foreach (EncounterBlueprintData encounter in EncounterManager.BaseGameEncounters)
            {
                if (encounter.randomReplacementCards != null && encounter.randomReplacementCards.Any(ci => ci.temple != CardTemple.Tech))
                {
                    continue;
                }

                bool valid = true;
                bool doneSearching = false;

                foreach (List<EncounterBlueprintData.CardBlueprint> turn in encounter.turns)
                {
                    foreach (EncounterBlueprintData.CardBlueprint ci in turn)
                    {
                        if (ci.card == null)
                        {
                            continue;
                        }

                        if (ci.card.temple != CardTemple.Tech)
                        {
                            valid = false;
                            doneSearching = true;
                            break;
                        }

                        if (!Generated(ci.card))
                        {
                            valid = false;
                            doneSearching = true;
                            break;
                        }
                    }

                    if (doneSearching)
                    {
                        break;
                    }
                }

                if (valid)
                {
                    encountersToExport.Add(encounter);
                }
            }

            // Now we export all of the encounters
            foreach (EncounterBlueprintData encounter in encountersToExport)
            {
                P03Plugin.Log.LogDebug($"Generating encounter {encounter.name}");
                string export = $"<body><h2 class=\"title\">{encounter.name}</h2><table cellpadding=\"0\" cellspacing=\"0\"><tr><td/>";
                for (int i = encounter.minDifficulty; i <= encounter.maxDifficulty; i++)
                {
                    export += $"<td colspan=5 class=\"levelheader\">Level {i}</td><td><div class=\"levelspacer\"/></td>";
                }

                export += "</tr>";

                Dictionary<string, string> styleSet = new() {
                    { "queue", GetImageEmbedded("queue") }
                };

                Dictionary<int, List<List<CardInfo>>> turnPlanDictionary = new();
                Dictionary<int, float> runningPowerLevelTotals = new();
                for (int i = encounter.minDifficulty; i <= encounter.maxDifficulty; i++)
                {
                    runningPowerLevelTotals[i] = 0f;
                    turnPlanDictionary[i] = DiskCardGame.EncounterBuilder.BuildOpponentTurnPlan(encounter, i);
                }

                for (int turnNumber = 0; turnNumber < encounter.turns.Count; turnNumber++)
                {
                    P03Plugin.Log.LogDebug($"Generating turn {turnNumber + 1}");
                    List<EncounterBlueprintData.CardBlueprint> turn = encounter.turns[turnNumber];
                    export += $"<tr><td class=\"turnlabel\">Turn {turnNumber + 1}</td>";
                    for (int i = encounter.minDifficulty; i <= encounter.maxDifficulty; i++)
                    {
                        P03Plugin.Log.LogDebug($"Generating difficulty {i}");
                        List<CardInfo> turnEncounterCards = turnPlanDictionary[i].Count > turnNumber ? turnPlanDictionary[i][turnNumber] : new();
                        foreach (CardInfo currentCard in turnEncounterCards)
                        {
                            if (!Generated(currentCard))
                            {
                                PlayableCard tempCard = CardSpawner.SpawnPlayableCard(currentCard);
                                Vector3 newPositon = new(tempCard.gameObject.transform.localPosition.x + xOffset, tempCard.gameObject.transform.localPosition.y, tempCard.gameObject.transform.localPosition.z);
                                yield return new WaitForSeconds(1.0f);
                                yield return GenerateCard(tempCard, newPositon, screenshot, camera);
                            }

                            if (!styleSet.Keys.Contains(GetRepr(currentCard)))
                            {
                                styleSet[GetRepr(currentCard)] = GetImageEmbedded(GetRepr(currentCard));
                            }
                        }
                        // foreach (var cardBp in turn)
                        // {
                        //     int modCount = cardBp.card != null && cardBp.card.mods != null ? cardBp.card.mods.Count : 0;
                        //     string cardName = cardBp.card != null ? cardBp.card.name : "EMPTY";
                        //     P03Plugin.Log.LogDebug($"Checking cardBp [{cardBp.minDifficulty}, {cardBp.maxDifficulty}] card={cardName} with {modCount} mods, replacement={cardBp.replacement} ({cardBp.difficultyReplace})");

                        //     if (cardBp.minDifficulty > i || cardBp.maxDifficulty < i)
                        //         continue;

                        //     CardInfo currentCard = cardBp.card;
                        //     if (cardBp.difficultyReplace && cardBp.difficultyReq <= i)
                        //         currentCard = cardBp.replacement;

                        //     if (currentCard != null &&
                        //         encounter.turnMods != null &&
                        //         encounter.turnMods.Any(
                        //             tm => tm.applyAtDifficulty <= i &&
                        //                   tm.overlockCards &&
                        //                   tm.turn == turnNumber
                        //         ))
                        //     {
                        //         currentCard.mods ??= new();
                        //         currentCard.mods.Add(new (1, 0) { fromOverclock = true });
                        //     }

                        //     if (currentCard != null)
                        //     {
                        //         P03Plugin.Log.LogDebug($"Adding card {currentCard.name} to turn");
                        //         turnEncounterCards.Add(currentCard);

                        //         if (!Generated(currentCard))
                        //         {
                        //             PlayableCard tempCard = CardSpawner.SpawnPlayableCard(currentCard);
                        //             Vector3 newPositon = new Vector3(tempCard.gameObject.transform.localPosition.x + xOffset, tempCard.gameObject.transform.localPosition.y, tempCard.gameObject.transform.localPosition.z);
                        //             yield return new WaitForSeconds(1.0f);
                        //             yield return GenerateCard(tempCard, newPositon, screenshot, camera);
                        //         }

                        //         if (!styleSet.Keys.Contains(GetRepr(currentCard)))
                        //             styleSet[GetRepr(currentCard)] = GetImageEmbedded(GetRepr(currentCard));
                        //     }
                        // }

                        float turnPowerLevel = turnEncounterCards.Select(c => c.PowerLevel).Sum();
                        runningPowerLevelTotals[i] += turnPowerLevel;
                        //export += $"<td class=\"powerlevellabel\">{turnPowerLevel:#,0.00} ({runningPowerLevelTotals[i]:#,0.00})</td>";

                        // We kinda try to think about how we assign cards to slots; a little bit anyway
                        // I just want this to look visually appealing - the game's AI will reassign them during the game
                        List<string> turnSlots = new() { "queue", "queue", "queue", "queue", "queue" };
                        List<CardInfo> conduitsInTurn = turnEncounterCards.Where(ci => ci.HasConduitAbility()).ToList();
                        if (conduitsInTurn.Count == 1)
                        {
                            turnSlots[turnNumber % 2 == 0 ? 0 : 4] = GetRepr(conduitsInTurn[0]);
                            turnEncounterCards.Remove(conduitsInTurn[0]);
                        }
                        else if (conduitsInTurn.Count == 2)
                        {
                            turnSlots[0] = GetRepr(conduitsInTurn[0]);
                            turnSlots[4] = GetRepr(conduitsInTurn[1]);
                            turnEncounterCards.Remove(conduitsInTurn[0]);
                            turnEncounterCards.Remove(conduitsInTurn[1]);
                        }
                        if (turnEncounterCards.Count > 5)
                        {
                            throw new InvalidOperationException("Somehow this turn has too many cards!");
                        }

                        if (turnEncounterCards.Count is 4 or 5)
                        {
                            for (int s = 0; s < turnEncounterCards.Count; s++)
                            {
                                turnSlots[s] = GetRepr(turnEncounterCards[s]);
                            }
                        }
                        else if (turnEncounterCards.Count == 3)
                        {
                            turnSlots[1] = GetRepr(turnEncounterCards[0]);
                            turnSlots[2] = GetRepr(turnEncounterCards[1]);
                            turnSlots[3] = GetRepr(turnEncounterCards[2]);
                        }
                        else if (turnEncounterCards.Count == 2)
                        {
                            turnSlots[1] = GetRepr(turnEncounterCards[0]);
                            turnSlots[3] = GetRepr(turnEncounterCards[1]);
                        }
                        else if (turnEncounterCards.Count == 1)
                        {
                            turnSlots[2] = GetRepr(turnEncounterCards[0]);
                        }

                        // Generate the slot codes
                        foreach (string slotCode in turnSlots)
                        {
                            export += $"<td class=\"cardcell\"><div class=\"{slotCode}\"/></td>";
                        }
                        export += "<td><div class=\"levelspacer\"/></td>";
                    }
                    export += "</tr>";
                }

                export += "</table></body></html>";

                string filename = $"cardexports/encounter_{encounter.name}.html";

                string finalExport = $"<html><head><title>{encounter.name}</title>\n<style>\n";
                finalExport += "h2.title {\n";
                finalExport += "	font-family: Daggersquare, Consolas, \"Comic Sans\";\n";
                finalExport += "	font-size: 25pt;\n";
                finalExport += "	font-weight: bold;\n";
                finalExport += "}\n";
                finalExport += "td.levelheader {\n";
                finalExport += "	font-family: Daggersquare, Consolas, \"Comic Sans\";\n";
                finalExport += "	font-size: 16pt;\n";
                finalExport += "	text-align: center;\n";
                finalExport += "	background-color: #cccccc;\n";
                finalExport += "}\n";
                finalExport += "td.turnlabel {\n";
                finalExport += "	font-family: Daggersquare, Consolas, \"Comic Sans\";\n";
                finalExport += "	font-size: 14pt; \n";
                finalExport += "	white-space: nowrap;\n";
                finalExport += "	background-color: #cccccc;\n";
                finalExport += "}\n";
                finalExport += "td.powerlevellabel {\n";
                finalExport += "	font-family: Daggersquare, Consolas, \"Comic Sans\";\n";
                finalExport += "	font-size: 9pt; \n";
                finalExport += "	white-space: nowrap;\n";
                finalExport += "	background-color: #cccccc;\n";
                finalExport += "}\n";
                finalExport += "div.levelspacer {\n";
                finalExport += "	width: 45px;\n";
                finalExport += "}\n";
                finalExport += "td.cardcell {\n";
                finalExport += "	background-color: #eeeeee;\n";
                finalExport += "}\n\n";

                foreach (string styleDef in styleSet.Values)
                {
                    finalExport += styleDef;
                }

                finalExport += "</style>\n" + export;

                File.WriteAllText(filename, finalExport);
            }

            Destroy(screenshot);

            ExportAllSigils();
            ExportJsons();

            ExplorableAreaManager.Instance.SetHangingLightColors(originalHangingLightColor, originalHangingLightCardColor);

            GameOptions.optionsData.noiseEnabled = noiseEnabled;
            GameOptions.optionsData.flickeringDisabled = flickeringDisabled;
            GameOptions.optionsData.screenshakeDisabled = screenshakeDisabled;

            ViewManager.Instance.controller.LockState = ViewLockState.Unlocked;

            inRender = false;

            yield break;
        }

        [HarmonyPatch(typeof(CardSpawner), nameof(CardSpawner.SpawnPlayableCard))]
        [HarmonyPostfix]
        private static void EnsureOverclocked(ref PlayableCard __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (!inRender)
            {
                return;
            }

            if (__result.Info != null && __result.Info.Mods != null && __result.Info.Mods.Any(m => m.fromOverclock))
            {
                __result.Anim.SetOverclocked(true);
            }
        }
    }
}
