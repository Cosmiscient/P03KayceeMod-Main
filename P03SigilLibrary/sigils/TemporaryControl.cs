using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class TemporaryControl : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        private static Ability AbilityID { get; set; }

        private const string TEMPORARY_MOD_ID = "TemporaryControlModId";

        static TemporaryControl()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Controlled";
            info.rulebookDescription = "[creature] is under control of its opponent. At the end of the turn, it will return to its owner. While being controlled, it cannot be hammered.";
            info.canStack = false;
            info.powerLevel = -2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(TemporaryControl),
                TextureHelper.GetImageAsTexture("ability_fishhook.png", typeof(TemporaryControl).Assembly)
            ).Id;
        }

        private static IEnumerator ReverseOwnerOfCard(PlayableCard targetCard)
        {
            if (targetCard == null)
                yield break;

            if (targetCard.OpposingCard() != null)
            {
                if (targetCard.OpponentCard)
                {
                    targetCard.Anim.StrongNegationEffect();
                    yield return new WaitForSeconds(0.2f);
                    yield break;
                }
                else
                {
                    yield return TurnManager.Instance.Opponent.ReturnCardToQueue(targetCard.OpposingCard(), 0.25f);
                }
            }
            targetCard.SetIsOpponentCard(!targetCard.OpponentCard);
            targetCard.transform.eulerAngles += new Vector3(0f, 0f, -180f);
            yield return BoardManager.Instance.AssignCardToSlot(targetCard, targetCard.OpposingSlot(), 0.1f, null, true);
            if (targetCard.FaceDown)
            {
                targetCard.SetFaceDown(false, false);
                targetCard.UpdateFaceUpOnBoardEffects();
            }
            yield return new WaitForEndOfFrame();
        }

        public static IEnumerator GainTemporaryControl(PlayableCard card)
        {
            if (card == null)
                yield break;

            bool wasOpponentCard = card.OpponentCard;
            yield return ReverseOwnerOfCard(card);
            if (wasOpponentCard != card.OpponentCard) // the card actually swapped
            {
                AbilityIconBehaviours.DynamicAbilityCardModIds.Add(TEMPORARY_MOD_ID);
                CardModificationInfo tempOwnership = new(AbilityID) { singletonId = TEMPORARY_MOD_ID };
                card.AddTemporaryMod(tempOwnership);
            }
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => playerTurnEnd == this.Card.IsPlayerCard();

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            CardModificationInfo tempMod = this.Card.GetOrCreateSingletonTempMod(TEMPORARY_MOD_ID);
            yield return ReverseOwnerOfCard(this.Card);
            this.Card.RemoveTemporaryMod(tempMod);
        }

        [HarmonyPatch(typeof(HammerItem), nameof(HammerItem.GetValidTargets))]
        [HarmonyPostfix]
        private static void RemoveCardsUnderTemporaryControl(ref List<CardSlot> __result)
        {
            __result.RemoveAll(s => s.Card == null || s.Card.HasAbility(AbilityID));
        }
    }
}