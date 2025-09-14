using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ActivatedStrafeSelfRight : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int EnergyCost => 1;

        private List<CardSlot> ValidSlots => Card.Slot.GetAdjacentSlots(true).Where(s => s.Index > this.Card.Slot.Index).Where(s => s.Card == null).ToList();

        public override bool CanActivate() => ValidSlots.Count > 0;

        static ActivatedStrafeSelfRight()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "D-Pad Right";
            info.rulebookDescription = "Move to the lane to the right, if it is empty.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedStrafeSelfRight),
                TextureHelper.GetImageAsTexture("ability_activated_strafe_right.png", typeof(ActivatedStrafeSelfRight).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            yield return BoardManager.Instance.AssignCardToSlot(Card, ValidSlots[0], 0.1f, null, false);
            yield return new WaitForSeconds(0.15f);
            yield break;
        }

        [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.GetDistinctShownAbilities))]
        [HarmonyPostfix]
        [HarmonyPriority(HarmonyLib.Priority.VeryLow)]
        private static void StraftRightAlwaysLast(ref List<Ability> __result)
        {
            if (__result.Contains(AbilityID))
            {
                __result.Remove(AbilityID);
                __result.Add(AbilityID);
            }
        }
    }
}
