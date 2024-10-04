using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GemOrangePrinter : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static GemOrangePrinter()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Orange Mox Printer";
            info.rulebookDescription = "If you control an Orange Mox, [creature] will draw a card from your side deck at the start of every turn.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.SetExtendedProperty(AbilityIconBehaviours.ORANGE_CELL, true);
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = GameColors.Instance.lightPurple;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(GemOrangePrinter),
                TextureHelper.GetImageAsTexture("ability_orangegemprinter.png", typeof(GemOrangePrinter).Assembly)
            ).Id;

            info.SetAbilityRedirect("Orange Mox", Ability.GainGemOrange, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == this.Card.IsPlayerCard() && this.Card.EligibleForGemBonus(GemType.Orange);

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return base.PreSuccessfulTriggerSequence();
            this.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.4f);
            if (BoardManager.Instance is BoardManager3D)
            {
                if (ViewManager.Instance.CurrentView != View.Default)
                {
                    yield return new WaitForSeconds(0.2f);
                    ViewManager.Instance.SwitchToView(View.Default, false, false);
                    yield return new WaitForSeconds(0.2f);
                }
                CardDrawPiles3D.Instance.SidePile.Draw();
                yield return CardDrawPiles3D.Instance.DrawFromSidePile();
            }
            else
            {
                yield return CardDrawPiles.Instance.DrawCardFromDeck();
            }
            yield return base.LearnAbility(0.5f);
            yield break;
        }
    }
}
