using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
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
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, FireBomb.FlamingAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MolotovAll),
                TextureHelper.GetImageAsTexture("ability_blast_all.png", typeof(MolotovAll).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            Card.Anim.LightNegationEffect();
            List<CardSlot> mySlots = BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard);
            List<CardSlot> otherSlots = BoardManager.Instance.GetSlotsCopy(Card.OpponentCard);

            for (int i = 0; i < mySlots.Count; i++)
            {
                yield return Molotov.BombCard(mySlots[i], Card, 3, 0.35f);
                yield return Molotov.BombCard(otherSlots[i], Card, 3, 0.35f);
            }
        }
    }
}
