using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class DrawBrittle : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static DrawBrittle()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Brittle Summoner";
            info.rulebookDescription = "When [creature] is played, a random brittle card is created in your hand.";
            info.canStack = false;
            info.powerLevel = 4;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(DrawBrittle),
                TextureHelper.GetImageAsTexture("ability_drawbrittle.png", typeof(DrawBrittle).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            yield return CreateDrawnCard();
            yield return LearnAbility();
        }

        protected IEnumerator CreateDrawnCard()
        {
            if (Singleton<ViewManager>.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                Singleton<ViewManager>.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            List<CardInfo> allCards = CardLoader.allData;
            List<CardInfo> cardsWithBrittle = new();

            //CardInfo cardObj = new CardInfo();

            //cardObj.AddAbilities(Ability.Brittle);

            //CardLoader.GetDistinctCardsFromPool(252535, 1, cardObj, 0, false);

            foreach (CardInfo card in allCards)
            {
                if (card != null)
                {
                    if (card.HasAbility(Ability.Brittle)
                        &&
                        (card.temple == CardTemple.Tech))
                    {
                        cardsWithBrittle.Add(card);

                        //Debug.Log(card.name + " ADDED");
                    }
                    else
                    {
                        //Debug.Log(card.name + " NOT ACCEPTED");
                    }

                    //Add the skeleton to the card pool
                    if (card.name == "Skeleton")
                    {
                        cardsWithBrittle.Add(card);
                    }
                }
            }

            CardInfo CardToDraw = cardsWithBrittle[Random.Range(0, cardsWithBrittle.Count)];

            yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardToDraw, null);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}