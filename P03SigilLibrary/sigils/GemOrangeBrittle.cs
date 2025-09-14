using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GemOrangeBrittle : Brittle
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemOrangeBrittle()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Brittle Without Orange";
            info.rulebookDescription = "After attacking, [creature] will perish if its owner does not control an Orange Mox.";
            info.canStack = true;
            info.powerLevel = -1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.ORANGE_CELL_INVERSE, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemOrangeBrittle),
                TextureHelper.GetImageAsTexture("ability_orangegembrittle.png", typeof(GemOrangeBrittle).Assembly)
            ).Id;

            info.SetAbilityRedirect("Orange Mox", Ability.GainGemOrange, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => base.RespondsToSlotTargetedForAttack(slot, attacker) && !Card.EligibleForGemBonus(GemType.Orange);

        public override bool RespondsToAttackEnded() => base.RespondsToAttackEnded() && !Card.EligibleForGemBonus(GemType.Orange);
    }
}
