using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class DrawUpgrade : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static DrawUpgrade()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Combat Research";
            info.rulebookDescription = "When [creature] deals damage, create a Forced Upgrade card in hand. Forced Upgrade is defined as a spell that costs 2 energy and caused any target to upgrade to a new version.";
            info.canStack = false;
            info.powerLevel = 2;
            info.activated = false;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_drawupgrade.png", typeof(DrawUpgrade).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(DrawUpgrade),
                TextureHelper.GetImageAsTexture("ability_draw_upgrade.png", typeof(DrawUpgrade).Assembly)
            ).Id;
        }

        public override bool RespondsToDealDamage(int amount, PlayableCard target) => true;

        public override bool RespondsToDealDamageDirectly(int amount) => true;

        public override IEnumerator OnDealDamage(int amount, PlayableCard target) => OnDealDamageDirectly(amount);

        public override IEnumerator OnDealDamageDirectly(int amount)
        {
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_FORCED_UPGRADE"), null);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}
