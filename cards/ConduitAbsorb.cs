using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.LightningBolt;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class ConduitAbsorb : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ConduitAbsorb()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Splice Conduit";
            info.rulebookDescription = "If [creature] completes a circuit, all cards within that circuit are moved towards it, splicing with it if possible.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.conduitCell = false;
            info.conduit = true;
            info.passive = false;
            info.hasColorOverride = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitAbsorb),
                TextureHelper.GetImageAsTexture("ability_conduitsplice.png", typeof(ConduitAbsorb).Assembly)
            ).Id;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => Card.OpponentCard != playerTurnEnd;

        private CardModificationInfo CreateMergeMod(PlayableCard existingCard, PlayableCard cardToMerge)
        {
            return new CardModificationInfo()
            {
                attackAdjustment = cardToMerge.Attack,
                healthAdjustment = cardToMerge.Health,
                abilities = new(cardToMerge.AllAbilities().Where(ab => ab != Ability.ConduitNull))
            };
        }

        private IEnumerator MoveTo(CardSlot originSlot, CardSlot newSlot)
        {
            if (originSlot.Card != null)
            {
                if (newSlot.Card != null)
                {
                    if (ViewManager.Instance.CurrentView != View.Board)
                    {
                        ViewManager.Instance.SwitchToView(View.Board, false, false);
                        yield return new WaitForSeconds(0.25f);
                    }
                    Tween.Position(newSlot.Card.transform, newSlot.transform.position + (Vector3.up * 0.2f), 0.1f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
                    Tween.Position(originSlot.Card.transform, newSlot.transform.position + (Vector3.up * 0.05f), 0.1f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
                    yield return new WaitForSeconds(0.1f);

                    newSlot.Card.AddTemporaryMod(CreateMergeMod(newSlot.Card, originSlot.Card));
                    Destroy(originSlot.Card.gameObject);
                    originSlot.Card = null;
                    TableVisualEffectsManager.Instance.ThumpTable(0.3f);
                    AudioController.Instance.PlaySound3D("teslacoil_overload", MixerGroup.TableObjectsSFX, newSlot.transform.position, 1f, 0f, null, null, null, null, false);
                    GameObject gameObject = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"));
                    gameObject.GetComponent<LightningBoltScript>().EndObject = newSlot.Card.gameObject;
                    Destroy(gameObject, 0.25f);
                    newSlot.Card.Anim.StrongNegationEffect();
                    yield return new WaitForSeconds(0.25f);
                    yield return BoardManager.Instance.AssignCardToSlot(newSlot.Card, newSlot, 0.1f, null, true);
                    yield return new WaitForSeconds(0.25f);
                }
                else
                {
                    yield return BoardManager.Instance.AssignCardToSlot(originSlot.Card, newSlot, 0.1f, null, true);
                }
            }
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            List<CardSlot> slots = Card.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;

            View view = ViewManager.Instance.CurrentView;

            // Start with cards to our left
            for (int i = Card.Slot.Index - 1; i >= 0; i--)
            {
                if (ConduitCircuitManager.Instance.SlotIsWithinCircuit(slots[i]))
                    yield return MoveTo(slots[i], slots[i + 1]);
            }

            for (int i = Card.Slot.Index + 1; i < slots.Count; i++)
            {
                if (ConduitCircuitManager.Instance.SlotIsWithinCircuit(slots[i]))
                    yield return MoveTo(slots[i], slots[i - 1]);
            }

            if (ViewManager.Instance.CurrentView != view)
            {
                ViewManager.Instance.SwitchToView(view, false, false);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}
