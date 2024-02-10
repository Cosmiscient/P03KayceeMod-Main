# 4.3.7

- Mr:Clock now has 2 health instead of 3.
- The Mox Obelisk terrain now has the "Dust Giver" sigil instead of the "Great Mox" sigil.
- Made a number of difficulty and balance tweaks.
  - Tweaked the difficulty of the following encounters down: Attack Conduits, Stinky Conduits, Zoo
  - Tweaked the difficulty of the following encounters up: Obnoxious Conduits, Gem Shielders, 
  - Final boss: Slightly tuned down the difficulty of the first phase.

# 4.3.6

- Fixed a defect that caused the G0lly bossfight to softlock.

# 4.3.5

- Fixed a defect where selectable cards (i.e., cards in deck review) with custom gun animations caused the game to softlock.

# 4.3.4

- The "Guardians" (Ruby/Sapphire/Emerald Guardian) are now named "Sentinels" (Ruby Sentinel, Sapphire Sentinel, and Emerald Sentinel).
- Dialogue can now be advanced using the spacebar in addition to clicking with the mouse.
- The Busted 3D Printer now changes its portrait when the L33pB0t sidedeck challenge is active.
- Curve Hopper, Dr. Zambot, Ignitron, and Street Sweeper now have special gun models.
- Skeletons created by the Skeleclock ability retain their name.
- G0lly's friend cards now can't have energy costs higher than 6.
- "Unkillable when Powered" and "Unkillable" now work correctly together.
- Dr. Zambot's upgrade effect no longer makes beast mode cards revert to bot mode.
- Fixed a performance issue with the "Conduit Gain Ability" template where opposing cards moving from the queue to the board would cause a framerate drop.
- Fixed a defect where if you somehow managed to draw a Seed, playing it would softlock the game.
- Fixed a defect with the Kaycee's Run lifetime stats screen where stats were not properly being displayed.
- Made a minor visual tweak to the second phase of the final boss fight.

# 4.3.3

- Refactored how encounters are managed internally to be compatible with the latest version of Pack Manager.
- Added three encounter packs to be compatible with the latest version of Pack Manager.

# 4.3.2

- Fixed a defect with Splice Conduit. It now correctly reads the card's current attack and health when splicing instead of its base attack and health.

# 4.3.1

- Balance Change: Emerald Guardian now has 1 power instead of 2.
- Balance Change: Green Mox Buff now grants 2 health per gem instead of 1.
- Balance Change: Weeper now has 2 health instead of 3.
- Balance Change: Encapsulator now has 3 health instead of 2.
- Balance Change: Any card which has a gem now counts as as Gem Vessel. The biggest impact this will have is that Gem Dependent is easier to work with now.
- Tweaked the color balance of rare cards.
- A new cosmetic reward has been added for completing the Scarlet Skull achievement.
- Fixed some defects with the random salmon painting rule.
- Fixed some defects with the minimap where sometimes you could trigger fast travel at the wrong time.
- Fixed some edge case defects with the \[redacted\] card.
- Fixed a defect where the Vessel Heart (Box Bot sigil) ability was not correctly working with slot modification abilities.
- Fixed a visual defect that affected SeedBot during the final boss.
- G0lly no longer makes an unnecessary connection to the game's back end server.

# 4.3.0

- Balance Change: Shield Smuggler now has 3 health instead of 4.
- Challenge Update: Both of the Bounty Hunter wanted level challenges have been combined into one. A new challenge: "Strange Encounters" has been added. Battle modifiers (Sticker Battles, Trap Battles, etc) are no longer active by default; they are now gated behind this challenge.
- A number of challenge points have been modified. This doesn't change the challenge itself, but the amount of points its worth.
- Fixed a defect with the Eccentric Painter challenge and the composite rule infinite loop trigger.
- Properly fixed the audio layering defect with the Eccentric Painter challenge and the Unfinished Boss fight.
- The Dead Byte sigil now always damages its owner instead of always damaging the player.
- Fixed a defect with the interaction between Big Strike and Flying.
- The "reroll" purchase button now disappears correctly during the trade cards sequence.
- Fixed yet antoher defect with cleaning up the Eccentric Painter's paintings at the end of the battle. The battle with these bugs continues on...
- Fixed a visual defect with \[redacted\]
- Fixed a defect with the map generation algorithm

# 4.2.9

- Fixed a defect where the Eccentric Painter's painting would not clean up after boss fights.

# 4.2.8

