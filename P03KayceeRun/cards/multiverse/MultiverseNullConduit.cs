using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseNullConduit : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MultiverseNullConduit()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.ConduitNull);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "Circuits completed by [creature] are completed in every universe.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = original.passive;
            info.conduit = original.conduit;
            info.conduitCell = original.conduitCell;
            info.hasColorOverride = true;
            info.colorOverride = Color.black;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseNullConduit),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_conduitnull")
            ).Id;
        }

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.GetConduitsForSlot))]
        [HarmonyPostfix]
        private static void MultiverseNullConduitManager(CardSlot slot, ref List<PlayableCard> __result)
        {
            if (MultiverseBattleSequencer.Instance == null || MultiverseBattleSequencer.Instance.MultiverseGames == null)
                return;

            int slotIdx = slot.Index % 10;
            bool isPlayerSlot = slot.IsPlayerSlot;

            // We just repeat the conduit logic for the other universes
            for (int m = 0; m < MultiverseBattleSequencer.Instance.MultiverseGames.Length; m++)
            {
                var universe = MultiverseBattleSequencer.Instance.MultiverseGames[m];

                if (universe == null)
                    continue;

                var slots = isPlayerSlot ? universe.PlayerSlots : universe.OpponentSlots;

                List<PlayableCard> otherMultiverseConduits = new();
                bool foundStart = false;
                bool foundEnd = false;
                bool foundMultiverseNull = false;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == slot)
                    {
                        foundStart = false;
                        break;
                    }

                    if (slots[i].Card != null && slots[i].Card.HasAbility(AbilityID))
                        foundMultiverseNull = true;

                    if (slots[i].Card != null && slots[i].Card.HasConduitAbility())
                    {
                        if (i < slotIdx)
                        {
                            foundStart = true;
                            otherMultiverseConduits.Add(slots[i].Card);
                        }
                        else if (i > slotIdx)
                        {
                            foundEnd = true;
                            otherMultiverseConduits.Add(slots[i].Card);
                        }
                    }
                }
                if (foundStart && foundEnd && foundMultiverseNull)
                {
                    __result.AddRange(otherMultiverseConduits);
                }
            }
        }
    }
}
