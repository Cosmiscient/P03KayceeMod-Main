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

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GemStrike : AbilityBehaviour, IGetOpposingSlots
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int NumberOfAttacks => BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard).Where(s => s.Card != null && s.Card.HasAnyOfAbilities(Ability.GainGemBlue, Ability.GainGemGreen, Ability.GainGemOrange, Ability.GainGemTriple)).Count();

        private bool hasDealtDamageDirectlyThisTurn = false;

        static GemStrike()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gem Strike";
            info.rulebookDescription = "[creature] attacks once for each gem provider its owner controls, but can only attack the opponent directly once.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.flipYIfOpponent = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
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

        public override bool RespondsToUpkeep(bool playerUpkeep) => true;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            hasDealtDamageDirectlyThisTurn = false;
            yield break;
        }

        public override bool RespondsToDealDamageDirectly(int amount) => amount > 0;

        public override IEnumerator OnDealDamageDirectly(int amount)
        {
            hasDealtDamageDirectlyThisTurn = true;
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AttackIsBlocked))]
        [HarmonyPrefix]
        private static bool BlockAttackIfGemStrikeAttacked(CardSlot opposingSlot, ref bool __result, PlayableCard __instance)
        {
            if (__instance.HasAbility(AbilityID) && opposingSlot.Card == null)
            {
                if (__instance.GetComponentInChildren<GemStrike>().hasDealtDamageDirectlyThisTurn)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
