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
using InscryptionAPI.RuleBook;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class SacrificeSlime : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeSlime()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Guts Sacrifice";
            info.rulebookDescription = "Cards sacrificed to pay for [creature] slime the opposing slot when they die. Cards in slimed slots have one less power.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            SacrificeSlime.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeSlime),
                TextureHelper.GetImageAsTexture("ability_sacrifice_slime.png", typeof(SacrificeSlime).Assembly)
            ).Id;

            info.SetSlotRedirect("slime", SlimedSlot.ID, GameColors.Instance.limeGreen);
        }

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => !sacrifice.HasAbility(Ability.ExplodeOnDeath);

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            sacrifice.Status.hiddenAbilities.Add(ThrowSlimeOnDeath.AbilityID);
            sacrifice.AddTemporaryMod(new(ThrowSlimeOnDeath.AbilityID));
            yield break;
        }
    }
}