- Fixed a visual defect in the third phase of the final boss fight when the Eccentric Painter challenge is active.
- Fixed a visual defect in the second phase of the final boss fight when a certain effect of the Eccentric Painter challenge is active.
- Fixed a visual defect with the painting displayer and the Eccentric Painter challenge.
- Fixed a visual defect where NPC models always had the same configuration as the player's model.
- Quests now have a fixed NPC face attached to each quest rather than a randomly generated face.
- Tweaked the Splinter Cell encounter.

# 4.2.7

- Fixed a defect with the Missile Launcher sigil where opponent cards could fire missiles even when out of ammo.
- Fixed a defect where the random bounty hunter ability was incorrectly calculating the energy cost of the bounty hunter.
- Fixed a defect where the sticker customization screen was trying to display ability stickers and then softlocking.
- Fixed a defect where Copypasta was not copying temporary mods.

# 4.2.6

- Fixed a defect where stickers in Sticker Battles sometimes didn't actually give the sticker ability to the card.

# 4.2.5

- The Eccentric Painter challenge paintings no longer orbit P03's face except during the Unfinished Boss.
- Mr:Clock's ability now always rotates clockwise.

# 4.2.4

- Fixed a softlock when using stickers on your cards.

# 4.2.3

- Latch Battles have now been replaced with Sticker Battles. They are functionally the same as before, except the abilities are not granted with latches, which means your latches can still work during the battle.
- The Eccentric Painter challenge now presents you with three randomly selected rules and asks you to pick one of them. This makes the challenge less difficult, but far more fair. As part of this change, all rules that had previously been banned are now unbanned.
- Gem Auger is now a rare, and the Gem Strike sigil is part of the rare card pool. The rulebook entry for Gem Strike has been updated to match the behavior of the sigil - there are now no longer any restrictions how many times the card can attack.
- Added a small delay between each of the molotovs when Sir Blast enters play.
- Hellfire Commando now costs 4 energy instead of 5.
- Made a small gameplay tweak to an early part of the P03 final boss battle.
- Made a small cosmetic tweak to the third phase of the P03 final boss battle.
- Fixed an issue where the Steel Trap sigil was accidentally giving out Wolf Pelts.
- Fixed a bug with the Laser Rifle animation/position during the target selection sequence.
- Fixed a bug with variable stat icons and the Photographer drone.
- Fixed a bug with the audio in the Unfinished Boss when the Eccentric Painter rule is active.
- Updated the icon for the Energy Hammer challenge and the No Remote challenge.

# 4.2.2

- Battle modifiers (latch battles, conveyor battles, etc) now appear less frequently and with lowered intensity.
- One of the empty backpack challenges is replaced with the "Missing Remote" challenge. This means that a Scarlet Skull run will still start with only one item (the Amplification Coil) but can now buy up to a maximum of two items. Scarlet Skull is *still* bonkers difficult.
- Fixed an animation issue with certain cards in the finale.
- Fixed a defect where quest givers would not remove cards from your deck when their purpose was complete.
- Tweaked the rewards for the Power Up The Tower quest.
- Fixed a defect that allowed you to duplicate the Hunter Brain.
- Updated the textures for slots on fire.

# 4.2.1

- A quick patch to fix the stats on one of the cards in the final boss.

# 4.2.0

- The final phase of the final boss has been modified to be a little bit harder.
- Death Latch is no longer considered a negative ability.
- The Electric sigil now rounds up instead of down.
- SwapBot actually works now. But I've said that before. Many times.
- Missile launchers fire faster now.

# 4.1.2

- In the last patch, I neglected to update the NPC dialogue to account for the change to quests. This is now fixed.
- \[redacted\] has lost its negative ability.
- Made a couple of small tweaks to how \[redacted\] are generated in the \[redacted\] region.
- Fixed an issue where paying to reroll card choices for \[redacted\] resulted in the same choices.
- Fixed a bug where abducting a card with a UFO did not cause gems to update.
- Fixed a bug where sometimes the Green Mox Buff sigil softlocked the game.
- Fixed a bug with the reward for the Bounty Target quest.
- Fixed a bug (hopefully) where the Mr:Clock sigil would softlock the game if the card died or was otherwise removed at an unexpected time.
- Fixed a bug where Conveyor Battles (and slot modifications in general) did not properly clean up when exiting to menu during battle.
- Fixed a bug where Conveyor Battles were not properly in the randomly selected battle modification pool.
- Fixed a bug where the missile launcher sigil breaks Build-a-Bot.
- Took another stab at making SwapBot actually work right.
- Added missing pixel art for Oil Jerry.

# 4.1.1

