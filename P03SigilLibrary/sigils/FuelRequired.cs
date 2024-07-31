using System.Collections;
using System.Collections.Generic;
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
    public class FuelRequired : AbilityBehaviour, IOnPostSlotAttackSequence
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static FuelRequired()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fuel Strike";
            info.rulebookDescription = "[creature] consumes one fuel to attack and can only attack if it has fuel.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = true;
            info.passive = false;
            info.SetExtendedProperty(AbilityIconBehaviours.ACTIVE_WHEN_FUELED, true);
            info.colorOverride = GameColors.Instance.darkLimeGreen;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FuelRequired),
                TextureHelper.GetImageAsTexture("ability_attack_when_fueled.png", typeof(FuelRequired).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
        [HarmonyPostfix]
        [HarmonyAfter("cyantist.inscryption.api")]
        [HarmonyPriority(HarmonyLib.Priority.VeryLow)]
        private static void FuelRequiredToAttack(PlayableCard __instance, ref List<CardSlot> __result)
        {
            if (__instance.HasAbility(AbilityID) && __instance.GetCurrentFuel() == 0)
                __result.Clear();
        }

        public bool RespondsToPostSlotAttackSequence(CardSlot attackingSlot) => attackingSlot == this.Card.Slot;

        public IEnumerator OnPostSlotAttackSequence(CardSlot attackingSlot)
        {
            if (this.Card.TrySpendFuel())
                yield return new WaitForSeconds(0.1f);
        }
    }
}
