using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using System.Linq;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class TreeStrafe : Strafe
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static TreeStrafe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Seed Sprinter";
            info.rulebookDescription = "At the end of its controller's turn, [creature] moves one space in the direction indicated (if it can) and leaves behind a tree.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            TreeStrafe.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(TreeStrafe),
                TextureHelper.GetImageAsTexture("ability_treestrafe.png", typeof(TreeStrafe).Assembly)
            ).Id;
        }

        public override IEnumerator PostSuccessfulMoveSequence(CardSlot cardSlot)
		{
			if (cardSlot.Card == null)
			{
                CardInfo treeCard = CardLoader.GetCardByName("Tree_Hologram");
                CardModificationInfo extraAbilities = new () { abilities = new () };

                foreach (var mod in this.Card.Info.Mods)
                {
                    if (mod.abilities != null && mod.abilities.Count > 0)
                    {
                        extraAbilities.abilities.AddRange(mod.abilities);
                        if (mod.fromOverclock)
                            extraAbilities.fromOverclock = true;
                    }
                }

                if (extraAbilities.abilities.Count > 0)
                {
                    if (extraAbilities.abilities.Contains(NewPermaDeath.AbilityID))
                        extraAbilities.abilities.Remove(NewPermaDeath.AbilityID);

                    if (extraAbilities.abilities.Contains(TreeStrafe.AbilityID))
                        extraAbilities.abilities.Remove(TreeStrafe.AbilityID);

                    treeCard.mods.Add(extraAbilities);
                }

				yield return BoardManager.Instance.CreateCardInSlot(treeCard, cardSlot, 0.1f, true);
			}
			yield break;
		}
    }
}