using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.RuleBook;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedTemporaryControl : FuelActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public override int FuelCost => 1;

        private List<CardSlot> ValidOpposingSlots
        {
            get
            {
                return BoardManager.Instance.GetSlotsCopy(!this.Card.IsPlayerCard())
                                     .Where(s => s.Card != null &&
                                                 (s.Card.OpposingCard() == null ||
                                                  (this.Card.OpponentCard && s.Card.OpposingCard() != this.Card)))
                                     .ToList();
            }
        }

        public override bool CanActivate() => ValidOpposingSlots.Count > 0 && base.CanActivate();

        static ActivatedTemporaryControl()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Tow Hook";
            info.rulebookDescription = "Spend one fuel: tow an opposing creature to your side of the board until end of turn. Creatures being towed cannot be hammered.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedTemporaryControl),
                TextureHelper.GetImageAsTexture("ability_activated_fishhook.png", typeof(ActivatedTemporaryControl).Assembly)
            ).Id;

            info.SetUniqueRedirect("fuel", "fuelManagerPage", GameColors.Instance.limeGreen);
        }

        private int EvaluateSlot(CardSlot slot) => slot.Card?.PowerLevel ?? 0;

        private IEnumerator OnSelectSlot(CardSlot slot)
        {
            yield return TemporaryControl.GainTemporaryControl(slot.Card);
        }

        public override IEnumerator ActivateAfterSpendFuel()
        {
            yield return base.PreSuccessfulTriggerSequence();

            bool useWeaponAnim = this.Card.Info.GetExtendedPropertyAsBool("WeaponTowHook") ?? false;
            yield return this.CardChooseSlotSequence(
                OnSelectSlot,
                this.ValidOpposingSlots,
                EvaluateSlot,
                aimWeapon: useWeaponAnim,
                cursor: CursorType.FishHook
            );

            yield return base.LearnAbility(0.2f);
            yield break;
        }
    }
}
