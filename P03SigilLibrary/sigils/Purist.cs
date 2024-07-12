using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class Purist : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public const string PURIST_KEY = "PuristTemporaryMod";

        static Purist()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Purist";
            info.rulebookDescription = "Cards opposing [creature] have their sigils removed.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Purist),
                TextureHelper.GetImageAsTexture("ability_purist.png", typeof(Purist).Assembly)
            ).Id;
        }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (GlobalTriggerHandler.Instance.StackSize != 0)
                return;

            PlayableCard opp = this.Card.OpposingCard();
            if (opp != null)
            {
                if (!opp.HasAbility(PuristImplementation.AbilityID))
                {
                    CardModificationInfo mod = new(PuristImplementation.AbilityID);
                    mod.singletonId = PURIST_KEY;
                    opp.Status.hiddenAbilities.Add(PuristImplementation.AbilityID);
                    opp.AddTemporaryMod(mod);
                }
            }
        }
    }
}