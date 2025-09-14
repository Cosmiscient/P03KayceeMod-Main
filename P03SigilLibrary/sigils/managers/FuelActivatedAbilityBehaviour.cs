using System.Collections;
using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public abstract class FuelActivatedAbilityBehaviour : ActivatedAbilityBehaviour
    {
        public abstract int FuelCost { get; }

        public bool HasActivatedThisTurn { get; internal set; } = false;

        public override bool CanActivate()
        {
            return base.CanActivate() && !HasActivatedThisTurn;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            if (playerUpkeep != this.Card.OpponentCard)
                HasActivatedThisTurn = false;

            return true;
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (this.Card.OpponentCard && AbilitiesUtil.GetInfo(this.Ability).opponentUsable)
            {
                if (CanAfford() && CanActivate())
                    yield return OnActivatedAbility();
            }
            yield break;
        }

        public abstract IEnumerator ActivateAfterSpendFuel();

        public sealed override IEnumerator Activate()
        {
            if (this.Card.TrySpendFuel(this.FuelCost))
            {
                yield return ActivateAfterSpendFuel();
                HasActivatedThisTurn = true;
            }
        }

        [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.CanAfford))]
        [HarmonyPostfix]
        private static void CanAffordFuel(ActivatedAbilityBehaviour __instance, ref bool __result)
        {
            if (__instance is FuelActivatedAbilityBehaviour ifc)
                __result = __result && __instance.Card.GetCurrentFuel() >= ifc.FuelCost;
        }
    }
}