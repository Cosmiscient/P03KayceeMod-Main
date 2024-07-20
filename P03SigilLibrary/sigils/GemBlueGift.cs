using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GemBlueGift : DrawRandomCardOnDeath
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemBlueGift()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Blue Mox Gift";
            info.rulebookDescription = "When [creature] perishes, if you control an Blue Mox, gain a random card.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.BLUE_CELL, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemBlueGift),
                TextureHelper.GetImageAsTexture("ability_bluegemgift.png", typeof(GemBlueGift).Assembly)
            ).Id;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => base.RespondsToDie(wasSacrifice, killer) && Card.EligibleForGemBonus(GemType.Blue);
    }
}
