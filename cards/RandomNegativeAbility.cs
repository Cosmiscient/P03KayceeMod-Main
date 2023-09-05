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
            CardModificationInfo newMod = new(ChooseAbility());

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
            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, -100, 0, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || Card.HasAbility(x) || x == AbilityID || AbilitiesUtil.GetInfo(x).powerLevel >= 0);

            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())]
                : Ability.Brittle;
        }
    }
}