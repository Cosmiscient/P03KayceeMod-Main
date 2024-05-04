using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ConduitSpawnUrchin : ConduitSpawn
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ConduitSpawnUrchin()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Urchin Spawn Conduit";
            info.rulebookDescription = "Empty spaces within a circuit completed by [creature] spawn Urchin Cells at the end of the owner's turn.";
            info.canStack = true;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.conduit = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ConduitSpawnUrchin.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitSpawnUrchin),
                TextureHelper.GetImageAsTexture("ability_conduit_urchin.png", typeof(ConduitSpawnUrchin).Assembly)
            ).Id;
        }

        public override string GetSpawnCardId()
        {
            return ExpansionPackCards_2.EXP_2_PREFIX + "_UrchinCell";
        }
    }
}
