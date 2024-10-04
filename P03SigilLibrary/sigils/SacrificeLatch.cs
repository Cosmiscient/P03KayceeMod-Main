using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class SacrificeLatch : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeLatch()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Emergence Latch";
            info.rulebookDescription = "When cards sacrificed to pay for [creature] die, their owner chooses a card to gain their first sigil.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            SacrificeLatch.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeLatch),
                TextureHelper.GetImageAsTexture("ability_latch_sacrifice.png", typeof(SacrificeLatch).Assembly)
            ).Id;
        }



        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => !sacrifice.HasAbility(Ability.ExplodeOnDeath);

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            sacrifice.Status.hiddenAbilities.Add(LatchFirst.AbilityID);
            sacrifice.AddTemporaryMod(new(LatchFirst.AbilityID));
            yield break;
        }
    }
}
