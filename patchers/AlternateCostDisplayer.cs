using HarmonyLib;
using DiskCardGame;
using UnityEngine;
using InscryptionAPI.Saves;
using System.Text;
using System.IO;
using System.IO.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Quests;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Helpers;
using TMPro;
using UnityEngine.PostProcessing;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class AlternateCostDisplayer
    {
        // public const string ENERGY_LIGHTS = "Anim/CardBase/Top/EnergyCostLights";
        // public const string UPPER_GEM_CONTAINER = "Anim/CardBase/Top/Gems";
        // public const string BLUE_GEM_ORIGINAL = "Anim/CardBase/Top/Gems/Gem_Blue";
        // public const string ORANGE_GEM_ORIGINAL = "Anim/CardBase/Bottom/Gems/Gem_Orange";
        // public const string GREEN_GEM_ORIGINAL = "Anim/CardBase/Bottom/Gems/Gem_Green";
        // public const string BLUE_GEM_COST = "Anim/CardBase/Top/Gems/Gem_Cost_Blue";
        // public const string ORANGE_GEM_COST = "Anim/CardBase/Top/Gems/Gem_Cost_Orange";
        // public const string GREEN_GEM_COST = "Anim/CardBase/Top/Gems/Gem_Cost_Green";
        // public const string BLOOD_COST = "Anim/CardBase/Top/Gems/Blood_Cost";
        // public const string BONES_COST = "Anim/CardBase/Top/Gems/Bones_Cost";

        // public static Color BoneColor => Color.white * 0.75f;
        // public static Color BloodColor => GameColors.Instance.glowRed;
        
        // public static GameObject GetPiece(this Card card, string key)
        // {
        //     Transform t = card.gameObject.transform.Find(key);
        //     return t == null ? null : t.gameObject;
        // }

        // private static ConditionalWeakTable<RenderStatsLayer, Card> CardRenderReverseLookup = new ();

        // public static Card GetCard(this RenderStatsLayer layer)
        // {
        //     Card retval;
        //     if (CardRenderReverseLookup.TryGetValue(layer, out retval))
        //         return retval;

        //     retval = layer.gameObject.GetComponentInParent<Card>();
        //     CardRenderReverseLookup.Add(layer, retval);
        //     return retval;
        // }

        // private static Texture2D _bgTexture = null;
        // private static Texture2D BackgroundTexture
        // {
        //     get
        //     {
        //         if (_bgTexture != null)
        //             return _bgTexture;

        //         _bgTexture = TextureHelper.GetImageAsTexture($"CostTextureBackground.png", typeof(AlternateCostDisplayer).Assembly);
        //         return _bgTexture;
        //     }
        // }

        // private static List<Texture2D> _sevenSegments = null;
        // private static List<Texture2D> SevenSegmentDisplay
        // {
        //     get
        //     {
        //         if (_sevenSegments != null)
        //             return _sevenSegments;

        //         _sevenSegments = new ();

        //         for (int i = 0; i <= 9; i++)
        //         {
        //             Texture2D tex = TextureHelper.GetImageAsTexture($"Display_{i}_small.png", typeof(AlternateCostDisplayer).Assembly);
        //             tex.name = $"Display_{i}";
        //             _sevenSegments.Add(tex);
        //         }

        //         Texture2D texx = TextureHelper.GetImageAsTexture($"Display_x_small.png", typeof(AlternateCostDisplayer).Assembly);
        //         texx.name = $"Display_x";
        //         _sevenSegments.Add(texx);

        //         return _sevenSegments;
        //     }
        // }

        // private static Texture2D _boneIcon = null;
        // private static Texture2D BoneCostIcon
        // {
        //     get
        //     {
        //         if (_boneIcon != null)
        //             return _boneIcon;

        //         _boneIcon = TextureHelper.GetImageAsTexture("BoneCostIcon_small.png", typeof(AlternateCostDisplayer).Assembly);
        //         return _boneIcon;                
        //     }
        // }

        // private static Texture2D _bloodIcon = null;
        // private static Texture2D BloodCostIcon
        // {
        //     get
        //     {
        //         if (_bloodIcon != null)
        //             return _bloodIcon;

        //         _bloodIcon = TextureHelper.GetImageAsTexture("BloodCostIcon_small.png", typeof(AlternateCostDisplayer).Assembly);
        //         return _bloodIcon;                
        //     }
        // }

        // private static Dictionary<int, Texture2D> BonesCostTextures = new ();
        // private static Dictionary<int, Texture2D> BonesCostEmissions = new ();
        // private static Dictionary<int, Texture2D> BloodCostTextures = new ();
        // private static Dictionary<int, Texture2D> BloodCostEmissions = new ();



        // // public static GameObject GetBloodCostContainer(this Card card, bool force = false)
        // // {
        // //     GameObject retval = card.StatsLayer.GetBloodCostContainer();

        // //     if (retval != null || !force)
        // //         return retval;

        // //     // Generate 5 blood
        // //     Transform parentTransform = card.GetPiece(UPPER_GEM_CONTAINER).transform;
        // //     for (int i = 0; i < 5; i++)
        // //     {
        // //         GameObject bloodToken = GameObject.CreatePrimitive(PrimitiveType.Quad);
        // //         bloodToken.name = $"Blood_Cost_{i+1}";
        // //         bloodToken.transform.SetParent(parentTransform);
        // //         bloodToken.GetComponent<Renderer>().material = MaterialHelper.GetBakedEmissiveMaterial(BloodCostIcon);
        // //         //MaterialHelper.RecolorAllMaterials(bloodToken, GameColors.Instance.glowRed * 1.5f, emissive: false, makeTransparent: true);
        // //         bloodToken.transform.localScale = new(0.06f, 0.08f, 0.08f);
        // //         bloodToken.transform.localPosition = new(.06f + .09f * i, 0.0565f, 0f);
        // //         bloodToken.transform.localEulerAngles = new(90f, 245f, 55f);
                
        // //         retval.Add(bloodToken);
        // //     }

        // //     // Make the three segment displays
        // //     for (int i = 0; i < 3; i++)
        // //     {
        // //         GameObject displaySymbol = GameObject.CreatePrimitive(PrimitiveType.Quad);
        // //         displaySymbol.name = $"Blood_Digit_Display_{i+1}";
        // //         displaySymbol.transform.SetParent(parentTransform);
        // //         displaySymbol.GetComponent<Renderer>().material = MaterialHelper.GetBakedEmissiveMaterial(SevenSegmentDisplay[0]);
        // //         //MaterialHelper.RecolorAllMaterials(displaySymbol, GameColors.Instance.glowRed * 1.5f, emissive: false, makeTransparent: true);
        // //         displaySymbol.transform.localScale = new(0.08f, 0.08f, 0.08f);
        // //         displaySymbol.transform.localPosition = new(.09f + .105f * i, 0.07f, 0f);
        // //         displaySymbol.transform.localEulerAngles = new(80f, 245f, 65f);

        // //         retval.Add(displaySymbol);
        // //     }

        // //     return retval;
        // // }

        // // private static ConditionalWeakTable<RenderStatsLayer, List<GameObject>> BonesContainerLookup = new ();
        // // public static List<GameObject> GetBonesCostContainer(this RenderStatsLayer layer) => BonesContainerLookup.GetOrCreateValue(layer);

        // // public static List<GameObject> GetBonesCostContainer(this Card card, bool force = false)
        // // {
        // //     List<GameObject> retval = card.StatsLayer.GetBonesCostContainer();

        // //     if (retval.Count > 0 || !force)
        // //         return retval;

        // //     // Generate 4 bones
        // //     Transform parentTransform = card.GetPiece(UPPER_GEM_CONTAINER).transform;
        // //     for (int i = 0; i < 4; i++)
        // //     {
        // //         GameObject boneToken = GameObject.CreatePrimitive(PrimitiveType.Quad);
        // //         boneToken.name = $"Bone_Cost_{i+1}";
        // //         boneToken.transform.SetParent(parentTransform);
        // //         boneToken.GetComponent<Renderer>().material = MaterialHelper.GetBakedEmissiveMaterial(BoneCostIcon);
        // //         //MaterialHelper.RecolorAllMaterials(boneToken, Color.white * 1.5f, emissive: false, makeTransparent: true);
        // //         boneToken.transform.localScale = new(0.08f, 0.08f, 0.08f);
        // //         boneToken.transform.localPosition = new(.09f + .105f * i, 0.0565f, 0f);
        // //         boneToken.transform.localEulerAngles = new(90f, 245f, 55f);
                
        // //         retval.Add(boneToken);
        // //     }

        // //     // Make the three segment displays
        // //     for (int i = 0; i < 3; i++)
        // //     {
        // //         GameObject displaySymbol = GameObject.CreatePrimitive(PrimitiveType.Quad);
        // //         displaySymbol.name = $"Bone_Digit_Display_{i+1}";
        // //         displaySymbol.transform.SetParent(parentTransform);
        // //         displaySymbol.GetComponent<Renderer>().material = MaterialHelper.GetBakedEmissiveMaterial(SevenSegmentDisplay[0]);
        // //         //MaterialHelper.RecolorAllMaterials(displaySymbol, Color.white * 1.5f, emissive: false, makeTransparent: true);
        // //         displaySymbol.transform.localScale = new(0.08f, 0.08f, 0.08f);
        // //         displaySymbol.transform.localPosition = new(.09f + .105f * i, 0.07f, 0f);
        // //         displaySymbol.transform.localEulerAngles = new(80f, 245f, 65f);

        // //         retval.Add(displaySymbol);
        // //     }

        // //     return retval;
        // // }

        // private static GameObject MakeCostContainer(Card card, string path, Color lightColor)
        // {
        //     GameObject container = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //     container.name = path.Split('/').Last();
        //     container.transform.SetParent(card.GetPiece(UPPER_GEM_CONTAINER).transform);

        //     GameObject.Destroy(container.GetComponent<MeshCollider>());

        //     Renderer renderer = container.GetComponent<Renderer>();
        //     renderer.material.EnableKeyword("_EMISSION");
        //     renderer.material.SetTexture("_MainTex", BackgroundTexture);
        //     renderer.material.SetTexture("_EmissionMap", BackgroundTexture);
        //     // renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        //     // renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        //     // renderer.material = MaterialHelper.GetBakedEmissiveMaterial(BackgroundTexture);

        //     container.transform.localPosition = new(.25f, .0565f, 0f);
        //     container.transform.localScale = new (0.43f, 0.12f, 0.08f);
        //     container.transform.localEulerAngles = new (90f, 245f, 65f);

        //     container.layer = 3;

        //     // // Now we add a light
        //     // GameObject lightObject = new($"{container.name}_Light");
        //     // lightObject.transform.SetParent(container.transform.parent);
        //     // lightObject.transform.localPosition = new (0.25f, 3.0f, 0f);
        //     // lightObject.transform.localEulerAngles = new (90f, 180f, 0f);


        //     // Light light = lightObject.AddComponent<Light>();
        //     // light.cullingMask = 1 << 3;
            
        //     // Light referenceLight = ExplorableAreaManager.Instance.handLight;
        //     // light.color = lightColor;
        //     // light.colorTemperature = referenceLight.colorTemperature;
        //     // light.useColorTemperature = referenceLight.useColorTemperature;
        //     // light.intensity = referenceLight.intensity;
        //     // light.range = 4f;
        //     // light.type = LightType.Spot;

        //     return container;
        // }

        // public static void DisplayBonesCost(this Card card, bool hide = false)
        // {
        //     GameObject bonesDisplayer = card.GetPiece(BONES_COST) ?? MakeCostContainer(card, BONES_COST, BoneColor);

        //     if (hide || card.Info.BonesCost == 0)
        //     {
        //         bonesDisplayer.SetActive(false);
        //         return;
        //     }

        //     bonesDisplayer.SetActive(true);

        //     Renderer renderer = bonesDisplayer.GetComponent<Renderer>();

        //     if (BonesCostTextures.ContainsKey(card.Info.BonesCost))
        //     {
        //         renderer.material.SetTexture("_MainTex", BonesCostTextures[card.Info.BonesCost]);
        //         renderer.material.SetTexture("_EmissionMap", BonesCostEmissions[card.Info.BonesCost]);
        //         return;
        //     }

        //     Texture2D newTexture = MaterialHelper.DuplicateTexture(BackgroundTexture);
        //     newTexture.name = $"BoneCost_{card.Info.BonesCost}";

        //     Texture2D newEmission = new(newTexture.width, newTexture.height, newTexture.format, false);
        //     newEmission.name = $"BoneCost_{card.Info.BonesCost}_emission";

        //     Color[] emptyFill = new Color[newEmission.width * newEmission.height];
        //     Color empty = new(0f, 0f, 0f, 0f);
        //     for (int i = 0; i < emptyFill.Length; i++)
        //         emptyFill[i] = empty;

        //     newEmission.SetPixels(emptyFill);

        //     List<Texture2D> texturesRightToLeft = new();
        //     if (card.Info.BonesCost <= 4)
        //     {
        //         for (int i = 0; i < card.Info.BonesCost; i++)
        //             texturesRightToLeft.Add(BoneCostIcon);
        //         while(texturesRightToLeft.Count < 4)
        //             texturesRightToLeft.Add(null);
        //     }
        //     else
        //     {
        //         texturesRightToLeft.Add(SevenSegmentDisplay[card.Info.BonesCost % 10]);
        //         if (card.Info.BonesCost < 10)
        //         {
        //             texturesRightToLeft.Add(SevenSegmentDisplay[10]);
        //             texturesRightToLeft.Add(BoneCostIcon);
        //             texturesRightToLeft.Add(null);
        //         }
        //         else
        //         {
        //             int tensDigit = Mathf.FloorToInt(card.Info.BonesCost / 10f) % 10;
        //             texturesRightToLeft.Add(SevenSegmentDisplay[tensDigit]);
        //             texturesRightToLeft.Add(SevenSegmentDisplay[10]);
        //             texturesRightToLeft.Add(BoneCostIcon);
        //         }
        //     }
            
        //     int leftWidthPad = 0;
        //     for (int d = 0; d < texturesRightToLeft.Count; d++)
        //     {
        //         if (texturesRightToLeft[d] == null)
        //             continue;

        //         int ty = Mathf.FloorToInt((newTexture.height - texturesRightToLeft[d].height) / 2f);
        //         leftWidthPad += texturesRightToLeft[d].width;
        //         int tx = newTexture.width - 13 - leftWidthPad - d * 28;
        //         for (int x = 0; x < texturesRightToLeft[d].width; x++)
        //         {
        //             for (int y = 0; y < texturesRightToLeft[d].height; y++)
        //             {
        //                 Color bc = texturesRightToLeft[d].GetPixel(x, y);
        //                 if (bc.a > 0f)
        //                 {
        //                     Color t = bc.a == 1f ? BoneColor : new(BoneColor.r, BoneColor.g, BoneColor.b, bc.a);                            
        //                     newTexture.SetPixel(tx + x, ty + y, t);
        //                     newEmission.SetPixel(tx + x, ty + y, t);
        //                 }
        //             }
        //         }
        //     }

        //     newTexture.Apply();
        //     newEmission.Apply();

        //     newTexture.filterMode = FilterMode.Point;
        //     newEmission.filterMode = FilterMode.Point;
        //     BonesCostTextures[card.Info.BonesCost] = newTexture;
        //     BonesCostEmissions[card.Info.BonesCost] = newEmission;
        //     //File.WriteAllBytes($"cardexports/{newTexture.name}.png", ImageConversion.EncodeToPNG(newTexture));

        //     renderer.material.SetTexture("_MainTex", newTexture);
        //     renderer.material.SetTexture("_EmissionMap", newEmission);
        // }

        // public static void DisplayBloodCost(this Card card, bool hide = false)
        // {
        //     // List<GameObject> display = card.GetBloodCostContainer();

        //     // if (display.Count == 0)
        //     //     return;

        //     // if (hide)
        //     // {
        //     //     for (int i = 0; i < display.Count; i++)
        //     //         display[i].SetActive(false);
                
        //     //     return;
        //     // }

        //     // if (card.Info.BloodCost <= 5)
        //     // {
        //     //     for (int i = 0; i < display.Count; i++)
        //     //         display[i].SetActive(card.Info.BloodCost >= (i + 1));

        //     //     return;
        //     // }

        //     // // Blood Icons
        //     // display[0].SetActive(false);
        //     // display[1].SetActive(false);
        //     // display[2].SetActive(false);
        //     // display[3].SetActive(true);
        //     // display[4].SetActive(false);

        //     // // Seven segments
        //     // display[5].SetActive(true);
        //     // display[6].SetActive(true);
        //     // display[7].SetActive(card.Info.BloodCost >= 10);

        //     // int onesDigit = card.Info.BloodCost % 10;
        //     // MaterialHelper.RetextureAllRenderers(display[5], SevenSegmentDisplay[onesDigit]);

        //     // if (card.Info.BloodCost < 10)
        //     // {
        //     //     MaterialHelper.RetextureAllRenderers(display[6], SevenSegmentDisplay[10]);
        //     //     return;
        //     // }

        //     // int tensDigit = Mathf.FloorToInt(card.Info.BloodCost / 10f) % 10;
        //     // MaterialHelper.RetextureAllRenderers(display[6], SevenSegmentDisplay[tensDigit]);
        //     // MaterialHelper.RetextureAllRenderers(display[7], SevenSegmentDisplay[10]);
        // }

        // private static ConditionalWeakTable<RenderStatsLayer, Dictionary<GemType, Renderer>> GemContainerLookup = new ();
        // public static Dictionary<GemType, Renderer> GetGemCostContainer(this RenderStatsLayer layer) => GemContainerLookup.GetOrCreateValue(layer);

        // public static Dictionary<GemType, Renderer> GetGemCostContainer(this Card card, bool force = false)
        // {
        //     Dictionary<GemType, Renderer> retval = card.StatsLayer.GetGemCostContainer();

        //     if (retval.ContainsKey(GemType.Blue))
        //         return retval;

        //     // First, see if it needs to be created
        //     GameObject blueCost = card.GetPiece(BLUE_GEM_COST);
        //     if (blueCost == null && force)
        //     {
        //         // Start by making the backup lookup
        //         CardRenderReverseLookup.Add(card.StatsLayer, card);

        //         // Make each gem
        //         GameObject gemContainer = card.GetPiece(UPPER_GEM_CONTAINER);
        //         blueCost = GameObject.Instantiate(card.GetPiece(BLUE_GEM_ORIGINAL), gemContainer.transform);
        //         blueCost.name = BLUE_GEM_COST.Split('/').Last();
        //         blueCost.transform.localScale = new (100f, 100f, 100f);
        //         blueCost.transform.localPosition = new (0.1f, 0.02f, 0f);
        //         retval[GemType.Blue] = blueCost.GetComponent<Renderer>();

        //         GameObject greenCost = GameObject.Instantiate(card.GetPiece(GREEN_GEM_ORIGINAL), gemContainer.transform);
        //         greenCost.name = GREEN_GEM_COST.Split('/').Last();
        //         greenCost.transform.localScale = new (90f, 90f, 25);
        //         greenCost.transform.localEulerAngles = new (270f, 90f, 0f);
        //         greenCost.transform.localPosition = new (0.38f, 0.0535f, 0f);
        //         retval[GemType.Green] = greenCost.GetComponent<Renderer>();

        //         GameObject orangeCost = GameObject.Instantiate(card.GetPiece(ORANGE_GEM_ORIGINAL), gemContainer.transform);
        //         orangeCost.name = ORANGE_GEM_COST.Split('/').Last();
        //         orangeCost.transform.localScale = new (100f, 100f, 25);
        //         orangeCost.transform.localPosition = new (0.25f, 0.053f, 0f);
        //         retval[GemType.Orange] = orangeCost.GetComponent<Renderer>();
        //     }
        //     else
        //     {
        //         retval[GemType.Blue] = blueCost == null ? null : blueCost.GetComponent<Renderer>();
        //         retval[GemType.Green] = card.GetPiece(GREEN_GEM_COST) == null ? null : card.GetPiece(GREEN_GEM_COST).GetComponent<Renderer>();
        //         retval[GemType.Orange] = card.GetPiece(ORANGE_GEM_COST) == null ? null : card.GetPiece(ORANGE_GEM_COST).GetComponent<Renderer>();
        //     }

        //     return retval;
        // }

        // public static void SetGemCostActive(this Card card, bool active = true)
        // {
        //     var gemContainer = card.GetGemCostContainer();
        //     foreach (var renderer in gemContainer.Values.Where(v => v != null))
        //         renderer.gameObject.SetActive(active);
        // }

        // [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        // [HarmonyPostfix]
        // private static void DisplayAlternateCostSingle(Card __instance)
        // {
        //     // This will render alternate costs to the cost window.
        //     // Note that right now this only support single alternate costs, not combined costs
        //     if (!(__instance.StatsLayer is DiskRenderStatsLayer drsl))
        //         return;

        //     if (__instance.Info.EnergyCost > 0)
        //     {
        //         __instance.DisplayBloodCost(hide: true);
        //         __instance.DisplayBonesCost(hide: true);
        //         __instance.SetGemCostActive(false);
        //         return;
        //     }

        //     if (__instance.Info.GemsCost.Count > 0)
        //     {
        //         // Turn off the energy lights
        //         __instance.GetPiece(ENERGY_LIGHTS).SetActive(false);
        //         __instance.DisplayBloodCost(hide: true);
        //         __instance.DisplayBonesCost(hide: true);

        //         // Force creation of the gem cost
        //         // Actual setting of color happens in managedupdate
        //         __instance.GetGemCostContainer(force: true);
        //         __instance.SetGemCostActive(true);

        //         // Turn off the gemified energy light
        //         __instance.GetPiece(BLUE_GEM_ORIGINAL).SetActive(false);

        //         return;
        //     }

        //     if (__instance.Info.BonesCost > 0)
        //     {
        //         __instance.GetPiece(ENERGY_LIGHTS).SetActive(false);
        //         __instance.DisplayBloodCost(hide: true);
        //         __instance.SetGemCostActive(false);

        //         __instance.DisplayBonesCost();
        //         __instance.GetPiece(BLUE_GEM_ORIGINAL).SetActive(false);

        //         return;
        //     }

        //     if (__instance.Info.BloodCost > 0)
        //     {
        //         __instance.GetPiece(ENERGY_LIGHTS).SetActive(false);
        //         __instance.DisplayBonesCost(hide: true);
        //         __instance.SetGemCostActive(false);

        //         __instance.DisplayBloodCost();
        //         __instance.GetPiece(BLUE_GEM_ORIGINAL).SetActive(false);

        //         return;
        //     }
        // }

        // [HarmonyPatch(typeof(DiskRenderStatsLayer), nameof(DiskRenderStatsLayer.ManagedUpdate))]
        // [HarmonyPostfix]
        // private static void UpdateGemsCost(DiskRenderStatsLayer __instance)
        // {
        //     var gemContainer = __instance.GetGemCostContainer();
        //     foreach (var gem in gemContainer.Keys)
        //     {
        //         if (gemContainer[gem] == null || !gemContainer[gem].gameObject.activeInHierarchy)
        //             continue;

        //         Card card = __instance.GetCard();
        //         Color emissionColor = Color.black;
        //         if (card.Info.gemsCost.Contains(gem))
        //             emissionColor = !GameFlowManager.IsCardBattle || ResourcesManager.Instance.HasGem(gem) ? Color.white : Color.gray;
        //         gemContainer[gem].GetPropertyBlock(__instance.gemsPropertyBlock);
        //         __instance.gemsPropertyBlock.SetColor("_EmissionColor", emissionColor);
        //         gemContainer[gem].SetPropertyBlock(__instance.gemsPropertyBlock);
        //     }
        // }
    }
}
