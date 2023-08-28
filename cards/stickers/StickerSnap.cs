// using System;
// using DiskCardGame;
// using InscryptionAPI.Helpers;
// using Pixelplacement;
// using UnityEngine;

// namespace Infiniscryption.P03KayceeRun.Cards.Stickers
// {
//     [RequireComponent(typeof(MeshCollider))]
//     [RequireComponent(typeof(Renderer))]
//     public class StickerSnap : MonoBehaviour
//     {
//         private Renderer renderer;
//         private Texture2D originalTex;
//         private Texture2D lastCroppedTex;
//         private Vector3 originalScale;
//         private Vector3 originalPos;
//         private float yAxisRotation = 0;

//         private float TWO_PI = 2f * Mathf.PI;
//         private float PI_OVER_2 = Mathf.PI / 2f;
//         private Color TRANSPARENT = new(1f, 1f, 1f, 0f);

//         private Card GetCard()
//         {
//             // must be attached to a render stats layer for this step to work
//             RenderStatsLayer layer = this.transform.parent.gameObject.GetComponent<RenderStatsLayer>();

//             if (layer == null)
//                 return null;

//             // If we're on a renderstatslayer, there's a card somewhere above us
//             return this.gameObject.GetComponentInParent<Card>();
//         }

//         protected void OnEnable()
//         {
//             renderer = gameObject.GetComponent<Renderer>();
//             originalTex = renderer.material.GetTexture("_MainTex") as Texture2D;
//             originalScale = gameObject.transform.localScale;
//         }

//         public void UnSnap(bool snapBack)
//         {
//             if (this.lastCroppedTex != null)
//                 GameObject.Destroy(this.lastCroppedTex);

//             this.renderer.material.SetTexture("_MainTex", this.originalTex);
//             this.renderer.material.SetTexture("_DetailAlbedoMap", this.originalTex);
//             this.renderer.material.SetTexture("_EmissionMap", this.originalTex);

//             if (snapBack)
//             {
//                 if (this.gameObject == Stickers.LastPrintedSticker)
//                 {
//                     Vector3 position = this.transform.position;
//                     this.transform.SetParent(Stickers.PrintedStickerParent);
//                     this.transform.localPosition = this.transform.parent.InverseTransformPoint(position);
//                     Tween.LocalPosition(this.transform, Stickers.STICKER_HOME_POSITION, 0.1f, 0f);
//                 }
//                 else
//                 {
//                     Tween.Position(this.transform, this.transform.position + new Vector3(0f, -2f, 0f), 0.2f, 0f, completeCallback: () => GameObject.Destroy(this.gameObject));
//                 }
//             }
//         }

//         protected Tuple<Vector2, Vector2> GetClipFromSimpleCase(float width, float height, float theta, float rightPad)
//         {
//             float x = theta == PI_OVER_2 ? width : rightPad / Mathf.Cos(theta);
//             float y = theta == 0 ? height : rightPad / Mathf.Sin(theta);

//             Vector2 from = new(width - x, height);
//             Vector2 to = new(width, height - y);

//             if (y >= height)
//             {
//                 try
//                 {
//                     float x_1 = theta == 0 ? x : (y - height) * Mathf.Tan(theta);
//                     to = new(width - x_1, 0);
//                 }
//                 catch
//                 {
//                     to = new(width - x, 0);
//                 }
//             }

//             if (x >= width)
//             {
//                 try
//                 {
//                     float y_1 = theta == PI_OVER_2 ? y : (x - width) * Mathf.Tan(PI_OVER_2 - theta);
//                     from = new(0, height - y_1);
//                 }
//                 catch
//                 {
//                     from = new(0, height - y);
//                 }
//             }

//             P03Plugin.Log.LogDebug($"Simple clip coordinates: ({from.x}, {from.y}); ({to.x}, {to.y})");
//             return new(from, to);
//         }

//         protected Tuple<Vector2, Vector2> GetClipFromRightPad(float width, float height, float theta, float rightPad)
//         {
//             while (theta > TWO_PI)
//                 theta -= TWO_PI;

