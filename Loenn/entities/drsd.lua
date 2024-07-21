local drawableSprite = require("structs.drawable_sprite")

local my_entity = {}

my_entity.name = "eow/DiamondRiderScoreDisplay"

my_entity.placements = {
    name = "drsd",
    data = {
        size = 16,
        score_to_show = 0,
    }
}

return my_entity

