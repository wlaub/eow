
local my_entity = {}

my_entity.name = "eow/RespawnTargetIfGravity"
my_entity.nodeLimits = {1, 1}
my_entity.nodeLineRenderType = "fan"

my_entity.placements = {
    {
        name = "gravity_spawn_norm",
        data = {
            flag = "",
            gravity_inverted = false,
        },
    },
    {
        name = "gravity_spawn_inv",
        data = {
            flag = "",
            gravity_inverted = true,
        },
    },
} 

--dirty hack to render nodes differently
function my_entity.category(trigger)
    if trigger.gravity_inverted then
        return "audio"
    else
        return "default"
    end
end

function my_entity.nodeColor(room, entity, node, nodeIndex)
    if entity.gravity_inverted then
        return {1,0,0}
    else
        return {0,0,1}
    end
end

return my_entity;

