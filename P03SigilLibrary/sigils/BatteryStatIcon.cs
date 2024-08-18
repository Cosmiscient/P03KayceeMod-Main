using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class BatteryPower : VariableStatBehaviour
    {
        public static SpecialStatIcon AbilityID { get; private set; }
        public override SpecialStatIcon IconType => AbilityID;

        public static int EnergySpent { get; private set; }

        static BatteryPower()
        {
            StatIconInfo info = StatIconManager.New(P03SigilLibraryPlugin.PluginGuid,
                "Energy Power",
                "The value represented with this sigil will be equal to the number of times the player has spent energy this turn.",
                typeof(BatteryPower));
            info.appliesToAttack = true;
            info.appliesToHealth = false;
            info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
            info.SetIcon(TextureHelper.GetImageAsTexture("battery_stat_icon.png", typeof(BatteryPower).Assembly));
            AbilityID = info.iconType;
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.SpendEnergy))]
        [HarmonyPrefix]
        private static void IncrementSpendCounter(int amount)
        {
            if (amount > 0)
                EnergySpent += 1;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPrefix]
        private static void ResetSpendCounter(bool playerUpkeep)
        {
            if (playerUpkeep)
                EnergySpent = 0;
        }

        public override int[] GetStatValues() => new int[] { EnergySpent, 0 };
    }
}