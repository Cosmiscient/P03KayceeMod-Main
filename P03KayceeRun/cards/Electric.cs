using System.Collections;
using System.Collections.Generic;
using DigitalRuby.LightningBolt;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class Electric : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static Electric()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Electric";
            info.rulebookDescription = "When [creature] declares an attack, it will deal half the damage to cards adjacent to the target.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(Electric),
                TextureHelper.GetImageAsTexture("void_Electric.png", typeof(Electric).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => Card == attacker;

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            CardSlot baseSlot = Card.slot;
            List<CardSlot> adjacentSlots = BoardManager.Instance.GetAdjacentSlots(baseSlot.opposingSlot);
            yield return new WaitForSeconds(0.2f);
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Index < baseSlot.Index)
            {
                if (adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
                {
                    yield return ShockCard(adjacentSlots[0].Card, baseSlot.Card, Mathf.CeilToInt(Card.Attack * 0.5f));
                }
                adjacentSlots.RemoveAt(0);
            }
            yield return new WaitForSeconds(0.2f);
            if (baseSlot.Card == null || baseSlot.Card != Card || Card == null || Card.Dead)
            {
                yield break;
            }
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
            {
                yield return ShockCard(adjacentSlots[0].Card, baseSlot.Card, Mathf.CeilToInt(Card.Attack * 0.5f));
            }
            yield break;
        }

        internal static IEnumerator ShockCard(PlayableCard target, PlayableCard attacker, int damage, Vector3? startPosition = null)
        {
            CardSlot centerSlot = target.slot;
            if (!SaveManager.SaveFile.IsPart2)
            {
                Singleton<TableVisualEffectsManager>.Instance.ThumpTable(0.3f);
                AudioController.Instance.PlaySound3D("teslacoil_overload", MixerGroup.TableObjectsSFX, centerSlot.transform.position, 1f, 0f);
                GameObject gameObject = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"));
                gameObject.GetComponent<LightningBoltScript>().StartPosition = startPosition.GetValueOrDefault(attacker.gameObject.transform.position);
                gameObject.GetComponent<LightningBoltScript>().EndObject = centerSlot.Card.gameObject;
                yield return new WaitForSeconds(0.2f);
                Destroy(gameObject, 0.25f);
                centerSlot.Card.Anim.StrongNegationEffect();
                target.Anim.PlayHitAnimation();
            }
            yield return target.TakeDamage(damage, attacker);
            yield return new WaitForSeconds(0.2f);
            yield break;
        }
    }
}