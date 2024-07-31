using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class CellExplodonate : Explodonate, IOnBellRung
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static CellExplodonate()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Explodonate When Powered";
            info.rulebookDescription = "If [creature] is within a circuit, it detonates itself and all five adjacent spaces.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.conduitCell = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = AbilityManager.BaseGameAbilities.AbilityByID(Ability.CellDrawRandomCardOnDeath).Info.colorOverride;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_cellexplodeondeath.png", typeof(CellExplodonate).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(CellExplodonate),
                TextureHelper.GetImageAsTexture("ability_explodewhenpowered.png", typeof(CellExplodonate).Assembly)
            ).Id;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => ShouldExplode && base.RespondsToPreDeathAnimation(wasSacrifice);

        private bool ShouldExplode => Card.OnBoard && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override bool RespondsToResolveOnBoard() => ShouldExplode;

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard) => ShouldExplode;

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => ShouldExplode;

        public bool RespondsToBellRung(bool playerCombatPhase) => ShouldExplode;

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => ShouldExplode;

        public override bool RespondsToUpkeep(bool playerUpkeep) => ShouldExplode;

        public override IEnumerator OnResolveOnBoard() { yield return Card.Die(false, null); }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard) { yield return Card.Die(false, null); }

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard) { yield return Card.Die(false, null); }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd) { yield return Card.Die(false, null); }

        public override IEnumerator OnUpkeep(bool playerUpkeep) { yield return Card.Die(false, null); }

        public IEnumerator OnBellRung(bool playerCombatPhase) { yield return Card.Die(false, null); }
    }
}
