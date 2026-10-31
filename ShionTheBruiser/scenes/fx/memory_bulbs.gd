extends Node2D

## Light bulbs slowly orbiting above a creature's head, one per stack of Flashback.
## Bulb size doesn't follow the creature's scale (Slal shrinks with its HP); only the orbit does.
## Colours cycle red, green, blue, yellow, purple, white. The orbit stays above the neck.
## Driven from C# (CreatureFxController.cs) through set_area(), set_count() and hop().

const COLORS: Array[Color] = [
	Color8(255, 70, 70),
	Color8(80, 230, 90),
	Color8(70, 140, 255),
	Color8(255, 220, 60),
	Color8(190, 90, 255),
	Color8(255, 255, 255),
]
const BASE_COLOR := Color8(150, 150, 160)
const BASE_DARK := Color8(95, 95, 105)

## Radians per second
@export var orbit_speed := 0.7
@export var bulb_radius := 20.0
@export var hop_height := 22.0
@export var hop_duration := 0.35
@export var grow_duration := 0.25
## Bulbs at the back of the orbit are this much smaller and darker
@export var depth_shrink := 0.25

var _radius_x := 70.0
var _radius_y := 14.0
var _angle := 0.0
var _count := 0
## Per bulb: scale (grows in when added, shrinks out when removed) and time since its last hop
var _scales: Array[float] = []
var _hops: Array[float] = []


## Places the orbit around the head of a creature whose visuals cover this rect.
func set_area(rect: Rect2) -> void:
	_radius_x = clampf(rect.size.x * 0.4, 50.0, 110.0)
	_radius_y = clampf(rect.size.y * 0.06, 10.0, 22.0)
	# Centered just above the top of the head, so the lowest point is around the neck
	# (raised by half a bulb so the bigger bulbs' bases stay above it)
	position = Vector2(rect.get_center().x, rect.position.y + _radius_y * 0.5 - bulb_radius * 0.5)


func set_count(count: int) -> void:
	_count = maxi(count, 0)
	while _scales.size() < _count:
		_scales.append(0.0)
		_hops.append(hop_duration)


## Makes the bulb at this index (cycling) jump.
func hop(index: int) -> void:
	if _count <= 0:
		return
	_hops[index % _count] = 0.0


func _process(delta: float) -> void:
	_angle = fmod(_angle + orbit_speed * delta, TAU)
	var step := delta / grow_duration
	for i in _scales.size():
		var target := 1.0 if i < _count else 0.0
		_scales[i] = move_toward(_scales[i], target, step)
		_hops[i] = minf(_hops[i] + delta, hop_duration)
	# Removed bulbs that finished shrinking
	while _scales.size() > _count and _scales[-1] <= 0.0:
		_scales.pop_back()
		_hops.pop_back()
	visible = not _scales.is_empty()
	queue_redraw()


func _draw() -> void:
	# Undo the creature's scale on the bulbs themselves, so they keep the same size on screen
	var comp := 1.0 / maxf(absf(get_global_transform().get_scale().y), 0.01)
	var total := maxi(_count, 1)
	var bulbs := []
	for i in _scales.size():
		if _scales[i] <= 0.0:
			continue
		var a := _angle + TAU * float(i) / float(total)
		var depth := (sin(a) + 1.0) * 0.5  # 0 = back, 1 = front
		var pos := Vector2(cos(a) * _radius_x, sin(a) * _radius_y)
		var hop_t := _hops[i] / hop_duration
		if hop_t < 1.0:
			pos.y -= sin(hop_t * PI) * hop_height * comp
		bulbs.append({"pos": pos, "depth": depth, "scale": _scales[i], "color": COLORS[i % COLORS.size()]})
	# Back bulbs first, so front ones are drawn over them
	bulbs.sort_custom(func(x, y): return x.depth < y.depth)
	for b in bulbs:
		_draw_bulb(b.pos, b.scale * comp * (1.0 - depth_shrink * (1.0 - b.depth)), b.color, b.depth)


func _draw_bulb(pos: Vector2, s: float, color: Color, depth: float) -> void:
	var r := bulb_radius * s
	var shade := lerpf(1.0 - depth_shrink, 1.0, depth)
	var glass := Color(color.r * shade, color.g * shade, color.b * shade, 1.0)
	# Glow
	draw_circle(pos, r * 2.0, Color(color.r, color.g, color.b, 0.12))
	draw_circle(pos, r * 1.45, Color(color.r, color.g, color.b, 0.22))
	# Glass, narrowing into the neck
	draw_circle(pos, r, glass)
	draw_colored_polygon(PackedVector2Array([
		pos + Vector2(-r * 0.75, r * 0.55),
		pos + Vector2(r * 0.75, r * 0.55),
		pos + Vector2(r * 0.42, r * 1.25),
		pos + Vector2(-r * 0.42, r * 1.25),
	]), glass)
	# Screw base
	var base := Rect2(pos + Vector2(-r * 0.42, r * 1.25), Vector2(r * 0.84, r * 0.6))
	draw_rect(base, BASE_COLOR * shade)
	draw_line(base.position + Vector2(0, base.size.y * 0.35), base.position + Vector2(base.size.x, base.size.y * 0.35), BASE_DARK, maxf(r * 0.12, 1.0))
	draw_line(base.position + Vector2(0, base.size.y * 0.7), base.position + Vector2(base.size.x, base.size.y * 0.7), BASE_DARK, maxf(r * 0.12, 1.0))
	draw_circle(pos + Vector2(0, r * 1.95), r * 0.18, BASE_DARK)
	# Highlight
	draw_circle(pos + Vector2(-r * 0.35, -r * 0.35), r * 0.25, Color(1, 1, 1, 0.75))
