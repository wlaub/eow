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
            min_distance = 40,
            max_distance = 240, 
            convert_distance = true,
            layers = "",
            variant_color = "ffffff88",
            type_color = "ffffffff",
            variant = 0,
        }
    }
}
entity.fieldInformation = {
    variant_color = {fieldType = "color", useAlpha = true},   
    type_color = {fieldType = "color", useAlpha = true},   
}


entity.depth=10000

function entity.draw(room, entity, viewport)

    local x, y = entity.x or 0, entity.y or 0

--   local c = utils.getColor(entity.variant_color or "00000000")
--    local r,g,b = c[1], c[2], c[3]

    local r,g,b = utils.hsvToRgb((entity.variant or 0), 1, 1)
    local w = entity.max_distance-entity.min_distance

    love.graphics.setColor(r,g,b,0.03)
    love.graphics.circle("fill", x, y, entity.max_distance)
    love.graphics.circle("fill", x, y, entity.min_distance + w*.67)
    love.graphics.circle("fill", x, y, entity.min_distance + w*.33)


    local c = utils.getColor(entity.type_color or "00000000")
    local r,g,b,a = c[1], c[2], c[3], c[4]
    love.graphics.setColor(r,g,b,a)
    love.graphics.circle("fill", x, y, entity.min_distance)
  
    love.graphics.setColor(1,1,1) 
    love.graphics.circle("line", x, y, entity.min_distance)
    love.graphics.circle("line", x, y, entity.max_distance)

    love.graphics.line(x-8, y, x+8, y)
    love.graphics.line(x, y-8, x, y+8)


end

function entity.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local w = entity.min_distance or 8
 
    return utils.rectangle(x-w, y-w, w*2,w*2)

end


return entity
