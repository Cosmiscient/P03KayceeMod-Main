using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class AllControl : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; set; }

        static AllControl()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Parasitic Control";
            info.rulebookDescription = "When [creature] is played, all opposing cards are moved to your side of the board.";
            info.canStack = false;
            info.powerLevel = 6;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(AllControl),
                TextureHelper.GetImageAsTexture("ability_control_all.png", typeof(AllControl).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            foreach (var slot in BoardManager.Instance.OpponentSlotsCopy)
            {
                if (slot.Card == null)
                    continue;

                if (slot.opposingSlot.Card != null)
                    continue;

                this.Card.Anim.StrongNegationEffect();
                yield return TemporaryControl.ReverseOwnerOfCard(slot.Card);
            }
        }
    }
}