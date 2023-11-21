using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class BountyTargetBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        private static QuestDefinition Quest => DefaultQuestDefinitions.BountyTarget;

        private static int DamageThisRun
        {
            get => ModdedSaveManager.RunState.GetValueAsInt(P03Plugin.PluginGuid, "BountyDamageThisRun");
            set => ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "BountyDamageThisRun", value);
        }

        static BountyTargetBehaviour()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "BountyTargetBehaviour", typeof(BountyTargetBehaviour)).Id;
        }

        private static readonly string[] NAMES = new string[] { "Scared", "Weenie", "Dork", "Dumb", "Oopsie", "Loser", "Tiny", "Shrimp", "Beige", "Vomit", "Puke" };
        private static readonly string[] NAME_MODS = new string[] { "Mc", "ass", "Van ", "pants", "face" };

        private static readonly Texture2D DECAL = TextureHelper.GetImageAsTexture("decal_wanted_poster.png", typeof(BountyTargetBehaviour).Assembly);

        private static Texture2D _cachedPortrait;

        private static Texture2D GetPortrait(DeathCardInfo dcInfo)
        {
            if (_cachedPortrait != null)
                return _cachedPortrait;

            _cachedPortrait = TextureHelper.DuplicateTexture(Resources.Load<Texture2D>($"art/cards/deathcardportraits/deathcard_base"));
            Texture2D head = TextureHelper.DuplicateTexture(Resources.Load<Texture2D>($"art/cards/deathcardportraits/deathcard_head_{dcInfo.headType}"));
            Texture2D eyes = TextureHelper.DuplicateTexture(Resources.Load<Texture2D>($"art/cards/deathcardportraits/deathcard_eyes_{dcInfo.eyesIndex + 1}"));
            Texture2D mouth = TextureHelper.DuplicateTexture(Resources.Load<Texture2D>($"art/cards/deathcardportraits/deathcard_mouth_{dcInfo.mouthIndex + 1}"));

            for (int x = 0; x < head.width; x++)
            {
                for (int y = 0; y < head.height; y++)
                {
                    if (head.GetPixel(x, y).a > 0)
                        _cachedPortrait.SetPixel(x, y, head.GetPixel(x, y));
                }
            }

            for (int x = 0; x < eyes.width; x++)
            {
                for (int y = 0; y < eyes.height; y++)
                    _cachedPortrait.SetPixel(x + 40, y + 32, eyes.GetPixel(x, y));
            }

            for (int x = 0; x < mouth.width; x++)
            {
                for (int y = 0; y < mouth.height; y++)
                    _cachedPortrait.SetPixel(x + 40, y + 15, mouth.GetPixel(x, y));
            }

            _cachedPortrait.Apply();
            return _cachedPortrait;
        }

        private static CardModificationInfo GetFreshMod()
        {
            CardModificationInfo info = new(1, 12);
            int seed = SaveManager.saveFile.GetCurrentRandomSeed() + 110;
            CompositeFigurine.FigurineType head = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, seed++);
            info.deathCardInfo = SeededRandom.Value(seed++) < 0.2
                ? new(head, true)
                : new(head, SeededRandom.Range(0, 6, seed++), SeededRandom.Range(0, 6, seed++));

            string firstName = NAMES[SeededRandom.Range(0, NAMES.Length, seed++)];
            string secondName = NAMES[SeededRandom.Range(0, NAMES.Length, seed++)];
            while (firstName.Equals(secondName))
                secondName = NAMES[SeededRandom.Range(0, NAMES.Length, seed++)];
            string modifier = NAME_MODS[SeededRandom.Range(0, NAME_MODS.Length, seed++)];

            info.nameReplacement = modifier[0] == modifier.ToUpperInvariant()[0] ? $"{firstName} {modifier}{secondName}" : $"{firstName} {secondName}{modifier}";

            return info;
        }

        private static CardModificationInfo GetCurrentMod()
        {
            string currentCardMod = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, "CurrentBountyMod");
            if (string.IsNullOrEmpty(currentCardMod))
            {
                CardModificationInfo info = GetFreshMod();
                string saveValue = info.nameReplacement
                                 + ";" + ((int)info.deathCardInfo.headType).ToString()
                                 + ";" + info.deathCardInfo.eyesIndex.ToString()
                                 + ";" + info.deathCardInfo.mouthIndex.ToString()
                                 + ";" + (info.deathCardInfo.lostEye ? "Y" : "N");
                ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, "CurrentBountyMod", saveValue);
                return info;
            }
            string[] modData = currentCardMod.Split(';');
            return new(1, 12 - DamageThisRun)
            {
                nameReplacement = modData[0],
                deathCardInfo = new((CompositeFigurine.FigurineType)int.Parse(modData[1]), int.Parse(modData[3]), int.Parse(modData[2]))
                {
                    lostEye = modData[4] == "Y"
                }
            };
        }

        public static CardInfo GetBountyTarget()
        {
            CardInfo dummyCard = ScriptableObject.CreateInstance<CardInfo>();
            dummyCard.name = "!BOUNTYTARGET";

            dummyCard.decals = new()
            {
                CustomCards.DUMMY_DECAL,
                CustomCards.DUMMY_DECAL_2,
                DECAL
            };

            CardModificationInfo mod = GetCurrentMod();
            dummyCard.SetPortrait(GetPortrait(mod.deathCardInfo));
            dummyCard.specialAbilities = new() { AbilityID };

            dummyCard.mods = new() { mod };
            return dummyCard;
        }

        public override bool RespondsToOtherCardDealtDamage(PlayableCard attacker, int amount, PlayableCard target) => target == PlayableCard;

        public override IEnumerator OnOtherCardDealtDamage(PlayableCard attacker, int amount, PlayableCard target)
        {
            if (target == PlayableCard)
                DamageThisRun += amount;
            yield break;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return NPCDescriptor.SayDialogue(Quest.EventId, "P03BountyTargetEnters");
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            yield return NPCDescriptor.SayDialogue(Quest.EventId, "P03BountyTargetCaught");
            Quest.CurrentState.Status = QuestState.QuestStateStatus.Success;
            yield break;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => !playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            List<CardSlot> slots = BoardManager.Instance.OpponentSlotsCopy;
            CardSlot newSlot = null;
            for (int i = PlayableCard.Slot.Index; i < slots.Count; i++)
            {
                if (slots[i].Card == null)
                {
                    newSlot = slots[i];
                    break;
                }
            }
            if (newSlot == null)
            {
                yield return NPCDescriptor.SayDialogue(Quest.EventId, "P03BountyTargetLeaves");
                PlayableCard.ExitBoard(0.2f, PlayableCard.transform.position + new Vector3(5f, 0f, 0f));
                yield return new WaitForSeconds(0.15f);
                yield break;
            }
            yield return BoardManager.Instance.AssignCardToSlot(PlayableCard, newSlot);
        }

        [HarmonyPatch(typeof(BountyHunterGenerator), nameof(BountyHunterGenerator.TryAddBountyHunterToTurnPlan))]
        [HarmonyPrefix]
        private static bool AddBountyTargetInstead(List<List<CardInfo>> turnPlan, ref List<List<CardInfo>> __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (!Quest.IsDefaultActive() || (Part3SaveData.Data.battlesSinceBountyHunter >= 3 && Part3SaveData.Data.BountyTier >= 1))
                return true;

            // So we're always going to add the bounty target to turn 3. Always.
            Part3SaveData.Data.battlesSinceBountyHunter += 1;

            // Find the card with the lowest power level
            List<CardInfo> turnThree = turnPlan[2];
            if (turnThree.Count > 0)
            {
                int minLevel = turnThree.Min(c => c.PowerLevel);
                turnThree.Remove(turnThree.First(c => c.PowerLevel == minLevel));
            }
            turnThree.Add(GetBountyTarget());

            __result = turnPlan;
            return false;
        }

        [HarmonyPatch(typeof(BoardStateEvaluator), nameof(BoardStateEvaluator.EvaluateCard))]
        [HarmonyPostfix]
        private static void MakeAIPreferLeftmostBountyTarget(BoardState.CardState card, BoardState board, ref int __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (Quest.IsDefaultActive())
            {
                if (board.opponentSlots.Contains(card.slot))
                {
                    if (card.info.mods.Any(m => m.deathCardInfo != null))
                        __result -= Math.Abs(board.opponentSlots.IndexOf(card.slot) * 2);
                }
            }
        }
    }
}