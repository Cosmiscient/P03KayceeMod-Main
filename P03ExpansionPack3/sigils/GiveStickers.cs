using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Cards.Stickers;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class GiveStickers : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private int RandomSeed = P03AscensionSaveData.RandomSeed;

        private static readonly Dictionary<Ability, bool> KnownValidAbilities = new();

        static GiveStickers()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Sticker Lord";
            info.rulebookDescription = "While [creature] is on the board, all friendly cards get a random sticker.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Pack3Plugin.PluginGuid,
                info,
                typeof(GiveStickers),
                TextureHelper.GetImageAsTexture("ability_all_randomability.png", typeof(GiveStickers).Assembly)
            ).Id;
        }

        private static bool IsValidAbility(Ability a)
        {
            if (KnownValidAbilities.ContainsKey(a))
                return KnownValidAbilities[a];

            var info = AbilityManager.AllAbilities.AbilityByID(a);
            bool isValid = info.AbilityBehavior.GetMethod(nameof(AbilityBehaviour.OnResolveOnBoard)).DeclaringType != typeof(TriggerReceiver);
            KnownValidAbilities[a] = isValid;
            return isValid;
        }

        private Ability ChooseAbility(PlayableCard card)
        {
            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, 0, 3, AbilityMetaCategory.Part3Modular);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || x == TreeStrafe.AbilityID || card.HasAbility(x) || x == RotatingAlarm.AbilityID || x == Ability.DeathShield);
            learnedAbilities.RemoveAll((Ability x) => !IsValidAbility(x));
            learnedAbilities.RemoveAll((Ability x) => string.IsNullOrEmpty(AbilitiesUtil.GetInfo(x).GetExtendedProperty(Stickers.STICKER_PROPERTY_KEY)));

            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, RandomSeed++)]
                : Ability.Sharp;
        }

        private IEnumerator StickerUpCard(PlayableCard otherCard)
        {
            CardModificationInfo mod = new(ChooseAbility(otherCard));
            string stickerName = AbilitiesUtil.GetInfo(mod.abilities[0]).GetExtendedProperty(Stickers.STICKER_PROPERTY_KEY);

            var stickerData = new Stickers.CardStickerData();
            stickerData.Rotations[stickerName] = new Vector3(0f, 180f, 110f + SeededRandom.Value(RandomSeed++) * 20f);
            stickerData.Positions[stickerName] = new Vector3(0.2301786f + (SeededRandom.Value(RandomSeed++) / 10f), -0.2767847f + (SeededRandom.Value(RandomSeed++) / 10f), 0f);
            stickerData.Ability[stickerName] = mod.abilities[0];

            mod.singletonId = stickerData.ToString();

            otherCard.AddTemporaryMod(mod);
            otherCard.Status.hiddenAbilities.Add(mod.abilities[0]);
            otherCard.RenderCard();
            yield return new WaitForEndOfFrame();
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            foreach (var slot in BoardManager.Instance.GetSlotsCopy(Card.IsPlayerCard()))
            {
                if (slot.Card != null)
                {
                    yield return StickerUpCard(slot.Card);
                }
            }
        }

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => otherCard.IsPlayerCard() == Card.IsPlayerCard();

        public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            yield return StickerUpCard(otherCard);
        }
    }
}
