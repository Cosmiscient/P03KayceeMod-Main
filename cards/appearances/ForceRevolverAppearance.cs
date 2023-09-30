using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ForceRevolverAppearance : CardAppearanceBehaviour
    {
        public static Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
            if (Card.Anim is DiskCardAnimationController dac)
                dac.SetWeaponMesh(DiskCardWeapon.Revolver);
        }

        static ForceRevolverAppearance()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ForceRevolverAppearance", typeof(ForceRevolverAppearance)).Id;
        }
    }
}