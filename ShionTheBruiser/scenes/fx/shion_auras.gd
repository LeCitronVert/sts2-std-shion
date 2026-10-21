extends Node2D

## Stackable auras displayed behind Shion, each casting a light of its colour on her.
## Driven from C# (CreatureFxController.cs) through set_auras().
## This node is placed at Shion's feet, horizontally centered on her.
## Auras are ordered by application: the first one is in front and the smallest,
## each later one is slightly bigger and behind the previous ones.

const AURA_PATH := "res://ShionTheBruiser/images/charui/shion/aura/%s.png"

## Spritesheet layout: 4 flame frames in a 2x2 grid in the top-left of each sheet
const FRAME_SIZE := Vector2(355.0, 450.0)
const FRAME_CELLS: Array[Vector2] = [Vector2(0, 0), Vector2(1, 0), Vector2(0, 1), Vector2(1, 1)]
## Distance from a frame's center to the bottom of its flames, in sheet pixels
const FRAME_BOTTOM := 203.0

## Light colour of each aura (the average colour of its spritesheet)
const AURA_COLORS := {
	"crit_above_50_or_vigor_above_20": Color8(251, 171, 11),
	"dark_arts": Color8(12, 225, 166),
	"fell_dragon": Color8(173, 10, 239),
	"spirit_break": Color8(16, 152, 241),
	"spirit_burst": Color8(253, 82, 8),
}

## Light layer used only by Shion's body, so the aura lights don't light enemies or the background.
## Must match CreatureFxController.AuraLightLayer.
const LIGHT_LAYER := 1 << 19

@export var base_scale := 1.15
## Extra scale for each aura stacked behind another one
@export var scale_step := 0.15
## Vertical offset of the flames' bottom from Shion's feet
@export var feet_offset := 14.0
@export var fps := 12.0
@export var fade_duration := 0.25
@export var light_energy := 0.7
## Each light behind another one is this much weaker, so stacked auras don't wash Shion out
@export var light_falloff := 0.65
## Height of Shion's body center above her feet, where the lights are placed
@export var light_height := 170.0
@export var light_scale := 2.2

var _frames_cache := {}
var _auras := {}  # name -> Node2D
var _order: Array[String] = []
var _light_texture: GradientTexture2D


func _ready() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1, 1, 1, 1))
	gradient.set_color(1, Color(1, 1, 1, 0))
	_light_texture = GradientTexture2D.new()
	_light_texture.gradient = gradient
	_light_texture.fill = GradientTexture2D.FILL_RADIAL
	_light_texture.fill_from = Vector2(0.5, 0.5)
	_light_texture.fill_to = Vector2(1.0, 0.5)
	_light_texture.width = 256
	_light_texture.height = 256


## names: active aura names, in order of application
func set_auras(names: PackedStringArray) -> void:
	var wanted: Array[String] = []
	for aura_name in names:
		if AURA_COLORS.has(aura_name):
			wanted.append(aura_name)

	for aura_name in _order.duplicate():
		if not wanted.has(aura_name):
			_remove_aura(aura_name)
	for aura_name in wanted:
		if not _auras.has(aura_name):
			_add_aura(aura_name)

	_order = wanted
	_layout()


func _add_aura(aura_name: String) -> void:
	var aura := Node2D.new()
	aura.name = aura_name

	var sprite := AnimatedSprite2D.new()
	sprite.name = "Sprite"
	sprite.sprite_frames = _get_frames(aura_name)
	sprite.light_mask = 0
	sprite.play(&"default")
	sprite.frame = randi() % FRAME_CELLS.size()
	aura.add_child(sprite)

	var light := PointLight2D.new()
	light.name = "Light"
	light.texture = _light_texture
	light.color = AURA_COLORS[aura_name]
	light.blend_mode = Light2D.BLEND_MODE_ADD
	light.range_item_cull_mask = LIGHT_LAYER
	light.texture_scale = light_scale
	light.position = Vector2(0.0, -light_height)
	aura.add_child(light)

	aura.modulate.a = 0.0
	add_child(aura)
	aura.create_tween().tween_property(aura, "modulate:a", 1.0, fade_duration)
	_auras[aura_name] = aura


func _remove_aura(aura_name: String) -> void:
	var aura: Node2D = _auras[aura_name]
	_auras.erase(aura_name)
	var light := aura.get_node("Light") as PointLight2D
	var tween := aura.create_tween().set_parallel(true)
	tween.tween_property(aura, "modulate:a", 0.0, fade_duration)
	tween.tween_property(light, "energy", 0.0, fade_duration)
	tween.chain().tween_callback(aura.queue_free)


func _layout() -> void:
	# Later auras are drawn first (behind), so walk the order backwards
	for i in range(_order.size() - 1, -1, -1):
		var aura: Node2D = _auras[_order[i]]
		move_child(aura, _order.size() - 1 - i)
		var aura_scale := base_scale * (1.0 + scale_step * i)
		var sprite := aura.get_node("Sprite") as AnimatedSprite2D
		var light := aura.get_node("Light") as PointLight2D
		var target_position := Vector2(0.0, feet_offset - FRAME_BOTTOM * aura_scale)
		if aura.has_meta("laid_out"):
			# Existing auras grow/shrink smoothly when one is added or removed in front of them
			sprite.create_tween().tween_property(sprite, "scale", Vector2.ONE * aura_scale, fade_duration)
			sprite.create_tween().tween_property(sprite, "position", target_position, fade_duration)
		else:
			sprite.scale = Vector2.ONE * aura_scale
			sprite.position = target_position
			aura.set_meta("laid_out", true)
		light.energy = light_energy * pow(light_falloff, i)


func _get_frames(aura_name: String) -> SpriteFrames:
	if _frames_cache.has(aura_name):
		return _frames_cache[aura_name]
	var sheet: Texture2D = load(AURA_PATH % aura_name)
	var frames := SpriteFrames.new()
	frames.set_animation_speed(&"default", fps)
	frames.set_animation_loop(&"default", true)
	for cell in FRAME_CELLS:
		var atlas := AtlasTexture.new()
		atlas.atlas = sheet
		atlas.region = Rect2(cell * FRAME_SIZE, FRAME_SIZE)
		frames.add_frame(&"default", atlas)
	_frames_cache[aura_name] = frames
	return frames
