using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class BurntOut : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static BurntOut()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Burnt Out";
            info.rulebookDescription = "When [creature] dies, it sets the land on fire for three turns.";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, FireBomb.FlamingAbility };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_burnt_out.png", typeof(Molotov).Assembly));

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(BurntOut),
                TextureHelper.GetImageAsTexture("ability_burnt_out.png", typeof(Molotov).Assembly)
            ).Id;
        }

        private CardSlot oldSlot = null;

        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
        {
            oldSlot = Card.Slot;
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            if (oldSlot != null)
            {
                AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, oldSlot.transform.position, .7f);
                // The fireball should play and then delete itself, but we'll destroy it after some time anyway
                GameObject fireball = Instantiate(AssetBundleManager.Prefabs["Fire_Ball"], oldSlot.transform);
                CustomCoroutine.WaitThenExecute(3f, delegate ()
                {
                    if (fireball != null)
                    {
                        Destroy(fireball);
                    }
                });
                yield return new WaitForSeconds(1f);
                yield return oldSlot.SetSlotModification(FireBomb.GetFireLevel(2, oldSlot));
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
        }
    }
}
