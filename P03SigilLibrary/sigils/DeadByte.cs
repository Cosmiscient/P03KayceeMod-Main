using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class DeadByte : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }


        static DeadByte()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Dead Byte";
            info.rulebookDescription = "When a card bearing this sigil perishes, take 1 damage.";
            info.canStack = false;
            info.powerLevel = -1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_deadbyte.png", typeof(DeadByte).Assembly));

            DeadByte.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(DeadByte),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_filesizedamage")
            ).Id;
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.1f);
            //int index = EventManagement.CompletedZones.Count;
            int damage = Card.OpponentCard ? -1 : 1;
            string prefabSuffix = "B";
            CustomCoroutine.WaitThenExecute(0.15f, delegate
            {
                AudioController.Instance.PlaySound3D("archivist_spawn_filecube", MixerGroup.TableObjectsSFX, LifeManager.Instance.Scales.transform.position, 1f, 0f, null, null, null, null, false);
            }, false);
            yield return LifeManager.Instance.ShowDamageSequence(damage, 1, true, 0.25f, ResourceBank.Get<GameObject>("Prefabs/Environment/ScaleWeights/Weight_DataFile_" + prefabSuffix), 0f, true);
            yield break;
        }

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;


    }
}
