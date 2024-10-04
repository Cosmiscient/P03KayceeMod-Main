using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Slots;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FullyLoaded : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public class FullyLoadedSlot : SlotModificationBehaviour, IPassiveAttackBuff
        {
            public int GetPassiveAttackBuff(PlayableCard target)
            {
                return Slot.Card == target ? 1 : 0;
            }
        }

        public static SlotModificationManager.ModificationType SlotModID { get; private set; }

        static FullyLoaded()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fully Loaded";
            info.rulebookDescription = "When [creature] dies, it leaves a permanent +1 attack bonus in the lane it occupied.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_fully_loaded.png", typeof(FullyLoaded).Assembly));
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(FullyLoaded),
                TextureHelper.GetImageAsTexture("ability_fully_loaded.png", typeof(FullyLoaded).Assembly)
            ).Id;

            info.SetSlotRedirect("attack bonus", SlotModID, GameColors.Instance.limeGreen);

            SlotModID = SlotModificationManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                "FullyLoaded",
                typeof(FullyLoadedSlot),
                TextureHelper.GetImageAsTexture("cardslot_fully_loaded.png", typeof(FullyLoaded).Assembly),
                TextureHelper.GetImageAsTexture("pixel_slot_fully_loaded.png", typeof(FullyLoaded).Assembly)
            ).SetRulebook(
                "Fully Loaded",
                "The card in this slot has its power increased by one.",
                TextureHelper.GetImageAsTexture("cardslot_fully_loaded.png", typeof(FullyLoaded).Assembly),
                    SlotModificationManager.ModificationMetaCategory.Part1Rulebook,
                    SlotModificationManager.ModificationMetaCategory.Part3Rulebook
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
    }
}
