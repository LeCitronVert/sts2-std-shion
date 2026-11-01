extends Node2D

## Glowing particles drifting up around a creature while it has a power:
## Lurking Intensity (red) and Flow Charge (orange).
## Driven from C# (CreatureFxController.cs) through set_color(), set_area() and set_active().

@export var spawn_rate := 14.0  # particles per second
@export var min_lifetime := 0.8
@export var max_lifetime := 1.6
@export var min_speed := 40.0
@export var max_speed := 110.0
@export var min_size := 2.5
@export var max_size := 6.0

var _color := Color(1, 0.2, 0.2)
var _area := Rect2(-80.0, -300.0, 160.0, 300.0)
var _active := false
var _accumulator := 0.0
var _particles: Array = []


func _ready() -> void:
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	material = additive


func set_color(color: Color) -> void:
	_color = color


## area: the creature's bounds (local)
func set_area(area: Rect2) -> void:
	_area = area.grow_individual(area.size.x * 0.15, 0.0, area.size.x * 0.15, 0.0)


func set_active(active: bool) -> void:
	_active = active


func _process(delta: float) -> void:
	if _active:
		_accumulator += spawn_rate * delta
		while _accumulator >= 1.0:
			_accumulator -= 1.0
			_spawn()
	for p in _particles:
		p["t"] += delta
		p["pos"] += p["velocity"] * delta
		p["pos"].x += sin(p["t"] * p["wobble_speed"] + p["wobble"]) * 20.0 * delta
	_particles = _particles.filter(func(p: Dictionary) -> bool: return p["t"] < p["life"])
	visible = not _particles.is_empty()
	if visible:
		queue_redraw()


func _spawn() -> void:
	# Mostly around the body's lower half, rising past the head
	var x := randf_range(_area.position.x, _area.end.x)
	var y := randf_range(_area.position.y + _area.size.y * 0.3, _area.end.y)
	_particles.append({
		"pos": Vector2(x, y),
		"velocity": Vector2(randf_range(-10.0, 10.0), -randf_range(min_speed, max_speed)),
		"t": 0.0,
		"life": randf_range(min_lifetime, max_lifetime),
		"size": randf_range(min_size, max_size),
		"wobble": randf() * TAU,
		"wobble_speed": randf_range(2.0, 5.0),
	})


func _draw() -> void:
	for p in _particles:
		var k: float = p["t"] / p["life"]
		var alpha := sin(k * PI)
		var size: float = p["size"] * (1.0 - k * 0.5)
		# Soft glow, then a bright core
		draw_circle(p["pos"], size * 2.2, Color(_color, alpha * 0.25))
		draw_circle(p["pos"], size, Color(_color.lightened(0.35), alpha))
