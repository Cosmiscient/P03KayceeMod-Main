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
    public class FuelSiphon : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static FuelSiphon()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fuel Siphon";
            info.rulebookDescription = "Whenever [creature] deals damage directly to the opponent, it gains that much fuel.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FuelSiphon),
                TextureHelper.GetImageAsTexture("ability_fuel_siphon.png", typeof(FuelSiphon).Assembly)
            ).Id;
        }

        public override bool RespondsToDealDamageDirectly(int amount) => amount > 0;

        public override IEnumerator OnDealDamageDirectly(int amount)
        {
            int fuelGained = Mathf.Min(amount, FuelExtensions.MAX_FUEL - (this.Card.GetCurrentFuel() ?? 0));
            if (fuelGained > 0)
            {
                this.Card.AddFuel(fuelGained);
                this.Card.Anim.StrongNegationEffect();
            };
            yield break;
        }
    }
}
