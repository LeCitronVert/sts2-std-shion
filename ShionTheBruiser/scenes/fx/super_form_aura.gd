extends Node2D

## Super Form's look (Act 4), all drawn in code. Driven from C# (SuperFormFx in Strife/RingVisuals.cs) through
## set_area(), set_aura() and set_sparks(). Placed behind the body, in the creature visuals' coordinates.
## - Aura (with Rings): a Super Saiyan-like golden aura. Tall flame tongues rise from the ground behind the body,
##   higher than the head (tallest in the middle): orange outside, gold, then a bright pale core. Embers drift up,
##   and a pulsing gold light lights the body. Slightly transparent.
## - Sparks: rare electric sparks crackling around the body (blue with Rings, red at 0 Rings).

## Light layer only players' bodies are on (CreatureFxController.AuraLightLayer), so the light doesn't hit enemies
const LIGHT_LAYER := 1 << 19
## Points on each side of a flame tongue
const TONGUE_STEPS := 10

## Overall opacity of the flames (they're additive, so this keeps them from washing the character out)
@export var opacity := 0.7
@export var outer_color := Color(1.0, 0.5, 0.06, 0.55)
@export var middle_color := Color(1.0, 0.78, 0.18, 0.55)
@export var core_color := Color(1.0, 0.96, 0.75, 0.5)
@export var ember_color := Color(1.0, 0.9, 0.4, 0.95)
@export var light_color := Color(1.0, 0.82, 0.3)

## Flames: how wide (compared to the body) and how tall (compared to the body) they rise from the feet
@export var width_scale := 1.5
@export var height_scale := 1.55
@export var tongue_count := 11
@export var flicker_speed := 7.0
@export var embers_per_second := 18.0
@export var light_energy := 0.9
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
## One per flame tongue: position across the aura (-1..1), height and width factors, flicker phase and speed
var _tongues: Array[Dictionary] = []
var _embers: Array[Dictionary] = []
var _ember_timer := 0.0
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
		var across: float = -1.0 + 2.0 * (float(i) + 0.5) / float(tongue_count)
		across += randf_range(-0.06, 0.06)
		# Tallest in the middle, lower on the sides
		var bell: float = exp(-across * across * 1.6)
		_tongues.append({
			"x": across,
			"height": lerpf(0.45, 1.0, bell) * randf_range(0.85, 1.1),
			"width": randf_range(0.16, 0.26),
			"phase": randf() * TAU,
			"speed": randf_range(0.8, 1.3),
			"sway": randf_range(-0.12, 0.12),
		})

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

	# Embers drifting up
	if _aura_on:
		_ember_timer -= delta
		while _ember_timer <= 0.0:
			_ember_timer += 1.0 / embers_per_second
			_spawn_ember()
	for ember in _embers:
		ember["y"] = float(ember["y"]) - float(ember["speed"]) * delta
		ember["x"] = float(ember["x"]) + sin(_time * 3.0 + float(ember["phase"])) * 20.0 * delta
		ember["life"] = float(ember["life"]) - delta
	_embers = _embers.filter(func(e: Dictionary) -> bool: return float(e["life"]) > 0.0)

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


func _spawn_ember() -> void:
	var half: float = _area.size.x * width_scale * 0.5
	var life: float = randf_range(0.6, 1.1)
	_embers.append({
		"x": _area.get_center().x + randf_range(-half, half),
		"y": _area.end.y - randf() * _area.size.y * 0.8,
		"speed": randf_range(160.0, 320.0),
		"size": randf_range(1.5, 3.5),
		"phase": randf() * TAU,
		"life": life,
		"max_life": life,
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


## One flame tongue rising from the feet: wide at the base, pointed at the top, waving as it goes up
func _tongue(tongue: Dictionary, layer_scale: float, time_offset: float) -> PackedVector2Array:
	var feet: float = _area.end.y + 6.0
	var aura_half: float = _area.size.x * width_scale * 0.5
	var base_x: float = _area.get_center().x + float(tongue["x"]) * aura_half * layer_scale
	var t_speed: float = flicker_speed * float(tongue["speed"])
	var phase: float = float(tongue["phase"]) + time_offset
	var flicker: float = 0.85 + 0.15 * sin(_time * t_speed + phase) + 0.05 * sin(_time * t_speed * 2.3 + phase)
	var height: float = _area.size.y * height_scale * float(tongue["height"]) * flicker * layer_scale
	var half_width: float = aura_half * float(tongue["width"]) * layer_scale
	var sway: float = float(tongue["sway"]) * height

	var left := PackedVector2Array()
	var right := PackedVector2Array()
	for i in TONGUE_STEPS + 1:
		var t: float = float(i) / float(TONGUE_STEPS)
		var wobble: float = sin(_time * t_speed * 1.4 + phase + t * 5.0) * half_width * 0.35 * t
		var center_x: float = base_x + sway * t * t + wobble
		var w: float = half_width * pow(1.0 - t, 0.75)
		var y: float = feet - height * t
		left.append(Vector2(center_x - w, y))
		right.append(Vector2(center_x + w, y))
	right.reverse()
	left.append_array(right)
	return left


func _draw() -> void:
	if _aura_strength > 0.0:
		var strength: float = _aura_strength * opacity
		for tongue in _tongues:
			draw_colored_polygon(_tongue(tongue, 1.0, 0.0), _faded(outer_color, strength))
		for tongue in _tongues:
			draw_colored_polygon(_tongue(tongue, 0.78, 1.1), _faded(middle_color, strength))
		# The bright core: only the middle tongues, closer to the body
		for tongue in _tongues:
			if absf(float(tongue["x"])) < 0.55:
				draw_colored_polygon(_tongue(tongue, 0.55, 2.2), _faded(core_color, strength))

		for ember in _embers:
			var fade: float = float(ember["life"]) / float(ember["max_life"])
			var size: float = float(ember["size"])
			var color := ember_color
			color.a *= fade * strength
			draw_rect(Rect2(float(ember["x"]) - size * 0.5, float(ember["y"]) - size, size, size * 2.0), color)

	for spark in _sparks:
		var fade: float = float(spark["life"]) / spark_lifetime
		var points: PackedVector2Array = spark["points"]
		var glow := _spark_color
		glow.a = 0.45 * fade
		var core := _spark_color.lerp(Color.WHITE, 0.6)
		core.a = fade
		draw_polyline(points, glow, 6.0, true)
		draw_polyline(points, core, 2.0, true)


func _faded(color: Color, strength: float) -> Color:
	var result := color
	result.a *= strength
	return result
