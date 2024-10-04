using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ForcedUpgrade : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ForcedUpgrade()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Upgrade";
            info.rulebookDescription = "When [creature] targets a slot, the target is immediately upgraded to a stronger version of itself.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ForcedUpgrade),
                TextureHelper.GetImageAsTexture("ability_upgrade_now.png", typeof(ForcedUpgrade).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => slot.Card != null && slot.IsOpponentSlot() == Card.OpponentCard;

        private CardInfo Cleanse(CardInfo info)
        {
            CardInfo cleansed = CardLoader.Clone(info);
            if (cleansed.abilities.Contains(Ability.Transformer))
            {
                cleansed.abilities.Remove(Ability.Transformer);
                if (info.name.ToLowerInvariant().Contains("xformer") && !info.HasCardMetaCategory(CardMetaCategory.ChoiceNode))
                    cleansed.evolveParams = null;
            }
            return cleansed;
        }

        private CardInfo GetEvolution(PlayableCard target)
        {
            CardInfo cleansedTargetInfo = Cleanse(target.Info);
            CardInfo baseInfo = cleansedTargetInfo.evolveParams != null
                        ? CardLoader.Clone(cleansedTargetInfo.evolveParams.evolution)
                        : EvolveParams.GetDefaultEvolution(cleansedTargetInfo);

            if (baseInfo.HasAbility(Ability.Transformer))
                baseInfo.mods.Add(new() { negateAbilities = new() { Ability.Transformer } });

            return baseInfo;
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            CardInfo newCard = GetEvolution(slot.Card);
            yield return slot.Card.TransformIntoCard(newCard);
        }
    }
}
