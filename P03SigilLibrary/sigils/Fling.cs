using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class Fling : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int totalPower = 0;

        private List<CardSlot> ValidTargets => BoardManager.Instance.GetSlotsCopy(this.Card.OpponentCard).Where(s => s.Card != null).ToList();

        static Fling()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fling";
            info.rulebookDescription = "When [creature] is played, if one or more cards was sacrificed to play it, [creature] deals damage to a card of the player's choice equal to the total attack value of all sacrificed cards.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            Fling.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Fling),
                TextureHelper.GetImageAsTexture("ability_fling.png", typeof(Fling).Assembly)
            ).Id;
        }

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => sacrifice.Attack > 0;

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            totalPower += sacrifice.Attack;
            yield break;
        }

        public override bool RespondsToResolveOnBoard() => totalPower > 0 && ValidTargets.Count > 0;

        private IEnumerator OnSelectionSequence(CardSlot slot)
        {
            if (slot.Card != null)
                yield return slot.Card.TakeDamage(totalPower, this.Card);
        }

        public override IEnumerator OnResolveOnBoard()
        {
            yield return this.CardChooseSlotSequence(
                (slot) => OnSelectionSequence(slot),
                ValidTargets,
                aimWeapon: true
            );
        }
    }
}
