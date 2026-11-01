extends Node2D

## Come What May's sweat drops: a rare drop falling from one side of Shion's head while she has the power,
## about once every 7s at 1 stack, more often with more stacks.
## Driven from C# (CreatureFxController.cs) through set_active(), set_stacks() and set_head().
## Each drop swells at the eye, then falls with gravity, drifting slightly outwards, and fades.

const COLOR := Color(0.62, 0.85, 1.0, 0.95)
const OUTLINE := Color(0.25, 0.5, 0.85, 0.9)

## Seconds between drops at 1 stack; divided by the stack count
@export var base_interval := 7.0
## Never more often than this
@export var min_interval := 1.5
## Delay before the first drop after gaining the power
@export var first_drop_delay := 0.6
@export var swell_time := 0.25
@export var gravity := 900.0
@export var lifetime := 0.9
@export var drop_size := 7.0

var _active := false
var _timer := 0.0
var _stacks := 1
## Left and right eye positions, in this node's local coordinates
var _eyes: Array[Vector2] = [Vector2(-18.0, -260.0), Vector2(18.0, -260.0)]
var _drops: Array = []


func set_active(active: bool) -> void:
	if active and not _active:
		_timer = first_drop_delay
	_active = active


func set_stacks(stacks: int) -> void:
	_stacks = maxi(stacks, 1)
	_timer = minf(_timer, _interval())


func _interval() -> float:
	return maxf(base_interval / _stacks, min_interval)


func set_head(left_eye: Vector2, right_eye: Vector2) -> void:
	_eyes = [left_eye, right_eye]


func _process(delta: float) -> void:
	if _active:
		_timer -= delta
		if _timer <= 0.0:
			_timer = _interval() * randf_range(0.85, 1.15)
			_spawn(randi() % 2, 0.0)
	for drop in _drops:
		drop["t"] += delta
		var t: float = drop["t"] - drop["delay"]
		if t > swell_time:
			drop["velocity"] += Vector2(0.0, gravity) * delta
			drop["pos"] += drop["velocity"] * delta
	_drops = _drops.filter(func(d: Dictionary) -> bool: return d["t"] - d["delay"] < swell_time + lifetime)
	queue_redraw()


func _spawn(side: int, delay: float) -> void:
	var outwards := -1.0 if side == 0 else 1.0
	_drops.append({
		"pos": _eyes[side] + Vector2(outwards * randf_range(0.0, 6.0), 0.0),
		"velocity": Vector2(outwards * randf_range(25.0, 70.0), randf_range(-40.0, 10.0)),
		"t": 0.0,
		"delay": delay,
	})


func _draw() -> void:
	for drop in _drops:
		var t: float = drop["t"] - drop["delay"]
		if t < 0.0:
			continue
		var grow := clampf(t / swell_time, 0.2, 1.0)
		var fall := clampf((t - swell_time) / lifetime, 0.0, 1.0)
		var alpha := 1.0 - fall * fall
		var size := drop_size * grow
		# Stretched a bit vertically as it speeds up
		var stretch := 1.0 + clampf(drop["velocity"].y / 900.0, 0.0, 0.6)
		_draw_drop(drop["pos"], size, stretch, alpha)


## A tear drop: round bottom, pointed top
func _draw_drop(pos: Vector2, size: float, stretch: float, alpha: float) -> void:
	var points := PackedVector2Array()
	for i in 17:
		var a := PI * i / 16.0
		points.append(pos + Vector2(cos(a) * size, sin(a) * size))
	points.append(pos + Vector2(0.0, -size * 2.2 * stretch))
	var outline := OUTLINE
	outline.a *= alpha
	var fill := COLOR
	fill.a *= alpha
	draw_colored_polygon(points, fill)
	points.append(points[0])
	draw_polyline(points, outline, 1.5, true)
	# Highlight
	draw_circle(pos + Vector2(-size * 0.35, -size * 0.1), size * 0.25, Color(1, 1, 1, 0.8 * alpha))
