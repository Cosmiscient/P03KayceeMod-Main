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
    public class TalkingCardSawyer : DiskTalkingCard, ITalkingCard
    {
        public static readonly DialogueEvent.Speaker ID = DialogueManagement.NewCardSpeaker("Sawyer");
        public override DialogueEvent.Speaker SpeakerType => ID;
        public override string OnDrawnDialogueId => "SawyerOnDrawn";
        public override string OnDrawnFallbackDialogueId => string.Empty;
        public override string OnPlayFromHandDialogueId => "SawyerOnPlayFromHand";
        public override string OnAttackedDialogueId => "SawyerOnAttacked";
        public override string OnSacrificedDialogueId => "SawyerOnSacrificed";
        public override string OnDiscoveredInExplorationDialogueId => "SawyerOnDiscoveredInExploration";
        public override string OnBecomeSelectablePositiveDialogueId => "SawyerOnBecomeSelectablePositive";
        public override string OnBecomeSelectableNegativeDialogueId => "SawyerOnBecomeSelectableNegative";

        private static readonly List<string> OTHER_DOGS = new() { "BoltHound", "P03KCM_BoltHound", "CXformerWolf", "P03KCMXP1_WolfBot", "P03KCMXP1_WolfBeast", "P03KCM_CXformerAlpha" };
        public override bool RespondsToOtherCardResolve(PlayableCard otherCard) => OTHER_DOGS.Contains(otherCard.Info.name);
        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            this.TriggerSoloDialogue("SawyerOtherDog");
            yield break;
        }

        private static readonly Dictionary<Opponent.Type, string> specialIds = new()
        {
            [Opponent.Type.ArchivistBoss] = "SawyerOnDrawnArchivist",
            [BossManagement.P03FinalBossOpponent] = "SawyerOnDrawnP03",
            [BossManagement.P03MultiverseOpponent] = "SawyerOnDrawnP03Multiverse"
        };
        public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => specialIds;

        public static string Name = "P03KCM_SAWYER_TALKING";
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
            return Sprite.Create(TextureHelper.GetImageAsTexture(filename, typeof(TalkingCardSawyer).Assembly), new Rect(0f, 0f, 114f, 94f), new Vector2(0.5f, 0.5f));
        }

        private static readonly List<EmotionData> _emotions = new()
        {
            new(Emotion.Neutral,
                AssetHelpers.MakeSprite("_"),
                (GetSprite("talkingsawyer_eyes_open.png"), AssetHelpers.MakeSprite("_")),
                (GetSprite("talkingsawyer_mouth_open.png"), GetSprite("talkingsawyer_mouth_closed.png")),
                AssetHelpers.MakeSpriteTuple(("_", "_"))
            )
        };

        public List<EmotionData> Emotions => _emotions;

        private static readonly FaceInfo _faceInfo = new(2f, "speechblip_sawyerpatel", 1f);
        public FaceInfo FaceInfo => _faceInfo;

        public SpecialTriggeredAbility DialogueAbility => _dialogueAbility;
        private static SpecialTriggeredAbility _dialogueAbility = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "TalkingCardSawyer", typeof(TalkingCardSawyer)).Id;

        static TalkingCardSawyer()
        {
            var talkingCard = CardManager.New(P03Plugin.CardPrefx, Name, "Dogbot", 0, 4)
                       .SetCost(energyCost: 3)
                       .AddAbilities(Ability.DoubleDeath)
                       .SetCardTemple(CardTemple.Tech);

            TalkingCardManager.New<TalkingCardSawyer>();

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