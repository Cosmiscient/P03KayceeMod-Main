using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
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
    public static class WiseclockItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        private static GameObject GetGameObject()
        {
            GameObject gameObject = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/items/PocketWatchItem"));
            Texture2D wiseclockTexture = TextureHelper.GetImageAsTexture("wiseclock_p03.png", typeof(WiseclockItem).Assembly);
            Texture2D wiseclockEmissionTexture = TextureHelper.GetImageAsTexture("wiseclock_p03_emission.png", typeof(WiseclockItem).Assembly);
            MaterialHelper.RetextureAllRenderers(gameObject, wiseclockTexture);
            MaterialHelper.RetextureAllRenderers(gameObject, wiseclockEmissionTexture, textureName: "_EmissionMap");
            GameObject.DontDestroyOnLoad(gameObject);
            return gameObject;
        }

        static WiseclockItem()
        {
            string prefabPathKey = "p03kayceemodwiseclock";
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{prefabPathKey}", GetGameObject());

            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "Wiseclock",
                "It rotates the board clockwise or something. I stole it from Leshy. Do you think he'll notice?",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(ShockerItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(PocketWatchItem),
                GetGameObject() // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("metal_object_short")
            .SetPickupSoundId("metal_object_short")
            .SetPlacedSoundId("metal_object_short")
            .SetRegionSpecific(true)
            .SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);
        }
    }
}