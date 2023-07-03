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

[TODO]
Sort of like a swap block that doesn't move. When the player dashes, there are a couple of frames where it grants a liftboost in the direction of its node to the player if the player is touching the block. The further the node, the stronger the boost. A node 3 tiles (24 pixels) away from the center of the block gives a liftboost equivalent to a swap block.

## Triggers

### Entity Remover

A bigger hammer to make any entity a flag entity.

Only works on entities that implement point or rect collision. Should fail silently if they don't, but be sure to test to make sure it doesn't crash.

When activated, removes the entity nearest each of its nodes. Can be configured to active on entering a room and/or only if a flag is set.

