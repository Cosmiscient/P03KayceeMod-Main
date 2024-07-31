using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedStrafeSelf : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int EnergyCost => 1;

        public override bool CanActivate()
        {
            return Card.Slot.GetAdjacentSlots(true).Where(s => s.Card == null).Count() > 0;
        }

        static ActivatedStrafeSelf()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "D-Pad";
            info.rulebookDescription = "Move to an adjacent empty lane.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedStrafeSelf),
                TextureHelper.GetImageAsTexture("ability_activated_strafe.png", typeof(ActivatedStrafeSelf).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            var validSlots = Card.Slot.GetAdjacentSlots(true).Where(s => s.Card == null).ToList();
            if (validSlots.Count == 1)
            {
                yield return BoardManager.Instance.AssignCardToSlot(Card, validSlots[0], 0.1f, null, false);
                yield return new WaitForSeconds(0.15f);
                yield break;
            }

            ViewManager.Instance.SwitchToView(View.Board, false, true);
            yield return new WaitForSeconds(0.25f);

            Vector3 a = Card.Slot.IsPlayerSlot ? Vector3.forward : Vector3.back;
            a *= 0.5f;
            Tween.Position(Card.transform, Card.transform.position + (a * 2f) + (Vector3.up * 0.25f), 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

            List<CardSlot> allslots = BoardManager.Instance.PlayerSlotsCopy;

            CardSlot selectedSlot = null;
            yield return BoardManager.Instance.ChooseTarget(
                allslots,
                validSlots,
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
    }
}
