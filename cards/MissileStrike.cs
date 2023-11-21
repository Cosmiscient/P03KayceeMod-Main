using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using InscryptionCommunityPatch.Card;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class MissileStrike : ActivatedAbilityBehaviour, IOnBellRung
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        private bool _firedThisTurn = false;

        public const string NUMBER_OF_MISSILES = "NumberOfMissiles";
        private int _numberOfShotsFired = 0;
        private int MissileCount => Card.Info.GetExtendedPropertyAsInt(NUMBER_OF_MISSILES).GetValueOrDefault(1);
        private int ShotsRemaining => MissileCount - _numberOfShotsFired;

        private static readonly List<Texture2D> MissileIcons = new()
        {
            TextureHelper.GetImageAsTexture("ability_missile_strike.png", typeof(MissileStrike).Assembly),
            TextureHelper.GetImageAsTexture("ability_missile_strike_2.png", typeof(MissileStrike).Assembly),
            TextureHelper.GetImageAsTexture("ability_missile_strike_3.png", typeof(MissileStrike).Assembly)
        };

        static MissileStrike()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Launch Missile";
            info.rulebookDescription = "Skip your attack this turn to launch a missile that lands on the next turn, splashing damage to adjacent spaces. Use carefully - ammo is limited.";
            info.canStack = false;
            info.powerLevel = 2;
            info.activated = true;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MissileStrike),
                TextureHelper.GetImageAsTexture("ability_missile_strike.png", typeof(MissileStrike).Assembly)
            ).Id;
        }

        [HarmonyPatch(typeof(AbilityIconInteractable), nameof(AbilityIconInteractable.LoadIcon))]
        [HarmonyPrefix]
        private static bool LoadMissileStrikeIcons(CardInfo info, AbilityInfo ability, ref Texture __result)
        {
            if (ability.ability != AbilityID)
                return true;

            int? numberOfMissiles = info.GetExtendedPropertyAsInt(NUMBER_OF_MISSILES);
            if (!numberOfMissiles.HasValue)
            {
                __result = MissileIcons[0];
                return false;
            }

            if (numberOfMissiles.Value >= 3)
            {
                __result = MissileIcons[2];
                return false;
            }

            __result = MissileIcons[numberOfMissiles.Value - 1];
            return false;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.DestroyWhenStackIsClear))]
        [HarmonyPostfix]
        private static IEnumerator DontDestroyPendingStrikes(IEnumerator sequence, PlayableCard __instance)
        {
            if (MissileStrikeManager.Instance != null && MissileStrikeManager.Instance.HasPendingStrike(__instance))
            {
                yield return new WaitForSeconds(2f);
                yield return new WaitUntil(() => GlobalTriggerHandler.Instance.StackSize == 0);
                __instance.Dead = true;
                __instance.transform.position = new(10f, -10f, 10f);
                yield break;
            }
            yield return sequence;
        }

        internal class MissileStrikeManager : NonCardTriggerReceiver
        {
            private static MissileStrikeManager _instance;
            internal static MissileStrikeManager Instance
            {
                get => _instance ?? BoardManager.Instance.gameObject.AddComponent<MissileStrikeManager>();
                private set => _instance = value;
            }

            internal const string DUMMY_SLOT_NAME = "DummyCardSlot";

            protected class StrikeInfo
            {
                public CardSlot Slot { get; set; }
                public int AttackValue { get; set; }
                public PlayableCard Attacker { get; set; }
                public GameObject Target { get; set; }
                public bool PlayerUpkeep { get; set; }
                public int QueuedTurnNumber { get; set; }
            }

            protected List<StrikeInfo> PendingAttacks = new();

            private CardSlot DummyCardSlot;

            private new void Awake()
            {
                base.Awake();
                Instance = this;
                Transform dummy = BoardManager.Instance.transform.Find("DummyCardSlot");
                if (dummy != null)
                {
                    DummyCardSlot = dummy.GetComponent<CardSlot>();
                }
                else
                {
                    CardSlot prefab = ResourceBank.Get<CardSlot>("Prefabs/Cards/CardSlot_Part3");
                    CardSlot newSlot = Instantiate(prefab, BoardManager.Instance.transform);
                    newSlot.name = "DummyCardSlot";
                    newSlot.transform.position = new(10f, -10f, 10f);
                    DummyCardSlot = newSlot.GetComponent<CardSlot>();
                }
            }

            internal void CleanUp()
            {
                foreach (StrikeInfo atk in PendingAttacks)
                    Destroy(atk.Target);
                PendingAttacks.Clear();
            }

            internal bool HasPendingStrike(PlayableCard card) => PendingAttacks.Any(t => t.Attacker == card);

            internal void QueueMissileStrike(PlayableCard attacker, int value, CardSlot target)
            {
                GameObject aimIcon = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/SniperTargetIcon"), target.transform);
                aimIcon.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                aimIcon.transform.localRotation = Quaternion.identity;
                aimIcon.transform.localScale = new(2f, 1f, 2f);
                OnboardDynamicHoloPortrait.HolofyGameObject(aimIcon, GameColors.Instance.brightBlue);

                foreach (Renderer renderer in aimIcon.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material material in renderer.materials)
                    {
                        Tween.ShaderColor(material, "_MainColor", GameColors.Instance.glowRed, 2f, 0f, loop: Tween.LoopType.PingPong);
                        Tween.ShaderColor(material, "_RimColor", GameColors.Instance.glowRed, 2f, 0f, loop: Tween.LoopType.PingPong);
                    }
                }

                PendingAttacks.Add(new() { Attacker = attacker, AttackValue = value, Slot = target, Target = aimIcon, PlayerUpkeep = TurnManager.Instance.IsPlayerTurn, QueuedTurnNumber = TurnManager.Instance.TurnNumber });
            }

            public override bool RespondsToUpkeep(bool playerUpkeep) => PendingAttacks.Any(t => t.Slot.IsPlayerSlot != playerUpkeep);

            private static IEnumerator OverkillSimulator(int damage, PlayableCard attacker, CardSlot opposingSlot)
            {
                PlayableCard queuedCard = BoardManager.Instance.GetCardQueuedForSlot(opposingSlot);
                if (queuedCard != null)
                {
                    yield return new WaitForSeconds(0.1f);
                    ViewManager.Instance.SwitchToView(BoardManager.Instance.QueueView, false, false);
                    yield return new WaitForSeconds(0.3f);
                    if (queuedCard.HasAbility(Ability.PreventAttack) && attacker != null)
                    {
                        yield return TurnManager.Instance.CombatPhaseManager.ShowCardBlocked(attacker);
                    }
                    else
                    {
                        yield return TurnManager.Instance.CombatPhaseManager.PreOverkillDamage(queuedCard);
                        yield return queuedCard.TakeDamage(damage, attacker);
                        yield return TurnManager.Instance.CombatPhaseManager.PostOverkillDamage(queuedCard);
                    }
                }
            }

            public override IEnumerator OnUpkeep(bool playerUpkeep)
            {
                List<StrikeInfo> strikeQueue = PendingAttacks.Where(t => t.PlayerUpkeep == playerUpkeep && t.QueuedTurnNumber < TurnManager.Instance.TurnNumber).ToList();
                while (strikeQueue.Count > 0)
                {
                    StrikeInfo atkDefn = strikeQueue[0];

                    bool setCardSlot = false;
                    if (atkDefn.Attacker != null && atkDefn.Attacker.Dead)
                    {
                        BoardManager.Instance.GetSlots(playerUpkeep).Add(DummyCardSlot);
                        DummyCardSlot.opposingSlot = atkDefn.Slot;
                        atkDefn.Attacker.Slot = DummyCardSlot;
                        DummyCardSlot.Card = atkDefn.Attacker;
                        setCardSlot = true;
                    }

                    Destroy(atkDefn.Target);

                    GameObject missile = Instantiate(AssetBundleManager.Prefabs["Missile"], atkDefn.Slot.transform);
                    OnboardDynamicHoloPortrait.HolofyGameObject(missile, GameColors.instance.glowRed);
                    missile.transform.localEulerAngles = new(90f, 0f, 0f);
                    missile.transform.localPosition = Vector3.up * 5f;

                    bool explosionDone = false;
                    Tween.LocalPosition(missile.transform, new(0f, 0f, 0f), .3f, 0f, completeCallback: () => explosionDone = true);
                    yield return new WaitUntil(() => explosionDone);
                    AudioController.Instance.PlaySound3D("missile_explosion", MixerGroup.TableObjectsSFX, atkDefn.Slot.transform.position);

                    Tween.LocalPosition(missile.transform, Vector3.down * 3, 0.5f, 0f, completeCallback: () => Destroy(missile));

                    List<CardSlot> slotsToAttack = new() { atkDefn.Slot };
                    List<CardSlot> opposingSlots = BoardManager.Instance.GetSlotsCopy(atkDefn.Slot.IsPlayerSlot);
                    if (atkDefn.Slot.Index > 0)
                        slotsToAttack.Add(opposingSlots[atkDefn.Slot.Index - 1]);
                    if (atkDefn.Slot.IsOpponentSlot())
                        slotsToAttack.Add(null); // Indicator for the queue slot
                    if (atkDefn.Slot.Index < opposingSlots.Count - 1)
                        slotsToAttack.Add(opposingSlots[atkDefn.Slot.Index + 1]);

                    foreach (CardSlot slot in slotsToAttack)
                    {
                        if (slot == null)
                        {
                            yield return OverkillSimulator(atkDefn.AttackValue, atkDefn.Attacker, atkDefn.Slot);
                            continue;
                        }
                        yield return CustomTriggerFinder.TriggerAll<IOnPreSlotAttackSequence>(false, x => x.RespondsToPreSlotAttackSequence(slot), x => x.OnPreSlotAttackSequence(slot));
                        yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.SlotTargetedForAttack, false, atkDefn.Slot, atkDefn.Attacker);

                        if (slot.Card != null)
                        {
                            yield return GlobalTriggerHandler.Instance.TriggerCardsOnBoard(Trigger.CardGettingAttacked, false, slot.Card);
                            yield return slot.Card.TakeDamage(atkDefn.AttackValue, atkDefn.Attacker);
                        }

                        if (atkDefn.Attacker != null)
                            yield return CustomTriggerFinder.TriggerAll<IOnPostSingularSlotAttackSlot>(false, x => x.RespondsToPostSingularSlotAttackSlot(atkDefn.Attacker.Slot, slot), x => x.OnPostSingularSlotAttackSlot(atkDefn.Attacker.Slot, slot));

                        yield return CustomTriggerFinder.TriggerAll<IOnPostSlotAttackSequence>(false, x => x.RespondsToPostSlotAttackSequence(slot), x => x.OnPostSlotAttackSequence(slot));
                    }

                    if (setCardSlot)
                    {
                        Destroy(atkDefn.Attacker.gameObject);
                        DummyCardSlot.Card = null;
                        DummyCardSlot.opposingSlot = null;
                        BoardManager.Instance.GetSlots(playerUpkeep).Remove(DummyCardSlot);
                    }

                    strikeQueue.Remove(atkDefn);
                    PendingAttacks.Remove(atkDefn);
                }
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPostfix]
        private static void EnsureSetup() => _ = MissileStrikeManager.Instance;

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPostfix]
        private static void CleanupStrikes() => MissileStrikeManager.Instance.CleanUp();

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
        [HarmonyPrefix]
        private static bool NoAttacksForLaunches(PlayableCard __instance, ref List<CardSlot> __result)
        {
            if (__instance.HasAbility(AbilityID))
            {
                if (__instance.GetComponent<MissileStrike>()._firedThisTurn)
                {
                    __result = new();
                    return false;
                }
            }
            return true;
        }

        public static IEnumerator LaunchMissile(CardSlot target, Transform source, int amount, PlayableCard attacker, Vector3? initialOffset = null, float? scale = null)
        {
            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.5f);
            GameObject missile = Instantiate(ResourceBank.Get<GameObject>("p03kcm/prefabs/Missile"), source.transform);

            OnboardDynamicHoloPortrait.HolofyGameObject(missile, GameColors.instance.glowRed);
            missile.transform.localPosition = initialOffset ?? Vector3.down;
            if (scale.HasValue)
                missile.transform.localScale *= scale.Value;

            float flyDuration = scale.HasValue ? scale.Value * 10f : 10f;

            Tween.LocalPosition(missile.transform, Vector3.up * flyDuration, .4f, 0f, completeCallback: () => Destroy(missile));
            AudioController.Instance.PlaySound3D("missile_launch", MixerGroup.TableObjectsSFX, source.position);
            yield return new WaitForSeconds(1f);

            MissileStrikeManager.Instance.QueueMissileStrike(attacker, amount, target);
            yield break;
        }

        public static IEnumerator LaunchMissile(CardSlot target, CardSlot source)
        {
            yield return source.Card != null
                ? LaunchMissile(target, source.transform, source.Card.Attack, source.Card)
                : (object)LaunchMissile(target, source.transform, 1, null);
        }

        public override IEnumerator Activate()
        {
            if (_firedThisTurn)
                yield break;

            // We're doing a fake sniper attack sequence here
            List<CardSlot> opposingSlots = new();
            ViewManager.Instance.SwitchToView(BoardManager.Instance.CombatView, false, false);
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

            opposingSlots.Clear();
            int numAttacks = SniperFix.GetAttackCount(Card);

            if (Card.OpponentCard)
            {
                List<CardSlot> playerSlots = BoardManager.Instance.PlayerSlotsCopy;
                if (numAttacks >= playerSlots.Count)
                {
                    opposingSlots.AddRange(playerSlots);
                }
                else
                {
                    List<CardSlot> filledSlots = playerSlots.Where(s => s.Card != null).ToList();
                    filledSlots.Sort((a, b) => b.Card.PowerLevel - a.Card.PowerLevel);
                    if (filledSlots.Count >= numAttacks)
                    {
                        opposingSlots.AddRange(filledSlots.Take(numAttacks));
                    }
                    else
                    {
                        opposingSlots.AddRange(filledSlots);
                        opposingSlots.AddRange(playerSlots.Where(s => s.Card == null).Take(numAttacks - opposingSlots.Count));
                    }
                }
            }
            else
            {
                ViewManager.Instance.Controller.SwitchToControlMode(BoardManager.Instance.ChoosingSlotViewMode, false);
                ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                for (int i = 0; i < numAttacks; i++)
                {
                    //TurnManager.Instance.CombatPhaseManager.VisualizeStartSniperAbility(Card.Slot);
                    CardSlot cardSlot = InteractionCursor.Instance.CurrentInteractable as CardSlot;
                    // if (cardSlot != null && opposingSlots.Contains(cardSlot))
                    // {
                    //     TurnManager.Instance.CombatPhaseManager.VisualizeAimSniperAbility(Card.Slot, cardSlot);
                    // }
                    List<CardSlot> opponentSlots = BoardManager.Instance.OpponentSlotsCopy;
                    yield return BoardManager.Instance.ChooseTarget(
                        opponentSlots,
                        opponentSlots,
                        delegate (CardSlot s)
                        {
                            opposingSlots.Add(s);
                            TurnManager.Instance.CombatPhaseManager.VisualizeConfirmSniperAbility(s);
                        },
                        null,
                        null,//(s) => TurnManager.Instance.CombatPhaseManager.VisualizeAimSniperAbility(Card.Slot, s),
                        () => false,
                        CursorType.Target
                    );
                }
                ViewManager.Instance.Controller.SwitchToControlMode(BoardManager.Instance.DefaultViewMode, false);
                ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
                TurnManager.Instance.CombatPhaseManager.VisualizeClearSniperAbility();
            }

            foreach (CardSlot opposingSlot in opposingSlots)
            {
                ViewManager.Instance.SwitchToView(BoardManager.Instance.DefaultView, false, false);
                yield return LaunchMissile(opposingSlot, Card.Slot);
            }

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

            _firedThisTurn = true;
            _numberOfShotsFired += 1;

            if (ShotsRemaining == 0)
            {
                Card.Status.hiddenAbilities.Add(AbilityID);
                Card.RenderCard();
                // CardModificationInfo noMoreMissiles = new() { negateAbilities = new() { AbilityID } };
                // Card.AddTemporaryMod(noMoreMissiles);
            }
            else
            {
                Card.RenderInfo.OverrideAbilityIcon(AbilityID, MissileIcons[ShotsRemaining - 1]);
                Card.RenderCard();
            }
            yield break;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => Card.OpponentCard != playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            _firedThisTurn = false;
            yield break;
        }

        public bool RespondsToBellRung(bool playerCombatPhase) => Card.OpponentCard && !playerCombatPhase;

        public IEnumerator OnBellRung(bool playerCombatPhase)
        {
            yield return Activate();
        }
    }
}
