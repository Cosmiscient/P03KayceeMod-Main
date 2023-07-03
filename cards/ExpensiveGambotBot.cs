using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using System.Linq;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ExpensiveActivatedRandomPowerEnergy : ActivatedRandomPowerEnergy
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ExpensiveActivatedRandomPowerEnergy()
        {
            AbilityInfo original = AbilityManager.BaseGameAbilities.First(fa => fa.Info.ability == Ability.ActivatedRandomPowerEnergy).Info;

            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = original.rulebookName;
            info.rulebookDescription = original.rulebookDescription.Replace("1 Energy", "3 Energy");
            info.canStack = false;
            info.powerLevel = original.powerLevel - 1;
            info.opponentUsable = original.opponentUsable;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ExpensiveActivatedRandomPowerEnergy.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ExpensiveActivatedRandomPowerEnergy),
                TextureHelper.GetImageAsTexture("ActivatedRandomPowerEnergy-3Energy.png", typeof(ExpensiveActivatedRandomPowerEnergy).Assembly)
            ).Id;
        }

        public override int EnergyCost => 3;
    }
}