using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class CellGemDraw : GemsDraw
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static CellGemDraw()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Mental Gymnastics When Powered";
            info.rulebookDescription = "When [creature] is played, if it is within a circuit, its owner draw cards equal to the number of Mox Cards on their side of the game board.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(CellGemDraw),
                TextureHelper.GetImageAsTexture("ability_cell_gymnastics.png", typeof(CellGemDraw).Assembly)
            ).Id;
        }

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return base.RespondsToOtherCardResolve(otherCard) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);
        }
    }
}
