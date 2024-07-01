using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class ActivatedGainItem : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int EnergyCost => 6;

        public override bool CanActivate()
        {
            return MultiverseGameState.ConsumableSlots.Where(s => s.Item != null).Count() < P03AscensionSaveData.MaxNumberOfItems;
        }

        static ActivatedGainItem()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Dispenser";
            info.rulebookDescription = "Activate: Gain a random item";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Pack3Plugin.PluginGuid,
                info,
                typeof(ActivatedGainItem),
                TextureHelper.GetImageAsTexture("ability_activatedgainconsumable.png", typeof(ActivatedGainItem).Assembly)
            ).Id;
        }

        public override IEnumerator Activate()
        {
            yield return base.PreSuccessfulTriggerSequence();
            yield return new WaitForSeconds(0.2f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.25f);

            List<string> validItems = new(UnlockAscensionItemSequencer.ValidItems);
            string item = validItems[SeededRandom.Range(0, validItems.Count, P03AscensionSaveData.RandomSeed)];
            foreach (var slot in MultiverseGameState.ConsumableSlots)
            {
                if (slot.Item == null)
                {
                    slot.CreateItem(item, false);
                    ItemsManager.Instance.OnUpdateItems(true);
                    ItemsManager.Instance.SaveDataItemsList.Add(item);
                    yield return new WaitForSeconds(0.2f);
                    yield return base.LearnAbility(0f);
                    yield break;
                }
            }
        }
    }
}
