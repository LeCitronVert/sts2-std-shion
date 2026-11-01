extends Node2D

## The anime anger mark (💢) popping on Shion's head (Competitive Spirit's proc, Pout).
## Spawned from C# (ShionVfx.PlayAngerMark) in the creature's visuals, then play(at) is called.
## Drawn as four separate red curved "veins" around a hollow center (gaps between them), throbbing twice before fading.

const COLOR := Color(0.92, 0.1, 0.12)
const OUTLINE := Color(0.35, 0.0, 0.0)

@export var size := 30.0
@export var width := 7.0
@export var duration := 1.1

var _time := 0.0


func _ready() -> void:
	set_process(false)


func play(at: Vector2) -> void:
	position = at
	scale = Vector2.ZERO
	set_process(true)
	var tween := create_tween()
	tween.tween_property(self, "scale", Vector2.ONE * 1.25, 0.12).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(self, "scale", Vector2.ONE, 0.1)
	# Throbs
	for i in 2:
		tween.tween_property(self, "scale", Vector2.ONE * 1.15, 0.12)
		tween.tween_property(self, "scale", Vector2.ONE, 0.12)
	tween.tween_interval(maxf(duration - 0.7, 0.05))
	tween.tween_property(self, "modulate:a", 0.0, 0.25)
	tween.tween_callback(queue_free)


func _process(delta: float) -> void:
	_time += delta
	queue_redraw()


func _draw() -> void:
	# Each vein is a curve bulging towards the center, one per diagonal quadrant
	for q in 4:
		var angle := PI * 0.25 + q * PI * 0.5
		var dir := Vector2.from_angle(angle)
		var side := dir.orthogonal()
		# Pushed out from the center and kept short, so the center stays hollow and the veins don't touch
		var mid := dir * size * 0.75
		var points := PackedVector2Array()
		for i in 13:
			var k := float(i) / 12.0 * 2.0 - 1.0
			# Curves inwards at its middle
			points.append(mid + side * k * size * 0.28 - dir * (1.0 - k * k) * size * 0.22)
		draw_polyline(points, OUTLINE, width + 4.0, true)
		draw_polyline(points, COLOR, width, true)
