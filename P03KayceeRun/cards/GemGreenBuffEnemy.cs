using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class GemGreenBuffEnemy : AbilityBehaviour, IPassiveAttackBuff
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemGreenBuffEnemy()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Annoying Without Green";
            info.rulebookDescription = "The creature opposing [creature] gains 1 power unless the owner of [creature] also controls a Green Mox.";
            info.canStack = true;
            info.powerLevel = -1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.GREEN_CELL_INVERSE, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(GemGreenBuffEnemy),
                TextureHelper.GetImageAsTexture("ability_greengembuffenemy.png", typeof(GemGreenBuffEnemy).Assembly)
            ).Id;
        }

        public int GetPassiveAttackBuff(PlayableCard target) => Card.OnBoard && target.Slot == Card.Slot.opposingSlot && !Card.EligibleForGemBonus(GemType.Green) ? 1 : 0;
    }
}
