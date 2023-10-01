
local trigger = {}

trigger.name = "eow/AreaIntroCutscene"
trigger.nodeLimits = {2, -1}
--trigger.nodeLineRenderType = "fan"

trigger.placements = {
    {
        name = "normal",
        data = {
            title = "",
            sub_title = "",
            next_room = "",
            once = true,
            speed = 1.0,
            pause = 0.5,
            hold = 0.5,
            initial = false,

        },
    },
} 

return trigger;

