using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class CellUndying : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellUndying()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Unkillable When Powered";
            info.rulebookDescription = "If [creature] is within a circuit when it perishes, a copy of it is created in your hand.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(CellUndying),
                TextureHelper.GetImageAsTexture("ability_cellundying.png", typeof(Programmer).Assembly)
            ).Id;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return base.PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                yield return new WaitForSeconds(0.2f);
            }
            yield return CardSpawner.Instance.SpawnCardToHand(base.Card.Info);
            yield return new WaitForSeconds(0.45f);
            yield return base.LearnAbility(0.5f);
            yield break;
        }
    }
}
