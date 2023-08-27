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
            info.rulebookName = "Ruby Warrior";
            info.rulebookDescription = "When [creature] is played, it attacks all enemy cards opposite Ruby Providers its owner controls.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemOrange).Info.colorOverride;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            RubyWarrior.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RubyWarrior),
                TextureHelper.GetImageAsTexture("ability_ruby_warrior.png", typeof(RubyWarrior).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => BoardManager.Instance.GetSlots(!this.Card.OpponentCard).Any(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemOrange) || s.Card.HasAbility(Ability.GainGemTriple)));

        public override IEnumerator OnResolveOnBoard()
        {
            var cardSlots = BoardManager.Instance.GetSlots(!this.Card.OpponentCard);

            DiskCardWeapon defaultWeapon = DiskCardWeapon.Default;
            if (base.Card.Anim is DiskCardAnimationController dac)
            {
                defaultWeapon = (DiskCardWeapon)dac.weaponMeshes.FindIndex(m => m.name == dac.weaponMeshFilter.mesh.name);
                dac.SetWeaponMesh(DiskCardWeapon.Revolver);
            }

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

            if (base.Card.Anim is DiskCardAnimationController dac2)
            {
                dac2.SetWeaponMesh(defaultWeapon);
            }

            yield break;
        }
    }
}