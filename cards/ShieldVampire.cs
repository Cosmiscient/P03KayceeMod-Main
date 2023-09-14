using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace P03KayceeRun.cards
{
    public class AbsorbShield : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static AbsorbShield()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Shield Absorption";
            info.rulebookDescription = "When [creature] is played, all creatures lose their shields. This creature gains 1 attack for each shield lost this way.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(AbsorbShield),
                TextureHelper.GetImageAsTexture("ability_shield_vampire.png", typeof(AbsorbShield).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            Card.Anim.StrongNegationEffect();
            int shields = 0;
            foreach (CardSlot slot in BoardManager.Instance.AllSlotsCopy)
            {
                if (slot.Card != null && slot.Card.HasShield())
                {
                    slot.Card.Status.lostShield = true;
                    slot.Card.Anim.StrongNegationEffect();
                    if (slot.Card.Info.name == "MudTurtle")
                    {
                        slot.Card.SwitchToAlternatePortrait();
                    }
                    slot.Card.UpdateFaceUpOnBoardEffects();
                    shields += 1;
                }
            }

            if (shields > 0)
                Card.AddTemporaryMod(new(shields, 0));

            yield break;
        }
    }
}