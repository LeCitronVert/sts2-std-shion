extends Node2D

## Stellar Strike's rainbow explosion at the center of the enemy.
## Spawned from C# (ShionVfx.CreateStellarExplosion) as an attack hit VFX node, which calls play(at, radius), then frees itself.
## A white flash, rainbow rings whose hue spins as they expand, and rainbow stars flying outwards, all additive.

@export var duration := 1.1
@export var ring_count := 3
## Delay between two rings
@export var ring_stagger := 0.08
@export var star_count := 36
@export var sparkle_count := 60

var _radius := 160.0
var _time := 0.0
var _stars: Array = []
var _sparkles: Array = []


func _ready() -> void:
	set_process(false)
	light_mask = 0
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	material = additive


## at: global position. radius: how far the explosion reaches, in global units (about the enemy's size)
func play(at: Vector2, radius: float) -> void:
	global_position = at
	global_scale = Vector2.ONE
	_radius = maxf(radius, 80.0)
	for i in star_count:
		var angle := TAU * i / star_count + randf_range(-0.12, 0.12)
		_stars.append({
			"dir": Vector2.from_angle(angle),
			"speed": randf_range(0.9, 1.6),
			"hue": float(i) / star_count,
			"size": randf_range(9.0, 16.0),
			"spin": randf_range(-6.0, 6.0),
			"delay": randf_range(0.0, 0.06),
		})
	for i in sparkle_count:
		_sparkles.append({
			"dir": Vector2.from_angle(randf() * TAU),
			"speed": randf_range(0.3, 1.2),
			"hue": randf(),
			"size": randf_range(2.0, 4.5),
			"delay": randf_range(0.0, 0.15),
		})
	set_process(true)


func _process(delta: float) -> void:
	_time += delta
	if _time >= duration:
		queue_free()
		return
	queue_redraw()


func _draw() -> void:
	var k := _time / duration

	# White core flash, quickly gone
	var flash := clampf(1.0 - _time / 0.22, 0.0, 1.0)
	if flash > 0.0:
		draw_circle(Vector2.ZERO, _radius * lerpf(0.25, 0.7, 1.0 - flash), Color(1, 1, 1, flash))
		draw_circle(Vector2.ZERO, _radius * 0.9 * (1.0 - flash * 0.5), Color(1, 0.95, 1, flash * 0.35))

	# Rainbow rings: hue around the ring, spinning, thinner as they expand
	for r in ring_count:
		var t := (_time - r * ring_stagger) / (duration * 0.7)
		if t <= 0.0 or t >= 1.0:
			continue
		var ease_t := 1.0 - pow(1.0 - t, 3.0)
		var ring_radius := _radius * (0.2 + ease_t * (1.0 + r * 0.25))
		var width := lerpf(22.0, 3.0, t) * (1.0 - r * 0.2)
		var alpha := (1.0 - t) * (1.0 - r * 0.15)
		_draw_rainbow_ring(ring_radius, width, alpha, _time * 2.5 + r * 0.33)

	# Stars flying outwards, slowing down
	for star in _stars:
		var t: float = (_time - star["delay"]) / (duration * 0.9)
		if t <= 0.0 or t >= 1.0:
			continue
		var travel := 1.0 - pow(1.0 - t, 2.5)
		var pos: Vector2 = star["dir"] * _radius * 1.3 * star["speed"] * travel
		var hue: float = fposmod(star["hue"] + _time * 0.6, 1.0)
		var size: float = star["size"] * (1.0 - t * 0.6)
		var alpha := 1.0 - t * t
		var rot: float = star["spin"] * _time
		# Short trail behind each star
		draw_line(pos - star["dir"] * size * 2.5 * (1.0 - t), pos, Color.from_hsv(hue, 0.8, 1.0, alpha * 0.5), size * 0.5)
		_draw_star(pos, size, rot, Color.from_hsv(hue, 0.75, 1.0, alpha))
		_draw_star(pos, size * 0.45, rot, Color(1, 1, 1, alpha))

	# Small rainbow sparkles
	for sparkle in _sparkles:
		var t: float = (_time - sparkle["delay"]) / duration
		if t <= 0.0 or t >= 1.0:
			continue
		var pos: Vector2 = sparkle["dir"] * _radius * 1.1 * sparkle["speed"] * (1.0 - pow(1.0 - t, 2.0))
		var twinkle := 0.6 + 0.4 * sin(_time * 30.0 + sparkle["hue"] * TAU)
		draw_circle(pos, sparkle["size"] * (1.0 - t), Color.from_hsv(sparkle["hue"], 0.6, 1.0, (1.0 - t) * twinkle))

	# Lingering rainbow glow at the center
	var glow := clampf(1.0 - k * 1.3, 0.0, 1.0)
	if glow > 0.0:
		draw_circle(Vector2.ZERO, _radius * 0.35, Color.from_hsv(fposmod(_time * 1.5, 1.0), 0.5, 1.0, glow * 0.4))


func _draw_rainbow_ring(radius: float, width: float, alpha: float, hue_offset: float) -> void:
	const SEGMENTS := 48
	for i in SEGMENTS:
		var a0 := TAU * i / SEGMENTS
		var a1 := TAU * (i + 1) / SEGMENTS + 0.01
		var hue := fposmod(float(i) / SEGMENTS + hue_offset, 1.0)
		draw_arc(Vector2.ZERO, radius, a0, a1, 3, Color.from_hsv(hue, 0.85, 1.0, alpha), width, true)


## Four-pointed star
func _draw_star(pos: Vector2, size: float, rot: float, color: Color) -> void:
	var points := PackedVector2Array()
	for i in 8:
		var a := rot + PI * 0.25 * i
		var r := size if i % 2 == 0 else size * 0.3
		points.append(pos + Vector2.from_angle(a) * r)
	draw_colored_polygon(points, color)
