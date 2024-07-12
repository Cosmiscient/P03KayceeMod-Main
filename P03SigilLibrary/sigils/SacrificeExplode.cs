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
    public class SacrificeExplode : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeExplode()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Burst";
            info.rulebookDescription = "Cards sacrificed to pay for [creature] explode, dealing 10 damage to each adjacent card.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SacrificeExplode.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeExplode),
                TextureHelper.GetImageAsTexture("ability_sacrifice_explode.png", typeof(SacrificeExplode).Assembly)
            ).Id;
        }



        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => !sacrifice.HasAbility(Ability.ExplodeOnDeath);

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            sacrifice.Status.hiddenAbilities.Add(Ability.ExplodeOnDeath);
            sacrifice.AddTemporaryMod(new(Ability.ExplodeOnDeath));
            yield break;
        }
    }
}
