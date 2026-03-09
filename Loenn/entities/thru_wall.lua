local liftboost_block = {}

liftboost_block.name = "eow/ThruWall"

liftboost_block.fillColor = {1,0.4,0.2,0}
liftboost_block.borderColor = {1,0.4,0.2,1}

liftboost_block.canResize = {false, true}

liftboost_block.placements = {
    name = "thru_wall",
    data = {
        width = 1,
        height = 8,
    }
}

return liftboost_block
