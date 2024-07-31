using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedGainItem : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int EnergyCost => 6;

        public override bool CanActivate()
        {
            return ItemsManager.Instance.consumableSlots.Where(s => s is not HammerItemSlot).Where(s => s.Item != null).Count() < (3 - AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessConsumables));
        }

        static ActivatedGainItem()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Dispenser";
            info.rulebookDescription = "Gain a random item.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
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

            string item = ItemsUtil.GetRandomUnlockedConsumable(P03SigilLibraryPlugin.RandomSeed).name;
            foreach (var slot in ItemsManager.Instance.consumableSlots.Where(s => s is not HammerItemSlot))
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
