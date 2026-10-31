extends Node2D

## Flurry of golden slashes on a target hit by a card Spirit Burst autoplayed.
## Spawned from C# (ShionVfx.cs), which calls play() with the target's center, then frees itself.

const SHEET := "res://ShionTheBruiser/images/charui/shion/spirit_burst/spirit_burst_strike.png"
## Slashes on the sheet, in the order they're played
const SLASHES: Array[Rect2] = [
	Rect2(0, 0, 110, 262),      # long sweep, top-left to bottom
	Rect2(134, 257, 185, 238),  # crossing pair
	Rect2(119, 0, 82, 247),     # upward sweep
	Rect2(0, 257, 139, 234),    # long sweep, top-left to bottom-right
	Rect2(312, 0, 110, 107),    # short tail
]

@export var slash_scale := 1.0
## Delay between two slashes
@export var interval := 0.05
@export var slash_duration := 0.22
## How far apart the slashes can land from the target's center
@export var spread := 40.0


func play(at: Vector2) -> void:
	global_position = at
	var texture: Texture2D = load(SHEET)
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD

	var delay := 0.0
	for region in SLASHES:
		var atlas := AtlasTexture.new()
		atlas.atlas = texture
		atlas.region = region

		var slash := Sprite2D.new()
		slash.texture = atlas
		slash.material = additive
		slash.light_mask = 0
		slash.position = Vector2(randf_range(-spread, spread), randf_range(-spread, spread))
		slash.rotation = randf_range(-0.35, 0.35)
		if randf() < 0.5:
			slash.flip_h = true
		slash.scale = Vector2.ONE * slash_scale * 0.7
		slash.modulate.a = 0.0
		add_child(slash)

		var tween := slash.create_tween()
		tween.tween_interval(delay)
		tween.tween_property(slash, "modulate:a", 1.0, 0.03)
		tween.parallel().tween_property(slash, "scale", Vector2.ONE * slash_scale * 1.1, slash_duration).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tween.parallel().tween_property(slash, "modulate:a", 0.0, slash_duration * 0.7).set_delay(slash_duration * 0.3).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
		delay += interval

	var lifetime := create_tween()
	lifetime.tween_interval(delay + slash_duration + 0.05)
	lifetime.tween_callback(queue_free)
