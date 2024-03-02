using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class TripleCardRenderIcons
    {
        public const float X_STEP_SIZE = 0.34f;

        // This patch modifies the way cards are rendered so that more than two sigils can be displayed on a single card
        private static Transform Find(Transform start, string target = "CardBase")
        {
            foreach (Transform child in start)
            {
                if (child.name == target)
                {
                    return child;
                }

                Transform result = Find(child, target);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private static void AddIconGroupToCard(Transform abilityIconParent, int numberOfIcons)
        {
            if (abilityIconParent == null)
                return;

            string name = $"DefaultIcons_{numberOfIcons}Abilities";

            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
                return;

            if (abilityIconParent.Find(name) == null)
            {
                GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
                GameObject newGroup = Object.Instantiate(twoAbilities, abilityIconParent);
                newGroup.name = name;

                float leftOffset = (-(numberOfIcons / 2f) * X_STEP_SIZE) + (X_STEP_SIZE / 2f);


                newGroup.transform.Find("Ability_1").localPosition = new(leftOffset, 0f, 0f);
                newGroup.transform.Find("Ability_2").localPosition = new(leftOffset + X_STEP_SIZE, 0f, 0f);

                for (int i = 2; i < numberOfIcons; i++)
                {
                    GameObject newIcon = Object.Instantiate(newGroup.transform.Find("Ability_1").gameObject, newGroup.transform);
                    newIcon.name = $"Ability_{i + 1}";
                    newIcon.transform.localPosition = new(leftOffset + (X_STEP_SIZE * i), 0f, 0f);
                }

                controller.defaultIconGroups.Add(newGroup);
                newGroup.gameObject.AddComponent<InverseStretch>();
            }
        }

        [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.UpdateAbilityIcons))]
        [HarmonyPostfix]
        private static void LimeGreenGoobertIcons(ref CardAbilityIcons __instance, PlayableCard playableCard)
        {
            if (playableCard != null && playableCard.Info != null && playableCard.Info.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                MaterialHelper.RecolorAllMaterials(__instance.gameObject, GameColors.Instance.brightLimeGreen, "Standard", true);
            }
        }

        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPrefix]
        private static void UpdateCard(ref Card __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            Transform cardBase = __instance.gameObject.transform;
            if (__instance.Info.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                Transform parent = Find(cardBase, "CardAbilityIcons_Part3_Invisible");
                for (int i = 5; i <= 12; i++)
                    AddIconGroupToCard(parent, i);

                if (parent.Find("DefaultIcons_1Ability").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_1Ability").gameObject.AddComponent<InverseStretch>();
                if (parent.Find("DefaultIcons_2Abilities").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_2Abilities").gameObject.AddComponent<InverseStretch>();
                if (parent.Find("DefaultIcons_3Abilities").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_3Abilities").gameObject.AddComponent<InverseStretch>();
                if (parent.Find("DefaultIcons_4Abilities").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_4Abilities").gameObject.AddComponent<InverseStretch>();

                if (__instance is PlayableCard pcard && pcard.OnBoard)
                {
                    MaterialHelper.RecolorAllMaterials(parent.gameObject, GameColors.Instance.brightLimeGreen, "Standard", true, forceEnable: true);
                }
                else
                {
                    foreach (Renderer r in parent.gameObject.GetComponentsInChildren<Renderer>())
                    {
                        r.enabled = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DiskScreenCardDisplayer), nameof(DiskScreenCardDisplayer.DisplayInfo))]
        [HarmonyPrefix]
        private static void UpdateCamera(ref DiskScreenCardDisplayer __instance, CardRenderInfo renderInfo, PlayableCard playableCard)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            Transform cardBase = __instance.gameObject.transform;
            if (renderInfo.baseInfo.specialAbilities.Contains(GoobertCenterCardBehaviour.AbilityID))
            {
                Transform parent = Find(cardBase, "CardAbilityIcons_Part3");
                for (int i = 5; i <= 12; i++)
                    AddIconGroupToCard(parent, i);

                if (parent.Find("DefaultIcons_1Ability").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_1Ability").gameObject.AddComponent<InverseStretch>();
                if (parent.Find("DefaultIcons_2Abilities").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_2Abilities").gameObject.AddComponent<InverseStretch>();
                if (parent.Find("DefaultIcons_3Abilities").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_3Abilities").gameObject.AddComponent<InverseStretch>();
                if (parent.Find("DefaultIcons_4Abilities").gameObject.GetComponent<InverseStretch>() == null)
                    parent.Find("DefaultIcons_4Abilities").gameObject.AddComponent<InverseStretch>();

                if (playableCard == null || !playableCard.OnBoard)
                {
                    MaterialHelper.RecolorAllMaterials(parent.gameObject, GameColors.Instance.brightLimeGreen, "Standard", true, forceEnable: true);
                }
                else
                {
                    foreach (Renderer r in parent.gameObject.GetComponentsInChildren<Renderer>())
                    {
                        r.enabled = false;
                    }
                }
            }
        }

        public class InverseStretch : MonoBehaviour
        {
            [SerializeField]
            private Vector3 baseline;

            [SerializeField]
            private Vector3 thisBaseline;

            private void Awake() => ResetBaselines();

            public void ResetBaselines()
            {
                if (_matchingTransform != null)
                    baseline = _matchingTransform.localScale;

                thisBaseline = gameObject.transform.localScale;
            }

            private void LateUpdate()
            {
                if (_matchingTransform != null)
                {

                    gameObject.transform.localScale = new(
                        thisBaseline.x * baseline.y / _matchingTransform.localScale.y,
                        thisBaseline.y * baseline.x / _matchingTransform.localScale.x,
                        thisBaseline.z * baseline.z / _matchingTransform.localScale.z
                    );
                    gameObject.transform.localPosition = new(0f, 0f, -0.05f);
                }
                foreach (AbilityIconInteractable icon in gameObject.GetComponentsInChildren<AbilityIconInteractable>())
                    icon.gameObject.transform.localPosition = new(icon.gameObject.transform.localPosition.x, 0f, 0f);
            }

            [SerializeField]
            private Transform _matchingTransform;
            public Transform MatchingTransform
            {
                get => _matchingTransform;
                set
                {
                    _matchingTransform = value;
                    baseline = _matchingTransform.localScale;
                }
            }

            public void TestShader(string shaderName) => MaterialHelper.RecolorAllMaterials(gameObject, GameColors.Instance.brightLimeGreen, shaderName, true);
        }
    }
}