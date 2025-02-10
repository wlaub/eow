local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local rectangle = require("structs.rectangle")

local my_entity = {}

my_entity.name = "eow/PoppingMirror"
my_entity.minimumSize = {8, 8}

my_entity.placements = {
    name = "popping_mirror",
    data = {
        width = 16,
        height = 16,
        depth = 9999,
        rate = 4, --tiles per frame
        sprite_directory = "objects/waldmo/popping_mirror/base/",
        control_flag = "",
        on_contact_flag = "",
        on_contact = false,
        at_least_once = true,
    }
}

my_entity.fieldOrder = {
"x", "y", "width", "height",
"spriteDirectory", "depth", "rate",
"control_flag", "on_contact_flag",
}

function my_entity.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8

    local nw = math.floor(width/8)
    local nh = math.floor(height/8)

    local rotate = false
    if nw > nh then
        nw, nh = nh, nw
        rotate = true
    end

    local texture = entity.sprite_directory..tostring(nw)..'.'..tostring(nh)..'.00.'


    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite:addPosition(width/2, height/2)

    if rotate then
        sprite.rotation = 3.14/2
    end

    return sprite

end

return my_entity



