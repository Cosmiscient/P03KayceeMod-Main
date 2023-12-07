using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Achievements;
using Pixelplacement;
using TMPro;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    [HarmonyPatch]
    internal class StickerInterfaceManager : MonoBehaviour
    {
        public static StickerInterfaceManager Instance { get; private set; }

        internal static readonly Vector3 CARD_HOME_INDEX = new(2.23f, .02f, 0f);
        internal static readonly Vector3 STICKER_HOME_POSITION = new(-2.23f, 0.01f, -1.5f);

        internal const int INTERFACE_STENCIL_NUMBER = 5;

        internal bool StickerInterfaceActive { get; set; } = false;

        private int DisplayedStickerIndex { get; set; }
        private int DisplayedCardIndex { get; set; }

        internal GameObject LastPrintedSticker { get; set; }
        internal SelectableCard LastDisplayedCard { get; set; }

        private Part3DeckReviewSequencer sequencer;
        private GameObject interfaceContainer;
        private TextMeshPro buttonText;
        private GameObject computerScreen;

        private void FlyOutLastDisplayedCard()
        {
            if (LastDisplayedCard != null)
            {
                GameObject refCard = LastDisplayedCard.gameObject;
                Tween.LocalPosition(
                    refCard.transform,
                    refCard.transform.localPosition + new Vector3(0f, 0f, -2f),
                    .2f, 0f,
                    completeCallback: () => Destroy(refCard)
                );
            }
        }

        private void DisplayCurrentCard()
        {
            FlyOutLastDisplayedCard();

            GameObject card = Instantiate(sequencer.cardArray.selectableCardPrefab, sequencer.transform);

            card.transform.localPosition = CARD_HOME_INDEX + new Vector3(0f, 0f, 2f);
            LastDisplayedCard = card.GetComponentInChildren<SelectableCard>();
            LastDisplayedCard.Initialize(Part3SaveData.Data.deck.Cards[DisplayedCardIndex]);
            LastDisplayedCard.SetEnabled(false);
            Tween.LocalPosition(
                card.transform,
                CARD_HOME_INDEX,
                .2f, 0f
            );
        }

        private void RightCardButtonClicked()
        {
            if (!StickerInterfaceActive)
                return;

            if (DisplayedCardIndex == Part3SaveData.Data.deck.Cards.Count - 1)
                DisplayedCardIndex = 0;
            else
                DisplayedCardIndex += 1;
            DisplayCurrentCard();
        }

        private void LeftCardButtonClicked()
        {
            if (!StickerInterfaceActive)
                return;

            if (DisplayedCardIndex == 0)
                DisplayedCardIndex = Part3SaveData.Data.deck.Cards.Count - 1;
            else
                DisplayedCardIndex -= 1;
            DisplayCurrentCard();
        }

        private void RightButtonClicked()
        {
            if (!StickerInterfaceActive)
                return;

            if (DisplayedStickerIndex == Stickers.AllStickerKeys.Count - 1)
                DisplayedStickerIndex = 0;
            else
                DisplayedStickerIndex += 1;
            ShowStickerAtIndex();
        }

        private void LeftButtonClicked()
        {
            if (!StickerInterfaceActive)
                return;

            if (DisplayedStickerIndex == 0)
                DisplayedStickerIndex = Stickers.AllStickerKeys.Count - 1;
            else
                DisplayedStickerIndex -= 1;
            ShowStickerAtIndex();
        }

        private void FlyOutLastPrintedSticker()
        {
            if (LastPrintedSticker != null && LastPrintedSticker.transform.parent == transform)
            {
                GameObject refObject = LastPrintedSticker;
                Tween.LocalPosition(
                    refObject.transform,
                    refObject.transform.localPosition + new Vector3(0f, -2f, 0f),
                    0.35f, 0f,
                    completeCallback: () => Destroy(refObject)
                );
            }
        }

        private void PrintOrRecallSticker()
        {
            FlyOutLastPrintedSticker();

            string stickerKey = Stickers.AllStickerKeys[DisplayedStickerIndex];

            // Check to see if the sticker is on the currently visible card
            if (Stickers.IsStickerApplied(stickerKey))
            {
                foreach (Projector proj in LastDisplayedCard.GetComponentsInChildren<Projector>())
                {
                    if (proj.transform.parent.gameObject.name.Equals(stickerKey))
                    {
                        Tween.Position(proj.transform.parent, proj.transform.parent.position + new Vector3(0f, -2f, 0f), 0.2f, 0f,
                            completeCallback: delegate ()
                            {
                                Destroy(proj.transform.parent.gameObject);
                            });
                        break;
                    }
                }
                Stickers.ClearStickerAppearance(stickerKey);
            }

            // Right now only print is implemented
            GameObject sticker = Stickers.GetSticker(Stickers.AllStickerKeys[DisplayedStickerIndex], true, true, Stickers.StickerStyle.Standard);
            sticker.transform.SetParent(computerScreen.transform.parent);
            sticker.transform.localPosition = new(
                computerScreen.transform.localPosition.x,
                STICKER_HOME_POSITION.y,
                computerScreen.transform.localPosition.z
            );
            sticker.transform.localEulerAngles = new(90f, 0f, 0f);

            StickerDrag dragger = sticker.GetComponent<StickerDrag>();
            dragger.SetEnabled(false);

            LastPrintedSticker = sticker;

            AudioController.Instance.PlaySound3D("modmachine_disk_insert", MixerGroup.TableObjectsSFX, computerScreen.transform.position, 0.5f);

            Tween.LocalPosition(sticker.transform, STICKER_HOME_POSITION, 1f, 0f, Tween.EaseInOut,
                completeCallback: delegate ()
                {
                    dragger.SetEnabled(true);
                });

            UpdateStickerDisplayer();
        }

        private void UpdateStickerDisplayer()
        {
            string stickerKey = Stickers.AllStickerKeys[DisplayedStickerIndex];
            ModdedAchievementManager.AchievementDefinition stickerAchievemnent = ModdedAchievementManager.AchievementById(Stickers.StickerRewards[stickerKey]);

            if (Stickers.IsStickerApplied(stickerKey))
            {
                buttonText.transform.parent.gameObject.SetActive(true);
                buttonText.SetText(Localization.Translate("RECALL"));
            }
            else if (!Stickers.DebugStickers && !stickerAchievemnent.IsUnlocked)
            {
                buttonText.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                buttonText.transform.parent.gameObject.SetActive(true);
                buttonText.SetText(Localization.Translate("PRINT"));
            }
        }

        private void ShowStickerAtIndex()
        {
            Transform previousSticker = interfaceContainer.transform.Find("DisplayedSticker");
            if (previousSticker != null)
                Destroy(previousSticker.gameObject);

            FlyOutLastPrintedSticker();

            string stickerKey = Stickers.AllStickerKeys[DisplayedStickerIndex];
            ModdedAchievementManager.AchievementDefinition stickerAchievemnent = ModdedAchievementManager.AchievementById(Stickers.StickerRewards[stickerKey]);

            Stickers.StickerStyle style = Stickers.StickerStyle.Standard;
            if (Stickers.IsStickerApplied(stickerKey))
            {
                style = Stickers.StickerStyle.Faded;
            }
            else if (!Stickers.DebugStickers && !stickerAchievemnent.IsUnlocked)
            {
                style = Stickers.StickerStyle.Shadow;
            }

            UpdateStickerDisplayer();

            GameObject sticker = Stickers.GetSticker(stickerKey, false, false, style);
            sticker.transform.SetParent(interfaceContainer.transform);
            sticker.name = "DisplayedSticker";
            sticker.transform.localPosition = new(0f, 1f, 0f);
            sticker.layer = LayerMask.NameToLayer("CardOffscreen");
            sticker.transform.localEulerAngles = new(0f, 0f, 0f);
            sticker.transform.localScale *= 9;
        }

        private void SetupStickerInterface(GameObject computerScreen)
        {
            this.computerScreen = computerScreen;
            Destroy(computerScreen.GetComponent<BuildACardScreen>());

            GameObject screenContents = computerScreen.transform.Find("RenderCamera/Content/BuildACardInterface").gameObject;
            GameObject screenInteractables = computerScreen.transform.Find("Anim/ScreenInteractables").gameObject;
            screenInteractables.transform.Find("STAGE_Abilities").gameObject.SetActive(false);

            interfaceContainer = screenContents.transform.Find("STAGE_Confirm").gameObject;
            GameObject clickablesContainer = screenInteractables.transform.Find("STAGE_Confirm").gameObject;

            // We will reappropriate the confirm stage section
            interfaceContainer.transform.Find("Name").gameObject.SetActive(false);
            GameObject printButton = interfaceContainer.transform.Find("Button").gameObject;
            buttonText = printButton.GetComponentInChildren<TextMeshPro>();
            buttonText.SetText(Localization.Translate("PRINT"));
            printButton.transform.localPosition = new(0f, -4f, 0f);

            GameObject printInteractable = clickablesContainer.transform.Find("Confirm").gameObject;
            printInteractable.transform.localPosition = printButton.transform.localPosition;

            DisplayedStickerIndex = 0;
            ShowStickerAtIndex();

            // Sort out the behavior of the left and right buttons
            clickablesContainer.gameObject.SetActive(true);
            GameObject leftButton = screenInteractables.transform.Find("ArrowButton_Left").gameObject;
            GameObject rightButton = screenInteractables.transform.Find("ArrowButton_Right").gameObject;

            HighlightedInteractable leftInt = leftButton.GetComponent<HighlightedInteractable>();
            HighlightedInteractable rightInt = rightButton.GetComponent<HighlightedInteractable>();

            leftInt.CursorSelectEnded = (mii) => LeftButtonClicked();
            rightInt.CursorSelectEnded = (mii) => RightButtonClicked();

            GenericMainInputInteractable printButtonInteractable = clickablesContainer.GetComponentInChildren<GenericMainInputInteractable>();
            printButtonInteractable.SetCursorType(CursorType.Pickup);
            printButtonInteractable.transform.localPosition = new(0f, -0.935f, 0.0028f);
            printButtonInteractable.transform.localScale = new(1.2f, .4f, .15f);
            printButtonInteractable.CursorSelectEnded = (mii) => PrintOrRecallSticker();

            // Make sure only the right stuff is visible
            foreach (Transform interfaceSibling in interfaceContainer.transform.parent)
            {
                if (interfaceSibling != interfaceContainer.transform)
                    interfaceSibling.gameObject.SetActive(false);
            }
        }

        internal IEnumerator ShowStickerInterfaceUntilCancelled(Part3DeckReviewSequencer sequencer)
        {
            StickerInterfaceActive = true;

            GameObject computerScreen = Instantiate(SpecialNodeHandler.Instance.buildACardSequencer.screen.gameObject, sequencer.transform);
            computerScreen.transform.localEulerAngles = new(90f, 0f, 0f);
            computerScreen.transform.localScale = new(0.8f, 0.8f, 0.8f);
            computerScreen.SetActive(true);

            Vector3 targetPos = new(-2.23f, 0.13f, 0.3721f);
            computerScreen.transform.localPosition = targetPos + new Vector3(0f, 0f, 2f);

            // GameObject printer = GameObject.Instantiate(SpecialNodeHandler.Instance.buildACardSequencer.recycleMachine.gameObject.transform.Find("Anim/CardSlot").gameObject, sequencer.transform);
            // printer.transform.localPosition = new(-2.25f, -.05f, .8f);
            // printer.transform.localEulerAngles = new(270f, 0f, 180f);
            // printer.transform.localScale = new(11.2f, 1f, 1f);

            SetupStickerInterface(computerScreen);

            this.sequencer = sequencer;

            GameObject cardLeftButton = Instantiate(computerScreen.transform.Find("Anim/ScreenInteractables/ArrowButton_Left").gameObject, sequencer.transform);
            cardLeftButton.name = "CardLeftButton";
            cardLeftButton.GetComponent<HighlightedInteractable>().CursorSelectEnded = (mii) => LeftCardButtonClicked();
            cardLeftButton.transform.localEulerAngles = new(90f, 270f, 0f);
            cardLeftButton.transform.localPosition = new(1.2f, 0f, 0f);

            GameObject cardRightButton = Instantiate(computerScreen.transform.Find("Anim/ScreenInteractables/ArrowButton_Right").gameObject, sequencer.transform);
            cardRightButton.name = "CardRightButton";
            cardRightButton.GetComponent<HighlightedInteractable>().CursorSelectEnded = (mii) => RightCardButtonClicked();
            cardRightButton.transform.localEulerAngles = new(90f, 270f, 0f);
            cardRightButton.transform.localPosition = new(3.25f, -.05f, 0f);

            Tween.LocalPosition(computerScreen.transform, targetPos, 0.25f, 0f, Tween.EaseOut,
                completeCallback: delegate ()
                {
                    DisplayCurrentCard();
                    ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                });

            yield return new WaitUntil(() => ViewManager.Instance.CurrentView != View.MapDeckReview);

            StickerInterfaceActive = false;
            Tween.LocalScale(cardLeftButton.transform, new(0f, 0f, 0f), 0.2f, 0f, completeCallback: () => Destroy(cardLeftButton));
            Tween.LocalScale(cardRightButton.transform, new(0f, 0f, 0f), 0.2f, 0f, completeCallback: () => Destroy(cardRightButton));
            Tween.LocalPosition(computerScreen.transform, targetPos + new Vector3(0f, 0f, 2f), 0.25f, 0f, Tween.EaseIn, completeCallback: () => Destroy(computerScreen));

            FlyOutLastPrintedSticker();
            FlyOutLastDisplayedCard();

            yield return new WaitForEndOfFrame();
            yield break;
        }

        protected void OnEnable() => Instance = this;
    }
}