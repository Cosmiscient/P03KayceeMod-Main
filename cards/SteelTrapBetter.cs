using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class BetterSteelTrap : SteelTrap
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private DiskCardAnimationController DiskAnim => Card.Anim as DiskCardAnimationController;

        internal static GameObject _holoTrapCopy;
        internal static GameObject HoloTrapCopy
        {
            get
            {
                if (_holoTrapCopy != null)
                    return _holoTrapCopy;

                GameObject roomParent = Resources.Load<GameObject>($"prefabs/map/holomapareas/HoloMapArea_NeutralWest_Secret");
                _holoTrapCopy = roomParent.transform.Find("Nodes/HoloPeltMinigame/HoloTrap").gameObject;
                return _holoTrapCopy;
            }
        }

        public override CardInfo CardToDraw
        {
            get
            {
                if (Card.Slot.opposingSlot.Card != null)
                {
                    if (Card.Slot.opposingSlot.Card.Anim is DiskCardAnimationController)
                        return CardLoader.GetCardByName("EmptyVessel");
                }

                return CardLoader.GetCardByName("PeltWolf");
            }
        }

        static BetterSteelTrap()
        {
            AbilityManager.FullAbility infoClone = AbilityManager.BaseGameAbilities.AbilityByID(Ability.SteelTrap);

            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Holo Trap";
            info.rulebookDescription = "When a card bearing this sigil perishes, the creature opposing it perishes as well. A Vessel is created in your hand.";
            info.canStack = false;
            info.powerLevel = infoClone.Info.powerLevel;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(BetterSteelTrap),
                infoClone.Texture
            ).Id;
        }

        public override bool RespondsToTakeDamage(PlayableCard source) => base.RespondsToTakeDamage(source) && DiskAnim == null;

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => !wasSacrifice && base.RespondsToTakeDamage(null) && DiskAnim != null;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            if (wasSacrifice)
                yield break;

            if (DiskAnim == null)
            {
                yield return OnTakeDamage(null);
                yield break;
            }

            yield return new WaitForSeconds(0.65f);
            AudioController.Instance.PlaySound3D("sacrifice_default", MixerGroup.TableObjectsSFX, Card.transform.position, 1f, 0f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.1f);

            GameObject trap = Instantiate(HoloTrapCopy, DiskAnim.holoPortraitParent);
            trap.transform.localEulerAngles = new(0f, 0f, 0f);
            trap.transform.localScale = new(1.2f, 1.2f, 1.2f);
            trap.transform.localPosition = new(0f, -.35f, 0f);
            yield return new WaitForSeconds(0.25f);
            trap.GetComponentInChildren<Animator>().Play("shut", 0, 1f);
            AudioController.Instance.PlaySound3D("dial_metal", MixerGroup.TableObjectsSFX, Card.transform.position, 1f, 0f);
            yield return new WaitForSeconds(1f);
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            Destroy(trap);
            yield break;
        }
    }
}