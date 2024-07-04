using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Slots;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ThrowFire : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ThrowFire()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fireball";
            info.rulebookDescription = "At the end of its turn, [creature] chooses a card slot to set on fire for three turns.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ThrowFire),
                TextureHelper.GetImageAsTexture("ability_throw_fire.png", typeof(ThrowFire).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card != null && Card.OpponentCard != playerTurnEnd;

        private int CardSlotAIEvaluate(CardSlot slot)
        {
            if (slot.Card == null)
                return 0;

            if (slot.Card.Health == 1 && slot.Card.GetTotalShields() == 0)
                return -slot.Card.PowerLevel;

            return -slot.Card.PowerLevel / 2;
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            ViewManager.Instance.SwitchToView(View.Board, false, true);
            yield return new WaitForSeconds(0.25f);

            Vector3 a = Card.Slot.IsPlayerSlot ? Vector3.forward * .5f : Vector3.back * 0.5f;
            Vector3 originalPosition = Card.transform.position;
            Tween.Position(Card.transform, Card.transform.position + (a * 2f) + (Vector3.up * 0.25f), 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

            CardSlot selectedSlot = null;
            if (Card.IsPlayerCard())
            {

                yield return BoardManager.Instance.ChooseTarget(
                    BoardManager.Instance.AllSlotsCopy,
                    BoardManager.Instance.AllSlotsCopy,
                    s => selectedSlot = s,
                    s => Card.Anim.StrongNegationEffect(),
                    null,
                    () => false,
                    CursorType.Target
                );
            }
            else
            {
                selectedSlot = BoardManager.Instance.PlayerSlotsCopy.OrderBy(s => CardSlotAIEvaluate(s)).First();
                yield return new WaitForSeconds(0.3f);
            }

            if (selectedSlot != null)
            {
                yield return BurningSlotBase.SetSlotOnFireBasic(2, selectedSlot, Card.Slot);

                // If you set your own slot on fire for some reason??
                // Have it do damage and burn down right now
                if (playerTurnEnd == selectedSlot.IsPlayerSlot)
                {
                    var burningBehaviour = selectedSlot.GetComponent<BurningSlotBase>();
                    if (burningBehaviour != null)
                        yield return burningBehaviour.OnTurnEnd(playerTurnEnd);
                }
            }

            Tween.Position(Card.transform, originalPosition, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.15f);

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            yield break;
        }
    }
}
