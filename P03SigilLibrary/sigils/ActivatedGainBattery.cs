using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedGainBattery : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int BonesCost => 2;

        public override bool CanActivate()
        {
            return ResourcesManager.Instance.PlayerMaxEnergy < 6 || ResourcesManager.Instance.PlayerEnergy < ResourcesManager.Instance.PlayerMaxEnergy;
        }

        static ActivatedGainBattery()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fossil Fuel";
            info.rulebookDescription = "Provides an additional energy cell.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedGainBattery),
                TextureHelper.GetImageAsTexture("ability_activated_gain_battery.png", typeof(ActivatedGainBattery).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            // Code copied exactly from battery bearer
            yield return base.PreSuccessfulTriggerSequence();
            if (ResourcesManager.Instance is Part3ResourcesManager)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }
            yield return ResourcesManager.Instance.AddMaxEnergy(1);
            yield return ResourcesManager.Instance.AddEnergy(1);
            if (ResourcesManager.Instance is Part3ResourcesManager)
            {
                yield return new WaitForSeconds(0.3f);
            }
            yield return base.LearnAbility(0.2f);
            yield break;
        }
    }
}
