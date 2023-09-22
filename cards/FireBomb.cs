using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Quests;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class FireBomb : AbilityBehaviour, IOnPostSingularSlotAttackSlot
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public static Ability FlameStokerID { get; private set; }

        public static readonly AbilityMetaCategory FlamingAbility = GuidManager.GetEnumValue<AbilityMetaCategory>(P03Plugin.PluginGuid, "FlamingAbility");

        private static List<SlotModificationManager.ModificationType> OnFire { get; set; }

        public static SlotModificationManager.ModificationType GetFireLevel(int fireLevel, CardSlot target, PlayableCard source = null)
        {
            if (source == null)
                return OnFire[fireLevel];

            if (target.IsOpponentSlot() != source.IsPlayerCard())
                return OnFire[fireLevel];

            if (BoardManager.Instance.GetSlotsCopy(source.IsPlayerCard())
                                     .Any(s => s.Card != null
                                            && s.Card.HasAbility(FlameStokerID)))
            {

                if (fireLevel < OnFire.Count - 1)
                    return OnFire[fireLevel + 1];
            }

            return OnFire[fireLevel];
        }

        public class BurningSlot : NonCardTriggerReceiver, ISlotModificationChanged
        {
            public static bool CardIsFireproof(PlayableCard card) => card.HasAbility(Ability.MadeOfStone);

            public IEnumerator OnSlotModificationChanged(CardSlot slot, SlotModificationManager.ModificationType previous, SlotModificationManager.ModificationType current)
            {
                float scale = 0f;
                if (current == OnFire[3])
                    scale = 1f;
                else if (current == OnFire[2])
                    scale = 0.8f;
                else if (current == OnFire[1])
                    scale = 0.6f / 0.8f;
                else if (current == OnFire[0])
                    scale = 0.4f / 0.6f;

                Transform flames = slot.transform.Find("Flames");
                if (flames != null && scale == 0f)
                    Destroy(flames.gameObject);

                if (scale == 0f)
                    yield break;

                GameObject newFlames = flames == null ? Instantiate(AssetBundleManager.Prefabs["Fire_Parent"], slot.transform) : flames.gameObject;
                newFlames.name = "Flames";
                newFlames.transform.localPosition = new(0f, 0f, -0.95f);
                foreach (ParticleSystem particles in newFlames.GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.ShapeModule shape = particles.shape;
                    shape.radius *= scale;
                }
                yield return new WaitForEndOfFrame();
            }

            public bool RespondsToSlotModificationChanged(CardSlot slot, SlotModificationManager.ModificationType previous, SlotModificationManager.ModificationType current) => true;

            public override bool RespondsToTurnEnd(bool playerTurnEnd) => true;

            public override IEnumerator OnTurnEnd(bool playerTurnEnd)
            {
                List<CardSlot> slots = BoardManager.Instance.GetSlots(playerTurnEnd);
                foreach (CardSlot slot in slots)
                {
                    if (OnFire.Contains(slot.GetSlotModification()))
                    {
                        if (slot.Card != null)
                        {
                            if (!CardIsFireproof(slot.Card))
                            {
                                if (slot.Card.Info.name.Equals("Tree_Hologram") || slot.Card.Info.name.Equals("Tree_Hologram_SnowCovered"))
                                {
                                    slot.Card.SetInfo(CardLoader.GetCardByName("DeadTree"));
                                }
                                else
                                {
                                    if (slot.Card.Health == 1)
                                        DefaultQuestDefinitions.Pyromania.IncrementQuestCounter(onlyIfActive: true);

                                    yield return slot.Card.TakeDamage(1, null);
                                }
                                yield return new WaitForSeconds(0.25f);
                            }
                        }

                        int idx = OnFire.IndexOf(slot.GetSlotModification());
                        yield return idx == 0
                            ? slot.SetSlotModification(SlotModificationManager.ModificationType.NoModification)
                            : (object)slot.SetSlotModification(OnFire[idx - 1]);

                        yield return new WaitForSeconds(0.25f);
                    }
                }
            }
        }

        static FireBomb()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Fire Strike";
            info.rulebookDescription = "When [creature] attacks, it sets the target space on fire for two turns.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular, FlamingAbility };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(FireBomb),
                TextureHelper.GetImageAsTexture("ability_fire_bomb.png", typeof(FireBomb).Assembly)
            ).Id;

            OnFire = new();
            for (int i = 1; i <= 4; i++)
            {
                OnFire.Add(SlotModificationManager.New(P03Plugin.PluginGuid, $"OnFire{i}", typeof(BurningSlot)));
            }

            AbilityInfo fsInfo = ScriptableObject.CreateInstance<AbilityInfo>();
            fsInfo.rulebookName = "Flame Stoker";
            fsInfo.rulebookDescription = "While [creature] is on board, all fires you start will be stronger, causing them to last one turn longer.";
            fsInfo.canStack = false;
            fsInfo.powerLevel = 1;
            fsInfo.opponentUsable = true;
            fsInfo.passive = true;
            fsInfo.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, FlamingAbility };

            FlameStokerID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                fsInfo,
                typeof(FireBomb),
                TextureHelper.GetImageAsTexture("ability_flame_stoker.png", typeof(FireBomb).Assembly)
            ).Id;
        }

        public bool RespondsToPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot) => attackingSlot == Card.Slot;

        public IEnumerator OnPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot)
        {
            AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, targetSlot.transform.position, .7f);
            // The fireball should play and then delete itself, but we'll destroy it after some time anyway
            GameObject fireball = Instantiate(AssetBundleManager.Prefabs["Fire_Ball"], targetSlot.transform);
            CustomCoroutine.WaitThenExecute(3f, delegate ()
            {
                if (fireball != null)
                {
                    Destroy(fireball);
                }
            });
            yield return new WaitForSeconds(1f);
            yield return targetSlot.SetSlotModification(GetFireLevel(2, targetSlot, attackingSlot.Card));
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        // ALL THIS STUFF BELOW HERE IS CODE I DON'T WANT TO LOSE
        // THIS WAS THE WAY IT USED TO WORK WHEN IT REPLACED THE ATTACK ENTIRELY
        // AND ITS CODE I PROBABLY WILL WANT TO USE AGAIN SOMEDAY

        // public bool RespondsToPreSlotAttackSequence(CardSlot attackingSlot) => attackingSlot == this.Card.Slot;

        // private IEnumerator BombCard(CardSlot target, PlayableCard attacker)
        // {
        //     yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.SlotTargetedForAttack, false, new object[] { target, attacker });
        //     yield return new WaitForSeconds(0.025f);

        //     GameObject bomb = GameObject.Instantiate<GameObject>(AssetBundleManager.Prefabs["Molotov"]);
        //     OnboardDynamicHoloPortrait.HolofyGameObject(bomb, GameColors.instance.glowRed);
        //     bomb.transform.position = attacker.transform.position + Vector3.up * 0.1f;

        //     var midpoint = Vector3.Lerp(attacker.Slot.transform.position, target.transform.position, 0.5f) + (Vector3.up * 0.25f);

        //     Tween.Position(bomb.transform, midpoint, 0.25f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
        //     Tween.Position(bomb.transform, target.transform.position, 0.25f, 0.25f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
        //     Tween.Position(bomb.transform, target.transform.position - (Vector3.up * 0.2f), 0.1f, 0.5f, Tween.EaseIn, Tween.LoopType.None, null, () => GameObject.Destroy(bomb), true);
        //     Tween.LocalRotation(bomb.transform, Quaternion.Euler(new(90f, 0f, 0f)), 0.5f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);

        //     yield return new WaitForSeconds(0.5f);
        //     AudioController.Instance.PlaySound3D("molotov", MixerGroup.TableObjectsSFX, target.transform.position, .7f);
        //     target.Card?.Anim.PlayHitAnimation();

        //     // The fireball should play and then delete itself, but we'll destroy it after some time anyway
        //     var fireball = GameObject.Instantiate<GameObject>(AssetBundleManager.Prefabs["Fire_Ball"], target.transform);
        //     CustomCoroutine.WaitThenExecute(3f, delegate ()
        //     {
        //         if (fireball != null)
        //             GameObject.Destroy(fireball);
        //     });

        //     yield return new WaitForSeconds(1f);
        //     yield return target.SetSlotModification(FireBomb.OnFire[Math.Min(OnFire.Count - 1, attacker.Attack + 1)]);
        //     yield return new WaitForSeconds(0.25f);
        //     yield break;
        // }

        // public IEnumerator OnPreSlotAttackSequence(CardSlot attackingSlot)
        // {
        //     // We're doing a fake attack sequence here
        //     List<CardSlot> opposingSlots = this.Card.GetOpposingSlots();
        //     ViewManager.Instance.SwitchToView(BoardManager.Instance.CombatView, false, false);
        //     ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
        //     if (this.Card.HasAbility(Ability.Sniper))
        //     {
        //         opposingSlots.Clear();
        //         int numAttacks = SniperFix.GetAttackCount(this.Card);

        //         if (this.Card.OpponentCard)
        //         {
        //             var playerSlots = BoardManager.Instance.PlayerSlotsCopy;
        //             if (numAttacks >= playerSlots.Count)
        //             {
        //                 opposingSlots.AddRange(playerSlots);
        //             }
        //             else
        //             {
        //                 List<CardSlot> filledSlots = playerSlots.Where(s => s.Card != null).ToList();
        //                 filledSlots.Sort((a, b) => b.Card.PowerLevel - a.Card.PowerLevel);
        //                 if (filledSlots.Count >= numAttacks)
        //                 {
        //                     opposingSlots.AddRange(filledSlots.Take(numAttacks));
        //                 }
        //                 else
        //                 {
        //                     opposingSlots.AddRange(filledSlots);
        //                     opposingSlots.AddRange(playerSlots.Where(s => s.Card == null).Take(numAttacks - opposingSlots.Count));
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             ViewManager.Instance.Controller.SwitchToControlMode(BoardManager.Instance.ChoosingSlotViewMode, false);
        //             ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        //             for (int i = 0; i < numAttacks; i++)
        //             {
        //                 TurnManager.Instance.CombatPhaseManager.VisualizeStartSniperAbility(this.Card.Slot);
        //                 CardSlot cardSlot = InteractionCursor.Instance.CurrentInteractable as CardSlot;
        //                 if (cardSlot != null && opposingSlots.Contains(cardSlot))
        //                 {
        //                     TurnManager.Instance.CombatPhaseManager.VisualizeAimSniperAbility(this.Card.Slot, cardSlot);
        //                 }
        //                 var opponentSlots = BoardManager.Instance.OpponentSlotsCopy;
        //                 yield return BoardManager.Instance.ChooseTarget(
        //                     opponentSlots,
        //                     opponentSlots,
        //                     delegate (CardSlot s)
        //                     {
        //                         opposingSlots.Add(s);
        //                         TurnManager.Instance.CombatPhaseManager.VisualizeConfirmSniperAbility(s);
        //                     },
        //                     null,
        //                     (s) => TurnManager.Instance.CombatPhaseManager.VisualizeAimSniperAbility(this.Card.Slot, s),
        //                     () => false,
        //                     CursorType.Target
        //                 );
        //             }
        //             ViewManager.Instance.Controller.SwitchToControlMode(BoardManager.Instance.DefaultViewMode, false);
        //             ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
        //             TurnManager.Instance.CombatPhaseManager.VisualizeClearSniperAbility();
        //         }
        //     }
        //     foreach (CardSlot opposingSlot in opposingSlots)
        //     {
        //         ViewManager.Instance.SwitchToView(BoardManager.Instance.CombatView, false, false);
        //         yield return BombCard(opposingSlot, this.Card);
        //     }
        //     yield break;
        // }

        // [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        // [HarmonyPostfix]
        // private static IEnumerator BlockAttacksFromFireBomb(IEnumerator sequence, CardSlot attackingSlot)
        // {
        //     if (attackingSlot.Card != null && attackingSlot.Card.HasAbility(FireBomb.AbilityID))
        //         yield break;

        //     yield return sequence;
        // }

        // [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AttackIsBlocked))]
        // [HarmonyPrefix]
        // private static bool NeverBlocked(PlayableCard __instance, ref bool __result)
        // {
        //     if (__instance.HasAbility(FireBomb.AbilityID))
        //     {
        //         __result = false;
        //         return false;
        //     }
        //     return true;
        // }

        // [HarmonyPatch(typeof(CombatPhaseManager3D), nameof(CombatPhaseManager3D.ShowCardBlocked))]
        // [HarmonyPostfix]
        // private static IEnumerator DontShowBlockedFireBombB(IEnumerator sequence, PlayableCard card)
        // {
        //     if (card.HasAbility(FireBomb.AbilityID))
        //         yield break;

        //     yield return sequence;
        // }
    }
}
