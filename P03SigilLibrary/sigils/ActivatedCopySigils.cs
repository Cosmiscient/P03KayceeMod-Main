using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ActivatedCopySigils : ActivatedAbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public const string SINGLETON_ID = "COPY_PASTE_SIGILS";

        public override int EnergyCost => 3;

        private CardSlot SelectedCopyFromSlot = null;

        private List<CardSlot> CopyFromSlots
        {
            get
            {
                return BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard())
                                     .Where(s => s.Card != null && s.Card.AllAbilities().Count > 0)
                                     .ToList();
            }
        }

        private List<CardSlot> CopyToSlots
        {
            get
            {
                return BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard())
                                     .Where(s => s.Card != null && s != SelectedCopyFromSlot)
                                     .ToList();
            }
        }

        public override bool CanActivate()
        {
            // Need at least two cards, and at least one must have abilities.
            // Of course, since we know this card has abilities because this is happening,
            // we can shortcut by just ensuring there are at least two cards.
            return BoardManager.Instance.GetSlotsCopy(this.Card.IsPlayerCard())
                                     .Where(s => s.Card != null).Count() >= 2;

        }

        static ActivatedCopySigils()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Copy and Paste";
            info.rulebookDescription = "The controller of [creature] chooses two cards they control. The second card's sigils are replaced with the sigils of the first.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.activated = true;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ActivatedCopySigils),
                TextureHelper.GetImageAsTexture("ability_activated_copy_sigils.png", typeof(ActivatedCopySigils).Assembly)
            ).Id;
        }

        private int EvaluateCopyFromSlot(CardSlot slot)
        {
            if (slot.Card == null)
                return 0;
            return slot.Card.AllAbilities().Select(a => AbilitiesUtil.GetInfo(a).powerLevel).Sum();
        }

        private int EvaluateCopyToSlot(CardSlot slot)
        {
            if (slot.Card == null)
                return -10000;
            return -slot.Card.AllAbilities().Select(a => AbilitiesUtil.GetInfo(a).powerLevel).Sum();
        }

        private IEnumerator OnSelectCopyFromSlot(CardSlot slot)
        {
            SelectedCopyFromSlot = slot;
            slot.Card.Anim.StrongNegationEffect();
            yield break;
        }

        private IEnumerator OnSelectCopyToSlot(CardSlot slot)
        {
            // We need all abilities from all mods that are NOT continuous effect mods
            // And we need to negate those.
            if (slot.Card == null)
                yield break;

            List<Ability> targetAbilities = new(slot.Card.Info.Abilities);
            foreach (var mod in slot.Card.TemporaryMods.Where(m => !m.IsContinousEffectMod() && m.abilities != null))
                targetAbilities.AddRange(mod.abilities);

            CardModificationInfo newAbilityMod = slot.Card.GetOrCreateSingletonTempMod(SINGLETON_ID);
            newAbilityMod.abilities = new(SelectedCopyFromSlot.Card.AllAbilities());
            newAbilityMod.negateAbilities = targetAbilities.Where(a => !newAbilityMod.abilities.Contains(a)).ToList();

            slot.Card.Anim.StrongNegationEffect();
            slot.Card.AddTemporaryMod(newAbilityMod);

            yield break;
        }

        public override IEnumerator Activate()
        {
            yield return base.PreSuccessfulTriggerSequence();

            yield return this.CardChooseSlotSequence(
                OnSelectCopyFromSlot,
                this.CopyFromSlots,
                EvaluateCopyFromSlot,
                "Choose a card to copy sigils from",
                cursor: CursorType.Pickup
            );

            yield return new WaitForSeconds(0.2f);

            yield return this.CardChooseSlotSequence(
                OnSelectCopyToSlot,
                this.CopyToSlots,
                EvaluateCopyToSlot,
                "Choose a card to paste sigils onto",
                cursor: CursorType.Place
            );

            yield return base.LearnAbility(0.2f);
            yield break;
        }
    }
}
