# What is the Quest API?

The quest API is a public-facing API that allows *anyone* to create a mod that adds new quests to the game. This API is subject to change at any time while we work out the kinks, but all internal quests (other than some super-secret quests) are built using this API.

## How do Quests work?

To understand how to use the Quest API, you need to understand how a quest works. Quests are defined using a `QuestDefinition`, which contains metadata about the quest and reports its overall status. Within a Quest are a number of `QuestState` objects, which represent the various states that a quest can be in. Each state has a `Status` which tells you if that state is active, successful, failed, or not started. Most `QuestState`s will be linked to a future `QuestState` based on status. Some `QuestState`s will have `QuestReward`s associated with them that are granted if the state is successfully completed.

### Example

Let's take a fairly complex example: the Power Up The Tower quest. In this quest, if the player accepts the quest, they are given a Radio Tower card in their deck. They must then take that card into battle, play it, and keep it alive for five total game turns. If they succeed, they are paid some money. If they fail; well, nothing happens other than they wasted their time.

Okay, let's think about this in terms of states:

1. The player does not know about the quest. In this state, the NPC explains the quest and says "talk to me again to start the quest." This quest should automatically advance to the next state after the NPC speaks.
2. The player is ready to accept the quest. If the player talks to the NPC now, they should be given the RadioTower card and moved to the next state.
3. This is the complicated state. Here, we need to count how many turns the player has used the RadioTower. If it's less than 5, this status is "Active." If it's 5 or greater, this status is "Success." If the player has lost the RadioTower card somehow, this status is "Failure."
4. (Failure) If the player failed, the NPC should say something snarky, and then the quest concludes.
5. (Success) If the player succeeds, the NPC should remove the RadioTower card from the player's deck and then give them some money, and the quest concludes.

So states 1, 2, 4, and 5 should automatically advance whenever you talk to the NPC.

Let's see how this is implemented in code:

```
using Infiniscryption.P03KayceeRun.Quests;

namespace ExampleQuests;

internal static class MyQuests
{
    internal static QuestDefinition PowerUpTheTower { get; private set; }

    internal state void CreateQuests()
    {
        PowerUpTheTower = QuestManager.Add(P03Plugin.PluginGuid, "Power Up The Tower");

        var towerActiveState = PowerUpTheTower.AddDialogueState("LOOKING FOR A JOB?", "P03PowerQuestStart")
                        .AddDialogueState("LOOKING FOR A JOB?", "P03PowerQuestAccepted")
                        .AddGainCardReward("POWER_TOWER")
                        .AddDefaultActiveState("GET BACK TO WORK", "P03PowerQuestInProgress")
                        .SetDynamicStatus(() => {
                            if (PowerUpTheTower.GetQuestCounter() >= 5)
                                return QuestState.QuestStateStatus.Success;
                            else if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == "POWER_TOWER"))
                                return QuestState.QuestStateStatus.Failure;
                            else
                                return QuestState.QuestStateStatus.Active;
                        });

        towerActiveState.AddDialogueState("YOU BROKE IT?!", "P03PowerQuestFailed", QuestState.QuestStateStatus.Failure);
        towerActiveState.AddDialogueState("HERE'S YOUR PAYMENT", "P03PowerQuestSucceeded")
                        .AddLoseCardReward("POWER_TOWER")
                        .AddDynamicMonetaryReward();
    }
}
```

The easiest (best) way to make quests is to use the fluent extension methods and chain properties together. Note that whenever you use this to create a new quest state, all further calls in the chain modify that new quest state.

### Creating a new Quest 

First, we create a blank `QuestDefinition` using `QuestManager.Add`. You need to supply your plugin's guid and the quest name.

### Creating Dialogue States

Next, you need to create the first quest state. Almost always, this is going to be a standard "dialogue state," in which the NPC says something and the quest automatically advances to the next other state. You need to provide two pieces of information: the text that will appear when the player hovers the mouse over the NPC, and the ID of the dialogue that will be played. In this example, we actually create *two* dialogue states. The first state is simply an introduction. The second state acts as the "confirmation" that the player wants to accept the quest.

