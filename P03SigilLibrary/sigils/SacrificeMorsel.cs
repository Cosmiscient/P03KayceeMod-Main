using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using EasyFeedback.APIs;
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
    public class SacrificeMorsel : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeMorsel()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Macabre Growth";
            info.rulebookDescription = "[creature] gains the attack and health of all cards sacrificed to play it.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            SacrificeMorsel.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeMorsel),
                TextureHelper.GetImageAsTexture("ability_sacrifice_morsel.png", typeof(SacrificeMorsel).Assembly)
            ).Id;
        }

        private int totalAttack = 0;
        private int totalHealth = 0;

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => true;

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            totalAttack += sacrifice.Attack;
            totalHealth += sacrifice.Health;
            yield break;
        }

        public override bool RespondsToPlayFromHand() => this.Card.IsPlayerCard() && (totalAttack > 0 || totalHealth > 0);

        public override IEnumerator OnPlayFromHand()
        {
            this.Card.AddTemporaryMod(new(totalAttack, totalHealth));
            yield break;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            if (this.Card.OpponentCard)
            {
                // Pick two cards without detonator
                List<PlayableCard> sacrificeTargets = BoardManager.Instance.OpponentSlotsCopy
                                                                .Where(s => s.Card != null && !s.Card.HasAbility(Ability.ExplodeOnDeath) && s.Card != this.Card)
                                                                .OrderBy(s => -(3 * s.Card.Attack + s.Card.Health))
                                                                .Select(s => s.Card)
                                                                .ToList();

                for (int i = 0; i < this.Card.BloodCost(); i++)
                {
                    if (i >= sacrificeTargets.Count)
                        break;

                    this.Card.AddTemporaryMod(new(sacrificeTargets[i].Attack, sacrificeTargets[i].Health));
                    yield return sacrificeTargets[i].Die(true);
                }
            }

            if (this.Card.Health <= 0)
                yield return this.Card.Die(false);
        }

        [HarmonyPatch(typeof(BoardStateEvaluator), nameof(BoardStateEvaluator.EvaluateCard))]
        [HarmonyPostfix]
        private static void EvaluateAIForSacrificing(BoardState.CardState card, BoardState board, ref int __result)
        {
            if (!card.HasAbility(AbilityID))
                return;

            // The higher the card slot, the higher the evaluation.
            // This means, all things being equal, it will prefer being on the right-hand side of the board
            __result += board.opponentSlots.IndexOf(card.slot);
        }
    }
}
