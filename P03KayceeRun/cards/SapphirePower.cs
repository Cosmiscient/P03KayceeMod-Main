using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class SapphirePower : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public static int NumberOfActiveAbilities = 0;

        static SapphirePower()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Sapphire Blessing";
            info.rulebookDescription = "[creature] reduces the cost of all cards in your hand by 1.";
            info.canStack = false;
            info.powerLevel = 5;
            info.opponentUsable = false;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemBlue).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SapphirePower),
                TextureHelper.GetImageAsTexture("ability_sapphire_power.png", typeof(SapphirePower).Assembly)
            ).Id;

            MultiverseGameState.StateRestored += (state) => UpdateCount();
        }

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;

        public override bool RespondsToResolveOnBoard() => true;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            UpdateCount();
            yield break;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            UpdateCount();
            yield break;
        }

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            UpdateCount();
            yield break;
        }

        internal static void UpdateCount()
        {
            List<CardSlot> slots = BoardManager.Instance.GetSlots(true);
            NumberOfActiveAbilities = slots.Where(s => s.Card != null && !s.Card.Dead && s.Card.HasAbility(AbilityID)).Count();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPrefix]
        private static void ResetCount() => NumberOfActiveAbilities = 0;

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.EnergyCost), MethodType.Getter)]
        [HarmonyPostfix]
        private static void AdjustCostForSapphirePower(PlayableCard __instance, ref int __result)
        {
            __result -= NumberOfActiveAbilities;
        }
    }
}