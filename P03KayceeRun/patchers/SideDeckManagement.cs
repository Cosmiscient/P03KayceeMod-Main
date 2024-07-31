using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class SideDeckManagement
    {
        public static bool ExpandedSideDeck = false;

        internal static readonly List<string> STANDARD_CARDNAMES = new()
        {
            "EmptyVessel_GreenGem",
            "EmptyVessel_OrangeGem",
            "EmptyVessel_BlueGem",
            CustomCards.VESSEL_CONDUIT,
            CustomCards.VESSEL_LEAP,
            CustomCards.VESSEL_BLOOD,
            CustomCards.VESSEL_BONES,
        };

        internal static readonly List<string> TURBO_CARDNAMES = new()
        {
            CustomCards.TURBO_VESSEL_GREENGEM,
            CustomCards.TURBO_VESSEL_REDGEM,
            CustomCards.TURBO_VESSEL_BLUEGEM,
            CustomCards.TURBO_VESSEL_CONDUIT,
            CustomCards.TURBO_VESSEL_LEAP,
            CustomCards.TURBO_VESSEL_BLOOD,
            CustomCards.TURBO_VESSEL_BONES,
        };

        private static bool SideDeckCardIsValid(int sideDeckIndex)
        {
            if (!ExpandedSideDeck)
                return sideDeckIndex <= 2;

            if (sideDeckIndex == 5 && !EventManagement.BloodCardsInPool)
                return false;
            if (sideDeckIndex == 6 && !EventManagement.BonesCardsInPool)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.OnGemCardSelected))]
        [HarmonyPrefix]
        private static bool AdvanceNextSideDeckCard(Part3DeckReviewSequencer __instance, SelectableCard gemCard)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
                return false;

            var reflist = AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS) ? TURBO_CARDNAMES : STANDARD_CARDNAMES;
            int idx = reflist.IndexOf(gemCard.Info.name);

            // Reduce the count of this thing
            Part3SaveData.Data.deckGemsDistribution[idx]--;

            // Find the next index
            while (true)
            {
                idx++;
                if (idx >= reflist.Count)
                    idx = 0;
                if (SideDeckCardIsValid(idx))
                    break;
            }

            Part3SaveData.Data.deckGemsDistribution[idx]++;
            gemCard.SetInfo(__instance.GetGemCard(reflist[idx]));

            return false;
        }

        private static readonly Texture2D TURBO_SPRINTER_TEXTURE = TextureHelper.GetImageAsTexture("portrait_turbovessel.png", typeof(AscensionChallengeManagement).Assembly);

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        private static void UpdateSidedeckMod(CardInfo info)
        {
            if (info == null)
                return;

            if (!ExpandedSideDeck && !info.HasAbility(Ability.ConduitNull) && !AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
            {
                CardModificationInfo mod = info.Mods.FirstOrDefault(m => m.sideDeckMod);
                if (mod == null)
                {
                    mod = new() { sideDeckMod = true };
                    info.Mods.Add(mod);
                }
                mod.abilities ??= new();
                mod.abilities.Add(Ability.ConduitNull);
            }

            // SIDEDECK:
            // if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
            // {
            //     CardModificationInfo antiMod = new()
            //     {
            //         negateAbilities = new() { Ability.ConduitNull }
            //     };
            //     info.mods.Add(antiMod);
            // }
        }

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.CreateVesselDeck))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        private static bool BuildPart3SideDeck(ref List<CardInfo> __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (SaveFile.IsAscension)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
                    ChallengeActivationUI.Instance.ShowActivation(AscensionChallengeManagement.LEEPBOT_SIDEDECK);

                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS))
                    ChallengeActivationUI.Instance.ShowActivation(AscensionChallengeManagement.TURBO_VESSELS);
            }

            // Start by getting all of the card names
            IEnumerable<string> cardNames = Enumerable.Empty<string>();
            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
            {
                cardNames = AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS)
                    ? cardNames.Concat(Enumerable.Repeat(CustomCards.TURBO_LEAPBOT, 10))
                    : cardNames.Concat(Enumerable.Repeat(CustomCards.SIDEDECK_LEAPBOT, 10));
            }
            else
            {
                var cardreflist = AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.TURBO_VESSELS) ? TURBO_CARDNAMES : STANDARD_CARDNAMES;
                for (int i = 0; i < cardreflist.Count; i++)
                {
                    if (i >= Part3SaveData.Data.deckGemsDistribution.Length)
                        continue;

                    int count = Part3SaveData.Data.deckGemsDistribution[i];
                    cardNames = cardNames.Concat(Enumerable.Repeat(cardreflist[i], count));
                }
            }

            // And now get each card
            __result = cardNames.Select(CardLoader.GetCardByName).ToList();
            __result.ForEach(Part3CardDrawPiles.AddModsToVessel);
            return false;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.BuildSideDeck))]
        [HarmonyPrefix]
        private static bool ReplaceBuildSideDeck(Part3DeckReviewSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            __instance.sideDeck.Clear();
            __instance.sideDeck.AddRange(Part3CardDrawPiles.CreateVesselDeck());
            return false;
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), nameof(Part3DeckReviewSequencer.ApplySideDeckAbilitiesToCard))]
        [HarmonyPrefix]
        private static bool ReplaceAddSideDeck(CardInfo cardInfo)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            string currentAbilityString = String.Join(", ", cardInfo.Abilities);
            P03Plugin.Log.LogDebug($"Before applying side deck abilities card {cardInfo.DisplayedNameEnglish} has {currentAbilityString}");
            Part3CardDrawPiles.AddModsToVessel(cardInfo);
            currentAbilityString = String.Join(", ", cardInfo.Abilities);
            P03Plugin.Log.LogDebug($"After applying side deck abilities card {cardInfo.DisplayedNameEnglish} has {currentAbilityString}");
            return false;
        }

        [HarmonyPatch(typeof(Part3VesselFigurine), nameof(Part3VesselFigurine.UpdateAppearance))]
        [HarmonyPrefix]
        private static bool P03KCMVesselFigurine(Part3VesselFigurine __instance)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            __instance.abilityIcons.ForEach(a => a.gameObject.SetActive(false));

            int num = 0;
            foreach (Ability ability in Part3SaveData.Data.sideDeckAbilities)
            {
                AbilityInfo info = AbilitiesUtil.GetInfo(ability);
                if (num < __instance.abilityIcons.Count)
                {
                    __instance.abilityIcons[num].gameObject.SetActive(true);
                    __instance.abilityIcons[num].mesh = info.mesh3D;
                }
                num++;
            }

            __instance.gems.ForEach(g => g.SetActive(false));

            if (StoryEventsData.EventCompleted(StoryEvent.GemsModuleFetched))
            {
                for (int g = 0; g < 3; g++)
                {
                    int c = Part3SaveData.Data.deckGemsDistribution[g];
                    __instance.gems[g].SetActive(c > 0);
                }
            }

            return false;
        }
    }
}