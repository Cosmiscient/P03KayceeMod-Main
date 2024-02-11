using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    internal static class MultiverseCards
    {
        internal static void CreateCards()
        {
            CardManager.New(P03Plugin.CardPrefx, "MultiverseMole", "M013", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_transformer_mole.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 4)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(MultiverseMole.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseFirewall", "Firewall", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_firewall.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 4)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(MultiverseMole.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseMineCart", "49er", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_minecart"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseStrafe.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseSentry", "Sentry Drone", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_sentrybot"))
                .SetCost(energyCost: 1)
                .AddAbilities(MultiverseSentry.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBolthound", "Bolthound", 2, 3)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_bolthound"))
                .SetCost(energyCost: 6)
                .AddAbilities(MultiverseGuardian.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBombbot", "Explode Bot", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_bombbot"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseExplodeOnDeath.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseConduitNull", "Null Conduit", 0, 2)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_conduitnull"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseExplodeOnDeath.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseGunner", "Multi Gunner", 2, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_gunnerbot"))
                .SetCost(energyCost: 6)
                .AddAbilities(MultiverseStrike.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseTechMoxTriple", "Mox Module", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_triplemox.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 3)
                .AddAbilities(MultiverseTripleGem.AbilityID)
                .AddPart3Decal(TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_2.png", typeof(CustomCards).Assembly))
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBombLatcher", "Bomb Latcher", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_bomblatcher"))
                .SetCost(energyCost: 1)
                .AddAbilities(MultiverseBombLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseShieldLatcher", "Shield Latcher", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_shieldlatcher"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseShieldLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBrittleLatcher", "Brittle Latcher", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_brittlelatcher"))
                .SetCost(energyCost: 3)
                .AddAbilities(MultiverseBrittleLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);
        }
    }
}