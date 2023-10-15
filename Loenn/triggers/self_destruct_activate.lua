
local trigger = {}

trigger.name = "eow/SelfDestructActivateTrigger"

trigger.placements = {
    {
        name = "sd_normal",
        data = {
            timer_duration = 600,
            start_sound = "",
            countdown_sound = "",
            death_sound = "",
        },
    },
} 


return trigger;

