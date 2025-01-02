using System;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class DiskWeaponAppearance : CardAppearanceBehaviour
    {
        private static readonly Sprite TRANSPARENT = Sprite.Create(TextureHelper.GetImageAsTexture("transparent.png", typeof(DiskWeaponAppearance).Assembly), new(0f, 0f, 5f, 5f), new(0.5f, 0.5f));

        public static Appearance ID { get; private set; }

        internal class WeaponSettings
        {
            internal string WeaponCode;
            internal Vector3 WeaponPosition;
            internal Vector3 WeaponRotation;
            internal Vector3 WeaponScale;
        }

        public const string WEAPON_KEY = "DiskCardWeapon";
        public const string WEAPON_SCALE = "DiskCardWeapon.Scale";
        public const string WEAPON_POSITION = "DiskCardWeapon.Position";
        public const string WEAPON_ROTATION = "DiskCardWeapon.Rotation";
        public const string DISABLE_MUZZLE_FLASH = "DiskCardWeapon.DisableMuzzleFlash";
        public const string AUDIO_ID = "DiskCardWeapon.AudioOverrideId";

        public static readonly Color WeaponColor = new Color(0f, 0.79f, 1f, 1f);

        private static Mesh GetCombinedMesh(GameObject obj)
        {
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        private WeaponSettings _currentWeaponSettings = null;

        internal static WeaponSettings GetWeaponAppearance(Card card)
        {
            var comp = card.GetComponent<DiskWeaponAppearance>();
            if (comp != null && comp._currentWeaponSettings != null)
                return comp._currentWeaponSettings;

            return new()
            {
                WeaponCode = DiskCardWeapon.Default.ToString(),
                WeaponPosition = Vector3.zero,
                WeaponRotation = Vector3.zero,
                WeaponScale = Vector3.one
            };
        }

        internal static void ApplyWeaponAppearance(Card card, WeaponSettings settings)
        {
            ApplyWeaponAppearance(card, settings.WeaponCode, settings.WeaponPosition, settings.WeaponRotation, settings.WeaponScale);
        }

        public static void ApplyWeaponAppearance(Card card, DiskCardWeapon weapon)
        {
            ApplyWeaponAppearance(card, weapon.ToString());
        }

        public static void ApplyWeaponAppearance(Card card, string weaponKey, Vector3? position = null, Vector3? rotation = null, Vector3? scale = null)
        {
            var comp = card.GetComponent<DiskWeaponAppearance>();
            if (comp != null)
            {
                comp._currentWeaponSettings = new()
                {
                    WeaponCode = weaponKey,
                    WeaponPosition = position ?? Vector3.zero,
                    WeaponRotation = rotation ?? Vector3.zero,
                    WeaponScale = scale ?? Vector3.one
                };
            }
            if (card.Anim is DiskCardAnimationController dac)
            {
                if (Enum.TryParse<DiskCardWeapon>(weaponKey, out DiskCardWeapon weapon))
                {
                    dac.SetWeaponMesh(weapon);
                }
                else
                {
                    GameObject obj = ResourceBank.Get<GameObject>(weaponKey);
                    if (obj != null)
                    {
                        dac.weaponMeshFilter.mesh = GetCombinedMesh(obj);
                        dac.weaponMeshFilter.transform.localPosition = position ?? Vector3.zero;
                        dac.weaponMeshFilter.transform.localEulerAngles = rotation ?? Vector3.zero;
                        dac.weaponMeshFilter.transform.localScale = scale ?? Vector3.one;

                        OnboardDynamicHoloPortrait.HolofyGameObject(dac.weaponRenderer.gameObject, WeaponColor, reference: dac.weaponMaterials[(int)DiskCardWeapon.Fish]);
                    }
                }
            }
        }

        public override void ApplyAppearance()
        {
            if (Card is not PlayableCard)
                return;

            string weaponCode = Card.Info.GetExtendedProperty(WEAPON_KEY);
            if (string.IsNullOrEmpty(weaponCode))
                return;

            ApplyWeaponAppearance(
                Card,
                weaponCode,
                OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_POSITION, 0, true),
                OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_ROTATION, 0, true),
                OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_SCALE, 0, false)
            );
        }

        static DiskWeaponAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "DiskWeaponAppearance", typeof(DiskWeaponAppearance)).Id;
        }

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.PlayAttackAnimation))]
        [HarmonyPrefix]
        private static void PrepareAttack(DiskCardAnimationController __instance)
        {
            if (__instance.Card.Info.GetExtendedPropertyAsBool(DISABLE_MUZZLE_FLASH) ?? false)
            {
                var obj = __instance.transform.Find("WeaponAnimation/Anim").gameObject;
                foreach (var rend in obj.GetComponentsInChildren<SpriteRenderer>().ToList())
                {
                    rend.sprite = TRANSPARENT;
                }
            }

            if (!string.IsNullOrEmpty(__instance.Card.Info.GetExtendedProperty(AUDIO_ID)))
            {
                var anim = __instance.transform.Find("WeaponAnimation/Anim");
                var kfe = anim.gameObject.GetComponent<AudioKeyframeEvents>();
                kfe.events[0].audioId = __instance.Card.Info.GetExtendedProperty(AUDIO_ID);
            }
        }
    }
}