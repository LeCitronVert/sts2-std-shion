extends Node2D

## A fiery aura that flares up around a creature for a few seconds, then peters out.
## Energy Bind (purple, on the enemy), Energy Burst (purple, on Shion), Berserk Flow Charge (orange, on Shion).
## Reuses Shion's aura spritesheets (see shion_auras.gd). Spawned from C# (ShionVfx.PlayFlameBurst)
## in the creature's visuals, then play() is called. Frees itself when done.

const AURA_PATH := "res://ShionTheBruiser/images/charui/shion/aura/%s.png"
const FRAME_SIZE := Vector2(355.0, 450.0)
const FRAME_CELLS: Array[Vector2] = [Vector2(0, 0), Vector2(1, 0), Vector2(0, 1), Vector2(1, 1)]
const FRAME_BOTTOM := 203.0
## Height of the flames on the spritesheet, used to size the aura to the creature
const FLAME_HEIGHT := 380.0

@export var fps := 14.0
@export var fade_in := 0.15
@export var hold := 1.6
@export var peter_out := 0.9
## Aura height relative to the creature's height
@export var height_ratio := 1.25

var _sprite: AnimatedSprite2D
var _base_scale := 1.0


## sheet: aura spritesheet name, feet: the creature's feet (local), height: the creature's height
func play(sheet: String, feet: Vector2, height: float) -> void:
	var texture: Texture2D = load(AURA_PATH % sheet)
	var frames := SpriteFrames.new()
	frames.set_animation_speed(&"default", fps)
	frames.set_animation_loop(&"default", true)
	for cell in FRAME_CELLS:
		var atlas := AtlasTexture.new()
		atlas.atlas = texture
		atlas.region = Rect2(cell * FRAME_SIZE, FRAME_SIZE)
		frames.add_frame(&"default", atlas)

	_sprite = AnimatedSprite2D.new()
	_sprite.sprite_frames = frames
	_sprite.light_mask = 0
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	_sprite.material = additive
	_sprite.offset = Vector2(0.0, -FRAME_BOTTOM)
	_sprite.play(&"default")
	_sprite.frame = randi() % FRAME_CELLS.size()
	add_child(_sprite)

	position = feet
	_base_scale = maxf(height, 100.0) * height_ratio / FLAME_HEIGHT
	_sprite.scale = Vector2(_base_scale * 0.8, _base_scale * 0.5)
	modulate.a = 0.0

	var tween := create_tween()
	tween.set_parallel(true)
	tween.tween_property(self, "modulate:a", 1.0, fade_in)
	tween.tween_property(_sprite, "scale", Vector2.ONE * _base_scale, fade_in * 2.0).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.set_parallel(false)
	tween.tween_interval(hold)
	# Peters out: the flames sink, thin and flicker away
	tween.set_parallel(true)
	tween.tween_property(_sprite, "scale", Vector2(_base_scale * 0.7, _base_scale * 0.15), peter_out).set_ease(Tween.EASE_IN)
	tween.tween_method(_flicker_out, 1.0, 0.0, peter_out)
	tween.set_parallel(false)
	tween.tween_callback(queue_free)


func _flicker_out(value: float) -> void:
	modulate.a = value * randf_range(0.6, 1.0)
