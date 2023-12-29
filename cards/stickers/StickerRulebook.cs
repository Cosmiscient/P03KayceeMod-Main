using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Cards.Stickers
{
    public class StickerRulebook : AlternateInputInteractable
    {
        public override CursorType CursorType => CursorType.Inspect;

        public Ability Ability { get; set; }

        public PlayableCard Card { get; set; }

        public override void OnAlternateSelectStarted()
        {
            RuleBookController.Instance.OpenToAbilityPage(this.Ability.ToString(), this.Card, false);
        }
    }
}