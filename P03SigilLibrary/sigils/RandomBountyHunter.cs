using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class RandomBountyHunter : AbilityBehaviour
    {
        public static Ability AbilityID;
        public override Ability Ability => AbilityID;

        private static PlayableCard LastTriggeredCard;

        public override bool RespondsToDrawn() => true;

        public override IEnumerator OnDrawn()
        {
            (PlayerHand.Instance as PlayerHand3D).MoveCardAboveHand(Card);
            yield return Card.FlipInHand(new Action(AddMod));
            yield return LearnAbility(0.5f);
            yield break;
        }

        private void AddMod()
        {
            LastTriggeredCard = Card;
            Card.Status.hiddenAbilities.Add(Ability);
            CardModificationInfo mod = BountyHunterGenerator.GenerateMod(Math.Min(TurnManager.Instance.TurnNumber, 3), 20);
            if (mod.energyCostAdjustment > 6) // Lucky you!
                mod.energyCostAdjustment = 6;
            Card.AddTemporaryMod(mod);
            Card.RenderCard();
        }

        static RandomBountyHunter()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bounty Hunter";
            info.rulebookDescription = "When drawn, [creature] will turn into a random bounty hunter";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(RandomBountyHunter),
                TextureHelper.GetImageAsTexture("ability_bounty_hunter.png", typeof(RandomBountyHunter).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.DisplayedNameEnglish), MethodType.Getter)]
        [HarmonyPostfix]
        private static void UpdatedDisplayNameTheHardWay(ref CardInfo __instance, ref string __result)
        {
            if (!SaveFile.IsAscension)
                return;

            if (__instance.HasAbility(AbilityID))
            {
                if (TurnManager.Instance != null && !TurnManager.Instance.GameEnded)
                {
                    // Look for this card on the board
                    if (BoardManager.Instance != null)
                    {
                        foreach (CardSlot slot in BoardManager.Instance.PlayerSlotsCopy)
                        {
                            if (slot.Card != null && slot.Card.Info == __instance)
                            {
                                foreach (CardModificationInfo tMod in slot.Card.TemporaryMods)
                                {
                                    if (tMod.bountyHunterInfo != null)
                                    {
                                        __result = tMod.nameReplacement;
                                        return;
                                    }
                                }
                            }
                        }

                        foreach (PlayableCard pCard in PlayerHand.Instance.CardsInHand)
                        {
                            if (pCard.Info == __instance)
                            {
                                foreach (CardModificationInfo tMod in pCard.TemporaryMods)
                                {
                                    if (tMod.bountyHunterInfo != null)
                                    {
                                        __result = tMod.nameReplacement;
                                        return;
                                    }
                                }
                            }
                        }

                        if (LastTriggeredCard != null && LastTriggeredCard.Info == __instance)
                        {
                            foreach (CardModificationInfo tMod in LastTriggeredCard.TemporaryMods)
                            {
                                if (tMod.bountyHunterInfo != null)
                                {
                                    __result = tMod.nameReplacement;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}