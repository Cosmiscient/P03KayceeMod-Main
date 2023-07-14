using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class ConduitNeighbor : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ConduitNeighbor()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Static Electricity";
            info.rulebookDescription = "[creature] will provide cause adjacent cards to behave as if they are part of a completed conduit.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ConduitNeighbor.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitNeighbor),
                TextureHelper.GetImageAsTexture("ability_staticelectricity.png", typeof(ConduitNeighbor).Assembly)
            ).Id;
        }

        //This code makes sure that sigils that only activate within conduits work properly
        [HarmonyPatch(typeof(ConduitCircuitManager))]
        [HarmonyPatch(nameof(ConduitCircuitManager.SlotIsWithinCircuit))]
        [HarmonyPrefix]
        private static bool SatisfyConduitSigils(ConduitCircuitManager __instance, ref bool __result, CardSlot slot)
        {
            __result = __instance.GetConduitsForSlot(slot).Count > 0;

            CardSlot toLeft = Singleton<BoardManager>.Instance.GetAdjacent(slot, adjacentOnLeft: true);
            CardSlot toRight = Singleton<BoardManager>.Instance.GetAdjacent(slot, adjacentOnLeft: false);

            //If adjacent to conduit neighbor, slot is within circuit
            if (toLeft != null)
            {
                if (toLeft.Card != null)
                {
                    if (toLeft.Card.HasAbility(ConduitNeighbor.AbilityID))
                    {
                        __result = true;
                    }
                }
            }

            if (toRight != null)
            {
                if (toRight.Card != null)
                {
                    if (toRight.Card.HasAbility(ConduitNeighbor.AbilityID))
                    {
                        __result = true;
                    }
                }
            }

            return false; // Skip the original method
        }

        //This code shows the circuit effect visually
        [HarmonyPatch(typeof(ConduitCircuitManager))]
        [HarmonyPatch(nameof(ConduitCircuitManager.UpdateCircuitsForSlots))]
        [HarmonyPrefix]
        private static bool CircuitEffect(ConduitCircuitManager __instance, List<CardSlot> slots)
        {
            foreach (CardSlot slot in slots)
            {
                if ((__instance.GetConduitsForSlot(slot).Count > 1) || __instance.SlotIsWithinCircuit(slot))
                {
                    if (slot.Card != null && !slot.WithinConduitCircuit)
                    {
                        slot.Card.RenderCard();
                    }
                    slot.SetWithinConduitCircuit(inCircuit: true);
                }
                else
                {
                    if (slot.Card != null && slot.WithinConduitCircuit)
                    {
                        slot.Card.RenderCard();
                    }
                    slot.SetWithinConduitCircuit(inCircuit: false);
                }
            }

            return false; // Skip the original method
        }

        [HarmonyPatch(typeof(ConduitCircuitManager))]
        [HarmonyPatch(nameof(ConduitCircuitManager.GetConduitsForSlot))]
        [HarmonyPrefix]
        private static bool ConduitNeighborAsConduit(ConduitCircuitManager __instance, ref List<PlayableCard> __result, CardSlot slot)
        {
            List<CardSlot> slots = Singleton<BoardManager>.Instance.GetSlots(slot.IsPlayerSlot);
            int num = slots.IndexOf(slot);
            List<PlayableCard> list = new List<PlayableCard>();
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Card != null && slots[i].Card.HasConduitAbility())
                {
                    if (i < num)
                    {
                        flag = true;
                        list.Add(slots[i].Card);
                    }
                    else if (i > num)
                    {
                        flag2 = true;
                        list.Add(slots[i].Card);
                    }
                }
            }
            if (!flag || !flag2)
            {
                list.Clear();
            }

            CardSlot toLeft = Singleton<BoardManager>.Instance.GetAdjacent(slot, adjacentOnLeft: true);
            CardSlot toRight = Singleton<BoardManager>.Instance.GetAdjacent(slot, adjacentOnLeft: false);

            //If slot is adjacent to conduit neighbor, add conduit neighbor card to list of conduits
            if (toLeft != null)
            {
                if (toLeft.Card != null)
                {
                    if (toLeft.Card.HasAbility(ConduitNeighbor.AbilityID))
                    {
                        list.Add(toLeft.Card);
                    }
                }
            }

            if (toRight != null)
            {
                if (toRight.Card != null)
                {
                    if (toRight.Card.HasAbility(ConduitNeighbor.AbilityID))
                    {
                        list.Add(toRight.Card);
                    }
                }
            }

            __result = list;

            return false; // Skip the original method
        }


    }
}
