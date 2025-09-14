# 1.1.17
- Surprise!
- Rebuilt the mod against the latest version of the API.

# 1.1.16
- Fixed a defect where using the Button Pusher sigil with the Submerge sigil would cause a softlock.
- The Forced Upgrade sigil will now copy modifications from the target card when the target has a pre-defined evolution.

# 1.1.15
- Fixed a defect with the Future Sight sigil interacting with sigils that respond to be drawn.
- Fixed a defect with the Copy and Paste sigil incorrectly copying sigils that should have been replaced.

# 1.1.14
- Fixed a defect with the Emergence Latch ability that prevented it from triggering on cards with the Detonator ability.

# 1.1.13
- The "Bounty Hunter" sigil now negates its own ability after the effect takes place. This prevents it from trying to trigger multiple times when redrawn or cloned in hand.

# 1.1.12
- Fixed a defect with the First Latch ability where it didn't always select the first ability visually on the card. 
- Fixed a defect where the Emerald Shard ability could trigger multiple times.
- Fixed a defect where using Shred on a Latcher with only a single card on board caused a softlock.
- Fixed a defect in the base game where gem providers dying to Sentry would sometimes still provide gems after they died.
- Updated the Iterate sigil to not copy card mods marked as "not copyable" when drawing a copy of itself.
- Updated the Button Pusher sigil to only cause *sigils* to fire instead of every possible behavior. This means that "secret" or "hidden" behaviors will not fire (e.g., the Bounty Hunter's death dialogue).

# 1.1.11
- Fixed a defect where abilities that caused a card to switch to the other side of the board would not always cause the gems manager to update.
- Fixed a defect with the Transform When Powered/Unpowered sigils where they didn't behave correctly when applied via latch or totem.
- Fixed a defect with the Conduit Gain Ability template that made it not always work correctly with the Photographer boss fight.

# 1.1.10
- Fixed a visual defect with the Dead Byte sigil when activating on the opponent's side of the board.

# 1.1.9
- Corrected the name for "Mental Gemnastics When Powered"
- Fixed the icon for the Drive sigil
- Fixed a defect where adding fuel to cards mid-game always resulted in maximizing their fuel gauge.
- Fixed a defect where triggered abilities granted by the Conduit Gain Ability template were not being properly cleaned up when the conduit died.
- Improved the AI for the Throw Slime and Tow Hook sigils.
- The Shred sigil no longer allows shredding of cards with "Unkillable When Powered"
- The Shred sigil now grants bones when cards are shredded.

# 1.1.8
- Fixed the behavior of the Macabre Growth sigil when on an enemy card.

# 1.1.7
- All fuel activated abilities were supposed to only be able to be activated once per turn. There was a bug that was preventing this from being enforced which has now been fixed. Relevant sigil descriptions have been updated to make this clear.
- Fixed a defect where sometimes cards with fuel would softlock the game when being spawned facedown.

# 1.1.6
- Updated the wording of the Slimeball sigil.
- Fixed the sigil art for the Annoying Latch sigil
- Macabre Growth now checks the card's health when it enters play and forces the card to die if it has 0 or less health. This allows you use this sigil to create cards with 0 base health.

# 1.1.5
- The replicating firewall behavior no longer triggers if the card was sacrificed.

# 1.1.4
- Fixed a defect with the Future Sight sigil that would cost the game to softlock when a nonplayable card is on the top of the deck.
- Fixed some edge case scenarios with the Iterate sigil that caused it to softlock.
- The Hopper sigil now causes triggers to fire when the card is reassigned to a new slot. The most immediate impact this will have is that hopping in front of a card with Sentry will cause it to be attacked.

# 1.1.3
- Fixed a defect with the Coal Roller sigil that caused it to spam the log with errors and not actually have an effect.
- Fixed a visual defect with the Coal Roller and Full of Blood sigils that caused the provided sigils to not be properly recolored.
- Fixed a defect with the Shatter sigil where it did nothing when sacrificing cards that had the Detonator sigil.
- Modified the Catch Fire sigil to allow it to affect any slot regardless of owner or card.
- Modified the Full of Blood sigil to not flip orientation on opponent cards.
- Added an act 2 renderer for fuel cards.

# 1.1.2
- Fixed a defect with abilities that changed color based on whether or not the player has fuel.
- Fixed a defect where the Molotov ability would soft lock the game if not running with P03 In Kaycee's Mod installed.
- Fixed a defect where the Button Pusher sigil would crash the game.

# 1.1.1
- Fixed a serious defect where cards could not be drawn from the side deck, and cards in the main deck were sometimes failing to draw correctly as well.

# 1.1.0
- Significant rulebook overhaul to take advantage of new API capabilities.
- Fire Strike is now a bounty hunter sigil.
- Fire effects now change color based on act (blue for P03, red for Leshy, green everywhere else)

# 1.0.9
- Updated the fuel gauge for disk cards.
- Fixed a number of issues with sigil descriptions being incorrect, poorly written, typos, etc.

# 1.0.8
- Fixed an issue where missile launches softlocked if they were spawned by non-card entities.

# 1.0.7
- Fixed an issue where the Bonus Energy Conduit ability would not work if the circuit was completed by an adjacent card.

# 1.0.6
- Increased the power level of Static Electricity. It now powers every card on the board, not just neighbors.

# 1.0.5
- Fixed a defect where "active when powered" sigils were not properly changing colors when powered.
- Added some new sigils

# 1.0.4
- Fixed a defect with the fuel manager and Paper Cards
- Fixed a defect where Explodonate When Powered was not triggering correctly.

# 1.0.3
- Added some new sigils.

# 1.0.2
- Fixed a defect with the code that discovers sigils on rare cards.

# 1.0.1
- Fixed a defect with missiles introduced by the refactor to the sigilarium

# 1.0.0

- Initial release of the P03 Sigilarium. I pulled a *lot* of code out of P03 in Kaycee's Mod to make this work, and there's a good chance I messed something up in here. Find me on discord if you find a bug in this.