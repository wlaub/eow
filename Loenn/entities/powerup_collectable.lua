local drawableSprite = require("structs.drawable_sprite")

local powerup_collectable = {}

powerup_collectable.name = "eow/PowerupCollectable"

powerup_collectable.nodeLimits = {0, -1}
powerup_collectable.nodeLineRenderType = "fan"

powerup_collectable.fieldOrder = {
"x", "y", 
"sprite", "flag",
"collect_sound", "collect_jingle",
"poem_dialog", "jingle_duration",
"strength", "music_volume",
"shatter_color", "heart_index",
"bob_height", "bob_period",
"show_animation", "show_poem",
"do_pulse", "do_wiggle", "must_dash_toward", "center_player",
"do_refill", "do_unfill", 
}

powerup_collectable.placements = {
    name = "normal",
    data = {
        flag = "",
        sprite = "",
        poem_dialog = "",
        collect_sound = "event:/game/07_summit/gem_get",
        collect_jingle = "",
        shatter_color = "ffffff",
        do_pulse = true,
        do_wiggle = true,
        do_refill = true,
        do_unfill = false,
        must_dash_toward = false,
        center_player = false,
        show_animation = true,
        show_poem = true,
        heart_index = 3,
        strength = 1,
        jingle_duration = 3,
        bob_height = 0,
        bob_period = 3,
        music_volume = -1,
    }
}

powerup_collectable.fieldInformation = {
    shatter_color = {
        fieldType = "color"
        },
    heart_index = {
        fieldType = "integer",
        editable = false,
        options = {
            blue = 0,
            red = 1,
            yellow = 2,
            white = 3,
            }
        }
}

function powerup_collectable.texture(room, entity)
    if drawableSprite.fromTexture(entity.sprite .. "00") ~= nil then
        return entity.sprite .. "00"
    else
        return entity.sprite
    end
end

return powerup_collectable

