using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class ExplosiveBattle : NonCardTriggerReceiver
    {
        public static BattleModManager.ID ID { get; private set; }

        static ExplosiveBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Explosive",
                new List<string>() { "Hm; it looks like the cards in this battle will [c:bR]explode[c:] when they die", "That seems dangerous" },
                typeof(ExplosiveBattle),
                difficulty: 3,
                iconPath: "p03kcm/prefabs/unlit-bomb"
            );
            BattleModManager.SetGlobalActivationRule(ID,
                () => AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.BOMB_CHALLENGE.challengeType)
                      || DefaultQuestDefinitions.BombBattles.IsDefaultActive());
        }

        private static bool CardShouldExplode(PlayableCard card) => !card.Info.name.ToLowerInvariant().Contains("vessel") && !card.Info.HasTrait(Trait.Terrain);

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard) => CardShouldExplode(otherCard);

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            CardModificationInfo explodeMod = new(Ability.ExplodeOnDeath);
            if (!otherCard.HasAbility(Ability.ExplodeOnDeath))
                otherCard.Status.hiddenAbilities.Add(Ability.ExplodeOnDeath);
            otherCard.AddTemporaryMod(explodeMod);
            otherCard.UpdateFaceUpOnBoardEffects();
            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Board);
            yield break;
        }
    }
}