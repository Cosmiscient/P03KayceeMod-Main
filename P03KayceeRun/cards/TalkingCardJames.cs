using System.Collections;
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
    public class TalkingCardJames : DiskTalkingCard, ITalkingCard
    {
        public static readonly DialogueEvent.Speaker ID = DialogueManagement.NewCardSpeaker("James");
        public override DialogueEvent.Speaker SpeakerType => ID;
        public override string OnDrawnDialogueId => string.Empty;
        public override string OnDrawnFallbackDialogueId => string.Empty;
        public override string OnPlayFromHandDialogueId => "JamesOnPlayFromHand";
        public override string OnAttackedDialogueId => "JamesOnAttacked";
        public override string OnSacrificedDialogueId => "JamesOnSacrificed";
        public override string OnBecomeSelectablePositiveDialogueId => string.Empty;
        public override string OnBecomeSelectableNegativeDialogueId => string.Empty;

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() => base.OnPlayFromHand();

        private static readonly Dictionary<Opponent.Type, string> specialIds = new();
        public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => specialIds;

        public static string Name = "P03KCM_JAMES_TALKING";
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
            return Sprite.Create(TextureHelper.GetImageAsTexture(filename, typeof(TalkingCardJames).Assembly), new Rect(0f, 0f, 114f, 94f), new Vector2(0.5f, 0.5f));
        }

        private static readonly List<EmotionData> _emotions = new()
        {
            new(Emotion.Neutral,
                AssetHelpers.MakeSprite("_"),
                AssetHelpers.MakeSpriteTuple(("_", "_")),
                (GetSprite("James_Open.png"), GetSprite("James_Closed.png")),
                AssetHelpers.MakeSpriteTuple(("_", "_"))
            )
        };

        public List<EmotionData> Emotions => _emotions;

        private static readonly FaceInfo _faceInfo = new(2f, "speechblip_jamescobb", 1f);
        public FaceInfo FaceInfo => _faceInfo;

        public SpecialTriggeredAbility DialogueAbility => _dialogueAbility;
        private static SpecialTriggeredAbility _dialogueAbility = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "TalkingCardJames", typeof(TalkingCardJames)).Id;

        static TalkingCardJames()
        {
            var talkingCard = CardManager.New(P03Plugin.CardPrefx, Name, "Painbot", 0, 3)
                       .SetCost(energyCost: 2)
                       .SetIceCube("EmptyVessel")
                       .SetCardTemple(CardTemple.Tech);

            talkingCard.mods = new() { new CardModificationInfo() { gemify = true } };

            TalkingCardManager.New<TalkingCardJames>();

            talkingCard.AnimatedPortrait.GetComponentInChildren<CharacterFace>().voiceSoundId = _faceInfo.voiceId;

            // A quick check to see if the API has been updated yet
            if (talkingCard.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.AnimatedPortrait))
            {

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
}