using System;
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
            Ability.PermaDeath,
            RandomNegativeAbility.AbilityID
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

        private static string GetSkeleName(string currentName)
        {
            if (currentName.ToLowerInvariant().Contains("skel-e"))
                return "Skel-E-" + currentName.Replace(" ", "-");

            return "Skele " + currentName;
        }

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

            CardInfo card = deck.Cards.Find(x => IsTargetCard(x, Card, AbilityID));

            // If the card is null, then congratulations - you've managed to find some sort of weird edge case.
            // We will just let the game play out without erroring
            if (card == null)
                yield break;

            CardInfo replacement = CardLoader.GetCardByName("RoboSkeleton");
            CardModificationInfo mod = new()
            {
                abilities = new(card.Abilities.Where(ab => ab != AbilityID && !NOT_COPYABLE_ABILITIES.Contains(ab)).Take(3)),
                nameReplacement = GetSkeleName(card.displayedName)
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

        private static bool IsTargetCard(CardInfo info, PlayableCard Card, Ability targetAbility)
        {
            try
            {
                if (info.name.Equals(Card.Info.name))
                {
                    if (info.HasAbility(targetAbility))
                        return true;

                    if (info.HasAbility(RandomNegativeAbility.AbilityID) && Card.HasAbility(RandomNegativeAbility.AbilityID))
                        return true;
                }

                if (!Card.HasAbility(Ability.Transformer) || Card.Info.evolveParams.evolution == null)
                    return false;

                if (info.name.Equals(Card.Info.evolveParams.evolution.name))
                {
                    if (info.HasAbility(targetAbility))
                        return true;

                    if (info.HasAbility(RandomNegativeAbility.AbilityID) && Card.Info.evolveParams.evolution.HasAbility(RandomNegativeAbility.AbilityID))
                        return true;
                }

                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(PermaDeath), nameof(PermaDeath.OnDie))]
        [HarmonyPostfix]
        private static IEnumerator MakePermaDeathWorkRight(IEnumerator sequence, PermaDeath __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            if (__instance.Card.HasTrait(CustomCards.QuestCard))
                AchievementManager.Unlock(P03AchievementManagement.KILL_QUEST_CARD);

            DeckInfo currentDeck = SaveManager.SaveFile.CurrentDeck;
            CardInfo card = currentDeck.Cards.Find(x => IsTargetCard(x, __instance.Card, Ability.PermaDeath));

            if (card != null)
                currentDeck.RemoveCard(card);

            yield break;
        }
    }
}
