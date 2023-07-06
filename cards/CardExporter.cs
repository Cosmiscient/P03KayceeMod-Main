using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Encounters;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
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
            if (__instance.gameObject.GetComponent<CardExporter>() == null)
            {
                P03Plugin.Log.LogDebug("Adding Card Exporter!");
                __instance.gameObject.AddComponent<CardExporter>();
            }
        }

        public void StartCardExport()
        {
            inRender = true;
            base.StartCoroutine(ExportAllCards());
        }

        [SerializeField]        
        private GameObject temporaryHolding;

        [SerializeField]
        private PlayableCard dummyCard;

        private static RenderStatsLayer statsLayer = null;

        private bool IsTalkingCard(CardInfo info)
        {
            return info.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.DynamicPortrait) || info.animatedPortrait != null;
        }

        [SerializeField]
        public float xOffset = 0.4f;

        internal static readonly string[] GameObjectPaths = new string[]
        {
            "Anim/CardBase/Rails",
            "Anim/CardBase/Top",
            "Anim/CardBase/Bottom"
        };

        private bool inRender = false;
        private bool skipBulkRender = false;

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
            }
        }

        private static Bounds GetMaxBounds(GameObject g) {

            List<Renderer> renderers = new ();
            foreach (string p in GameObjectPaths)
            {
                Transform t = g.transform.Find(p);
                if (t != null)
                    renderers.AddRange(t.gameObject.GetComponents<Renderer>());
            }

            if (renderers.Count == 0) return new Bounds(g.transform.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (Renderer r in renderers) {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

        private static Dictionary<string, string> imageCache = new ();
        private static string GetImageEmbedded(string cardName)
        {
            if (imageCache.Keys.Contains(cardName))
                return imageCache[cardName];

            byte[] sourceBytes = cardName.Equals("queue", StringComparison.InvariantCultureIgnoreCase) ?
                                  TextureHelper.GetResourceBytes("cadslot_down.png", typeof(CardExporter).Assembly) :
                                  File.ReadAllBytes($"cardexports/{cardName}.png");
            string b64string = Convert.ToBase64String(sourceBytes);

            imageCache[cardName] = "div." + cardName + " {\n\tbackground-image: url(data:image/png;base64," + b64string + ");\n\twidth: 62px;\n\theight: 92px;\n\tbackground-size: 62px 92px;\n}\n";

            return imageCache[cardName];
        }

        private static System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]");
        private static string GetRepr(CardInfo info)
        {
            if (info.mods == null || info.mods.Count == 0)
                return info.name;
            return rgx.Replace(CustomCards.ConvertCardToCompleteCode(info), "");
        }

        private static bool Generated(CardInfo info)
        {
            return File.Exists($"cardexports/{GetRepr(info)}.png");
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
                screenshot.ReadPixels(new (0, 0, Screen.currentResolution.width, Screen.currentResolution.height), 0, 0, false);
                screenshot.Apply();

                Bounds cardBounds = GetMaxBounds(card.gameObject);
                Vector2 lower = camera.WorldToScreenPoint(cardBounds.min);
                Vector2 upper = camera.WorldToScreenPoint(cardBounds.max);
                int width = Mathf.RoundToInt(Mathf.Abs(lower.x - upper.x));
                int height = Mathf.RoundToInt(Mathf.Abs(lower.y - upper.y));
                int xMin = Mathf.RoundToInt(Mathf.Min(lower.x, upper.x));
                int yMin = Mathf.RoundToInt(Mathf.Min(lower.y, upper.y));

                finalTexture = new (width, height);
                finalTexture.filterMode = FilterMode.Trilinear;

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        finalTexture.SetPixel(x, y, screenshot.GetPixel(x + xMin, y + yMin));


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
                GameObject.DestroyImmediate(card.gameObject);
            if (finalTexture != null)
                GameObject.DestroyImmediate(finalTexture);
            yield return new WaitForEndOfFrame();
        }

        public IEnumerator ExportAllCards()
        {
            ViewManager.Instance.SwitchToView(View.MapDeckReview);
            yield return new WaitForSeconds(0.25f);
            Tween.LocalRotation(ViewManager.Instance.cameraParent, new Vector3(90f, 0f, 0f), 0f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
            ViewManager.Instance.controller.LockState = ViewLockState.Locked;

            Color originalHangingLightColor = ExplorableAreaManager.Instance.hangingLight.color;
            Color originalHangingLightCardColor = ExplorableAreaManager.Instance.hangingCardsLight.color;

            bool noiseEnabled = GameOptions.optionsData.noiseEnabled;
            GameOptions.optionsData.noiseEnabled = false;

            bool flickeringDisabled = GameOptions.optionsData.flickeringDisabled;
            GameOptions.optionsData.flickeringDisabled = true;

            bool screenshakeDisabled = GameOptions.optionsData.screenshakeDisabled;
            GameOptions.optionsData.screenshakeDisabled = true;

            //ExplorableAreaManager.Instance.SetHangingLightColors(GameColors.instance.brightSeafoam, GameColors.instance.brightSeafoam);

            yield return new WaitForSeconds(.15f);

            if (!Directory.Exists("cardexports"))
                Directory.CreateDirectory("cardexports");

            Camera camera = ViewManager.Instance.CameraParent.gameObject.GetComponentInChildren<Camera>();
            Vector3 renderPosition = new(0f,0f,0f);

            Texture2D screenshot = new Texture2D(Screen.currentResolution.width, Screen.currentResolution.height);
            screenshot.filterMode = FilterMode.Trilinear;

            List<CardInfo> cardsToRender = CardManager.AllCardsCopy.Where(ci => ci.temple == CardTemple.Tech && ci.name[0] != '!').ToList();
            if (skipBulkRender)
            {
                cardsToRender.Clear();
                skipBulkRender = false;
            }

            while (cardsToRender.Count > 0)
            {
                List<PlayableCard> currentBatch = new ();

                while (cardsToRender.Count > 0 && currentBatch.Count < 20)
                {
                    CardInfo info = cardsToRender[0];
                    cardsToRender.RemoveAt(0);
                    
                    PlayableCard card = CardSpawner.SpawnPlayableCard(info);
                    card.gameObject.transform.localPosition = new Vector3(card.gameObject.transform.localPosition.x + xOffset, card.gameObject.transform.localPosition.y, card.gameObject.transform.localPosition.z);
                    renderPosition = card.gameObject.transform.localPosition;
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

            // Now we export all of the encounters
            foreach (var encounter in EncounterManager.AllEncountersCopy.Where(ebd => Infiniscryption.P03KayceeRun.Encounters.EncounterExtensions.P03OnlyEncounters.Contains(ebd.name)))
            {
                P03Plugin.Log.LogDebug($"Generating encounter {encounter.name}");
                string export = $"<body><h2 class=\"title\">{encounter.name}</h2><table cellpadding=\"0\" cellspacing=\"0\"><tr><td/>";
                for (int i = 0; i <= 6; i++)
                    export += $"<td colspan=5 class=\"levelheader\">Level {i}</td><td><div class=\"levelspacer\"/></td>";
                export += "</tr>";

                Dictionary<string, string> styleSet = new() {
                    { "queue", GetImageEmbedded("queue") }
                };

                for (int turnNumber = 0; turnNumber < encounter.turns.Count; turnNumber++)
                {
                    P03Plugin.Log.LogDebug($"Generating turn {turnNumber+1}");
                    var turn = encounter.turns[turnNumber];
                    export += $"<tr><td class=\"turnlabel\">Turn {turnNumber+1}</td>";
                    for (int i = 0; i <= 6; i++)
                    {
                        P03Plugin.Log.LogDebug($"Generating difficulty {i}");
                        List<CardInfo> turnEncounterCards = new();
                        foreach (var cardBp in turn)
                        {
                            P03Plugin.Log.LogDebug($"Generating cardBp card={cardBp.card}, replacement={cardBp.replacement} ({cardBp.difficultyReplace})");

                            if (cardBp.minDifficulty > i || cardBp.maxDifficulty < i)
                                continue;

                            CardInfo currentCard = cardBp.card;
                            if (cardBp.difficultyReplace && cardBp.difficultyReq <= i)
                                currentCard = cardBp.replacement;

                            if (currentCard != null &&
                                encounter.turnMods != null &&
                                encounter.turnMods.Any(
                                    tm => tm.applyAtDifficulty <= i &&
                                          tm.overlockCards &&
                                          tm.turn == turnNumber
                                ))
                            {
                                currentCard.mods ??= new();
                                currentCard.mods.Add(new (1, 0) { fromOverclock = true });
                            }
                            
                            if (currentCard != null)
                            {
                                P03Plugin.Log.LogDebug($"Adding card {currentCard.name} to turn");
                                turnEncounterCards.Add(currentCard);

                                if (!styleSet.Keys.Contains(GetRepr(currentCard)))
                                    styleSet[GetRepr(currentCard)] = GetImageEmbedded(GetRepr(currentCard));

                                if (!Generated(currentCard))
                                {
                                    PlayableCard tempCard = CardSpawner.SpawnPlayableCard(currentCard);
                                    Vector3 newPositon = new Vector3(tempCard.gameObject.transform.localPosition.x + xOffset, tempCard.gameObject.transform.localPosition.y, tempCard.gameObject.transform.localPosition.z);
                                    yield return new WaitForSeconds(0.2f);
                                    yield return GenerateCard(tempCard, newPositon, screenshot, camera);
                                }
                            }
                        }

                        // We kinda try to think about how we assign cards to slots; a little bit anyway
                        // I just want this to look visually appealing - the game's AI will reassign them during the game
                        List<string> turnSlots = new() { "queue", "queue", "queue", "queue", "queue" };
                        List<CardInfo> conduitsInTurn = turnEncounterCards.Where(ci => ci.HasConduitAbility()).ToList();
                        if (conduitsInTurn.Count == 1)
                        {
                            turnSlots[turnNumber % 2 == 0 ? 0 : 4] = GetRepr(conduitsInTurn[0]);
                            turnEncounterCards.Remove(conduitsInTurn[0]);
                        } else if (conduitsInTurn.Count == 2)
                        {
                            turnSlots[0] = GetRepr(conduitsInTurn[0]);
                            turnSlots[4] = GetRepr(conduitsInTurn[1]);
                            turnEncounterCards.Remove(conduitsInTurn[0]);
                            turnEncounterCards.Remove(conduitsInTurn[1]);
                        }
                        if (turnEncounterCards.Count > 5)
                            throw new InvalidOperationException("Somehow this turn has too many cards!");
                        if (turnEncounterCards.Count == 4 || turnEncounterCards.Count == 5)
                        {
                            for (int s = 0; s < turnEncounterCards.Count; s++)
                                turnSlots[s] = GetRepr(turnEncounterCards[s]);
                        } else if (turnEncounterCards.Count == 3)
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
                finalExport += "div.levelspacer {\n";
                finalExport += "	width: 45px;\n";
                finalExport += "}\n";
                finalExport += "td.cardcell {\n";
                finalExport += "	background-color: #eeeeee;\n";
                finalExport += "}\n\n";

                foreach (string styleDef in styleSet.Values)
                    finalExport += styleDef;

                finalExport += "</style>\n" + export;

                File.WriteAllText(filename, finalExport);
            }

            GameObject.Destroy(screenshot);
            ExplorableAreaManager.Instance.SetHangingLightColors(originalHangingLightColor, originalHangingLightCardColor);

            GameOptions.optionsData.noiseEnabled = noiseEnabled;
            GameOptions.optionsData.flickeringDisabled = flickeringDisabled;
            GameOptions.optionsData.screenshakeDisabled = screenshakeDisabled;

            ViewManager.Instance.controller.LockState = ViewLockState.Unlocked;

            inRender = false;

            yield break;
        }
    }
}
