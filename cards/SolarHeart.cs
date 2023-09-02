using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
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
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SolarHeart.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SolarHeart),
                TextureHelper.GetImageAsTexture("ability_solar_heart.png", typeof(SolarHeart).Assembly)
            ).Id;

            SolarHeart.SlotModID = SlotModificationManager.New(
                P03Plugin.PluginGuid,
                "SolarHeart",
                typeof(SolarHeartSlot),
                null
            );
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return this.Card.Slot.SetSlotModification(SlotModID);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.UpdateCircuitsForSlots))]
        [HarmonyPostfix]
        private static void UpdateCircuitsForModdedSlots(List<CardSlot> slots)
        {
            foreach (var slot in slots)
            {
                if (slot.GetSlotModification() == SlotModID)
                {
                    if (slot.Card != null && !slot.WithinConduitCircuit)
                        slot.Card.RenderCard();

                    slot.SetWithinConduitCircuit(true);
                }
            }
        }
    }
}
