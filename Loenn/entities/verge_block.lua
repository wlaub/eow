-- Adapted from Loenn base plugin
-- https://github.com/CelestialCartographers/Loenn/blob/master/src/entities/dream_block.lua
--
--


local verge_block = {}

verge_block.name = "eow/VergeBlock"
verge_block.fillColor = {0.0, 0.0, 0.0}
verge_block.borderColor = {1.0, 1.0, 1.0}
verge_block.nodeLineRenderType = "fan"
verge_block.nodeLimits = {0, -1}
verge_block.placements = {
    name = "verge_block",
    alternativeName = "space_jam",
    data = {
        fastMoving = false,
        below = false,
        oneUse = false,
        width = 8,
        height = 8,
        fall_threshold = 180.1,
        texture = 'objects/eow/VergeBlock',
        fall_enter = true,
        trigger_mode = 0,
        layer_count = 3,
        vanilla_render = false,
        animate_fill = true,
        node_mode = 0,
        layer_0_color = 'ff0000',
        layer_1_color = 'ff88ff',
        layer_2_color = 'ff9900',
        layer_0_flag_color = '555555',
        layer_1_flag_color = 'eeeeee',
        layer_2_flag_color = '999999',
        layer_0_flag = '',
        layer_1_flag = '',
        layer_2_flag = '',
        trigger_ids = '',
    }
}

verge_block.fieldOrder = {
"x", "y", 
"width", "height",
"texture", "layer_count",
"node_mode", "trigger_mode",
"layer_0_color", "layer_0_flag_color",
"layer_1_color", "layer_1_flag_color",
"layer_2_color", "layer_2_flag_color",
"layer_0_flag", "layer_1_flag", 
"layer_2_flag", "fall_threshold",
"vanilla_render", "below", "oneUse", "fastMoving",
"animate_fill", "fall_enter"

}



verge_block.fieldInformation = {
    layer_count = {fieldType = "integer", minimumValue = 0, maximumValue = 3,
                    options = {{"0",0},{"1",1},{"2",2},{"3",3}}
                    },
    node_mode = {fieldType = "integer", options = {trigger = 0, move = 1}},
    trigger_mode = {fieldType = "integer", options = {fall_enter = 0, dash_enter =1}},

    layer_0_color = {fieldType = "color"},
    layer_1_color = {fieldType = "color"},
    layer_2_color = {fieldType = "color"},
    layer_0_flag_color = {fieldType = "color"},
    layer_1_flag_color = {fieldType = "color"},
    layer_2_flag_color = {fieldType = "color"},
}

function verge_block.depth(room, entity)
    return entity.below and 5000 or -11000
end

return verge_block
