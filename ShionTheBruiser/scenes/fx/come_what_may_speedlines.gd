extends Node2D

## Full screen "running" speedlines behind Shion during Come What May's cast animation (an Umamusume's run).
## Spawned from C# (ShionVfx.cs), which places this node behind Shion and calls play().
## The lines burst from the bottom center of the screen and cover all of it. To feel like running:
## - the two frames swap at random, each one randomly mirrored (4 variants, never the same twice in a row),
## - each frame zooms outward a little while it's shown, so the lines rush toward the edges,
## - a shader (come_what_may_speedlines.gdshader) cuts the lines into dashes racing outward.
## Then it fades out and frees itself.

const FRAMES: Array[String] = [
	"res://ShionTheBruiser/images/charui/shion/come_what_may/come_what_may_1.png",
	"res://ShionTheBruiser/images/charui/shion/come_what_may/come_what_may_2.png",
]
const SHADER_PATH := "res://ShionTheBruiser/scenes/fx/come_what_may_speedlines.gdshader"
## Point the lines radiate from, on the frames (bottom center)
const ORIGIN := Vector2(426.0, 470.0)
const FRAME_SIZE := Vector2(852.0, 561.0)

@export var fps := 12.0
## How much a frame zooms outward while it's shown
@export var frame_zoom := 0.08
@export var fade_in := 0.08
@export var hold := 0.6
@export var fade_out := 0.25

var _textures: Array[Texture2D] = []
var _sprite: Sprite2D
var _shader: ShaderMaterial
var _variant := -1
var _frame_time := 0.0


func play() -> void:
	for path in FRAMES:
		_textures.append(load(path))

	_shader = ShaderMaterial.new()
	_shader.shader = load(SHADER_PATH)

	_sprite = Sprite2D.new()
	_sprite.centered = false
	_sprite.offset = -ORIGIN
	_sprite.light_mask = 0
	_sprite.material = _shader
	add_child(_sprite)
	_next_frame()
	_place()

	modulate.a = 0.0
	var tween := create_tween()
	tween.tween_property(self, "modulate:a", 1.0, fade_in)
	tween.tween_interval(hold)
	tween.tween_property(self, "modulate:a", 0.0, fade_out)
	tween.tween_callback(queue_free)


func _process(delta: float) -> void:
	if _sprite == null:
		return
	_frame_time += delta
	if _frame_time >= 1.0 / fps:
		_frame_time = 0.0
		_next_frame()
	_place()


## Swaps to a random other variant (frame + mirroring)
func _next_frame() -> void:
	var variant := randi() % (_textures.size() * 2)
	if variant == _variant:
		variant = (variant + 1) % (_textures.size() * 2)
	_variant = variant
	_sprite.texture = _textures[variant >> 1]
	# The origin is horizontally centered, so mirroring keeps it in place
	_sprite.flip_h = variant % 2 == 1
	_shader.set_shader_parameter("seed", randf() * 100.0)


## Covers the whole screen, the lines' origin at its bottom center
func _place() -> void:
	var screen := get_canvas_transform().affine_inverse() * get_viewport_rect()
	var cover: float = max(screen.size.x / FRAME_SIZE.x, screen.size.y / ORIGIN.y)
	var zoom := 1.0 + frame_zoom * _frame_time * fps
	_sprite.global_rotation = 0.0
	_sprite.global_scale = Vector2.ONE * cover * zoom
	_sprite.global_position = Vector2(screen.get_center().x, screen.end.y)
