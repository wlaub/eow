
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
            fade_in_time = 2.0,
            initial = false,
            title_size = 2.5,
            sub_title_size = 1.25,
            title_color = "ffffff",
            sub_title_color = "ffffff"

        },
    },
} 

trigger.fieldInformation = {
    title_color = {
        fieldType = "color"
        },
     sub_title_color = {
        fieldType = "color"
        },
} 


return trigger;

