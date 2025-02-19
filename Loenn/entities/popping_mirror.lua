local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local rectangle = require("structs.rectangle")
local drawing = require("utils.drawing")

local my_entity = {}

my_entity.name = "eow/PoppingMirror"
my_entity.minimumSize = {8, 8}
my_entity.maximumSize = {40, 40}
my_entity.nodeLimits = {0, 1}

my_entity.placements = {
    name = "popping_mirror",
    data = {
        width = 16,
        height = 16,
        depth = 9999,
        rate = 4, --tiles per frame
        sprite_directory = "objects/eow/popping_mirror/default/",
        frame_delay = 0.2,
        break_frame_delay = 0.2,
        shrink = 4,
        trigger_sound = "",
        shatter_sound = "",
        control_flag = "",
        on_contact_flag = "",
        enable_flag = "",
        on_contact = false,
        at_least_once = true,
        only_this = true,
        only_on_contact = true,
        change_spawn_normal = false,
        cull_edges = false,
        shatter_group = "",
    }
}

my_entity.fieldOrder = {
"x", "y", "width", "height",
"rate", "shrink",
"sprite_directory", "depth", 
"frame_delay", "break_frame_delay",
"trigger_sound", "shatter_sound",
"control_flag", "on_contact_flag",
"shatter_group", "enable_flag" 
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
    if sprite == nil then
        sprite = drawableSprite.fromTexture(texture .. '00', entity)
    end

    sprite:addPosition(width/2, height/2)

    if rotate then
        sprite.rotation = 3.14/2
    end

    local sprites = {}
    table.insert(sprites, sprite)

    if entity.on_contact then
        local shrink = entity.shrink or 0
        local hitbox = drawableRectangle.fromRectangle("line", x+shrink, y+shrink, width-shrink*2, height-shrink*2, {1,1,1, 0.25})

        table.insert(sprites, hitbox)
    else
        local hitbox = drawableRectangle.fromRectangle("line", x, y, width, height, {1,1,1, 0.125})

        table.insert(sprites, hitbox)
    end

    return sprites

end

my_entity.nodeLineRenderType = "fan"

my_entity.nodeColor = {0, .5, .5, .5}
function my_entity.nodeRectangle(room, entity, node)
    local w = entity.width or 16
    local h = entity.height or 16

    local cx = w/2
    local cy = h/2

    local result = rectangle.create(cx+node.x-2, cy+node.y-2, 4,4) 
    return result
end


return my_entity



