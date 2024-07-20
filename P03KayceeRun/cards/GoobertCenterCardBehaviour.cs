using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Sequences;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class GoobertCenterCardBehaviour : SpecialCardBehaviour, IRestoreFromSnapshot
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        public static List<GoobertCenterCardBehaviour> Instances { get; private set; } = new();

        public static bool IsOnBoard => Instances.IsOnBoard();

        public enum SlotPosition
        {
            Center = 0,
            Left = 1,
            Right = 2
        }

        private SlotPosition _currentPosition = SlotPosition.Center;

        public CardSlot CenterSlot
        {
            get
            {
                if (_currentPosition == SlotPosition.Left)
                {
                    List<CardSlot> slots = MultiverseBattleSequencer.GetParentSlotList(PlayableCard.Slot);
                    return slots[slots.IndexOf(PlayableCard.Slot) + 1];
                }
                if (_currentPosition == SlotPosition.Right)
                {
                    List<CardSlot> slots = MultiverseBattleSequencer.GetParentSlotList(PlayableCard.Slot);
                    return slots[slots.IndexOf(PlayableCard.Slot) - 1];
                }
                return PlayableCard.Slot;
            }
        }

        public List<CardSlot> AllSlots
        {
            get
            {
                List<CardSlot> slots = MultiverseBattleSequencer.GetParentSlotList(PlayableCard.Slot);
                int sidx = slots.IndexOf(PlayableCard.Slot);
                if (_currentPosition == SlotPosition.Left)
                    return new() { slots[sidx], slots[sidx + 1], slots[sidx + 2] };
                if (_currentPosition == SlotPosition.Right)
                    return new() { slots[sidx - 2], slots[sidx - 1], slots[sidx] };

                return new() { slots[sidx - 1], slots[sidx], slots[sidx + 1] };
            }
        }

        private void AssignToInternalSlot(SlotPosition newPosition)
        {
            if (newPosition != _currentPosition)
            {
                CardSlot centerSlot = CenterSlot;
                if (newPosition == SlotPosition.Center)
                {
                    PlayableCard.slot = centerSlot;
                }
                else if (newPosition == SlotPosition.Left)
                {
                    List<CardSlot> slots = MultiverseBattleSequencer.GetParentSlotList(centerSlot);
                    PlayableCard.slot = slots[slots.IndexOf(centerSlot) - 1];
                }
                else if (newPosition == SlotPosition.Right)
                {
                    List<CardSlot> slots = MultiverseBattleSequencer.GetParentSlotList(centerSlot);
                    PlayableCard.slot = slots[slots.IndexOf(centerSlot) + 1];
                }
                P03Plugin.Log.LogInfo($"{PlayableCard} is now officially in slot {PlayableCard.Slot}");
                _currentPosition = newPosition;
            }
        }

        static GoobertCenterCardBehaviour()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03Plugin.PluginGuid, "GoobertCenterCardAppearance", typeof(GoobertCenterCardBehaviour)).Id;
        }

        private bool IsInMycoBoss => TurnManager.Instance.Opponent is MycologistAscensionBossOpponent;

        public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
        {
            DropHighlightedConduitsForTripleCard();
            return false;
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            DropHighlightedConduitsForTripleCard();
            return false;
        }

        public override bool RespondsToResolveOnBoard() => true;

        private float GetGoobertEntrySpeed() => IsInMycoBoss ? 3f : restoredFromSnapshot ? 0.001f : 1f;

        private float GetArmEntrySpeed() => IsInMycoBoss ? 2f : restoredFromSnapshot ? 0.001f : 0.7f;

        internal static CardModificationInfo GetExperimentModInfo()
        {
            CardModificationInfo info = new();
            info.nameReplacement = "Experiment #" + AscensionStatsData.GetStatValue(StatManagement.EXPERIMENTS_CREATED, true);
            int randomAbilityCount = 0;
            foreach (CardInfo card in EventManagement.MycologistTestSubjects)
            {
                info.healthAdjustment += card.Health;
                info.attackAdjustment += card.Attack;
                if (card.Gemified)
                {
                    info.healthAdjustment += 2;
                    info.attackAdjustment += 1;
                }
                foreach (Ability ab in card.Abilities)
                {
                    if (ab == Ability.RandomAbility)
                    {
                        randomAbilityCount += 1;
                    }
                    else if (ab == Ability.Transformer)
                    {
                        CardModificationInfo beastTransformer = card.mods.FirstOrDefault(m => !string.IsNullOrEmpty(m.transformerBeastCardId));
                        if (beastTransformer != null)
                        {
                            info.healthAdjustment -= card.Health;
                            info.attackAdjustment -= card.Attack;
                            CardInfo transformer = CustomCards.ConvertCodeToCard(beastTransformer.transformerBeastCardId);
                            info.healthAdjustment += transformer.Health;
                            info.attackAdjustment += transformer.Attack;
                            info.abilities.AddRange(transformer.abilities.Where(a => a != Ability.Transformer));
                        }
                    }
                    else
                    {
                        if (!info.abilities.Contains(ab) || AbilitiesUtil.GetInfo(ab).canStack)
                            info.abilities.Add(ab);
                    }
                }
            }
            for (int i = 0; i < randomAbilityCount; i++)
            {
                List<Ability> possibles = AbilitiesUtil.GetLearnedAbilities(false, 0, 5, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
                possibles.RemoveAll(a => info.abilities.Contains(a));
                info.abilities.Add(possibles[SeededRandom.Range(0, possibles.Count, P03AscensionSaveData.RandomSeed + i)]);
            }
            return info;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            Instances.RemoveAll(g => g.SafeIsUnityNull());
            Instances.Add(this);
            DiskCardAnimationController dcac = Card.Anim as DiskCardAnimationController;

            PlayableCard.RenderInfo.hidePortrait = true;
            PlayableCard.SetInfo(PlayableCard.Info);

            if (!IsInMycoBoss && !restoredFromSnapshot)
                ViewManager.Instance.SwitchToView(View.Default, false, true);

            // Get the goobert face
            GameObject goobert = GoobertHuh.GetGameObject();
            goobert.transform.SetParent(dcac.holoPortraitParent);
            ConsumableItem itemcontroller = goobert.GetComponentInChildren<ConsumableItem>();
            Destroy(itemcontroller);
            Destroy(goobert.GetComponentInChildren<GooWizardAnimationController>());

            Animator[] animators = goobert.GetComponentsInChildren<Animator>();
            foreach (Animator anim in animators)
                Destroy(anim);
            goobert.transform.Find("GooBottleItem(Clone)/GooWizardBottle/GooWizard/Bottle").gameObject.SetActive(false);
            goobert.transform.Find("GooBottleItem(Clone)/GooWizardBottle/GooWizard/Cork").gameObject.SetActive(false);
            OnboardDynamicHoloPortrait.HolofyGameObject(goobert, GameColors.Instance.darkLimeGreen);

            Transform gooWizard = goobert.transform.Find("GooBottleItem(Clone)/GooWizardBottle/GooWizard");
            gooWizard.localEulerAngles = new(90f, 0f, 0f);

            Vector3 target = new(-.1f, -.7f, .6f);
            goobert.transform.localPosition = target + Vector3.down;
            goobert.transform.localEulerAngles = new(46.2497f, 121.8733f, 222.0276f);
            Tween.LocalPosition(goobert.transform, target, GetGoobertEntrySpeed(), 0f);

            if (IsInMycoBoss)
                StartCoroutine(GooSpeakBackground());

            //this.Card.RenderInfo.hidePortrait = true;
            Card.SetInfo(Card.Info);
            Card.RenderCard();
            yield return new WaitForSeconds(GetGoobertEntrySpeed());

            if (IsInMycoBoss)
            {
                ViewManager.Instance.SwitchToView(View.Default, false, true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("GooWTF2", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            }

            // Make room for the left and right halves of the card
            List<CardSlot> friendlySlots = new(PlayableCard.OpponentCard ? BoardManager.Instance.opponentSlots : BoardManager.Instance.playerSlots);
            int mySlot = friendlySlots.IndexOf(PlayableCard.Slot);

            int leftSlot = mySlot - 1;
            yield return MakeSlotEmpty(friendlySlots[leftSlot], true);
            int rightSlot = mySlot + 1;
            yield return MakeSlotEmpty(friendlySlots[rightSlot], false);

            // Get the arm
            GameObject rightArm = Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloClaw"), dcac.holoPortraitParent);
            GameObject leftArm = Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloClaw"), dcac.holoPortraitParent);
            OnboardDynamicHoloPortrait.HolofyGameObject(rightArm, GameColors.Instance.darkLimeGreen);
            OnboardDynamicHoloPortrait.HolofyGameObject(leftArm, GameColors.Instance.darkLimeGreen);
            rightArm.transform.localPosition = new(-0.5f, -1f, 0f);
            leftArm.transform.localPosition = new(0.5f, -1f, 0f);
            rightArm.transform.localEulerAngles = new(0f, 0f, 270f);
            leftArm.transform.localEulerAngles = new(0f, 180f, 270f);
            Tween.LocalPosition(rightArm.transform, new Vector3(-0.5f, 0f, 0f), GetArmEntrySpeed(), 0f);
            Tween.LocalPosition(leftArm.transform, new Vector3(0.5f, 0f, 0f), GetArmEntrySpeed(), 0f);
            yield return new WaitForSeconds(GetArmEntrySpeed());

            SwapAnimationController(PlayableCard, true);

            // Rotate the arm into place
            Tween.LocalPosition(leftArm.transform, new Vector3(1.14f, -0.08f, 0f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalRotation(leftArm.transform, new Vector3(0f, 180f, 34f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalPosition(rightArm.transform, new Vector3(-1.14f, -0.08f, 0f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalRotation(rightArm.transform, new Vector3(0f, 0f, 34f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);

            // Scale the cards
            Tween.LocalScale(gameObject.transform.Find("Anim/CardBase"), new Vector3(0.5263f, 2.5f, 1f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalScale(gameObject.transform.Find("Anim/ShieldEffect"), new Vector3(6f, 7.9f, 1.7f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalScale(gameObject.transform.Find("Anim/CardBase/HoloportraitParent"), new Vector3(.8f / 2.5f, 1f, 1f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalScale(gameObject.transform.Find("Anim/CardBase/Top/Name"), new Vector3(0.5f, -1f, 1f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);
            Tween.LocalScale(gameObject.transform.Find("Anim/CardBase/Top/Fuse"), new Vector3(1.3f, 0.4f, 1f), restoredFromSnapshot ? 0.001f : 0.3f, 0f);

            if (TurnManager.Instance.opponent is MycologistAscensionBossOpponent)
                PlayableCard.AddTemporaryMod(GetExperimentModInfo());

            //PlayableCard.SetInfo(PlayableCard.Info);

            AudioController.Instance.PlaySound3D("mushroom_large_appear", MixerGroup.TableObjectsSFX, gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            if (!restoredFromSnapshot)
                yield return new WaitForSeconds(0.3f);

            AddMushroom(-1.78f, -.26f, dcac.holoPortraitParent);
            AddMushroom(-.97f, .2436f, dcac.holoPortraitParent);
            AddMushroom(1.7f, .4f, dcac.holoPortraitParent);
            AddMushroom(.8f, -.12f, dcac.holoPortraitParent);

            if (!restoredFromSnapshot)
                yield return new WaitForSeconds(1.5f);

            DropHighlightedConduitsForTripleCard();

            if (!IsInMycoBoss)
                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            yield break;
        }

        private void AddMushroom(float x, float z, Transform parent)
        {
            GameObject mushroom = Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloMushroom_1"), parent);
            OnboardDynamicHoloPortrait.HolofyGameObject(mushroom, GameColors.Instance.darkLimeGreen);
            Vector3 target = new(x, -.7f, z);
            mushroom.transform.localPosition = target + Vector3.down;
            Tween.LocalPosition(mushroom.transform, target, 1f, 0f);
        }

        private static void SwapAnimationController(PlayableCard card, bool isMain = false)
        {
            GameObject obj = card.gameObject;
            DiskCardAnimationController thisOldController = obj.GetComponent<DiskCardAnimationController>();
            TripleDiskCardAnimationController newThisAnim = obj.AddComponent<TripleDiskCardAnimationController>();
            newThisAnim.InitializeWith(thisOldController);
            Destroy(thisOldController);

            // Update the dynamic stretchers in the live render cameras

            DiskScreenCardDisplayer displayer = CardRenderCamera.Instance.GetLiveRenderCamera(card.StatsLayer).GetComponentInChildren<DiskScreenCardDisplayer>();
            Transform abilityParent = displayer.gameObject.transform.Find("CardAbilityIcons_Part3");
            UpdateAllStretchers(abilityParent, card);

            Transform liveAbilityParent = card.gameObject.transform.Find("Anim/CardBase/Bottom/CardAbilityIcons_Part3_Invisible");
            UpdateAllStretchers(liveAbilityParent, card);
        }

        private static void UpdateAllStretchers(Transform abilityParent, PlayableCard card)
        {
            for (int i = 1; i <= 12; i++)
            {
                string name = i == 1 ? "DefaultIcons_1Ability" : $"DefaultIcons_{i}Abilities";
                GameObject container = abilityParent.Find(name).gameObject;
                TripleCardRenderIcons.InverseStretch stretcher = container.GetComponent<TripleCardRenderIcons.InverseStretch>();
                if (stretcher != null)
                    stretcher.MatchingTransform = card.gameObject.transform.Find("Anim/CardBase");
            }
        }

        private IEnumerator MakeSlotEmpty(CardSlot slot, bool left = true)
        {
            if (slot.Card == null || slot.Card == this.Card)
                yield break;

            var friendlySlots = BoardManager.Instance.GetSlotsCopy(slot.IsPlayerSlot);
            int index = friendlySlots.IndexOf(slot);
            int nextIndex = left ? index - 1 : index + 1;

            if (index < 0 || index >= friendlySlots.Count)
            {
                slot.Card.ExitBoard(0.1f, Vector3.down);
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            if (nextIndex < 0 || nextIndex >= friendlySlots.Count)
                yield break;

            yield return MakeSlotEmpty(friendlySlots[nextIndex], left);
            yield return BoardManager.Instance.AssignCardToSlot(slot.Card, friendlySlots[nextIndex]);
        }

        private IEnumerator GooSpeakBackground()
        {
            yield return TextDisplayer.Instance.PlayDialogueEvent("GooWTF", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
        }

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.RenderCard))]
        [HarmonyPrefix]
        private static bool LiveRenderGooCard(CardRenderInfo info, ref RenderStatsLayer __instance)
        {
            if (__instance is DiskRenderStatsLayer drsl && info.baseInfo.specialAbilities.Contains(AbilityID))
            {
                CardRenderCamera.Instance.LiveRenderCard(info, drsl, drsl.PlayableCard);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.GetAdjacentSlots))]
        [HarmonyPrefix]
        private static bool GetAdjacentAccountingForTripleCard(CardSlot slot, ref List<CardSlot> __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (TurnManager.Instance == null || TurnManager.Instance.GameIsOver() || !IsOnBoard)
                return true;

            if (slot.Card == null)
                return true;

            GoobertCenterCardBehaviour gcb = slot.Card.GetComponent<GoobertCenterCardBehaviour>();

            if (gcb == null)
                return true;

            if (slot.Card.Dead)
                return true;

            List<CardSlot> slotsToCheck = MultiverseBattleSequencer.GetParentSlotList(slot);
            int idx = slotsToCheck.IndexOf(slot);

            __result = new();
            for (int i = idx; i >= 0; i--)
            {
                if (slotsToCheck[i].Card == null || slotsToCheck[i].Card != gcb.PlayableCard)
                {
                    __result.Add(slotsToCheck[i]);
                    break;
                }
            }
            for (int i = idx; i < slotsToCheck.Count; i++)
            {
                if (slotsToCheck[i].Card == null || slotsToCheck[i].Card != slot.Card)
                {
                    __result.Add(slotsToCheck[i]);
                    break;
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.SacrificesCreateRoomForCard))]
        [HarmonyPrefix]
        private static bool EnsureRoomForTripleCard(PlayableCard card, ref bool __result)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (card.Info.specialAbilities.Contains(AbilityID))
            {
                int emptiesInARow = 0;
                for (int i = 0; i < BoardManager.Instance.playerSlots.Count; i++)
                {
                    if (BoardManager.Instance.playerSlots[i].Card == null)
                    {
                        emptiesInARow += 1;
                        if (emptiesInARow == 3)
                        {
                            __result = true;
                            return false;
                        }
                    }
                    else
                    {
                        emptiesInARow = 0;
                    }
                }
                __result = false;
                return false;
            }
            return true;
        }

        internal static void DropHighlightedConduitsForTripleCard()
        {
            foreach (CardSlot slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
                slot.gameObject.transform.Find("ConduitBorder/GravityParticles").gameObject.SetActive(slot.Card == null || !slot.Card.Info.HasSpecialAbility(AbilityID));
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPrefix]
        private static void EnsureSlotsOnUpkeep()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            Instances.RemoveAll(g => g.SafeIsUnityNull());
            DropHighlightedConduitsForTripleCard();
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        private static void EnsureSlotsOnCleanup()
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            DropHighlightedConduitsForTripleCard();
        }

        [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.Card), MethodType.Getter)]
        [HarmonyPostfix]
        private static void TripleCardGetter(CardSlot __instance, ref PlayableCard __result)
        {
            if (__result != null)
                return;

            foreach (var gcs in Instances.OnBoard().Where(g => !g.PlayableCard.Dead))
            {
                CardSlot slot = gcs.CenterSlot;
                if (slot.IsPlayerSlot != __instance.IsPlayerSlot)
                    continue;

                if (slot.Index - 1 == __instance.Index || slot.Index + 1 == __instance.Index || slot.Index == __instance.Index)
                {
                    __result = gcs.PlayableCard;
                    return;
                }
            }
        }

        private static IEnumerator OriginalAssignCardToSlot(PlayableCard card, CardSlot slot, float transitionDuration = 0.1f, Action tweenCompleteCallback = null, bool resolveTriggers = true)
        {
            CardSlot slot2 = card.Slot;

            if (card.Slot != null)
                card.Slot.Card = null;

            if (slot.Card != null)
                slot.Card.Slot = null;

            card.SetEnabled(false);
            slot.Card = card;
            card.Slot = slot;
            card.RenderCard();

            if (!slot.IsPlayerSlot)
                card.SetIsOpponentCard(true);

            card.transform.parent = slot.transform;
            card.Anim.PlayRiffleSound();
            Tween.LocalPosition(card.transform, Vector3.up * (BoardManager.Instance.SlotHeightOffset + card.SlotHeightOffset), transitionDuration, 0.05f, Tween.EaseOut, Tween.LoopType.None, null, delegate ()
            {
                tweenCompleteCallback?.Invoke();
                card.Anim.PlayRiffleSound();
            }, true);

            Tween.Rotation(card.transform, slot.transform.GetChild(0).rotation, transitionDuration, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            if (resolveTriggers && slot2 != card.Slot)
            {
                yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.OtherCardAssignedToSlot, false, new object[]
                {
                    card
                });
            }
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        private static IEnumerator HackyTripleCardReassignmentStrategy(IEnumerator sequence, PlayableCard card, CardSlot slot, float transitionDuration = 0.1f, Action tweenCompleteCallback = null, bool resolveTriggers = true)
        {
            if (!P03AscensionSaveData.IsP03Run)
            {
                yield return sequence;
                yield break;
            }

            // If this is a triple card we might actually change which slot its being assigned to:
            if (!card.Info.specialAbilities.Contains(AbilityID))
            {
                yield return sequence;
                yield break;
            }

            // Okay, is this not on board yet?
            if (!card.OnBoard)
            {
                // If the slot can fit the card, great - just play it
                if (slot.SlotCanHoldTripleCard(card))
                {
                    yield return OriginalAssignCardToSlot(card, slot, transitionDuration, tweenCompleteCallback, resolveTriggers);
                    yield break;
                }

                // Otherwise, find a slot where it does fit
                List<CardSlot> container = BoardManager.Instance.GetSlots(slot.IsPlayerSlot);
                foreach (var testSlot in container)
                {
                    if (testSlot.SlotCanHoldTripleCard(card))
                    {
                        yield return OriginalAssignCardToSlot(card, testSlot, transitionDuration, tweenCompleteCallback, resolveTriggers);
                        yield break;
                    }
                }

                throw new InvalidOperationException("This shouldn't be possible - I was unable to play Goobert for some reason!");
            }

            // Okay, the card is currently on board. 
            List<CardSlot> boardSlots = BoardManager.Instance.GetSlots(slot.IsPlayerSlot);
            GoobertCenterCardBehaviour gcb = card.GetComponent<GoobertCenterCardBehaviour>();

            // Okay - are you trying to move to the other side of the board?
            // Secret behavior! We kill all cards in the way.
            if (slot.IsPlayerSlot != card.Slot.IsPlayerSlot)
            {
                List<CardSlot> otherSlots = BoardManager.Instance.GetSlots(!slot.IsPlayerSlot);
                CardSlot targetSlot = slot.Index == 0 ? otherSlots[1]
                                      : slot.Index == otherSlots.Count - 1 ? otherSlots[otherSlots.Count - 2]
                                      : slot;
                for (int i = -1; i <= 1; i++)
                    while (otherSlots[targetSlot.Index + i].Card != null)
                        otherSlots[targetSlot.Index + i].Card.ExitBoard(0.5f, Vector3.down * 5);

                yield return OriginalAssignCardToSlot(card, targetSlot, transitionDuration, tweenCompleteCallback, resolveTriggers);
                yield break;
            }

            // If you're trying to move this card to a slot two spaces away from the
            // center, we actually try to move it one slot away. We assume this is a strafe
            if (gcb.CenterSlot.Index + 2 == slot.Index)
            {
                CardSlot newTarget = boardSlots[gcb.CenterSlot.Index + 1];
                if (newTarget.SlotCanHoldTripleCard())
                {
                    yield return OriginalAssignCardToSlot(card, newTarget, transitionDuration, tweenCompleteCallback, resolveTriggers);
                    yield break;
                }
            }
            if (gcb.CenterSlot.Index - 2 == slot.Index)
            {
                CardSlot newTarget = boardSlots[gcb.CenterSlot.Index - 1];
                if (newTarget.SlotCanHoldTripleCard())
                {
                    yield return OriginalAssignCardToSlot(card, newTarget, transitionDuration, tweenCompleteCallback, resolveTriggers);
                    yield break;
                }
            }

            // Finally, just try this

            if (slot.SlotCanHoldTripleCard(card))
            {
                yield return sequence;
                yield break;
            }

            // Okay. We don't fit. Is there any place we can fit that will cover this slot?
            // So we give up
            card.Anim.StrongNegationEffect();
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.CardsOnBoard), MethodType.Getter)]
        [HarmonyPostfix]
        private static void RemoveDuplicateGoobertCards(ref List<PlayableCard> __result)
        {
            if (!IsOnBoard)
                return;

            __result = __result.Distinct().ToList();
        }

        [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.RespondsToTrigger))]
        [HarmonyPrefix]
        private static bool WouldHaveRespondedInAnotherSlot(CardTriggerHandler __instance, ref bool __result, Trigger trigger, object[] otherArgs)
        {
            if (!IsOnBoard)
                return true;

            GoobertCenterCardBehaviour gcb = Instances.FirstOrDefault(g => !g.SafeIsUnityNull() && g.PlayableCard != null && g.PlayableCard.TriggerHandler == __instance);

            if (gcb == null)
                return true;

            // Okay, we need to check if this card would respond in any of the three possible positions
            __result = false;
            var currentPosition = gcb._currentPosition;

            P03Plugin.Log.LogInfo($"Testing {gcb.PlayableCard} to see if it will trigger {trigger}");
            foreach (SlotPosition pos in Enum.GetValues(typeof(SlotPosition)))
            {
                gcb.AssignToInternalSlot(pos);
                foreach (TriggerReceiver receiver in __instance.GetAllReceivers())
                {
                    if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                    {
                        __result = true;
                        P03Plugin.Log.LogInfo($"Triggered in position {pos} for {receiver}");
                        break;
                    }
                }
            }
            P03Plugin.Log.LogInfo($"Returning back to {currentPosition}");
            gcb.AssignToInternalSlot(currentPosition);
            return false;
        }

        [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.OnTrigger))]
        [HarmonyPostfix]
        private static IEnumerator HandleTripleCardTrigger(IEnumerator sequence, CardTriggerHandler __instance, Trigger trigger, object[] otherArgs)
        {
            if (!IsOnBoard)
            {
                yield return sequence;
                yield break;
            }

            GoobertCenterCardBehaviour gcb = Instances.FirstOrDefault(g => !g.SafeIsUnityNull() && g.PlayableCard != null && g.PlayableCard.TriggerHandler == __instance);

            if (gcb == null)
            {
                yield return sequence;
                yield break;
            }

            var currentPosition = gcb._currentPosition;
            foreach (TriggerReceiver receiver in __instance.GetAllReceivers())
            {
                P03Plugin.Log.LogInfo($"Testing {receiver} for {gcb.PlayableCard} to see if it will trigger {trigger}");
                List<SlotPosition> positiveResponses = new();
                foreach (SlotPosition pos in Enum.GetValues(typeof(SlotPosition)))
                {
                    gcb.AssignToInternalSlot(pos);
                    if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs))
                    {
                        P03Plugin.Log.LogInfo($"Positive trigger for {receiver} in position {pos}");
                        positiveResponses.Add(pos);
                    }
                }

                // If you would have triggered in all three spaces, we only trigger
                // from the center slot. With *ONE* exception - the ondie trigger
                P03Plugin.Log.LogInfo($"Found {positiveResponses.Count} positive triggers for {receiver} {trigger}");
                if (positiveResponses.Count == 3 && !GoobertCenterCardBehaviourHelpers.CanFireOnAllSlots(trigger, receiver))
                {
                    P03Plugin.Log.LogInfo($"Triggering {receiver} {gcb.PlayableCard} ONCE in center slot.");
                    gcb.AssignToInternalSlot(SlotPosition.Center);
                    yield return GlobalTriggerHandler.Instance.TriggerSequence(trigger, receiver, otherArgs);
                }
                else
                {
                    P03Plugin.Log.LogInfo($"Iterating through {positiveResponses.Count} positive triggers");
                    foreach (var p in positiveResponses)
                    {
                        P03Plugin.Log.LogInfo($"Triggering {receiver} {gcb.PlayableCard} in {p}.");
                        gcb.AssignToInternalSlot(p);
                        yield return GlobalTriggerHandler.Instance.TriggerSequence(trigger, receiver, otherArgs);
                    }
                }
            }
            P03Plugin.Log.LogInfo($"Returning back to {currentPosition}");
            gcb.AssignToInternalSlot(currentPosition);
            yield break;
        }

        private bool restoredFromSnapshot = false;
        public void RestoreFromSnapshot(BoardState.CardState state)
        {
            restoredFromSnapshot = true;
            StartCoroutine(OnResolveOnBoard());
        }
    }
}