using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
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
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Hopper),
                TextureHelper.GetImageAsTexture("ability_hopper.png", typeof(Hopper).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card != null && Card.OpponentCard != playerTurnEnd && !Card.OpponentCard;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            List<CardSlot> validslots = BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard()).FindAll(x => x.Card == null || x.Card == Card);
            if (validslots.Count == 0)
            {
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.25f);
                Card.Anim.StrongNegationEffect();
            }
            else
            {
                yield return this.CardChooseSlotSequence(
                    s => BoardManager.Instance.AssignCardToSlot(Card, s, 0.1f, null, false),
                    validslots,
                    cursor: CursorType.Place
                );
            }
            yield break;
        }
    }
}
