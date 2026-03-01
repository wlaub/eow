
local entityRemover = {}

entityRemover.name = "eow/SpeedBoostFieldTrigger"
entityRemover.nodeLimits = {1, 1}
entityRemover.nodeLineRenderType = "fan"

entityRemover.placements = {
    {
        name = "speed_boost_field_trigger",
        data = {
            speed = 240,
--            flag = "",
        },
    },
} 

return entityRemover;

