using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class KrakenTransformer : Transformer
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static KrakenTransformer()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Kraken Transformer";
            info.rulebookDescription = "At the beginning of your turn, [creature] will transform to, or from, Kraken mode.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Pack3Plugin.PluginGuid,
                info,
                typeof(KrakenTransformer),
                TextureHelper.GetImageAsTexture("ability_krakentransformer.png", typeof(KrakenTransformer).Assembly)
            ).Id;
        }

        public override CardInfo GetTransformCardInfo()
        {
            if (!Card.Info.name.ToLowerInvariant().Contains("tentacle") || Card.Info.evolveParams == null)
            {
                // Pick a random tentacle card
                List<CardInfo> possibles = CardLoader.AllData.Where(c => c.name.ToLowerInvariant().Contains("tentacle") && c.temple == CardTemple.Tech).ToList();
                var squid = possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed)];
                return CardLoader.Clone(squid);
            }
            else
            {
                return base.GetTransformCardInfo();
            }
        }
    }
}
