using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Triggers;

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

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard) => otherCard.OpponentCard && !otherCard.HasTrait(Trait.Terrain);

        private Ability ChooseAbility(PlayableCard card)
        {
            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, 0, 5, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || card.HasAbility(x));
            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())]
                : Ability.Sharp;
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            CardModificationInfo mod = new(ChooseAbility(otherCard))
            {
                fromLatch = true
            };
            otherCard.Anim.ShowLatchAbility();
            otherCard.AddTemporaryMod(mod);
            otherCard.UpdateFaceUpOnBoardEffects();
            yield break;
        }

        public bool RespondsToBellRung(bool playerCombatPhase) => !playerCombatPhase;
        public IEnumerator OnBellRung(bool playerCombatPhase)
        {
            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Board);
        }
    }
}