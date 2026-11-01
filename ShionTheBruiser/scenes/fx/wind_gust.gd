extends Node2D

## Dragon Pulse's wind gust: translucent purple gusts emanating from Shion in every direction.
## Placed by C# (ShionVfx.PlayDragonPulse) at Shion's body center, then play() is called.
## Made of wind streaks (curved strokes flying outwards and curling) and a soft expanding ring.

const COLOR := Color(0.65, 0.4, 1.0, 0.55)

@export var streak_count := 26
@export var duration := 1.0
@export var min_distance := 40.0
@export var max_distance := 420.0
@export var streak_length := 0.9  # radians of curl along the streak
@export var ring_radius := 380.0

var _streaks: Array[Dictionary] = []
var _time := 0.0


func _ready() -> void:
	set_process(false)


func play() -> void:
	for i in streak_count:
		_streaks.append({
			"angle": TAU * i / streak_count + randf_range(-0.15, 0.15),
			"delay": randf() * 0.25,
			"distance": randf_range(max_distance * 0.6, max_distance),
			"curl": randf_range(0.3, 1.0) * (1.0 if randf() < 0.5 else -1.0),
			"width": randf_range(3.0, 8.0),
		})
	set_process(true)


func _process(delta: float) -> void:
	_time += delta
	if _time > duration + 0.25:
		queue_free()
		return
	queue_redraw()


func _draw() -> void:
	# Soft ring pushed by the gust
	var rt := clampf(_time / (duration * 0.7), 0.0, 1.0)
	var ring_color := COLOR
	ring_color.a *= (1.0 - rt) * 0.6
	draw_arc(Vector2.ZERO, lerpf(30.0, ring_radius, 1.0 - pow(1.0 - rt, 2.0)), 0.0, TAU, 64, ring_color, lerpf(30.0, 4.0, rt), true)

	for s in _streaks:
		var t: float = (_time - s["delay"]) / duration
		if t < 0.0 or t > 1.0:
			continue
		var eased := 1.0 - pow(1.0 - t, 2.0)
		var head: float = lerpf(min_distance, s["distance"], eased)
		var tail: float = lerpf(min_distance, s["distance"], maxf(eased - 0.35, 0.0))
		var points := PackedVector2Array()
		var steps := 10
		for i in steps + 1:
			var k := float(i) / steps
			var r := lerpf(tail, head, k)
			# The streak curls sideways as it goes out
			var a: float = s["angle"] + s["curl"] * streak_length * k * eased
			points.append(Vector2.from_angle(a) * r)
		var color := COLOR
		color.a *= sin(t * PI)
		draw_polyline(points, color, s["width"] * (1.0 - t * 0.5), true)
