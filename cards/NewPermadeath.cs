using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class NewPermaDeath : AbilityBehaviour
    {
        public static readonly Ability[] NOT_COPYABLE_ABILITIES = new Ability[] {
            Ability.QuadrupleBones,
            Ability.Evolve,
            Ability.IceCube,
            Ability.TailOnHit,
            Ability.PermaDeath
        };

        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static NewPermaDeath()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Skeleclocked";
            info.rulebookDescription = "When [creature] dies, it permanently becomes an Exeskeleton with the same abilities. If [creature] has Unkillable, it will be unaffected.";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(NewPermaDeath),
                TextureHelper.GetImageAsTexture("ability_newpermadeath.png", typeof(NewPermaDeath).Assembly)
            ).Id;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (Card.HasAbility(Ability.DrawCopy) || Card.HasAbility(Ability.DrawCopyOnDeath))
                yield break;

            if (Card.Slot != null && Card.HasAbility(CellUndying.AbilityID) && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot))
                yield break;

            if (Card.HasTrait(CustomCards.QuestCard))
                AchievementManager.Unlock(P03AchievementManagement.KILL_QUEST_CARD);

            // Create an exeskeleton
            DeckInfo deck = SaveManager.SaveFile.CurrentDeck;

            CardInfo card = deck.Cards.Find((CardInfo x) => x.HasAbility(AbilityID) && x.name == Card.Info.name);

            // If there is no card with this name in your deck, it's probably because it's a transformer and it's
            // currently on its other side
            if (card == null && Card.HasAbility(Ability.Transformer) && Card.Info.evolveParams != null)
                card = deck.Cards.Find(x => x.HasAbility(AbilityID) && x.name == Card.Info.evolveParams.evolution.name);

            // If the card is STILL null, then congratulations - you've managed to find some sort of weird edge case.
            // We will just let the game play out without erroring
            if (card == null)
                yield break;

            CardInfo replacement = CardLoader.GetCardByName("RoboSkeleton");
            CardModificationInfo mod = new()
            {
                abilities = new(card.Abilities.Where(ab => ab != AbilityID && !NOT_COPYABLE_ABILITIES.Contains(ab)).Take(3))
            };
            replacement.mods.Add(mod);
            deck.AddCard(replacement);

            deck.RemoveCard(card);
            yield return LearnAbility(0.5f);
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPrefix]
        public static bool PretendHasPermadeath(Ability ability, ref PlayableCard __instance, ref bool __result)
        {
            if (ability == Ability.PermaDeath && __instance.HasAbility(AbilityID))
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PermaDeath), nameof(PermaDeath.OnDie))]
        [HarmonyPrefix]
        private static void CheckForAchievement(PermaDeath __instance)
        {
            if (__instance.Card.HasTrait(CustomCards.QuestCard))
                AchievementManager.Unlock(P03AchievementManagement.KILL_QUEST_CARD);
        }
    }
}
