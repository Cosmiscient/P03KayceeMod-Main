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
    public class MissileStrikeSelf : MissileStrike
    {
        public static new Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private bool _sacrificedSelf = false;

        static MissileStrikeSelf()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Launch Self";
            info.rulebookDescription = "[creature] can sacrifice itself to launch a missile that lands on the next turn, splashing damage to adjacent spaces.";
            info.canStack = false;
            info.powerLevel = 2;
            info.activated = true;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MissileStrikeSelf),
                TextureHelper.GetImageAsTexture("ability_missile_strike_self.png", typeof(MissileStrikeSelf).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            _sacrificedSelf = true;
            yield return Card.Die(false, null, true);
            yield break;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => _sacrificedSelf;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            yield return base.Activate();
        }

        public new IEnumerator OnBellRung(bool playerCombatPhase)
        {
            yield return Activate();
        }
    }
}
