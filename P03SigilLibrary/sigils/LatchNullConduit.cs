using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class LatchNullConduit : Latch
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability LatchAbility => Ability.ConduitNull;

        static LatchNullConduit()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Conduit Latch";
            info.rulebookDescription = "When [creature] perishes, its owner chooses a creature to gain the Null Conduit sigil.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.conduit = true;
            info.passive = false;
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_latch_conduit.png", typeof(LatchNullConduit).Assembly));
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(LatchNullConduit),
                TextureHelper.GetImageAsTexture("ability_latch_nullconduit.png", typeof(LatchNullConduit).Assembly)
            ).Id;
        }
    }
}