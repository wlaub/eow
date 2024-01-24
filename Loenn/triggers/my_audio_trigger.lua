
local trigger = {}

trigger.name = "eow/MyAudioTrigger"
trigger.nodeLimits = {0, -1}
trigger.nodeLineRenderType = "fan"

trigger.placements = {
    {
        name = "my_audio_trigger",
        data = {
            flag = "",
            block_group = "",
            flag_state = true,
            audio_event = "event:/",
            once_per_run = true,
            once_per_room = false,
            once_per_session = false,
            once_per_save = false,
            no_repeat = true,
        },
    },
} 

return trigger;

