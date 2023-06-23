using System.Collections;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using System;
using DigitalRuby.LightningBolt;
using System.Collections.Generic;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Helpers;
using InscryptionAPI.Resource;
using Infiniscryption.P03KayceeRun.Helpers;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    public class ShockerItem : ConsumableItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        private static readonly Vector3 BASE_POSITION = new(0f, 0.2f, 0f);

        private static readonly float sfxVolume = 0.3f;
        private GameObject audioObject = new GameObject("StaticAudioObject");
        public AudioSource audioSource;

        public static GameObject GetBaseGameObject(string basePrefabId, string objName)
        {
            GameObject gameObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("prefabs/items/bombremoteitem"));
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.name = $"{P03Plugin.CardPrefx}_{objName}";

            GameObject tempObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>(basePrefabId), gameObject.transform);

            if (tempObject.GetComponent<Animator>() != null)
                GameObject.Destroy(tempObject.GetComponent<Animator>());

            GameObject.Destroy(gameObject.GetComponent<BombRemoteItem>());
            GameObject.Destroy(gameObject.gameObject.transform.Find("BombRemote").gameObject);

            GameObject.DontDestroyOnLoad(gameObject);

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

            GameObject.Destroy(gameObject.GetComponentInChildren<AutoRotate>());
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
            return (ResourcesManager.Instance.PlayerMaxEnergy < 6 
                 || ResourcesManager.Instance.PlayerEnergy < ResourcesManager.Instance.PlayerMaxEnergy);
        }

        public override void OnExtraActivationPrerequisitesNotMet()
        {
            base.OnExtraActivationPrerequisitesNotMet();
            this.PlayShakeAnimation();
        }

        private Transform _coilTransform;
        private Transform CoilTransform => (_coilTransform ??= this.gameObject.transform.Find("TeslaCoil(Clone)"));

        public override IEnumerator ActivateSequence()
        {
            Vector3 target = this.CoilTransform.position + (Vector3.up * 11);
            Tween.Position(this.CoilTransform, target, 0.5f, 0f);
            this.PlayPickUpSound();
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.1f);
            this.CoilTransform.position = new Vector3(0f, 11f, 0f);
            yield return new WaitForEndOfFrame();
            Tween.Position(this.CoilTransform, new Vector3(0f, 5.3f, 0f), 0.3f, 0f, completeCallback:() => this.PlayPlacedSound());
            yield return new WaitForSeconds(0.3f);

            //Start custom sound effect
            audioSource = audioObject.AddComponent<AudioSource>();
            string path = AudioHelper.FindAudioClip("static");
            AudioClip audioClip = InscryptionAPI.Sound.SoundManager.LoadAudioClip(path);
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.volume = sfxVolume;
            audioSource.Play();

            Renderer renderer = this.gameObject.transform.Find("TeslaCoil(Clone)/Base/Rod/ball_low").gameObject.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            Tween.ShaderColor(renderer.material, "_EmissionColor", GameColors.Instance.blue, 0.4f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);

            yield return new WaitForSeconds(0.5f);

            GameObject selfLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            selfLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            selfLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            selfLightning.GetComponent<LightningBoltScript>().EndObject = Camera.main.gameObject;

            GameObject resourceLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            resourceLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            resourceLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            resourceLightning.GetComponent<LightningBoltScript>().EndObject = ResourceDrone.Instance.gameObject;

            GameObject lifeLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            lifeLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            lifeLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            lifeLightning.GetComponent<LightningBoltScript>().EndObject = LifeManager.Instance.scales.gameObject;
            lifeLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 2f;

            GameObject upLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            upLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            upLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            upLightning.GetComponent<LightningBoltScript>().EndObject = this.CoilTransform.gameObject;
            upLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 11f;

            selfLightning.SetActive(false);
            resourceLightning.SetActive(false);
            lifeLightning.SetActive(false);

            List<GameObject> lightnings = new List<GameObject>() { selfLightning, resourceLightning, lifeLightning };

            for (int i = 0; i < 8; i++)
            {
                lightnings[UnityEngine.Random.Range(0, 3)].SetActive(true);
                //AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, selfLightning.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
                yield return new WaitForSeconds(0.05f);
                selfLightning.SetActive(false);
                resourceLightning.SetActive(false);
                lifeLightning.SetActive(false);
            }

            for (int i = 0; i < 8; i++)
            {
                lightnings[UnityEngine.Random.Range(0, 3)].SetActive(true);
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
                GameObject.Destroy(obj);

            GameObject.Destroy(upLightning);

            yield return ResourcesManager.Instance.AddMaxEnergy(1);
            //yield return ResourcesManager.Instance.AddEnergy(1);

            yield return new WaitForSeconds(0.1f);

            yield return ResourcesManager.Instance.AddMaxEnergy(1);
            //yield return ResourcesManager.Instance.AddEnergy(1);

            yield return new WaitForSeconds(0.5f);

            //GameObject.Destroy(upLightning);

            Destroy(audioObject);

            yield return new WaitForSeconds(0.15f);

            target = this.CoilTransform.position + (Vector3.up * 11);
            Tween.Position(this.CoilTransform, target, 1f, 0f);
            yield return new WaitForSeconds(0.5f);
        }
    }
}