- If an NPC would give you an item as a quest reward, and your backpack is full, the NPC will now stick around until you have an empty slot to take the reward.
- Quests that modified battles for 5 games now only require 3.
- Fixed a bug where sometimes the Detonator ability would softlock the game in rare situations.
- Fixed a bug where the "set slot on fire" Canvas rule would softlock the game in some situations.
- Fixed a bug where the random Canvas rule challenge would break the Canvas boss.
- Fixed a bug where a random Canvas rule could be selected a second time during the Canvas boss.
- Fixed a bug where missiles don't cause Swapbot to swap under certain circumstances.

# 4.1.0

- A number of bugs that had their roots in the API have been fixed. **Make sure you upgrade to the latest version of the API when installing this!!**
- Updated the terrain (starting card conditions) for a number of battles. Also added two new terrain cards; one for the Gem region and one for the Undead region.
- Added three new effects to the Canvas rule set.
- Overhauled the encounters in the Resplendent Bastion zone to be slightly more intelligent.
- Fixed an issue relating to the interaction between Seedbot and the Fecundity sigil.
- In Latch Battles, the latched ability is now visible in the queue. This makes it easier to plan around your opponent's cards and makes their "on resolve on board" effects actually happen.
- Fixed some broken stuff with Build-A-Bot that was making it better than it was supposed to be. Also fixed some bugs with the Build-A-Bot user interface.
- Fixed a bug with the Photographer boss and Ability Conduits.
- Fixed a bug with Explodenate breaking under certain conditions.
- Fixed (hopefully for good) an issue where sometimes nodes on the map appeared on top of each other.
- Updated the dialogue database to consistently apply punctuation

# 4.0.4

- Cards with flying will now be able to fly over Holo Traps during Holo Trap Battles.
- Holo Trap Battles now deal 10 damage when the trap is sprung instead of outright killing it. This means you can survive them with a shield (e.g., shield generator) or tank it if you can somehow get enough health.
- Fixed a defect with the burning slots that I accidentally introduced with the last patch.
- Updated the texture of the Laser Rifle item.
- Fixed an issue that could arise with some bosses where certain scene effects wouldn't reset after battle.
- Made a small tweak to the boss scene effects for the final boss.

# 4.0.3

- Fire starting animations are faster across the board.
- Fire now triggers the Swapper sigil, and I made another tweak to the Swapper sigil to make it behave a little better. With these changes, the Swapper starter deck is also changed.
- One of the hidden quests was misbehaving - it should be fixed now.
- G0ph3r, P00dl3, and Hellfire Commando have new portraits.
- Flamecharmer is now 0/3 instead of 0/4, and gives all cards on your side of the board Made of Stone instead of just itself.
- Overhauled some of the trigger management code to prevent some enumeration changed bugs from happening.
- KNOWN ISSUE: There are problems with the latest version of the API and the way that shields behave. We are working on those as fast as we can.

# 4.0.2

- Some small tweaks to the way data is saved. This should hopefully make it safer to quit and reload in the middle of game sequences.
- Tweak to the battle modification system to prevent occasional softlocks when battles tried to clean up.

# 4.0.1

- OP Bot: Rolling Overclock or Skeleclock now adds +1 to attack. These abilities also now behave properly and actually remove the card from your deck.
- Latch Battles: Abilities that are not marked as usable by the opponent will no longer be assigned.
- Swapper Sigil: This has been re-implemented and should behave as expected.
- Keyboard Movement: Fixed some bugs related to keyboard movement.
- Updated the power level of a number of sigils. This was a change I meant to do earlier and just forgot.

# 4.0.0

- Huge new update!
- Expansion Pack 2 is now complete - with 50 (!!) new cards!
- The save system has been re-done. You might have some work to do.
- There are now achievements!
- And STICKERS! Completing achievements unlocks stickers that can be used to customize your cards with each run.
- Also a ton of bug fixes and balance changes.

# 3.1.6

- Fixed some defects with the \[redacted\] boss.

# 3.1.5

- Fixed a defect where Gamblobot's ability did not work
- Fixed a defect where Copypasta would get double impact from Build-A-Card cards
- Fixed a defect where L33pb0t side deck cards still had the null conduit ability
- Fixed a defect where effects could cause the scales to change after they had already tipped all the way to one side.
- Added a number of new abilities to the Build-A-Card pool
- Curve Hopper now costs 3 energy
- The text description for Summon Familiar is more concise
- Turbo Vessels now flip their portraits when switching direction
- Zip Bomb is now a neutral rare instead of an Undead rare
- Box Bot now costs 1 energy instead of 2
- The following battles have been turned down slightly: Bombs & Shields (Neutral), Bat Transformers (Foul Backwater), Gem Exploder (Gaudy Gem Land), Big Gem Ripper (Gaudy Gem Land), Spy Planes (Neutral)
- The following battles have been turned up slightly: Snake Transformers (Foul Backwater), Conduit Protectors (Resplendent Bastion), Shield Latchers (Filthy Corpse World), Final Boss

