using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.CustomRules;
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
                difficulty: 5,
                bossValid: true,
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
            CompositeBattleRule currentRule = new();
            List<Effect> validEffects = new(CompositeBattleRule.AVAILABLE_EFFECTS);
            List<Trigger> validTriggers = new(CompositeBattleRule.AVAILABLE_TRIGGERS);

            if (TurnManager.Instance.Opponent is ArchivistBossOpponent)
            {
                validEffects = validEffects.Where(r => r is not RandomCardDestroyedEffect and not AllCardsDamagedEffect).ToList();
            }

            int randomSeed = P03AscensionSaveData.RandomSeed;
            currentRule.trigger = validTriggers[SeededRandom.Range(0, validTriggers.Count, randomSeed++)];

            if (currentRule.trigger == Trigger.OtherCardResolve)
                validEffects = validEffects.Where(r => r is not RandomCardDestroyedEffect and not RandomSalmon).ToList();
            currentRule.effect = validEffects[SeededRandom.Range(0, validEffects.Count, randomSeed++)];

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