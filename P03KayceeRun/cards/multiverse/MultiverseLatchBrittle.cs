using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseBrittleLatch : MultiverseLatchBase
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;
        public override Ability LatchAbility => MultiverseExplodeOnDeath.AbilityID;

        static MultiverseBrittleLatch()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.LatchBrittle);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When [creature] perishes, its owner chooses a creature in any universe to gain the Brittle sigil.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = Color.black;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseBrittleLatch),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_latchbrittle")
            ).Id;
        }
    }
}
