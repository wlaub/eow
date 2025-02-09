
local dyno = {}

dyno.name = "eow/EyeOfTheWednesday"
dyno.placements = {
    {
        name = "eye_of_the_wednesday",
        data = {
            verge_block_enable = false,
            music_layer_source_enable = false,
            global_decal_enable = false,
            cannot_transition_to_enable = false,
            refill_bubbler_enable = false,
        }
    }
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"

return dyno
