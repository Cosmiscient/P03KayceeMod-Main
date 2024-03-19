using System.Collections;
using System.Collections.Generic;
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
    public class MultiverseTutor : AbilityBehaviour, IMultiverseAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MultiverseTutor()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.Tutor);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"Multiverse {original.rulebookName}";
            info.rulebookDescription = "When [creature] is played, you may search the multiverse for a card that has perished that is capable of traversing the multiverse.";
            info.canStack = original.canStack;
            info.powerLevel = original.powerLevel;
            info.activated = original.activated;
            info.opponentUsable = original.opponentUsable;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseTutor),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_tutor")
            ).Id;
        }

        public override bool RespondsToDrawn() => MultiverseBattleSequencer.Instance != null;

        public override IEnumerator OnDrawn()
        {
            if (MultiverseBattleSequencer.Instance != null)
            {
                if (!MultiverseBattleSequencer.Instance.HasSeenGrimoraQuill)
                {
                    (PlayerHand.Instance as PlayerHand3D).MoveCardAboveHand(this.Card);
                    yield return TextDisplayer.Instance.PlayDialogueEvent("GrimoraMultiverseQuill", TextDisplayer.MessageAdvanceMode.Input);
                    MultiverseBattleSequencer.Instance.HasSeenGrimoraQuill = true;
                    (PlayerHand.Instance as PlayerHand3D).ClearAboveHandCards();
                }
            }
        }

        public override bool RespondsToResolveOnBoard() => MultiverseBattleSequencer.Instance.DeadMultiverseCards.Count > 0;

        public override IEnumerator OnResolveOnBoard()
        {
            ViewManager.Instance.SwitchToView(View.DeckSelection, false, true);
            SelectableCard selectedCard = null;
            var cardsToSelect = MultiverseBattleSequencer.Instance.DeadMultiverseCards;
            while (cardsToSelect.Count > 15)
                cardsToSelect.RemoveAt(0);

            yield return BoardManager.Instance.CardSelector.SelectCardFrom(
                MultiverseBattleSequencer.Instance.DeadMultiverseCards,
                null,
                s => selectedCard = s
            );
            Tween.Position(selectedCard.transform, selectedCard.transform.position + Vector3.back * 4f, 0.1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Object.Destroy(selectedCard.gameObject, 0.1f);
            ViewManager.Instance.SwitchToView(View.Default);
            MultiverseBattleSequencer.Instance.RemoveDeadMultiverseCard(selectedCard.Info);
            yield return CardSpawner.Instance.SpawnCardToHand(selectedCard.Info);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
    }
}
