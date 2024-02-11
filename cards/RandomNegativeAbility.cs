using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RandomNegativeAbility : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static RandomNegativeAbility()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Nerf This!";
            info.rulebookDescription = "When [creature] is draw, it gains a randomly selected negative ability";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RandomNegativeAbility),
                TextureHelper.GetImageAsTexture("ability_nerf.png", typeof(RandomNegativeAbility).Assembly)
            ).Id;
        }

        private Ability? overrideAbility = null;

        public void OverclockDebugTest()
        {
            if (!Card.InHand)
                return;

            overrideAbility = Ability.PermaDeath;

            if (Card.TemporaryMods.Count > 0)
                Card.RemoveTemporaryMod(Card.TemporaryMods[0]);

            Card.OnStatsChanged();
            StartCoroutine(OnDrawn());
        }

        public override bool RespondsToDrawn() => true;

        public override IEnumerator OnDrawn()
        {
            if (PlayerHand.Instance is PlayerHand3D ph3d)
            {
                ph3d.MoveCardAboveHand(Card);
            }

            yield return Card.FlipInHand(AddMod);
            yield return LearnAbility(0.5f);
            yield break;
        }

        private void AddMod()
        {
            Card.Status.hiddenAbilities.Add(Ability);
            Ability newAbility = ChooseAbility();
            CardModificationInfo newMod = new(newAbility);

            if (newAbility == Ability.PermaDeath || newAbility == NewPermaDeath.AbilityID)
            {
                newMod.attackAdjustment = 1;
            }

            bool IsMatch(CardModificationInfo x)
            {
                return x.HasAbility(Ability);
            }

            CardModificationInfo existMod = Card.TemporaryMods.Find(IsMatch) ?? Card.Info.Mods.Find(IsMatch);

            if (existMod != null)
            {
                newMod.fromTotem = existMod.fromTotem;
                newMod.fromCardMerge = existMod.fromCardMerge;
            }

            Card.AddTemporaryMod(newMod);
        }

        private Ability ChooseAbility()
        {
            if (overrideAbility.HasValue)
                return overrideAbility.Value;

            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, -100, 0, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Rulebook : AbilityMetaCategory.Part3Rulebook);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || Card.HasAbility(x) || x == AbilityID || AbilitiesUtil.GetInfo(x).powerLevel >= 0);

            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())]
                : Ability.Brittle;
        }
    }
}