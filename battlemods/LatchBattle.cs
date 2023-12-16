using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Triggers;
using Sirenix.Serialization.Utilities;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class LatchBattle : NonCardTriggerReceiver, IOnBellRung
    {
        public static BattleModManager.ID ID { get; private set; }

        static LatchBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Latches",
                new List<string>() { "Well look at that. It seems that my cards are getting latched automatically.", "You don't mind an extra challenge, do you?" },
                typeof(LatchBattle),
                difficulty: 1,
                regions: new() { CardTemple.Tech, CardTemple.Nature, CardTemple.Wizard },
                iconPath: "p03kcm/prefabs/latcher-claw"
            );
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.QueueCardForSlot))]
        [HarmonyPrefix]
        private static void SetQueueMaybe(PlayableCard card)
        {
            List<NonCardTriggerReceiver> handlers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
            foreach (NonCardTriggerReceiver handler in handlers)
            {
                if (!handler.SafeIsUnityNull() && handler is LatchBattle lb)
                {
                    if (lb.RespondsToLatchCard(card))
                    {
                        lb.LatchCard(card);
                    }
                }
            }
        }


        public bool RespondsToLatchCard(PlayableCard otherCard) => otherCard.OpponentCard && !otherCard.HasTrait(Trait.Terrain) && !otherCard.Info.Mods.Any(m => m.bountyHunterInfo != null);

        private Ability ChooseAbility(PlayableCard card)
        {
            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, 0, 3, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || x == TreeStrafe.AbilityID || card.HasAbility(x));
            learnedAbilities.RemoveAll((Ability x) => !AbilitiesUtil.GetInfo(x).opponentUsable);

            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())]
                : Ability.Sharp;
        }

        public void LatchCard(PlayableCard otherCard)
        {
            CardModificationInfo mod = new(ChooseAbility(otherCard))
            {
                fromLatch = true
            };
            otherCard.Anim.ShowLatchAbility();
            otherCard.AddTemporaryMod(mod);
            otherCard.UpdateFaceUpOnBoardEffects();
        }

        public bool RespondsToBellRung(bool playerCombatPhase) => !playerCombatPhase;
        public IEnumerator OnBellRung(bool playerCombatPhase)
        {
            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Board);
        }
    }
}