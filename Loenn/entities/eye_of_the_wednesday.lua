
local dyno = {}

dyno.name = "eow/EyeOfTheWednesday"
dyno.placements = {
    {
        name = "eye_of_the_wednesday",
        data = {
            verge_block_enable = false,
            music_layer_source_enable = false,
            music_source_light_control_flag="",
            global_decal_enable = false,
            bistable_decal_enable = false,
            cannot_transition_to_enable = false,
            refill_bubbler_enable = false,
            popping_mirror_enable = false,
            bird_down = false,
            guitar_hands_enable = false,
            guitar_hands_flag = "",
            guitar_hands_duration = 0.08,
        }
    }
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"

return dyno
