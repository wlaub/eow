
local celeste = require('#celeste')

function onBegin()
    celeste.Mod.ErrandOfWednesday.EstateController.undraft_room(nil, arguments)
end


function onEnd(room, wasSkipped)
end

