extends Node2D

## Super Form's sparks (Act 4): rare electric zigzag bolts crackling around the body, blue with Rings, red at 0 Rings.
## (The aura itself is Shion's gold aura, scenes/fx/shion_auras.tscn.)
## Driven from C# (SuperFormFx in Strife/RingVisuals.cs) through set_area() and set_sparks().

## Random delay between two sparks, how long one lasts, how many zigzags, how long a bolt is
@export var spark_min_interval := 0.5
@export var spark_max_interval := 1.6
@export var spark_lifetime := 0.14
@export var spark_segments := 6
@export var spark_length := 70.0

var _area := Rect2(-100.0, -300.0, 200.0, 300.0)
var _on := false
var _color := Color(0.45, 0.75, 1.0)
var _timer := 0.0
var _sparks: Array[Dictionary] = []


func _ready() -> void:
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	additive.light_mode = CanvasItemMaterial.LIGHT_MODE_UNSHADED
	material = additive
	light_mask = 0


## Bounds of the creature (in this node's local coordinates)
func set_area(area: Rect2) -> void:
	_area = area


func set_sparks(on: bool, color: Color) -> void:
	_color = color
	if on and not _on:
		_timer = randf_range(0.0, spark_min_interval)
	_on = on


func _process(delta: float) -> void:
	if _on:
		_timer -= delta
		if _timer <= 0.0:
			_timer = randf_range(spark_min_interval, spark_max_interval)
			_spawn()
	for spark in _sparks:
		spark["life"] = float(spark["life"]) - delta
	_sparks = _sparks.filter(func(s: Dictionary) -> bool: return float(s["life"]) > 0.0)
	queue_redraw()


## A zigzag bolt starting near the body's edge
func _spawn() -> void:
	var angle: float = randf() * TAU
	var center: Vector2 = _area.get_center()
	var start := center + Vector2(cos(angle) * _area.size.x * 0.45, sin(angle) * _area.size.y * 0.45)
	var direction := Vector2.from_angle(randf() * TAU)
	var points := PackedVector2Array([start])
	var step: float = spark_length / float(spark_segments)
	var point := start
	for i in spark_segments:
		point += direction * step + Vector2(randf_range(-1.0, 1.0), randf_range(-1.0, 1.0)) * step * 0.8
		points.append(point)
	_sparks.append({"points": points, "life": spark_lifetime})


func _draw() -> void:
	for spark in _sparks:
		var fade: float = float(spark["life"]) / spark_lifetime
		var points: PackedVector2Array = spark["points"]
		var glow := _color
		glow.a = 0.45 * fade
		var core := _color.lerp(Color.WHITE, 0.6)
		core.a = fade
		draw_polyline(points, glow, 6.0, true)
		draw_polyline(points, core, 2.0, true)
