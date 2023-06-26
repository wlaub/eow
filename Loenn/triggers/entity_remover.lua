
local entityRemover = {}

entityRemover.name = "eow/EntityRemover"
entityRemover.nodeLimits = {1, -1}
entityRemover.nodeLineRenderType = "fan"

entityRemover.placements = {
    {
        name = "normal",
        data = {
            flag = "",
            invert = false,
            on_load = false,
        },
    },
} 

return entityRemover;