### Creating a State Reward

In this example, the next thing we do is add a quest reward. The second state grants a reward: the "POWER_TOWER" card. This is done as `.AddGainCardReward`.

The types of rewards currently supported are:

- AddGainCardReward: Adds the named card to the player's deck
- AddLoseCardReward: Removes the named card from the player's deck
- AddGainItemReward: The player gains an item with the given name, assuming they have an empty item slot
- AddLoseItemReward: The player loses the item with the given name, assuming they currently own that item.
- AddGemifyCardsReward: A specified number of cards in the player's deck (chosen randomly) are gemified
- AddGainAbilitiesReward: A specified number of cards in the player's deck (chosen randomly) gain the specified ability
- AddMonetaryReward: The player earns the specified number of robobucks
- AddDynamicMonetaryReward: The player gains a monetary reward that scales with difficulty and map (equal to the average number of robobucks they would expect to get from one of the coins scattered across the map).
- AddSuccessAction: Accepts an `Action` delegate which is executed whenever the state is completed successfully. Functionally, this allows you to code your own quest reward on the fly.

### Default Active States

Next, we add a "default active state." The purpose of this helper is to simplify tracking the basic state of a quest. Normally, if you want to ask what state a quest is in, you have to check against the state name (e.g., `MyQuest.CurrentState.name == SOMETHING`). However, this function creates a super simple helper "default" state, with a matching helper to see if you're in that default state (e.g., `MyQuest.IsDefaultState()`). If your quest just has a single "main" state, this helper will be the simplest way to construct the quest.

### Default Quest Counter

Each quest comes with a helper called "GetQuestCounter()" and "IncrementQuestCounter()". These are just simple dummy counters that start at 0 and can be incremented whenever you want. These are just helpers so that you don't have to interact with the save file yourself; you can use this to track anything you want. In this example, we are tracking how many times the player uses the radio tower. The radio tower card has a special card ability that increments this counter during the upkeep every turn it is on board, and the state status (see below) looks at this counter to see if it's high enough.

### How State Status Changes

By default, the status of a quest state does not change. Something external must tell the quest to move to the next state (by saying `MyQuest.CurrentState.Status = QuestState.QuestStatus.Success` for example). Dialogue states automatically advance with a success status once you talk to the NPC, but all other states just wait until something moves them. It's then up to you, the quest writer, to do whatever you need to do (with patches and whatever else) to move the quest along.

The second option is to code a dynamic status on the quest. The line that does that in this example is:

```
    .SetDynamicStatus(() => {
        if (PowerUpTheTower.GetQuestCounter() >= 5)
            return QuestState.QuestStateStatus.Success;
        else if (!Part3SaveData.Data.deck.Cards.Any(c => c.name == "POWER_TOWER"))
            return QuestState.QuestStateStatus.Failure;
        else
            return QuestState.QuestStateStatus.Active;
    });
```

The dynamic status is set using a simple delegate that returns a quest status. In this case, the status first asks if the radio tower was used enough times; if so, the status is Success. Next, if the player is missing the critical card, they fail (note that the way this is coded, they can lose the power tower card *after* having used it enough times, and it's still okay). Finally, if neither of these are true, the status is Active (in other words, we're still waiting).

Note that since there are multiple paths out of this `QuestState`, the "chained" calls stop here in the original example. We need to keep a reference to this quest state because we need to add two different states to it: what happens when you fail, and what happens when you succeed.

### End States

In this example, we go ahead and add a quest state for the failure dialogue. To do this, we use the same `AddDialogueState` helper we've been using, but we use the secret third parameter to say that this state is only used in failure.

```
towerActiveState.AddDialogueState("YOU BROKE IT?!", "P03PowerQuestFailed", QuestState.QuestStateStatus.Failure);
```

