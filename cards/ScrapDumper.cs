using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
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
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ScrapDumper.AbilityID = AbilityManager.Add(
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
            CardSlot target = this.Card.slot.opposingSlot;


            if (target == null || target.Card != null)
            {
                this.Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
            CardInfo familiar = CardLoader.GetCardByName("BrokenBot");

            yield return BoardManager.Instance.CreateCardInSlot(familiar, this.Card.slot.opposingSlot);
            yield return new WaitForSeconds(0.25f);

        }

        
    }
}