//             while (theta < 0f)
//                 theta += TWO_PI;

//             // All of the math assumes a rotation < 90 degrees
//             if (theta <= PI_OVER_2)
//                 return GetClipFromSimpleCase(width, height, theta, rightPad);

//             // For a rotation between 90 degrees and 180 degrees
//             // We can kinda use all of the same math. However, we will give it
//             // an adjusted theta (180 - theta). And we swap width and height
//             // Then we'll rotate the result:
//             if (theta <= Mathf.PI)
//             {
//                 var rotResult = GetClipFromSimpleCase(height, width, theta - PI_OVER_2, rightPad);
//                 var from = rotResult.Item1;
//                 var to = rotResult.Item2;

//                 Tuple<Vector2, Vector2> retval = new(new(width - from.y, from.x), new(width - to.y, to.x));
//                 P03Plugin.Log.LogDebug($"90 degree rotation clip coordinates: ({retval.Item1.x}, {retval.Item1.y}); ({retval.Item2.x}, {retval.Item2.y})");
//                 return retval;
//             }

//             // For a rotation above 180, let's just pretend the whole thing is flipped!
//             // We just get the result for 180 - theta, then invert the x and y coordinates:
//             var flipResult = GetClipFromRightPad(width, height, theta - Mathf.PI, rightPad);
//             var flipFrom = flipResult.Item1;
//             var flipTo = flipResult.Item2;
//             Tuple<Vector2, Vector2> retvalFlip = new(new(width - flipFrom.x, height - flipFrom.y), new(width - flipTo.x, height - flipTo.y));
//             P03Plugin.Log.LogDebug($"180 degree rotation clip coordinates: ({retvalFlip.Item1.x}, {retvalFlip.Item1.y}); ({retvalFlip.Item2.x}, {retvalFlip.Item2.y})");
//             return retvalFlip;
//         }

//         protected Tuple<Vector2, Vector2> GetClipFromTopPad(float width, float height, float theta, float topPad)
//         {
//             // When we get a clip from the top pad, all we need to do is rotate the frame of reference
//             // such that top becomes right.

//             // So let's start by getting the right pad answer and then just rotate it
//             var rotResult = GetClipFromRightPad(height, width, theta, topPad);
//             var from = rotResult.Item1;
//             var to = rotResult.Item2;
//             Tuple<Vector2, Vector2> retval = new(new(width - from.y, from.x), new(width - to.y, to.x));
//             P03Plugin.Log.LogDebug($"Rotation back to top pad coordinates: ({retval.Item1.x}, {retval.Item1.y}); ({retval.Item2.x}, {retval.Item2.y})");
//             return retval;
//         }

//         protected Func<int, int, bool> GetFilterFromClipPoints(Tuple<Vector2, Vector2> padPoints, bool keepAbove)
//         {
//             // First, we need to convert from world units to texture units
//             int x_1 = Mathf.RoundToInt((padPoints.Item1.x / this.originalScale.x) * this.originalTex.width);
//             int x_2 = Mathf.RoundToInt((padPoints.Item2.x / this.originalScale.x) * this.originalTex.width);
//             int y_1 = Mathf.RoundToInt((padPoints.Item1.y / this.originalScale.y) * this.originalTex.height);
//             int y_2 = Mathf.RoundToInt((padPoints.Item2.y / this.originalScale.y) * this.originalTex.height);

//             // In the very special case of a vertical line, the ambiguous meaning of "keep above"
//             // is assumed to mean "keep everything to the right"
//             if (x_1 == x_2)
//             {
//                 if (keepAbove)
//                 {
//                     P03Plugin.Log.LogDebug($"Texture Clipper: x >= {x_1}");
//                     return (x, y) => x >= x_1;
//                 }
//                 else
//                 {
//                     P03Plugin.Log.LogDebug($"Texture Clipper: x <= {x_1}");
//                     return (x, y) => x <= x_1;
//                 }
//             }