End states are very important. When an end state concludes, the NPC disappears - forever. The pattern is always to end a quest with a dialogue state that wraps up the quest and concludes the player's interaction with that NPC.

"But what if I want my quest to end in a state where the NPC just keeps saying the same thing over and over again instead of concluding?" This is currently not supported (although it might be in the future). The problem is that this can only happen if a quest state stays in Active status forever. If you do this, the game will see that you have an unfinished quest as you move from map to map, and will move that NPC along with you so that the player has a chance to finish each quest that they start. If you want the NPC to stop appearing, the quest needs to finish.

To conclude the example: the last thing we need to do is add what happens when the quest concludes successfully:

```
towerActiveState.AddDialogueState("HERE'S YOUR PAYMENT", "P03PowerQuestSucceeded")
                .AddLoseCardReward("POWER_TOWER")
                .AddDynamicMonetaryReward();
```

We have a final dialogue where the NPC says "thank you," and when that dialogue concludes the player will lose the power tower card and gain a dynamic monetary reward.

And that's it! We're done!

### Multi-Part Quests

A multi-part quest is a quest that is specifically designed to take part across multiple maps. Functionally, this is implemented by creating multiple quests, one for each part. The second part of the quest must indicate which quest is the previous part like so:

```
Donation = QuestManager.Add(P03Plugin.PluginGuid, "Donation");
DonationPartTwo = QuestManager.Add(P03Plugin.PluginGuid, "DonationPartTwo").SetPriorQuest(Donation);
```

If a player completes the first part of a quest, the second part will be generated in the next map they explore.

Quests will only be generated if there is (theoretically) enough time to complete it. So (for example), a two-part quest will never be started on the final map of the run.

### Quest Generation

There are two ways that a quest can be added to a map.

#### Randomly Selected Quests

Each map is given *exactly one* randomly selected quest. Any quest in the pool that has not yet been selected is eligible here, unless there is not enough time left in the run to complete it (see the section on Multi-Part Quests). 

You can also add additional criteria for your quest to be selected, using the `SetGenerateCondition` extension method. This requires you to supply a `Predicate` that returns `true` or `false` based on whether or not the quest should be generated. There is also an additional helper method called `SetRegionCondition` which creates a generate condition based on which zone is being generated (for example, if your quest should only appear in the forest).

#### "Must Be Generated" Quests

Additionally, some quests *must* be generated. Some of the game's built-in secret quests are like this, but generally this is reserved for quests which are currently active. This could be a single-part quest that never finished, or a multi-part quest where the first part did complete and the second part needs to be generated.

## Another Example

Just to help with understanding the API: here is another example of another quest. 
If the player takes the quest on, they start each battle by taking one damage until they've won five battles like this.
The quest itself just tracks when the default counter has reached 5...
```
TippedScales = QuestManager.Add(P03Plugin.PluginGuid, "Tipped Scales")
                    .SetGenerateCondition(() => EventManagement.CompletedZones.Count <= 2);
TippedScales.AddDialogueState("TOO EASY...", "P03TooEasyQuest")
            .AddDialogueState("TOO EASY...", "P03TooEasyAccepted")
            .AddDefaultActiveState("KEEP GOING...", "P03TooEasyInProgress", threshold:5)
            .AddDialogueState("IMPRESSIVE...", "P03TooEasyComplete")
            .AddGainAbilitiesReward(1, Ability.DrawCopyOnDeath);
```

...then we have to write patches for the game to make the rest of the quest work. 
Not all of those patches are shown here, but here's a simple one to show how we engage with the quest to see if it is active by 
checking the `IsDefaultActive` helper:

```
[HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
[HarmonyPostfix]
private static IEnumerator TippedScalesQuest(IEnumerator sequence)
{
    yield return sequence;

    if (SaveFile.IsAscension)
    {
        if (MyQuests.TippedScales.IsDefaultActive())
        {
            yield return LifeManager.Instance.ShowDamageSequence(1, 1, true, 0.125f, null, 0f, false);
        }
    }
}
```