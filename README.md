# ErrandOfWednesday

Disclaimer: I have no idea what I'm doing.

Many of these entities were created to server specific purposes in specific maps. Those maps can also serve as examples of their intended applications.

## Entities

Some entities must be placed in a room called !eow or ~eow to work. This allows the mod to locate these entities quickly when loading without scanning the entire map. !eow will be sorted to the top of the rooms list, and ~eow will be sorted to the bottom.

### Eye Of The Wednesday

This is a controller entity palced in a room called  either !eow (top of list) or ~eow (bottom of list) in order to indicate that certain entities are used in the map and should be loaded. Without it, the following entities/triggers will not work and might break something. The entity has a set of enable flags that must be enabled when using the corresponding entity. The log will show whether the entity has been found or not at the debug log level.

While I want the functionality provided by this mod to be available for use by others, I also want to minimize side effects in other maps that don't use this mod. Eye Of The Wednesday is a compromise that makes the mod less accessible, but guarantees that some functionality won't be enabled unless a map explicitly needs it. I also considered not publishing Loenn plugins for some of these entities, which would require other users to either develop from a local git repo instead of using gamebanana or manually supplement their gamebanana download with plugins from the source. That sounds like a pain in the ass compared to just looking up and reading the readme, so I didn't do that.

### The Wednesday On The Edge Of Forever

These entities were made for https://gamebanana.com/mods/492702

#### Unique Jellyfish

When the player enters a room or otherwise respawns, only the unique jellyfish closest to the player will spawn. If the player is holding a jellyfish when entering a room, then none of the other jellyfish will spawn unless the one closest to the player has 'confiscate' enabled, in which case the player's held jellyfish will be removed.

The effect is that a player can seemlessly carry a jellyfish between rooms without producing duplicates, and a jellyfish can be placed near every respawn in a room without producing duplicates.

Currently doesn't interact with other jellyfish.

#### Watchtower decal

Because of the way watchtowers are implemented, this only works with vanilla watchtowers.

A decal that can be configured to appear depending on whether the player is using a watchtower. If the decal is configured to appear when using a watchtower, it will appeari instally after the start animation finishes.

#### Liftboost Block

Sort of like a swap block that doesn't move. 

When the player dashes while touching the block, it gives liftboost in the direction of the node and then goes on cooldown for 0.8 s.

With normalize enabled (the default), the liftboost will be scaled so that largest component is 240 (equivalent to a swap block). When normalize is disabled, the liftboost will be the vector to the node scaled by 5 (e.g. 1x1 tile offset gives 40x40 liftboost).

With instant enabled (the default), the block will grant liftboost on contact during the first 5 frames of activation. With instant disabled, it will grant liftboost for 30 frames.

With always on enabled (disabled by default), the usual timers are bypassed and the block is always in the active state.

If specified, the flag determines whether the block will be enabled on loading the room.

#### Yet another collectable

Problem: there are 15 competing standards.

A custom collectable created for the wednesday on the edge of forever. It can do the heartbeat thing, it can do the wiggle thing, it can require that the player dash toward it to collect, it can pull the player to its center, it can play a sound, it can do a little animation, it can show a poem (without the heart background), it can activate triggers at its nodes, it can refill or unfill the player's dashes, it can set a flag, and (as with all other custom collectables), it's probably not quite what you're looking for.

#### Mirror Block

It's like a lock block, but I needed it to have a different depth a mirror effect for The Wednesday On The Edge Of Forever. I also wanted to be able to start the animation at the start of the unlock routine, but I couldn't do that without copypasting 100% of the code for the base object (private members are fascism).

#### Fake Wall Dash Block

It's exactly the same thing as a regular Dash Block, but it tracks as a Fake Wall, which causes certain UI elements to (NPC interaction indicators) to be hidden "behind" it. It's a dash block that can hide NPCs and hopefully won't break anything.

### Reasons

These entities were made for reasons

#### Dyno Hold

