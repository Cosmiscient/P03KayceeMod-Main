using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace Infiniscryption.P03ExpansionPack3.Sigils
{
    public class ActivateEverything : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static ActivateEverything()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Button Pusher";
            info.rulebookDescription = "When [creature] is played, all activated sigils and sigils that trigger on play or on death are activated.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AbilityID = AbilityManager.Add(
                P03Pack3Plugin.PluginGuid,
                info,
                typeof(ActivateEverything),
                TextureHelper.GetImageAsTexture("ability_activate_everything.png", typeof(ActivateEverything).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            var slots = BoardManager.Instance.GetSlotsCopy(Card.IsPlayerCard())
                        .Concat(BoardManager.Instance.GetSlotsCopy(!Card.IsPlayerCard()))
                        .ToList();

            // Resolve on board
            foreach (var slot in slots)
                if (slot.Card != null && !slot.Card.Dead)
                    yield return slot.Card.TriggerHandler.OnTrigger(Trigger.ResolveOnBoard);

            // ACtivated abilities
            foreach (var slot in slots)
            {
                if (slot.Card != null && !slot.Card.Dead)
                {
                    // Get all abilities and hunt for activated abilities
                    foreach (var ability in slot.Card.AllAbilities())
                    {
                        var info = AbilitiesUtil.GetInfo(ability);
                        if (info.activated)
                        {
                            yield return slot.Card.TriggerHandler.OnTrigger(Trigger.ActivatedAbility, ability);
                        }
                    }
                }
            }

            // Pre death animation
            foreach (var slot in slots)
                if (slot.Card != null && !slot.Card.Dead)
                    yield return slot.Card.TriggerHandler.OnTrigger(Trigger.PreDeathAnimation, false);

            // Die
            foreach (var slot in slots)
                if (slot.Card != null && !slot.Card.Dead)
                    yield return slot.Card.TriggerHandler.OnTrigger(Trigger.Die, false, null);
        }
    }
}
