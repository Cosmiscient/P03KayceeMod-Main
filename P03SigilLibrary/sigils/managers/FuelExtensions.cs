using System.Linq;
using System.Runtime.CompilerServices;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public static class FuelExtensions
    {
        public const string STARTING_FUEL = "FuelManager.StartingFuel";
        public const int MAX_FUEL = 4;
        internal static ConditionalWeakTable<Card, FuelManager.Status> FuelStatus = new();

        public static int GetStartingFuel(this CardInfo info)
        {
            if (info == null)
                return 0;
            var fuel = info.GetExtendedPropertyAsInt(STARTING_FUEL);
            if (fuel.HasValue)
                return fuel.Value;
            if (info.Abilities == null)
                return 0;
            return info.Abilities.Select(a => AbilityManager.AllAbilityInfos.AbilityByID(a).GetExtendedPropertyAsInt(STARTING_FUEL) ?? 0).DefaultIfEmpty(0).Max();
        }

        public static CardInfo SetStartingFuel(this CardInfo info, int fuel)
        {
            return info.SetExtendedProperty(STARTING_FUEL, Mathf.Min(fuel, MAX_FUEL));
        }

        public static AbilityInfo SetDefaultFuel(this AbilityInfo info, int fuel)
        {
            return info.SetExtendedProperty(STARTING_FUEL, fuel);
        }

        public static int? GetCurrentFuel(this Card card)
        {
            if (card == null)
                return null;
            if (FuelStatus.TryGetValue(card, out FuelManager.Status status))
                return status.CurrentFuel;
            return null;
        }

        public static bool HasFuel(this Card card)
        {
            return (card.GetCurrentFuel() ?? -1) > 0;
        }

        public static bool AddFuel(this PlayableCard card, int fuel = 1)
        {
            if (FuelStatus.TryGetValue(card, out FuelManager.Status status))
            {
                if (status.CurrentFuel < MAX_FUEL)
                {
                    status.CurrentFuel = Mathf.Min(fuel + status.CurrentFuel, MAX_FUEL);
                    FuelManager.Instance.RenderCurrentFuel(card);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static bool TrySpendFuel(this PlayableCard card, int fuel = 1)
        {
            if (FuelStatus.TryGetValue(card, out FuelManager.Status status))
            {
                if (status.CurrentFuel >= fuel)
                {
                    status.CurrentFuel -= fuel;
                    FuelManager.Instance.RenderCurrentFuel(card);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}