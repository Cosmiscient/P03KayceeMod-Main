using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GemBlueLoot : Loot
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemBlueLoot()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Blue Mox Looter";
            info.rulebookDescription = "When [creature] deals damage directly, if you control a Blue Mox, draw a card for each damage dealt.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.SetExtendedProperty(AbilityIconBehaviours.BLUE_CELL, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemBlueLoot),
                TextureHelper.GetImageAsTexture("ability_bluegemloot.png", typeof(GemBlueLoot).Assembly)
            ).Id;

            info.SetAbilityRedirect("Blue Mox", Ability.GainGemBlue, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToDealDamageDirectly(int amount) => base.RespondsToDealDamageDirectly(amount) && Card.EligibleForGemBonus(GemType.Blue) && amount > 0;

        public override IEnumerator OnDealDamageDirectly(int amount)
        {
            if (CardDrawPiles.Instance is not CardDrawPiles3D)
            {
                yield return base.OnDealDamageDirectly(amount);
                yield break;
            }

            CardDrawPiles3D piles = CardDrawPiles3D.Instance;

            bool drewFromMainDeck = false;
            View oldView = ViewManager.Instance.CurrentView;
            ViewManager.Instance.SwitchToView(piles.pilesView);
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < amount; i++)
            {
                if (piles.Deck.CardsInDeck > 0 && (!drewFromMainDeck || piles.SideDeck.CardsInDeck == 0))
                {
                    drewFromMainDeck = true;
                    piles.pile.Draw();
                    yield return piles.DrawCardFromDeck();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (piles.SideDeck.CardsInDeck > 0 && (drewFromMainDeck || piles.Deck.CardsInDeck == 0))
                {
                    drewFromMainDeck = false;
                    piles.sidePile.Draw();
                    yield return piles.DrawFromSidePile();
                    yield return new WaitForSeconds(0.1f);
                }
            }

            ViewManager.Instance.SwitchToView(oldView);
            yield return new WaitForSeconds(0.4f);
        }
    }
}
