using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class RandomRareAbility : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static RandomRareAbility()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Apotheosis";
            info.rulebookDescription = "When [creature] is drawn, it gains a randomly selected rare ability.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(RandomRareAbility),
                TextureHelper.GetImageAsTexture("ability_randomrareability.png", typeof(RandomRareAbility).Assembly)
            ).Id;
        }

        private Ability? overrideAbility = null;

        private static bool IsValidCard(CardInfo card) => card.HasCardMetaCategory(CardMetaCategory.Rare) && card.HasAnyOfCardMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer) && card.temple == SaveManager.SaveFile.GetSceneAsCardTemple();

        private static bool HasMetacategory(AbilityInfo info, AbilityMetaCategory cat)
        {
            if (info == null)
                return false;
            if (info.metaCategories == null)
                return false;
            return info.metaCategories.Contains(cat);
        }

        private static bool HasInterface(AbilityManager.FullAbility info, Type iType)
        {
            if (info == null)
                return false;
            if (info.AbilityBehavior == null)
                return false;
            var ifaces = info.AbilityBehavior.GetInterfaces();
            if (ifaces == null)
                return false;
            return ifaces.Contains(iType);
        }

        private static bool IsModularOrSacrifice(Ability ab)
        {
            var info = AbilityManager.AllAbilities.AbilityByID(ab);
            var matches = ab == Ability.RandomAbility;
            if (info == null || info.Info == null)
                return matches;
            if (SaveManager.SaveFile.IsPart1)
                matches = matches || HasMetacategory(info.Info, AbilityMetaCategory.Part1Modular);
            else if (SaveManager.SaveFile.IsPart3)
                matches = matches || HasMetacategory(info.Info, AbilityMetaCategory.Part3Modular);
            matches = matches || HasInterface(info, typeof(IAbsorbSacrifices));
            return matches;
        }

        public static List<Ability> RareAbilities
        {
            get
            {
                List<Ability> allBustedAbilities = CardManager.AllCardsCopy
                                                              .Where(IsValidCard)
                                                              .SelectMany(c => c.abilities)
                                                              .ToList();
                allBustedAbilities.Add(Ability.TriStrike);
                allBustedAbilities.Remove(Ability.Evolve);
                allBustedAbilities.Remove(Ability.IceCube);
                allBustedAbilities.Remove(Ability.TailOnHit);
                allBustedAbilities.Remove(Ability.Transformer);
                allBustedAbilities.RemoveAll(IsModularOrSacrifice);
                return allBustedAbilities.Distinct().ToList();
            }
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

            List<Ability> learnedAbilities = RareAbilities;

            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())]
                : Ability.Brittle;
        }
    }
}