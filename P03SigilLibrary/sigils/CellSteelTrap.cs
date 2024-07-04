using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class CellSteelTrap : BetterSteelTrap
    {
        public static new Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static CellSteelTrap()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Trap When Powered";
            info.rulebookDescription = "When a card bearing this sigil perishes while in a circuit, the creature opposing it perishes as well. A Vessel is created in your hand.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };


            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(CellSteelTrap),
                TextureHelper.GetImageAsTexture("ability_cellsteeltrap.png", typeof(CellExplodonate).Assembly)
            ).Id;
        }

        public override bool RespondsToTakeDamage(PlayableCard source) => base.RespondsToTakeDamage(source) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => base.RespondsToDie(wasSacrifice, killer) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => base.RespondsToPreDeathAnimation(wasSacrifice) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);
    }
}