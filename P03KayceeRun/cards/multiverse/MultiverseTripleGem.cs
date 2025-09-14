using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class MultiverseTripleGem : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MultiverseTripleGem()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.GainGemTriple);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When [creature] is played, a Green, Orange, and Blue Gem is provided to the owner's side in every universe.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = Color.black;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseTripleGem),
                TextureHelper.GetImageAsTexture("GainGemTriple.png", typeof(MultiverseTripleGem).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AddTemporaryMod))]
        [HarmonyPostfix]
        private static void ForceResourceUpdateOnTempMod() => ResourcesManager.Instance.ForceGemsUpdate();

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.RemoveTemporaryMod))]
        [HarmonyPostfix]
        private static void ForceResourceUpdateOnRemoveTempMod() => ResourcesManager.Instance.ForceGemsUpdate();

        [HarmonyPatch(typeof(OpponentGemsManager), nameof(OpponentGemsManager.ForceGemsUpdate))]
        [HarmonyPostfix]
        private static void MultiverseOpponentGems(OpponentGemsManager __instance)
        {
            if (__instance == null)
                return;

            if (MultiverseBattleSequencer.Instance == null)
                return;

            if (MultiverseBattleSequencer.Instance.MultiverseGames == null)
                return;

            if (MultiverseBattleSequencer.Instance.MultiverseGames.Any(m => m?.HasAbility(AbilityID, true) ?? false))
            {
                __instance.AddGems(
                    GemType.Green,
                    GemType.Orange,
                    GemType.Blue
                );
            }
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ForceGemsUpdate))]
        [HarmonyPostfix]
        private static void MultiverseGems(ResourcesManager __instance)
        {
            OpponentGemsManager.Instance.ForceGemsUpdate();

            if (MultiverseBattleSequencer.Instance == null)
                return;

            if (MultiverseBattleSequencer.Instance.MultiverseGames == null)
                return;

            if (MultiverseBattleSequencer.Instance.MultiverseGames.Any(m => m?.HasAbility(AbilityID, true) ?? false))
            {
                __instance.gems.Add(GemType.Green);
                __instance.gems.Add(GemType.Orange);
                __instance.gems.Add(GemType.Blue);
                for (int j = 0; j < 3; j++)
                {
                    __instance.SetGemOnImmediate((GemType)j, true);
                }
            }
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            if (!base.Card.OpponentCard)
            {
                yield return ResourcesManager.Instance.AddGem(GemType.Green);
                yield return ResourcesManager.Instance.AddGem(GemType.Orange);
                yield return ResourcesManager.Instance.AddGem(GemType.Blue);
            }
            else
            {
                OpponentGemsManager.Instance.AddGems(
                    GemType.Green,
                    GemType.Orange,
                    GemType.Blue
                );
            }
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (!base.Card.OpponentCard)
            {
                yield return ResourcesManager.Instance.LoseGem(GemType.Green);
                yield return ResourcesManager.Instance.LoseGem(GemType.Orange);
                yield return ResourcesManager.Instance.LoseGem(GemType.Blue);
            }
            else
            {
                OpponentGemsManager.Instance.LoseGems(
                    GemType.Green,
                    GemType.Orange,
                    GemType.Blue
                );
            }
            yield break;
        }
    }
}
