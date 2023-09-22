using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GemOrangeBrittle : Brittle
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemOrangeBrittle()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Brittle Without Ruby";
            info.rulebookDescription = "After attacking, [creature] will perish if its owner does not control a ruby";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.ORANGE_CELL_INVERSE, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.darkPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(GemOrangeBrittle),
                TextureHelper.GetImageAsTexture("ability_orangegembrittle.png", typeof(GemOrangeBrittle).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => base.RespondsToSlotTargetedForAttack(slot, attacker) && !Card.EligibleForGemBonus(GemType.Orange);

        public override bool RespondsToAttackEnded() => base.RespondsToAttackEnded() && !Card.EligibleForGemBonus(GemType.Orange);
    }
}
