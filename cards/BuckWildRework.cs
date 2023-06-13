using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Pixelplacement;
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

            BuckWildRework.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(BuckWildRework),
                TextureHelper.GetImageAsTexture("ability_buckwild.png", typeof(BuckWildRework).Assembly)
            ).Id;
        }

        public override bool RespondsToTakeDamage(PlayableCard source) => source != null;

        public override IEnumerator OnTakeDamage(PlayableCard source)
        {
            CardSlot opposingSlot = base.Card.Slot.opposingSlot;

            //PlayableCard target = this.Card.OpposingCard();

            if (opposingSlot.Card != null)
            {
                //target.Die(false, base.Card);
                //base.Card.Slot.opposingSlot.Card.Die(false, base.Card);
                this.Card.Anim.StrongNegationEffect();
                yield return new WaitForSeconds(0.2f);
                yield return opposingSlot.Card.Die(false, base.Card);
                //Debug.Log("Card found");
            }

            yield return new WaitForSeconds(0.05f);

            base.Card.SetIsOpponentCard(!base.Card.OpponentCard);

            base.Card.transform.eulerAngles += new Vector3(0f, 0f, -180f);
            yield return Singleton<BoardManager>.Instance.AssignCardToSlot(base.Card, base.Card.OpposingSlot(), 0.25f);

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

            Debug.Log(base.Card.transform.eulerAngles);
            Debug.Log(base.Card.transform.rotation);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
