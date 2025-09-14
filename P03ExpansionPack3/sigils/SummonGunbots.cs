using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03ExpansionPack3;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class SummonGunbots : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SummonGunbots()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gunbot Summoner";
            info.rulebookDescription = "When [creature] is played, play a Gunbot in all empty spaces on the board.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03Pack3Plugin.PluginGuid,
                info,
                typeof(SummonGunbots),
                TextureHelper.GetImageAsTexture("ability_spawn_gunbots.png", typeof(SummonGunbots).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            foreach (var slot in BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard()))
            {
                if (slot.Card != null)
                    continue;

                yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName(P03Pack3Plugin.CardPrefix + "_Gunbot"), slot);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
