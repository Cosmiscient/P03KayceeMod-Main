using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class TreeStrafe : Strafe
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static TreeStrafe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Seed Sprinter";
            info.rulebookDescription = "At the end of its controller's turn, [creature] moves one space in the direction indicated (if it can) and plants a seed in its previous space.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_treestrafe.png", typeof(TreeStrafe).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(TreeStrafe),
                TextureHelper.GetImageAsTexture("ability_treestrafe.png", typeof(TreeStrafe).Assembly)
            ).Id;
        }

        public override IEnumerator PostSuccessfulMoveSequence(CardSlot cardSlot)
        {
            if (cardSlot.Card == null)
            {
                CardInfo treeCard = CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_SEED");
                CardModificationInfo extraAbilities = new() { abilities = new(Card.Info.Abilities) };

                if (extraAbilities.abilities.Count > 0)
                {
                    if (extraAbilities.abilities.Contains(NewPermaDeath.AbilityID))
                        extraAbilities.abilities.Remove(NewPermaDeath.AbilityID);

                    if (extraAbilities.abilities.Contains(AbilityID))
                        extraAbilities.abilities.Remove(AbilityID);

                    treeCard.mods.Add(extraAbilities);
                }

                yield return BoardManager.Instance.CreateCardInSlot(treeCard, cardSlot, 0.1f, true);
            }
            yield break;
        }
    }
}