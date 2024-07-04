using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class LatchSwapper : Latch
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override Ability LatchAbility => Ability.SwapStats;

        static LatchSwapper()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Swapper Latch";
            info.rulebookDescription = "When [creature] perishes, its owner chooses a creature to gain the Swapper sigil. The target will immediately swap stats once when the latch is applied.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_latch_swapper.png", typeof(LatchSwapper).Assembly));
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(LatchSwapper),
                TextureHelper.GetImageAsTexture("ability_latch_swapper.png", typeof(LatchSwapper).Assembly)
            ).Id;
        }

        private PlayableCard lastTarget = null;

        public override void OnSuccessfullyLatched(PlayableCard target) => lastTarget = target;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            yield return base.OnPreDeathAnimation(wasSacrifice);
            if (lastTarget != null)
            {
                yield return lastTarget.GetComponent<SwapStats>().OnTakeDamage(lastTarget);
            }
        }
    }
}