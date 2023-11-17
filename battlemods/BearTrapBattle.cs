using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Triggers;
using Sirenix.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class BearTrapBattle : NonCardTriggerReceiver, IBattleModSetup, IBattleModCleanup, IOnCardDealtDamageDirectly
    {
        public static BattleModManager.ID ID { get; private set; }

        static BearTrapBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Traps",
                new List<string>() { "It seems there are some [c:bR]traps[c:] here", "Your bots will have to fight past them to get to me" },
                typeof(BearTrapBattle),
                difficulty: 2,
                iconPath: "p03kcm/prefabs/mantrap"
            );
        }

        private readonly List<GameObject> traps = new();

        public IEnumerator OnBattleModSetup()
        {
            ViewManager.Instance.SwitchToView(View.Default);
            yield return OnBattleModCleanup();
            foreach (CardSlot slot in BoardManager.Instance.OpponentSlotsCopy)
            {
                GameObject trap = Instantiate(BetterSteelTrap.HoloTrapCopy, slot.transform);
                trap.transform.localEulerAngles = new(0f, 0f, 0f);
                trap.transform.localScale = new(1.2f, 1.2f, 1.2f);
                trap.transform.localPosition = new(0f, 0.1f, 1.2f);
                traps.Add(trap);
                AudioController.Instance.PlaySound3D("dial_metal", MixerGroup.TableObjectsSFX, trap.transform.position, 1f, 0f);
                yield return new WaitForSeconds(0.25f);
            }

            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Default);
        }

        public IEnumerator OnBattleModCleanup()
        {
            foreach (GameObject obj in traps)
            {
                if (!obj.SafeIsUnityNull())
                    Destroy(obj);
            }

            traps.Clear();
            yield break;
        }

        public bool RespondsToCardDealtDamageDirectly(PlayableCard attacker, CardSlot opposingSlot, int damage) => attacker.IsPlayerCard() && traps.Count > opposingSlot.Index && traps[opposingSlot.Index] != null;

        public IEnumerator OnCardDealtDamageDirectly(PlayableCard attacker, CardSlot opposingSlot, int damage)
        {
            GameObject trap = traps[opposingSlot.Index];
            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.65f);
            trap.GetComponentInChildren<Animator>().Play("shut", 0, 1f);
            AudioController.Instance.PlaySound3D("dial_metal", MixerGroup.TableObjectsSFX, trap.transform.position, 1f, 0f);
            yield return new WaitForSeconds(1f);
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            Destroy(trap);
            yield return attacker.Die(false, null, true);
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                yield return new WaitForSeconds(0.2f);
            }

            string peltName = attacker.Anim is DiskCardAnimationController ? "EmptyVessel" : "PeltWolf";
            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(peltName), new(), 0.25f, null);
            yield return new WaitForSeconds(0.45f);
            yield break;
        }
    }
}