using HarmonyLib;
using InscryptionAPI.Card;
using DiskCardGame;
using InscryptionAPI.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class ExpansionPackCards_2
    {
        internal const string EXP_2_PREFIX = "P03KCMXP2";

        static ExpansionPackCards_2()
        {
            CardManager.New(EXP_2_PREFIX, "SwapperLatcher", "Swapper Latcher", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_swapper_latcher.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 4)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(LatchSwapper.AbilityID);

            CardManager.New(EXP_2_PREFIX, "BoxBot", "Box Bot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_boxbot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetRegionalP03Card(CardTemple.Wizard)
                .AddAbilities(Ability.Brittle, VesselHeart.AbilityID);

            CardManager.New(EXP_2_PREFIX, "ScrapBot", "Scrap Bot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_scrapbot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 3)
                .SetNeutralP03Card()
                .AddAbilities(ScrapDumper.AbilityID);

            CardManager.New(EXP_2_PREFIX, "RobotRingworm", "R!N9W0RM", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_scrapbot.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetRare()
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(TakeDamageSigil.AbilityID);

            CardManager.New(EXP_2_PREFIX, "Shovester", "Shovester", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_shovester.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetNeutralP03Card()
                .AddAbilities(Shove.AbilityID);

            CardManager.New(EXP_2_PREFIX, "Librarian", "Librarian", 1, 2)
                .SetPortrait(TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_librarian"), TextureHelper.SpriteType.CardPortrait))
                .SetCost(energyCost: 1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Reach, DeadByte.AbilityID);


        }
    }
}