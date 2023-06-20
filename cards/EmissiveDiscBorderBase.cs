using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public abstract class EmissiveDiscBorderBase : CardAppearanceBehaviour
    {
        protected virtual Color EmissionColor => GameColors.Instance.blue;
        protected virtual float Intensity => 0.5f;

        internal static readonly string[] GameObjectPaths = new string[]
        {
            "Anim/CardBase/Rails",
            "Anim/CardBase/Top",
            "Anim/CardBase/Bottom"
        };

        public override void ApplyAppearance()
        {
			foreach (string key in GameObjectPaths)
            {
                Transform tComp = this.gameObject.transform.Find(key);
                if (tComp != null && tComp.gameObject != null)
                {
                    GameObject component = tComp.gameObject;
                    MeshRenderer renderer = component.GetComponent<MeshRenderer>();
                    Material material = renderer.material;
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", EmissionColor * Intensity);
                }
            }
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            this.ApplyAppearance();
        }
    }
}