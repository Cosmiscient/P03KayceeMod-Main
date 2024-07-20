using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class SwapCardCostSequencer : DiskDriveModSequencer
    {
        internal static SwapCardCostSequencer Instance { get; set; }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool HandleTradeTokens(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            if (nodeData is SwapCardCostNodeData)
            {
                if (Instance.SafeIsUnityNull())
                {
                    Instance = __instance.gameObject.AddComponent<SwapCardCostSequencer>();
                    Instance.cardArray = __instance.addCardAbilitySequencer.cardArray;
                    Instance.deckPile = __instance.addCardAbilitySequencer.deckPile;
                    Instance.diskDrive = __instance.addCardAbilitySequencer.diskDrive;
                    Instance.selectedCardExamineMarker = __instance.addCardAbilitySequencer.selectedCardExamineMarker;
                    Instance.selectedCardPositionMarker = __instance.addCardAbilitySequencer.selectedCardPositionMarker;
                }

                SpecialNodeHandler.Instance.StartCoroutine(Instance.ModifyCardSequence());
                return false;
            }
            return true;
        }

        private P03AbilityFace p03AbilityFace = null;
        private static Dictionary<string, Texture> _swapTextures = new();
        private static Texture GetSwapTexture(CardModificationInfo mod)
        {
            bool addsBlue = mod.addGemCost != null && mod.addGemCost.Contains(GemType.Blue);
            bool addsGreen = mod.addGemCost != null && mod.addGemCost.Contains(GemType.Green);
            bool addsOrange = mod.addGemCost != null && mod.addGemCost.Contains(GemType.Orange);
            bool addsBleene = addsBlue && addsGreen;
            bool addsGoranj = addsGreen && addsOrange;
            bool addsOlru = addsOrange && addsBlue;

            string fromKey = mod.bloodCostAdjustment < 0 ? "blood"
                             : mod.bonesCostAdjustment < 0 ? "bones"
                             : mod.energyCostAdjustment < 0 ? "energy"
                             : mod.HasRemoveBlueGemCost() && mod.HasRemovedGreenGemCost() ? "bleene"
                             : mod.HasRemovedGreenGemCost() && mod.HasRemovedOrangeGemCost() ? "goranj"
                             : mod.HasRemoveBlueGemCost() && mod.HasRemovedOrangeGemCost() ? "orlu"
                             : mod.HasRemoveBlueGemCost() ? "blue"
                             : mod.HasRemovedGreenGemCost() ? "green"
                             : "orange";

            string toKey = mod.bloodCostAdjustment > 0 ? "blood"
                           : mod.bonesCostAdjustment > 0 ? "bones"
                           : mod.energyCostAdjustment > 0 ? "energy"
                           : addsBleene ? "bleene"
                           : addsGoranj ? "goranj"
                           : addsOlru ? "orlu"
                           : addsBlue ? "blue"
                           : addsGreen ? "green"
                           : "orange";

            string fullKey = $"swap_{fromKey}_for_{toKey}.png";

            if (!_swapTextures.ContainsKey(fullKey))
                _swapTextures[fullKey] = TextureHelper.GetImageAsTexture(fullKey, typeof(SwapCardCostSequencer).Assembly);

            return _swapTextures[fullKey];
        }

        public override void OnStartModSelection()
        {
            base.OnStartModSelection();
            this.p03AbilityFace = P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AbilityIcon, true, true).GetComponent<P03AbilityFace>();
        }

        public override void DisplayMod(CardModificationInfo mod, bool fromCursorExit = false)
        {
            this.p03AbilityFace.abilityRenderer.material.SetTexture("_MainTex", GetSwapTexture(mod));
        }

        public override void ShowOverviewOnScreen()
        {
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AbilityIcon, false, true);
        }

        public override void ShowDetailsOnScreen()
        {
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.AddMod, false, true).GetComponent<P03AddModFace>().DisplayCardWithMod(this.selectedCard.Info, this.currentValidModChoices[this.currentModIndex]);
        }

        public override List<CardInfo> GetValidCardsFromDeck() =>
            new(Part3SaveData.Data.deck.Cards.Where(ci =>
                ci.EnergyCost > 0 ||
                ci.BloodCost > 0 ||
                ci.BonesCost > 0 ||
                ci.GemsCost.Count > 0
                )
            );

        private static CardModificationInfo CloneForCostSwap(CardInfo forCard, CardModificationInfo mod, ref int randomSeed)
        {
            int mySeed = randomSeed;
            CardModificationInfo retval = (CardModificationInfo)mod.Clone();
            if (mod.addGemCost != null && mod.addGemCost.Count > 0)
            {
                retval.addGemCost = new();
                List<int> possibles = new() { 0, 1, 2 };
                for (int i = 0; i < mod.addGemCost.Count; i++)
                {
                    int index = SeededRandom.Range(0, possibles.Count, mySeed++);
                    retval.addGemCost.Add((GemType)possibles[index]);
                    possibles.RemoveAt(index);
                }
            }
            if (mod.HasRemovedAnyGemCost())
            {
                int count = 0;
                if (mod.HasRemoveBlueGemCost())
                    count += 1;
                if (mod.HasRemovedGreenGemCost())
                    count += 1;
                if (mod.HasRemovedOrangeGemCost())
                    count += 1;
                retval.ClearAllRemovedGemsCosts();
                var gems = forCard.GemsCost.OrderBy(o => SeededRandom.Value(mySeed++) * 100).ToList();
                for (int i = 0; i < count; i++)
                {
                    if (i >= gems.Count)
                        continue;

                    if (gems[i] == GemType.Green)
                        retval.RemoveGreenGemCost(true);
                    if (gems[i] == GemType.Orange)
                        retval.RemoveOrangeGemCost(true);
                    if (gems[i] == GemType.Blue)
                        retval.RemoveBlueGemCost(true);
                }
            }
            randomSeed = mySeed;
            return retval;
        }

        private static readonly List<List<CardModificationInfo>> ValidSwaps = new()
        {
            // Blood
            new() { new() { bloodCostAdjustment = -1, energyCostAdjustment = 2 },
                    new() { bloodCostAdjustment = -1, bonesCostAdjustment = 2 },
                    new() { bloodCostAdjustment = -1, addGemCost = new() { GemType.Green, GemType.Green} } },
            
            // Bones
            new() { new() { bonesCostAdjustment = -2, bloodCostAdjustment = 1 },
                    new() { bonesCostAdjustment = -2, energyCostAdjustment = 2 },
                    new() { bonesCostAdjustment = -1, addGemCost = new() { GemType.Green} } },
            
            // Gems
            new() { new() { energyCostAdjustment = 2 },
                    new() { bonesCostAdjustment = 3 },
                    new() { bloodCostAdjustment = 1 }},

            // Energy
            new() { new() { energyCostAdjustment = -2, bloodCostAdjustment = 1 },
                    new() { energyCostAdjustment = -1, bonesCostAdjustment = 2 },
                    new() { energyCostAdjustment = -3, addGemCost = new() { GemType.Green, GemType.Green} } }
        };

        static SwapCardCostSequencer()
        {
            ValidSwaps[2][0].RemoveBlueGemCost(true);
            ValidSwaps[2][1].RemoveBlueGemCost(true);
            ValidSwaps[2][2].RemoveBlueGemCost(true);
        }

        private Dictionary<CardInfo, List<CardModificationInfo>> ValidMods = new();

        public override void UpdateModChoices(CardInfo selectedCard)
        {
            if (this.modChoices == null)
            {
                ValidMods.Clear();
            }
            if (ValidMods.ContainsKey(selectedCard))
            {
                this.modChoices = ValidMods[selectedCard];
                this.currentValidModChoices = ValidMods[selectedCard];
                return;
            }
            List<CardModificationInfo> possibles = new();
            int randomSeed = P03AscensionSaveData.RandomSeed;
            if (selectedCard.BloodCost > 0)
                possibles.AddRange(ValidSwaps[0].Select(m => CloneForCostSwap(selectedCard, m, ref randomSeed)));
            if (selectedCard.BonesCost > 0)
                possibles.AddRange(ValidSwaps[1].Select(m => CloneForCostSwap(selectedCard, m, ref randomSeed)));
            if (selectedCard.GemsCost.Count > 0)
                possibles.AddRange(ValidSwaps[2].Select(m => CloneForCostSwap(selectedCard, m, ref randomSeed)));
            if (selectedCard.EnergyCost > 0)
                possibles.AddRange(ValidSwaps[3].Select(m => CloneForCostSwap(selectedCard, m, ref randomSeed)));

            possibles.RemoveAll(p => p == null);

            while (possibles.Count > 3)
                possibles.RemoveAt(SeededRandom.Range(0, possibles.Count, randomSeed++));

            ValidMods[selectedCard] = possibles;
            this.modChoices = possibles;
            this.currentValidModChoices = possibles;
        }

        [HarmonyPatch(typeof(P03AddModFace), nameof(P03AddModFace.DisplayCardWithMod))]
        [HarmonyPostfix]
        private static void MoveIfNoAbility(P03AddModFace __instance, CardModificationInfo mod)
        {
            __instance.transform.Find("Icon").gameObject.SetActive(mod.abilities.Count > 0);
            __instance.transform.Find("Arrow").gameObject.SetActive(mod.abilities.Count > 0);
            __instance.transform.Find("P03FaceCardDisplayer").localPosition =
                mod.abilities.Count > 0 ? new(-0.237f, 0f, 0f) : Vector3.zero;
            foreach (var bc in __instance.transform.Find("P03FaceCardDisplayer/CardAbilityIcons").GetComponentsInChildren<BlinkColor>())
                bc.enabled = mod.abilities.Count > 0;
        }
    }
}