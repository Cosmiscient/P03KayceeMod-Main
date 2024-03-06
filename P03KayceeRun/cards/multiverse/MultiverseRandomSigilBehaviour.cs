using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseRandomSigilBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID => SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "MultiverseRandomSigilBehaviour", typeof(MultiverseRandomSigilBehaviour)).Id;

        private static int NumberOfActivations = 0;

        public override bool RespondsToDrawn() => true;

        public override IEnumerator OnDrawn()
        {

            (PlayerHand.Instance as PlayerHand3D).MoveCardAboveHand(this.PlayableCard);
            if (MultiverseBattleSequencer.Instance != null)
            {
                if (!MultiverseBattleSequencer.Instance.HasSeenMagnificusBrush)
                {
                    yield return TextDisplayer.Instance.PlayDialogueEvent("MagnificusMultiverseBrush", TextDisplayer.MessageAdvanceMode.Input);
                    MultiverseBattleSequencer.Instance.HasSeenMagnificusBrush = true;
                }
            }

            yield return this.PlayableCard.FlipInHand(new Action(this.AddMod));
            yield break;
        }

        private Ability ChooseAbility()
        {
            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, 0, 10, CustomCards.MultiverseAbility);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || this.PlayableCard.HasAbility(x));
            if (learnedAbilities.Count > 0)
            {
                return learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, P03AscensionSaveData.RandomSeed + NumberOfActivations++)];
            }
            return MultiverseStrafe.AbilityID;
        }

        private void AddMod()
        {
            CardModificationInfo cardModificationInfo = new CardModificationInfo(this.ChooseAbility());
            this.PlayableCard.AddTemporaryMod(cardModificationInfo);
        }
    }
}