TODO: add an optional use sound
TODO: make a default sprite
TODO: make lonn plugin handle sprites, draw hitbox

It's sort of like a glider you can't pick up. If you grab while intersecting it, then you get a little boost depending on how you're moving. Grabbing while moving upward (or not falling too fast) gives a little upward hop, and grabbing while dashing gives a larger boost (actually it's smaller, but you still go further than if you weren't dashing). Grabbing while dashing horizontally gives a slightly larger horizontal boost, and grabbing while dashing diagonally down gives a much larger horizontal boost.

##### Sprite
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

#### Verge Block

Previously descoped from The Wednesday on the Edge of Forever.

Must be enabled by Eye Of The Wednesday:
When the level loads, this entity does a bunch of preprocessing to make outlines render correctly and connect across rooms.

Modified dream block. You can fastfall into it to enter without dashing, and then use that to trigger, for example, a gravityhelper trigger so that when you fall through it gravity flips kind of like in that video game "Verge".

Also custom textures.

The custom textures rendering is awful, and I'm sure there's gotta be a better way to do it using pre-generated looping backgrounds and masks (i mean reflections do masking), but I have no clue how and no desire to learn.


### EDGE (a game by) Mirrors

#### Invisible Spinner

An invisible spinner that can render its hitbox in Loenn and has options for locking and showing if it's off-grid. For building larger hazards with custom graphics using a familiar hitbox geometry.

#### Flag Initializer

Must place in !eow/~eow. Can place multiple. The listed non-empty flags will be set true when the chapter is started or restarted (Session.JustStarted = True). This allows flags to be consistently initialized when starting a map even when using ctrl+click to teleport.

#### Music Layer Source

Must be enabled by Eye Of The Wednesday:
When the level loads, this entity scans the map of instances of itself and creates instances of them to use internally for updating music parameters, including layers.

Fade a layer (or other music parameter) in and out based on player or camera distance. The sound sources are global so that they can be heard across rooms.

#### Global Decal

Must be enabled by Eye Of The Wednesday
When the map is loaded, the mod scans every room for instances of this entity and instantiates them.

This is a decal entity that has its global tag set so that it always renders no matter where you are in the map. 

#### Refill Bubbler

Must be enabled by Eye Of The Wednesday
This entity hooks Player.UseRefill to detect refills in an entity agnostic way.

When the player collects a refill while inside the entity's area, bubble the player and set/clear the use flag if specified.

#### Popping Mirror

Must be enabled by Eye Of The Wednesday
Hooks Session.SetFlag to detect flag changes.

A mirror that plays and animation and changes to a different sprite (e.g. breaks or unbreaks) when touched and/or when a flag is set. See 
`Graphics/Atlases/Gameplay/objects/eow/popping_mirror/default` for sprite structure.

#### Cannot Transition To Trigger

Must be enabled by Eye Of The Wednesdays to minimize side effects when not in use.

When the player is inside this trigger (and applicable flag conditions are met), MapData.CanTransitionTo returns false, preventing transitions even when gravity is inverted.

