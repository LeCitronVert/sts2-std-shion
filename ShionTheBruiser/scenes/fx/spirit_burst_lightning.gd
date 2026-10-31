extends Node2D

## Lightning crossing the whole screen horizontally behind Shion while Spirit Burst autoplays her cards.
## Driven from C# (CreatureFxController.cs) through set_active().
## This node is placed behind Shion, at her body's center; the lightning follows her height
## and is stretched to the screen's width every frame.
## Stopping lingers a little, so autoplays chaining into each other keep the same lightning.

const FRAMES: Array[String] = [
	"res://ShionTheBruiser/images/charui/shion/spirit_burst/spirit_burst_1.png",
	"res://ShionTheBruiser/images/charui/shion/spirit_burst/spirit_burst_2.png",
	"res://ShionTheBruiser/images/charui/shion/spirit_burst/spirit_burst_3.png",
]
## Horizontal extent of the lightning on its frames (they all share the same 852x640 canvas)
const LIGHTNING_LEFT := 50.0
const LIGHTNING_RIGHT := 826.0
## Vertical center of the lightning on its frames
const LIGHTNING_CENTER_Y := 320.0

@export var fps := 14.0
## Vertical scale of the lightning (horizontally, it's stretched to the screen)
@export var thickness := 0.9
## Extra width on each side of the screen, so the ends never show
@export var overscan := 0.04
@export var fade_in := 0.12
@export var fade_out := 0.3
## How long the lightning stays after an autoplay ends, waiting for the next one
@export var linger := 0.35

var _sprite: AnimatedSprite2D
var _tween: Tween
var _active := false


func _ready() -> void:
	var frames := SpriteFrames.new()
	frames.set_animation_speed(&"default", fps)
	frames.set_animation_loop(&"default", true)
	for path in FRAMES:
		frames.add_frame(&"default", load(path))

	_sprite = AnimatedSprite2D.new()
	_sprite.sprite_frames = frames
	_sprite.centered = false
	_sprite.light_mask = 0
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	_sprite.material = additive
	add_child(_sprite)

	modulate.a = 0.0
	visible = false
	set_process(false)


func set_active(active: bool) -> void:
	if active == _active:
		return
	_active = active
	if _tween:
		_tween.kill()
	_tween = create_tween()

	if active:
		visible = true
		set_process(true)
		_sprite.play(&"default")
		_sprite.frame = randi() % FRAMES.size()
		_stretch()
		_tween.tween_property(self, "modulate:a", 1.0, fade_in)
	else:
		_tween.tween_interval(linger)
		_tween.tween_property(self, "modulate:a", 0.0, fade_out)
		_tween.tween_callback(func() -> void:
			visible = false
			set_process(false)
			_sprite.stop()
		)


func _process(_delta: float) -> void:
	_stretch()


## Stretches the lightning across the whole screen, at this node's height
func _stretch() -> void:
	var screen := get_canvas_transform().affine_inverse() * get_viewport_rect()
	var width := screen.size.x * (1.0 + overscan * 2.0)
	var scale_x := width / (LIGHTNING_RIGHT - LIGHTNING_LEFT)
	_sprite.global_scale = Vector2(scale_x, thickness * global_scale.y)
	_sprite.global_rotation = 0.0
	_sprite.global_position = Vector2(
		screen.position.x - screen.size.x * overscan - LIGHTNING_LEFT * scale_x,
		global_position.y - LIGHTNING_CENTER_Y * _sprite.global_scale.y)
