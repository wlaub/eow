local liftboost_block = {}

liftboost_block.name = "eow/JumpOff"

liftboost_block.fillColor = {1,0.4,0.2,0}
liftboost_block.borderColor = {0,0,0,1}

liftboost_block.canResize = {true, false}

liftboost_block.placements = {
    name = "jump_off",
    data = {
        width = 8,
        height = 1,
    }
}

return liftboost_block
