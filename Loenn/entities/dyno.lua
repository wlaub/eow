
local dyno = {}

dyno.name = "eow/Dyno"
dyno.placements = {
    {
        name = "dyno",
        data = {
        single_use = false,
        radius = 8,
        idle_sprite = "dyno",
        active_sprite = "",
        used_sprite = "",
        yboost_threshold = 80,
        yboost = 180,
        yboost_dash = 120,
        xboost = 80,
        xboost_dash = 100,
        xboost_diag = 180,
        }
    }
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"

return dyno
