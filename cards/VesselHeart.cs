using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class VesselHeart : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }


        static VesselHeart()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Vessel Heart";
            info.rulebookDescription = "When a card bearing this sigil perishes, a vessel is created in its place.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_vessel_heart.png", typeof(VesselHeart).Assembly));

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(VesselHeart),
                TextureHelper.GetImageAsTexture("ability_vessel_heart.png", typeof(VesselHeart).Assembly)
            ).Id;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            // Figure out the card we're getting
            int randomSeed = P03AscensionSaveData.RandomSeed + (TurnManager.Instance.TurnNumber * 25) + Card.Slot.Index;
            float randomDraw = SeededRandom.Value(randomSeed);

            string familiarName = "EmptyVessel_BlueGem";
            if (randomDraw < 0.3f)
                familiarName = "EmptyVessel_GreenGem";
            else if (randomDraw < 0.6f)
                familiarName = "EmptyVessel_OrangeGem";
            else if (randomDraw < 0.9f)
                familiarName = "EmptyVessel_BlueGem";

            CardInfo familiar = CardLoader.GetCardByName(familiarName);

            yield return new WaitForSeconds(0.4f);
            yield return BoardManager.Instance.CreateCardInSlot(familiar, Card.slot);
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;


    }
}
