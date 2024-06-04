
local dyno = {}

dyno.name = "eow/Dyno"
dyno.placements = {
    {
        name = "dyno",
        data = {

        yboost_threshold = 40,
        yboost = 180,
        yboost_dash = 80,
        xboost = 60,
        xboost_dash = 100,
        xboost_diag = 160,
        }
    }
}

dyno.depth = -5
dyno.texture = "objects/booster/booster00"

return dyno
