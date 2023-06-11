
local uniqueGlider = {}

uniqueGlider.name = "eow/UniqueJellyfish"
uniqueGlider.placements = {
    {
        name = "normal",
        data = {
            tutorial = false,
            bubble = false,
            confiscate = false
        }
    },
    {
        name = "floating",
        data = {
            tutorial = false,
            bubble = true,
            confiscate = false
        }
    }
}

uniqueGlider.depth = -5
uniqueGlider.texture = "objects/glider/idle0"

return uniqueGlider
