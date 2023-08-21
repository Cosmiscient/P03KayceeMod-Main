// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using DiskCardGame;
// using HarmonyLib;
// using Pixelplacement;
// using UnityEngine;

// namespace Infiniscryption.P03KayceeRun.Cards.Stickers
// {
//     /// <summary>
//     /// Replacement for the RulebookPageFlipper that behaves the way we want the sticker book to behave
//     /// The main difference is that we always animate a forward flip and never a backward flip
//     /// </summary>
//     [HarmonyPatch]
//     internal class StickerPageFlipper : RulebookPageFlipper
//     {
//         public override void ShowFlip(bool forwards, float duration)
//         {
//             base.ShowFlip(true, duration);
//         }

//         [HarmonyPatch(typeof(RulebookPageFlipper), nameof(RulebookPageFlipper.RenderPages))]
//         [HarmonyPostfix]
//         private static void RenderStickerOverPages(Transform topPage, Transform bottomPage, bool forwardsFlip)
//         {
//             foreach (var dragger in topPage.GetComponentsInChildren<StickerDrag>().ToList())
//                 GameObject.Destroy(dragger.gameObject);

//             foreach (var dragger in bottomPage.GetComponentsInChildren<StickerDrag>().ToList())
//                 GameObject.Destroy(dragger.gameObject);


//         }

//         internal static void ReplaceRulebookFlipper(GameObject obj)
//         {
//             RulebookPageFlipper oldFlipper = obj.GetComponentInChildren<RulebookPageFlipper>(true);
//             if (oldFlipper == null)
//                 return;

//             StickerPageFlipper newFlipper = oldFlipper.gameObject.AddComponent<StickerPageFlipper>();
//             newFlipper.page1 = oldFlipper.page1;
//             newFlipper.page2 = oldFlipper.page2;
//             newFlipper.pageLoader1 = oldFlipper.pageLoader1;
//             newFlipper.pageLoader2 = oldFlipper.pageLoader2;
//             newFlipper.topPagePos = oldFlipper.topPagePos;
//             newFlipper.bottomPagePos = oldFlipper.bottomPagePos;
//             newFlipper.parentBounce = oldFlipper.parentBounce;
//             newFlipper.topPage = oldFlipper.topPage;

//             GameObject.Destroy(oldFlipper);
//         }

//         [HarmonyPatch(typeof(PageFlipper), nameof(PageFlipper.PageData), MethodType.Getter)]
//         [HarmonyPostfix]
//         private static void GetPageDataForStickerBook(ref PageFlipper __instance, ref List<RuleBookPageInfo> __result)
//         {
//             if (__instance is StickerPageFlipper)
//             {
//                 __result = Stickers.StickerPages;
//             }
//         }
//     }
// }