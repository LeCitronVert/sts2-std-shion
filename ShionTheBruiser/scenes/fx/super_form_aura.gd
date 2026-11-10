extends Node2D

## Super Form's look (Act 4), all drawn in code. Driven from C# (SuperFormFx in Strife/RingVisuals.cs) through
## set_area(), set_aura() and set_sparks(). Placed behind the body, in the creature visuals' coordinates.
## - Aura (with Rings): a Super Saiyan-like golden flame around the creature, flame tongues licking upward, rising
##   energy streaks, and a pulsing gold light that lights the body.
## - Sparks: rare electric sparks crackling around the body (blue with Rings, red at 0 Rings).

## Light layer only players' bodies are on (CreatureFxController.AuraLightLayer), so the light doesn't hit enemies
const LIGHT_LAYER := 1 << 19
const POINTS := 64

@export var outer_color := Color(1.0, 0.62, 0.08, 0.42)
@export var middle_color := Color(1.0, 0.84, 0.2, 0.5)
@export var core_color := Color(1.0, 0.97, 0.72, 0.4)
@export var streak_color := Color(1.0, 0.92, 0.45, 0.85)
@export var light_color := Color(1.0, 0.82, 0.3)

## Aura size compared to the creature's bounds
@export var width_scale := 1.3
@export var height_scale := 1.3
## Height of the flame tongues, as a part of the aura's height
@export var tongue_height := 0.22
@export var tongue_count := 9
@export var flicker_speed := 9.0
@export var streaks_per_second := 22.0
@export var light_energy := 0.95
@export var fade_duration := 0.3

## Sparks: random delay between two sparks, how long one lasts, how many zigzags
@export var spark_min_interval := 0.5
@export var spark_max_interval := 1.6
@export var spark_lifetime := 0.14
@export var spark_segments := 6
@export var spark_length := 70.0

var _area := Rect2(-100.0, -300.0, 200.0, 300.0)
var _time := 0.0

var _aura_on := false
var _aura_strength := 0.0
var _aura_tween: Tween
var _phases: Array[float] = []
var _streaks: Array[Dictionary] = []
var _streak_timer := 0.0
var _light: PointLight2D

var _sparks_on := false
var _spark_color := Color(0.45, 0.75, 1.0)
var _spark_timer := 0.0
var _sparks: Array[Dictionary] = []


func _ready() -> void:
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	additive.light_mode = CanvasItemMaterial.LIGHT_MODE_UNSHADED
	material = additive
	light_mask = 0

	for i in tongue_count:
		_phases.append(randf() * TAU)

	var gradient := Gradient.new()
	gradient.set_color(0, Color(1, 1, 1, 1))
	gradient.set_color(1, Color(1, 1, 1, 0))
	var texture := GradientTexture2D.new()
	texture.gradient = gradient
	texture.fill = GradientTexture2D.FILL_RADIAL
	texture.fill_from = Vector2(0.5, 0.5)
	texture.fill_to = Vector2(1.0, 0.5)
	texture.width = 256
	texture.height = 256

	_light = PointLight2D.new()
	_light.texture = texture
	_light.color = light_color
	_light.energy = 0.0
	_light.blend_mode = Light2D.BLEND_MODE_ADD
	_light.range_item_cull_mask = LIGHT_LAYER
	add_child(_light)
	_place_light()


## Bounds of the creature (in this node's local coordinates)
func set_area(area: Rect2) -> void:
	_area = area
	_place_light()


func set_aura(on: bool) -> void:
	if on == _aura_on:
		return
	_aura_on = on
	if _aura_tween:
		_aura_tween.kill()
	_aura_tween = create_tween()
	_aura_tween.tween_property(self, "_aura_strength", 1.0 if on else 0.0, fade_duration)


func set_sparks(on: bool, color: Color) -> void:
	_spark_color = color
	if on and not _sparks_on:
		_spark_timer = randf_range(0.0, spark_min_interval)
	_sparks_on = on


func _place_light() -> void:
	if _light == null:
		return
	_light.position = _area.get_center()
	_light.texture_scale = maxf(_area.size.x, _area.size.y) / 256.0 * 2.4


