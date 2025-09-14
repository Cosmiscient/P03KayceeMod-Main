using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedDrawDefend : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int EnergyCost => 2;
        private bool hasActivatedThisTurn = false;

        public override bool CanActivate() => !hasActivatedThisTurn;

        static ActivatedDrawDefend()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Holo Weaver";
            info.rulebookDescription = "Once per turn, create a Defend! card in hand. Defend! is defined as a spell that costs 2 energy and gives the target a shield.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedDrawDefend),
                TextureHelper.GetImageAsTexture("ability_activated_draw_defend.png", typeof(ActivatedDrawDefend).Assembly)
            ).Id;

            info.SetAbilityRedirect("shield", Ability.DeathShield, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == this.Card.IsPlayerCard();
        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            hasActivatedThisTurn = false;
            yield break;
        }

        public override IEnumerator Activate()
        {
            hasActivatedThisTurn = true;
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_DEFEND"), null);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}
