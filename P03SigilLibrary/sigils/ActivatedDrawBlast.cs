using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedDrawBlast : ActivatedAbilityBehaviour, IFuelCostActivation
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public int FuelCost => 1;

        static ActivatedDrawBlast()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Cannon Fire";
            info.rulebookDescription = "Activate: Create a Blast! card in hand. Blast! is defined as a spell that costs 2 energy and sets the target on fire.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedDrawBlast),
                TextureHelper.GetImageAsTexture("ability_activated_draw_blast.png", typeof(ActivatedDrawBlast).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_BLAST"), null);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}
