using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03SigilLibrary.Sigils;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03SigilLibrary
{
    internal static class Cards
    {
        static Cards()
        {
            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "UrchinCell", "Urch1n Cell", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_urchin_cell.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(Ability.Sharp, CellDeSubmerge.AbilityID);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "CHARGE", "Charge!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_charge.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 0)
                .SetInstaGlobalSpell()
                .AddAbilities(RefillBattery.AbilityID);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "FORCED_UPGRADE", "Upgrade!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_forced_upgrade.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .AddAbilities(ForcedUpgrade.AbilityID);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "ZAP", "Zap!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_zap.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .AddAbilities(Zap.AbilityID);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "BLAST", "Blast!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_blast.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .AddAbilities(CatchFire.AbilityID);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "DEFEND", "Defend!", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_defend.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .AddAbilities(GainShield.AbilityID);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "EMPTY_PILE_OF_SCRAP", "Junk", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_scrappile.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "Salmon", "S4LM0N", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_salmon.png", typeof(Cards).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_salmon.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech)
                .SetEvolve("Angler_Fish_More", 1);

            CardManager.BaseGameCards.CardByName("Angler_Fish_More").SetEvolve("Angler_Fish_Good", 1);

            CardManager.New(P03SigilLibraryPlugin.CardPrefix, "SEED", "Seed", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_seed.png", typeof(Cards).Assembly))
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech)
                .AddSpecialAbilities(SeedBehaviour.AbilityID);

            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                cards.CardByName("CXformerWolf").AddMetaCategories(SummonFamiliar.BeastFamiliars);
                cards.CardByName("CXformerElk").AddMetaCategories(SummonFamiliar.BeastFamiliars);
                cards.CardByName("CXformerRaven").AddMetaCategories(SummonFamiliar.BeastFamiliars);
                cards.CardByName("CXformerAdder").AddMetaCategories(SummonFamiliar.BeastFamiliars);
                return cards;
            };
        }
    }
}