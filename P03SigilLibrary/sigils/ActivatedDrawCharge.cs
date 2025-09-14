using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedDrawCharge : ActivatedAbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public const int ENERGY_COST = 2;

        static ActivatedDrawCharge()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Recharge Reserves";
            info.rulebookDescription = $"Pay {ENERGY_COST} Energy to create a Charge in your hand. Charge is defined as a spell that refills 1 Energy when played.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_activated_drawcharge.png", typeof(ActivatedDrawCharge).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedDrawCharge),
                TextureHelper.GetImageAsTexture("ability_store_charge.png", typeof(ActivatedDrawCharge).Assembly)
            ).Id;
        }

        public override int EnergyCost => ENERGY_COST;

        public override IEnumerator Activate()
        {
            Card.Anim.StrongNegationEffect();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_CHARGE"), null);
            yield return new WaitForSeconds(0.25f);
            yield return LearnAbility();
        }

    }
}
