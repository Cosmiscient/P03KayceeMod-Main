using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class ConduitGainDebuffEnemy : ConduitGainAbility
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        protected override Ability AbilityToGive => Ability.DebuffEnemy;

        static ConduitGainDebuffEnemy()
        {
            string refname = AbilityManager.BaseGameAbilities.AbilityByID(Ability.DebuffEnemy).Info.rulebookName;
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = $"{refname} Conduit";
            info.rulebookDescription = $"Cards within a circuit completed by [creature] have {refname}.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.conduit = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ConduitGainDebuffEnemy),
                TextureHelper.GetImageAsTexture("ability_conduitdebuffenemy.png", typeof(ConduitGainDebuffEnemy).Assembly)
            ).Id;
        }
    }
}