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

        public override void ApplyAppearance()
        {
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
                        dac.weaponMeshFilter.mesh = obj.GetComponentInChildren<MeshFilter>().mesh;
                        dac.weaponMeshFilter.transform.localPosition = OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_POSITION, 0, true);
                        dac.weaponMeshFilter.transform.localEulerAngles = OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_ROTATION, 0, true);
                        dac.weaponMeshFilter.transform.localScale = OnboardDynamicHoloPortrait.GetVector3(Card, WEAPON_SCALE, 0, false);
                        OnboardDynamicHoloPortrait.HolofyGameObject(dac.weaponRenderer.gameObject, GameColors.Instance.brightBlue);
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