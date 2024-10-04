using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class FuelShield : AbilityBehaviour, IModifyDamageTaken
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static FuelShield()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fuel Shield";
            info.rulebookDescription = "As long as [creature] has fuel, it takes damage by losing fuel instead of health.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.ACTIVE_WHEN_FUELED, true);
            info.colorOverride = GameColors.Instance.darkLimeGreen;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FuelShield),
                TextureHelper.GetImageAsTexture("ability_shield_when_fueled.png", typeof(FuelShield).Assembly)
            ).Id;

            info.SetUniqueRedirect("fuel", "fuelManagerPage", GameColors.Instance.limeGreen);
        }

        public bool RespondsToModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage) => target == this.Card && this.Card.GetCurrentFuel() > 0;

        public int OnModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage)
        {
            int fuelDamage = Mathf.Min(damage, this.Card.GetCurrentFuel() ?? 0);
            if (fuelDamage > 0)
                if (this.Card.TrySpendFuel(fuelDamage))
                    this.Card.Anim.StrongNegationEffect();
            return damage - fuelDamage;
        }

        public int TriggerPriority(PlayableCard target, int damage, PlayableCard attacker) => int.MinValue;
    }
}
