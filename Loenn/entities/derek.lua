local drawableSprite = require("structs.drawable_sprite")

local my_entity = {}

my_entity.name = "eow/Derek"

my_entity.placements = {
    name = "derek",
    data = {
        initial_speed = -1,
        max_speed = 7,
        min_speed = 0.1,
        sprite = "objects/eow/DiamondRider/derek",
    }
}

function my_entity.texture(room, entity)
    if drawableSprite.fromTexture( entity.sprite .. "00") ~= nil then
        return entity.sprite .. "00"
    else
        return entity.sprite
    end
end

return my_entity

