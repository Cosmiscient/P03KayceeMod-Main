using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class MirrorImage : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MirrorImage()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Rubber Stamp";
            info.rulebookDescription = "Whenever you play [creature], it becomes a copy of another creature of your choosing. If this creature has other abilities, those abilities will be transferred (up to the maximum of 4).";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MirrorImage),
                TextureHelper.GetImageAsTexture("ability_copy.png", typeof(MirrorImage).Assembly)
            ).Id;
        }

        private static List<CardSlot> GetCopyableSlots()
        {
            List<CardSlot> possibles = new();
            foreach (CardSlot slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
            {
                if (slot.Card != null)
                    possibles.Add(slot);
            }

            return possibles;
        }

        public override bool RespondsToPlayFromHand() => !Card.OpponentCard && GetCopyableSlots().Count > 0;

        public override bool RespondsToResolveOnBoard() => Card.OpponentCard;

        public override IEnumerator OnResolveOnBoard()
        {
            List<CardSlot> possibles = GetCopyableSlots();
            if (possibles.Count == 0)
                yield break;

            possibles.Sort((a, b) => b.Card.PowerLevel - a.Card.PowerLevel);

            View currentview = ViewManager.Instance.CurrentView;
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.2f);

            yield return PreSuccessfulTriggerSequence();
            yield return Card.TransformIntoCard(CloneForRubberstamp(possibles[0].Card));
            foreach (var mod in possibles[0].Card.TemporaryMods)
            {
                Card.AddTemporaryMod((CardModificationInfo)mod.Clone());
            }
            yield return new WaitForSeconds(0.8f);

            ViewManager.Instance.SwitchToView(currentview, false, false);
        }

        private CardInfo CloneForRubberstamp(PlayableCard target)
        {
            CardInfo clone = CardLoader.Clone(target.Info);
            if (clone.mods.Count == 0)
            {
                foreach (CardModificationInfo mod in target.Info.mods)
                    clone.mods.Add(mod.Clone() as CardModificationInfo);
            }

            foreach (CardModificationInfo mod in Card.Info.mods)
            {
                if (clone.Abilities.Count < 4)
                    clone.mods.Add(mod.Clone() as CardModificationInfo);
            }

            return clone;
        }

        public override IEnumerator OnPlayFromHand()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.2f);

            CardSlot target = null;
            List<CardSlot> copyableSlots = GetCopyableSlots();

            if (copyableSlots.Count == 0)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MirrorImageFail", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield break;
            }

            yield return TextDisplayer.Instance.PlayDialogueEvent("MirrorImage", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            yield return BoardManager.Instance.ChooseTarget(
                copyableSlots,
                copyableSlots,
                slot => target = slot,
                null,
                null,
                null,
                CursorType.Target
            );

            CardInfo clone = CloneForRubberstamp(target.Card);

            Card.SetInfo(clone);
            foreach (var mod in target.Card.TemporaryMods)
            {
                Card.AddTemporaryMod((CardModificationInfo)mod.Clone());
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
