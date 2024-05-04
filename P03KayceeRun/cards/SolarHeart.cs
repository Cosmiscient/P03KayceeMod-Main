using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class SolarHeart : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public class SolarHeartSlot : NonCardTriggerReceiver, ISlotModificationChanged
        {
            public IEnumerator OnSlotModificationChanged(CardSlot slot, SlotModificationManager.ModificationType previous, SlotModificationManager.ModificationType current)
            {
                if (current == SlotModID)
                {
                    if (slot.Card != null && !slot.WithinConduitCircuit)
                        slot.Card.RenderCard();

                    slot.SetWithinConduitCircuit(true);

                    yield return new WaitForEndOfFrame();
                }
            }

            public bool RespondsToSlotModificationChanged(CardSlot slot, SlotModificationManager.ModificationType previous, SlotModificationManager.ModificationType current) => true;
        }

        public static SlotModificationManager.ModificationType SlotModID { get; private set; }

        static SolarHeart()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Solar Heart";
            info.rulebookDescription = "When [creature] dies, its heart stays behind and provides conduit power to the slot.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_solar_heart.png", typeof(SolarHeart).Assembly));
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SolarHeart),
                TextureHelper.GetImageAsTexture("ability_solar_heart.png", typeof(SolarHeart).Assembly)
            ).Id;

            SlotModID = SlotModificationManager.New(
                P03Plugin.PluginGuid,
                "SolarHeart",
                typeof(SolarHeartSlot),
                null
            );
        }

        private CardSlot oldSlot = null;

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            oldSlot = Card.Slot;
            yield break;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (oldSlot != null)
                yield return oldSlot.SetSlotModification(SlotModID);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.UpdateCircuitsForSlots))]
        [HarmonyPostfix]
        private static void UpdateCircuitsForModdedSlots(List<CardSlot> slots)
        {
            foreach (CardSlot slot in slots)
            {
                if (slot.GetSlotModification() == SlotModID)
                {
                    if (slot.Card != null && !slot.WithinConduitCircuit)
                        slot.Card.RenderCard();

                    slot.SetWithinConduitCircuit(true);
                }
            }
        }

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.SlotIsWithinCircuit))]
        [HarmonyPrefix]
        private static bool SolarHeartWithin(CardSlot slot, ref bool __result)
        {
            if (slot.GetSlotModification() == SlotModID)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.GetConduitsForSlot))]
        [HarmonyPostfix]
        private static void UpdateWithSolarHeart(CardSlot slot, ref List<PlayableCard> __result)
        {
            if (slot.GetSlotModification() == SlotModID && slot.Card != null && !__result.Contains(slot.Card))
                __result.Add(slot.Card);
        }
    }
}
