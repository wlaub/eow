local drawing = require("utils.drawing")
local utils = require("utils")

local entity = {}

entity.name = "eow/MusicLayerSource"
entity.placements = {
    {
        name = "music_layer_source",
        data = {
            track_player = true, 
            z_distance = 0, 
            min_distance = 16,
            max_distance = 80, 
            convert_distance = true,
            layers = "",

        }
    }
}

function entity.draw(room, entity, viewport)

    local x, y = entity.x or 0, entity.y or 0
   
    love.graphics.circle("line", x, y, entity.min_distance)
    love.graphics.circle("line", x, y, entity.max_distance)
    love.graphics.line(x-8, y, x+8, y)
    love.graphics.line(x, y-8, x, y+8)


end

function entity.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
 
    return utils.rectangle(x-8, y-8, 16, 16)

end


return entity
