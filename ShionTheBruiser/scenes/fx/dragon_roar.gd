extends Node2D

## Dragon Roar's shockwave: purple rings bursting out of Shion's mouth towards the enemies.
## Placed by C# (ShionVfx.PlayDragonRoar) at the mouth, in the creature's visuals, then play() is called.
## Each ring is an arc facing right (towards the enemies) that widens, thins and fades as it travels.

const COLOR := Color(0.72, 0.35, 1.0)

@export var ring_count := 5
@export var ring_interval := 0.09
@export var ring_duration := 0.7
@export var start_radius := 20.0
@export var end_radius := 900.0
@export var start_width := 22.0
## Half the opening angle of each arc
@export var spread := deg_to_rad(55.0)

var _rings: Array[float] = []  # start time of each ring
var _time := 0.0


func _ready() -> void:
	set_process(false)
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	material = additive


func play() -> void:
	for i in ring_count:
		_rings.append(i * ring_interval)
	set_process(true)


func _process(delta: float) -> void:
	_time += delta
	if _time > _rings[-1] + ring_duration:
		queue_free()
		return
	queue_redraw()


func _draw() -> void:
	for start in _rings:
		var t := (_time - start) / ring_duration
		if t < 0.0 or t > 1.0:
			continue
		var eased := 1.0 - pow(1.0 - t, 2.5)
		var radius := lerpf(start_radius, end_radius, eased)
		var width := lerpf(start_width, 2.0, t)
		var color := COLOR
		color.a = (1.0 - t) * 0.9
		draw_arc(Vector2.ZERO, radius, -spread, spread, 48, color, width, true)
		# Faint inner echo
		color.a *= 0.4
		draw_arc(Vector2.ZERO, radius * 0.85, -spread * 0.9, spread * 0.9, 48, color, width * 0.5, true)
