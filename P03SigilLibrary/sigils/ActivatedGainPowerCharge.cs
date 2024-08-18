using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ActivatedGainPowerCharge : ActivatedAbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private static readonly List<Texture2D> PowerupIcons = new()
        {
            TextureHelper.GetImageAsTexture("ability_activated_powerup_0.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activated_powerup_1.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activated_powerup_2.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activated_powerup_3.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activated_powerup_4.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activated_powerup_5.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activated_powerup_6.png", typeof(ActivatedGainPowerCharge).Assembly)
        };

        static ActivatedGainPowerCharge()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Enrage";
            info.rulebookDescription = $"Pay Energy equal to the Power of this card plus 2 to increase the Power of this card by 1.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedGainPowerCharge),
                TextureHelper.GetImageAsTexture("ability_activated_powerup_2.png", typeof(TakeDamageSigil).Assembly)
            ).Id;
        }

        public override int EnergyCost => Mathf.Min(this.Card.NonPassiveAttack() + 2, 6);

        public override IEnumerator Activate()
        {
            CardModificationInfo cardModificationInfo = this.Card.GetOrCreateSingletonTempMod("statsUp");
            cardModificationInfo.attackAdjustment++;
            Card.AddTemporaryMod(cardModificationInfo);

            Card.RenderInfo.OverrideAbilityIcon(AbilityID, PowerupIcons[EnergyCost]);
            Card.RenderCard();
            yield return new WaitForSeconds(0.25f);
        }

        [HarmonyPatch(typeof(AbilityIconInteractable), nameof(AbilityIconInteractable.LoadIcon))]
        [HarmonyPrefix]
        private static bool LoadMissileStrikeIcons(CardInfo info, AbilityInfo ability, ref Texture __result)
        {
            if (ability.ability != AbilityID)
                return true;

            int attack = info.GetPlayableCard()?.NonPassiveAttack() ?? info.Attack;
            int cost = Mathf.Min(attack + 2, 6);
            __result = PowerupIcons[cost];

            return false;
        }

    }
}
