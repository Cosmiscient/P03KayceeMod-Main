using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Resource;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    public class ShockerItem : ConsumableItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        private static readonly Vector3 BASE_POSITION = new(0f, 0.2f, 0f);

        private static readonly float sfxVolume = 0.3f;

        public static GameObject GetBaseGameObject(string basePrefabId, string objName)
        {
            GameObject gameObject = Instantiate(ResourceBank.Get<GameObject>("prefabs/items/bombremoteitem"));
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.name = $"{P03Plugin.CardPrefx}_{objName}";

            GameObject tempObject = Instantiate(ResourceBank.Get<GameObject>(basePrefabId), gameObject.transform);

            if (tempObject.GetComponent<Animator>() != null)
                Destroy(tempObject.GetComponent<Animator>());

            Destroy(gameObject.GetComponent<BombRemoteItem>());
            Destroy(gameObject.gameObject.transform.Find("BombRemote").gameObject);

            DontDestroyOnLoad(gameObject);

            return gameObject;
        }

        private static GameObject GetGameObject()
        {
            GameObject gameObject = GetBaseGameObject("prefabs/specialnodesequences/teslacoil", "Shocker");

            Transform coil = gameObject.transform.Find("TeslaCoil(Clone)");
            coil.localPosition = BASE_POSITION;
            Renderer renderer = gameObject.transform.Find("TeslaCoil(Clone)/Base/Rod/rings_low").gameObject.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", GameColors.Instance.blue);

            Destroy(gameObject.GetComponentInChildren<AutoRotate>());
            gameObject.AddComponent<ShockerItem>();

            return gameObject;
        }

        static ShockerItem()
        {
            string prefabPathKey = "p03kayceemodshocker";
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{prefabPathKey}", GetGameObject());

            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "Amplification Coil",
                "Increases your max energy by 2. I suppose you can find some use for this.",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(ShockerItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(ShockerItem),
                GetGameObject() // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("metal_object_short")
            .SetPickupSoundId("teslacoil_spark")
            .SetPlacedSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);
        }

        public override bool ExtraActivationPrerequisitesMet()
        {
            return ResourcesManager.Instance.PlayerMaxEnergy < 6
                 || ResourcesManager.Instance.PlayerEnergy < ResourcesManager.Instance.PlayerMaxEnergy;
        }

        public override void OnExtraActivationPrerequisitesNotMet()
        {
            base.OnExtraActivationPrerequisitesNotMet();
            PlayShakeAnimation();
        }

        private Transform _coilTransform;
        private Transform CoilTransform => _coilTransform ??= gameObject.transform.Find("TeslaCoil(Clone)");

        public override IEnumerator ActivateSequence()
        {
            Vector3 target = CoilTransform.position + (Vector3.up * 11);
            Tween.Position(CoilTransform, target, 0.5f, 0f);
            PlayPickUpSound();
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.1f);
            CoilTransform.position = new Vector3(0f, 11f, 0f);
            yield return new WaitForEndOfFrame();
            Tween.Position(CoilTransform, new Vector3(0f, 5.3f, 0f), 0.3f, 0f, completeCallback: () => PlayPlacedSound());
            yield return new WaitForSeconds(0.3f);

            //Start custom sound effect
            AudioController.Instance.PlaySound3D("static", MixerGroup.TableObjectsSFX, CoilTransform.position, sfxVolume, 0f);

            Renderer renderer = gameObject.transform.Find("TeslaCoil(Clone)/Base/Rod/ball_low").gameObject.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            Tween.ShaderColor(renderer.material, "_EmissionColor", GameColors.Instance.blue, 0.4f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);

            yield return new WaitForSeconds(0.5f);

            GameObject selfLightning = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), gameObject.transform.parent);
            selfLightning.GetComponent<LightningBoltScript>().StartObject = CoilTransform.gameObject;
            selfLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            selfLightning.GetComponent<LightningBoltScript>().EndObject = Camera.main.gameObject;

            GameObject resourceLightning = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), gameObject.transform.parent);
            resourceLightning.GetComponent<LightningBoltScript>().StartObject = CoilTransform.gameObject;
            resourceLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            resourceLightning.GetComponent<LightningBoltScript>().EndObject = ResourceDrone.Instance.gameObject;

            GameObject lifeLightning = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), gameObject.transform.parent);
            lifeLightning.GetComponent<LightningBoltScript>().StartObject = CoilTransform.gameObject;
            lifeLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            lifeLightning.GetComponent<LightningBoltScript>().EndObject = LifeManager.Instance.scales.gameObject;
            lifeLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 2f;

            GameObject upLightning = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), gameObject.transform.parent);
            upLightning.GetComponent<LightningBoltScript>().StartObject = CoilTransform.gameObject;
            upLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            upLightning.GetComponent<LightningBoltScript>().EndObject = CoilTransform.gameObject;
            upLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 11f;

            selfLightning.SetActive(false);
            resourceLightning.SetActive(false);
            lifeLightning.SetActive(false);

            List<GameObject> lightnings = new() { selfLightning, resourceLightning, lifeLightning };

            for (int i = 0; i < 8; i++)
            {
                lightnings[Random.Range(0, 3)].SetActive(true);
                //AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, selfLightning.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
                yield return new WaitForSeconds(0.05f);
                selfLightning.SetActive(false);
                resourceLightning.SetActive(false);
                lifeLightning.SetActive(false);
            }

            for (int i = 0; i < 8; i++)
            {
                lightnings[Random.Range(0, 3)].SetActive(true);
                //AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, selfLightning.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
                yield return new WaitForSeconds(0.1f);
                selfLightning.SetActive(false);
                resourceLightning.SetActive(false);
                lifeLightning.SetActive(false);
            }

            foreach (GameObject obj in lightnings)
                obj.SetActive(true);

            //AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, selfLightning.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            foreach (GameObject obj in lightnings)
                Destroy(obj);

            Destroy(upLightning);

            yield return ResourcesManager.Instance.AddMaxEnergy(1);
            //yield return ResourcesManager.Instance.AddEnergy(1);

            yield return new WaitForSeconds(0.1f);

            yield return ResourcesManager.Instance.AddMaxEnergy(1);
            //yield return ResourcesManager.Instance.AddEnergy(1);

            yield return new WaitForSeconds(0.3f);

            //GameObject.Destroy(upLightning);

            yield return new WaitForSeconds(0.15f);

            target = CoilTransform.position + (Vector3.up * 11);
            Tween.Position(CoilTransform, target, 1f, 0f);
            yield return new WaitForSeconds(0.5f);
        }
    }
}