using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RubyWarrior : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static RubyWarrior()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Orange Mox Warrior";
            info.rulebookDescription = "When [creature] is played, it attacks all enemy cards opposite Orange Mox providers its owner controls.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemOrange).Info.colorOverride;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RubyWarrior),
                TextureHelper.GetImageAsTexture("ability_ruby_warrior.png", typeof(RubyWarrior).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => BoardManager.Instance.GetSlots(!Card.OpponentCard).Any(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemOrange) || s.Card.HasAbility(Ability.GainGemTriple)));

        public override IEnumerator OnResolveOnBoard()
        {
            List<CardSlot> cardSlots = BoardManager.Instance.GetSlots(!Card.OpponentCard);

            foreach (CardSlot slot in cardSlots)
            {
                if (slot.Card != null && (slot.Card.HasAbility(Ability.GainGemOrange) || slot.Card.HasAbility(Ability.GainGemTriple)))
                {
                    if (slot.opposingSlot.Card == null)
                        continue;

                    // Borrowed this code from ActivatedDealDamage
                    bool impactFrameReached = false;
                    Card.Anim.PlayAttackAnimation(false, slot.opposingSlot, delegate ()
                    {
                        impactFrameReached = true;
                    });
                    yield return new WaitUntil(() => impactFrameReached);
                    yield return slot.opposingSlot.Card.TakeDamage(Card.Attack, Card);
                    yield return new WaitForSeconds(0.25f);
                }
            }

            yield break;
        }
    }
}