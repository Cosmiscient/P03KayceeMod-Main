using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Resource;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    public class UfoItem : TargetSlotItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        private static readonly Vector3 BASE_POSITION = new(0f, 0.2f, 0f);

        private static readonly float sfxVolume = 0.3f;

        static UfoItem()
        {
            // string prefabPathKey = "p03kayceemodufo";
            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "UFO",
                "Abducts a card of your choice. It gets the job done I suppose",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(UfoItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(UfoItem),
                AssetBundleManager.Prefabs["UFO"] // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("factory_light_change")
            .SetPickupSoundId("factory_light_change")
            .SetPlacedSoundId("factory_wardrobe_unlock_steam")
            .SetRegionSpecific(true)
            //.SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);

            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/FirstPersonAnimations/{ItemData.PrefabId}", AssetBundleManager.Prefabs["UFO"]);
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{ItemData.PrefabId}", AssetBundleManager.Prefabs["UFO"]);
        }

        public override bool ExtraActivationPrerequisitesMet() => BoardManager.Instance.OpponentSlotsCopy.Any(s => s.Card != null && !s.Card.HasTrait(Trait.Uncuttable));

        public override void OnExtraActivationPrerequisitesNotMet()
        {
            base.OnExtraActivationPrerequisitesNotMet();
            PlayShakeAnimation();
        }

        public override void OnInvalidTargetSelected(CardSlot targetSlot)
        {
            if (targetSlot.Card != null)
            {
                CustomCoroutine.Instance.StartCoroutine(TextDisplayer.Instance.PlayDialogueEvent("P03UFOInvalid", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.CancelSelf));
            }
        }

        public override IEnumerator OnValidTargetSelected(CardSlot targetSlot, GameObject firstPersonItem)
        {
            Tween.Position(firstPersonItem.transform, new Vector3(
                targetSlot.transform.position.x,
                firstPersonItem.transform.position.y,
                targetSlot.transform.position.z
            ), 0.2f, 0f);
            yield return new WaitForSeconds(0.5f);

            CardInfo info = CardLoader.Clone(targetSlot.Card.Info);
            foreach (CardModificationInfo mod in targetSlot.Card.TemporaryMods)
                info.Mods.Add((CardModificationInfo)mod.Clone());
            AudioController.Instance.PlaySound3D("ufo", MixerGroup.TableObjectsSFX, targetSlot.transform.position, sfxVolume);
            targetSlot.Card.ExitBoard(1.4f, Vector3.up * 3);
            Tween.Position(firstPersonItem.transform, firstPersonItem.transform.position + (Vector3.up * 3), 1.4f, 0f);
            yield return new WaitForSeconds(1.4f);
            ResourcesManager.Instance.ForceGemsUpdate();

            ViewManager.Instance.SwitchToView(View.Hand, false, false);
            yield return new WaitForSeconds(0.7f);
            yield return CardSpawner.Instance.SpawnCardToHand(info);
            yield return new WaitForSeconds(0.7f);
        }

        public override string FirstPersonPrefabId => ItemData.PrefabId;

        public override Vector3 FirstPersonItemPos => new(0f, 0f, 5f);

        public override Vector3 FirstPersonItemEulers => new(-90f, 0f, 0f);

        public override View SelectionView => BoardManager.Instance.CombatView;

        public override CursorType SelectionCursorType => CursorType.Target;

        public override List<CardSlot> GetAllTargets() => BoardManager.Instance.AllSlotsCopy;

        public override List<CardSlot> GetValidTargets() => BoardManager.Instance.AllSlotsCopy.Where(s => s.Card != null && !s.Card.HasTrait(Trait.Uncuttable)).ToList();
    }
}