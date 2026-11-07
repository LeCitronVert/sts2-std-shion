extends Node2D

## Small golden butterfly perched next to the energy cost of Novel cards, slowly beating its wings.
## Added and shown/hidden from C# (NovelButterflyPatch).

const GOLD := Color(1.0, 0.8, 0.25)
const GOLD_LIGHT := Color(1.0, 0.95, 0.65)
const GOLD_DARK := Color(0.55, 0.36, 0.05)
const BODY := Color(0.35, 0.22, 0.04)

@export var size := 18.0
## Seconds for one full wing beat (open -> closed -> open)
@export var beat_period := 1.8
## How far the wings fold in (0 = fully closed, 1 = no movement)
@export var min_open := 0.18

var _time := 0.0


func _ready() -> void:
	# Desync butterflies so a hand full of Novel cards doesn't flap in unison
	_time = randf() * beat_period


func _process(delta: float) -> void:
	if not is_visible_in_tree():
		return
	_time += delta
	queue_redraw()


func _draw() -> void:
	var phase := _time / beat_period * TAU
	# Ease the beat so the wings linger open and snap shut a little faster
	var open := lerpf(min_open, 1.0, pow(0.5 + 0.5 * cos(phase), 0.7))
	var bob := sin(phase) * size * 0.06

	# Faint glow
	draw_circle(Vector2(0.0, bob), size * 0.95, Color(GOLD, 0.12 * open + 0.05))

	for side in [-1.0, 1.0]:
		var sx: float = side * open
		_draw_wing(Vector2(0.5, -0.32), Vector2(0.52, 0.38), -0.55, sx, bob)
		_draw_wing(Vector2(0.36, 0.3), Vector2(0.32, 0.27), 0.55, sx, bob)

	# Body
	var body_top := Vector2(0.0, -0.42 * size + bob)
	var body_bottom := Vector2(0.0, 0.5 * size + bob)
	draw_line(body_top, body_bottom, BODY, size * 0.16, true)
	draw_circle(body_top, size * 0.11, BODY)

	# Antennae
	for side in [-1.0, 1.0]:
		var tip := body_top + Vector2(side * size * 0.28, -size * 0.38)
		draw_line(body_top, tip, BODY, maxf(1.0, size * 0.05), true)
		draw_circle(tip, size * 0.06, GOLD_DARK)


func _draw_wing(center: Vector2, radii: Vector2, rot: float, sx: float, bob: float) -> void:
	var outer := _ellipse(center, radii, rot, sx, bob)
	var inner := _ellipse(center * 1.05, radii * 0.5, rot, sx, bob)
	draw_colored_polygon(outer, GOLD)
	draw_colored_polygon(inner, GOLD_LIGHT)
	var closed := outer.duplicate()
	closed.append(outer[0])
	draw_polyline(closed, GOLD_DARK, maxf(1.0, size * 0.07), true)


func _ellipse(center: Vector2, radii: Vector2, rot: float, sx: float, bob: float) -> PackedVector2Array:
	var points := PackedVector2Array()
	for i in 20:
		var a := TAU * i / 20.0
		var p := (Vector2(cos(a) * radii.x, sin(a) * radii.y)).rotated(rot) + center
		points.append(Vector2(p.x * sx, p.y + bob / size) * size)
	return points
