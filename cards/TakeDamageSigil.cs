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
    public class TakeDamageSigil : ActivatedAbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static TakeDamageSigil()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Parasite";
            info.rulebookDescription = "Take 1 damage.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            TakeDamageSigil.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(TakeDamageSigil),
                TextureHelper.GetImageAsTexture("ability_lifecycle.png", typeof(TakeDamageSigil).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            yield return LifeManager.Instance.ShowDamageSequence(1, 1, true, 0f, null, 0f, false);
        }
    }
}
