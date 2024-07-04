using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class GreenGemPower : VariableStatBehaviour
    {
        public static SpecialStatIcon AbilityID { get; private set; }
        public override SpecialStatIcon IconType => AbilityID;

        public static int EnergySpent { get; private set; }

        static GreenGemPower()
        {
            StatIconInfo info = StatIconManager.New(P03SigilLibraryPlugin.PluginGuid,
                "Emerald Power",
                "The value represented with this sigil will be equal to the number of emerald gems its owner controls",
                typeof(GreenGemPower));
            info.appliesToAttack = true;
            info.appliesToHealth = false;
            info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
            info.SetIcon(TextureHelper.GetImageAsTexture("green_gen_stat_icon.png", typeof(GreenGemPower).Assembly));
            AbilityID = info.iconType;
        }

        private bool CardProvidesGreenGem(CardSlot slot)
        {
            return slot.Card != null &&
                   (slot.Card.HasAbility(Ability.GainGemGreen) || slot.Card.HasAbility(Ability.GainGemTriple));
        }

        public override int[] GetStatValues()
        {
            int power = BoardManager.Instance
                                    .GetSlots(!PlayableCard.OpponentCard)
                                    .Where(CardProvidesGreenGem)
                                    .Count();
            return new int[] { power, 0 };
        }
    }
}