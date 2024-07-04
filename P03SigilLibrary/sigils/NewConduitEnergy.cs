using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class NewConduitEnergy : Conduit
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static NewConduitEnergy()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bonus Energy Conduit";
            info.rulebookDescription = "If [creature] is part of a completed circuit, it provides three additional energy.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.conduit = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(NewConduitEnergy),
                TextureHelper.GetImageAsTexture("NewConduitEnergy.png", typeof(NewConduitEnergy).Assembly)
            ).Id;
        }

        public const int MAX_ENERGY = 3;
        public int RemainingEnergy { get; private set; } = MAX_ENERGY;

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            RemainingEnergy = MAX_ENERGY;
            Card.RenderCard();
            yield break;
        }

        public IEnumerator TryRestoreEnergy()
        {
            if (!CompletesCircuit())
            {
                P03SigilLibraryPlugin.Log.LogDebug($"New Energy Conduit does not complete circuit");
                yield break;
            }

            yield return PreSuccessfulTriggerSequence();

            int energyToMax = ResourcesManager.Instance.PlayerMaxEnergy - ResourcesManager.Instance.PlayerEnergy;
            P03SigilLibraryPlugin.Log.LogDebug($"Energy to max is: {energyToMax}. Remaining energy is {RemainingEnergy}");
            if (energyToMax > 0 && RemainingEnergy > 0)
            {
                int energyToGive = Math.Min(energyToMax, RemainingEnergy);
                P03SigilLibraryPlugin.Log.LogDebug($"Energy to give is: {energyToGive}");
                yield return ResourcesManager.Instance.AddEnergy(energyToGive);
                RemainingEnergy -= energyToGive;
                Card.RenderCard();
            }
            yield break;
        }

        public bool CompletesCircuit()
        {
            TryCreateConduitManager();
            ConduitCircuitManager.Instance.UpdateCircuits();
            foreach (CardSlot slot in BoardManager.Instance.GetSlots(true))
            {
                if (ConduitCircuitManager.Instance.GetConduitsForSlot(slot).Contains(Card))
                {
                    return true;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.SpendEnergy))]
        [HarmonyPostfix]
        private static IEnumerator RefreshEnergySequence(IEnumerator sequence)
        {
            yield return sequence;

            P03SigilLibraryPlugin.Log.LogDebug($"Seeing if I should refresh energy");
            foreach (CardSlot slot in BoardManager.Instance.GetSlots(true).Where(s => s.Card != null))
            {
                if (slot.Card.HasAbility(AbilityID))
                    yield return slot.Card.GetComponent<NewConduitEnergy>().TryRestoreEnergy();
            }
        }
    }
}
