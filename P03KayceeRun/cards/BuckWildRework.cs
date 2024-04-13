using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class BuckWildRework : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static BuckWildRework()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Buck Wild";
            info.rulebookDescription = "Upon taking damage, this card will charge into the opposing slot, killing anything in its way.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(BuckWildRework),
                TextureHelper.GetImageAsTexture("ability_buckwild.png", typeof(BuckWildRework).Assembly)
            ).Id;
        }

        public override bool RespondsToTakeDamage(PlayableCard source) => source != null && !Card.Dead && Card.Health > 0;

        public override IEnumerator OnTakeDamage(PlayableCard source)
        {
            CardSlot opposingSlot = Card.Slot.opposingSlot;

            //PlayableCard target = this.Card.OpposingCard();

            // Just loop this over and over again.
            // Just in case the card has IceCube or something
            int sanityCount = 0;
            while (opposingSlot.Card != null && !opposingSlot.Card.Dead && sanityCount < 10)
            {
                //target.Die(false, base.Card);
                //base.Card.Slot.opposingSlot.Card.Die(false, base.Card);
                Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield return opposingSlot.Card.Die(false, Card);
                sanityCount += 1;
                //Debug.Log("Card found");
            }

            // Lots of bad stuff can happen if we're currently dead at this point
            // There's no point in continuing to move if we're just going to die anyway
            if (Card.Dead || Card.Health < 0 || opposingSlot.Card != null)
                yield break;

            yield return new WaitForSeconds(0.25f);

            Card.SetIsOpponentCard(!Card.OpponentCard);

            Card.transform.eulerAngles += new Vector3(0f, 0f, -180f);
            yield return Singleton<BoardManager>.Instance.AssignCardToSlot(Card, Card.OpposingSlot(), 0.25f);

            //base.Card.Anim.Anim.rootRotation = new Quaternion(0.6f, 0.4f, -0.4f, 0.6f);

            //if (base.Card.transform.eulerAngles != new Vector3(0f, 0f, -180f))
            //{
            //    base.Card.transform.eulerAngles = new Vector3(0f, 0f, -180f);
            //    Debug.Log("flipped");
            //}
            //else
            //{
            //    Debug.Log("Didn't flip");
            //}

            Debug.Log(Card.transform.eulerAngles);
            Debug.Log(Card.transform.rotation);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
