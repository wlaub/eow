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

### Mirror Block

It's like a lock block, but I needed it to have a different depth a mirror effect for The Wednesday On The Edge Of Forever. I also wanted to be able to start the animation at the start of the unlock routine, but I couldn't do that without copypasting 100% of the code for the base object (private members are fascism).

### Fake Wall Dash Block

It's exactly the same thing as a regular Dash Block, but it tracks as a Fake Wall, which causes certain UI elements to (NPC interaction indicators) to be hidden "behind" it. It's a dash block that can hide NPCs and hopefully won't break anything.

### Dyno Hold

TODO: add an optional use sound
TODO: make a default sprite
TODO: make lonn plugin handle sprites, draw hitbox

It's sort of like a glider you can't pick up. If you grab while intersecting it, then you get a little boost depending on how you're moving. Grabbing while moving upward (or not falling too fast) gives a little upward hop, and grabbing while dashing gives a larger boost (actually it's smaller, but you still go further than if you weren't dashing). Grabbing while dashing horizontally gives a slightly larger horizontal boost, and grabbing while dashing diagonally down gives a much larger horizontal boost.

#### Sprite
If using Sprites.xml, the sprint needs an `idle` loop, an `active_single` that goes to `used`, an `active_multi` that goes to `idle`, and a `used` loop. `active_single` plays when using a single-use dyno, and `active_multi` plays for a multi-use dyno. `idle` is the default state for an unused dyno. `used` is the loop for a single-use dyno that has been used.

Example using booster as a placeholder, because it's round:
```xml
  <dyno path="objects/waldmo/booster/" start="idle">
    <Justify x="0.5" y="0.5"/>
    <Loop id="idle" path="booster" delay="0.1" frames="0-4"/>
    <Anim id="active_single" path="boosterRed" delay="0.08" frames="9-17,35" goto="used"/>
    <Anim id="active_multi" path="booster" delay="0.08" frames="0,2,0,2,0,2,0,2,3,4" goto="idle"/>
    <Loop id="used" path="boosterRed" delay="0.08" frames="35"/>
  </dyno>
```

### TODO: Verge Block

It's like a dream block but with custom textures and also if you fast fall into it then you'll dream dash and also maybe gravity could reverse when you do it that way like in that other video game "Verge".

## Triggers

### Entity Remover

A bigger hammer to make any entity a flag entity.

Warning: This can do some unexpected things. For example, Removing a red boost the player has used will also remove the player, rendering the game unresponsive to player input.

Only works on entities that implement point or rect collision. Should fail silently if they don't, but be sure to test to make sure it doesn't crash.

When activated, removes the entity nearest each of its nodes. Can be configured to active on entering a room and/or only if a flag is set.

### Area Introduction Cutscene

A trigger that pans the camera across a series of nodes and displays some text. 

The first trigger in the sequence should be marked as initial, and will trigger when the player enters the room. It will immediately jump to the `next_room`. During an intro cutscene, entering a room will cause the introduction cutscene trigger in it to activate, traverse the camera across its nodes, and then jump to the `next_room`. When a trigger has no `next_room`, it will instead end the cutscene and return the player to the spawn nearest where they entered the room.

The first node of the first trigger determine's the player's spawn point at the end of the cutscene (place it at the player's feet).

The trigger parameters control the speed that the camera moves, the delay before it starts moving (pause), and the delay after it stops moving before moving to the next room (hold). At present the speed is the duration in second between nodes, but should be the total duration to traverse the path or an actual fixed camera speed.

Note: the cutscene spawns the player at a spawn point in each room it visits. The player won't be visible, but can still be killed if spawning inside a hazard, causing the cutscene to loop forever. This softlocks the map. If you find yourself dying repeatedly, check for stray spawn points.

Note: if a cutscene trigger visits a room that doesn't have a cutscene trigger, then the player will probably be messed up until at least a room transition due to state not getting cleaned up. if the cutscene ends prematurely and the player is messed up, then you might be missing a cutscene trigger.

Note: there's no validation on the `next_room` field. Be careful.

Note: the intended usage of this is to have an initial cutscene trigger at the entrance to a room, jump through a sequence of rooms each containing a cutscene trigger with the trigger outside the bounds of the room and nodes denoting the camera path. The penultimate trigger has the initial room as its `next_room`. The final camera trigger is in the intial room and specifies a path that ends on the player. I am assuming there will be two camera triggers in the room where the cutscene starts, and one in each room it traverses. This is not enforced. Deviations are possible but may lead to undefined behavior. It may be possible to end in a room other than the initial room and then simply teleport back to the start. For an example of the intended usage, see https://github.com/wlaub/twoteof.

### Self-Destruct Triggers

These triggers are intended to create a timed self-destruct sequence that ends in the player escaping (by activating a cancel trigger) or dying in the mine (ending the map).

#### Self-Destruct Activate Trigger

The Activate trigger sets a timer, plays the start sound, and sets the `eow_sd_active` flag.

The while the self-destruct is active, the remaining time is displayed on the screen. When the player changes rooms, the current time is saved, and if the player dies then the timer is reset to the saved time. The countdown does not proceed while the game is paused.

Note: Pressing F5 will reset the timer to the saved checkpoint time. This means that a player can traverse a room, press f5, and then proceed to the next room without the time spent traversing the room counting. It's up to the player to decide what they do with this information.

The countdown sound, if provided, is played so that it ends at the same time as the timer. While the countdown sound is playing, the player can't pause, but can use a Cancel Trigger.

Once the countdown ends, the player is frozen, the game timer stops, the death sound plays, and the screen shakes while fading to white. After the screen fade and death sound both end, the maps is ended with no fanfare, and the player returned to chapter select.

#### Self-Destruct Cancel Trigger

The Cancel trigger cancels the self-destruct sequence as long as it's entered before the countdown ends.

### Trigger Sequence

TODO

A sequence of triggers that will do something if the player passes through them in the correct order. This is a noded trigger with the options:

* start active (for first trigger)
* activate triggers (for last trigger)

If the trigger is active when the player enters it, then it will set all the triggers at its nodes to active and set itself to inactive. If it's configured to activate triggers, then it will activate the triggers at its nodes instead

