# 5.0.24
- Bugfix: Resolved an issue with how opponent cards were displayed in the secret final boss.
- Balance Change: Elektron now powers every card on the board, not just adjacent cards.

# 5.0.23
- Bugfix: Fixed a defect where if quest NPC tried to remove an item from your backpack, it would fail if your backpack was full.
- Bugfix: Fixed a defect related to non-combat damage in the secret final boss.
- Minor Bugfix: Updated the coloration of GEMS in dialogue.
- Balance Change: RH1N0 has an updated activated sigil. Instead of costing a flat 4 energy to activate for a +1 power boost, it now costs (Attack + 2) Energy to activate. 
- Balance Change: Energy Conduit now grants a reserve of energy equal to the player's max energy, rather than a flat 3 energy.

# 5.0.22
- Fixed a defect with Tapeworm introduced by the refactor to the sigilarium.

# 5.0.21
- Refactor: The entire mod has been re-organized. The majority of sigils are now part of the P03 Sigil Library mod, allowing other mods to reuse sigils without having to use this mod.
- Refactor: The slot modification code now exists in the API instead of in this mod.
- Balance Change: Mr:Clock now has 3 health instead of 2.

# 5.0.20
- Enhancement: Added a new behavior for when the Skeleclock, Gemify, and Beast Transformer nodes don't have any valid cards to upgrade in your deck. The Beast Transformer node will allow you to select a random Beast card to add to your deck. The Gemify node will allow you to add a Mox Sigil to your cards. And the Skeleclock node will let you choose a card to add to your deck that has been skeleclocked.
- Enhancement: A new upgrade node is now technically available. This node allows you to adjust the card cost of cards in your deck between energy, bones, blood, and gems. **However** this node only appears if there are already cards in the card pool with bones, blood, and gems cost. Since this does not happen in this mod, you won't see this unless you install other mods that add cards with other costs. Think of this as a "preview" feature in advance of additional work to come.
- Bug fix: Fixed a case where the Steel Trap sigil was still giving out Wolf Pelts.
- Bug fix: Gemfication properly copies to transforming cards.
- Hopeful bug fix: Added some additional validation checks when fast traveling to hopefully prevent random crashes when using the waypoint machine.
- Minor bug fix: Transforming cards will keep their original names when transforming.
- Minor bug fix: Changed the order that abilities trigger during the final challenge boss.
- Non functional change: The Steel Trap sigil will now give out "Junk" instead of "Empty Vessels"
- Non functional change: V1P3R is now called PYTH0N
- Non functional change: Updated bounty hunter art
- Non functional change: Updated the portrait for "yarr.torrent"

# 5.0.19
- Balance Change: Gamblobot's ability now costs 2 energy instead of 3.
- Bug Fix: Cards attacking the Experiment should no longer also attack the player.
- Bug Fix: The "I Love Bones" quest no longer automatically completes.
- Non functional change: Gamblobot has a new portait based off of the IRL Inscryption card.
- Non functional change: Plasma Jimmy's ability is now called "Head Shot" based off of the IRL Inscryption card.
- Non functional change: The fourth map no longer creates an empty secret room.
- Non functional change: Finally updated the internal plugin version number from 4.0 to 5.0

# 5.0.18
- G0lly's mole man now costs 1 blood instead of being free.
- Fixed a defect where the Scarlet Skull achievment was not unlocking after beating the final challenge boss.
- Fixed a defect with how delayed callbacks in the final challenge boss are processed to avoid rare collection modified errors.

# 5.0.17
- Slightly improved the random seed manager, specifically as it relates to the final challenge boss.
- The L33pBot model that appears on the player's deck when the Leaping Side Deck challenge is active is now animated.
- The Eccentric Painter challenge no longer activates during the fight with the Dredger.
- The Dredger's dialogue during battle now advances on input.

# 5.0.16
- I found and fixed the actual issue with the Challenges screen that I talked about in patch 5.0.14. The screen should work as expected now.
- Fixed a rarely occurring defect where the game could enter into an infinite loop on startup trying to load the save file.

# 5.0.15
- Leapbot Neo is now visually presented as a rare card.
- Fixed a defect where sometimes the Double Death sigil doesn't trigger.
- Fixed a visual bug with how the effects of the Eccentric Painter and Great Transcendence challenges interact.
- Improved some save file handling code to hopefully prevent some of the challenge level issues.

# 5.0.14
- Multiple bug reports have come in for the Challenge setup screen; I can't recreate those to properly test, but it's blocking enough people that I'm going to try this fix anyway. Here goes nothing.

