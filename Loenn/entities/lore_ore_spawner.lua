
local uniqueGlider = {}

uniqueGlider.name = "eow/LoreOreSpawner"
uniqueGlider.placements = {
    {
        name = "lore_ore_spawner",
        data = {
            interval = 7,
            start_offset = 0,
            max_lores= 7,
            lore_health = 7,
            lore_options = "",
        }
    }
}

uniqueGlider.depth = -5
uniqueGlider.texture = "objects/glider/idle0"

return uniqueGlider
