local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local rectangle = require("structs.rectangle")

local liftboost_block = {}

liftboost_block.name = "eow/LiftboostBlock"
liftboost_block.nodeLimits = {1, 1}
liftboost_block.minimumSize = {16, 16}

liftboost_block.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,
        spriteDirectory = "objects/eow/LiftboostBlock/",
        normalize = true,
        instant = true,
    }
}

liftboost_block.nodeLineRenderType = "fan"

local block_depth = -9999

local frameNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local trailNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    useRealSize = true
}

local pathNinePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}


local function add_block_sprite(sprites, entity, frameTexture)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8

    local frameNinePatch = drawableNinePatch.fromTexture(frameTexture, frameNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    for _, sprite in ipairs(frameSprites) do
        sprite.depth = block_depth

        table.insert(sprites, sprite)
    end

end

function add_arrow_sprite(sprites, entity, arrow_texture)

    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local angle = math.atan2(nodes[1].y-y, nodes[1].x-x)
    local width, height = entity.width, entity.height
    local arrow_sprite = drawableSprite.fromTexture(arrow_texture, entity)
    arrow_sprite:addPosition(math.floor(width/2), math.floor(height/2))
    arrow_sprite.depth = block_depth
    arrow_sprite.rotation=angle
    table.insert(sprites, arrow_sprite)

end

function liftboost_block.sprite(room, entity)
    local sprites = {}

    add_block_sprite(sprites, entity, entity.spriteDirectory .. "block")
    add_arrow_sprite(sprites, entity, entity.spriteDirectory .. "arrow_idle")

    return sprites
end

liftboost_block.nodeColor = {0, .5, .5, .5}
function liftboost_block.nodeRectangle(room, entity, node)
    local w = entity.width or 16
    local h = entity.height or 16

    local cx = w/2
    local cy = h/2

    local result = rectangle.create(cx+node.x-4, cy+node.y-4, 8,8)    
    return result
end

return liftboost_block



