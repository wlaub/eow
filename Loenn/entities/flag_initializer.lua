local drawable_text = require("structs.drawable_text")

local dyno = {}

dyno.name = "eow/FlagInitializer"
dyno.placements = {
    {
        name = "flag_initializer",
        data = {
            width = 64,
            height = 16,
            flag1 = "",
            flag2 = "",
            flag3 = "",
            flag4 = "",
            flag5 = "",
            flag6 = "",
        }
    }
}

dyno.depth = -5

function dyno.sprite(room, entity)

    local text = 'Flags:\n' .. entity.flag1 .. '\n' .. entity.flag2 .. '\n' .. entity.flag3 .. '\n' .. entity.flag4 .. '\n' .. entity.flag5 .. '\n' .. entity.flag6

    return drawable_text.fromText(text, entity.x, entity.y, entity.width, entity.height, nil, 1, {1,1,1})
end

return dyno
