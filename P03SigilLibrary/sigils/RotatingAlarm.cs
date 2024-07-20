using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class RotatingAlarm : AbilityBehaviour, IPassiveAttackBuff
    {
        public enum AlarmState : int
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        public const string DEFAULT_STATE_KEY = "DefaultAlarmState";

        private AlarmState GetDefaultState()
        {
            int? defaultState = Card.Info.GetExtendedPropertyAsInt(DEFAULT_STATE_KEY);
            if (defaultState.HasValue && defaultState.Value >= 0 && defaultState.Value <= 3)
                return (AlarmState)defaultState.Value;

            if (Card.OpponentCard)
                return AlarmState.Down;

            return AlarmState.Up;
        }

        private AlarmState CurrentState = AlarmState.Up;

        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private static readonly Dictionary<AlarmState, Texture> Textures = new();

        private void Start()
        {
            CurrentState = Card != null && Card.RenderInfo != null && Card.RenderInfo.overriddenAbilityIcons != null && Card.RenderInfo.overriddenAbilityIcons.ContainsKey(AbilityID)
                ? Card.RenderInfo.overriddenAbilityIcons[AbilityID].name.Equals(Textures[AlarmState.Up].name)
                    ? AlarmState.Up
                    : Card.RenderInfo.overriddenAbilityIcons[AbilityID].name.Equals(Textures[AlarmState.Down].name)
                    ? AlarmState.Down
                    : Card.RenderInfo.overriddenAbilityIcons[AbilityID].name.Equals(Textures[AlarmState.Left].name)
                    ? AlarmState.Left
                    : Card.RenderInfo.overriddenAbilityIcons[AbilityID].name.Equals(Textures[AlarmState.Right].name)
                    ? AlarmState.Right
                    : GetDefaultState()
                : GetDefaultState();

            Card.RenderInfo.OverrideAbilityIcon(AbilityID, GetTextureForAlarm(CurrentState));
            Card.RenderCard();
        }

        static RotatingAlarm()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Alarm Clock";
            info.rulebookDescription = "The creature that this sigil is pointing to will gain +1 attack. The clock will turn at the beginning of the player's turn.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.flipYIfOpponent = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixelability_rotating_clock.png", typeof(RotatingAlarm).Assembly));

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(RotatingAlarm),
                TextureHelper.GetImageAsTexture("ability_alarmup.png", typeof(RotatingAlarm).Assembly)
            ).Id;

            Textures.Add(AlarmState.Up, TextureHelper.GetImageAsTexture("ability_alarmup.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Left, TextureHelper.GetImageAsTexture("ability_alarmleft.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Right, TextureHelper.GetImageAsTexture("ability_alarmright.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Down, TextureHelper.GetImageAsTexture("ability_alarmdown.png", typeof(RotatingAlarm).Assembly));
        }

        public AlarmState GetNextAbility(AlarmState current)
        {
            int cIdx = (int)current + 1;
            if (cIdx > 3)
                cIdx = 0;
            return (AlarmState)cIdx;
        }

        public Texture GetTextureForAlarm(AlarmState current)
        {
            return Textures[current];
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            Card.RenderInfo.OverrideAbilityIcon(AbilityID, GetTextureForAlarm(CurrentState));
            Card.RenderCard();
            yield return new WaitForEndOfFrame();
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep != Card.OpponentCard;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            if (Card == null || Card.Dead || Card.RenderInfo == null)
                yield break;

            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.25f);
            AudioController.Instance.PlaySound3D("cuckoo_clock_open", MixerGroup.TableObjectsSFX, gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), null, null, null, false);
            yield return new WaitForSeconds(0.1f);
            if (Card != null && !Card.Dead && Card.RenderInfo != null)
            {
                CurrentState = GetNextAbility(CurrentState);
                Card.RenderInfo.OverrideAbilityIcon(AbilityID, GetTextureForAlarm(CurrentState));
                Card.RenderCard();
                yield return new WaitForSeconds(0.3f);
            }
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield break;
        }

        public int GetPassiveAttackBuff(PlayableCard target)
        {
            if (Card.Slot != null && target.Slot != null)
            {
                if (CurrentState == AlarmState.Up)
                {
                    if (Card.OpponentCard && target == Card)
                        return 1;
                    if (!Card.OpponentCard && target.Slot == Card.Slot.opposingSlot)
                        return 1;
                }
                if (CurrentState == AlarmState.Down)
                {
                    if (Card.OpponentCard && target.Slot == Card.Slot.opposingSlot)
                        return 1;
                    if (!Card.OpponentCard && target == Card)
                        return 1;
                }
                if (CurrentState == AlarmState.Left && target.Slot == BoardManager.Instance.GetAdjacent(Card.Slot, true))
                    return 1;
                if (CurrentState == AlarmState.Right && target.Slot == BoardManager.Instance.GetAdjacent(Card.Slot, false))
                    return 1;
            }
            return 0;
        }
    }
}