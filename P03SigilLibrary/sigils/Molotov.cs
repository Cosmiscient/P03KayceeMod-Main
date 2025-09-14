using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Slots;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
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
            info.rulebookDescription = "When [creature] dies, it detonates and sets adjacent spaces on fire for three turns.";
            info.canStack = false;
            info.powerLevel = 0;
            info.opponentUsable = false;
            info.flipYIfOpponent = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular, BurningSlotBase.FlamingAbility };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_molotov.png", typeof(Molotov).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Molotov),
                TextureHelper.GetImageAsTexture("ability_molotov.png", typeof(Molotov).Assembly)
            ).Id;

            info.SetSlotRedirect("on fire", BurningSlotBase.GetFireLevel(2), GameColors.Instance.limeGreen);
        }

        public static IEnumerator ThrowMolotov(CardSlot target, PlayableCard attacker, float speed = 0.35f, float delay = 0f)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            GameObject bomb = Instantiate(AssetBundleManager.Prefabs["Molotov"]);
            AssetBundleManager.HolofyGameObject(bomb, GameColors.instance.glowRed);
            bomb.transform.position = attacker.transform.position + (Vector3.up * 0.1f);

            Vector3 midpoint = Vector3.Lerp(attacker.Slot.transform.position, target.transform.position, 0.5f) + (Vector3.up * 0.25f);

            Tween.Position(bomb.transform, midpoint, speed / 2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, target.transform.position, speed / 2f, speed / 2f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Tween.Position(bomb.transform, target.transform.position - (Vector3.up * 0.2f), 0.1f, speed, Tween.EaseIn, Tween.LoopType.None, null, () => Destroy(bomb), true);
            Tween.LocalRotation(bomb.transform, Quaternion.Euler(new(90f, 0f, 0f)), speed, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);

            yield return new WaitForSeconds(speed);

            if (BoardManager.Instance is BoardManager3D)
                AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, target.transform.position, .7f);

            target.Card?.Anim.PlayHitAnimation();

            // The fireball should play and then delete itself, but we'll destroy it after some time anyway
            string prefab = "Fire_Ball";
            if (SaveManager.SaveFile.IsPart1)
                prefab += "_Red";
            else if (!SaveManager.SaveFile.IsPart3)
                prefab += "_Green";

            GameObject fireball = Instantiate(AssetBundleManager.Prefabs[prefab], target.transform);
            CustomCoroutine.WaitThenExecute(3f, delegate ()
            {
                if (!fireball.SafeIsUnityNull())
                    Destroy(fireball);
            });
        }

        public static IEnumerator BombCardsAsync(List<CardSlot> target, PlayableCard attacker, int level = 2, float speed = 0.35f)
        {
            if (BoardManager.Instance is BoardManager3D)
            {
                for (int i = 0; i < target.Count; i++)
                    attacker.StartCoroutine(ThrowMolotov(target[i], attacker, speed, 0.05f * (float)i));

                yield return new WaitForSeconds(speed * 2f);
            }

            foreach (CardSlot slot in target)
                yield return slot.SetSlotModification(BurningSlotBase.GetFireLevel(level, slot, attacker));

            yield return new WaitForSeconds(speed / 2f);
            yield break;
        }

        public static IEnumerator BombCard(CardSlot target, PlayableCard attacker, int level = 2, float speed = 0.35f)
        {
            if (BoardManager.Instance is BoardManager3D)
            {
                yield return ThrowMolotov(target, attacker, speed);
                yield return new WaitForSeconds(speed * 2f);
                yield return target.SetSlotModification(BurningSlotBase.GetFireLevel(level, target, attacker));
                yield return new WaitForSeconds(speed / 2f);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                yield return target.SetSlotModification(BurningSlotBase.GetFireLevel(level, target, attacker));
            }
            yield break;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            List<CardSlot> adjSlots = BoardManager.Instance.GetAdjacentSlots(Card.Slot);
            if (adjSlots.Count > 0 && adjSlots[0].Index < Card.Slot.Index)
            {
                Card.Anim.StrongNegationEffect();
                yield return BombCard(adjSlots[0], Card);
                adjSlots.RemoveAt(0);
            }
            Card.Anim.StrongNegationEffect();
            yield return BombCard(Card.Slot.opposingSlot, Card);
            if (adjSlots.Count > 0 && adjSlots[0].Index > Card.Slot.Index)
            {
                Card.Anim.StrongNegationEffect();
                yield return BombCard(adjSlots[0], Card);
            }
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.UpdateFaceUpOnBoardEffects))]
        [HarmonyPostfix]
        private static void SetExplosiveForMolotov(PlayableCard __instance)
        {
            if (!__instance.Dead && __instance.HasAbility(AbilityID))
                __instance.Anim.SetExplosive(true);
        }
    }
}
