local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local rectangle = require("structs.rectangle")

local liftboost_block = {}

liftboost_block.name = "eow/RefillBubbler"
liftboost_block.nodeLimits = {2, 2}
liftboost_block.nodeLineRenderType = "line"


liftboost_block.placements = {
    name = "refill_bubbler",
    data = {
        width = 16,
        height = 16,
        enable_flag = "",
        use_flag = "",
        only_once = false,
    }
}

function liftboost_block.draw(room, entity, viewport)

    local x, y = entity.x or 0, entity.y or 0
   
    love.graphics.circle("line", x+entity.width/2, y+entity.height/2, 12)
    love.graphics.rectangle("line", x, y, entity.width, entity.height)


end

function liftboost_block.nodeRectangle(room, entity, node)
    local w = entity.width or 16
    local h = entity.height or 16

    local cx = w/2
    local cy = h/2

    local result = rectangle.create(cx+node.x-4, cy+node.y-4, 8,8)    
    return result
end



return liftboost_block



