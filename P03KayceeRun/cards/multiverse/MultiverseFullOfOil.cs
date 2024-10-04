using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    public class MultiverseFullOfOil : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private GameObject oilPrefab;

        private void Awake()
        {
            this.oilPrefab = ResourceBank.Get<GameObject>("prefabs/map/holomapscenery/HoloSlime_Pile_2");
        }

        static MultiverseFullOfOil()
        {
            var original = AbilityManager.AllAbilityInfos.AbilityByID(Ability.ExplodeOnDeath);
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Multiverse Full of Oil";
            info.rulebookDescription = "When [creature] dies, it adds 3 health to the creature on either side and across from it in every universe.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.flipYIfOpponent = true;
            info.passive = false;
            info.hasColorOverride = true;
            info.colorOverride = Color.black;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, CustomCards.MultiverseAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MultiverseFullOfOil),
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(MultiverseFullOfOil).Assembly)
            ).Id;
        }

        private static IEnumerator OilCard(PlayableCard target, CardSlot source, GameObject oilPrefab)
        {
            GameObject oil = Object.Instantiate<GameObject>(oilPrefab);
            OnboardDynamicHoloPortrait.HolofyGameObject(oil, GameColors.instance.darkBlue);
            oil.transform.position = source.transform.position + Vector3.up * 0.1f;

            Vector3 midpoint = Vector3.Lerp(source.transform.position, target.transform.position, 0.5f) + (Vector3.up * 0.25f);

            Tween.Position(oil.transform, midpoint, 0.35f / 2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
            Tween.Position(oil.transform, target.transform.position, 0.35f / 2f, 0.35f / 2f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
            Tween.Position(oil.transform, target.transform.position - (Vector3.up * 0.2f), 0.1f, 0.35f, Tween.EaseIn, Tween.LoopType.None, null, () => Destroy(oil), true);

            AudioController.Instance.PlaySound3D("eyeball_squish", MixerGroup.TableObjectsSFX, target.transform.position, .7f, randomization: new AudioParams.Randomization(), pitch: new AudioParams.Pitch(AudioParams.Pitch.Variation.Medium));
            target.Anim.StrongNegationEffect();

            yield return new WaitForSeconds(0.1f);
            target.TemporaryMods.Add(new(0, 3));
            yield break;
        }

        private static IEnumerator ExplodeFromSlot(CardSlot slot, GameObject oilPrefab)
        {
            List<CardSlot> adjacentSlots = BoardManager.Instance.GetAdjacentSlots(slot);
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Index < slot.Index)
            {
                if (adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
                {
                    yield return OilCard(adjacentSlots[0].Card, slot, oilPrefab);
                }
                adjacentSlots.RemoveAt(0);
            }
            if (slot.opposingSlot.Card != null && !slot.opposingSlot.Card.Dead)
            {
                yield return OilCard(slot.opposingSlot.Card, slot, oilPrefab);
            }
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
            {
                yield return OilCard(adjacentSlots[0].Card, slot, oilPrefab);
            }
            yield break;
        }

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            Card.Anim.LightNegationEffect();
            yield return PreSuccessfulTriggerSequence();
            yield return ExplodeFromSlot(Card.Slot, this.oilPrefab);
            // Set up the multiverse explosion:
            if (MultiverseBattleSequencer.Instance != null)
            {
                int slotIdx = Card.Slot.Index % 10;
                for (int i = 0; i < MultiverseBattleSequencer.Instance.MultiverseGames.Length; i++)
                {
                    if (i == MultiverseBattleSequencer.Instance.GetUniverseId(Card.Slot))
                        continue;

                    var universe = MultiverseBattleSequencer.Instance.MultiverseGames[i];
                    CardSlot newSlot = Card.OpponentCard ? universe.OpponentSlots[slotIdx] : universe.PlayerSlots[slotIdx];

                    universe.RegisterCallback(new OtherUniverseExplode(newSlot, oilPrefab));
                }
            }
            yield break;
        }

        protected class OtherUniverseExplode : IMultiverseDelayedCoroutine
        {
            public CardSlot targetSlot;
            public GameObject bombPrefab;

            public OtherUniverseExplode(CardSlot target, GameObject bomb)
            {
                targetSlot = target;
                bombPrefab = bomb;
            }

            public IEnumerator DoCallback()
            {
                MultiverseBattleSequencer.Instance.TeleportationEffects(targetSlot);
                yield return ExplodeFromSlot(targetSlot, bombPrefab);
            }
        }
    }
}
