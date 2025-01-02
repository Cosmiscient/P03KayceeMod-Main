using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ConditionalDynamicPortrait : CardAppearanceBehaviour
    {
        public static Appearance ID { get; private set; }

        static ConditionalDynamicPortrait()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ConditionalDynamicPortrait", typeof(ConditionalDynamicPortrait)).Id;
        }

        private GameObject _animatedPortrait;
        private GameObject AnimatedPortrait
        {
            get
            {
                if (_animatedPortrait != null)
                    return _animatedPortrait;

                _animatedPortrait = CardLoader.GetCardByName("!BOUNTYHUNTER_BASE").animatedPortrait;
                return _animatedPortrait;
            }
        }

        public override void ApplyAppearance()
        {
            Card.RenderInfo.prefabPortrait = null;
            Card.RenderInfo.hidePortrait = false;
            //base.Card.RenderInfo.portraitColor = GameColors.Instance.gold;
            if (Card is PlayableCard pCard)
            {
                if (pCard.temporaryMods.Any(mod => mod.bountyHunterInfo != null))
                {
                    Card.RenderInfo.prefabPortrait = AnimatedPortrait;
                    Card.RenderInfo.hidePortrait = true;
                    Card.RenderInfo.nameOverride = pCard.temporaryMods.First(mod => mod.bountyHunterInfo != null).nameReplacement;
                    P03Plugin.Log.LogDebug($"Bounty hunter name override {Card.renderInfo.nameOverride}");
                }
            }
            else
            {
                if (Card.Info.Mods.Any(mod => mod.bountyHunterInfo != null))
                {
                    Card.RenderInfo.prefabPortrait = AnimatedPortrait;
                    Card.RenderInfo.hidePortrait = true;
                    Card.RenderInfo.nameOverride = Card.Info.Mods.First(mod => mod.bountyHunterInfo != null).nameReplacement;
                    P03Plugin.Log.LogDebug($"Bounty hunter name override {Card.renderInfo.nameOverride}");
                }
            }
        }

        public override void ResetAppearance()
        {
            Card.RenderInfo.prefabPortrait = null;
            Card.RenderInfo.hidePortrait = false;
        }

        public override void OnPreRenderCard() => ApplyAppearance();
    }
}