using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionCommunityPatch.Card;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;
using System.Linq;
using Pixelplacement;
using System;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class Molotov : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static Molotov()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Flammable";
            info.rulebookDescription = "When [creature] dies, it detonates and sets adjacent spaces on fire for three turns";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            Molotov.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Molotov),
                TextureHelper.GetImageAsTexture("ability_molotov.png", typeof(Molotov).Assembly)
            ).Id;
        }

        private IEnumerator BombCard(CardSlot target, PlayableCard attacker)
        {
            GameObject bomb = GameObject.Instantiate<GameObject>(AssetBundleManager.Prefabs["Molotov"]);
            OnboardDynamicHoloPortrait.HolofyGameObject(bomb, GameColors.instance.glowRed);
            bomb.transform.position = attacker.transform.position + Vector3.up * 0.1f;

            var midpoint = Vector3.Lerp(attacker.Slot.transform.position, target.transform.position, 0.5f) + (Vector3.up * 0.25f);

            Tween.Position(bomb.transform, midpoint, 0.25f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, target.transform.position, 0.25f, 0.25f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, target.transform.position - (Vector3.up * 0.2f), 0.1f, 0.5f, Tween.EaseIn, Tween.LoopType.None, null, () => GameObject.Destroy(bomb), true);
            Tween.LocalRotation(bomb.transform, Quaternion.Euler(new(90f, 0f, 0f)), 0.5f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);

            yield return new WaitForSeconds(0.5f);
            AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, target.transform.position, .7f);
            target.Card?.Anim.PlayHitAnimation();

            // The fireball should play and then delete itself, but we'll destroy it after some time anyway
            var fireball = GameObject.Instantiate<GameObject>(AssetBundleManager.Prefabs["Fire_Ball"], target.transform);
            CustomCoroutine.WaitThenExecute(3f, delegate ()
            {
                if (fireball != null)
                    GameObject.Destroy(fireball);
            });

            yield return new WaitForSeconds(1f);
            yield return target.SetSlotModification(FireBomb.OnFire[2]);
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => this.Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            this.Card.Anim.LightNegationEffect();
            var adjSlots = BoardManager.Instance.GetAdjacentSlots(this.Card.Slot);
            if (adjSlots.Count > 0 && adjSlots[0].Index < this.Card.Slot.Index)
            {
                yield return BombCard(adjSlots[0], this.Card);
                adjSlots.RemoveAt(0);
            }
            yield return BombCard(this.Card.Slot.opposingSlot, this.Card);
            if (adjSlots.Count > 0 && adjSlots[0].Index > this.Card.Slot.Index)
            {
                yield return BombCard(adjSlots[0], this.Card);
            }
        }
    }
}
