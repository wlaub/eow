local liftboost_block = {}

liftboost_block.name = "eow/JumpOff"

liftboost_block.fillColor = {0,0,0,0}
liftboost_block.borderColor = {1,0.4,0.2,1}

liftboost_block.canResize = {true, false}

liftboost_block.placements = {
    name = "jump_off",
    data = {
        width = 8,
        height = 3,
    }
}

return liftboost_block
