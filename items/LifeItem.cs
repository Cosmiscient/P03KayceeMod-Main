using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Resource;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    public class LifeItem : ConsumableItem
    {
        public class LifeItemUglyHack : ManagedBehaviour
        {
            public override void ManagedUpdate()
            {
                base.ManagedUpdate();
                transform.localEulerAngles = Vector3.zero;
                transform.localPosition = new Vector3(0f, 0.322f, 0f);
            }
        }

        public static ConsumableItemData ItemData { get; private set; }
        private const string PREFAB = "Weight_DataFile_GB";

        private static GameObject GetGameObject()
        {
            GameObject gameObject = ShockerItem.GetBaseGameObject($"Prefabs/Environment/ScaleWeights/{PREFAB}", "LifeCube");

            Destroy(gameObject.GetComponentInChildren<Rigidbody>());
            Destroy(gameObject.GetComponentInChildren<Part3Weight>());

            Transform weight = gameObject.transform.Find($"{PREFAB}(Clone)");
            // weight.transform.localEulerAngles = Vector3.zero;
            // weight.transform.localPosition = new Vector3(0f, 0.322f + 0.322f + .1636f, 0f);
            weight.gameObject.AddComponent<LifeItemUglyHack>();

            weight.Find("Cube").gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            gameObject.AddComponent<LifeItem>();

            return gameObject;
        }

        static LifeItem()
        {
            string prefabPathKey = "p03kayceemodlifecube";
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{prefabPathKey}", GetGameObject());

            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "Data Cube",
                "Can be placed on the scales for some damage, if you're into that sort of thing.",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(ShockerItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(LifeItem),
                GetGameObject() // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("metal_object_short")
            .SetPickupSoundId("archivist_spawn_filecube")
            .SetPlacedSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);
        }

        public static ConsumableItem FixGameObject(GameObject obj)
        {
            Destroy(obj.GetComponentInChildren<Rigidbody>());
            Destroy(obj.GetComponentInChildren<Part3Weight>());

            Transform weight = obj.transform.Find($"{PREFAB}(Clone)");
            // weight.transform.localEulerAngles = Vector3.zero;
            // weight.transform.localPosition = new Vector3(0f, 0.322f + 0.322f + .1636f, 0f);
            weight.gameObject.AddComponent<LifeItemUglyHack>();

            weight.Find("Cube").gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            return obj.AddComponent<LifeItem>();
        }

        public override IEnumerator ActivateSequence()
        {
            PlayExitAnimation();
            yield return new WaitForSeconds(0.5f);
            if (TurnManager.Instance.SpecialSequencer is not null and DamageRaceBattleSequencer)
            {
                TurnManager.Instance.SpecialSequencer.DamageAddedToScale(2, true);
            }
            else
            {
                yield return LifeManager.Instance.ShowDamageSequence(2, 1, false, 0f, ResourceBank.Get<GameObject>("Prefabs/Environment/ScaleWeights/Weight_DataFile_KB"), 0.1f);
            }

            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield return EventManagement.SayDialogueOnce("P03AscensionLifeItem", EventManagement.USED_LIFE_ITEM);
            yield break;
        }
    }
}