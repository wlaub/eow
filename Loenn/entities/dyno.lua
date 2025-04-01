local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local rectangle = require("structs.rectangle")
local drawing = require("utils.drawing")



local dyno = {}

dyno.name = "eow/Dyno"
dyno.placements = {
    {
        name = "dyno",
        data = {
        single_use = false,
        radius = 8,
        idle_sprite = "dyno",
        active_sprite = "",
        used_sprite = "",
        yboost_threshold = 80,
        yboost = 180,
        yboost_dash = 120,
        xboost = 80,
        xboost_dash = 100,
        xboost_diag = 180,
        holdoff_duration = 0.75,
        }
    }
}

dyno.fieldOrder = {
"x", "y",
"idle_sprite", "active_sprite", 
"used_sprite","radius",
"xboost", "yboost",
"xboost_dash", "yboost_dash",
"xboost_diag", "yboost_threshold"
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"

function dyno.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local r = entity.radius or 8

    local sprite = drawableSprite.fromTexture('objects/booster/booster00', entity)

    local sprites = {}
    table.insert(sprites, sprite)

    if entity.single_use then
        local shrink = entity.shrink or 0
        local hitbox = drawableRectangle.fromRectangle("line", x-r, y-r, r*2, r*2, {0,0,0, 0.75})

        table.insert(sprites, hitbox)
    end

    return sprites

end



return dyno
