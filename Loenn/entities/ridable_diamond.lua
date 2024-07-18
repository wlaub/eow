local drawableSprite = require("structs.drawable_sprite")

local my_entity = {}

my_entity.name = "eow/RideableDiamond"

my_entity.placements = {
    name = "ridable_diamond",
    data = {
        radius = 32,
        speed = 3.31,
        sprite = "objects/eow/DiamondRider/diamond",
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

