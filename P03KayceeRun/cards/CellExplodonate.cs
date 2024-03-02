using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class CellExplodonate : AbilityBehaviour, IOnBellRung
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
                P03Plugin.PluginGuid,
                info,
                typeof(CellExplodonate),
                TextureHelper.GetImageAsTexture("ability_explodewhenpowered.png", typeof(CellExplodonate).Assembly)
            ).Id;
        }

        private IEnumerator BombCard(CardSlot slot)
        {
            if (slot.Card == null)
                yield break;

            GameObject bomb = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/DetonatorHoloBomb"));
            bomb.transform.position = Card.transform.position + (Vector3.up * 0.1f);
            Tween.Position(bomb.transform, slot.Card.transform.position + (Vector3.up * 0.1f), 0.5f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);
            yield return new WaitForSeconds(0.5f);
            slot.Card.Anim.PlayHitAnimation();
            Destroy(bomb);
            yield return slot.Card.TakeDamage(10, Card);
            yield break;
        }

        private IEnumerator Explodonate()
        {
            CardSlot slot = Card.Slot;
            List<CardSlot> friendlySlots = BoardManager.Instance.GetSlotsCopy(!Card.OpponentCard);
            List<CardSlot> opposingSlots = BoardManager.Instance.GetSlotsCopy(Card.OpponentCard);

            if (slot.Index > 0)
            {
                yield return BombCard(friendlySlots[slot.Index - 1]);
                yield return BombCard(opposingSlots[slot.Index - 1]);
            }
            yield return BombCard(opposingSlots[slot.Index]);
            if (slot.Index < friendlySlots.Count - 1)
            {
                yield return BombCard(opposingSlots[slot.Index + 1]);
                yield return BombCard(friendlySlots[slot.Index + 1]);
            }
        }

        private bool ShouldExplode => !Card.Dead && Card.Slot != null && ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

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

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => ConduitCircuitManager.Instance.SlotIsWithinCircuit(Card.Slot);

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer) { yield return Explodonate(); }
    }
}
