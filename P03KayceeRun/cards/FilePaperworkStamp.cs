using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class FilePaperworkStamp : ActivatedAbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static List<string> StampedPaperwork
        {
            get
            {
                string val = P03AscensionSaveData.RunStateData.GetValue(P03Plugin.PluginGuid, "StampedPaperwork");
                if (string.IsNullOrEmpty(val))
                    return new();
                return val.Split(',').ToList();
            }
        }

        private void StampPaperwork()
        {
            var curPaperwork = StampedPaperwork;
            if (!curPaperwork.Contains(this.Card.Info.name))
            {
                curPaperwork.Add(this.Card.Info.name);
                string newVal = string.Join(",", curPaperwork);
                P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "StampedPaperwork", newVal);
            }
        }

        public const int ENERGY_COST = 1;

        static FilePaperworkStamp()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Stamp Paperwork";
            info.rulebookDescription = $"Pay {ENERGY_COST} Energy to stamp this paperwork as properly filed and ready for further processing.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(FilePaperworkStamp),
                TextureHelper.GetImageAsTexture("ability_activatedstamppaperwork.png", typeof(FilePaperworkStamp).Assembly)
            ).Id;
        }

        public override int EnergyCost => ENERGY_COST;

        public override IEnumerator Activate()
        {
            StampPaperwork();
            AudioController.Instance.PlaySound3D("rulebook_enter", MixerGroup.TableObjectsSFX, this.Card.transform.position, 1.2f);
            TableVisualEffectsManager.Instance.ThumpTable(0.3f);
            this.Card.AddTemporaryMod(new() { negateAbilities = new() { AbilityID } });
            //this.Card.RenderCard();
            yield break;
        }

    }
}