# 5.0.13
- Added some foundational code in preparation for future expansion.
- Added some additional error handling to the "rotate board clockwise" effect. This should prevent the rare error that has been reported here.
- Updated the name of the deck editor bug fix patch so that it's clear it's not an issue if it fails to load.
- You can no longer pause the game while fast traveling (this is to prevent an exploit that would allow you to reset nodes in a region).
- All Page Two challenges (conveyor battles, bomb battles, and traditional lives) have been removed.
- Balance Change: Chippy no longer can shred spells or cards with the Unkillable sigil.

# 5.0.12
- More bug fixes!
  - Fixed a bug where a card with both Frozen Away and Full of Oil would softlock the game when it died.
  - Fixed some bugs with the Double Death sigil to prevent infinite loops.
  - Fixed The Perfect Crime achievement, which had been broken to where most players could not earn it.

# 5.0.11
- Another emergency bug fix for the day: Fixed a defect where going to Resplendent Bastion sometimes caused the game to loop indefinitely while trying to generate the map.

# 5.0.10
- Emergency bug fix: the starter deck screen is no longer broken

# 5.0.9
- Bug Fixes
  - Fixed a defect that prevented partner NPCs (like the Librarians and the Trader) from spawning.
  - The Experiment should no longer duplicate with the photographer drone.
  - Fixed a defect with one of the special cards in the final boss challenge.
  - Fixed some bugs with Transformer and build-a-bot cards. Note: Transformer has historically been the buggiest thing in this mod, and it's even weirdly buggy in the base game in some cases. There is a non-zero chance I broke something else that I haven't come across yet while fixing this issue. Only time will tell.


# 5.0.8
- Balance Changes
  - Reduced the frequency at which talking card quests appear.
- Bug Fixes
  - Fixed a defect where hammering a card with Tutor ability while the Double Death sigil is on the board caused a softlock.
  - Properly parsed new bounty hunter dialogue as being spoken by bounty hunters.
  - Removed the Gem trait from Mox Obelisk

# 5.0.7
- Balance Changes
  - Necronomaton now has 3 health instead of 1.
- Bug Fixes
  - Rebecha no longer shares a common node id with other interactables in the central hub node. This *really* resolves the softlock I thought I had fixed in the last patch.
  - Talking cards no longer trigger their "drawn" dialogue multiple times in a game if they have the Unkillable or Fecundity sigils.
  - Talking cards now use their "negative" dialogue when being selected to be traded away or recycled.
  - Fixed a broken interaction between Frozen Away and sigils that modify slots when the card dies.
  - Fixed yet another scripting error that was causing the Librarians to appear in multiple maps.

# 5.0.6
- Cards in the side deck can no longer be sacrificed to pay blood costs. This is *almost* a completely meaningless change, but is setting up some behaviors for future card expansions.
- Fixed a scripting error in Rebecha's dialogue that could softlock the game in some scenarios.
- Fixed a scripting error with the Librarians can could cause them to continue to appear in multiple maps.

# 5.0.5
- Hotfixes:
  - Fixed a defect where the game would softlock in geographies that use the `,` symbol as a decimal marker.

- Some known defects that are *still not* yet fixed (wait for a future patch):
  - Stickers misbehave on talking cards.
  - The two new talking cards have an oversized breathing animation.

# 5.0.4
- Hotfixes:
  - Fixed a defect inadvertently caused by the last hotfix - talking cards no longer softlock the game when discovered.
  - Modifications made to the training dummy are now transferred to the Lonely Wizbot

- Some known defects that are *still not* yet fixed (wait for a future patch):
  - Stickers misbehave on talking cards.
  - The two new talking cards have an oversized breathing animation.

# 5.0.3
- Hotfixes:
  - The dialogue for the Melter has been updated and improved to switch the voice sample sound depending upon the speaker.
  - Talking cards now speak when in selectable card events.
  - Talking cards no longer appear misaligned when displayed on P03's face in card upgrade events.
  - Chippy now properly identifies card selection as a "negative" event (i.e., talking cards will use their negative reactions instead of their positive reactions when you are choosing who to shred).
  - All talking card lines now have a slight delay at the end, which aligns with the base game behavior and should hopefully make them easier to read when discovered at the end of a quest.

- Balance Change:
  - Gas Conduit now costs 2 energy.

- Some known defects that are *not* yet fixed (wait for a future patch):
  - Stickers misbehave on talking cards.
  - The two new talking cards have an oversized breathing animation.

