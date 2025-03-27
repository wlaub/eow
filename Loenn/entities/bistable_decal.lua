local drawableSprite = require("structs.drawable_sprite")

local lookout_decal = {}

lookout_decal.name = "eow/BistableDecal"

lookout_decal.placements = {
    name = "bistable_decal",
    data = {
        control_flag = "",
        sprite = "1-forsakencity/flag",
        depth = 8999,
        scaleX = 1.0,
        scaleY = 1.0,
        rotation = 0.0,
        color = "ffffffff",
        at_least_once=true,
        global=false,
        idle_loop="idle",
        open_anim="open",
        done_loop="done",
        close_loop="close",
        editor_texture = "1-forsakencity/flag",
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
    if drawableSprite.fromTexture("decals/" .. entity.editor_texture .. "00") ~= nil then
        return "decals/" .. entity.editor_texture .. "00"
    else
        return "decals/" .. entity.editor_texture
    end
end

function lookout_decal.scale(room, entity)
    return { entity.scaleX or 1, entity.scaleY or 1 }
end
function lookout_decal.rotation(room, entity)
    return (entity.rotation or 0) * math.pi / 180
end

return lookout_decal

