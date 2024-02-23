using System;
using System.Collections;
using System.Linq;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Cards;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Resource;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    public class GoobertHuh : ConsumableItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        internal static Tuple<Color, string> GetGoobertRulebookDialogue()
        {
            // We look at the name of the current state of the goobert quest to figure out what to display in the rulebook
            QuestState goobertState = DefaultQuestDefinitions.FindGoobert.CurrentState;

            if (goobertState == DefaultQuestDefinitions.FindGoobert.InitialState || goobertState.StateName.EndsWith("P03WhereIsGoobert"))
            {
                return new(GameColors.Instance.brightLimeGreen, "Please! You've got to help me get out of here!");
            }

            if (Part3SaveData.Data.items.Contains(ItemData.name))
            {
                return new(GameColors.Instance.brightLimeGreen, "Thank you! I hope he doesn't notice me here...");
            }

            if (DefaultQuestDefinitions.FindGoobert.CurrentState.StateName.ToLowerInvariant().EndsWith("P03GoobertHome"))
            {
                return new(GameColors.Instance.brightLimeGreen, "Thank you!");
            }

            // Okay, you only get to this point if you've bought goobert but don't have him anymore.
            // If he's in your collection as a card, we'll say something different
            return Part3SaveData.Data.deck.Cards.Any(c => c.name == CustomCards.MYCO_CONSTRUCT_BASE)
                ? new(GameColors.Instance.brightLimeGreen, "So much power, but so much pain...")
                : new(GameColors.Instance.brightBlue, "Good riddance to that little freak.");
        }

        public static GameObject GetGameObject()
        {
            GameObject gameObject = ShockerItem.GetBaseGameObject("Prefabs/Items/GooBottleItem", "GoobertBottle");
            Destroy(gameObject.GetComponentInChildren<GooBottleItem>());
            gameObject.AddComponent<GoobertHuh>();
            return gameObject;
        }

        static GoobertHuh()
        {
            string prefabPathKey = "p03kayceemodgoobert";
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{prefabPathKey}", GetGameObject());

            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "Goobert",
                "Please! You've got to help me get out of here!",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(ShockerItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(LifeItem),
                GetGameObject() // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("eyeball_squish")
            .SetPickupSoundId("eyeball_squish")
            .SetPlacedSoundId("eyeball_drop_metal")
            .SetRegionSpecific(true)
            .SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);
        }

        public override IEnumerator ActivateSequence()
        {
            ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
            yield return new WaitForSeconds(0.2f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03GoobertAnnoyed", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            PlayShakeAnimation();
            yield return TextDisplayer.Instance.PlayDialogueEvent("GoobertConfused", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03GoobertShutUp", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.3f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);

            // Mark the quest as a failure - you lost Goobert.
            DefaultQuestDefinitions.FindGoobert.CurrentState.Status = QuestState.QuestStateStatus.Failure;

            PlayExitAnimation();
            yield break;
        }
    }
}