extends Node2D

## Dragonsong's music notes, gently orbiting and floating around Shion, one per point of damage reduction.
## White with a black outline; they all flash red when Dragonsong reduces incoming damage.
## Driven from C# (CreatureFxController.cs) through set_area(), set_count() and flash().
## Each note picks a random shape: quarter, eighth, beamed eighths or sixteenth.

enum Kind { QUARTER, EIGHTH, BEAMED, SIXTEENTH }

const WHITE := Color(1, 1, 1)
const RED := Color(0.95, 0.12, 0.15)
const OUTLINE := Color(0, 0, 0)

@export var note_size := 1.0
@export var orbit_speed := 0.55
@export var bob_amplitude := 10.0
@export var outline_width := 2.5
@export var flash_duration := 0.6
@export var fade_time := 0.3

var _center := Vector2(0.0, -150.0)
var _radius := Vector2(130.0, 40.0)
var _notes: Array = []
var _time := 0.0
var _flash := 0.0


## area: the creature's bounds (local)
func set_area(area: Rect2) -> void:
	_center = area.get_center() + Vector2(0.0, -area.size.y * 0.1)
	_radius = Vector2(area.size.x * 0.75, area.size.y * 0.12)


func set_count(count: int) -> void:
	var alive := _notes.filter(func(n: Dictionary) -> bool: return not n["leaving"])
	while alive.size() < count:
		var note := {
			"kind": randi() % 4,
			"phase": 0.0,
			"bob": randf() * TAU,
			"spin": randf_range(-0.35, 0.35),
			"height": randf_range(-0.5, 0.5),
			"alpha": 0.0,
			"leaving": false,
		}
		_notes.append(note)
		alive.append(note)
	while alive.size() > count:
		var leaving: Dictionary = alive.pop_back()
		leaving["leaving"] = true
	# Spread the remaining notes evenly around the orbit
	for i in alive.size():
		alive[i]["target_phase"] = TAU * i / maxf(alive.size(), 1.0)
		if alive[i]["alpha"] == 0.0:
			alive[i]["phase"] = alive[i]["target_phase"]


func flash() -> void:
	_flash = flash_duration


func _process(delta: float) -> void:
	_time += delta
	_flash = maxf(_flash - delta, 0.0)
	for note in _notes:
		if note["leaving"]:
			note["alpha"] = maxf(note["alpha"] - delta / fade_time, 0.0)
		else:
			note["alpha"] = minf(note["alpha"] + delta / fade_time, 1.0)
			# Ease towards its slot when the count changes
			note["phase"] = lerp_angle(note["phase"], note["target_phase"], minf(delta * 3.0, 1.0))
	_notes = _notes.filter(func(n: Dictionary) -> bool: return not (n["leaving"] and n["alpha"] <= 0.0))
	visible = not _notes.is_empty()
	if visible:
		queue_redraw()


func _draw() -> void:
	var color := WHITE.lerp(RED, clampf(_flash / (flash_duration * 0.5), 0.0, 1.0))
	# Back notes first, so the front ones are drawn over them
	var order := _notes.duplicate()
	order.sort_custom(func(a: Dictionary, b: Dictionary) -> bool: return sin(a["phase"] + _time * orbit_speed) < sin(b["phase"] + _time * orbit_speed))
	for note in order:
		var angle: float = note["phase"] + _time * orbit_speed
		var depth := sin(angle)  # -1 = behind Shion, 1 = in front
		var pos := _center + Vector2(cos(angle) * _radius.x, depth * _radius.y + note["height"] * _radius.y * 2.0)
		pos.y += sin(_time * 1.7 + note["bob"]) * bob_amplitude
		var size := note_size * lerpf(0.75, 1.1, (depth + 1.0) * 0.5)
		var alpha: float = note["alpha"] * lerpf(0.55, 1.0, (depth + 1.0) * 0.5)
		var rot: float = sin(_time * 1.3 + note["bob"]) * 0.25 + note["spin"]
		draw_set_transform(pos, rot, Vector2.ONE * size)
		_draw_note(note["kind"], Color(color, alpha), Color(OUTLINE, alpha))
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


## Draws a note around (0, 0) (its head), first as a black outline, then filled
func _draw_note(kind: int, fill: Color, outline: Color) -> void:
	for pass_index in 2:
		var c := outline if pass_index == 0 else fill
		var grow := outline_width if pass_index == 0 else 0.0
		match kind:
			Kind.BEAMED:
				_head(Vector2(-12.0, 4.0), grow, c)
				_head(Vector2(12.0, 0.0), grow, c)
				_stem(Vector2(-12.0, 4.0), 34.0, grow, c)
				_stem(Vector2(12.0, 0.0), 34.0, grow, c)
				# Beam between both stem tops
				draw_line(Vector2(-5.5, -30.0), Vector2(18.5 + grow * 0.5, -34.0), c, 6.0 + grow * 2.0)
			_:
				_head(Vector2.ZERO, grow, c)
				_stem(Vector2.ZERO, 34.0, grow, c)
				if kind == Kind.EIGHTH or kind == Kind.SIXTEENTH:
					_flag(Vector2(6.5, -34.0), grow, c)
				if kind == Kind.SIXTEENTH:
					_flag(Vector2(6.5, -25.0), grow, c)


func _head(at: Vector2, grow: float, c: Color) -> void:
	var points := PackedVector2Array()
	for i in 16:
		var a := TAU * i / 16.0
		var p := Vector2(cos(a) * (8.0 + grow), sin(a) * (5.8 + grow))
		points.append(at + p.rotated(-0.4))
	draw_colored_polygon(points, c)


func _stem(head: Vector2, length: float, grow: float, c: Color) -> void:
	var x := head.x + 6.5
	draw_line(Vector2(x, head.y - 1.0 + grow), Vector2(x, head.y - length - grow), c, 2.5 + grow * 2.0)


func _flag(top: Vector2, grow: float, c: Color) -> void:
	var points := PackedVector2Array()
	for i in 9:
		var k := float(i) / 8.0
		points.append(top + Vector2(k * 9.0, k * 14.0 + sin(k * PI) * -4.0))
	draw_polyline(points, c, 3.0 + grow * 2.0, true)
