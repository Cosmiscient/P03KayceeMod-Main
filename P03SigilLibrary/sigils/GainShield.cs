using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GainShield : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static GainShield()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Defend";
            info.rulebookDescription = "When [creature] targets a slot, the target gains Nano Armor.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GainShield),
                TextureHelper.GetImageAsTexture("ability_gainshield.png", typeof(GainShield).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => slot.Card != null && slot.IsOpponentSlot() == Card.OpponentCard;

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            CardModificationInfo newShieldMod = new(Ability.DeathShield);
            slot.Card.AddTemporaryMod(newShieldMod);
            yield return new WaitForSeconds(0.2f);
            yield return LearnAbility();
            yield break;
        }
    }
}
