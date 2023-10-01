# ErrandOfWednesday

Disclaimer: I have no idea what I'm doing.

## Entities

### Unique Jellyfish

When the player enters a room or otherwise respawns, only the unique jellyfish closest to the player will spawn. If the player is holding a jellyfish when entering a room, then none of the other jellyfish will spawn unless the one closest to the player has 'confiscate' enabled, in which case the player's held jellyfish will be removed.

The effect is that a player can seemlessly carry a jellyfish between rooms without producing duplicates, and a jellyfish can be placed near every respawn in a room without producing duplicates.

Currently doesn't interact with other jellyfish.

### Watchtower decal

Because of the way watchtowers are implemented, this only works with vanilla watchtowers.

A decal that can be configured to appear depending on whether the player is using a watchtower. If the decal is configured to appear when using a watchtower, it will appeari instally after the start animation finishes.

### Liftboost Block

Sort of like a swap block that doesn't move. 

When the player dashes while touching the block, it gives liftboost in the direction of the node and then goes on cooldown for 0.8 s.

With normalize enabled (the default), the liftboost will be scaled so that largest component is 240 (equivalent to a swap block). When normalize is disabled, the liftboost will be the vector to the node scaled by 5 (e.g. 1x1 tile offset gives 40x40 liftboost).

With instant enabled (the default), the block will grant liftboost on contact during the first 5 frames of activation. With instant disabled, it will grant liftboost for 30 frames.

With always on enabled (disabled by default), the usual timers are bypassed and the block is always in the active state.

If specified, the flag determines whether the block will be enabled on loading the room.

### Yet another collectable

Problem: there are 15 competing standards.

A custom collectable created for the wednesday on the edge of forever. It can do the heartbeat thing, it can do the wiggle thing, it can require that the player dash toward it to collect, it can pull the player to its center, it can play a sound, it can do a little animation, it can show a poem (without the heart background), it can activate triggers at its nodes, it can refill or unfill the player's dashes, it can set a flag, and (as with all other custom collectables), it's probably not quite what you're looking for.


## Triggers

### Entity Remover

A bigger hammer to make any entity a flag entity.

Warning: This can do some unexpected things. For example, Removing a red boost the player has used will also remove the player, rendering the game unresponsive to player input.

Only works on entities that implement point or rect collision. Should fail silently if they don't, but be sure to test to make sure it doesn't crash.

When activated, removes the entity nearest each of its nodes. Can be configured to active on entering a room and/or only if a flag is set.

### Area Introduction Cutscene

TODO

A trigger that pans the camera across a series of nodes and displays some text. Not sure yet how this will work given the need to transition between rooms. Might be a chain of triggers with one per room, which would also allow camera movement settings to be configured per-room or per-trigger.

### Self-destruct Timer

TODO

A trigger that sets a timer and ends the level when the timer expires. The timer should save its time each time the player's spawn location changes and reset when the player respawns. May also include triggers for canceling the timer and adding or removing time.

Should have the option to play arbitrary sounds at the beginning and end of the timer as well as at specific times.

Would be nice to be able to have the timer simply activate other triggers, but that doesn't seem feasible without ensuring the player will be in a specific room when the timer ends or duplicating the set of triggers in every applicable room.

### Trigger Sequence

TODO

A sequence of triggers that will do something if the player passes through them in the correct order. This is a noded trigger with the options:

* start active (for first trigger)
* activate triggers (for last trigger)

If the trigger is active when the player enters it, then it will set all the triggers at its nodes to active and set itself to inactive. If it's configured to activate triggers, then it will activate the triggers at its nodes instead

