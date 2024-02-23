using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Miner : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int triggerPriority = int.MinValue;
        public override int Priority => triggerPriority;

        static Miner()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Miner";
            info.rulebookDescription = "[creature] buries itself during its opponent's turn. While buried, opposing creatures attack its owner directly. When it comes back up, it creates a card in its owner's hand.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_miner.png", typeof(Miner).Assembly));

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Miner),
                TextureHelper.GetImageAsTexture("ability_miner.png", typeof(Miner).Assembly)
            ).Id;
        }

        // I would love to just derive from Submerge but DM for some reason make OnResurface 
        // a void return type instead of an IEnumerator return type so it doesn't work for our
        // use case. So I basically have to copy/paste the whole Submerge ability back in here

        public override bool RespondsToUpkeep(bool playerUpkeep) => Card.OpponentCard != playerUpkeep && Card.FaceDown;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            ViewManager.Instance.SwitchToView(View.Board, false, true);
            yield return new WaitForSeconds(0.15f);
            yield return PreSuccessfulTriggerSequence();
            Card.SetFaceDown(false, false);
            Card.UpdateFaceUpOnBoardEffects();
            yield return OnResurface();
            yield return new WaitForSeconds(0.3f);
            triggerPriority = int.MinValue;
            yield break;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card.OpponentCard != playerTurnEnd && !Card.FaceDown;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            ViewManager.Instance.SwitchToView(View.Board, false, true);
            yield return new WaitForSeconds(0.15f);
            Card.SetCardbackSubmerged();
            Card.SetFaceDown(true, false);
            yield return new WaitForSeconds(0.3f);
            yield return LearnAbility(0f);
            triggerPriority = int.MaxValue;
            yield break;
        }

        private IEnumerator OnResurface()
        {
            List<CardInfo> list = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Part3Random));
            CardInfo cardToDraw = list[SeededRandom.Range(0, list.Count, GetRandomSeed())];
            if (ViewManager.Instance.CurrentView != View.Hand)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Hand, false, false);
                yield return new WaitForSeconds(0.2f);
            }
            yield return CardSpawner.Instance.SpawnCardToHand(cardToDraw, waitTime: 0.25f);
            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
            yield break;
        }
    }
}