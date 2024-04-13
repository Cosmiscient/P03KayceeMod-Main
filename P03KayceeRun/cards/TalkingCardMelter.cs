using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.TalkingCards;
using InscryptionAPI.TalkingCards.Animation;
using InscryptionAPI.TalkingCards.Create;
using InscryptionAPI.TalkingCards.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class TalkingCardMelter : DiskTalkingCard, ITalkingCard
    {
        public static readonly DialogueEvent.Speaker ID = DialogueManagement.NewCardSpeaker("Melter");
        public override DialogueEvent.Speaker SpeakerType => ID;
        public override string OnDrawnDialogueId => "MelterOnDrawn";
        public override string OnDrawnFallbackDialogueId => string.Empty;
        public override string OnPlayFromHandDialogueId => "MelterOnPlayFromHand";
        public override string OnAttackedDialogueId => "MelterOnAttacked";
        public override string OnSacrificedDialogueId => "MelterOnSacrificed";
        public override string OnDiscoveredInExplorationDialogueId => "MelterOnDiscoveredInExploration";
        public override string OnBecomeSelectablePositiveDialogueId => "MelterOnBecomeSelectablePositive";
        public override string OnBecomeSelectableNegativeDialogueId => "MelterOnBecomeSelectableNegative";

        private static readonly Dictionary<Opponent.Type, string> specialIds = new()
        {
            [Opponent.Type.TelegrapherBoss] = "MelterOnDrawnGolly",
            [BossManagement.P03FinalBossOpponent] = "MelterOnDrawnP03",
            [BossManagement.P03MultiverseOpponent] = "MelterOnDrawnP03Multiverse"
        };
        public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => specialIds;

        public static string Name = "P03KCM_MELTER_TALKING";
        public string CardName => Name;

        private static void RecursiveSetLayer(GameObject obj, string layerName)
        {
            if (obj == null)
                return;

            obj.layer = LayerMask.NameToLayer(layerName);
            for (int i = 0; i < obj.transform.childCount; i++)
                RecursiveSetLayer(obj.transform.GetChild(i)?.gameObject, layerName);
        }

        private static Sprite GetSprite(string filename)
        {
            return Sprite.Create(TextureHelper.GetImageAsTexture(filename, typeof(TalkingCardMelter).Assembly), new Rect(0f, 0f, 114f, 94f), new Vector2(0.5f, 0.5f));
        }

        private static readonly List<EmotionData> _emotions = new()
        {
            new(Emotion.Neutral,
                GetSprite("MFace_white.png"),
                (GetSprite("MEyes_Open_white.png"), GetSprite("MEyes_Closed_white.png")),
                (GetSprite("MMouth_Open_white.png"), GetSprite("MMouth_Closed_white.png")),
                AssetHelpers.MakeSpriteTuple(("_", "_"))
            )
        };

        public List<EmotionData> Emotions => _emotions;

        private static readonly FaceInfo _faceInfo = new(2f, "speechblip_melter", 1f);
        public FaceInfo FaceInfo => _faceInfo;

        public SpecialTriggeredAbility DialogueAbility => _dialogueAbility;
        private static SpecialTriggeredAbility _dialogueAbility = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "TalkingCardMelter", typeof(TalkingCardMelter)).Id;

        static TalkingCardMelter()
        {
            var talkingCard = CardManager.New(P03Plugin.CardPrefx, Name, "Melter", 2, 2)
                       .SetCost(energyCost: 4)
                       .SetIceCube(TalkingCardJames.Name)
                       .AddAbilities(Ability.IceCube)
                       .SetCardTemple(CardTemple.Tech);

            TalkingCardManager.New<TalkingCardMelter>();

            talkingCard.AnimatedPortrait.GetComponentInChildren<CharacterFace>().voiceSoundId = _faceInfo.voiceId;

            talkingCard.RemoveAppearances(CardAppearanceBehaviour.Appearance.AnimatedPortrait);
            talkingCard.AddAppearances(CardAppearanceBehaviour.Appearance.DynamicPortrait);

            foreach (var rend in talkingCard.AnimatedPortrait.GetComponentsInChildren<SpriteRenderer>())
                rend.color = new(0f, 0.755f, 1f);

            RecursiveSetLayer(talkingCard.AnimatedPortrait, "CardOffscreenEmission");

            talkingCard.AnimatedPortrait.transform.localScale = new(1f, 1f, 1f);
            talkingCard.AnimatedPortrait.transform.Find("Anim/Body").localPosition = new(0f, 0.2f, 0f);

            // Add the dialogue text
            GameObject.Instantiate(CardLoader.GetCardByName("Angler_Talking").AnimatedPortrait.transform.Find("DialogueText").gameObject, talkingCard.AnimatedPortrait.transform);
        }
    }
}