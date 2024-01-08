-- Adapted from Loenn base plugin
-- https://github.com/CelestialCartographers/Loenn/blob/master/src/entities/dash_block.lua
--
--

local fakeTilesHelper = require("helpers.fake_tiles")

local dashBlock = {}

dashBlock.name = "eow/FakeWallDashBlock"
dashBlock.depth = 0

function dashBlock.placements()
    return {
        name = "eow_dash_block",
        data = {
            tiletype = "3",
            blendin = true,
            canDash = true,
            permanent = true,
            width = 8,
            height = 8
        }
    }
end

dashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
dashBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return dashBlock
