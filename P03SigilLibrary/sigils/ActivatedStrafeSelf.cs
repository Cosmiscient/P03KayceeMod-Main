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
    public class ActivatedStrafeSelf : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override bool CanActivate() => false;
        public override IEnumerator Activate()
        {
            yield break;
        }

        static ActivatedStrafeSelf()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "D-Pad";
            info.rulebookDescription = "Move to an adjacent empty lane.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedStrafeSelf),
                TextureHelper.GetImageAsTexture("ability_activated_strafe.png", typeof(ActivatedStrafeSelf).Assembly)
            ).Id;
        }

        public override bool RespondsToDrawn() => true;
        public override bool RespondsToOtherCardResolve(PlayableCard otherCard) => otherCard == this.Card;

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            yield return OnDrawn();
        }

        public override IEnumerator OnDrawn()
        {
            if (PlayerHand.Instance is PlayerHand3D p3d)
            {
                p3d.MoveCardAboveHand(this.Card);
                yield return this.Card.FlipInHand(this.AddMod);
            }
            else
            {
                this.AddMod();
            }
            yield return this.LearnAbility(0.5f);

        }

        private void AddMod()
        {
            base.Card.Status.hiddenAbilities.Add(this.Ability);
            CardModificationInfo mod = new();
            mod.abilities.Add(ActivatedStrafeSelfLeft.AbilityID);
            mod.abilities.Add(ActivatedStrafeSelfRight.AbilityID);
            mod.negateAbilities.Add(AbilityID);
            this.Card.AddTemporaryMod(mod);
        }
    }
}

