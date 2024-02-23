using System;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class DiskWeaponAppearance : CardAppearanceBehaviour
    {
        public static Appearance ID { get; private set; }

        public const string WEAPON_KEY = "DiskCardWeapon";
        public const string WEAPON_SCALE = "DiskCardWeapon.Scale";
        public const string WEAPON_POSITION = "DiskCardWeapon.Position";
        public const string WEAPON_ROTATION = "DiskCardWeapon.Rotation";

        public static readonly Color WeaponColor = new Color(0f, 0.79f, 1f, 1f);

        private Material ReferenceMaterial => (Card.Anim as DiskCardAnimationController).weaponMaterials[(int)DiskCardWeapon.Fish];

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

        public override void ApplyAppearance()
        {
            if (Card is not PlayableCard)
                return;

            string weaponCode = Card.Info.GetExtendedProperty(WEAPON_KEY);
            if (string.IsNullOrEmpty(weaponCode))
                return;

            if (Card.Anim is DiskCardAnimationController dac)
            {
                if (Enum.TryParse<DiskCardWeapon>(weaponCode, out DiskCardWeapon weapon))
                {
                    dac.SetWeaponMesh(weapon);
                }
                else
                {
                    GameObject obj = ResourceBank.Get<GameObject>(weaponCode);
                    if (obj != null)
                    {
                        dac.weaponMeshFilter.mesh = GetCombinedMesh(obj);
                        dac.weaponMeshFilter.transform.localPosition = OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_POSITION, 0, true);
                        dac.weaponMeshFilter.transform.localEulerAngles = OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_ROTATION, 0, true);
                        dac.weaponMeshFilter.transform.localScale = OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_SCALE, 0, false);
                        OnboardDynamicHoloPortrait.HolofyGameObject(dac.weaponRenderer.gameObject, WeaponColor, reference: ReferenceMaterial);
                    }
                }
            }
        }

        static DiskWeaponAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "DiskWeaponAppearance", typeof(DiskWeaponAppearance)).Id;
        }
    }
}