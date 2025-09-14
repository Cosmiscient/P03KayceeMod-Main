using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class EmeraldShard : AbilityBehaviour
    {
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static EmeraldShard()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Emerald Shard";
            info.rulebookDescription = "When [creature] would be struck, an Emerald Mox is created in its place and a card bearing this sigil moves to the right.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.hasColorOverride = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            EmeraldShard.AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(EmeraldShard),
                TextureHelper.GetImageAsTexture("ability_emerald_shard.png", typeof(EmeraldShard).Assembly)
            ).Id;
        }

        public override bool RespondsToCardGettingAttacked(PlayableCard source) => source == this.Card && !this.lostTail;

        // Token: 0x060015BF RID: 5567 RVA: 0x00049C8F File Offset: 0x00047E8F
        public override IEnumerator OnCardGettingAttacked(PlayableCard card)
        {
            CardSlot slot = this.Card.Slot;
            CardSlot toLeft = BoardManager.Instance.GetAdjacent(this.Card.Slot, true);
            CardSlot toRight = BoardManager.Instance.GetAdjacent(this.Card.Slot, false);
            bool flag = toLeft != null && toLeft.Card == null;
            bool toRightValid = toRight != null && toRight.Card == null;
            if (flag || toRightValid)
            {
                yield return this.PreSuccessfulTriggerSequence();
                yield return new WaitForSeconds(0.2f);

                // Just do this first
                this.Card.Anim.StrongNegationEffect();
                this.Card.Status.hiddenAbilities.Add(this.Ability);
                this.Card.RenderCard();
                this.SetTailLost(true);

                if (toRightValid)
                {
                    yield return BoardManager.Instance.AssignCardToSlot(this.Card, toRight, 0.1f, null, true);
                }
                else
                {
                    yield return BoardManager.Instance.AssignCardToSlot(this.Card, toLeft, 0.1f, null, true);
                }
                yield return new WaitForSeconds(0.2f);
                CardInfo info = CardLoader.GetCardByName(this.Card.Info.temple == CardTemple.Tech ? "EmptyVessel_GreenGem" : "MoxEmerald");
                PlayableCard tail = CardSpawner.SpawnPlayableCard(info);
                tail.transform.position = slot.transform.position + Vector3.back * 2f + Vector3.up * 2f;
                tail.transform.rotation = Quaternion.Euler(110f, 90f, 90f);
                yield return BoardManager.Instance.ResolveCardOnBoard(tail, slot, 0.1f, null, true);
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return new WaitForSeconds(0.2f);
                tail.Anim.StrongNegationEffect();
                yield return this.StartCoroutine(this.LearnAbility(0.5f));
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }

        private void SetTailLost(bool lost)
        {
            this.lostTail = lost;
            if (this.Card.Info.tailParams != null)
            {
                if (lost)
                {
                    if (this.Card.Info.tailParams.tailLostPortrait != null)
                    {
                        this.Card.SwitchToPortrait(this.Card.Info.tailParams.tailLostPortrait);
                        return;
                    }
                }
                else
                {
                    this.Card.SwitchToPortrait(this.Card.Info.portraitTex);
                }
            }
        }

        private bool lostTail;
    }
}