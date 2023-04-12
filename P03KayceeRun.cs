using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class P03Plugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
		public const string PluginName = "Infiniscryption P03 in Kaycee's Mod";
		public const string PluginVersion = "2.3";   
        public const string CardPrefx = "P03KCM";

        internal static P03Plugin Instance;  

        internal static ManualLogSource Log; 

        internal static bool Initialized = false;
        
        internal string DebugCode
        {
            get
            {
                return Config.Bind("P03KayceeMod", "DebugCode", "nothing", new BepInEx.Configuration.ConfigDescription("A special code to use for debugging purposes only. Don't change this unless your name is DivisionByZorro or he told you how it works.")).Value;
            }
        }

        internal string SecretCardComponents
        {
            get
            {
                return Config.Bind("P03KayceeMod", "SecretCardComponents", "nothing", new BepInEx.Configuration.ConfigDescription("The secret code for the secret card")).Value;
            }
        }

        private void Awake()
        {
            CreateShockerItem();
            //CreateCubeItem();
            //CreateGooItem();

            Instance = this;

            Log = base.Logger;
            Log.LogInfo($"Debug code = {DebugCode}");

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(); 

            foreach (Type t in typeof(P03Plugin).Assembly.GetTypes())
            {
                try
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
                } catch (TypeLoadException ex)
                {
                    Log.LogWarning("Failed to force load static constructor!");
                    Log.LogWarning(ex);
                }
            }
            
            CustomCards.RegisterCustomCards(harmony);
            StarterDecks.RegisterStarterDecks();
            AscensionChallengeManagement.UpdateP03Challenges();
            BossManagement.RegisterBosses();

            SceneManager.sceneLoaded += this.OnSceneLoaded;

            EncounterBlueprintHelper.TestAllKnownEncounterData();

            Initialized = true;

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FixDeckEditor()
        {
            Traverse.Create((Chainloader.PluginInfos["inscryption_deckeditor"].Instance as DeckEditor)).Field("save").SetValue(SaveManager.SaveFile);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Chainloader.PluginInfos.ContainsKey("inscryption_deckeditor"))
                FixDeckEditor();
        }

        private void CreateShockerItem()
        {
            GameObject teslaCoil = new GameObject("TeslaCoil");
            //GameObject animation = new GameObject("Anim");
            //animation.AddComponent<Animator>();
            //animation.transform.SetParent(teslaCoil.transform);
            //GameObject model = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/specialnodesequences/teslacoil"));
            //model.transform.SetParent(animation.transform);

            ////"prefabs/specialnodesequences/teslacoil";

            //print(teslaCoil);
            //print(model);



            //Transform coil = teslaCoil.transform.Find("TeslaCoil(Clone)");
            //Vector3 BASE_POSITION = new(0f, 0.2f, 0f);
            //coil.localPosition = BASE_POSITION;
            //Renderer renderer = teslaCoil.transform.Find("TeslaCoil(Clone)/Base/Rod/rings_low").gameObject.GetComponent<Renderer>();
            //renderer.material.EnableKeyword("_EMISSION");
            //renderer.material.SetColor("_EmissionColor", GameColors.Instance.blue);

            //GameObject.Destroy(teslaCoil.GetComponentInChildren<AutoRotate>());

            //print(coil);
            //print(renderer);

            //teslaCoil.AddComponent<ShockerItem>();

            Texture2D ruleIcon = TextureHelper.GetImageAsTexture("ability_coder.png", typeof(ShockerItem).Assembly);

            InscryptionAPI.Items.ConsumableItemManager.New(
                PluginGuid,
                "Amplification Coil",
                "Increases your max energy. I suppose you can find some use for this.",
                ruleIcon,
                typeof(ShockerItem),
                teslaCoil)
            .SetAct3()
            .SetPickupSoundId("teslacoil_spark")
            .SetPlacedSoundId("metal_object_short")
            .SetExamineSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true)
            .SetPrefabID("prefabs/specialnodesequences/teslacoil")
            .SetRulebookCategory(AbilityMetaCategory.Part3Rulebook);
        }

        private void CreateCubeItem()
        {
            GameObject FileCube = new GameObject("FileCube");
            GameObject animation = new GameObject("Anim");
            animation.AddComponent<Animator>();
            animation.transform.SetParent(FileCube.transform);
            string PREFAB = "Weight_DataFile_GB";
            GameObject model = GameObject.Instantiate(Resources.Load<GameObject>($"Prefabs/Environment/ScaleWeights/{PREFAB}"));
            model.transform.localEulerAngles = Vector3.zero;
            model.transform.localPosition = new Vector3(0f, 0.322f, 0f);
            model.transform.SetParent(animation.transform);

            Texture2D ruleIcon = TextureHelper.GetImageAsTexture("ability_coder.png", typeof(LifeItem).Assembly);

            //LifeItem.FixGameObject(FileCube);
            //$"Prefabs/Environment/ScaleWeights/{PREFAB}";
            InscryptionAPI.Items.ConsumableItemManager.New(PluginGuid, "Data Cube", "Can be placed on the scales for some damage, if you're into that sort of thing.", ruleIcon, typeof(LifeItem), FileCube)
            .SetAct3()
            .SetPickupSoundId("archivist_spawn_filecube")
            .SetPlacedSoundId("metal_object_short")
            .SetExamineSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true)
            .SetPrefabID($"Prefabs/Environment/ScaleWeights/{PREFAB}")
            .SetRulebookCategory(AbilityMetaCategory.Part3Rulebook);
        }

        private void CreateGooItem()
        {
            GameObject GooBottle = new GameObject("GooBottle");
            GameObject animation = new GameObject("Anim");
            animation.AddComponent<Animator>();
            animation.transform.SetParent(GooBottle.transform);
            GameObject model = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Items/GooBottleItem"));
            model.transform.SetParent(animation.transform);

            Texture2D ruleIcon = TextureHelper.GetImageAsTexture("ability_coder.png", typeof(GoobertHuh).Assembly);
            //GoobertHuh.FixGameObject(GooBottle);
            //"Prefabs/Items/GooBottleItem";
            InscryptionAPI.Items.ConsumableItemManager.New(PluginGuid, "Goobert", "Please! You've got to help me get out of here!", ruleIcon, typeof(GoobertHuh), GooBottle)
            .SetAct3()
            .SetPickupSoundId("eyeball_squish")
            .SetPlacedSoundId("eyeball_drop_metal")
            .SetExamineSoundId("eyeball_squish")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true)
            .SetPrefabID("Prefabs/Items/GooBottleItem")
            .SetRulebookCategory(AbilityMetaCategory.Part3Rulebook); ;
        }
    }
}