func _process(delta: float) -> void:
	_time += delta

	# Pulsing light, flickering a little with the flames
	var pulse: float = 0.85 + 0.1 * sin(_time * 6.0) + 0.05 * sin(_time * 17.0)
	_light.energy = light_energy * _aura_strength * pulse

	# Rising streaks
	if _aura_on:
		_streak_timer -= delta
		while _streak_timer <= 0.0:
			_streak_timer += 1.0 / streaks_per_second
			_spawn_streak()
	for streak in _streaks:
		streak["y"] = float(streak["y"]) - float(streak["speed"]) * delta
		streak["life"] = float(streak["life"]) - delta
	_streaks = _streaks.filter(func(s: Dictionary) -> bool: return float(s["life"]) > 0.0)

	# Sparks
	if _sparks_on:
		_spark_timer -= delta
		if _spark_timer <= 0.0:
			_spark_timer = randf_range(spark_min_interval, spark_max_interval)
			_spawn_spark()
	for spark in _sparks:
		spark["life"] = float(spark["life"]) - delta
	_sparks = _sparks.filter(func(s: Dictionary) -> bool: return float(s["life"]) > 0.0)

	queue_redraw()


func _spawn_streak() -> void:
	var half: float = _area.size.x * width_scale * 0.45
	var x: float = _area.get_center().x + randf_range(-half, half)
	var y: float = _area.end.y - randf() * _area.size.y * 0.5
	var life: float = randf_range(0.35, 0.6)
	_streaks.append({
		"x": x, "y": y, "speed": randf_range(420.0, 700.0), "length": randf_range(22.0, 48.0),
		"life": life, "max_life": life, "width": randf_range(1.5, 3.0),
	})


## A zigzag bolt starting near the body's edge
func _spawn_spark() -> void:
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


## Flame silhouette: an egg shape standing on the feet, with flickering tongues on its upper half
func _shape(shape_scale: float, time_offset: float) -> PackedVector2Array:
	var rx: float = _area.size.x * width_scale * 0.5 * shape_scale
	var ry: float = _area.size.y * height_scale * 0.5 * shape_scale
	var feet: float = _area.end.y + 8.0
	var center := Vector2(_area.get_center().x, feet - ry)
	var points := PackedVector2Array()
	for i in POINTS:
		var a: float = TAU * float(i) / float(POINTS)
		var up: float = cos(a)  # 1 at the top, -1 at the bottom
		var p := Vector2(sin(a) * rx, -up * ry)

		if up > -0.2:
			# Tongues: peaks spread around the upper part, each flickering on its own
			var t: float = (a / TAU) * float(tongue_count)
			var k: int = int(floorf(t)) % tongue_count
			var within: float = t - floorf(t)
			var peak: float = pow(sin(within * PI), 3.0)
			var flicker: float = 0.55 + 0.45 * sin(_time * flicker_speed + _phases[k] + time_offset)
			var rise: float = pow(maxf(up + 0.2, 0.0) / 1.2, 0.8)
			var reach: float = tongue_height * ry * 2.0 * peak * flicker * rise
			# Tongues lean upward rather than outward, like rising flames
			p += Vector2(p.x * 0.15 / maxf(rx, 1.0), -1.0).normalized() * reach

		var point := center + p
		point.y = minf(point.y, feet)  # Flat bottom at the feet
		points.append(point)
	return points


func _draw() -> void:
	if _aura_strength > 0.0:
		# The whole aura breathes a little
		var breath: float = 1.0 + 0.03 * sin(_time * 4.0)
		draw_colored_polygon(_shape(1.0 * breath, 0.0), _faded(outer_color))
		draw_colored_polygon(_shape(0.84 * breath, 1.3), _faded(middle_color))
		draw_colored_polygon(_shape(0.64 * breath, 2.6), _faded(core_color))

		for streak in _streaks:
			var fade: float = float(streak["life"]) / float(streak["max_life"])
			var color := streak_color
			color.a *= fade * _aura_strength
			var start := Vector2(float(streak["x"]), float(streak["y"]))
			draw_line(start, start + Vector2(0.0, float(streak["length"])), color, float(streak["width"]), true)

	for spark in _sparks:
		var fade: float = float(spark["life"]) / spark_lifetime
		var points: PackedVector2Array = spark["points"]
		var glow := _spark_color
		glow.a = 0.45 * fade
		var core := _spark_color.lerp(Color.WHITE, 0.6)
		core.a = fade
		draw_polyline(points, glow, 6.0, true)
		draw_polyline(points, core, 2.0, true)


func _faded(color: Color) -> Color:
	var result := color
	result.a *= _aura_strength
	return result
