using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ActivatedGainPower : ActivatedAbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public const int ENERGY_COST = 6;

        static ActivatedGainPower()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Overcharge";
            info.rulebookDescription = $"Pay {ENERGY_COST} Energy to increase the Power of this card by 1";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ActivatedGainPower),
                TextureHelper.GetImageAsTexture("ability_activatedexpensivepowerup.png", typeof(TakeDamageSigil).Assembly)
            ).Id;
        }

        public override int EnergyCost => ENERGY_COST;

        public override IEnumerator Activate()
        {
            CardModificationInfo cardModificationInfo = Card.TemporaryMods.Find((x) => x.singletonId == "statsUp");
            if (cardModificationInfo == null)
            {
                cardModificationInfo = new CardModificationInfo
                {
                    singletonId = "statsUp"
                };
                Card.AddTemporaryMod(cardModificationInfo);
            }
            cardModificationInfo.attackAdjustment++;
            Card.OnStatsChanged();
            yield return new WaitForSeconds(0.25f);
        }

    }
}
