using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Zap : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Zap";
            info.rulebookDescription = "Deals damage directly to the target.";
            info.canStack = true;
            info.powerLevel = 1;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };

            Zap.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Zap),
                TextureHelper.GetImageAsTexture("ability_zap.png", typeof(Zap).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            if (slot.Card == null)
                return false;

            if (slot.IsPlayerSlot)
                return false;

            return true;
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            if (slot.Card != null)
                yield return slot.Card.TakeDamage(1, attacker);
        }
    }
}
