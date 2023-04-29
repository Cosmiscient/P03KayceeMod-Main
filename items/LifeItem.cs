using System.Collections;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using System;
using DigitalRuby.LightningBolt;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Items;

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
                this.transform.localEulerAngles = Vector3.zero;
                this.transform.localPosition = new Vector3(0f, 0.322f, 0f);
            }
        }
        //public string rulebookName = "Data Cube";
        public static ConsumableItemData ItemData { get; private set; }
        private const string PREFAB = "Weight_DataFile_GB";

        static LifeItem()
        {
            ItemData = ScriptableObject.CreateInstance<ConsumableItemData>();
            //ItemData.name = $"{P03Plugin.CardPrefx}_LifeCube";
            ItemData.name = P03Plugin.PluginGuid + "_Data Cube";
        }

        public static ConsumableItem FixGameObject(GameObject obj)
        {
            GameObject.Destroy(obj.GetComponentInChildren<Rigidbody>());
            GameObject.Destroy(obj.GetComponentInChildren<Part3Weight>());
            Transform weight = obj.transform.Find($"{PREFAB}(Clone)");

            // weight.transform.localEulerAngles = Vector3.zero;
            // weight.transform.localPosition = new Vector3(0f, 0.322f + 0.322f + .1636f, 0f);
            weight.gameObject.AddComponent<LifeItemUglyHack>();

            weight.Find("Cube").gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            return obj.AddComponent<LifeItem>();
        }

        public override IEnumerator ActivateSequence()
        {
            base.PlayExitAnimation();
            yield return new WaitForSeconds(0.5f);
            yield return LifeManager.Instance.ShowDamageSequence(2, 1, false, 0f, ResourceBank.Get<GameObject>("Prefabs/Environment/ScaleWeights/Weight_DataFile_KB"), 0.1f);
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield return EventManagement.SayDialogueOnce("P03AscensionLifeItem", EventManagement.USED_LIFE_ITEM);
            yield break;
        }

        public static void CreateCubeItem()
        {
            GameObject lifeCube = new GameObject("LifeCube");
            //GameObject animation = new GameObject("Anim");
            //animation.AddComponent<Animator>();
            //animation.transform.SetParent(lifeCube.transform);
            GameObject model = Instantiate(Resources.Load<GameObject>($"Prefabs/Environment/ScaleWeights/{PREFAB}"));
            model.transform.SetParent(lifeCube.transform);


            //GameObject model = Instantiate(Resources.Load<GameObject>($"Prefabs/Environment/ScaleWeights/{PREFAB}"));
            //GameObject model = Instantiate(ResourceBank.Get<GameObject>($"Prefabs/Environment/ScaleWeights/{PREFAB}"));
            //GameObject model = new GameObject("LifeCube(Clone)");
            //GameObject model = (Resources.Load<GameObject>("prefabs/specialnodesequences/teslacoil"));

            //model.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>($"Prefabs/Environment/ScaleWeights/{PREFAB}");
            //model.AddComponent<MeshRenderer>().materials = new Material[]
            //{
            //    Resources.Load<Material>($"Prefabs/Environment/ScaleWeights/{PREFAB}")
            //};

            //GameObject gameObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("prefabs/items/bombremoteitem"), LifeCube.transform);
            //gameObject.transform.localPosition = Vector3.zero;

            //GameObject tempObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>($"Prefabs/Environment/ScaleWeights/{PREFAB}"), LifeCube.transform);

            //if (tempObject.GetComponent<Animator>() != null)
            //    GameObject.Destroy(tempObject.GetComponent<Animator>());

            //GameObject.Destroy(gameObject.GetComponent<BombRemoteItem>());
            //GameObject.Destroy(gameObject.gameObject.transform.Find("BombRemote").gameObject);

            //print(LifeCube);
            //print(model);

            LifeItem.FixGameObject(lifeCube);

            //ConsumableItemResource resource = new ConsumableItemResource();
            //resource.FromPrefab(model);

            //ConsumableItemManager.ModelType modelType = ConsumableItemManager.RegisterPrefab(P03KayceeRun.P03Plugin.PluginGuid, "Data Cube", resource);

            Texture2D ruleIcon = TextureHelper.GetImageAsTexture("ability_coder.png", typeof(LifeItem).Assembly);

            //LifeItem.FixGameObject(FileCube);
            //$"Prefabs/Environment/ScaleWeights/{PREFAB}";
            InscryptionAPI.Items.ConsumableItemManager.New(P03KayceeRun.P03Plugin.PluginGuid, "Data Cube", "Can be placed on the scales for some damage, if you're into that sort of thing.", ruleIcon, typeof(LifeItem), lifeCube)
            .SetAct3()
            .SetPickupSoundId("archivist_spawn_filecube")
            .SetPlacedSoundId("metal_object_short")
            .SetExamineSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true)
            //.SetPrefabID($"Prefabs/Environment/ScaleWeights/{PREFAB}")
            .SetRulebookCategory(AbilityMetaCategory.Part3Rulebook);
            //.SetRulebookName("Data Cube")
            //.SetRulebookDescription("Can be placed on the scales for some damage, if you're into that sort of thing.");
        }
    }
}