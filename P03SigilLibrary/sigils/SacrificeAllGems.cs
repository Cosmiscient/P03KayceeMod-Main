using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class SacrificeGems : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeGems()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gem Consumer";
            info.rulebookDescription = "When [creature] is played, all gem vessels on its owner's side of the board are sacrificed.";
            info.canStack = false;
            info.powerLevel = -2;
            info.opponentUsable = true;
            info.flipYIfOpponent = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeGems),
                TextureHelper.GetImageAsTexture("ability_sacrificegems.png", typeof(SacrificeGems).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            foreach (var slot in BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard()))
            {
                if (slot.Card != null && slot.Card.HasTrait(Trait.Gem))
                {
                    yield return slot.Card.Die(true);
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
    }
}