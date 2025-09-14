using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class Necromancer : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static Necromancer()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Second Chance";
            info.rulebookDescription = "Whenever a friendly non-Brittle creature dies, if [creature] is on the battlefield, a 1/1 skeleton will be created in its place.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Necromancer),
                TextureHelper.GetImageAsTexture("ability_necromancer.png", typeof(Necromancer).Assembly)
            ).Id;

            info.SetAbilityRedirect("Brittle", Ability.Brittle, GameColors.Instance.limeGreen);
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => fromCombat && Card.OnBoard && card.OpponentCard == Card.OpponentCard && !card.HasAbility(Ability.Brittle) && !card.HasAbility(Ability.IceCube);

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return PreSuccessfulTriggerSequence();
            yield return new WaitForSeconds(0.3f);

            CardInfo info = null;
            if (card != null && card.Info != null && card.Info.iceCubeParams != null && card.Info.iceCubeParams.creatureWithin != null)
            {
                info = CardLoader.Clone(card.Info.iceCubeParams.creatureWithin);
            }
            else
            {
                if (SaveManager.SaveFile.IsPart3)
                {
                    info = CardLoader.GetCardByName("RoboSkeleton");
                    CardModificationInfo mod = new()
                    {
                        healthAdjustment = -1,
                        attackAdjustment = -1,
                        energyCostAdjustment = -2
                    };
                    info.Mods.Add(mod);
                }
                else
                {
                    info = CardLoader.GetCardByName("Skeleton");
                }
            }

            yield return BoardManager.Instance.CreateCardInSlot(info, deathSlot, 0.15f, true);
            yield return LearnAbility(0.5f);
            yield break;
        }
    }
}