using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class FlameStoker : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static FlameStoker()
        {
            AbilityInfo fsInfo = ScriptableObject.CreateInstance<AbilityInfo>();
            fsInfo.rulebookName = "Flame Stoker";
            fsInfo.rulebookDescription = "While [creature] is on board, all fires you start will be stronger, causing them to last one turn longer.";
            fsInfo.canStack = false;
            fsInfo.powerLevel = 1;
            fsInfo.opponentUsable = true;
            fsInfo.passive = true;
            fsInfo.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, BurningSlotBase.FlamingAbility };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                fsInfo,
                typeof(FireBomb),
                TextureHelper.GetImageAsTexture("ability_flame_stoker.png", typeof(FireBomb).Assembly)
            ).Id;

            fsInfo.SetSlotRedirect("fires", BurningSlotBase.GetFireLevel(2), GameColors.Instance.limeGreen);
        }
    }
}