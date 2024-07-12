using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class DrawTwoZap : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static DrawTwoZap()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Phasers Ready";
            info.rulebookDescription = "When [creature] is played, create two Zap! cards in hand. Zap! is defined as a spell that costs 2 energy and deals 1 damage to any target.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_drawtwozap.png", typeof(DrawTwoZap).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(DrawTwoZap),
                TextureHelper.GetImageAsTexture("ability_drawtwozap.png", typeof(DrawTwoZap).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_ZAP"), null);
            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_ZAP"), null);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}