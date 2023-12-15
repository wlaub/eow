local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local textures = {
    wood = "objects/door/lockdoor00",
    temple_a = "objects/door/lockdoorTempleA00",
    temple_b = "objects/door/lockdoorTempleB00",
    moon = "objects/door/moonDoor11"
}
local textureOptions = {}

for name, _ in pairs(textures) do
    textureOptions[utils.humanizeVariableName(name)] = name
end

local entity = {}

entity.justification = {0.25, 0.25}
entity.fieldInformation = {
    sprite = {
        options = textureOptions,
        editable = false
    }
}

entity.name = "eow/MirrorBlock"

entity.placements = {
    name = "mirror_block",
    data = {
        sprite = "wood",
        unlock_sfx = "",
        stepMusicProgress = false,
        depth = 9400,
        mirrormask = "",
    }
}


function entity.sprite(room, entity)
    local spriteName = entity.sprite or "wood"
    local texture = textures[spriteName] or textures["wood"]
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite:addPosition(16, 16)

    return sprite
end


return entity

