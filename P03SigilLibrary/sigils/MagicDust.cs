using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Slots;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class MagicDust : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static readonly Dictionary<Ability, SlotModificationManager.ModificationType> SlotIDs = new();

        public static readonly List<Ability> AllGemAbilities = new() { Ability.GainGemTriple, Ability.GainGemOrange, Ability.GainGemGreen, Ability.GainGemBlue };

        public class TripleGemSlot : SlotModificationGainAbilityBehaviour { protected override Ability AbilityToGain => Ability.GainGemTriple; }
        public class OrangeGemSlot : SlotModificationGainAbilityBehaviour { protected override Ability AbilityToGain => Ability.GainGemOrange; }
        public class GreenGemSlot : SlotModificationGainAbilityBehaviour { protected override Ability AbilityToGain => Ability.GainGemGreen; }
        public class BlueGemSlot : SlotModificationGainAbilityBehaviour { protected override Ability AbilityToGain => Ability.GainGemBlue; }

        static MagicDust()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Magic Dust";
            info.rulebookDescription = "When [creature] dies, it leaves one of its gems behind on the board. Any future cards in that space will provide those gems.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(MagicDust),
                TextureHelper.GetImageAsTexture("ability_magic_dust.png", typeof(MagicDust).Assembly)
            ).Id;

            SlotIDs[Ability.GainGemBlue] =
                SlotModificationManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                "BlueGemSlot",
                typeof(BlueGemSlot),
                TextureHelper.GetImageAsTexture("cardslot_blue_gem.png", typeof(MagicDust).Assembly),
                TextureHelper.GetImageAsTexture("pixel_slot_blue_gem.png", typeof(MagicDust).Assembly)
            );

            SlotIDs[Ability.GainGemOrange] =
                SlotModificationManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                "OrangeGemSlot",
                typeof(OrangeGemSlot),
                TextureHelper.GetImageAsTexture("cardslot_orange_gem.png", typeof(MagicDust).Assembly),
                TextureHelper.GetImageAsTexture("pixel_slot_orange_gem.png", typeof(MagicDust).Assembly)
            );

            SlotIDs[Ability.GainGemGreen] =
                SlotModificationManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                "GreenGemSlot",
                typeof(GreenGemSlot),
                TextureHelper.GetImageAsTexture("cardslot_green_gem.png", typeof(MagicDust).Assembly),
                TextureHelper.GetImageAsTexture("pixel_slot_green_gem.png", typeof(MagicDust).Assembly)
            );

            SlotIDs[Ability.GainGemTriple] =
                SlotModificationManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                "TripleGemSlot",
                typeof(TripleGemSlot),
                TextureHelper.GetImageAsTexture("cardslot_triple_gem.png", typeof(MagicDust).Assembly),
                TextureHelper.GetImageAsTexture("pixel_slot_all_gem.png", typeof(MagicDust).Assembly)
            );
        }

        private List<Ability> CardGemAbilities => new(AllGemAbilities.Where(Card.HasAbility));

        private CardSlot oldSlot = null;

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            oldSlot = Card.Slot;
            yield break;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            Ability target = CardGemAbilities[0];

            if (oldSlot != null)
                yield return oldSlot.SetSlotModification(SlotIDs[target]);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => CardGemAbilities.Count > 0;
    }
}
