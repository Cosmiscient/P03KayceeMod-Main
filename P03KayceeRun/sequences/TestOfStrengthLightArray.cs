using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class TestOfStrengthLightArray : ManagedBehaviour
    {
        [SerializeField]
        private Transform[] lightChildren;

        [SerializeField]
        private HoloFloatingLabel label;

        public static TestOfStrengthLightArray Create(Transform parent)
        {
            GameObject lightArray = new GameObject("LightArray");
            lightArray.transform.SetParent(parent);
            lightArray.transform.localPosition = new(0f, 0f, 0f);
            var array = lightArray.AddComponent<TestOfStrengthLightArray>();

            GameObject upperArray = new GameObject("UpperArray");
            upperArray.transform.SetParent(lightArray.transform);
            upperArray.transform.localPosition = new(0f, 0f, 1f);

            GameObject lowerArray = new GameObject("LowerArray");
            lowerArray.transform.SetParent(lightArray.transform);
            lowerArray.transform.localPosition = new(0f, 0f, -1f);

            array.lightChildren = new Transform[] { upperArray.transform, lowerArray.transform };

            for (int i = 0; i < 12; i++)
            {
                foreach (Transform lightParent in array.lightChildren)
                {
                    GameObject light = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/map/holomapscenery/HoloGemBlue"));
                    light.transform.SetParent(lightParent);
                    light.transform.localScale = new(0.25f, 0.25f, 0.25f);
                    light.transform.localPosition = new(-2.75f + (float)i * (5.5f / 12f), 0f, 0f);
                    MaterialHelper.HolofyAllRenderers(light, GameColors.Instance.glowRed);
                }
            }

            // And now the label
            GameObject sampleObject = RunBasedHoloMap.SpecialNodePrefabs[HoloMapNode.NodeDataType.BuildACard].transform.Find("HoloFloatingLabel").gameObject;
            GameObject labelObject = UnityEngine.Object.Instantiate(sampleObject, lightArray.transform);
            labelObject.transform.localPosition = new(0.49f, -0.5f, -1.2792f);
            labelObject.transform.localScale = new(1.3638f, 1.3638f, 1.3638f);
            HoloFloatingLabel label = labelObject.GetComponent<HoloFloatingLabel>();
            label.line.gameObject.SetActive(false);
            label.line = null;
            array.label = label;

            return array;
        }

        private int lastOffset = -1;
        private string lastLabel = string.Empty;
        public override void ManagedUpdate()
        {

            int offset = Mathf.FloorToInt(Time.fixedTime % 2);
            if (offset != lastOffset)
            {
                foreach (Transform parent in lightChildren)
                {
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        GameObject child = parent.GetChild(i).gameObject;
                        MaterialHelper.HolofyAllRenderers(child, i % 2 == offset ? GameColors.Instance.glowRed : GameColors.Instance.brightGold);
                    }
                }

                label.gameObject.SetActive(true);
                string text = $"High Score: {TestOfStrengthBattleSequencer.HighScore}";
                if (!text.Equals(lastLabel, System.StringComparison.OrdinalIgnoreCase))
                {
                    label.SetText(text);
                    lastLabel = text;
                }
            }
            lastOffset = offset;
        }
    }
}