# 3.1.4

- Fixed compatibility with the Side Deck Selector mod

# 3.1.3

- Fixed softlock in phase 2 of G0lly
- Properly fixed the issue with the transformer event
- Fixed issue with the Seed Sprinter ability
- Fixed a defect with the map generator that sometimes caused battles to be set to difficulty level zero regardless of map location.
- Statistics count the correct number of bosses
- Trade nodes will no longer be generated in the initial quadrant of a map
- Fixed defect with Phase Through
- Full of Oil sigil now flips on the opponent side
- *Really* fixed \[redacted\] this time. At least, I hope so.
- Added the Electric sigil to the rulebook.
- Reduced coin reward for bosses from 10 to 5.

# 3.1.2

- All encounters have been reworked again.
- We are now capturing metrics every time you lose a battle. Those metrics are: name of encounter, turn you lost, and difficulty level. All metrics are captured anonymously.
- The Gamblobot sigil has updated art and rulebook text.
- Copypasta has new art.
- Copypasta and Angelbot can now be used by opponents.
- \[redacted\] should no longer throw an error when trying to remove \[redacted\]
- Transformer and Skeleclock should no longer spin out of control.
- Full of Oil will revert the camera view to its previous view instead of to View.Default at the conclusion of the animation.
- Save scumming the Skeleclock event is no longer possible.
- Quitting during the Recycler event no longer breaks the game.
- The Transformer event no longer allows you to set a card's energy cost higher than 6.
- Shovester now costs 1 Energy to activate (down from 2)
- ScrapBot now costs 4 Energy (up from 3)
- R4M's default stats are now 2/1 instead of 1/2.
- Gem Cycler works now. I'm about 66% sure.
- Non Functional Trinkets are slightly stronger.
- The Activated Bounty Hunter Brain is slightly stronger.
- The Gain Gem abilities are now part of the modular pool


# 3.0.3

- Tokens received from the recycler now have an energy cost based on the number of abilities they gain.
- CellEvolve and CellDeEvolve have proper behaviors for cards that don't have set evolutions.
- Rare cards are now red instead of gold and have portrait colors to match.
- There is now an animation for whenever a quest reward involves cards in your deck (to show you what's actually changed).
- The data cube is now functional during a damage race battle.
- NFTs should now be unique.
- The entire quest system was rewritten and now supports adding custom quests via an API (see included QuestAPI.md file for full documentation)

# 3.0.2

- Bosses now make you pay double your respawn fee after increasing it
- P03 final boss music volume increased
- Wording corrected on Turbo Vessels challenge
- Fixed bug: projector quad remained active once P03 dialogue started for the lives system explanation

# 3.0.1

- Eccentric Painter no longer places the canvas boss background behind bosses
- Side Deck Review sequence now updates along with enabled challenges
- Updated icon
- Updated challenge point values
- Fixed bug: l33pbots counted as gems
- Fixed bug: Viper was 3 energy, it was supposed to be 5
- Fixed bug: Boss rares didn’t appear
- Fixed bug: After dying once, players would receive a penalty after interacting with some nodes
- Known bug: When playing with Turbo Vessels enabled only, enemy vessels recieve the Double Sprinter sigil

# 3.0.0

- New lives system! When you die, you get one freebie. The next death, you’ll have to pay 5 coins to respawn. Next time, 10 coins, and so on.
- New dialogue possibilities on losing a run
- Explosive challenge and conveyor challenge are made neutral challenges and moved to the next page
- New Eccentric Painter challenge
- New Leaping Sidedeck challenge
- New Turbo Vessels challenge
- New Traditional Lives challenge
- New(ish?) Costly Lives challenge
- Tesla Coil item buffed, now provides two additional max energy
- Viper card buffed
- Oil Jerry buffed
- Goranj Vessel nerfed
- Bleene Vessel buffed
- Skeleton Lord buffed
- New Swapper Latcher card
- New art for mirror tentacle
- Fixed an API compatibility issue that made the quest system break with mods that added dialogue (thanks WhistleWind and NeverNamed)

# 2.0.0

- The NPC Update! Quests! New cards! A new boss maybe?!

# 1.0.0
- Initial version.