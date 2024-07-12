using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class SacrificeMorsel : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static SacrificeMorsel()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Macabre Growth";
            info.rulebookDescription = "[creature] gains the attack and health of all cards sacrificed to play it.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            SacrificeMorsel.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(SacrificeMorsel),
                TextureHelper.GetImageAsTexture("ability_sacrifice_morsel.png", typeof(SacrificeMorsel).Assembly)
            ).Id;
        }

        private int totalAttack = 0;
        private int totalHealth = 0;

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => true;

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            totalAttack += sacrifice.Attack;
            totalHealth += sacrifice.Health;
            yield break;
        }

        public override bool RespondsToPlayFromHand() => totalAttack > 0 || totalHealth > 0;

        public override IEnumerator OnPlayFromHand()
        {
            this.Card.AddTemporaryMod(new(totalAttack, totalHealth));
            yield break;
        }
    }
}
