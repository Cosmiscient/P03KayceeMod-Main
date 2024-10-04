using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class SideDeckPower : VariableStatBehaviour
    {
        public static SpecialStatIcon AbilityID { get; private set; }
        public override SpecialStatIcon IconType => AbilityID;

        public static int EnergySpent { get; private set; }

        static SideDeckPower()
        {
            StatIconInfo info = StatIconManager.New(P03SigilLibraryPlugin.PluginGuid,
                "Side Deck Power",
                "The value represented with this sigil will be equal to the number of cards its owner has drawn from the side deck.",
                typeof(SideDeckPower));
            info.appliesToAttack = true;
            info.appliesToHealth = false;
            info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook);
            info.SetIcon(TextureHelper.GetImageAsTexture("vessel_stat_icon.png", typeof(SideDeckPower).Assembly));
            AbilityID = info.iconType;
        }

        public override int[] GetStatValues()
        {
            int power = 10 - CardDrawPiles3D.Instance.SideDeck.Cards.Count;
            return new int[] { power, 0 };
        }
    }
}