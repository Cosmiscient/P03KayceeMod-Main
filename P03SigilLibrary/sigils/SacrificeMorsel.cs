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

        public override bool RespondsToResolveOnBoard() => this.Card.OpponentCard && this.Card.Info.BloodCost > 0;

        public override IEnumerator OnResolveOnBoard()
        {
            // Pick two cards without detonator
            List<PlayableCard> sacrificeTargets = BoardManager.Instance.OpponentSlotsCopy
                                                              .Where(s => s.Card != null && !s.Card.HasAbility(Ability.ExplodeOnDeath) && !s.Card == this.Card)
                                                              .OrderBy(s => -(3 * s.Card.Attack + s.Card.Health))
                                                              .Select(s => s.Card)
                                                              .Take(this.Card.Info.BloodCost)
                                                              .ToList();

            foreach (var c in sacrificeTargets)
            {
                this.Card.AddTemporaryMod(new(c.Attack, c.Health));
                yield return c.Die(true);
            }
        }
    }
}