This is an alternative to the bits & bolts Block Screen Transitions trigger (https://codeberg.org/micycle/libbitsbolts/src/branch/main/Docs/index.md#user-content-block-screen-transitions), which only works when gravity is normal, but can block transitions in multiple directions. The bits & bolts trigger IL hooks Level.EnforceBounds, but gravity helper completely replaces Level.EnforceBounds when gravity is inverted, which prevents the bits & bolts trigger from working. Since both versions of Level.EnforceBounds use MapData.CanTransitionTo to determine whether to allow a transition, this issue can be bypassed by overriding that function, but such a transition block cannot readily determine direction (except maybe by checking the player's coordinates relative to the room bounds?) and so cannot block conditionally block transition based on direction.

Although the result from the original CanTransitionTo is not used when the player is still inside the trigger, it's always called in order to ensure other hooks can run.

## Triggers

### The Wednesday on the Edge of Forever

#### Entity Remover

A bigger hammer to make any entity a flag entity.

Warning: This can do some unexpected things. For example, Removing a red boost the player has used will also remove the player, rendering the game unresponsive to player input.

Only works on entities that implement point or rect collision. Should fail silently if they don't, but be sure to test to make sure it doesn't crash.

When activated, removes the entity nearest each of its nodes. Can be configured to active on entering a room and/or only if a flag is set.

#### Area Introduction Cutscene

A trigger that pans the camera across a series of nodes and displays some text. 

The first trigger in the sequence should be marked as initial, and will trigger when the player enters the room. It will immediately jump to the `next_room`. During an intro cutscene, entering a room will cause the introduction cutscene trigger in it to activate, traverse the camera across its nodes, and then jump to the `next_room`. When a trigger has no `next_room`, it will instead end the cutscene and return the player to the spawn nearest where they entered the room.

The first node of the first trigger determine's the player's spawn point at the end of the cutscene (place it at the player's feet).

The trigger parameters control the speed that the camera moves, the delay before it starts moving (pause), and the delay after it stops moving before moving to the next room (hold). At present the speed is the duration in second between nodes, but should be the total duration to traverse the path or an actual fixed camera speed.

Note: the cutscene spawns the player at a spawn point in each room it visits. The player won't be visible, but can still be killed if spawning inside a hazard, causing the cutscene to loop forever. This softlocks the map. If you find yourself dying repeatedly, check for stray spawn points.

Note: if a cutscene trigger visits a room that doesn't have a cutscene trigger, then the player will probably be messed up until at least a room transition due to state not getting cleaned up. if the cutscene ends prematurely and the player is messed up, then you might be missing a cutscene trigger.

Note: there's no validation on the `next_room` field. Be careful.

Note: the intended usage of this is to have an initial cutscene trigger at the entrance to a room, jump through a sequence of rooms each containing a cutscene trigger with the trigger outside the bounds of the room and nodes denoting the camera path. The penultimate trigger has the initial room as its `next_room`. The final camera trigger is in the intial room and specifies a path that ends on the player. I am assuming there will be two camera triggers in the room where the cutscene starts, and one in each room it traverses. This is not enforced. Deviations are possible but may lead to undefined behavior. It may be possible to end in a room other than the initial room and then simply teleport back to the start. For an example of the intended usage, see https://github.com/wlaub/twoteof.

#### Self-Destruct Triggers

These triggers are intended to create a timed self-destruct sequence that ends in the player escaping (by activating a cancel trigger) or dying in the mine (ending the map).

##### Self-Destruct Activate Trigger

The Activate trigger sets a timer, plays the start sound, and sets the `eow_sd_active` flag.

The while the self-destruct is active, the remaining time is displayed on the screen. When the player changes rooms, the current time is saved, and if the player dies then the timer is reset to the saved time. The countdown does not proceed while the game is paused.

Note: Pressing F5 will reset the timer to the saved checkpoint time. This means that a player can traverse a room, press f5, and then proceed to the next room without the time spent traversing the room counting. It's up to the player to decide what they do with this information.

The countdown sound, if provided, is played so that it ends at the same time as the timer. While the countdown sound is playing, the player can't pause, but can use a Cancel Trigger.

Once the countdown ends, the player is frozen, the game timer stops, the death sound plays, and the screen shakes while fading to white. After the screen fade and death sound both end, the maps is ended with no fanfare, and the player returned to chapter select.

##### Self-Destruct Cancel Trigger

The Cancel trigger cancels the self-destruct sequence as long as it's entered before the countdown ends.

### Future

#### My State Machine

TODO

The trigger has a state machine name, a list of input states and a single output state. When activated if, the current state is in the input states, transition to the output state and activate at nodes. Maybe also have flags of the form `eow_msm_<state_machine_name>_<state_machine_state>`.

Option to clear on death (default). Option to clear on leave room (default). Maybe allow saving state machines in session. Maybe option to clear on exit, end session.
