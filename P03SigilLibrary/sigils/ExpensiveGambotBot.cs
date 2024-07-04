using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using System.Linq;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
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
            info.rulebookDescription = original.rulebookDescription.Replace("1 Energy", "2 Energy");
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel - 1;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = original.passive;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ExpensiveActivatedRandomPowerEnergy.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ExpensiveActivatedRandomPowerEnergy),
                TextureHelper.GetImageAsTexture("ActivatedRandomPowerEnergy-2Energy.png", typeof(ExpensiveActivatedRandomPowerEnergy).Assembly)
            ).Id;
        }

        public override int EnergyCost => 2;

        public override bool RespondsToUpkeep(bool playerUpkeep) => !playerUpkeep && Card.OpponentCard;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (!playerUpkeep && Card.OpponentCard)
            {
                yield return Activate();
            }
        }
    }
}