//             if (padPoints.Item1.y == padPoints.Item2.y)
//             {
//                 if (keepAbove)
//                 {
//                     P03Plugin.Log.LogDebug($"Texture Clipper: y >= {y_1}");
//                     return (x, y) => y >= y_1;
//                 }
//                 else
//                 {
//                     P03Plugin.Log.LogDebug($"Texture Clipper: y <= {y_1}");
//                     return (x, y) => y <= y_1;
//                 }
//             }

//             // good ol' y = mx + b
//             float m = (float)(y_1 - y_2) / (float)(x_1 - x_2);
//             float b = (float)y_1 - m * (float)x_1;

//             if (keepAbove)
//             {
//                 P03Plugin.Log.LogDebug($"Texture Clipper: y >= {m:0.00}x + {b:0.00}");
//                 return (x, y) => y >= m * (float)x + b;
//             }
//             else
//             {
//                 P03Plugin.Log.LogDebug($"Texture Clipper: y <= {m:0.00}x + {b:0.00}");
//                 return (x, y) => y <= m * (float)x + b;
//             }
//         }

//         protected Func<int, int, bool> MergeFilters(Func<int, int, bool> prev, Func<int, int, bool> add)
//         {
//             if (prev == null)
//                 return add;

//             return (x, y) => prev(x, y) && add(x, y);
//         }

//         protected Texture2D GetClippedTexture(float leftPad, float rightPad, float topPad, float bottomPad)
//         {
//             // Start by copying the old texture into a new texture
//             Texture2D retval = TextureHelper.DuplicateTexture(this.originalTex);

//             // Now we want to start selectively eliminating pixels from the texture
//             Func<int, int, bool> filters = null;
//             if (rightPad > 0)
//             {
//                 var padPoints = GetClipFromRightPad(this.originalScale.x, this.originalScale.y, this.yAxisRotation, rightPad);
//                 P03Plugin.Log.LogDebug($"Right Pad [{rightPad}] from ({padPoints.Item1.x}, {padPoints.Item1.y}) to ({padPoints.Item2.x}, {padPoints.Item2.y})");
//                 filters = MergeFilters(filters, GetFilterFromClipPoints(padPoints, false));
//             }

//             if (topPad > 0)
//             {
//                 var padPoints = GetClipFromTopPad(this.originalScale.x, this.originalScale.y, this.yAxisRotation, topPad);
//                 P03Plugin.Log.LogDebug($"Top Pad [{topPad}] from ({padPoints.Item1.x}, {padPoints.Item1.y}) to ({padPoints.Item2.x}, {padPoints.Item2.y})");
//                 filters = MergeFilters(filters, GetFilterFromClipPoints(padPoints, false));
//             }

//             if (bottomPad > 0)
//             {
//                 float topPadEquivalent = this.renderer.bounds.size.z - bottomPad;
//                 var padPoints = GetClipFromTopPad(this.originalScale.x, this.originalScale.y, this.yAxisRotation, topPadEquivalent);
//                 P03Plugin.Log.LogDebug($"Bottom Pad [{bottomPad}] -> equiv[{topPadEquivalent}] from ({padPoints.Item1.x}, {padPoints.Item1.y}) to ({padPoints.Item2.x}, {padPoints.Item2.y})");
//                 filters = MergeFilters(filters, GetFilterFromClipPoints(padPoints, true));
//             }

//             if (leftPad > 0)
//             {
//                 float rightPadEquivalent = this.renderer.bounds.size.x - leftPad;
//                 var padPoints = GetClipFromRightPad(this.originalScale.x, this.originalScale.y, this.yAxisRotation, rightPadEquivalent);
//                 P03Plugin.Log.LogDebug($"Left Pad [{leftPad}] -> equiv[{rightPadEquivalent}] from ({padPoints.Item1.x}, {padPoints.Item1.y}) to ({padPoints.Item2.x}, {padPoints.Item2.y})");
//                 filters = MergeFilters(filters, GetFilterFromClipPoints(padPoints, true));
//             }

