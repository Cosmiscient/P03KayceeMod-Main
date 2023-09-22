using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class EnergySiphon : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static EnergySiphon()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Energy Siphon";
            info.rulebookDescription = "Whenever another card you own dies in combat, [creature] creates a Charge in your hand. Charge is defined as a spell that refills 1 Energy when played.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(EnergySiphon),
                TextureHelper.GetImageAsTexture("ability_draw_charge.png", typeof(EnergySiphon).Assembly)
            ).Id;
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => Card.OnBoard && fromCombat && card.OpponentCard == Card.OpponentCard;

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return PreSuccessfulTriggerSequence();
            Card.Anim.StrongNegationEffect();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(ExpansionPackCards_2.CHARGE_CARD), null);
            yield return new WaitForSeconds(0.25f);
            yield return LearnAbility();
        }
    }
}
