using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Patchers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class MissileStrikeBattle : NonCardTriggerReceiver, IBattleModSetup, IBattleModCleanup
    {
        public static BattleModManager.ID ID { get; private set; }

        private GameObject missileLauncher;

        private static readonly Vector3 LAUNCHER_POSITION = new(-1.3f, 5f, 4.44f);

        static MissileStrikeBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Missiles",
                new List<string>() { "Oh, there appears to be a malfunctioning missile launcher here", "Watch out for rogue missile strikes I guess" },
                typeof(MissileStrikeBattle),
                difficulty: 2,
                iconPath: "p03kcm/prefabs/rocket-bomb"
            );
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => !playerUpkeep;

        private static bool CardWouldDie(PlayableCard card) => card.Health == 1 && !(card.HasAbility(Ability.DeathShield) && !card.Status.lostShield);

        private static int ScorePlayerSlot(CardSlot slot)
        {
            List<PlayableCard> cardsThatWouldDie = new();
            List<PlayableCard> adjacentCards = BoardManager.Instance.GetAdjacentSlots(slot).Where(s => s.Card != null).Select(s => s.Card).ToList();
            if (slot.Card != null)
            {
                if (CardWouldDie(slot.Card))
                {
                    cardsThatWouldDie.Add(slot.Card);
                    if (slot.Card.HasAbility(Ability.ExplodeOnDeath))
                        cardsThatWouldDie.AddRange(adjacentCards);
                }
            }
            foreach (PlayableCard adj in adjacentCards)
            {
                if (CardWouldDie(adj))
                {
                    cardsThatWouldDie.Add(adj);
                    if (adj.HasAbility(Ability.ExplodeOnDeath) && slot.Card != null)
                        cardsThatWouldDie.Add(slot.Card);
                }
            }
            return cardsThatWouldDie.Where(c => c != null).Distinct().Select(c => c.PowerLevel).Sum() + (2 * adjacentCards.Count()) + (slot.Card != null ? 2 : 0);
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (playerUpkeep)
                yield break;

            if (TurnManager.Instance.TurnNumber % 2 != 1)
                yield break;

            // Choose opponent slot at random
            int randomSeed = P03AscensionSaveData.RandomSeed;
            CardSlot opponentSlot = BoardManager.Instance.OpponentSlotsCopy[SeededRandom.Range(0, BoardManager.Instance.OpponentSlotsCopy.Count, randomSeed++)];

            // Choose player slot by score
            List<CardSlot> scores = BoardManager.Instance.PlayerSlotsCopy.OrderBy(s => SeededRandom.Value(randomSeed++)).OrderBy(s => -ScorePlayerSlot(s)).ToList();
            CardSlot playerSlot = scores[0];

            Transform parent = missileLauncher.transform.Find("LaunchParent");

            ViewManager.Instance.SwitchToView(View.Default);
            yield return MissileStrike.LaunchMissile(opponentSlot, parent, 1, null, Vector3.zero, 6.25f);
            yield return new WaitForSeconds(0.25f);
            yield return MissileStrike.LaunchMissile(playerSlot, parent, 1, null, Vector3.zero, 6.25f);
            yield return new WaitForSeconds(0.25f);
        }

        public IEnumerator OnBattleModSetup()
        {
            ViewManager.Instance.SwitchToView(View.Default);
            missileLauncher = Instantiate(ResourceBank.Get<GameObject>("p03kcm/prefabs/Launcher"), TurnManager.Instance.opponent.transform);
            OnboardDynamicHoloPortrait.HolofyGameObject(missileLauncher.transform.Find("polySurface5").gameObject, GameColors.Instance.brightBlue);
            OnboardDynamicHoloPortrait.HolofyGameObject(missileLauncher.transform.Find("pCube10").gameObject, GameColors.Instance.brightBlue);
            OnboardDynamicHoloPortrait.HolofyGameObject(missileLauncher.transform.Find("pCube9").gameObject, GameColors.Instance.brightRed);

            missileLauncher.transform.localPosition = LAUNCHER_POSITION + (Vector3.right * 3f);
            Tween.LocalPosition(missileLauncher.transform, LAUNCHER_POSITION, 0.5f, 0f,
                completeCallback: () => AudioController.Instance.PlaySound3D("wood_object_up", MixerGroup.TableObjectsSFX, missileLauncher.transform.position));
            yield return new WaitForSeconds(0.5f);

            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Default);
        }

        public IEnumerator OnBattleModCleanup()
        {
            Tween.LocalPosition(missileLauncher.transform, LAUNCHER_POSITION + (Vector3.right * 3f), 0.5f, 0f,
                completeCallback: () => Destroy(missileLauncher));
            yield break;
        }
    }
}