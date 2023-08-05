local drawableSprite = require("structs.drawable_sprite")

local powerup_collectable = {}

powerup_collectable.name = "eow/PowerupCollectable"

powerup_collectable.nodeLimits = {0, -1}
powerup_collectable.nodeLineRenderType = "fan"

powerup_collectable.placements = {
    name = "normal",
    data = {
        flag = "",
        sprite = "",
        poem_dialog = "",
        shatter_color = "ffffff",
        do_pulse = true,
        must_dash_toward = false,
        center_player = false,
        show_animation = true,
        show_poem = true,
        strength = 1,
    }
}

powerup_collectable.fieldInformation = {
    shatter_color = {
        fieldType = "color"
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