# 5.0.2
- Hotfixes:
  - The Kaycee NPC now uses the Gravedigger body instead of the Wildling body.
  - Updated the icon for the Strange Encounters challenge.
  - The Rebecha NPC will no longer appear on the regional maps.
  - The carnival game should no longer softlock the game if you have less than three items.
  - The Cell Steel Trap ability should no longer trigger when the card is not in a circuit.
  - The odds of talking card quests being prioritized have been lowered.
  - Fixed the border color of rare cards and the experiment card.

- Some known defects that are *not* yet fixed (wait for a future patch):
  - Stickers misbehave on talking cards.
  - Talking cards do not properly pause their dialogue when discovered, making it hard to read their dialogue.
  - Some talking cards do not properly talk when they appear in selection events.
  - The two new talking cards have an oversized breathing animation.
  - Some of the Melter's dialogue was improperly transcribed.

# 5.0.1

- Hotfixes:
  - Added some better error handling for the code that generates the random bounty hunter dialogue choices to prevent errors if too many bounty hunters have been generated already.
  - Fixed a defect where Rebecha's dialogue would not advance as the player moves from region to region.
  - Fixed a defect where G0lly would crash to desktop in certain situations. Ooops.
  - Fixed a defect where turning cards that provide gems into salmon would not cause the gem to be lost.
  - Replaced the portrait for the Sawyer talking card with the correct portrait.
  - Beating the Dredger no longer gives you an additional rare card. Ooops.
  - The Double Death ability now has the appropriate name in the rulebook.
  - Custom talking card dialogue has been broken down into smaller sections.
  - The volume on the Melter talking card has been lowered.
  - Fixed the emotion settings for a number of NPC dialogue entries.
  - The Melter now has the Flamethrower weapon mesh properly assigned.
  - Updated the figurine type for the Inspector NPC.

- Some known defects that are *not* yet fixed (wait for a future patch):
  - The border color on rare cards is incorrect.
  - Stickers misbehave on talking cards.
  - Talking cards do not properly pause their dialogue when discovered, making it hard to read their dialogue.
  - Some talking cards do not properly talk when they appear in selection events.
  - The two new talking cards have an oversized breathing animation.
  - Some of the Melter's dialogue was improperly transcribed.

# 5.0

- The Great Transcendence is complete

- Additions:
  - Two new achievements (and associated stickers) have been added.
  - One new challenge has been added.
  - Significantly overhauled the map generation algorithm, focused on expanding the types of terrain and environments that can be generated. The forest now has a snowline, the factory and undead crypt are now explorable, etc.
  - Added four new quests that have talking cards as their rewards for completion.
  - Two new secret rooms have been added to the map; one providing a cosmetic customization opportunity and one with an optional minigame.
  - Lockjaw Cell is now a common.
  - Urchin Conduit has been added as a new rare. It is a factory conduit that spawns Urchin Cells. Consequently, Urchin Cell is no longer in the card pool.

- Balance Changes
  - The blocker that G0lly plays during the first phase of the boss fight now scales with difficulty.
  - The "Buy Your Buddy" achievement now correctly unlocks at SP 6 instead of SP 5.
  - There was a defect with how pre-queued opposing terrain was being processed. A number of encounters that were supposed to start with pre-defined cards in the opponent queue simply were not. Fixing this defect will have a side effect of making a handful of encounters more difficult than they were before.

- Bug Fixes and Tweaks
  - Activated abilities can no longer be activated while the hammer is active.
  - Taurus now repeatedly destroys cards in the opposing slot until it is empty (e.g., Frozen Away, Bounty Hunter Brains, etc)
  - Custom cards with no names now are assigned a random name.
  - Wiseclock and UFO have been retextured.
  - Oil Jerry now has a small animation when he dies to help visualize his effect.
  - Bounty Hunter kills are now tracked as statistics.
  - The Hopper sigil can now appear in the sigil machine and the Build-A-Card machine.
  - The Flammable (M0l0t0v) sigil now flips vertically when on an opponent card. It also causes cards to have the "fuse" animation.
  - Cards with holographic portraits now properly re-activate their normal portraits when transforming.
  - Amber no longer appears on the map when she shouldn't. 
  - NPCs that had unique models in the base game (e.g., Amber/Pikemage) now have the same/similar models in this mod.
  - Expanded the bounty hunter generation algorithm with new names, dialogue, and portrait components.
  - Fixed a bug in the interaction between Electric and Sharp.
  - Fixed a bug where the original three beast transformation options were not appearing in the beast transformation node.
  - The "Transform When Powered" sigil now interacts properly with all cards that have predefined evolutions.
  - A bug fix for the Deck Editor mod by Peaiace is now included.
  - Fixed a number of bugs with the Experiment card. This card is still quite buggy; feel free to reach out to me on Discord as you discover more bugs.

