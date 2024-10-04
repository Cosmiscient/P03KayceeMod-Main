using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class CellDrawZapUpkeep : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellDrawZapUpkeep()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Short Circuit";
            info.rulebookDescription = "If [creature] is within a circuit at the beginning of the turn, create a Zap! in your hand. Zap! is defined as a spell that costs 2 energy and deals 1 damage to any target.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(CellDrawZapUpkeep),
                TextureHelper.GetImageAsTexture("ability_celldrawzap.png", typeof(CellDrawZapUpkeep).Assembly)
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep != Card.OpponentCard && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_ZAP"), null);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}
