using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Spells;
using Infiniscryption.Spells.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class RefillBattery : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static RefillBattery()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Recharge";
            info.rulebookDescription = "When [creature] is played, one energy will be refilled (up to the player's maximum energy)";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RefillBattery),
                TextureHelper.GetImageAsTexture("ability_refill_battery.png", typeof(RefillBattery).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return ResourcesManager.Instance.AddEnergy(1);
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.CanPlay))]
        [HarmonyPrefix]
        private static bool OnlyPlayIfNeedsEnergy(PlayableCard __instance, ref bool __result)
        {
            if (__instance.HasAbility(RefillBattery.AbilityID) && __instance.Info.IsSpell() && ResourcesManager.Instance.PlayerEnergy == ResourcesManager.Instance.PlayerMaxEnergy)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
