extends Node2D

## Energy Bomb's white screen flash, added in screen space above the whole combat (ShionVfx.PlayImpactFrame).
## The "impact frame" itself (white background, characters drawn as black outlines) is set up from C#,
## which then calls play() to fade this flash out and frees it.

@export var hold := 0.12
@export var fade_out := 0.35
@export var flash_alpha := 0.85

var _rect: ColorRect


func _ready() -> void:
	_rect = ColorRect.new()
	_rect.color = Color(1, 1, 1, 0)
	_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_rect)


## Starts the flash at full strength, keeps it for `hold` seconds, then fades it out
func play() -> void:
	var screen := get_viewport_rect()
	_rect.position = screen.position
	_rect.size = screen.size
	_rect.color.a = flash_alpha
	var tween := create_tween()
	tween.tween_interval(hold)
	tween.tween_property(_rect, "color:a", 0.0, fade_out).set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_QUAD)
	tween.tween_callback(queue_free)
