using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ScrapDumper : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ScrapDumper()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Scrap Dumper";
            info.rulebookDescription = "When a card bearing this sigil is played, a broken bot is played on the opposing space.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ScrapDumper),
                TextureHelper.GetImageAsTexture("ability_scrapdumper.png", typeof(ScrapDumper).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            CardSlot target = Card.slot.opposingSlot;


            if (target == null || target.Card != null)
            {
                Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
            CardInfo familiar = CardLoader.GetCardByName("BrokenBot");

            yield return BoardManager.Instance.CreateCardInSlot(familiar, Card.slot.opposingSlot);
            yield return new WaitForSeconds(0.25f);

        }


    }
}
