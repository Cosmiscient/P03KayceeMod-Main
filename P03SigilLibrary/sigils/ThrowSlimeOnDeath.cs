using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Slots;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ThrowSlimeOnDeath : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ThrowSlimeOnDeath()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Full of Guts";
            info.rulebookDescription = "When [creature] dies, it slimes the opposing slot.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ThrowSlimeOnDeath),
                TextureHelper.GetImageAsTexture("ability_slime_on_death.png", typeof(ThrowSlimeOnDeath).Assembly)
            ).Id;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => this.Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            yield return this.LearnAbility();
            yield return FullOfOil.ThrowOil(this.Card.Slot, this.Card.OpposingSlot(), 0.5f, GameColors.Instance.brightLimeGreen);
            yield return this.Card.OpposingSlot().SetSlotModification(SlimedSlot.ID);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
