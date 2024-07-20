using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GemGreenGift : DrawRandomCardOnDeath
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemGreenGift()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Green Mox Gift";
            info.rulebookDescription = "When [creature] perishes, if you control a Green Mox, gain a random card.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.GREEN_CELL, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemGreenGift),
                TextureHelper.GetImageAsTexture("ability_greengemgift.png", typeof(GemGreenGift).Assembly)
            ).Id;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => base.RespondsToDie(wasSacrifice, killer) && Card.EligibleForGemBonus(GemType.Green);
    }
}
