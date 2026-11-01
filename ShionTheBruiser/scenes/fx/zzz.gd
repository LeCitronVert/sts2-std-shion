extends Node2D

## Sleepy "Z"s rising from Shion's head and gently floating off screen (Nap, Eepy).
## Driven from C# (CreatureFxController.cs): play_burst(count) for Nap, set_active() for Eepy.
## all_gone() tells C# when the last Z has left, to restore Shion's animation speed after Nap.

## Emitted when the last Z has left and no more are coming
signal cleared

const COLOR := Color(1, 1, 1)
const OUTLINE := Color(0.15, 0.15, 0.3)

@export var interval := 0.7
@export var speed := 70.0
@export var sway := 25.0
@export var min_size := 18.0
@export var max_size := 34.0
@export var lifetime := 6.0

var _origin := Vector2(30.0, -280.0)
var _active := false
var _timer := 0.0
var _pending := 0
var _zs: Array = []


func set_origin(origin: Vector2) -> void:
	_origin = origin


## Nap: spawns `count` Zs one after the other
func play_burst(count: int) -> void:
	_pending += count


## Eepy: keeps spawning Zs while active
func set_active(active: bool) -> void:
	_active = active


func all_gone() -> bool:
	return _zs.is_empty() and _pending == 0 and not _active


func _process(delta: float) -> void:
	_timer -= delta
	if (_active or _pending > 0) and _timer <= 0.0:
		_timer = interval
		_pending = maxi(_pending - 1, 0)
		_zs.append({
			"pos": _origin,
			"t": 0.0,
			"size": randf_range(min_size, max_size),
			"phase": randf() * TAU,
		})
	var screen := get_canvas_transform().affine_inverse() * get_viewport_rect()
	var local_top := to_local(screen.position).y
	var local_right := to_local(screen.end).x
	for z in _zs:
		z["t"] += delta
		# Up and to the right, getting bigger
		z["pos"] += Vector2(speed * 0.55, -speed) * delta
	var had_zs := not _zs.is_empty()
	_zs = _zs.filter(func(z: Dictionary) -> bool:
		return z["t"] < lifetime and z["pos"].y > local_top - 60.0 and z["pos"].x < local_right + 60.0)
	if had_zs and all_gone():
		cleared.emit()
	visible = not _zs.is_empty()
	if visible:
		queue_redraw()


func _draw() -> void:
	# Keeps the Zs readable when the creature is mirrored (Full Nelson)
	var mirror := -1.0 if get_global_transform().determinant() < 0.0 else 1.0
	for z in _zs:
		var t: float = z["t"]
		var size: float = z["size"] * (1.0 + t * 0.15)
		var pos: Vector2 = z["pos"] + Vector2(sin(t * 1.5 + z["phase"]) * sway, 0.0)
		var alpha := clampf(t / 0.3, 0.0, 1.0)
		var rot := sin(t * 1.2 + z["phase"]) * 0.2
		draw_set_transform(pos, rot, Vector2(mirror, 1.0) * size)
		# The letter Z, drawn as a polyline on a unit square
		var z_points := PackedVector2Array([
			Vector2(-0.4, -0.45), Vector2(0.4, -0.45), Vector2(-0.4, 0.45), Vector2(0.4, 0.45),
		])
		draw_polyline(z_points, Color(OUTLINE, alpha), 0.32, false)
		draw_polyline(z_points, Color(COLOR, alpha), 0.18, false)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
