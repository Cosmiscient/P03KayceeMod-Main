using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Spells.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class MoveBesideAndFuel : MoveBeside
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static MoveBesideAndFuel()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Refueler";
            info.rulebookDescription = "As long as it has fuel, [creature] will attempt to move beside any creatures you play that also use fuel. Each turn, [creature] will refuel adjacent creatures.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.SetExtendedProperty(AbilityIconBehaviours.ACTIVE_WHEN_FUELED, true);
            info.colorOverride = GameColors.Instance.darkLimeGreen;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(MoveBesideAndFuel),
                TextureHelper.GetImageAsTexture("ability_movebeside_refuel.png", typeof(MoveBesideAndFuel).Assembly)
            ).Id;
        }

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return base.RespondsToOtherCardResolve(otherCard) && otherCard.Info.GetStartingFuel() > 0 && this.Card.GetCurrentFuel() > 0;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => this.Card.IsPlayerCard() == playerTurnEnd && this.Card.GetCurrentFuel() > 0;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            foreach (var slot in BoardManager.Instance.GetAdjacentSlots(this.Card.Slot).Where(s => s.Card != null))
            {
                if (slot.Card.Info.GetStartingFuel() > 0 && slot.Card.GetCurrentFuel() < FuelExtensions.MAX_FUEL)
                {
                    if (this.Card.TrySpendFuel(1))
                    {
                        this.Card.Anim.StrongNegationEffect();
                        slot.Card.Anim.StrongNegationEffect();
                        slot.Card.AddFuel();
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }
    }
}
