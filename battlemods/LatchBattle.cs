using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Cards.Stickers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.BattleMods
{
    [HarmonyPatch]
    public class LatchBattle : NonCardTriggerReceiver, IOnBellRung
    {
        public static BattleModManager.ID ID { get; private set; }

        public static readonly AbilityMetaCategory STICKER_ABILITY = GuidManager.GetEnumValue<AbilityMetaCategory>(P03Plugin.PluginGuid, "StickerAbility");

        static LatchBattle()
        {
            ID = BattleModManager.New(
                P03Plugin.PluginGuid,
                "Stickers",
                new List<string>() { "See those [c:bR]stickers[c:]? They give my cards extra abilities.", "You don't mind an extra challenge, do you?" },
                typeof(LatchBattle),
                difficulty: 1,
                regions: new() { CardTemple.Tech, CardTemple.Nature, CardTemple.Wizard },
                iconPath: "p03kcm/prefabs/question"
            );

            AbilityManager.ModifyAbilityList += delegate (List<AbilityManager.FullAbility> abilities)
            {
                foreach (var fab in abilities)
                {
                    if (fab.Info.metaCategories.Contains(AbilityMetaCategory.Part3Modular))
                    {
                        Stickers.AddAbilitySticker(fab);
                    }
                }
                return abilities;
            };
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.QueueCardForSlot))]
        [HarmonyPrefix]
        private static void SetQueueMaybe(PlayableCard card)
        {
            List<NonCardTriggerReceiver> handlers = new(GlobalTriggerHandler.Instance.nonCardReceivers);
            foreach (NonCardTriggerReceiver handler in handlers)
            {
                if (!handler.SafeIsUnityNull() && handler is LatchBattle lb)
                {
                    if (lb.RespondsToLatchCard(card))
                    {
                        lb.LatchCard(card);
                    }
                }
            }
        }


        public bool RespondsToLatchCard(PlayableCard otherCard) => otherCard.OpponentCard && !otherCard.HasTrait(Trait.Terrain) && !otherCard.Info.Mods.Any(m => m.bountyHunterInfo != null);

        private Ability ChooseAbility(PlayableCard card)
        {
            List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false, 0, 3, AbilityMetaCategory.Part3Modular);
            learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || x == TreeStrafe.AbilityID || card.HasAbility(x) || x == RotatingAlarm.AbilityID || x == Ability.DeathShield);
            learnedAbilities.RemoveAll((Ability x) => !AbilitiesUtil.GetInfo(x).opponentUsable);
            learnedAbilities.RemoveAll((Ability x) => string.IsNullOrEmpty(AbilitiesUtil.GetInfo(x).GetExtendedProperty(Stickers.STICKER_PROPERTY_KEY)));

            return learnedAbilities.Count > 0
                ? learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())]
                : Ability.Sharp;
        }

        // private Ability ChooseAbility(PlayableCard card)
        // {
        //     List<Ability> possibles = AbilitiesUtil.GetLearnedAbilities(false, 0, 3, STICKER_ABILITY);
        //     possibles.RemoveAll(a => card.HasAbility(a));
        //     return possibles.Count > 0
        //          ? possibles[SeededRandom.Range(0, possibles.Count, GetRandomSeed())]
        //          : Ability.Sharp;
        // }

        public void LatchCard(PlayableCard otherCard)
        {
            // CardModificationInfo mod = new(ChooseAbility(otherCard))
            // {
            //     fromLatch = true
            // };
            // otherCard.Anim.ShowLatchAbility();

            CardModificationInfo mod = new(ChooseAbility(otherCard));
            string stickerName = AbilitiesUtil.GetInfo(mod.abilities[0]).GetExtendedProperty(Stickers.STICKER_PROPERTY_KEY);

            var stickerData = new Stickers.CardStickerData();
            stickerData.Rotations[stickerName] = new Vector3(0f, 180f, 117.06f);
            stickerData.Positions[stickerName] = new Vector3(0.2301786f, -0.2767847f, 0f);
            //stickerData.Scales[stickerName] = new Vector3(10f, 10f, 10f);
            stickerData.Ability[stickerName] = mod.abilities[0];

            mod.singletonId = stickerData.ToString();

            otherCard.AddTemporaryMod(mod);
            otherCard.Status.hiddenAbilities.Add(mod.abilities[0]);
            otherCard.RenderCard();
        }

        public bool RespondsToBellRung(bool playerCombatPhase) => !playerCombatPhase;
        public IEnumerator OnBellRung(bool playerCombatPhase)
        {
            yield return BattleModManager.GiveOneTimeIntroduction(ID, View.Board);
        }
    }
}