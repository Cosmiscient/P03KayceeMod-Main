using System.Collections;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class TrackKrakenDeaths : SpecialCardBehaviour
    {
        public static readonly SpecialTriggeredAbility ID = SpecialTriggeredAbilityManager.Add(P03Pack3Plugin.PluginGuid, "TrackKrakenDeaths", typeof(TrackKrakenDeaths)).Id;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => Pack3Quests.KrakenLord.IsDefaultActive();

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            Pack3Quests.KrakenLord.IncrementQuestCounter();
            yield break;
        }
    }
}