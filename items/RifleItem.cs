using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Resource;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    public class RifleItem : TargetSlotItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        private static readonly float sfxVolume = 0.3f;

        static RifleItem()
        {
            // string prefabPathKey = "p03kayceemodufo";
            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "Laser Rifle",
                "This will give one of your cards the Sniper sigil for the rest of the battle. It's a gun - what else do you want?",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(RifleItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(RifleItem),
                AssetBundleManager.Prefabs["LaserRifle"] // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("metal_object_short")
            .SetPickupSoundId("disk_card_shoot")
            .SetPlacedSoundId("metal_object_short")
            .SetRegionSpecific(true)
            //.SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);

            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/FirstPersonAnimations/{ItemData.PrefabId}", AssetBundleManager.Prefabs["LaserRifle"]);
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{ItemData.PrefabId}", AssetBundleManager.Prefabs["LaserRifle"]);
        }

        public override bool ExtraActivationPrerequisitesMet() => BoardManager.Instance.PlayerSlotsCopy.Any(s => s.Card != null && !s.Card.HasAbility(Ability.Sniper));

        public override void OnExtraActivationPrerequisitesNotMet()
        {
            base.OnExtraActivationPrerequisitesNotMet();
            PlayShakeAnimation();
        }

        public override void OnInvalidTargetSelected(CardSlot targetSlot)
        {
            if (targetSlot.Card != null)
            {
                CustomCoroutine.Instance.StartCoroutine(TextDisplayer.Instance.PlayDialogueEvent("P03CannotUseSniper", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.CancelSelf));
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

            Tween.Position(firstPersonItem.transform, firstPersonItem.transform.position - (Vector3.up * 4), 0.7f, 0f);
            yield return new WaitForSeconds(0.7f);

            targetSlot.Card.Anim.PlayTransformAnimation();
            yield return new WaitForSeconds(0.15f);
            targetSlot.Card.AddTemporaryMod(new(Ability.Sniper));

            yield return new WaitForSeconds(0.7f);
        }

        public override string FirstPersonPrefabId => ItemData.PrefabId;

        public override Vector3 FirstPersonItemPos => new(0f, 0f, 4f);

        public override Vector3 FirstPersonItemEulers => new(-90f, 90f, 0f);

        public override View SelectionView => BoardManager.Instance.CombatView;

        public override CursorType SelectionCursorType => CursorType.Target;

        public override List<CardSlot> GetAllTargets() => BoardManager.Instance.PlayerSlotsCopy;

        public override List<CardSlot> GetValidTargets() => BoardManager.Instance.PlayerSlotsCopy.Where(s => s.Card != null && !s.Card.HasAbility(Ability.Sniper)).ToList();
    }
}