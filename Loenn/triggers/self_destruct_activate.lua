
local trigger = {}

trigger.name = "eow/SelfDestructActivateTrigger"

trigger.placements = {
    {
        name = "sd_normal",
        data = {
            timer_duration = 600,
        },
    },
} 


return trigger;

