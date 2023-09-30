using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
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
            return defaultState.HasValue && defaultState.Value >= 0 && defaultState.Value <= 3 ? (AlarmState)defaultState.Value : AlarmState.Up;
        }

        private AlarmState CurrentState = AlarmState.Up;

        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private static readonly Dictionary<AlarmState, Texture> Textures = new();

        private RotatingAlarm()
        {
            CurrentState = GetDefaultState();
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

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RotatingAlarm),
                TextureHelper.GetImageAsTexture("ability_alarmup.png", typeof(RotatingAlarm).Assembly)
            ).Id;

            Textures.Add(AlarmState.Up, TextureHelper.GetImageAsTexture("ability_alarmup.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Left, TextureHelper.GetImageAsTexture("ability_alarmleft.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Right, TextureHelper.GetImageAsTexture("ability_alarmright.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Down, TextureHelper.GetImageAsTexture("ability_alarmdown.png", typeof(RotatingAlarm).Assembly));
        }

        public static AlarmState GetNextAbility(AlarmState current)
        {
            return current == AlarmState.Up
                ? AlarmState.Right
                : current == AlarmState.Right ? AlarmState.Down : current == AlarmState.Down ? AlarmState.Left : AlarmState.Up;
        }

        public Texture GetTextureForAlarm(AlarmState current)
        {
            return Card.OpponentCard
                ? current == AlarmState.Up
                    ? Textures[AlarmState.Down]
                    : current == AlarmState.Down ? Textures[AlarmState.Up] : Textures[current]
                : Textures[current];
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
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.25f);
            AudioController.Instance.PlaySound3D("cuckoo_clock_open", MixerGroup.TableObjectsSFX, gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), null, null, null, false);
            yield return new WaitForSeconds(0.1f);
            CurrentState = GetNextAbility(CurrentState);
            Card.RenderInfo.OverrideAbilityIcon(AbilityID, GetTextureForAlarm(CurrentState));
            Card.RenderCard();
            yield return new WaitForSeconds(0.3f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield break;
        }

        public int GetPassiveAttackBuff(PlayableCard target)
        {
            if (Card.Slot != null && target.Slot != null)
            {
                if (CurrentState == AlarmState.Up && target.Slot == Card.Slot.opposingSlot)
                    return 1;
                if (CurrentState == AlarmState.Down && target == Card)
                    return 1;
                if (CurrentState == AlarmState.Left && target.Slot == BoardManager.Instance.GetAdjacent(Card.Slot, true))
                    return 1;
                if (CurrentState == AlarmState.Right && target.Slot == BoardManager.Instance.GetAdjacent(Card.Slot, false))
                    return 1;
            }
            return 0;
        }
    }
}