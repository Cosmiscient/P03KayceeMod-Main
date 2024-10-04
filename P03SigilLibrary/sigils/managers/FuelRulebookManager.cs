using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Slots;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public static class FuelRulebookManager
    {
        internal static int GetInsertPositionFunc(PageRangeInfo pageRangeInfo, List<RuleBookPageInfo> pages)
        {
            return pages.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
        }

        internal static List<RuleBookPageInfo> CreatePages(RuleBookInfo instance, PageRangeInfo currentRange, AbilityMetaCategory metaCategory)
        {
            List<RuleBookPageInfo> retval = new();

            RuleBookPageInfo page = new();
            page.pageId = "fuelManagerPage";
            retval.Add(page);

            return retval;
        }

        internal static Sprite FuelGaugeSprite = Sprite.Create(
            TextureHelper.GetImageAsTexture("fuelgauge_art.png", typeof(FuelRulebookManager).Assembly),
            new Rect(0, 0, 154, 226),
            new Vector2(0.5f, 0.5f)
        );

        public static void OpenToFuelRulebookPage(PlayableCard card)
        {
            RuleBookController.Instance.SetShown(true, RuleBookController.Instance.OffsetViewForCard(card));
            int pageIndex = RuleBookController.Instance.PageData.IndexOf(RuleBookController.Instance.PageData.Find((RuleBookPageInfo x) => !string.IsNullOrEmpty(x.pageId) && x.pageId.Contains("fuelManagerPage")));
            RuleBookController.Instance.StopAllCoroutines();
            RuleBookController.Instance.StartCoroutine(RuleBookController.Instance.flipper.FlipToPage(pageIndex, 0.2f));
        }

        private static void FillPage(RuleBookPage page, string pageId, object[] otherArgs)
        {
            if (page is AbilityPage aPage && pageId.Contains("fuelManagerPage"))
            {
                aPage.mainAbilityGroup.nameTextMesh.text = Localization.Translate("Fuel");
                aPage.mainAbilityGroup.descriptionTextMesh.text = Localization.Translate("This card has fuel! One or more sigils on this card will consume fuel, either actively or passively. Once this card runs out of fuel, those sigils will no longer function.");

                // Find the "slot renderer" (borrowing code from WW)
                aPage.mainAbilityGroup.transform.Find("IconBox").gameObject.SetActive(false);
                Transform transform = aPage.mainAbilityGroup.transform.Find("SlotRenderer");
                if (transform != null && transform.GetComponent<SpriteRenderer>() == null)
                {
                    GameObject.DestroyImmediate(transform.gameObject);
                    transform = null;
                }
                if (transform == null)
                {
                    var template = aPage.transform.Find("PageAbilityGroup/IconBox");
                    transform = GameObject.Instantiate(template, aPage.mainAbilityGroup.transform).transform;
                    transform.gameObject.name = "SlotRenderer";
                    transform.localPosition += new Vector3(0.1f, -0.1f, 0f);
                    transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

                    var icon = transform.Find("Icon");
                    if (!icon.SafeIsUnityNull())
                        GameObject.Destroy(icon.gameObject);
                }
                transform.gameObject.SetActive(true);
                transform.GetComponent<SpriteRenderer>().sprite = FuelGaugeSprite;
            }
        }

        static FuelRulebookManager()
        {
            var section = RuleBookManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                PageRangeType.Abilities,
                "Additional Mechanics",
                GetInsertPositionFunc,
                CreatePages,
                fillPageAction: FillPage
            );
        }
    }
}