using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class CanvasRuleBattle : CompositeRuleTriggerHandler, IBattleModSetup, IBattleModCleanup
    {
        public static BattleModManager.ID ID { get; private set; }

        static CanvasRuleBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Random Rule",
                new List<string>() { "For this battle, I'm going to add an additional rule." },
                typeof(CanvasRuleBattle),
                difficulty: 4,
                bossValid: true,
                regions: new() { },
                iconPath: "p03kcm/prefabs/frame"
            );
            BattleModManager.SetGlobalActivationRule(ID,
                () => AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.PAINTING_CHALLENGE)
                      && TurnManager.Instance.opponent is Part3BossOpponent);
        }

        public static void MakeCanvasRuleDisplayer()
        {
            GameObject effects = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightQuadTableEffect"));
            Renderer renderer = effects.GetComponentInChildren<Renderer>();
            renderer.enabled = false;
        }

        public IEnumerator OnBattleModSetup()
        {
            // Make sure there is a Rule Painting Manager
            if (RulePaintingManager.Instance == null)
                MakeCanvasRuleDisplayer();

            // Setup the battle rule at random
            CompositeBattleRule currentRule = new CompositeBattleRule();
            List<Effect> validEffects = new(CompositeBattleRule.AVAILABLE_EFFECTS);
            List<Trigger> validTriggers = new(CompositeBattleRule.AVAILABLE_TRIGGERS);

            if (TurnManager.Instance.Opponent is ArchivistBossOpponent)
            {
                validEffects.Remove(CompositeBattleRule.AVAILABLE_EFFECTS[1]); // 5 damage to random card
                validEffects.Remove(CompositeBattleRule.AVAILABLE_EFFECTS[4]); // All cards damaged by 1
            }

            int randomSeed = P03AscensionSaveData.RandomSeed;
            currentRule.effect = validEffects[SeededRandom.Range(0, validEffects.Count, randomSeed++)];
            currentRule.trigger = validTriggers[SeededRandom.Range(0, validTriggers.Count, randomSeed++)];

            Rules.Clear();
            Rules.Add(currentRule);

            yield return RulePaintingManager.Instance.SpawnPainting(currentRule);
            RulePaintingManager.Instance.SetPaintingsShown(true);
        }

        public IEnumerator OnBattleModCleanup()
        {
            RulePaintingManager.Instance.ShowMostRecentPaintingCancelled();
            RulePaintingManager.Instance.SetPaintingsShown(false);
            yield break;
        }
    }
}