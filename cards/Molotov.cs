using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

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
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular, FireBomb.FlamingAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Molotov),
                TextureHelper.GetImageAsTexture("ability_molotov.png", typeof(Molotov).Assembly)
            ).Id;
        }

        public static IEnumerator BombCard(CardSlot target, PlayableCard attacker, int level = 2, float speed = 0.5f)
        {
            GameObject bomb = Instantiate(AssetBundleManager.Prefabs["Molotov"]);
            OnboardDynamicHoloPortrait.HolofyGameObject(bomb, GameColors.instance.glowRed);
            bomb.transform.position = attacker.transform.position + (Vector3.up * 0.1f);

            Vector3 midpoint = Vector3.Lerp(attacker.Slot.transform.position, target.transform.position, 0.5f) + (Vector3.up * 0.25f);

            Tween.Position(bomb.transform, midpoint, speed / 2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, target.transform.position, speed / 2f, speed / 2f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, target.transform.position - (Vector3.up * 0.2f), 0.1f, speed, Tween.EaseIn, Tween.LoopType.None, null, () => Destroy(bomb), true);
            Tween.LocalRotation(bomb.transform, Quaternion.Euler(new(90f, 0f, 0f)), speed, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);

            yield return new WaitForSeconds(speed);
            AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, target.transform.position, .7f);
            target.Card?.Anim.PlayHitAnimation();

            // The fireball should play and then delete itself, but we'll destroy it after some time anyway
            GameObject fireball = Instantiate(AssetBundleManager.Prefabs["Fire_Ball"], target.transform);
            CustomCoroutine.WaitThenExecute(3f, delegate ()
            {
                if (fireball != null)
                    Destroy(fireball);
            });

            yield return new WaitForSeconds(speed * 2f);
            yield return target.SetSlotModification(FireBomb.GetFireLevel(level, target, attacker));
            yield return new WaitForSeconds(speed / 2f);
            yield break;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            Card.Anim.LightNegationEffect();
            List<CardSlot> adjSlots = BoardManager.Instance.GetAdjacentSlots(Card.Slot);
            if (adjSlots.Count > 0 && adjSlots[0].Index < Card.Slot.Index)
            {
                yield return BombCard(adjSlots[0], Card);
                adjSlots.RemoveAt(0);
            }
            yield return BombCard(Card.Slot.opposingSlot, Card);
            if (adjSlots.Count > 0 && adjSlots[0].Index > Card.Slot.Index)
            {
                yield return BombCard(adjSlots[0], Card);
            }
        }
    }
}