# 4.4.1

- Resolved a memory leak with how asset bundles and audio files were being loaded and (not always) unloaded.
- Improved the performance of rendering RGB cards and made them only render on player cards.
- Fixed a defect where the Data Cube didn't work in generator battles
- All turbo vessels now are marked to flip their portraits when strafe goes in the opposite direction
- Fixed an issue with the rulebook as it related to a special item.
- Properly marked both sides of Asmanteus as being rare colored.
- Fixed an issue with how the generator battle resolved.
- Fixed an issue with how certain 3D cards rendered in the final boss battle.
- Made the "Burning Adrenaline" secret achievement slightly less difficult to unlock.

# 4.4

- Balance change: The "Overwhelming Entrance" sigil (3leph4nt's sigil) is now an activated ability that still triggers on entering play. The activation cost is 4 energy. This allows you to retrigger the Elephant's disruptive effect multiple times during battle. Additionally, this effect now shuffles the opponent queue and no longer shuffles terrain.
- The currency catchup mechanic was broken and players were receiving way too much extra money. Ultimately we decided to remove it altogether. Players will receive significantly less currency in pickup nodes. We will keep an eye on how this affects gameplay and determine if any additional tweaks to the currency system are necessary.
- Fixed a defect where a particular NPC's quest would never complete.
- Fixed a defect where the sticker printer could print stickers that were no unlocked.
- Added another patch to handle the randomly appearing robobucks node in Central Botopia.
- The Clock battle has been completely reworked and a new terrain card has been added to support the encounter.
- A **significant** update to difficulty: the top half of all battles (according to number of player deaths) have been made less difficult. This was accomplished by entirely eliminating one opponent card somewhere in turns 2-4.

# 4.3.13

- Fixed a bug where the Scarlet Skull achievement would not unlock. If you have been personally affected by this bug on any version less than 4.3.13, feel free to reach out to me on Discord for a retroactive fix.
- Fixed a bug where switching to and from the deck review with the fast travel map open would allow players to bypass the Broken Bridges challenge.
- Copypasta can no longer copy itself.
- Stickers no longer show through cards when facedown. 
- The enigmatic Tapeworm has replaced the Ringworm.

# 4.3.12

- Billdozer and 50er are now appropriately tagged to flip their portrait horizontally when the card moves left.
- Updated the border color of rare cards.
- Fixed a defect where items in the item shop showed two mouse cursors when hovering over them.
- Fixed a defect where the effects of the Strange Encounters challenge could bleed over into a subsequent run.
- Fixed a defect where sometimes the Overwhelming Entrance sigil would freeze the game indefinitely. 

# 4.3.11

- Made a slight change to the global difficulty curve. Regardless of the number of difficulty challenges active, Map 2 will now be slightly easier.
- Fixed an error in the Minecarts encounter where turn 3 spawned a Double Gunner on difficulty 4+. Made a couple of fun tweaks to the encounter while I was in there.

# 4.3.10

- All Mr:Clocks now have 2 health as previously advertised.

# 4.3.9

- Potentially fixed a couple of long-reported and difficult to reproduce bugs that I had not previously been able to resolve.
  - In some strange cases, an extra money drop would appear on the Central Botopia map. I was finally able to come up with a way to consistently make this happen, and then fixed it. Hopefully this will no longer happen.
  - If a sticker was already on a card, and you then opened the sticker interface and rotated that sticker without also moving that sticker, the position of the sticker would be lost. This has now been resolved.
  - The sticker tablet should no longer get stuck on the table if you rapidly open and close the deck review screen.

# 4.3.8

- Fixed a defect where the Recycler was giving out the incorrect token type.

# 4.3.7

- The formula for how much SP the Build-A-Bot machine gives you for your card has been modified to be more consistent across all types of cards. 
- Fixed a defect in the formula that determines how many Robobucks are in each pickup on the map. This should slightly increase the amount of money the player receives.
- Bounty Hunters and Friend Cards with Bifurcated Strike now have an additional attack reduction. Surprisingly, the base game did not have a check for this already. This will mostly affect very high bounty levels.
- Mr:Clock now has 2 health instead of 3.
- The Mox Obelisk terrain now has the "Dust Giver" sigil instead of the "Great Mox" sigil.
- The following encounters are moderately easier: Attack Conduits, Stinky Conduits, Zoo, Final Boss Phase One
- The following encounters are moderately harder: Obnoxious Conduits, Gem Shielders, Conveyor Latchers, Sentry Wall, Emerald Squids

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