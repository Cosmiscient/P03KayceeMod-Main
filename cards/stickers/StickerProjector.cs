using System;
using System.IO;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    public class StickerProjector : MonoBehaviour
    {
        public Camera Camera;
        public Projector Projector;

        public Card AttachedCard => this.GetComponentInParent<Card>();

        private bool HasGeneratedClipTexture = false;
        private bool ReadyToGenerate = false;

        private static readonly Color ON = new(1f, 1f, 1f, 1f);
        private static readonly Color OFF = new(0f, 0f, 0f, 0f);

        void Start()
        {
            HasGeneratedClipTexture = false;
            Camera = this.GetComponent<Camera>();
            if (Camera != null)
                Camera.depth = -5;
            else
                this.enabled = false;
            Projector = this.GetComponent<Projector>();
        }

        private static Func<int, int, bool> GetCondition(Vector2Int a, Vector2Int b, Vector2Int o1)
        {
            // Special case: vertical line
            if (a.x == b.x)
            {
                int xt = a.x;
                if (o1.x < a.x)
                {
                    // P03Plugin.Log.LogDebug($"Generating condition: x <= {xt}");
                    return (x, y) => x <= xt;
                }
                else
                {
                    // P03Plugin.Log.LogDebug($"Generating condition: x >= {xt}");
                    return (x, y) => x >= xt;
                }
            }

            // Special case: horizontal line
            if (a.y == b.y)
            {
                int yt = a.y;
                if (o1.y < a.y)
                {
                    // P03Plugin.Log.LogDebug($"Generating condition: y <= {yt}");
                    return (x, y) => y <= yt;
                }
                else
                {
                    // P03Plugin.Log.LogDebug($"Generating condition: y >= {yt}");
                    return (x, y) => y >= yt;
                }
            }

            // Good ol y = mx + b
            float mm = ((float)(a.y - b.y)) / ((float)(a.x - b.x));
            float bb = ((float)(a.y)) - mm * (float)a.x;

            if (o1.y < (mm * (float)o1.x + bb))
            {
                // P03Plugin.Log.LogDebug($"Generating condition: y <= {mm} * x + {bb}");
                return (x, y) => y <= (mm * (float)x + bb);
            }
            else
            {
                // P03Plugin.Log.LogDebug($"Generating condition: y >= {mm} * x + {bb}");
                return (x, y) => y >= (mm * (float)x + bb);
            }
        }

        private static Func<int, int, bool> GetFillCondition(Vector2Int topLeft, Vector2Int topRight, Vector2Int bottomLeft, Vector2Int bottomRight)
        {
            // P03Plugin.Log.LogDebug($"Getting condition for ({topLeft.x}, {topLeft.y}) ({topRight.x}, {topRight.y}) ({bottomLeft.x}, {bottomLeft.y}) ({bottomRight.x}, {bottomRight.y})");

            var cond1 = GetCondition(topLeft, topRight, bottomRight);
            var cond2 = GetCondition(topRight, bottomRight, bottomLeft);
            var cond3 = GetCondition(bottomRight, bottomLeft, topLeft);
            var cond4 = GetCondition(bottomLeft, topLeft, topRight);

            return (x, y) => cond1(x, y) && cond2(x, y) && cond3(x, y) && cond4(x, y);
        }

        private static Func<int, int, bool> GetFillCondition(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, float xScale, float yScale)
        {
            Vector2Int tlInt = new(Mathf.RoundToInt(topLeft.x * xScale), Mathf.RoundToInt(topLeft.y * yScale));
            Vector2Int trInt = new(Mathf.RoundToInt(topRight.x * xScale), Mathf.RoundToInt(topRight.y * yScale));
            Vector2Int blInt = new(Mathf.RoundToInt(bottomLeft.x * xScale), Mathf.RoundToInt(bottomLeft.y * yScale));
            Vector2Int brInt = new(Mathf.RoundToInt(bottomRight.x * xScale), Mathf.RoundToInt(bottomRight.y * yScale));

            return GetFillCondition(tlInt, trInt, blInt, brInt);
        }

        void Update()
        {
            if (Camera == null || Projector == null)
            {
                this.enabled = false;
                HasGeneratedClipTexture = false;
                return;
            }

            if (HasGeneratedClipTexture || !Camera.enabled)
            {
                this.enabled = false;
                Camera.enabled = false;
                return;
            }

            if (AttachedCard == null)
            {
                HasGeneratedClipTexture = true;
                this.enabled = false;
                return;
            }

            if (!ReadyToGenerate)
            {
                // Okay, we need to create the clip texture
                // Start by making the camera and the projector have the same properties
                Camera.orthographic = Projector.orthographic;
                Camera.orthographicSize = Projector.orthographicSize;
                Camera.farClipPlane = 100;
                Camera.nearClipPlane = 0.01f;
                Camera.fieldOfView = Projector.fieldOfView;
                Camera.aspect = Projector.aspectRatio;

                // Set the camera to generate a depth texture
                Camera.depthTextureMode = DepthTextureMode.Depth;


                // Hope the rest of the settings match
                ReadyToGenerate = true;
                return;
            }

            if (ReadyToGenerate)
            {
                ReadyToGenerate = false;

                // Get the bounds of the card
                GameObject stLayer = AttachedCard.StatsLayer.gameObject;
                Vector3 topLeft = stLayer.transform.position + stLayer.transform.TransformVector(1f, -.7f, 0f);
                Vector3 topRight = stLayer.transform.position + stLayer.transform.TransformVector(1f, .7f, 0f);
                Vector3 bottomLeft = stLayer.transform.position + stLayer.transform.TransformVector(-1f, -.7f, 0f);
                Vector3 bottomRight = stLayer.transform.position + stLayer.transform.TransformVector(-1f, .7f, 0f);

                Vector3 topLeftUV = Camera.WorldToScreenPoint(topLeft, Camera.MonoOrStereoscopicEye.Mono);
                Vector3 topRightUV = Camera.WorldToScreenPoint(topRight, Camera.MonoOrStereoscopicEye.Mono);
                Vector3 bottomLeftUV = Camera.WorldToScreenPoint(bottomLeft, Camera.MonoOrStereoscopicEye.Mono);
                Vector3 bottomRightUV = Camera.WorldToScreenPoint(bottomRight, Camera.MonoOrStereoscopicEye.Mono);

                //Texture2D stickerTexture = Projector.material.GetTexture("_ShadowTex") as Texture2D;
                Texture2D clipTexture = new(Camera.pixelWidth / 10, Camera.pixelHeight / 10, TextureFormat.RGBA32, false);

                float xScale = (float)clipTexture.width / (float)Camera.pixelWidth;
                float yScale = (float)clipTexture.height / (float)Camera.pixelHeight;

                var fillCondition = GetFillCondition(topLeftUV, topRightUV, bottomLeftUV, bottomRightUV, xScale, yScale);

                for (int x = 0; x < clipTexture.width; x++)
                    for (int y = 0; y < clipTexture.height; y++)
                        clipTexture.SetPixel(x, y, fillCondition(x, y) ? ON : OFF);

                clipTexture.Apply();

                // Apply the clip texture to the material on the projector
                Projector.material.SetTexture("_ClipTex", clipTexture);

                //File.WriteAllBytes("_lastClipTexture.png", ImageConversion.EncodeToPNG(clipTexture));

                // And we're done
                HasGeneratedClipTexture = true;
                this.enabled = false;
            }
        }
    }
}