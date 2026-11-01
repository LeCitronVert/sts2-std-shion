extends Node2D

## Improvise's idea: a big light bulb pops above Shion's head, lights up, then explodes into shards.
## Spawned from C# (ShionVfx.PlayLightBulb) in the creature's visuals, then play(at) is called.

const GLASS := Color(1.0, 0.95, 0.6)
const GLASS_OFF := Color(0.85, 0.85, 0.8)
const OUTLINE := Color(0.1, 0.1, 0.1)
const METAL := Color(0.6, 0.62, 0.68)
const GLOW := Color(1.0, 0.9, 0.35)

@export var size := 34.0
@export var pop_time := 0.15
@export var light_time := 0.45
@export var shake_time := 0.25
@export var explode_time := 0.55

var _time := 0.0
var _lit := 0.0
var _exploded := false
var _shards: Array[Dictionary] = []
var _rays := 0.0


func _ready() -> void:
	set_process(false)


func play(at: Vector2) -> void:
	position = at
	scale = Vector2.ZERO
	set_process(true)
	var tween := create_tween()
	tween.tween_property(self, "scale", Vector2.ONE, pop_time).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(self, "_lit", 1.0, 0.12)
	tween.tween_interval(light_time)
	tween.tween_callback(_explode).set_delay(shake_time)
	tween.tween_interval(explode_time)
	tween.tween_callback(queue_free)


func _explode() -> void:
	_exploded = true
	for i in 14:
		var dir := Vector2.from_angle(randf() * TAU)
		_shards.append({
			"pos": dir * size * 0.5 + Vector2(0.0, -size * 0.3),
			"velocity": dir * randf_range(220.0, 480.0),
			"spin": randf_range(-12.0, 12.0),
			"rotation": randf() * TAU,
			"size": randf_range(5.0, 11.0),
		})


func _process(delta: float) -> void:
	_time += delta
	if _exploded:
		_rays += delta
		for s in _shards:
			s["velocity"].y += 900.0 * delta
			s["pos"] += s["velocity"] * delta
			s["rotation"] += s["spin"] * delta
	queue_redraw()


func _draw() -> void:
	if _exploded:
		var k := clampf(_rays / explode_time, 0.0, 1.0)
		# Flash and rays
		draw_circle(Vector2(0.0, -size * 0.3), size * (1.2 + k * 2.5), Color(GLOW, (1.0 - k) * 0.6))
		for i in 10:
			var dir := Vector2.from_angle(TAU * i / 10.0)
			draw_line(dir * size * (0.8 + k * 1.5), dir * size * (1.4 + k * 3.0), Color(GLOW, 1.0 - k), 4.0)
		for s in _shards:
			var tri := PackedVector2Array([
				Vector2(-1.0, -0.6), Vector2(1.0, -0.4), Vector2(0.1, 0.9),
			])
			for j in 3:
				tri[j] = s["pos"] + (tri[j] * s["size"]).rotated(s["rotation"])
			draw_colored_polygon(tri, Color(GLASS, 1.0 - k))
		return

	var shake := Vector2.ZERO
	if _lit >= 1.0:
		shake = Vector2(randf_range(-2.0, 2.0), randf_range(-2.0, 2.0)) * clampf(_time - pop_time - 0.12 - light_time, 0.0, 1.0) * 10.0
	draw_set_transform(shake, 0.0, Vector2.ONE)
	# Glow behind the bulb
	if _lit > 0.0:
		draw_circle(Vector2(0.0, -size * 0.3), size * 1.6, Color(GLOW, 0.3 * _lit))
	# Glass
	var glass := GLASS_OFF.lerp(GLASS, _lit)
	draw_circle(Vector2(0.0, -size * 0.3), size * 0.75 + 3.0, OUTLINE)
	draw_circle(Vector2(0.0, -size * 0.3), size * 0.75, glass)
	var neck := PackedVector2Array([
		Vector2(-size * 0.42, 0.0), Vector2(size * 0.42, 0.0),
		Vector2(size * 0.3, size * 0.45), Vector2(-size * 0.3, size * 0.45),
	])
	draw_colored_polygon(neck, glass)
	# Screw base
	draw_rect(Rect2(-size * 0.32, size * 0.45, size * 0.64, size * 0.4), OUTLINE)
	for i in 3:
		draw_rect(Rect2(-size * 0.28, size * (0.48 + i * 0.12), size * 0.56, size * 0.08), METAL)
	# Filament
	draw_polyline(PackedVector2Array([
		Vector2(-size * 0.15, size * 0.1), Vector2(-size * 0.1, -size * 0.35),
		Vector2(0.0, -size * 0.2), Vector2(size * 0.1, -size * 0.35), Vector2(size * 0.15, size * 0.1),
	]), OUTLINE.lerp(Color(1, 0.6, 0.1), _lit), 2.0, true)
	# Shine
	draw_circle(Vector2(-size * 0.3, -size * 0.6), size * 0.12, Color(1, 1, 1, 0.8))
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
