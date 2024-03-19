using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class FullOfOil : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        static FullOfOil()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Full of Oil";
            info.rulebookDescription = "When [creature] dies, it adds 3 health to the creature on either side and across from it.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.flipYIfOpponent = true;
            info.passive = false;
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_fullofoil.png", typeof(FullyLoaded).Assembly));
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(FullOfOil),
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(FullOfOil).Assembly)
            ).Id;
        }

        public static IEnumerator ThrowOil(CardSlot fromSlot, CardSlot toSlot, float speed = 0.35f)
        {
            if (toSlot.Card == null)
                yield break;

            fromSlot.Card?.Anim.StrongNegationEffect();
            GameObject bomb = Instantiate(ResourceBank.Get<GameObject>("prefabs/map/holomapscenery/HoloSlime_Pile_2"));
            OnboardDynamicHoloPortrait.HolofyGameObject(bomb, GameColors.instance.darkBlue);
            bomb.transform.position = fromSlot.transform.position + (Vector3.up * 0.1f);

            Vector3 midpoint = Vector3.Lerp(fromSlot.transform.position, toSlot.transform.position, 0.5f) + (Vector3.up * 0.25f);

            Tween.Position(bomb.transform, midpoint, speed / 2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, toSlot.transform.position, speed / 2f, speed / 2f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, toSlot.transform.position - (Vector3.up * 0.2f), 0.1f, speed, Tween.EaseIn, Tween.LoopType.None, null, () => Destroy(bomb), true);

            yield return new WaitForSeconds(speed);

            AudioController.Instance.PlaySound3D("eyeball_squish", MixerGroup.TableObjectsSFX, toSlot.transform.position, .7f, randomization: new AudioParams.Randomization(), pitch: new AudioParams.Pitch(AudioParams.Pitch.Variation.Medium));
            toSlot.Card.Anim.StrongNegationEffect();

            yield return new WaitForSeconds(0.1f);
            toSlot.Card.TemporaryMods.Add(new(0, 3));
            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            List<CardSlot> targets = new();
            targets.AddRange(BoardManager.Instance.GetAdjacentSlots(Card.Slot));
            targets.Add(Card.Slot.opposingSlot);
            targets.RemoveAll(s => s == null || s.Card == null);

            if (targets.Count == 0)
                yield break;

            View originalView = ViewManager.Instance.CurrentView;
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.15f);

            foreach (CardSlot slot in targets)
            {
                yield return ThrowOil(Card.Slot, slot);
                // Card.Anim.StrongNegationEffect();
                // slot.Card.Anim.StrongNegationEffect();
                // yield return new WaitForSeconds(0.25f);
                // slot.Card.TemporaryMods.Add(new(0, 3));
                // yield return new WaitForSeconds(0.25f);
            }

            ViewManager.Instance.SwitchToView(originalView, false, false);

            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;


    }
}
