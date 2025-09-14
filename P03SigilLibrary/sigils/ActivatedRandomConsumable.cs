using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ActivatedGainItem : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private static readonly List<Texture2D> Icons = new()
        {
            TextureHelper.GetImageAsTexture("ability_activatedgainconsumable_1.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activatedgainconsumable_2.png", typeof(ActivatedGainPowerCharge).Assembly),
            TextureHelper.GetImageAsTexture("ability_activatedgainconsumable_3.png", typeof(ActivatedGainPowerCharge).Assembly),
        };

        public const string ACTIVATION_KEY = "NumberOfActivationsRemaining";

        public override int EnergyCost => 6;

        private static bool IsCountMod(CardModificationInfo m) => !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(ACTIVATION_KEY);

        private static int GetNumberOfActivationsRemaining(CardInfo info)
        {
            // Try to get the number of remaining activations from the card tracker
            var trackingMod = info.mods.FirstOrDefault(IsCountMod);
            if (trackingMod == null)
            {
                trackingMod = new();
                trackingMod.singletonId = ACTIVATION_KEY;
                trackingMod.SetExtendedProperty(ACTIVATION_KEY, AscensionSaveData.Data.currentRun.MaxConsumables);
                SaveManager.SaveFile.CurrentDeck.ModifyCard(info, trackingMod);
            }

            return trackingMod.GetExtendedPropertyAsInt(ACTIVATION_KEY) ?? -1;
        }

        public override bool CanActivate()
        {
            return ItemsManager.Instance.consumableSlots.Where(s => s is not HammerItemSlot).Where(s => s.Item != null).Count() < AscensionSaveData.Data.currentRun.MaxConsumables
                   && GetNumberOfActivationsRemaining(this.Card.Info) > 0;
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
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedGainItem),
                TextureHelper.GetImageAsTexture("ability_activatedgainconsumable.png", typeof(ActivatedGainItem).Assembly)
            ).Id;
        }

        private void ReduceCount()
        {
            int currentCount = GetNumberOfActivationsRemaining(this.Card.Info);
            var mod = this.Card.Info.Mods.FirstOrDefault(IsCountMod);
            if (currentCount == 1)
                mod.negateAbilities = new() { AbilityID };
            else
                mod.SetExtendedProperty(ACTIVATION_KEY, currentCount - 1);
            SaveManager.SaveFile.CurrentDeck.ModifyCard(this.Card.Info, mod);
            this.Card.RenderCard();
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
                    ReduceCount();
                    yield return base.LearnAbility(0f);
                    yield break;
                }
            }
        }

        [HarmonyPatch(typeof(AbilityIconInteractable), nameof(AbilityIconInteractable.LoadIcon))]
        [HarmonyPrefix]
        private static bool LoadMissileStrikeIcons(CardInfo info, AbilityInfo ability, ref Texture __result)
        {
            if (ability.ability != AbilityID)
                return true;

            int index = GetNumberOfActivationsRemaining(info) - 1;
            if (index < 0)
                return true;

            __result = Icons[index];
            return false;
        }
    }
}
