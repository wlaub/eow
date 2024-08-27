local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local my_entity = {}

my_entity.name = "eow/InvisibleSpinner"

my_entity.placements = {
    name = "invisible_spinner",
    data = {
        color = 'ffffffff',
        lock_color = 'ff0000ff',
        off_grid_color = 'ffffff88',
        depth = '-1000',
        locked = false,
        show_locked = true,
        show_off_grid = true,
    }
}
my_entity.fieldInformation = {
    color = {fieldType = "color", useAlpha = true},   
    lock_color = {fieldType = "color", useAlpha = true},   
    off_grid_color = {fieldType = "color", useAlpha = true},   
    depth = {fieldType = "integer"}
}

function my_entity.depth(room, entity)
    return tonumber(entity.depth or -1000)
end

function my_entity.texture(room, entity)
    return "objects/eow/InvisibleSpinner/hitbox"
end

function my_entity.color(room, entity)
    if entity.locked and entity.show_locked then
        return utils.getColor(entity.lock_color or 'ff0000ff')
    elseif entity.show_off_grid and (math.floor(entity.x/8) ~= entity.x/8  or math.floor(entity.y/8) ~= entity.y/8) then
         return utils.getColor(entity.off_grid_color or 'ffffff88')
    else
        return utils.getColor(entity.color or 'ffffffff')
    end
end

function my_entity.onMove(room, entity, nodeIndex, offsetX, offsetY)
    if entity.locked then
        return false
    end
    return true
end
function my_entity.onDelete(room, entity, nodeIndex, offsetX, offsetY)
    if entity.locked then
        return false
    end
    return true
end


return my_entity

