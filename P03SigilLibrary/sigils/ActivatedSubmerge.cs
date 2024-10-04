using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ActivatedSubmerge : FuelActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int FuelCost => 1;

        private const string TEMPORARY_MOD_ID = "ACTIVATED_SUBMERGE_MOD";

        static ActivatedSubmerge()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Submerge";
            info.rulebookDescription = "Spend one fuel: submerge during the opponent's turn. While submerged, opposing creatures attack its owner directly.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedSubmerge),
                TextureHelper.GetImageAsTexture("ability_activated_submerge.png", typeof(ActivatedSubmerge).Assembly)
            ).Id;

            info.SetUniqueRedirect("fuel", "fuelManagerPage", GameColors.Instance.limeGreen);
        }

        public override IEnumerator ActivateAfterSpendFuel()
        {
            CardModificationInfo mod = this.Card.TemporaryMods.FirstOrDefault(m => m.singletonId != null && m.singletonId.Equals(TEMPORARY_MOD_ID));
            if (mod == null)
            {
                mod = new(Ability.Submerge);
                mod.singletonId = TEMPORARY_MOD_ID;
                this.Card.Status.hiddenAbilities.Add(AbilityID);
                this.Card.AddTemporaryMod(mod);
            }

            yield break;
        }

        [HarmonyPatch(typeof(Submerge), nameof(Submerge.OnUpkeep))]
        [HarmonyPostfix]
        private static IEnumerator ResolveActivatedSubmerge(IEnumerator sequence, Submerge __instance)
        {
            yield return sequence;
            CardModificationInfo mod = __instance.Card.TemporaryMods.FirstOrDefault(m => m.singletonId != null && m.singletonId.Equals(TEMPORARY_MOD_ID));
            if (mod != null)
            {
                __instance.Card.Status.hiddenAbilities.Remove(AbilityID);
                __instance.Card.RemoveTemporaryMod(mod);
            }
        }
    }
}
