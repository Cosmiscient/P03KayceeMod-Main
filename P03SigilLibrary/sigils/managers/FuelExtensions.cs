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
            return info.GetExtendedPropertyAsInt(STARTING_FUEL) ?? 0;
        }

        public static CardInfo SetStartingFuel(this CardInfo info, int fuel)
        {
            return info.SetExtendedProperty(STARTING_FUEL, Mathf.Min(fuel, MAX_FUEL));
        }

        public static int? GetCurrentFuel(this Card card)
        {
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
                    status.CurrentFuel = Mathf.Max(fuel + status.CurrentFuel, MAX_FUEL);
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