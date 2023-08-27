using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class SapphireSummoner : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static SapphireSummoner()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Ruby Warrior";
            info.rulebookDescription = "When [creature] is played, it attacks all enemy cards opposite Ruby Providers its owner controls.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemBlue).Info.colorOverride;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SapphireSummoner.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SapphireSummoner),
                TextureHelper.GetImageAsTexture("ability_sapphire_summoner.png", typeof(SapphireSummoner).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => BoardManager.Instance.GetSlots(!this.Card.OpponentCard).Any(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemBlue) || s.Card.HasAbility(Ability.GainGemTriple)));

        public override IEnumerator OnResolveOnBoard()
        {
            var cardSlots = BoardManager.Instance.GetSlots(!this.Card.OpponentCard);

            foreach (var slot in cardSlots)
            {
                if (slot.Card != null && (slot.Card.HasAbility(Ability.GainGemOrange) || slot.Card.HasAbility(Ability.GainGemTriple)))
                {
                    if (slot.opposingSlot.Card == null)
                        continue;

                    // Borrowed this code from ActivatedDealDamage
                    bool impactFrameReached = false;
                    this.Card.Anim.PlayAttackAnimation(false, slot.opposingSlot, delegate ()
                    {
                        impactFrameReached = true;
                    });
                    yield return new WaitUntil(() => impactFrameReached);
                    yield return slot.opposingSlot.Card.TakeDamage(this.Card.Attack, this.Card);
                    yield return new WaitForSeconds(0.25f);
                }
            }



            yield break;
        }
    }
}