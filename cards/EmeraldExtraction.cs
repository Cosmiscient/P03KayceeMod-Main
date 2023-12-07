using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class EmeraldExtraction : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static EmeraldExtraction()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Green Mox Buff";
            info.rulebookDescription = "When [creature] is played, it gains one health for each Green Mox its owner controls.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.GainGemGreen).Info.colorOverride;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(EmeraldExtraction),
                TextureHelper.GetImageAsTexture("ability_emerald_extraction.png", typeof(EmeraldExtraction).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => BoardManager.Instance.GetSlots(!Card.OpponentCard).Where(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemGreen) || s.Card.HasAbility(Ability.GainGemTriple))).Count() > 0;

        public override IEnumerator OnResolveOnBoard()
        {
            if (Card.TemporaryMods.Any(m => m.singletonId.Equals(nameof(EmeraldExtraction))))
                yield break;

            int healthBuff = BoardManager.Instance.GetSlots(!Card.OpponentCard).Where(s => s.Card != null && (s.Card.HasAbility(Ability.GainGemGreen) || s.Card.HasAbility(Ability.GainGemTriple))).Count();
            CardModificationInfo mod = new(0, healthBuff)
            {
                singletonId = nameof(EmeraldExtraction)
            };
            Card.Anim.StrongNegationEffect();
            Card.AddTemporaryMod(mod);

            yield break;
        }
    }
}