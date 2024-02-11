using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
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
            info.rulebookName = "Zip of Death";
            info.rulebookDescription = "Take 2 damage.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(TakeDamageSigil),
                TextureHelper.GetImageAsTexture("ability_activatedzipbomb.png", typeof(TakeDamageSigil).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            yield return LifeManager.Instance.ShowDamageSequence(2, 2, true, 0f, null, 0f, false);
        }
    }
}
