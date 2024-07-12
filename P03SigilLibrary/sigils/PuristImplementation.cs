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
    [HarmonyPatch]
    public class PuristImplementation : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;
        public const string PURIST_KEY = "PuristImplementationTemporaryMod";
        private CardModificationInfo purityMod = null;
        private int lastCheckedTempModCount = -1;

        static PuristImplementation()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "PuristImplementation";
            info.rulebookDescription = "This handles actually removing abilities. You should never see this ability on a card!!!";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(PuristImplementation),
                TextureHelper.GetImageAsTexture("ability_purist.png", typeof(PuristImplementation).Assembly)
            ).Id;
        }

        public override void ManagedUpdate()
        {
            base.ManagedUpdate();

            purityMod ??= this.Card.TemporaryMods.FirstOrDefault(m => m.singletonId != null && m.singletonId.Equals(PURIST_KEY));
            bool shouldBePure = this.Card.OpposingCard()?.HasAbility(Purist.AbilityID) ?? false;

            if (shouldBePure)
            {
                if (purityMod == null)
                {
                    purityMod = new();
                    purityMod.negateAbilities = new(this.Card.AllAbilities().Where(a => a != AbilityID));
                    purityMod.singletonId = PURIST_KEY;
                    this.Card.AddTemporaryMod(purityMod);
                    lastCheckedTempModCount = this.Card.temporaryMods.Count;
                }
                else if (lastCheckedTempModCount != this.Card.temporaryMods.Count)
                {
                    var thisCardAbilities = this.Card.AllAbilities();
                    if (thisCardAbilities.Count != 1 || thisCardAbilities[0] != AbilityID)
                    {
                        purityMod.negateAbilities.AddRange(thisCardAbilities.Where(a => a != AbilityID));
                        this.Card.AddTemporaryMod(purityMod);
                    }
                    lastCheckedTempModCount = this.Card.temporaryMods.Count;
                }
            }
            else
            {
                // We shouldn't be pure; remove this whole thing entirely
                if (purityMod != null)
                    this.Card.RemoveTemporaryMod(purityMod);

                CardModificationInfo mod = this.Card.TemporaryMods.FirstOrDefault(m => m.singletonId != null && m.singletonId.Equals(Purist.PURIST_KEY));
                if (mod != null)
                    this.Card.RemoveTemporaryMod(mod);
            }
        }
    }
}