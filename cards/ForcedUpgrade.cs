using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
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
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ForcedUpgrade),
                TextureHelper.GetImageAsTexture("ability_upgrade_now.png", typeof(ForcedUpgrade).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => slot.Card != null && slot.IsOpponentSlot() == Card.OpponentCard;

        private CardInfo GetEvolution(PlayableCard target)
        {
            CardInfo baseInfo = (!target.HasAbility(Ability.Transformer) || target.Info.HasCardMetaCategory(CardMetaCategory.ChoiceNode)) && target.Info.evolveParams != null
                        ? CardLoader.Clone(target.Info.evolveParams.evolution)
                        : EvolveParams.GetDefaultEvolution(target.Info);

            if (target.HasAbility(Ability.Transformer))
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
