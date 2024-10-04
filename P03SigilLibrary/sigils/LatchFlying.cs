using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class LatchFlying : Latch
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability LatchAbility => Ability.Flying;

        static LatchFlying()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Airborne Latch";
            info.rulebookDescription = "When [creature] perishes, its owner chooses a creature to gain the Airborne sigil.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(LatchFlying),
                TextureHelper.GetImageAsTexture("ability_latch_flying.png", typeof(LatchFlying).Assembly)
            ).Id;

            info.SetAbilityRedirect("Airborne", Ability.Flying, GameColors.Instance.limeGreen);
        }
    }
}