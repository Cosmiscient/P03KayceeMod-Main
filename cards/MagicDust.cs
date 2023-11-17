using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class MagicDust : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static readonly Dictionary<SlotModificationManager.ModificationType, Ability> SlotIDs = new();

        public static List<Ability> AllGemAbilities = new() { Ability.GainGemTriple, Ability.GainGemOrange, Ability.GainGemGreen, Ability.GainGemBlue };

        public class MagicDustSlot : NonCardTriggerReceiver
        {
            public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;

            private const string MOD_ID = "MAGIC_DUST_ID";

            private static CardModificationInfo GetGemMod(PlayableCard card, bool create = false)
            {
                CardModificationInfo mod = card.TemporaryMods.FirstOrDefault(m => m.singletonId.Equals(MOD_ID));
                if (mod == null && create)
                    mod = new() { singletonId = MOD_ID };

                return mod;
            }

            public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
            {
                if (otherCard.Slot == null)
                    yield break;

                SlotModificationManager.ModificationType slotMod = otherCard.Slot.GetSlotModification();
                if (SlotIDs.Keys.Contains(slotMod))
                {
                    CardModificationInfo mod = GetGemMod(otherCard, true);
                    mod.abilities = new() { SlotIDs[slotMod] };
                    otherCard.AddTemporaryMod(mod);
                    ResourcesManager.Instance.ForceGemsUpdate();
                }
                else
                {
                    CardModificationInfo mod = GetGemMod(otherCard);
                    if (mod != null)
                    {
                        otherCard.RemoveTemporaryMod(mod);
                        ResourcesManager.Instance.ForceGemsUpdate();
                    }
                }
            }
        }

        public static SlotModificationManager.ModificationType SlotModID { get; private set; }

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
                P03Plugin.PluginGuid,
                info,
                typeof(MagicDust),
                TextureHelper.GetImageAsTexture("ability_magic_dust.png", typeof(MagicDust).Assembly)
            ).Id;

            SlotIDs[SlotModificationManager.New(
                P03Plugin.PluginGuid,
                "BlueGemslot",
                typeof(MagicDustSlot),
                TextureHelper.GetImageAsTexture("cardslot_blue_gem.png", typeof(MagicDust).Assembly)
            )] = Ability.GainGemBlue;

            SlotIDs[SlotModificationManager.New(
                P03Plugin.PluginGuid,
                "GreenGemslot",
                typeof(MagicDustSlot),
                TextureHelper.GetImageAsTexture("cardslot_green_gem.png", typeof(MagicDust).Assembly)
            )] = Ability.GainGemGreen;

            SlotIDs[SlotModificationManager.New(
                P03Plugin.PluginGuid,
                "OrangeGemslot",
                typeof(MagicDustSlot),
                TextureHelper.GetImageAsTexture("cardslot_orange_gem.png", typeof(MagicDust).Assembly)
            )] = Ability.GainGemOrange;

            SlotIDs[SlotModificationManager.New(
                P03Plugin.PluginGuid,
                "TripleGemslot",
                typeof(MagicDustSlot),
                TextureHelper.GetImageAsTexture("cardslot_triple_gem.png", typeof(MagicDust).Assembly)
            )] = Ability.GainGemTriple;
        }

        private List<Ability> CardGemAbilities => new(AllGemAbilities.Where(Card.HasAbility));

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            Ability target = CardGemAbilities[0];
            yield return Card.Slot.SetSlotModification(SlotIDs.First(kvp => kvp.Value == target).Key);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => CardGemAbilities.Count > 0;
    }
}
