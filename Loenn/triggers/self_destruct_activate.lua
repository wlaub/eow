
local trigger = {}

trigger.name = "eow/SelfDestructActivateTrigger"

trigger.placements = {
    {
        name = "normal",
        data = {
            timer_duration = 600,
            start_sound = "",
            countdown_sound = "",
            death_sound = "",
            timer_color = "00ff00",
        },
    },
} 

trigger.fieldInformation = {
    timer_color = {
        fieldType = "color"
        },
}

return trigger;

