extends Node2D

## Elaborate Flourish: Slal whirls on herself like a spinning top, with colorful motion-blur streaks around her.
## The body (our parent visuals' current body) is flipped back and forth on X to fake a spin around the vertical axis.
## Spawned from C# (SlalVfx.PlayWhirl) in the creature's visuals, then play(center, size) is called.

const COLORS := [
	Color(1.0, 0.45, 0.75),
	Color(1.0, 0.85, 0.3),
	Color(0.45, 0.85, 0.4),
	Color(0.4, 0.7, 1.0),
	Color(0.95, 0.3, 0.3),
]

@export var duration := 0.75
@export var turns := 3.0

var _center := Vector2.ZERO
var _size := Vector2(200, 300)
var _t := 0.0          # 0 -> 1 over the whirl
var _body: Node2D
var _base_scale := Vector2.ONE


func _ready() -> void:
	set_process(false)


func play(center: Vector2, size: Vector2) -> void:
	_center = center
	_size = size
	var visuals := get_parent()
	if visuals != null and visuals.has_method("GetCurrentBody"):
		_body = visuals.call("GetCurrentBody")
	elif visuals is Node2D:
		_body = visuals
	if _body != null:
		_base_scale = _body.scale
	set_process(true)
	var tween := create_tween()
	# Speeds up, then slows down
	tween.tween_property(self, "_t", 1.0, duration).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tween.tween_callback(_finish)


func _finish() -> void:
	if _body != null and is_instance_valid(_body):
		_body.scale = _base_scale
	queue_free()


func _exit_tree() -> void:
	# Never leave the body flipped or squashed
	if _body != null and is_instance_valid(_body):
		_body.scale = _base_scale


func _process(_delta: float) -> void:
	if _body != null and is_instance_valid(_body):
		var flip := cos(_t * turns * TAU)
		# Never fully flat, so she doesn't vanish mid-turn
		var x := signf(flip) * maxf(absf(flip), 0.12)
		if x == 0.0:
			x = 0.12
		_body.scale = Vector2(_base_scale.x * x, _base_scale.y)
	queue_redraw()


func _draw() -> void:
	# Streaks are strongest at full speed (middle of the whirl)
	var speed := sin(_t * PI)
	if speed <= 0.01:
		return
	var rx := _size.x * 0.55
	for i in COLORS.size():
		var y := _center.y - _size.y * 0.35 + _size.y * 0.7 * i / float(COLORS.size() - 1)
		var phase := _t * turns * TAU * 1.5 + i * 1.3
		var ry := 10.0 + 4.0 * (i % 3)
		# Front half of an ellipse, sliding around as she turns
		var points := PackedVector2Array()
		for j in 17:
			var a := phase + PI * 0.9 * j / 16.0
			points.append(Vector2(_center.x + cos(a) * rx, y + sin(a) * ry))
		draw_polyline(points, Color(COLORS[i], 0.65 * speed), 7.0, true)
		draw_polyline(points, Color(1, 1, 1, 0.35 * speed), 2.0, true)
