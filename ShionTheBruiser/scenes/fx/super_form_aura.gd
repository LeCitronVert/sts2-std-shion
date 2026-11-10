extends Node2D

## Super Form's aura (Act 4), drawn in code: a Super Saiyan-like golden flame around the creature, with flame
## tongues licking upward, rising energy streaks, and a gold light that lights the body.
## Driven from C# (SuperFormFx in Strife/RingVisuals.cs) through set_area() and set_active().
## Placed behind the body, in the creature visuals' coordinates.

## Light layer only players' bodies are on (CreatureFxController.AuraLightLayer), so the light doesn't hit enemies
const LIGHT_LAYER := 1 << 19

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

const POINTS := 64

var _area := Rect2(-100.0, -300.0, 200.0, 300.0)
var _active := false
var _time := 0.0
var _strength := 0.0
var _phases: Array[float] = []
var _streaks: Array[Dictionary] = []
var _streak_timer := 0.0
var _light: PointLight2D
var _fade: Tween


func _ready() -> void:
	var material := CanvasItemMaterial.new()
	material.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	material.light_mode = CanvasItemMaterial.LIGHT_MODE_UNSHADED
	self.material = material
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
	modulate.a = 0.0
	visible = false


## Bounds of the creature (in this node's local coordinates)
func set_area(area: Rect2) -> void:
	_area = area
	_place_light()


func set_active(active: bool) -> void:
	if active == _active:
		return
	_active = active
	if _fade:
		_fade.kill()
	if active:
		visible = true
	_fade = create_tween()
	_fade.tween_property(self, "modulate:a", 1.0 if active else 0.0, fade_duration)
	_fade.parallel().tween_property(self, "_strength", 1.0 if active else 0.0, fade_duration)
	if not active:
		_fade.tween_callback(func(): visible = false)


func _place_light() -> void:
	if _light == null:
		return
	_light.position = _area.get_center()
	_light.texture_scale = max(_area.size.x, _area.size.y) / 256.0 * 2.4


func _process(delta: float) -> void:
	if not visible:
		return
	_time += delta

	# Pulsing light, flickering a little with the flames
	_light.energy = light_energy * _strength * (0.85 + 0.1 * sin(_time * 6.0) + 0.05 * sin(_time * 17.0))

	# Rising streaks
	if _active:
		_streak_timer -= delta
		while _streak_timer <= 0.0:
			_streak_timer += 1.0 / streaks_per_second
			_spawn_streak()
	for streak in _streaks:
		streak.y -= streak.speed * delta
		streak.life -= delta
	_streaks = _streaks.filter(func(s): return s.life > 0.0)

	queue_redraw()


func _spawn_streak() -> void:
	var half := _area.size.x * width_scale * 0.45
	var x := _area.get_center().x + randf_range(-half, half)
	var y := _area.end.y - randf() * _area.size.y * 0.5
	var life := randf_range(0.35, 0.6)
	_streaks.append({
		"x": x, "y": y, "speed": randf_range(420.0, 700.0), "length": randf_range(22.0, 48.0),
		"life": life, "max_life": life, "width": randf_range(1.5, 3.0),
	})


## Flame silhouette: an egg shape standing on the feet, with flickering tongues on its upper half
func _shape(scale: float, time_offset: float) -> PackedVector2Array:
	var rx := _area.size.x * width_scale * 0.5 * scale
	var ry := _area.size.y * height_scale * 0.5 * scale
	var feet := _area.end.y + 8.0
	var center := Vector2(_area.get_center().x, feet - ry)
	var points := PackedVector2Array()
	for i in POINTS:
		var a := TAU * i / POINTS
		var up := cos(a)  # 1 at the top, -1 at the bottom
		var p := Vector2(sin(a) * rx, -up * ry)

		if up > -0.2:
			# Tongues: peaks spread around the upper part, each flickering on its own
			var t := (a / TAU) * tongue_count
			var k := int(floor(t)) % tongue_count
			var within := t - floor(t)
			var peak := pow(sin(within * PI), 3.0)
			var flicker := 0.55 + 0.45 * sin(_time * flicker_speed + _phases[k] + time_offset)
			var reach := tongue_height * ry * 2.0 * peak * flicker * pow(max(up + 0.2, 0.0) / 1.2, 0.8)
			# Tongues lean upward rather than outward, like rising flames
			p += Vector2(p.x * 0.15, -1.0).normalized() * reach

		# Flat bottom at the feet
		var point := center + p
		point.y = min(point.y, feet)
		points.append(point)
	return points


func _draw() -> void:
	if _strength <= 0.0:
		return
	# The whole aura breathes a little
	var breath := 1.0 + 0.03 * sin(_time * 4.0)
	draw_colored_polygon(_shape(1.0 * breath, 0.0), outer_color)
	draw_colored_polygon(_shape(0.84 * breath, 1.3), middle_color)
	draw_colored_polygon(_shape(0.64 * breath, 2.6), core_color)

	for streak in _streaks:
		var fade: float = streak.life / streak.max_life
		var color := streak_color
		color.a *= fade * _strength
		var start := Vector2(streak.x, streak.y)
		draw_line(start, start + Vector2(0.0, streak.length), color, streak.width, true)
