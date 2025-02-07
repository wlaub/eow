local drawableSprite = require("structs.drawable_sprite")

local lookout_decal = {}

lookout_decal.name = "eow/GlobalDecal"

lookout_decal.placements = {
    name = "global_decal",
    data = {
        flag = "",
        sprite = "1-forsakencity/flag",
        depth = 8999,
        scaleX = 1.0,
        scaleY = 1.0,
        rotation = 0.0,
        color = "ffffffff",
    }
}

lookout_decal.fieldOrder = {
"x", "y",
"scaleX", "scaleY",
"sprite", "depth",
"rotation", "color",
"flag",
}

function lookout_decal.depth(room, entity)
    return tonumber(entity.depth or -1000)
end


function lookout_decal.texture(room, entity)
    if drawableSprite.fromTexture("decals/" .. entity.sprite .. "00") ~= nil then
        return "decals/" .. entity.sprite .. "00"
    else
        return "decals/" .. entity.sprite
    end
end

function lookout_decal.scale(room, entity)
    return { entity.scaleX or 1, entity.scaleY or 1 }
end
function lookout_decal.rotation(room, entity)
    return (entity.rotation or 0) * math.pi / 180
end

return lookout_decal

