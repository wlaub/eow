local drawableSprite = require("structs.drawable_sprite")


local lookout_decal = {}

lookout_decal.name = "eow/MultipartDecal"

lookout_decal.placements = {
    name = "multipart_decal",
    data = {
        flag = "",
        sprite = "1-forsakencity/flag",
        xmlfile = "",
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

local file_locations = require("file_locations")
local mod_handler = require("mods")
local utils = require("utils")
local logging = require("logging")

local xmlHandler = require("lib.xml2lua.xmlhandler.tree")
local xml2lua = require("lib.xml2lua.xml2lua")

function lookout_decal.sprite(room, entity)
    local xmlfile = mod_handler.commonModContent .. "/Graphics/" .. entity.xmlfile 

    local handler = xmlHandler:new()
    local parser = xml2lua.parser(handler)
    local xmlString = utils.readAll(xmlfile, "rb")

    if not xmlString then
        logging.error("can't read it")
    end

    local xml = utils.stripByteOrderMark(xmlString)
    parser:parse(xml)

    local sprites = {}

    element = handler.root.Stuff[entity.sprite]
        base_path = element._attr.path
        for _, part in pairs(element.part) do
            texture = base_path .. part._attr.path
            local ds = drawableSprite.fromTexture(texture, entity)
            if ds == nil then
                ds = drawableSprite.fromTexture(texture .. '00', entity)
            end
            if ds ~= nil then
                xoff = part._attr.x or 0
                yoff = part._attr.y or 0
                ds:addPosition(xoff, yoff)
                ds.depth = entity.depth + (part._attr.depth or 0)
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

