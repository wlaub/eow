
local entityRemover = {}

entityRemover.name = "eow/EntityRemover"
entityRemover.nodeLimits = {1, -1}
entityRemover.nodeLineRenderType = "fan"

entityRemover.placements = {
    {
        name = "normal",
        data = {
            flag = "",
            type_filter = "",
            invert = false,
            on_load = false,
            remove_player = false,
        },
    },
} 

return entityRemover;

