extends Node2D

## Team Blast's (Dragon Cheer) confetti, falling from the top of the whole screen.
## Spawned from C# (ShionVfx.PlayConfetti) in screen space, then calls play(). Frees itself when done.
## Each piece is a small paper rectangle that flutters (flips around its own axis) and sways while it falls.

const COLORS: Array[Color] = [
	Color("ff4d6d"), Color("ffd23f"), Color("3bceac"), Color("4d96ff"),
	Color("b15cff"), Color("ff8c42"), Color("ffffff"), Color("7cff6b"),
]

@export var piece_count := 160
## Time over which pieces keep appearing at the top
@export var spawn_duration := 0.9
@export var min_fall_speed := 160.0
@export var max_fall_speed := 340.0
@export var sway_amplitude := 60.0
@export var min_size := Vector2(8.0, 12.0)
@export var max_size := Vector2(14.0, 22.0)
## Fade out duration, once the last pieces are leaving the screen
@export var fade_out := 0.6
@export var max_lifetime := 5.0

var _pieces: Array[Dictionary] = []
var _time := 0.0
var _screen := Rect2()


func play() -> void:
	_screen = get_viewport_rect()
	for i in piece_count:
		_pieces.append({
			"delay": randf() * spawn_duration,
			"x": randf_range(_screen.position.x, _screen.end.x),
			"y": _screen.position.y - randf_range(20.0, 120.0),
			"speed": randf_range(min_fall_speed, max_fall_speed),
			"sway_phase": randf() * TAU,
			"sway_speed": randf_range(1.5, 3.5),
			"rotation": randf() * TAU,
			"spin": randf_range(-4.0, 4.0),
			"flip_phase": randf() * TAU,
			"flip_speed": randf_range(5.0, 12.0),
			"size": Vector2(randf_range(min_size.x, max_size.x), randf_range(min_size.y, max_size.y)),
			"color": COLORS[randi() % COLORS.size()],
		})
	set_process(true)


func _ready() -> void:
	set_process(false)


func _process(delta: float) -> void:
	_time += delta
	var all_gone := true
	for piece in _pieces:
		if _time < piece["delay"]:
			all_gone = false
			continue
		piece["y"] += piece["speed"] * delta
		piece["rotation"] += piece["spin"] * delta
		if piece["y"] < _screen.end.y + 40.0:
			all_gone = false
	if _time > max_lifetime - fade_out:
		modulate.a = clampf((max_lifetime - _time) / fade_out, 0.0, 1.0)
	if all_gone or _time >= max_lifetime:
		queue_free()
		return
	queue_redraw()


func _draw() -> void:
	for piece in _pieces:
		if _time < piece["delay"]:
			continue
		var t: float = _time - piece["delay"]
		var sway: float = sin(piece["sway_phase"] + t * piece["sway_speed"]) * sway_amplitude
		var pos := Vector2(piece["x"] + sway, piece["y"])
		# Fluttering: the paper flips around its own axis, which squashes it and darkens its back side
		var flip: float = cos(piece["flip_phase"] + t * piece["flip_speed"])
		var size: Vector2 = piece["size"]
		var color: Color = piece["color"]
		if flip < 0.0:
			color = color.darkened(0.3)
		draw_set_transform(pos, piece["rotation"], Vector2(1.0, maxf(absf(flip), 0.08)))
		draw_rect(Rect2(-size * 0.5, size), color)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
