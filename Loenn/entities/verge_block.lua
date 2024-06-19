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
    }
}

function verge_block.depth(room, entity)
    return entity.below and 5000 or -11000
end

return verge_block
