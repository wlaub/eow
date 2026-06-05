local drawableSprite = require("structs.drawable_sprite")


local lookout_decal = {}

lookout_decal.name = "eow/MultipartDecal"

lookout_decal.placements = {
    name = "multipart_decal",
    data = {
        flag = "",
        sprite_prefix = "1-forsakencity/flag",
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


local file_locations = require("file_locations")
local mod_handler = require("mods")
local utils = require("utils")
local logging = require("logging")

function lookout_decal.sprite(room, entity)
    local sprite_prefix = entity.sprite_prefix
    local sprite_base, prefix = sprite_prefix:match("^(.-)/?([^/]*)$")

    local basedir = mod_handler.commonModContent .. "/Graphics/Atlases/Gameplay/decals/"..sprite_base

    logging.info('doing one !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11')
    logging.info(basedir)


    local filenames = {}
    utils.getFilenames(basedir, true, filenames, function(filename)
        logging.info(filename)
        return utils.fileExtension(filename) == "png" and filename:find(prefix, 1, true) == 1
        end)
    local sprites = {}
    for i, filename in ipairs(filenames) do
        basebase = 'decals/'..sprite_base.."/"
        local _, sprite = filename:match("^(.-)/?([^/]*)$")
        sprite = sprite:sub(1,-5)
        logging.info(filename)
        if sprite ~= '' then
            local ds = drawableSprite.fromTexture(basebase..sprite .. "00")
            if ds == nil then
                logging.info(basebase .. sprite)
                ds = drawableSprite.fromTexture(basebase .. sprite)
            end
            ds:addPosition(entity.x, entity.y)
            table.insert(sprites, ds)
        end
    end
    return sprites
end

function lookout_decal.scale(room, entity)
    return { entity.scaleX or 1, entity.scaleY or 1 }
end
function lookout_decal.rotation(room, entity)
    return (entity.rotation or 0) * math.pi / 180
end

return lookout_decal

