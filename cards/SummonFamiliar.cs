using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class SummonFamiliar : AbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        static SummonFamiliar()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Summon Familiar";
            info.rulebookDescription = "When [creature] enters play, it summons a familiar bot in the slot adjacent to the right. If that slot is full, it will summon it in the slot to the left. If both are full, nothing will be summoned.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SummonFamiliar.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(SummonFamiliar),
                TextureHelper.GetImageAsTexture("ability_familiar.png", typeof(SummonFamiliar).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            CardSlot target = BoardManager.Instance.GetAdjacent(this.Card.Slot, false);
            if (target == null || target.Card != null)
                target = BoardManager.Instance.GetAdjacent(this.Card.Slot, true);

            if (target == null || target.Card != null)
            {
                this.Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            //Create list of possible familiars
            List<CardInfo> beastOptions = new List<CardInfo>();

            //Add baseline familiar options
            beastOptions.Add(CardLoader.GetCardByName("CXformerWolf"));
            beastOptions.Add(CardLoader.GetCardByName("CXformerRaven"));
            beastOptions.Add(CardLoader.GetCardByName("CXformerAdder"));
            beastOptions.Add(CardLoader.GetCardByName(ExpansionPackCards_1.EXP_1_PREFIX + "_Salmon"));

            //Add any tech card with the NewBeastTransformer metacategory
            foreach (CardInfo ci in CardLoader.AllData.Where(ci => ci.temple == CardTemple.Tech && ci.metaCategories.Contains(CustomCards.NewBeastTransformers)))
            {
                beastOptions.Add(ci);
            }

            CardInfo familiar = beastOptions[UnityEngine.Random.Range(0, beastOptions.Count)];

            if (familiar.HasAbility(Ability.Transformer))
                familiar.mods.Add(new() { negateAbilities = new() { Ability.Transformer }});

            yield return BoardManager.Instance.CreateCardInSlot(familiar, target);
            yield return new WaitForSeconds(0.25f);

        }

        
    }
}
