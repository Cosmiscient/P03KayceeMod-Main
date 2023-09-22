using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
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

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => slot.Card != null && slot.IsOpponentSlot() == attacker.OpponentCard;

        private CardInfo GetEvolution(PlayableCard target)
        {
            CardInfo defaultCard = target.Info.evolveParams != null
                                 ? CardLoader.Clone(target.Info.evolveParams.evolution)
                                 : EvolveParams.GetDefaultEvolution(target.Info);
            return defaultCard;
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            CardInfo newCard = GetEvolution(slot.Card);
            yield return slot.Card.TransformIntoCard(newCard);
        }
    }
}
