using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
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
            info.rulebookDescription = "When [creature] is played, it plays a techbeast in an empty adjacent slot.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
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
            CardSlot target = BoardManager.Instance.GetAdjacent(Card.Slot, false);
            if (target == null || target.Card != null)
                target = BoardManager.Instance.GetAdjacent(Card.Slot, true);

            if (target == null || target.Card != null)
            {
                Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            //Create list of possible familiars
            List<CardInfo> beastOptions = new() {
                //Add baseline familiar options
                CardLoader.GetCardByName("CXformerWolf"),
                CardLoader.GetCardByName("CXformerRaven"),
                CardLoader.GetCardByName("CXformerAdder"),
                CardLoader.GetCardByName(ExpansionPackCards_1.EXP_1_PREFIX + "_Salmon")
            };

            //Add any tech card with the NewBeastTransformer metacategory
            foreach (CardInfo ci in CardLoader.AllData.Where(ci => ci.temple == CardTemple.Tech && ci.metaCategories.Contains(CustomCards.NewBeastTransformers)))
            {
                beastOptions.Add(ci);
            }

            CardInfo familiar = beastOptions[Random.Range(0, beastOptions.Count)];

            if (familiar.HasAbility(Ability.Transformer))
                familiar.mods.Add(new() { negateAbilities = new() { Ability.Transformer } });

            yield return BoardManager.Instance.CreateCardInSlot(familiar, target);
            yield return new WaitForSeconds(0.25f);

        }


    }
}
