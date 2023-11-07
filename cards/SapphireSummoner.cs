using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class SapphireEnergy : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static SapphireEnergy()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Blue Mox Charger";
            info.rulebookDescription = "When [creature] is played, it creates a Charge in your hand for each Blue Mox its owner controls. Charge is a spell that refills 1 Energy when played.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemBlue).Info.colorOverride;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SapphireEnergy.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SapphireEnergy),
                TextureHelper.GetImageAsTexture("ability_sapphire_charge.png", typeof(SapphireEnergy).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => BoardManager.Instance.GetSlots(!this.Card.OpponentCard).Any(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemBlue) || s.Card.HasAbility(Ability.GainGemTriple)));

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            Card.Anim.StrongNegationEffect();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return LearnAbility();
            foreach (var slot in BoardManager.Instance.GetSlots(!this.Card.OpponentCard).Where(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemBlue) || s.Card.HasAbility(Ability.GainGemTriple))))
            {
                yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(ExpansionPackCards_2.CHARGE_CARD), null);
                yield return new WaitForSeconds(0.25f);
            }

            yield break;
        }
    }
}