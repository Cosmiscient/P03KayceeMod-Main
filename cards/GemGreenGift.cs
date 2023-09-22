using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GemGreenGift : DrawRandomCardOnDeath
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemGreenGift()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gift With Emerald";
            info.rulebookDescription = "When [creature] perishes, if you control an Emerald, gain a random card.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.GREEN_CELL, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.darkPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(GemGreenGift),
                TextureHelper.GetImageAsTexture("ability_greengemgift.png", typeof(GemGreenGift).Assembly)
            ).Id;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => base.RespondsToDie(wasSacrifice, killer) && Card.EligibleForGemBonus(GemType.Green);
    }
}
