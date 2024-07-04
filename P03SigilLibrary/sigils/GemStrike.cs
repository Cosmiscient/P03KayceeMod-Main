using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class GemStrike : AbilityBehaviour, IGetOpposingSlots
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int NumberOfAttacks => BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard).Where(s => s.Card != null && s.Card.HasAnyOfAbilities(Ability.GainGemBlue, Ability.GainGemGreen, Ability.GainGemOrange, Ability.GainGemTriple)).Count();

        static GemStrike()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gem Strike";
            info.rulebookDescription = "[creature] attacks once for each gem provider its owner controls.";
            info.canStack = false;
            info.powerLevel = 5;
            info.opponentUsable = true;
            info.flipYIfOpponent = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemStrike),
                TextureHelper.GetImageAsTexture("ability_gemstrike.png", typeof(GemStrike).Assembly)
            ).Id;
        }

        public bool RespondsToGetOpposingSlots() => true;

        public List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots)
        {
            int n = NumberOfAttacks;
            List<CardSlot> retval = new();
            for (int i = 2; i <= n; i++)
                retval.Add(Card.Slot.opposingSlot);
            return retval;
        }
        public bool RemoveDefaultAttackSlot() => NumberOfAttacks == 0;
    }
}
