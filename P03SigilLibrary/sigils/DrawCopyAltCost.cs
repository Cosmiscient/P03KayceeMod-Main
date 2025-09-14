using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class DrawCopyAltCost : DrawCreatedCard
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public override CardInfo CardToDraw => GetNextCard();

        private const string MOD_SINGLETON_ID_PREFIX = "DrawCopyAltCost";
        private const string MOD_SINGLETON_ID_BLOOD = "DrawCopyAltCost-Blood";
        private const string MOD_SINGLETON_ID_BONES = "DrawCopyAltCost-Bones";
        private const string MOD_SINGLETON_ID_GEMS = "DrawCopyAltCost-Gems";
        private const string MOD_SINGLETON_ID_ENERGY = "DrawCopyAltCost-Energy";

        static DrawCopyAltCost()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Iterate";
            info.rulebookDescription = "When [creature] is played, a copy of it with a different cost is created in your hand.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(DrawCopyAltCost),
                TextureHelper.GetImageAsTexture("ability_draw_copy_alt_cost.png", typeof(DrawCopyAltCost).Assembly)
            ).Id;
        }

        private CardModificationInfo GetGemsCostMod(CardInfo info)
        {
            var options = Enumerable.Repeat(GemType.Green, Part3SaveData.Data.deckGemsDistribution[0])
                            .Concat(Enumerable.Repeat(GemType.Orange, Part3SaveData.Data.deckGemsDistribution[1]))
                            .Concat(Enumerable.Repeat(GemType.Blue, Part3SaveData.Data.deckGemsDistribution[2]))
                            .ToList();

            GemType gem = options.Count == 0 ? GemType.Green : options[SeededRandom.Range(0, options.Count, P03SigilLibraryPlugin.RandomSeed)];
            return new()
            {
                singletonId = MOD_SINGLETON_ID_GEMS,
                energyCostAdjustment = -info.EnergyCost,
                bloodCostAdjustment = -info.BloodCost,
                addGemCost = info.GemsCost.Count == 0 ? new() { gem } : new(),
                bonesCostAdjustment = -info.BonesCost
            };
        }

        private CardModificationInfo GetEnergyCostMod(CardInfo info)
        {
            return new()
            {
                singletonId = MOD_SINGLETON_ID_ENERGY,
                energyCostAdjustment = 3 - info.EnergyCost,
                bloodCostAdjustment = -info.BloodCost,
                nullifyGemsCost = true,
                bonesCostAdjustment = -info.BonesCost
            };
        }

        private CardModificationInfo GetBloodCostMod(CardInfo info)
        {
            return new()
            {
                singletonId = MOD_SINGLETON_ID_BLOOD,
                energyCostAdjustment = -info.EnergyCost,
                bloodCostAdjustment = 1 - info.BloodCost,
                nullifyGemsCost = true,
                bonesCostAdjustment = -info.BonesCost
            };
        }

        private CardModificationInfo GetBonesCostMod(CardInfo info)
        {
            return new()
            {
                singletonId = MOD_SINGLETON_ID_BONES,
                energyCostAdjustment = -info.EnergyCost,
                bloodCostAdjustment = -info.BloodCost,
                nullifyGemsCost = true,
                bonesCostAdjustment = 3 - info.BonesCost
            };
        }

        private CardInfo GetNextCard()
        {
            CardInfo card = CardLoader.Clone(this.Card.Info);

            // ENERGY > BONES > BLOOD > GEMS > ENERGY
            CardModificationInfo existingMod = card.Mods?.FirstOrDefault(m => m != null && !string.IsNullOrEmpty(m.singletonId) && m.singletonId.StartsWith(MOD_SINGLETON_ID_PREFIX));
            if (existingMod != null)
                card.mods.Remove(existingMod);

            // Don't bring over non-copyable mods
            card.mods.RemoveAll(m => m.nonCopyable);

            if ((existingMod == null && card.EnergyCost > 0) || (existingMod != null && existingMod.singletonId.Equals(MOD_SINGLETON_ID_ENERGY)))
                card.mods.Add(GetBonesCostMod(card));
            else if ((existingMod == null && card.BonesCost > 0) || (existingMod != null && existingMod.singletonId.Equals(MOD_SINGLETON_ID_BONES)))
                card.mods.Add(GetBloodCostMod(card));
            else if ((existingMod == null && card.BloodCost > 0) || (existingMod != null && existingMod.singletonId.Equals(MOD_SINGLETON_ID_BLOOD)))
                card.mods.Add(GetGemsCostMod(card));
            else
                card.mods.Add(GetEnergyCostMod(card));

            return card;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            yield return CreateDrawnCard();
            yield return LearnAbility();
        }
    }
}