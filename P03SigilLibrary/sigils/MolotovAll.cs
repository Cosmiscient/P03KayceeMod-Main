using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class MolotovAll : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MolotovAll()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Arsonist";
            info.rulebookDescription = "When [creature] is played, it sets all spaces on the board on fire for four turns.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, BurningSlotBase.FlamingAbility };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(MolotovAll),
                TextureHelper.GetImageAsTexture("ability_blast_all.png", typeof(MolotovAll).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            Card.Anim.LightNegationEffect();
            List<CardSlot> slots = BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard);
            slots.AddRange(BoardManager.Instance.GetSlotsCopy(Card.OpponentCard));

            yield return Molotov.BombCardsAsync(slots, Card, 2, 0.35f);
        }
    }
}
