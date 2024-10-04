using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class LatchFirst : Latch
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability LatchAbility
        {
            get
            {
                List<Ability> possibles = this.Card.AllAbilities().Where(a => a != AbilityID).ToList();
                if (possibles.Count == 0)
                    return Ability.Sharp;
                return possibles[0];
            }
        }

        static LatchFirst()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "First Latch";
            info.rulebookDescription = "When [creature] perishes, its owner chooses a creature to gain the first sigil this card has.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(LatchFirst),
                TextureHelper.GetImageAsTexture("ability_latch_hidden.png", typeof(LatchFirst).Assembly)
            ).Id;
        }
    }
}