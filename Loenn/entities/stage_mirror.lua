local liftboost_block = {}

liftboost_block.name = "eow/StageMirror"

liftboost_block.fillColor = {0,0,0,0}
liftboost_block.borderColor = {1,1,1,1}

liftboost_block.placements = {
    name = "stage_mirror",
    data = {
        width = 8,
        height = 8,
    }
}

function liftboost_block.draw(room, entity, viewport)

    local x, y = entity.x or 0, entity.y or 0

    love.graphics.setColor(1,1,1) 
    love.graphics.line(x-0.5, y, x-0.5, y+entity.height)
    love.graphics.line(x+0.5, y, x+0.5, y+entity.height)



end



return liftboost_block
