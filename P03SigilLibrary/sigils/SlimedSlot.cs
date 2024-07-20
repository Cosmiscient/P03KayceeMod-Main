using DiskCardGame;
using InscryptionAPI.Helpers;
using InscryptionAPI.Slots;
using InscryptionAPI.Triggers;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class SlimedSlot : SlotModificationBehaviour, IPassiveAttackBuff
    {
        public static SlotModificationManager.ModificationType ID { get; private set; }

        static SlimedSlot()
        {
            ID = SlotModificationManager.New(
                P03SigilLibraryPlugin.PluginGuid,
                "SlimedSlot",
                typeof(SlimedSlot),
                TextureHelper.GetImageAsTexture("cardslot_slimed.png", typeof(SlimedSlot).Assembly),
                TextureHelper.GetImageAsTexture("pixel_slot_slimed.png", typeof(SlimedSlot).Assembly)
            );
        }

        public int GetPassiveAttackBuff(PlayableCard target) => target.Slot == this.Slot ? -1 : 0;
    }
}