//             // And now apply the filters
//             if (filters != null)
//             {
//                 for (int x = 0; x < retval.width; x++)
//                     for (int y = 0; y < retval.height; y++)
//                         if (!filters(x, y))
//                             retval.SetPixel(x, y, TRANSPARENT);

//                 retval.Apply();
//             }

//             return retval;
//         }

//         public void Reshader(string shaderName)
//         {
//             this.gameObject.GetComponent<Projector>().material.shader = Shader.Find(shaderName);
//         }

//         public void Snap()
//         {
//             Card attachedCard = GetCard();
//             if (attachedCard != null)
//             {
//                 Bounds cardBounds = CardExporter.GetMaxBounds(attachedCard.gameObject);
//                 Bounds stickerBounds = renderer.bounds;

//                 // P03Plugin.Log.LogDebug($"Card Bounds X {cardBounds.min.x} {cardBounds.max.x} Y {cardBounds.min.y} {cardBounds.max.y} Z {cardBounds.min.z} {cardBounds.max.z}");
//                 // P03Plugin.Log.LogDebug($"Sticker Bounds X {stickerBounds.min.x} {stickerBounds.max.x} Y {stickerBounds.min.y} {stickerBounds.max.y} Z {stickerBounds.min.z} {stickerBounds.max.z}");

//                 float leftPad = Mathf.Max(cardBounds.min.x - stickerBounds.min.x, 0f);
//                 float rightPad = Mathf.Max(stickerBounds.max.x - cardBounds.max.x, 0f);

//                 float bottomPad = Mathf.Max(cardBounds.min.z - stickerBounds.min.z, 0f);
//                 float topPad = Mathf.Max(stickerBounds.max.z - cardBounds.max.z, 0f);

//                 // int leftPadInt = Mathf.RoundToInt(this.originalTex.width * (leftPad / (stickerBounds.size.x)));
//                 // int rightPadInt = Mathf.RoundToInt(this.originalTex.width * (rightPad / (stickerBounds.size.x)));

//                 // int bottomPadInt = Mathf.RoundToInt(this.originalTex.height * (bottomPad / (stickerBounds.size.z)));
//                 // int topPadInt = Mathf.RoundToInt(this.originalTex.height * (topPad / (stickerBounds.size.z)));

//                 // P03Plugin.Log.LogDebug($"Calculated texture trim {leftPadInt} {rightPadInt} {bottomPadInt} {topPadInt}");

//                 this.lastCroppedTex = GetClippedTexture(leftPad, rightPad, topPad, bottomPad);

//                 // int width = originalTex.width - leftPadInt - rightPadInt;
//                 // int height = originalTex.height - bottomPadInt - topPadInt;

//                 // this.lastCroppedTex = new(width, height, TextureFormat.RGBA32, false);
//                 // this.lastCroppedTex.SetPixels(this.originalTex.GetPixels(leftPadInt, bottomPadInt, width, height));
//                 // this.lastCroppedTex.Apply();

//                 this.renderer.material.SetTexture("_MainTex", this.lastCroppedTex);
//                 this.renderer.material.SetTexture("_DetailAlbedoMap", this.lastCroppedTex);
//                 this.renderer.material.SetTexture("_EmissionMap", this.lastCroppedTex);

//                 // float widthFactor = (float)width / (float)originalTex.width;
//                 // float heightFactor = (float)height / (float)originalTex.height;

//                 // this.originalPos = this.transform.localPosition;
//                 // this.transform.localPosition = this.transform.parent.InverseTransformPoint(new(this.transform.position.x + ((leftPad - rightPad) * 0.5f), this.transform.position.y, this.transform.position.z + ((bottomPad - topPad) * 0.5f)));
//                 // this.transform.localScale = new(this.originalScale.x * widthFactor, this.originalScale.y * heightFactor, this.originalScale.z);
//             }
//             else
//             {
//                 this.UnSnap(true);
//             }
//         }

//         protected void OnDestroy()
//         {
//             if (this.lastCroppedTex != null)
//             {
//                 GameObject.Destroy(this.lastCroppedTex);
//             }
//         }
//     }
// }