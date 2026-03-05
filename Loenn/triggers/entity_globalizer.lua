
local entityRemover = {}

entityRemover.name = "eow/EntityGlobalizer"
entityRemover.nodeLimits = {1, -1}
entityRemover.nodeLineRenderType = "fan"

entityRemover.placements = {
    {
        name = "entity_globalizer",
        data = {
            flag = "",
            type_filter = "",
            on_load = false,
            remove_player = false,
        },
    },
} 

return entityRemover;
