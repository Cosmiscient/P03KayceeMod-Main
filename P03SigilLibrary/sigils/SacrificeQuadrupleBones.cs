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
    public class SacrificeQuadrupleBones : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeQuadrupleBones()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Shatter";
            info.rulebookDescription = "Cards sacrificed to pay for [creature] drop 4 bones instead of 1 when they die.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            SacrificeQuadrupleBones.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeQuadrupleBones),
                TextureHelper.GetImageAsTexture("ability_sacrifice_quadbones.png", typeof(SacrificeQuadrupleBones).Assembly)
            ).Id;
        }



        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => !sacrifice.HasAbility(Ability.ExplodeOnDeath);

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            sacrifice.Status.hiddenAbilities.Add(Ability.QuadrupleBones);
            sacrifice.AddTemporaryMod(new(Ability.QuadrupleBones));
            yield break;
        }
    }
}
