using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class Overheat : AbilityBehaviour, InscryptionAPI.Triggers.IPassiveAttackBuff
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static Overheat()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Overheat";
            info.rulebookDescription = "When the scales are in your favor, [creature] loses 1 power. When the scales aren't in your favor, [creature] gains 1 power.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Overheat),
                TextureHelper.GetImageAsTexture("ability_overheat.png", typeof(Overheat).Assembly)
            ).Id;
        }

        public int GetPassiveAttackBuff(PlayableCard currentCard)
        {
            if (currentCard != Card) return 0;

            if (LifeManager.Instance.DamageUntilPlayerWin > 5)
            {
                return 1;
            }

            if (LifeManager.Instance.DamageUntilPlayerWin < 5)
            {
                return -1;
            }

            return LifeManager.Instance.DamageUntilPlayerWin == 5 ? 0 : 0;
        }
    }
}
