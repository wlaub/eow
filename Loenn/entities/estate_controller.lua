
local dyno = {}

dyno.name = "eow/EstateController"
dyno.placements = {
    {
        name = "estate_controller",
        data = {
            room_width = 46,
            room_height = 36,
            grid_width = 9,
            grid_height = 5,
        }
    }
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"

return dyno
