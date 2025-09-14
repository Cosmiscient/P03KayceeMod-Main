using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class GemBluePurist : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public const string PURIST_KEY = "PuristTemporaryMod";

        static GemBluePurist()
        {
            AbilityInfo info2 = ScriptableObject.CreateInstance<AbilityInfo>();
            info2.rulebookName = "Purist With Blue";
            info2.rulebookDescription = "If the owner of [creature] controls a Blue Mox, cards opposing [creature] have their sigils removed.";
            info2.canStack = false;
            info2.powerLevel = 1;
            info2.opponentUsable = true;
            info2.passive = false;
            info2.SetExtendedProperty(AbilityIconBehaviours.BLUE_CELL, true);
            info2.hasColorOverride = true;
            info2.colorOverride = GameColors.Instance.lightPurple;
            info2.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info2,
                typeof(GemBluePurist),
                TextureHelper.GetImageAsTexture("ability_bluegempurist.png", typeof(GemBluePurist).Assembly)
            ).Id;

            info2.SetAbilityRedirect("Blue Mox", Ability.GainGemBlue, GameColors.Instance.limeGreen);
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasAbility))]
        [HarmonyPrefix]
        private static bool IncludeBluePurist(PlayableCard __instance, Ability ability, ref bool __result)
        {
            if (ability == Purist.AbilityID && __instance.HasAbility(AbilityID) && __instance.EligibleForGemBonus(GemType.Blue))
            {
                __result = true;
                return false;
            }
            return true;
        }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (GlobalTriggerHandler.Instance.StackSize != 0)
            {
                P03SigilLibraryPlugin.Log.LogDebug("Stack size > 0 - blue purist does nothing");
                return;
            }

            if (!this.Card.EligibleForGemBonus(GemType.Blue))
            {
                P03SigilLibraryPlugin.Log.LogDebug("no blue gem - blue purist does nothing");
                return;
            }

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