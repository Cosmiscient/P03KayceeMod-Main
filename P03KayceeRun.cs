using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Items.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        private const string PREFAB = "Weight_DataFile_GB";

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
            CreateCubeItem();
            CreateGooItem();

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
            GameObject animation = new GameObject("Anim");
            animation.AddComponent<Animator>();
            animation.transform.SetParent(teslaCoil.transform);

            GameObject model = new GameObject("Model");
            model.transform.SetParent(animation.transform);
            //model.transform.localPosition = new Vector3(0, 0.85f, 0);
            //model.transform.localRotation = Quaternion.Euler(0, 180, 0);
            //model.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("art/assets3d/cabin/camera/leshy_camera");
            //model.AddComponent<MeshRenderer>().materials = new Material[]
            //{
            //    Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_2"),
            //    Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_1")
            //};

            //ShockerItem.FixGameObject(teslaCoil);
            //"prefabs/specialnodesequences/teslacoil";
            //Texture2D shockerTexture = Resources.Load<Texture2D>("art/rulebookitemicon_hourglass.png");
            //Tools.LoadTexture("ability_necromancer.png");
            //Texture2D shockerTexture = TextureHelper.GetImageAsTexture("ability_necromancer.png", typeof(P03KayceeRun).Assembly);
            //Texture2D shockerTexture = TextureHelper.GetImageAsTexture("Art/rulebookitemicon_meatpile.png");

            InscryptionAPI.Items.ConsumableItemManager.New(
                PluginGuid,
                "Amplification Coil",
                "Increases your max energy. I suppose you can find some use for this.",
                Tools.LoadTexture("ability_necromancer");,
                typeof(ShockerItem),
                teslaCoil)
            //.SetAct3()
            .SetPickupSoundId("teslacoil_spark")
            .SetPlacedSoundId("metal_object_short")
            .SetExamineSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true);
        }

        private void CreateCubeItem()
        {
            GameObject FileCube = new GameObject("FileCube");
            GameObject animation = new GameObject("Anim");
            animation.AddComponent<Animator>();
            animation.transform.SetParent(FileCube.transform);

            GameObject model = new GameObject("Model");
            model.transform.SetParent(animation.transform);
            //model.transform.localPosition = new Vector3(0, 0.85f, 0);
            //model.transform.localRotation = Quaternion.Euler(0, 180, 0);
            //model.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("art/assets3d/cabin/camera/leshy_camera");
            //model.AddComponent<MeshRenderer>().materials = new Material[]
            //{
            //    Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_2"),
            //    Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_1")
            //};

            LifeItem.FixGameObject(FileCube);
            //$"Prefabs/Environment/ScaleWeights/{PREFAB}";
            InscryptionAPI.Items.ConsumableItemManager.New(PluginGuid, "Data Cube", "Can be placed on the scales for some damage, if you're into that sort of thing.", null, typeof(LifeItem), FileCube)
            //.SetAct3()
            .SetPickupSoundId("archivist_spawn_filecube")
            .SetPlacedSoundId("metal_object_short")
            .SetExamineSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true);
        }

        private void CreateGooItem()
        {
            GameObject GooBottle = new GameObject("GooBottle");
            GameObject animation = new GameObject("Anim");
            animation.AddComponent<Animator>();
            animation.transform.SetParent(GooBottle.transform);

            GameObject model = new GameObject("Model");
            model.transform.SetParent(animation.transform);
            //model.transform.localPosition = new Vector3(0, 0.85f, 0);
            //model.transform.localRotation = Quaternion.Euler(0, 180, 0);
            //model.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("art/assets3d/cabin/camera/leshy_camera");
            //model.AddComponent<MeshRenderer>().materials = new Material[]
            //{
            //    Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_2"),
            //    Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_1")
            //};

            GoobertHuh.FixGameObject(GooBottle);
            //"Prefabs/Items/GooBottleItem";
            InscryptionAPI.Items.ConsumableItemManager.New(PluginGuid, "Goobert", "Please! You've got to help me get out of here!", null, typeof(GoobertHuh), GooBottle)
            //.SetAct3()
            .SetPickupSoundId("eyeball_squish")
            .SetPlacedSoundId("eyeball_drop_metal")
            .SetExamineSoundId("eyeball_squish")
            .SetRegionSpecific(true)
            .SetNotRandomlyGiven(true);
        }
    }
}
