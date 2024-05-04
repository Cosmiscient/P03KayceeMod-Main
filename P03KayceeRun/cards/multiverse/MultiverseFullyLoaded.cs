using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Spells.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    public class MultiverseFullyLoaded : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static MultiverseFullyLoaded()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Multiverse Fully Loaded";
            info.rulebookDescription = "When [creature] dies, it leaves a permanent +1 attack bonus in the lane it occupied in every universe.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_fully_loaded.png", typeof(FullyLoaded).Assembly));
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseFullyLoaded),
                TextureHelper.GetImageAsTexture("ability_fully_loaded.png", typeof(FullyLoaded).Assembly)
            ).Id;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return Card.Slot.SetSlotModification(FullyLoaded.SlotModID);

            if (MultiverseBattleSequencer.Instance != null)
            {
                int slotId = Card.Slot.Index % 10;
                int universeId = MultiverseBattleSequencer.Instance.GetUniverseId(Card.Slot);
                for (int i = 0; i < MultiverseBattleSequencer.Instance.MultiverseGames.Count(); i++)
                {
                    if (universeId == i)
                        continue;

                    var universe = MultiverseBattleSequencer.Instance.MultiverseGames[i];
                    List<CardSlot> slots = Card.OpponentCard ? universe.OpponentSlots : universe.PlayerSlots;
                    yield return slots[slotId].SetSlotModification(FullyLoaded.SlotModID);
                }
            }

            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => Card.OnBoard;
    }
}
