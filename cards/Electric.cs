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
                typeof(EmeraldExtraction),
                TextureHelper.GetImageAsTexture("void_Electric.png", typeof(Electric).Assembly)
            ).Id;
        }

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker) => base.Card == attacker;

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            CardSlot baseSlot = Card.slot;
            List<CardSlot> adjacentSlots = BoardManager.Instance.GetAdjacentSlots(baseSlot.opposingSlot);
            yield return new WaitForSeconds(0.2f);
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Index < baseSlot.Index)
            {
                if (adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
                {
                    yield return this.ShockCard(adjacentSlots[0].Card, baseSlot.Card, base.Card.Attack);
                }
                adjacentSlots.RemoveAt(0);
            }
            yield return new WaitForSeconds(0.2f);
            if (adjacentSlots.Count > 0 && adjacentSlots[0].Card != null && !adjacentSlots[0].Card.Dead)
            {
                yield return this.ShockCard(adjacentSlots[0].Card, baseSlot.Card, base.Card.Attack);
            }
            yield break;
        }

        private IEnumerator ShockCard(PlayableCard target, PlayableCard attacker, int damage)
        {
            CardSlot centerSlot = target.slot;
            int finalDamage = Mathf.FloorToInt((float)damage * 0.5f);
            bool flag = !SaveManager.SaveFile.IsPart2;
            if (!SaveManager.SaveFile.IsPart2)
            {
                Singleton<TableVisualEffectsManager>.Instance.ThumpTable(0.3f);
                AudioController.Instance.PlaySound3D("teslacoil_overload", MixerGroup.TableObjectsSFX, centerSlot.transform.position, 1f, 0f);
                GameObject gameObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"));
                gameObject.GetComponent<LightningBoltScript>().StartObject = attacker.gameObject;
                gameObject.GetComponent<LightningBoltScript>().EndObject = centerSlot.Card.gameObject;
                yield return new WaitForSeconds(0.2f);
                GameObject.Destroy(gameObject, 0.25f);
                centerSlot.Card.Anim.StrongNegationEffect();
                target.Anim.PlayHitAnimation();
            }
            yield return target.TakeDamage(finalDamage, attacker);
            yield return new WaitForSeconds(0.2f);
            yield break;
        }
    }
}