using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Hopper : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static Hopper()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Hopper";
            info.rulebookDescription = "At the end of each turn, [creature] moves to an empty space of its owner's choosing.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Hopper),
                TextureHelper.GetImageAsTexture("ability_hopper.png", typeof(Hopper).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card != null && Card.OpponentCard != playerTurnEnd && !Card.OpponentCard;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            if (!BoardManager.Instance.PlayerSlotsCopy.Any(x => x.Card == null))
            {
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.25f);
                Card.Anim.StrongNegationEffect();
            }
            else
            {
                ViewManager.Instance.SwitchToView(View.Board, false, true);
                yield return new WaitForSeconds(0.25f);

                Vector3 a = Card.Slot.IsPlayerSlot ? Vector3.forward : Vector3.back;
                a *= 0.5f;
                Tween.Position(Card.transform, Card.transform.position + (a * 2f) + (Vector3.up * 0.25f), 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

                List<CardSlot> allslots = BoardManager.Instance.PlayerSlotsCopy;
                List<CardSlot> validslots = BoardManager.Instance.PlayerSlotsCopy.FindAll(x => x.Card == null || x.Card == Card);

                CardSlot selectedSlot = null;
                yield return BoardManager.Instance.ChooseTarget(
                    allslots,
                    validslots,
                    s => selectedSlot = s,
                    s => Card.Anim.StrongNegationEffect(),
                    null,
                    () => false,
                    CursorType.Target
                );

                if (selectedSlot != null)
                    yield return BoardManager.Instance.AssignCardToSlot(Card, selectedSlot, 0.1f, null, false);

                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            }
            yield break;
        }
    }
}
