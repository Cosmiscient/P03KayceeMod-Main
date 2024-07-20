using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using GBC;
using Infiniscryption.P03SigilLibrary.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Slots;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public abstract class BurningSlotBase : SlotModificationBehaviour
    {
        public static readonly AbilityMetaCategory FlamingAbility = GuidManager.GetEnumValue<AbilityMetaCategory>(P03SigilLibraryPlugin.PluginGuid, "FlamingAbility");

        private static List<SlotModificationManager.ModificationType> OnFire;

        public static event Action<int, PlayableCard> FireDamageTrigger;

        public static bool SlotIsOnFire(CardSlot slot) => OnFire.Contains(slot.GetSlotModification());

        public static SlotModificationManager.ModificationType GetFireLevel(int fireLevel, CardSlot target, PlayableCard source = null)
        {
            if (source == null)
                return OnFire[fireLevel];

            if (target.IsOpponentSlot() != source.IsPlayerCard())
                return OnFire[fireLevel];

            if (BoardManager.Instance.GetSlotsCopy(source.IsPlayerCard())
                                     .Any(s => s.Card != null
                                            && s.Card.HasAbility(FlameStoker.AbilityID)))
            {

                if (fireLevel < OnFire.Count - 1)
                    return OnFire[fireLevel + 1];
            }

            return OnFire[fireLevel];
        }

        static BurningSlotBase()
        {
            OnFire = new()
            {
                SlotModificationManager.New(
                    P03SigilLibraryPlugin.PluginGuid,
                    "OnFire1",
                    typeof(BurningSlotOne),
                    TextureHelper.GetImageAsTexture("card_slot_fire_1.png", typeof(FireBomb).Assembly),
                    TextureHelper.GetImageAsTexture("pixel_slot_fire_1.png", typeof(FireBomb).Assembly)
                ),
                SlotModificationManager.New(
                    P03SigilLibraryPlugin.PluginGuid,
                    "OnFire2",
                    typeof(BurningSlotTwo),
                    TextureHelper.GetImageAsTexture("card_slot_fire_2.png", typeof(FireBomb).Assembly),
                    TextureHelper.GetImageAsTexture("pixel_slot_fire_2.png", typeof(FireBomb).Assembly)
                ),
                SlotModificationManager.New(
                    P03SigilLibraryPlugin.PluginGuid,
                    "OnFire3",
                    typeof(BurningSlotThree),
                    TextureHelper.GetImageAsTexture("card_slot_fire_3.png", typeof(FireBomb).Assembly),
                    TextureHelper.GetImageAsTexture("pixel_slot_fire_3.png", typeof(FireBomb).Assembly)
                ),
                SlotModificationManager.New(
                    P03SigilLibraryPlugin.PluginGuid,
                    "OnFire4",
                    typeof(BurningSlotFour),
                    TextureHelper.GetImageAsTexture("card_slot_fire_4.png", typeof(FireBomb).Assembly),
                    TextureHelper.GetImageAsTexture("pixel_slot_fire_4.png", typeof(FireBomb).Assembly, FilterMode.Point)
                )
            };
        }

        public abstract int BurnTurns { get; }

        public static bool CardIsFireproof(PlayableCard card) => card.HasAbility(Ability.MadeOfStone);

        public static IEnumerator SetSlotOnFireBasic(int fireLevel, CardSlot targetSlot, CardSlot attackingSlot)
        {
            GameObject fireball = Instantiate(AssetBundleManager.Prefabs["Fire_Ball"], targetSlot.transform);

            if (BoardManager.Instance is BoardManager3D)
                AudioController.Instance.PlaySound3D("fireball", MixerGroup.TableObjectsSFX, fireball.transform.position, 0.5f);

            CustomCoroutine.WaitThenExecute(3f, delegate ()
            {
                if (fireball != null)
                {
                    Destroy(fireball);
                }
            });
            yield return new WaitForSeconds(0.3f);
            yield return targetSlot.SetSlotModification(GetFireLevel(fireLevel, targetSlot, attackingSlot?.Card));
            yield return new WaitForSeconds(0.05f);
        }

        protected void SynchronizeFlames()
        {
            if (Slot is PixelCardSlot)
                return;

            Transform flames = Slot.transform.Find("Flames");
            if (BurnTurns <= 0 && flames != null)
            {
                CustomCoroutine.WaitOnConditionThenExecute(() => GlobalTriggerHandler.Instance.StackSize == 0, () => Destroy(flames.gameObject));
            }
            else
            {
                if (flames == null)
                {
                    GameObject newFlames = Instantiate(AssetBundleManager.Prefabs["Fire_Parent"], Slot.transform);
                    newFlames.name = "Flames";
                    newFlames.transform.localPosition = new(0f, 0f, -0.95f);
                    flames = newFlames.transform;
                }

                for (int i = 1; i <= 4; i++)
                    flames.transform.Find($"Fire_System_{i}").gameObject.SetActive(i == BurnTurns);
            }
        }

        public override IEnumerator Setup()
        {
            SynchronizeFlames();

            yield return new WaitForEndOfFrame();
        }

        public override IEnumerator Cleanup(SlotModificationManager.ModificationType replacement)
        {
            if (OnFire.Contains(replacement))
                yield break;

            Transform flames = Slot.transform.Find("Flames");
            if (flames != null)
                CustomCoroutine.WaitOnConditionThenExecute(() => GlobalTriggerHandler.Instance.StackSize == 0, () => Destroy(flames.gameObject));
            yield break;
        }

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => BurnTurns > 0 && playerTurnEnd == Slot.IsPlayerSlot;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            if (Slot.Card != null && Slot.Card.Info != null)
            {
                if (!CardIsFireproof(Slot.Card))
                {
                    if (Slot.Card.Info.name.Equals("Tree_Hologram") || Slot.Card.Info.name.Equals("Tree_Hologram_SnowCovered"))
                    {
                        Slot.Card.SetInfo(CardLoader.GetCardByName("DeadTree"));
                        yield return new WaitForSeconds(0.25f);
                        if (Slot.Card.Health <= 0)
                        {
                            yield return Slot.Card.Die(false);
                            yield return new WaitForSeconds(0.15f);
                        }
                    }
                    else
                    {

                        FireDamageTrigger?.Invoke(1, Slot.Card);

                        yield return Slot.Card.TakeDamage(1, null);

                        if (Slot.Card != null && !Slot.Card.Dead)
                        {
                            if (Slot.Card.HasAbility(Ability.SwapStats))
                            {
                                SwapStats component = Slot.Card.GetComponent<SwapStats>();
                                if (component != null)
                                    yield return component.OnTakeDamage(Slot.Card);
                            }
                        }
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            }

            // Reduce the number of turns remaining
            if (BurnTurns == 1)
            {
                yield return Slot.SetSlotModification(SlotModificationManager.ModificationType.NoModification);
            }
            else
            {
                yield return Slot.SetSlotModification(OnFire[BurnTurns - 2]);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    public class BurningSlotFour : BurningSlotBase { public override int BurnTurns => 4; }
    public class BurningSlotThree : BurningSlotBase { public override int BurnTurns => 3; }
    public class BurningSlotTwo : BurningSlotBase { public override int BurnTurns => 2; }
    public class BurningSlotOne : BurningSlotBase { public override int BurnTurns => 1; }
}