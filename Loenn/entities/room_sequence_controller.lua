local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local rectangle = require("structs.rectangle")
local drawing = require("utils.drawing")



local dyno = {}

dyno.name = "eow/RoomSequenceController"
dyno.placements = {
    {
        name = "room_sequence_controller",
        data = {
        sequence_name = "my_sequence",
        sequence_number = 0,
        flag_on_transition = "",
        }
    }
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
