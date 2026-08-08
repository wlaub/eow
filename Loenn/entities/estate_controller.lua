
local dyno = {}

dyno.name = "eow/EstateController"
dyno.placements = {
    {
        name = "estate_controller",
        data = {
            room_width = 46,
            room_height = 36,
            grid_width = 9,
            grid_height = 5,
        }
    }
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"
dyno.nodeLimits = {1, 1}

local rectangle = require("structs.rectangle")
dyno.nodeLineRenderType = "fan"
dyno.nodeColor = {0, .5, .5, .5}
function dyno.nodeRectangle(room, entity, node)
--    local w = entity.width or 16
--    local h = entity.height or 16

--    local cx = w/2
--    local cy = h/2

    local result = rectangle.create(node.x-4, node.y-4, 8,8)    
    return result
end



return dyno
